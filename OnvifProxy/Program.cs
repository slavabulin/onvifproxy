﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

//---------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Threading;
using Device;
using Media;
using System.Net;
using System.ServiceModel.Channels;

using System.Globalization;
using System.ServiceModel.Dispatcher;



//----------------------------
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Web.Services.Protocols;
using System.Runtime.InteropServices; // added to set system time&date
using Microsoft.Win32;
using System.Management; 
using System.Text.RegularExpressions;
//----------------------------


namespace OnvifProxy
{    
    public class Program
    {
        public static AutoResetEvent ev_RebootEnded;
        public static AutoResetEvent ev_RebootHost;
        
        private static XmlConfig conf;
        private static ConfigStruct confstr;

        public static ServiceHost host;

        public static Guid uuid;

        //private static MediaSource mediaSource;

        public static void Main(string[] args)
        {
            //makexml();//make template for pwd.xml

            uuid = Guid.NewGuid();
            
            ev_RebootEnded = new AutoResetEvent(false);
            ev_RebootHost = new AutoResetEvent(false);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("onvif proxy started\n");
            Console.ResetColor();
            TyphoonCom.log.Debug("----------------------------------------------------------------");

            conf = new XmlConfig();
            confstr = new ConfigStruct();
            confstr = conf.Read();

            //--------------------------------------------------------------------------------------------
            //логика работы следующая - если у нас хотя бы один интерфейс с включенным dhcp - мы считаем, 
            //что именно этот интерфейс смотрит на онвиф-клиентов(иначе нам их не различить, да и в тайфуновской 
            //сетки все адреса должны быть статическими), смотрим какой ip он получил от dhcp-сервака, прописываем
            //этот ip в конфиг и поднимаем сервис на этом адресе. Если у нас все интерфейсы статические, смотрим 
            //ip в конфиге и поднимаем сервис на этом ip
            //--------------------------------------------------------------------------------------------
            //проверим не включен ли dhcp
            IPmgmnt ipmgmnt = new IPmgmnt();
            string interfacename = ipmgmnt.UseDHCP();
            //если включен то берем ip-адрес с интерфейса с dhcp
            //поднимаем сервис на этом адресе
            if (interfacename != null && ipmgmnt.GetDHCPIP(interfacename)!=null)
            {
                confstr.IPAddr = ipmgmnt.GetDHCPIP(interfacename);
            }          
            //и пишем себе в конфиг
            conf.Write(confstr);

            ///это выпихнуть в отдельный тред, тогда управление должно вернуться сюда
            ///после запуска треда (???)
            Thread thr_TyphCom = new Thread(new ParameterizedThreadStart(TyphoonCom.TyphoonComInit));
            thr_TyphCom.IsBackground = true;
            thr_TyphCom.Start(confstr.TyphoonIP);

            do
            {
                Thread.Sleep(1);
            } while (TyphoonCom.ev_TyphComStarted == null && TyphoonCom.ev_TyphComStoped == null);

            TyphoonCom.ev_TyphComStarted.WaitOne();

            Thread thr_MainThr = new Thread(new ThreadStart(MainThread));
            thr_MainThr.Priority = ThreadPriority.Normal;
            thr_MainThr.Start();
            Thread.Sleep(1);             
        }
                
        public static void MainThread()
        {
            host = CreateServiceHost();
            Program.StartHost();
        }

        public static void StartHost()
        {
            TyphoonCom.ev_TyphComStarted.WaitOne();

            //проверим, что хост создан
            if (host.State != CommunicationState.Created)
            {
                //иначе пересоздавать
                RebootHost();
            }
            //проверим, что адрес не 0.0.0.0 , иначе сервис не поднимется
            if (host.Description.Endpoints.ElementAt(0).ListenUri.Host == "0.0.0.0")
            {
                //иначе пересоздавать
                RebootHost();
            }
            WaitHandle[] handlesToReboot = new WaitHandle[] { Program.ev_RebootHost, TyphoonCom.ev_TyphComStoped };
            object LockObj = new object();
            try
            {
                lock (LockObj)
                {
                    TyphoonCom.log.DebugFormat("host.State - {0}",host.State.ToString());
                    host.Open();   
                }
                TyphoonCom.log.Debug("Host opened");
                FlagHostThreadReboot.Ended = true;
                ev_RebootEnded.Set();
                FlagHostThreadReboot.Start = false;

                MediaSource.RenewTimer.Start();
                //------testing purposes-----------------------
                //if (mediaSource != null)
                //{
                //    TestMediaSource mds = new TestMediaSource();
                //}
                //---------------------------------------------

                TyphoonCom.log.DebugFormat("NetworkVideoTransmitter Service started at {0}", host.BaseAddresses.ElementAt(0));

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("NetworkVideoTransmitter Service started at {0}", host.BaseAddresses.ElementAt(0));
                Console.ResetColor();

                
                {
                    try
                    {
                        WaitHandle.WaitAny(handlesToReboot);
                        TyphoonCom.ev_TyphComStarted.Reset();
                        Program.RebootHost();
                    }
                    catch (Exception ex)
                    {
                        TyphoonCom.log.DebugFormat("StartHost - rebooting - {0}",ex.Message);
                        Program.RebootHost();
                    }
                }
                foreach (WaitHandle handle in handlesToReboot)
                {
                    handle.Dispose();
                }
            }
            catch(AddressAccessDeniedException aade)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(aade.Message.ToString());
                Console.ResetColor();
                Program.RebootHost();
            }
            catch (CommunicationException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message.ToString());
                Console.ResetColor();
                Program.RebootHost();
            }
            catch (TimeoutException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (SystemException e)
            {
                Console.WriteLine("!!!!! - {0}",e.Message);
                Console.ReadLine();
            }
        }

        public static void RebootHost()
        {
            //Thread.Sleep(10000);
            //Console.WriteLine("Got command to reboot host.");
            object LockObj = new object();
            //mediaSource = null;

            try
            {
                lock (LockObj)
                {
                    TyphoonCom.log.Debug(host.State.ToString());
                    if (host.State != CommunicationState.Closed)
                    {
                        try
                        {
                            MediaSource.RenewTimer.Stop();
                            host.Close();                            
                            TyphoonCom.log.Debug("Host closed");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Could'nt close host - {0}", e.Message);
                            Console.WriteLine("host state - {0}", host.State.ToString());
                        }
                    }
                }
                Thread.Sleep(5000);
                host = CreateServiceHost();
                Program.StartHost();
            }
            catch (InvalidOperationException e )
            {
                Console.WriteLine(e.Message);
            }
            catch (CommunicationObjectFaultedException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (TimeoutException e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        static void makexml()
        {
            UserList userlistfromfile;

            using (FileStream fs = new FileStream("pwd.xml", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserList));
                try
                {
                    userlistfromfile = new UserList();
                    Device.User user = new Device.User();
                    user.Username = "admin";
                    user.UserLevel = Device.UserLevel.Administrator;
                    user.Password = "admin";
                    userlistfromfile.Add(user);
                    

                    xmlSerializer.Serialize(fs, userlistfromfile);
                }
                catch (FaultException fe)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    TyphoonCom.log.Debug("CreateUsers threw exception - {0}", ex);
                    throw;
                }
            }
        }
        public static ServiceHost CreateServiceHost()
        {
            //---------------------------------------------- 
            host = null;
            try
            {
                confstr = conf.Read();
            }
            catch (Exception ex)
            {
                TyphoonCom.log.DebugFormat(ex.Message);
                RebootHost();
            }

            //проверим не включен ли dhcp
            IPmgmnt ipmgmnt = new IPmgmnt();
            string interfacename = ipmgmnt.UseDHCP();
            //если включен то берем ip-адрес с интерфейса с dhcp
            //поднимаем сервис на этом адресе
            if (interfacename != null && ipmgmnt.GetDHCPIP(interfacename) != null)
            {
                confstr.IPAddr = ipmgmnt.GetDHCPIP(interfacename);
            }
            //и пишем себе в конфиг
            //чтобы поменять все адреса сервисов
            try
            {
                conf.Write(confstr);
            }
            catch (Exception ex)
            {
                TyphoonCom.log.DebugFormat(ex.Message);
                RebootHost();
            }

            FlagServiceDiscoverable.Mode = confstr.ServiceDiscoveryStatus;
            TyphoonCom.log.DebugFormat("(CreateServiceHost)FlagServiceDiscoverable = {0}", FlagServiceDiscoverable.Mode);

            #region CreateConfigBody
            //////--------для формирования конфигурашки + закоментированное в MakeConfig.cs
            //conf.Write(new ConfigStruct());
            //return null;
            ////////////----------
            #endregion CreateConfigBody

            ServicePointManager.Expect100Continue = false;

            Uri baseAddress = new Uri("http://" + confstr.IPAddr + ":80");

            try
            {
                host = new ServiceHost(typeof(Service1), baseAddress);
            }
            catch (ArgumentNullException e)
            {
                TyphoonCom.log.ErrorFormat("ServiceHost creation failed - {0}", e.Message);
            }

            //--------------------------
            HttpTransportBindingElement httpTransportBindingElement = new HttpTransportBindingElement();
            httpTransportBindingElement.KeepAliveEnabled = false;            

            HttpsTransportBindingElement httpsTransportBindingElement = new HttpsTransportBindingElement();
            httpsTransportBindingElement.RequireClientCertificate = true;

            CustomBinding binding = new CustomBinding(
                    new TextMessageEncodingBindingElement(MessageVersion.Soap12, Encoding.UTF8),
                    httpTransportBindingElement);
            CustomBinding binding1 = new CustomBinding(
                    new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8),
                    httpTransportBindingElement);
            WSDualHttpBinding binding2 = new WSDualHttpBinding(WSDualHttpSecurityMode.None);

            //--------------------------
            CustomBinding binding3 = new CustomBinding(
                new TextMessageEncodingBindingElement(MessageVersion.Soap12, Encoding.UTF8),
                httpTransportBindingElement);
            
            CustomBinding bindingReplay = new CustomBinding(
                new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8),
                httpTransportBindingElement);

            CustomBinding bindingRecSearch = new CustomBinding(
                new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8),
                httpTransportBindingElement);

            CustomBinding bindingPTZ = new CustomBinding(
                new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8),
                httpTransportBindingElement);

            CustomBinding bindingImaging = new CustomBinding(
                new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8),
                httpTransportBindingElement);

            CustomBinding bindingMediaRestrictions = new CustomBinding(
                new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8),
                httpTransportBindingElement);

            CustomBinding bindingMediaMarkup = new CustomBinding(
                new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8),
                httpTransportBindingElement);

            CustomBinding bindingTaskManger = new CustomBinding(
                new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8),
                httpTransportBindingElement);
            //--------------------------
            //binding
            binding.Namespace = "http://www.onvif.org/ver10/device/wsdl";
            binding1.Namespace = "http://www.onvif.org/ver10/event/wsdl";
            binding2.Namespace = "http://www.onvif.org/ver10/event/wsdl";

            binding3.Namespace = "urn:ias:cvss:msp:1.0";
            bindingReplay.Namespace = "http://www.onvif.org/ver10/replay/wsdl";
            bindingRecSearch.Namespace = "http://www.onvif.org/ver10/search/wsdl";
            bindingImaging.Namespace = "http://www.onvif.org/ver20/imaging/wsdl";
            bindingMediaRestrictions.Namespace = "urn:ias:cvss:mrm:1.0";
            bindingMediaMarkup.Namespace = "urn:ias:cvss:mm:1.0";
            bindingTaskManger.Namespace = "urn:ias:cvss:tm:1.0";
            //--------------------------

            ServiceDiscoveryBehavior serviceDiscoveryBehavior = new ServiceDiscoveryBehavior();

            ServiceEndpoint DeviceServiceEndpoint,
                    MediaServiceEndpoint,
                    NotificationProducerServiceEndpoint,
                    EventPortTypeServiceEndpoint,
                    SubscriptionManagerServiceEndpoint,
                    PullPointSubscriptionServiceEndpoint,
                    MediaSourceProviderServiceEndpoint,
                    ReplayServiceEndpoint,
                    RecordingSearchEndPoint,
                    PTZEndpoint,
                    ImagingEndpoint,
                    MediaRestrictionsEndpoint,
                    MediaMarkupEndpoint,
                    TaskManagerEndpoint;

            EndpointDiscoveryBehavior MediaServiceBehavior = new EndpointDiscoveryBehavior();
            EndpointDiscoveryBehavior NotificationProducerServiceBehavior = new EndpointDiscoveryBehavior();
            EndpointDiscoveryBehavior EventPortTypeServiceBehavior = new EndpointDiscoveryBehavior();
            EndpointDiscoveryBehavior SubscriptionManagerServiceBehavior = new EndpointDiscoveryBehavior();
            EndpointDiscoveryBehavior PullPointSubscriptionServiceBehavior = new EndpointDiscoveryBehavior();
            EndpointDiscoveryBehavior udpAnnouncementEndpointBehavior = new EndpointDiscoveryBehavior();
            EndpointDiscoveryBehavior ReplayServiceBehavior = new EndpointDiscoveryBehavior();
            EndpointDiscoveryBehavior PTZServiceBehavior = new EndpointDiscoveryBehavior();
            EndpointDiscoveryBehavior ImagingServiceBehavior = new EndpointDiscoveryBehavior();
            EndpointDiscoveryBehavior MediaRestrictionsServiceBehavior = new EndpointDiscoveryBehavior();
            EndpointDiscoveryBehavior MediaMarkupServiceBehavior = new EndpointDiscoveryBehavior();
            EndpointDiscoveryBehavior TaskManagerServiceBehavior = new EndpointDiscoveryBehavior();
            
            UdpAnnouncementEndpoint udpAnnouncementEndpoint;
            UdpDiscoveryEndpoint udpDiscoveryEndpoint;
            EndpointDiscoveryBehavior discoverableEndpointBehavior = new EndpointDiscoveryBehavior();

            //------------
            // записать скопы из конфигурашки в свойства эндпойнта
            try
            {
                for (int i = 0; i < confstr.Scopes.Count; i++)
                {
                    discoverableEndpointBehavior.Scopes.Add(new Uri(confstr.Scopes.ElementAt(i).Data));
                    udpAnnouncementEndpointBehavior.Scopes.Add(new Uri(confstr.Scopes.ElementAt(i).Data));
                }
            }
            catch (SerializationException g)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Оперный Экибастуз! Конфиг то не читается!" + g.Message);
                Console.ResetColor();
            }

            try
            {
                ////Create metadata endpoint
                ////------------------------------
                //// Check to see if service host already has a ServiceMetadataBehavior
                ServiceMetadataBehavior smb = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
                // If not, add one
                if (smb == null)
                    smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;

                smb.HttpGetUrl = new Uri(baseAddress + "onvif/mex");
                //wtf!!!!!

                host.Description.Behaviors.Add(smb);

                MediaServiceBehavior.Enabled = false;
                NotificationProducerServiceBehavior.Enabled = false;
                EventPortTypeServiceBehavior.Enabled = false;
                SubscriptionManagerServiceBehavior.Enabled = false;
                PullPointSubscriptionServiceBehavior.Enabled = false;

                ReplayServiceBehavior.Enabled = false;
                PTZServiceBehavior.Enabled = false;
                ImagingServiceBehavior.Enabled = false;
                MediaRestrictionsServiceBehavior.Enabled = false;
                MediaMarkupServiceBehavior.Enabled = false;
                TaskManagerServiceBehavior.Enabled = false;
                //------------------------------
                // Add endpoints to the service
                try
                {
                    DeviceServiceEndpoint = host.AddServiceEndpoint(
                    typeof(Device.IDevice),
                    binding,
                    "/onvif/device_service");

                    MediaServiceEndpoint = host.AddServiceEndpoint(
                    typeof(Media.IMedia),
                    binding,
                    "/onvif/media_service");
                    MediaServiceEndpoint.Behaviors.Add(MediaServiceBehavior);

                    NotificationProducerServiceEndpoint = host.AddServiceEndpoint(
                    typeof(Event.INotificationProducer),
                    binding1,
                    "/onvif/event_service");
                    NotificationProducerServiceEndpoint.Behaviors.Add(NotificationProducerServiceBehavior);

                    EventPortTypeServiceEndpoint = host.AddServiceEndpoint(
                    typeof(Event.IEventPortType),
                    binding1,
                    "/onvif/event_service");
                    EventPortTypeServiceEndpoint.Behaviors.Add(EventPortTypeServiceBehavior);

                    SubscriptionManagerServiceEndpoint = host.AddServiceEndpoint(
                    typeof(Event.SubscriptionManager),
                    binding1,
                    "/onvif/event_service/bn_subscription_manager");
                    SubscriptionManagerServiceEndpoint.Behaviors.Add(SubscriptionManagerServiceBehavior);

                    PullPointSubscriptionServiceEndpoint = host.AddServiceEndpoint(
                    typeof(Event.PullPointSubscription),
                    binding1,
                    "/onvif/event_service/bn_subscription_manager");
                    PullPointSubscriptionServiceEndpoint.Behaviors.Add(PullPointSubscriptionServiceBehavior);


                    MediaSourceProviderServiceEndpoint = host.AddServiceEndpoint(//added
                        typeof(MediaSourcesProvider.IMediaSourcesProvider),
                        binding3,
                        "/onvif/msp_service");

                    ReplayServiceEndpoint = host.AddServiceEndpoint(//added
                        typeof(ReplayService.IReplayPort),
                        bindingReplay,//?????
                        "/onvif/replay_service");
                    ReplayServiceEndpoint.Behaviors.Add(ReplayServiceBehavior);

                    RecordingSearchEndPoint = host.AddServiceEndpoint(//added
                        typeof(RecordingSearch.ISearchPort),
                        bindingRecSearch,
                        "/onvif/recordingsearch_service");

                    PTZEndpoint = host.AddServiceEndpoint(
                        typeof(PTZ.IPTZ),
                        bindingPTZ,
                        "/onvif/ptz_service");

                    ImagingEndpoint = host.AddServiceEndpoint(
                        typeof(Imaging.IImagingPort),
                        bindingImaging,
                        "/onvif/imaging_service");

                    MediaRestrictionsEndpoint = host.AddServiceEndpoint(
                        typeof(MediaRestrictions.IMediaRestrictionsManager),
                        bindingMediaRestrictions,
                        "/onvif/mediarestrictions_service");

                    //MediaMarkupEndpoint = host.AddServiceEndpoint(
                    //    typeof(MediaMarkup.IMediaMarkupPort),
                    //    bindingMediaMarkup,
                    //    "/onvif/mediamarkup_service");
                    
                    TaskManagerEndpoint = host.AddServiceEndpoint(
                        typeof(TaskManager.ITaskManager),
                        bindingTaskManger,
                        "/onvif/taskmanager_service");
                    

                    DeviceServiceEndpoint.Contract.Name = "NetworkVideoTransmitter";
                    DeviceServiceEndpoint.Contract.Namespace = "http://www.onvif.org/ver10/network/wsdl";

                    MediaServiceEndpoint.Contract.Name = "Media";
                    MediaServiceEndpoint.Contract.Namespace = "http://www.onvif.org/ver10/media/wsdl";

                    NotificationProducerServiceEndpoint.Contract.Name = "NotificationProducer";
                    NotificationProducerServiceEndpoint.Contract.Namespace = "http://docs.oasis-open.org/wsn/bw-2";

                    EventPortTypeServiceEndpoint.Contract.Name = "EventPortType";
                    EventPortTypeServiceEndpoint.Contract.Namespace = "http://docs.oasis-open.org/wsn/bw-2";

                    SubscriptionManagerServiceEndpoint.Contract.Name = "SubscriptionManager";
                    SubscriptionManagerServiceEndpoint.Contract.Namespace = "http://docs.oasis-open.org/wsn/bw-2";

                    PullPointSubscriptionServiceEndpoint.Contract.Name = "PullPointSubscription";
                    PullPointSubscriptionServiceEndpoint.Contract.Namespace = "http://www.onvif.org/ver10/events/wsdl";

                    MediaSourceProviderServiceEndpoint.Contract.Name = "MediaSourceProvider";
                    MediaSourceProviderServiceEndpoint.Contract.Namespace = "urn:ias:cvss:msp:1.0";

                    ReplayServiceEndpoint.Contract.Name = "ReplayService";
                    ReplayServiceEndpoint.Contract.Namespace = "http://www.onvif.org/ver10/replay/wsdl";

                    RecordingSearchEndPoint.Contract.Name = "RecordingSearchService";
                    RecordingSearchEndPoint.Contract.Namespace = "http://www.onvif.org/ver10/search/wsdl";

                    PTZEndpoint.Contract.Name = "PTZService";
                    PTZEndpoint.Contract.Namespace = "http://www.onvif.org/ver20/ptz/wsdl";

                    ImagingEndpoint.Contract.Name = "ImagingService";
                    ImagingEndpoint.Contract.Namespace = "http://www.onvif.org/ver20/imaging/wsdl";

                    MediaRestrictionsEndpoint.Contract.Name = "MediaRestrictionsService";
                    MediaRestrictionsEndpoint.Contract.Namespace = "urn:ias:cvss:mrm:1.0";

                    //MediaMarkupEndpoint.Contract.Name = "MediaMarkupService";
                    //MediaMarkupEndpoint.Contract.Namespace = "urn:ias:cvss:mm:1.0";

                    TaskManagerEndpoint.Contract.Name = "TaskManagerService";
                    TaskManagerEndpoint.Contract.Namespace = "urn:ias:cvss:tm:1.0";
                    //----------------------------------------------------------------------------------
                    //если размер отсылаемого пакета больше 1518 байт, например слишком много скопов, 
                    //то система отсылает только первые 1518 и больше не досылает ни при Announcement'е
                    //ни при Discovery - Решение, джамбо-фреймы с обеих сторон
                    //----------------------------------------------------------------------------------
                    if (confstr.ServiceDiscoveryStatus == DiscoveryMode.Discoverable)
                    {
                        

                        TyphoonCom.log.DebugFormat("DiscoveryMode - {0}", confstr.ServiceDiscoveryStatus.ToString());
                        udpDiscoveryEndpoint = new UdpDiscoveryEndpoint(DiscoveryVersion.WSDiscoveryApril2005);
                        udpDiscoveryEndpoint.TransportSettings.MaxMulticastRetransmitCount = 10;
                        host.AddServiceEndpoint(udpDiscoveryEndpoint);

                        udpAnnouncementEndpoint = new UdpAnnouncementEndpoint(DiscoveryVersion.WSDiscoveryApril2005);
                        udpAnnouncementEndpoint.Name = "NetworkVideoTransmitter";
                        udpAnnouncementEndpoint.Address = new EndpointAddress("urn:uuid:" + uuid.ToString());
                        udpAnnouncementEndpoint.Contract.Namespace = "http://www.onvif.org/ver10/network/wsdl";

                        udpAnnouncementEndpointBehavior.Enabled = true;
                        udpAnnouncementEndpoint.Behaviors.Add(udpAnnouncementEndpointBehavior);
                        

                        serviceDiscoveryBehavior.AnnouncementEndpoints.Add(udpAnnouncementEndpoint);


                        host.Description.Behaviors.Add(serviceDiscoveryBehavior);
                        //----------?????----------
                        discoverableEndpointBehavior.Enabled = true;
                        DeviceServiceEndpoint.Behaviors.Add(discoverableEndpointBehavior);
                    }                   
                    
                    
                    //DeviceServiceEndpoint.Address = new EndpointAddress("urn:uuid:00000000-0000-0000-0000-000000000000");
                    DeviceServiceEndpoint.Address = new EndpointAddress("urn:uuid:" + uuid.ToString());
                    DeviceServiceEndpoint.ListenUri = new Uri(confstr.Capabilities.Device.XAddr);
                    Console.WriteLine("uuid - {0}", DeviceServiceEndpoint.Address);
                }
                catch (ArgumentNullException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0}", e.Message);
                    Console.ResetColor();
                    DeviceServiceEndpoint = null;
                    MediaServiceEndpoint = null;
                    NotificationProducerServiceEndpoint = null;
                    EventPortTypeServiceEndpoint = null;
                    SubscriptionManagerServiceEndpoint = null;
                    PullPointSubscriptionServiceEndpoint = null;

                    MediaSourceProviderServiceEndpoint = null;
                    ReplayServiceEndpoint = null;
                    RecordingSearchEndPoint = null;
                    PTZEndpoint = null;
                    ImagingEndpoint = null;
                    MediaRestrictionsEndpoint = null;
                    MediaMarkupEndpoint = null;
                    TaskManagerEndpoint = null;
                }
                catch (NullReferenceException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0}", e.Message);
                    Console.ResetColor();
                    DeviceServiceEndpoint = null;
                    MediaServiceEndpoint = null;
                    NotificationProducerServiceEndpoint = null;
                    EventPortTypeServiceEndpoint = null;
                    SubscriptionManagerServiceEndpoint = null;
                    PullPointSubscriptionServiceEndpoint = null;

                    MediaSourceProviderServiceEndpoint = null;
                    ReplayServiceEndpoint = null;
                    RecordingSearchEndPoint = null;
                    PTZEndpoint = null;
                    ImagingEndpoint = null;
                    MediaRestrictionsEndpoint = null;
                    MediaMarkupEndpoint = null;
                    TaskManagerEndpoint = null;
                }   
                return host;
            }
            catch (CommunicationException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (TimeoutException e)
            {
                Console.WriteLine(e.Message);
            }

            if (host.State != CommunicationState.Closed)
            {
                Console.WriteLine("Aborting the service...");
                host.Abort();
            }
            return null;

        }

    }
    //-----------------------------------------------------
    
    public static class FlagHostThreadReboot
    {
        public static bool Start;
        public static bool Ended;
    }
    public static class FlagServiceDiscoverable
    {
        public static DiscoveryMode Mode;
    }
}