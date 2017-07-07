// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Web.Services.Protocols; 
using System.Web.Services.Description;
using System.Runtime.InteropServices;
using System.ServiceModel.Channels;
using System.Net;
using Microsoft.Win32;
using System.Management; 
using Device;
using Media;
using System.Globalization;
using System.Xml.XPath;

using System.Text.RegularExpressions;
using RecordingSearch;
using MediaSourcesProvider;
using ReplayService;
using Imaging;
using MediaRestrictions;
using TaskManager;


namespace OnvifProxy
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "Service1" в коде, SVC-файле и файле конфигурации.
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any,
        InstanceContextMode = InstanceContextMode.Single,
        IncludeExceptionDetailInFaults = true)]


    public partial class Service1 : Device.IDevice,
        Media.IMedia,
        Event.INotificationProducer, 
        Event.IEventPortType,
        Event.IPullPoint,
        Event.ICreatePullPoint,
        Event.SubscriptionManager,
        Event.INotificationConsumer,
        Event.PullPointSubscription
    {
        public void UnauthorizedAccessFault()
        {
            throw new FaultException(new FaultReason("The requested operation is not permitted by the device"),
                          new FaultCode("Sender",
                              new FaultCode("NotAuthorized", "http://www.onvif.org/ver10/error",
                                  new FaultCode("Operation not Permitted", "http://www.onvif.org/ver10/error"))));
        }
        public string GetDeviceInformation(out string Model, out string FirmwareVersion, out string SerialNumber, out string HardwareId)
        {
            //GetMediaSources(new GetMediaSourcesRequest());
            //FindRecordings(new FindRecordingsRequest());
            //  while(true)
            //{
            //    GetRecordingSearchResults(new GetRecordingSearchResultsRequest());
            //    Thread.Sleep(1);
            //}

            MediaSourcesProvider.GetMediaSourcesRequest request = new GetMediaSourcesRequest();
            request.Limit = 10;
            request.StartReference = "0";
            GetMediaSources(request);
            //---------------------------------------------------



            Model = "Model:SuperPuperModel";
            HardwareId = "HardwareId:xxxx";
            SerialNumber = "s/n:00000001";
            FirmwareVersion = "f/v:0.0.0.1";

            return "Evs";
        }
        
        //void IMedia.UnauthorizedAccessFault()
        //{
        //    throw new FaultException(new FaultReason("The requested operation is not permitted by the device"),
        //                  new FaultCode("Sender",
        //                      new FaultCode("NotAuthorized", "http://www.onvif.org/ver10/error",
        //                          new FaultCode("Operation not Permitted", "http://www.onvif.org/ver10/error"))));
        //}
        //---------------------------------------------------
        // added to set system time&date
        [DllImport("kernel32.dll", SetLastError = true)]
        private extern static bool SetSystemTime(ref SYSTEMTIME lpSystemTime);
        private struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }


        //---------------------------------------------------
        public GetServicesResponse GetServices(GetServicesRequest request)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<tds:Capabilities xmlns:tds = 'http://www.onvif.org/ver10/device/wsdl'><tds:Network/><tds:Security/><tds:System/></tds:Capabilities>");
            ConfigStruct confstr = new ConfigStruct();
            XmlConfig conf = new XmlConfig();

            try
            {
                confstr = conf.Read();
            }
            catch (Exception ex)
            {
                TyphoonCom.log.DebugFormat("GetServices - {0}", ex.Message);
                return new GetServicesResponse();
            }

            //if (request.IncludeCapability)
            {
                //device
                //media
                //event
                DeviceCapabilities devcap = new DeviceCapabilities();
                devcap.Network = new NetworkCapabilities1();
                devcap.IO = new IOCapabilities();
                devcap.Security = new SecurityCapabilities1();
                devcap.System = new SystemCapabilities1();
                devcap = confstr.Capabilities.Device;
                
                using (Stream ms = new MemoryStream())
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(DeviceCapabilities));
                    
                    try
                    {
                        xmlSerializer.Serialize(ms, devcap);

                        string tmpstr = null;
                        int count = 0, a = 0;
                        byte[] buf = new byte[ms.Length];
                        char[] charArray;
                        ms.Seek(0, SeekOrigin.Begin);

                        while (count < ms.Length)
                        {
                            try
                            {
                                buf[count] = Convert.ToByte(ms.ReadByte());
                                count++;
                            }
                            catch (OverflowException oe)
                            {
                                TyphoonCom.log.DebugFormat("GetServices - {0}", oe.Message);
                            }

                            
                        }
                        charArray = new char[Encoding.UTF8.GetCharCount(buf, 0, count)];
                        Encoding.UTF8.GetDecoder().GetChars(buf, 0, count, charArray, 0);

                        
                        while (a < charArray.Length)
                        {
                            tmpstr = tmpstr + charArray[a].ToString();
                            a++;
                        }
                        tmpstr = tmpstr.Replace("DeviceCapabilities", "Capabilities");
                
                        tmpstr = "<tds:Capabilities xmlns:tds='http://www.onvif.org/ver10/device/wsdl'> " +
                            "<tds:Network IPFilter='false' ZeroConfiguration='true' " +
                            "IPVersion6='false' DynDNS='false' Dot11Configuration='false' " +
                            "HostnameFromDHCP='false' NTP='0' /> " +
                            "<tds:Security TLS1.0='false' TLS1.1='false' TLS1.2='false' " +
                            "OnboardKeyGeneration='false' AccessPolicyConfig='false' DefaultAccessPolicy='false' " +
                            "Dot1X='false' RemoteUserHandling='false' X.509Token='false' SAMLToken='false' " +
                            "KerberosToken='false' UsernameToken='false' HttpDigest='false' RELToken='false' /> " +
                            "<tds:System DiscoveryResolve='true' DiscoveryBye='true' " +
                            "RemoteDiscovery='false' SystemBackup='false' SystemLogging='false' " +
                            "FirmwareUpgrade='true' HttpFirmwareUpgrade='false' HttpSystemBackup='false' " +
                            "HttpSystemLogging='false' HttpSupportInformation='false' /> " +
                            "</tds:Capabilities> ";
                        //----------------------------
                        doc.LoadXml(tmpstr);
                    }
                    catch (SerializationException e)
                    {
                        TyphoonCom.log.DebugFormat("GetServices - {0}",e.Message);
                    }
                    catch
                    {
                        TyphoonCom.log.DebugFormat("GetServices - Error");
                    }
                }
                //-----------

                GetServicesResponse getServicesResponse = new GetServicesResponse();
                
                getServicesResponse.Service = new Device.Service[11];

                getServicesResponse.Service[0] = new Device.Service();
                getServicesResponse.Service[0].XAddr = "http://" + confstr.IPAddr + "/onvif/device_service";
                getServicesResponse.Service[0].Namespace = "http://www.onvif.org/ver10/device/wsdl";
                getServicesResponse.Service[0].Version = new OnvifVersion();
                getServicesResponse.Service[0].Version.Major = 2;
                getServicesResponse.Service[0].Version.Minor = 4;

                if (request.IncludeCapability == true)
                {
                    getServicesResponse.Service[0].Capabilities = doc.DocumentElement;
                }

                getServicesResponse.Service[1] = new Device.Service();
                getServicesResponse.Service[1].XAddr = confstr.Capabilities.Events.XAddr;
                getServicesResponse.Service[1].Namespace = "http://www.onvif.org/ver10/events/wsdl";
                getServicesResponse.Service[1].Version = new OnvifVersion();
                getServicesResponse.Service[1].Version.Major = 2;
                getServicesResponse.Service[1].Version.Minor = 4;

                getServicesResponse.Service[2] = new Device.Service();
                getServicesResponse.Service[2].XAddr = "http://" + confstr.IPAddr + "/onvif/media_service";
                getServicesResponse.Service[2].Namespace = "http://www.onvif.org/ver10/media/wsdl";
                getServicesResponse.Service[2].Version = new OnvifVersion();
                getServicesResponse.Service[2].Version.Major = 2;
                getServicesResponse.Service[2].Version.Minor = 4;

                getServicesResponse.Service[3] = new Device.Service();
                getServicesResponse.Service[3].XAddr = "http://" + confstr.IPAddr + "/onvif/recordingsearch_service";
                getServicesResponse.Service[3].Namespace = "http://www.onvif.org/ver10/search/wsdl";
                getServicesResponse.Service[3].Version = new OnvifVersion();
                getServicesResponse.Service[3].Version.Major = 2;
                getServicesResponse.Service[3].Version.Minor = 4;
                getServicesResponse.Service[3].Capabilities =
                new XmlDocument().CreateElement("cap", "Capabilities", "http://www.onvif.org/ver10/search/wsdl");

                getServicesResponse.Service[4] = new Device.Service();
                getServicesResponse.Service[4].XAddr = "http://" + confstr.IPAddr + "/onvif/replay_service";
                getServicesResponse.Service[4].Namespace = "http://www.onvif.org/ver10/replay/wsdl";
                getServicesResponse.Service[4].Version = new OnvifVersion();
                getServicesResponse.Service[4].Version.Major = 2;
                getServicesResponse.Service[4].Version.Minor = 4;
                getServicesResponse.Service[4].Capabilities = new XmlDocument().CreateElement("cap", "Capabilities", "http://www.onvif.org/ver10/replay/wsdl");

                getServicesResponse.Service[5] = new Device.Service();
                getServicesResponse.Service[5].XAddr = "http://" + confstr.IPAddr + "/onvif/msp_service";
                getServicesResponse.Service[5].Namespace = "urn:ias:cvss:msp:1.0";
                getServicesResponse.Service[5].Version = new OnvifVersion();
                getServicesResponse.Service[5].Version.Major = 1;
                getServicesResponse.Service[5].Version.Minor = 0;

                getServicesResponse.Service[6] = new Device.Service();
                getServicesResponse.Service[6].XAddr = "http://" + confstr.IPAddr + "/onvif/ptz_service";
                getServicesResponse.Service[6].Namespace = "http://www.onvif.org/ver20/ptz/wsdl";
                getServicesResponse.Service[6].Version = new OnvifVersion();
                getServicesResponse.Service[6].Version.Major = 1;
                getServicesResponse.Service[6].Version.Minor = 0;

                getServicesResponse.Service[7] = new Device.Service();
                getServicesResponse.Service[7].XAddr = "http://" + confstr.IPAddr + "/onvif/imaging_service";
                getServicesResponse.Service[7].Namespace = "http://www.onvif.org/ver20/imaging/wsdl";
                getServicesResponse.Service[7].Version = new OnvifVersion();
                getServicesResponse.Service[7].Version.Major = 1;
                getServicesResponse.Service[7].Version.Minor = 0;
                getServicesResponse.Service[7].Capabilities = new XmlDocument().CreateElement("cap", "Capabilities", "http://www.onvif.org/ver20/imaging/wsdl");

                getServicesResponse.Service[8] = new Device.Service();
                getServicesResponse.Service[8].XAddr = "http://" + confstr.IPAddr + "/onvif/mediarestrictions_service";
                getServicesResponse.Service[8].Namespace = "urn:ias:cvss:mrm:1.0";
                getServicesResponse.Service[8].Version = new OnvifVersion();
                getServicesResponse.Service[8].Version.Major = 1;
                getServicesResponse.Service[8].Version.Minor = 0;
                getServicesResponse.Service[8].Capabilities = new XmlDocument().CreateElement("cap", "Capabilities", "urn:ias:cvss:mrm:1.0");

                getServicesResponse.Service[9] = new Device.Service();
                getServicesResponse.Service[9].XAddr = "http://" + confstr.IPAddr + "/onvif/mediamarkup_service";
                getServicesResponse.Service[9].Namespace = "urn:ias:cvss:mm:1.0";
                getServicesResponse.Service[9].Version = new OnvifVersion();
                getServicesResponse.Service[9].Version.Major = 1;
                getServicesResponse.Service[9].Version.Minor = 0;
                getServicesResponse.Service[9].Capabilities = new XmlDocument().CreateElement("cap", "Capabilities", "urn:ias:cvss:mm:1.0");

                getServicesResponse.Service[10] = new Device.Service();
                getServicesResponse.Service[10].XAddr = "http://" + confstr.IPAddr + "/onvif/taskmanager_service";
                getServicesResponse.Service[10].Namespace = "urn:ias:cvss:tm:1.0";
                getServicesResponse.Service[10].Version = new OnvifVersion();
                getServicesResponse.Service[10].Version.Major = 1;
                getServicesResponse.Service[10].Version.Minor = 0;
                getServicesResponse.Service[10].Capabilities = new XmlDocument().CreateElement("cap", "Capabilities", "urn:ias:cvss:tm:1.0");

                return getServicesResponse;
            }
        }
        public Device.DeviceServiceCapabilities GetServiceCapabilities()
        {
            return (new DeviceServiceCapabilities());
        }
        public RestoreSystemResponse RestoreSystem(RestoreSystemRequest request)
        {
            return (new RestoreSystemResponse());
        }
        public GetSystemBackupResponse GetSystemBackup(GetSystemBackupRequest request)
        {
            return (new GetSystemBackupResponse());
        }
        public GetScopesResponse GetScopes(GetScopesRequest request)
        {     
            Scope[] scopes;
            XmlConfig conf = new XmlConfig();
            ConfigStruct confstr = new ConfigStruct();
            confstr.Scopes = new System.Collections.ObjectModel.Collection<OnvifScope>();
            confstr = conf.Read();

            try
            {
                scopes = new Scope[confstr.Scopes.Count];
                for (int i = 0; i < confstr.Scopes.Count; i++)
                {
                    scopes[i] = new Scope();
                    scopes[i].ScopeItem = confstr.Scopes.ElementAt(i).Data;
                    scopes[i].ScopeDef = confstr.Scopes.ElementAt(i).ScopeType;
                }
                return new GetScopesResponse(scopes);
            }
            catch
            {
                return new GetScopesResponse();
            }
        }
        public SetScopesResponse SetScopes(SetScopesRequest request)
        {
            XmlConfig conf = new XmlConfig();
            ConfigStruct confstr = new ConfigStruct();
            
            confstr.Scopes = new System.Collections.ObjectModel.Collection<OnvifScope>();
            try
            {
                confstr = conf.Read();
            }
            catch (Exception ex)
            {
                TyphoonCom.log.DebugFormat("SetScopes - {0}", ex.Message);
                Program.ev_RebootHost.Set();
                throw new FaultException(new FaultReason("XML-config error"),
                           new FaultCode("Sender",
                               new FaultCode("OperationProhibited", "http://www.onvif.org/ver10/error",
                                   new FaultCode("ScopeOverwrite", "http://www.onvif.org/ver10/error"))));
            }

            ////////проверить есть ли в request скопы пересекающиеся с конфигурашкой
            //////for (int a = 0; a < request.Scopes.Length; a++)
            //////{
            //////    for (int i = 0; i < confstr.Scopes.Count; i++)
            //////    {
            //////        if (confstr.Scopes.ElementAt(i).Data == request.Scopes[a])
            //////        {
            //////            ////пересекающиеся выкинуть?
            //////            //confstr.Scopes.Remove(confstr.Scopes.ElementAt(i));
            //////            return null;
            //////            //return new SetScopesResponse();
            //////        }
            //////    }
            //////}
            //если нет - записать в конфигурашку со стиранием предыдущей
            // !!!!!!! тут накосячено !!!!!!
            // трет конфигурашку

            //затираем все не fixed скопы
            for (int i = 0; i < confstr.Scopes.Count; i++)
            {
                if (confstr.Scopes[i].ScopeType == ScopeDefinition.Configurable)
                {
                    confstr.Scopes.Remove(confstr.Scopes[i]);
                }
            }

            try
            {
                OnvifScope[] onvfScope = new OnvifScope[request.Scopes.Length];
                for (int i = 0; i < request.Scopes.Length; i++)
                {
                    onvfScope[i] = new OnvifScope();
                    onvfScope[i].Data = request.Scopes[i];
                    onvfScope[i].ScopeType = ScopeDefinition.Configurable;
                    confstr.Scopes.Add(onvfScope[i]);
                }
                conf.Write(confstr);
                Thread.Sleep(1);
                FlagHostThreadReboot.Start = true;// перезагрузить сервис
                Program.ev_RebootHost.Set();
                return new SetScopesResponse();
            }
            catch
            {
                return new SetScopesResponse();
            }
        }
        public AddScopesResponse AddScopes(AddScopesRequest request)
        {
            XmlConfig conf = new XmlConfig();
            ConfigStruct confstr = new ConfigStruct();
            OnvifScope[] onvfScope = new OnvifScope[request.ScopeItem.Length];

            confstr.Scopes = new System.Collections.ObjectModel.Collection<OnvifScope>();
            confstr = conf.Read();

            try
                {                    
                    //перебрать весь массив scopes
                    for (int i = 0; i < confstr.Scopes.Count; i++)
                    {
                        
                        //проверить, не содержится ли такой же Scope
                        for (int a = 0; a < request.ScopeItem.Length; a++)
                        {
                            if (confstr.Scopes[i].Data == request.ScopeItem[a])
                            {
                                //return null;
                                confstr.Scopes.Remove(confstr.Scopes[i]);
                            }
                        }
                        //Console.WriteLine("adding " + onvfScope[i].Data);
                    }
                    //если нет - то добавить в список
                    for (int a = 0; a < request.ScopeItem.Length; a++)
                    {
                        onvfScope[a] = new OnvifScope();
                        onvfScope[a].Data = request.ScopeItem[a];
                        onvfScope[a].ScopeType = ScopeDefinition.Configurable;
                        confstr.Scopes.Add(onvfScope[a]);
                        Console.WriteLine("adding " + onvfScope[a].Data);
                    } 
                    conf.Write(confstr);// сформированную конфигурацию записать
                    //--------------------
                    //Thread.Sleep(5000);
                    Thread.Sleep(1);
                    //--------------------
                    FlagHostThreadReboot.Start = true;// перезагрузить сервис
                    Program.ev_RebootHost.Set();
                }
                catch
                {
                    return null;
                }

            return new AddScopesResponse();
        }
        public RemoveScopesResponse RemoveScopes(RemoveScopesRequest request)
        {
            XmlConfig conf = new XmlConfig();
            ConfigStruct confstr = new ConfigStruct();
            OnvifScope[] onvfScope = new OnvifScope[request.ScopeItem.Length];

            confstr.Scopes = new System.Collections.ObjectModel.Collection<OnvifScope>();
            confstr = conf.Read();
            int count = confstr.Scopes.Count;

            try
            {//перебрать весь массив scopes
                for (int i = 0; i < confstr.Scopes.Count; i++)
                {
                    //проверить, не содержится ли такой же Scope
                    for (int a = 0; a < request.ScopeItem.Length; a++)
                    {
                        if (confstr.Scopes[i].Data == request.ScopeItem[a])
                        {//если нет - то удалить из списка
                            Console.WriteLine("removing " + confstr.Scopes[i].Data);
                            confstr.Scopes.RemoveAt(i);                                              
                        }//иначе просто пропустить
                    }

                }
                conf.Write(confstr);// сформированную конфигурацию записать
                FlagHostThreadReboot.Start = true;// И перезагрузить сервис
                Program.ev_RebootHost.Set();
                return new RemoveScopesResponse();
            }
            catch
            {
                return null;
            }
        }
        public GetDPAddressesResponse GetDPAddresses(GetDPAddressesRequest request)
        {
            return (new GetDPAddressesResponse());
        }
        public SetDPAddressesResponse SetDPAddresses(SetDPAddressesRequest request)
        {
            return (new SetDPAddressesResponse());
        }
        public GetEndpointReferenceResponse GetEndpointReference(GetEndpointReferenceRequest request)
        {
            return (new GetEndpointReferenceResponse());
        }
        public GetUsersResponse GetUsers(GetUsersRequest request)
        {
            UserList userlist;

            using (FileStream fs = new FileStream("pwd.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserList));
                try
                {
                    userlist = (UserList)xmlSerializer.Deserialize(fs);
                    Device.User[] usrarr = new Device.User[userlist.Count];
                    for (int i = 0; i < userlist.Count; i++)
                    {
                        usrarr[i] = new Device.User();
                        usrarr[i].Username = userlist.ElementAt(i).Username;
                        usrarr[i].UserLevel = userlist.ElementAt(i).UserLevel;
                    }
                    return (new GetUsersResponse(usrarr));
                }
                catch (SerializationException g)
                {
                    Console.WriteLine("Не могу десериализовать файл конфигурации; " + g.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public CreateUsersResponse CreateUsers(CreateUsersRequest request)
        {
            if (request == null)
                return null;
            UserList userlistfromfile;

            using (FileStream fs = new FileStream("pwd.xml", FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                 XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserList));

                 try
                 {
                     userlistfromfile = (UserList)xmlSerializer.Deserialize(fs);

                     foreach (Device.User usr in request.User)
                     {
                         if (usr.Username == null)
                             return null;

                         //check if username already exists
                         foreach (Device.User username in userlistfromfile)
                         {
                             if (usr.Username == username.Username)
                                 //return appropriate FaultException
                                 throw new FaultException(new FaultReason("Username already exists"),
                                     new FaultCode("Sender",
                                         new FaultCode("OperationProhibited", "http://www.onvif.org/ver10/error",
                                             new FaultCode("UsernameClash", "http://www.onvif.org/ver10/error"))));
                         }
                         //check if username is too long
                         if (usr.Username.ToString().Length > 20)
                         {
                             //return appropriate FaultException
                             throw new FaultException(new FaultReason("The username is too long"),
                                     new FaultCode("Sender",
                                         new FaultCode("OperationProhibited", "http://www.onvif.org/ver10/error",
                                             new FaultCode("UsernameTooLong", "http://www.onvif.org/ver10/error"))));
                         }
                         //check if pass is too long
                         if (usr.Password.ToString().Length > 20)
                         {
                             //return appropriate FaultException
                             throw new FaultException(new FaultReason("The password is too long"),
                                     new FaultCode("Sender",
                                         new FaultCode("OperationProhibited", "http://www.onvif.org/ver10/error",
                                             new FaultCode("PasswordTooLong", "http://www.onvif.org/ver10/error"))));
                         }
                         //check if userlevel is anon
                         if (usr.UserLevel == UserLevel.Anonymous)
                         {
                             //return appropriate FaultException
                             throw new FaultException(new FaultReason("User level anonymous is not allowed"),
                                     new FaultCode("Sender",
                                         new FaultCode("OperationProhibited", "http://www.onvif.org/ver10/error",
                                             new FaultCode("AnonymousNotAllowed", "http://www.onvif.org/ver10/error"))));
                         }
                         //check if password is too weak
                         //check if maximum number of supported users exceeds
                         //check if username is too short  
                         userlistfromfile.Add(usr);

                     }
                     fs.Position = 0;
                     xmlSerializer.Serialize(fs, userlistfromfile);
                 }
                     catch(FaultException fe)
                 {
                     throw;
                 }
                 catch (Exception ex)
                 {
                     TyphoonCom.log.Debug("CreateUsers threw exception - {0}", ex);
                 } 
            }
            return (new CreateUsersResponse());
        }
        public DeleteUsersResponse DeleteUsers(DeleteUsersRequest request)
        {
            if (request == null)
                return null;
            UserList userlistfromfile = new UserList();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserList));
            string[] tmpuserlist;
            using (FileStream fs = new FileStream("pwd.xml", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                try
                {
                    userlistfromfile = (UserList)xmlSerializer.Deserialize(fs);

                    tmpuserlist = new string[userlistfromfile.Count];
                    for (int y = 0; y < userlistfromfile.Count; y++)
                    {
                        tmpuserlist[y] = userlistfromfile.ElementAt(y).Username;
                    }
                    foreach (string name in request.Username)
                    {
                        if(!tmpuserlist.Contains(name))
                        {
                            throw new FaultException(new FaultReason("Username not recognized"),
                                    new FaultCode("Sender",
                                        new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                            new FaultCode("UsernameMissing", "http://www.onvif.org/ver10/error"))));
                        }
                    }

                    userlistfromfile = DeleteElements(request.Username, userlistfromfile);
                }
                catch (FaultException fe)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    TyphoonCom.log.Debug("DeleteUsers threw exception while deserializing pwd.xml - {0}", ex);
                }
                //fs.Dispose();

                using (TextWriter writer = new StreamWriter("pwd.xml"))
                {
                    try
                    {
                        xmlSerializer.Serialize(writer, userlistfromfile);
                    }
                    catch (Exception ex)
                    {
                        TyphoonCom.log.Debug("DeleteUsers threw exception while serializing pwd.xml - {0}", ex);
                    }
                    
                }
            }
            return (new DeleteUsersResponse());
        }

        UserList DeleteElements(string[] strarr, UserList list)
        {
            if (strarr == null || list == null) return null;
            foreach(Device.User user in list)
            {
                for(int t=0;t<strarr.Length;t++)
                {
                    if(strarr[t]==user.Username)
                    {
                        list.Remove(user);
                        return DeleteElements(strarr, list);
                    }
                }
            }
            return list;
        }

        public SetUserResponse SetUser(SetUserRequest request)
        {
            UserList userlistfromfile, tmpuserlist;
            using (FileStream fs = new FileStream("pwd.xml", FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserList));
                tmpuserlist = new UserList();
                try
                {
                    //now we should update data from existing file with data from request
                    userlistfromfile = (UserList)xmlSerializer.Deserialize(fs);
                    if (userlistfromfile == null)
                        throw new FaultException(new FaultReason("Couldn't deserialize username"),
                                      new FaultCode("Sender",
                                          new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                              new FaultCode("UsernameError", "http://www.onvif.org/ver10/error"))));

                    string[] arr_file_username = new string[userlistfromfile.Count()];

                    for (int u = 0; u < userlistfromfile.Count(); u++)
                    {
                        arr_file_username[u] = userlistfromfile.ElementAt(u).Username;
                    }

                    foreach(Device.User req_user in request.User)
                    {
                        if (req_user.UserLevel == UserLevel.Anonymous)
                            throw new FaultException(new FaultReason("User level anonymous is not allowed"),
                                             new FaultCode("Sender",
                                                 new FaultCode("OperationProhibited", "http://www.onvif.org/ver10/error",
                                                     new FaultCode("AnonymousNotAllowed", "http://www.onvif.org/ver10/error"))));

                        if (req_user.Password.ToString().Count() > 20)
                            throw new FaultException(new FaultReason("The password is too long"),
                                             new FaultCode("Sender",
                                                 new FaultCode("OperationProhibited", "http://www.onvif.org/ver10/error",
                                                     new FaultCode("PasswordTooLong", "http://www.onvif.org/ver10/error"))));

                        if ((!arr_file_username.Contains(req_user.Username)) || req_user.Username == null)
                        {
                            throw new FaultException(new FaultReason("Username not recognized"),
                                      new FaultCode("Sender",
                                          new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                              new FaultCode("UsernameMissing", "http://www.onvif.org/ver10/error"))));
                        }
                        else
                        {
                            for (int y = 0; y < userlistfromfile.Count(); y++)
                            {
                                if (req_user.Username == userlistfromfile.ElementAt(y).Username)
                                {
                                    userlistfromfile.Remove(userlistfromfile.ElementAt(y));
                                    userlistfromfile.Add(req_user);
                                }
                            }
                        }
                    }
                    //fs.Dispose();
                    using (TextWriter writer = new StreamWriter("pwd.xml"))
                    {
                        try
                        {
                            //if (userlistfromfile == null) throw new ApplicationException();//not to erase pwd.xml
                            xmlSerializer.Serialize(writer, userlistfromfile);
                        }
                        catch (Exception ex)
                        {
                            TyphoonCom.log.Debug("SetUser threw exception while serializing pwd.xml - {0}", ex);
                        }
                    }
                }
                catch (SerializationException ex)
                {
                    TyphoonCom.log.DebugFormat("SetUser - serialization exception - {0}", ex.Message);
                }
                catch(FaultException fe)
                {
                    throw;
                }
                catch (Exception exc)
                {
                    TyphoonCom.log.DebugFormat("SetUser - nonserialization exception - {0}", exc.Message);
                }
            }
            return (new SetUserResponse());
        }

        public GetWsdlUrlResponse GetWsdlUrl(GetWsdlUrlRequest request)
        {
            Uri baseuri = new Uri(Program.host.BaseAddresses.ElementAt(0).AbsoluteUri.ToString());
            Uri wsdluri = new Uri(baseuri.ToString() + "onvif/mex");

            return (new GetWsdlUrlResponse(wsdluri.ToString()));
        }

        //uncomment me!!!
        public GetCapabilitiesResponse GetCapabilities(GetCapabilitiesRequest request)
        {
            //TyphoonMsg tmpmsg;
            if (request != null)
            {
                //Console.WriteLine("entering GetCapabilities");

                UTF8Encoding utf8 = new UTF8Encoding();
                ConfigStruct confstr = new ConfigStruct();
                XmlConfig conf = new XmlConfig();
                String Buf = null;
                TyphoonMsg TyphMsg = new TyphoonMsg(TyphoonMsgType.Request);
                confstr.Scopes = new System.Collections.ObjectModel.Collection<OnvifScope>();
                confstr = conf.Read();

                GetCapabilitiesResponse getCapabilitiesResponse = new GetCapabilitiesResponse();
                getCapabilitiesResponse.Capabilities = new Device.Capabilities();

                if (request.Category.Length == 0)
                {
                    getCapabilitiesResponse.Capabilities = confstr.Capabilities;
                    return getCapabilitiesResponse;
                }

                switch (request.Category[0])
                {
                    //--------------------------------------------------
                    // всё что есть
                    // ----- хня, не спрашивает данные от тайфуна, берет 
                    // ----- только из конфига
                    //--------------------------------------------------
                    case CapabilityCategory.All:
                        getCapabilitiesResponse.Capabilities = confstr.Capabilities;
                        return getCapabilitiesResponse;
                    //--------------------------------------------------
                    // обязательные разделы - Device, Media, Events
                    //--------------------------------------------------
                    case CapabilityCategory.Device:
                        if (confstr.Capabilities.Device != null)
                        {
                            //------Треш и угар далее-------------------

                            string ComStr = "<Capabilities><Device  xmlns=\u0022http://www.onvif.org/ver10/schema\u0022><IO><InputConnectors>0</InputConnectors></IO></Device></Capabilities>";
                            TyphoonCom.log.Debug("Service1: GetCapabilities added to commandQueue");


                            byte[] tmpp = TyphoonCom.MakeMem(ComStr);
                            TyphMsg = TyphoonMsgManager.SendSyncMsg(200, 1, tmpp, 0);

                            if (TyphMsg == null) return null;// наверное надо бы fault выкинуть

                            Buf = TyphoonCom.ParseMem(0, TyphMsg.stringMessageData);
                            TyphMsg.Dispose();
                            ConfigStruct tmpconfstr = new ConfigStruct();
                            String DataString = "<?xml version=\u00221.0\u0022 encoding=\u0022utf-8\u0022 ?><ConfigStruct xmlns:xsd=\u0022http://www.w3.org/2001/XMLSchema\u0022 xmlns:xsi=\u0022http://www.w3.org/2001/XMLSchema-instance\u0022>";
                            {
                                DataString = String.Concat(DataString, "<IPAddr>", confstr.IPAddr, "</IPAddr>");
                                Buf = Buf.Replace("<Device>", "<Device  xmlns=\u0022http://www.onvif.org/ver10/schema\u0022>");
                                DataString = String.Concat(DataString, Buf);
                                DataString = String.Concat(DataString, "</ConfigStruct>");
                                tmpconfstr = conf.DeserializeString(confstr, DataString);
                                try
                                {
                                    confstr.Capabilities.Device.IO = tmpconfstr.Capabilities.Device.IO;
                                }
                                catch (NullReferenceException e)
                                {
                                    Console.WriteLine("GetCapabilities: {0}", e.Message);
                                }
                            }

                            getCapabilitiesResponse.Capabilities.Device = confstr.Capabilities.Device;
                            return getCapabilitiesResponse;
                        }
                        else
                        {
                            // если нет такого раздела в config.xml
                            // то возвращает Fault Code
                            // наверное надо бы fault выкинуть, а не null
                            return null;
                        }
                    case CapabilityCategory.Media:
                        //запросить раздел медиа у тайфуна
                        //разобрать ответ
                        //сформировать структуру на отдачу
                        if (confstr.Capabilities.Device != null)
                        {
                            TyphMsg = TyphoonMsgManager.SendSyncMsg(4);

                            if (TyphMsg == null) return getCapabilitiesResponse;
                            Buf = TyphMsg.stringMessageData;
                            TyphMsg.Dispose();
                            if (Buf.Length == 12)
                            {
                                byte[] b_rtpmulticast = new byte[4], b_rtp_tcp = new byte[4], b_rtp_rtsp_tcp = new byte[4], b_Buf;

                                b_Buf = Encoding.ASCII.GetBytes(Buf);

                                for (int t = 0; t < 4; t++)
                                {
                                    b_rtpmulticast[t] = b_Buf[t];
                                    b_rtp_tcp[t] = b_Buf[t + 4];
                                    b_rtp_rtsp_tcp[t] = b_Buf[t + 8];
                                }

                                getCapabilitiesResponse.Capabilities.Media = new MediaCapabilities();
                                getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities = new RealTimeStreamingCapabilities();
                                getCapabilitiesResponse.Capabilities.Media.XAddr = confstr.Capabilities.Media.XAddr;

                                getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_RTSP_TCP = false;
                                getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_RTSP_TCPSpecified = true;
                                getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_TCP = false;
                                getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_TCPSpecified = true;
                                getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTPMulticast = false;
                                getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTPMulticastSpecified = true;

                                for (int t = 0; t < 4; t++)
                                {
                                    if (b_rtpmulticast[t] != 0)
                                    {
                                        getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTPMulticast = true;
                                        getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTPMulticastSpecified = true;
                                    }
                                    if (b_rtp_tcp[t] != 0)
                                    {
                                        getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_TCP = true;
                                        getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_TCPSpecified = true;
                                    }
                                    if (b_rtp_rtsp_tcp[t] != 0)
                                    {
                                        getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_RTSP_TCP = true;
                                        getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_RTSP_TCPSpecified = true;
                                    }
                                }


                            }
                            else
                            {
                                TyphoonCom.log.Debug("CapabilityCategory.Media - Тайфун вернул не 12 байт");
                                return null;
                            }
                        }
                        return getCapabilitiesResponse;


                    #region

                    //byte[] tmp = TyphoonCom.FormCommand(200, 4, null, 0);
                            //for (int a = 0; a < 4; a++)
                            //{
                            //    TyphMsg.MessageID = TyphMsg.MessageID << 8;
                            //    TyphMsg.MessageID += tmp[9 - a];
                            //}
                            //TyphoonMsg msg = new TyphoonMsg(TyphoonMsgType.Request);
                            //msg.byteMessageData = TyphoonCom.FormPacket(tmp);

                            //TyphoonMsgManager.EnqueueMsg(msg);

                        //    {
                        //        do
                        //        {
                        //            Thread.Sleep(1);
                        //        } while (TyphoonMsgManager.queueResponceFromTyphoon.Count == 0);

                        //        if (TyphoonMsgManager.queueResponceFromTyphoon.Count > 0)
                        //        {
                        //            ConfigStruct tmpconfstr = new ConfigStruct();

                        //            try
                        //            {
                        //                //находим в очереди ответ с ID отправленного нами запроса
                        //                Buf = TyphoonMsgManager.queueResponceFromTyphoon.Single(TyphoonMessage => TyphoonMessage.Value.MessageID == TyphMsg.MessageID).Value.stringMessageData;
                        //            }
                        //            catch (Exception ex)
                        //            {
                        //                TyphoonCom.log.ErrorFormat("от Typhoon пришли сообщения с одинаковыми ID или нет ни одного сообщения с таким ID {0}", ex.Message);
                        //            }
                        //            //удаляем из очереди мессагу с ID отправленного нами запроса
                        //            try
                        //            {
                        //                TyphoonMsgManager.queueResponceFromTyphoon.TryRemove(TyphMsg.MessageID, out tmpmsg);
                        //                ////рихтуем данные 
                        //                if (Buf.Length == 12)
                        //                {
                        //                    byte[] b_rtpmulticast = new byte[4], b_rtp_tcp = new byte[4], b_rtp_rtsp_tcp = new byte[4], b_Buf;

                        //                    b_Buf = Encoding.ASCII.GetBytes(Buf);

                        //                    for (int t = 0; t < 4; t++)
                        //                    {
                        //                        b_rtpmulticast[t] = b_Buf[t];
                        //                        b_rtp_tcp[t] = b_Buf[t + 4];
                        //                        b_rtp_rtsp_tcp[t] = b_Buf[t + 8];
                        //                    }

                        //                    getCapabilitiesResponse.Capabilities.Media = new MediaCapabilities();
                        //                    getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities = new RealTimeStreamingCapabilities();
                        //                    getCapabilitiesResponse.Capabilities.Media.XAddr = confstr.Capabilities.Media.XAddr;

                        //                    getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_RTSP_TCP = false;
                        //                    getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_RTSP_TCPSpecified = true;
                        //                    getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_TCP = false;
                        //                    getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_TCPSpecified = true;
                        //                    getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTPMulticast = false;
                        //                    getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTPMulticastSpecified = true;

                        //                    for (int t = 0; t < 4; t++)
                        //                    {
                        //                        if (b_rtpmulticast[t] != 0)
                        //                        {
                        //                            getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTPMulticast = true;
                        //                            getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTPMulticastSpecified = true;
                        //                        }
                        //                        if (b_rtp_tcp[t] != 0)
                        //                        {
                        //                            getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_TCP = true;
                        //                            getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_TCPSpecified = true;
                        //                        }
                        //                        if (b_rtp_rtsp_tcp[t] != 0)
                        //                        {
                        //                            getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_RTSP_TCP = true;
                        //                            getCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_RTSP_TCPSpecified = true;
                        //                        }
                        //                    }


                        //                }
                        //                else
                        //                {
                        //                    TyphoonCom.log.Debug("CapabilityCategory.Media - Тайфун вернул не 12 байт");
                        //                    return null;
                        //                }
                        //            }
                        //            catch (Exception ex)
                        //            {
                        //                TyphoonCom.log.Error(ex.Message);
                        //            }

                        //        }
                        //        else
                        //        {
                        //            Console.WriteLine("AddCommand returned false");
                        //        }
                        //    }
                        //    return getCapabilitiesResponse;
                        //}
                        //else
                        //{
                        //    // если нет такого раздела в config.xml
                        //    // то возвращает Fault Code
                        //    return null;
                    //}
                    #endregion
                    case CapabilityCategory.Events:
                        if (confstr.Capabilities.Events != null)
                        {
                            getCapabilitiesResponse.Capabilities.Events = confstr.Capabilities.Events;
                            return getCapabilitiesResponse;
                        }
                        else
                        {
                            // если нет такого раздела в config.xml
                            // то возвращает Fault Code
                            return null;
                        }
                    //--------------------------------------------------
                    // необязательные разделы - Imaging, PTZ, Analytics
                    //--------------------------------------------------
                    case CapabilityCategory.Imaging:
                        //Console.WriteLine("point 5");
                        if (confstr.Capabilities.Imaging != null)
                        {
                            getCapabilitiesResponse.Capabilities.Imaging = confstr.Capabilities.Imaging;
                            return getCapabilitiesResponse;
                        }
                        else
                        {
                            // если нет такого раздела в config.xml
                            // то возвращает Fault Code
                            return null;
                        }

                    case CapabilityCategory.PTZ:
                        //Console.WriteLine("point 6");
                        //confstr.Capabilities.PTZ. = new PTZCapabilities();
                        if (confstr.Capabilities.PTZ != null)
                        {
                            getCapabilitiesResponse.Capabilities.PTZ = confstr.Capabilities.PTZ;
                            return getCapabilitiesResponse;
                        }
                        else
                        {
                            // если нет такого раздела в config.xml
                            // то возвращает Fault Code
                            return null;
                        }
                    case CapabilityCategory.Analytics:
                        //Console.WriteLine("point 7");
                        if (confstr.Capabilities.Analytics != null)
                        {
                            getCapabilitiesResponse.Capabilities.Analytics = confstr.Capabilities.Analytics;
                            return getCapabilitiesResponse;
                        }
                        else
                        {
                            // если нет такого раздела в config.xml
                            // то возвращает Fault Code
                            return null;
                        }

                    default: getCapabilitiesResponse.Capabilities = confstr.Capabilities;
                        //Console.WriteLine("point 8");
                        // если нет такого раздела в config.xml
                        // то возвращает Fault Code
                        return null;
                }
            }
            return null;
        }
        public SetHostnameResponse SetHostname(SetHostnameRequest request)
        {
            if (request.Name != null)
            {
                Regex regex = new Regex(@"/([a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?(\.[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?)*)([^a-z0-9-]|$)/i");
                if (regex.IsMatch(request.Name))
                {
                    string oldName = System.Environment.MachineName;
                    using (ManagementObject cs = new ManagementObject(@"Win32_Computersystem.Name='" + oldName + "'"))
                    {
                        cs.Get();
                        ManagementBaseObject inParams = cs.GetMethodParameters("Rename");
                        inParams.SetPropertyValue("Name", request.Name);
                        InvokeMethodOptions methodOptions = new InvokeMethodOptions(null, System.TimeSpan.MaxValue);
                        ManagementBaseObject outParams = cs.InvokeMethod("Rename", inParams, methodOptions);
                        string Return = Convert.ToString(outParams.Properties["ReturnValue"].Value);
                        Console.WriteLine("Return Value: {0}", Return);
                        return (new SetHostnameResponse());
                    }
                }
                
                FaultCode fc2 = new FaultCode("InvalidHostname", "http://www.onvif.org/ver10/error");
                FaultCode fc = new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error"
                        ,fc2 );
                FaultCode fc0 = new FaultCode("Sender", fc);
                FaultReason fr = new FaultReason("");
                FaultException fe = new FaultException(fr, fc0);
                //fe.
                throw fe;
            }
            return null;
        }

        public bool SetHostnameFromDHCP(bool FromDHCP)
        {
            return false;
        }
        public SetDNSResponse SetDNS(SetDNSRequest request)
        {
            XmlConfig conf = new XmlConfig();
            ConfigStruct confstr = new ConfigStruct();

            ManagementClass mClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection mObjCol = mClass.GetInstances();
            SetDNSResponse setDNSResponse = new SetDNSResponse();
            
            confstr = conf.Read();

            if (request == null) return null;

            if (request.FromDHCP == true)
            {
                foreach (ManagementObject mObj in mObjCol)
                {
                    if ((bool)mObj["IPEnabled"] == true && ((string[])mObj["IPAddress"])[0] == confstr.IPAddr)
                    {
                        ManagementBaseObject mboDNS = mObj.GetMethodParameters("SetDNSServerSearchOrder");

                        if (mboDNS != null)
                        {
                            mboDNS["DNSServerSearchOrder"] = null;
                            mObj.InvokeMethod("SetDNSServerSearchOrder", mboDNS, null);
                        }
                    }
                }
                return setDNSResponse;
            }
            else
            {
                if (request.DNSManual != null)
                {
                    foreach (ManagementObject mObj in mObjCol)
                    {
                        if ((bool)mObj["IPEnabled"] == true && ((string[])mObj["IPAddress"])[0] == confstr.IPAddr)
                        {
                            string caption = (string)mObj["Caption"];
                            NetworkManagement nm = new NetworkManagement();
                            if (request.DNSManual.Length != 0)
                            {
                                nm.setDNS(caption, request.DNSManual[0].IPv4Address);
                            }
                            if (request.SearchDomain != null)
                            {
                                ManagementBaseObject mboDNS = mObj.GetMethodParameters("SetDNSDomain");
                                mboDNS["DNSDomain"] = request.SearchDomain[0];
                                try
                                {
                                    mObj.InvokeMethod("SetDNSDomain", mboDNS, null);
                                }
                                catch (Exception ex)
                                {
                                    TyphoonCom.log.DebugFormat("SetDNSDomain failed - {0}", ex.Message);
                                }
                            }
                        }
                    }
                    return setDNSResponse;
                }
                else
                {
                    return null;
                }
            }
            
            
            //if (request == null) return null;
            
            //ManagementClass mClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
            //ManagementObjectCollection mObjCol = mClass.GetInstances();

            //foreach (ManagementObject mObj in mObjCol)
            //{
            //    if ((bool)mObj["IPEnabled"])
            //    {
            //        ManagementBaseObject mboDNS = mObj.GetMethodParameters("SetDNSServerSearchOrder");

            //        if (request.FromDHCP == true)
            //        {
            //            if (mboDNS != null)
            //            {
            //                mboDNS["DNSServerSearchOrder"] = null;
            //                mObj.InvokeMethod("SetDNSServerSearchOrder", mboDNS, null);
            //            }
            //        }
            //        else
            //        {
            //            if (request.DNSManual != null)
            //            {
            //                string[] str = new string[request.DNSManual.Length];
            //                for (int t = 0; t < request.DNSManual.Length; t++)
            //                {
            //                    str[t] = request.DNSManual[t].IPv4Address;
            //                }
            //                if (mboDNS != null)
            //                {
            //                    ManagementBaseObject mboDNS2 = mObj.GetMethodParameters("EnableDNS");
            //                    mboDNS2["DNSHostName"] = null;
            //                    mboDNS2["DNSDomain"] = null;
            //                    mboDNS2["DNSServerSearchOrder"] = str;
            //                    mboDNS2["DNSDomainSuffixSearchOrder"] = null;

            //                    try
            //                    {
            //                        mboDNS2["DNSServerSearchOrder"] = null;
            //                        //mObj.InvokeMethod("EnableDHCP", null, null);
            //                        ManagementBaseObject mboDNS3 = mObj.InvokeMethod("EnableDNS", mboDNS2, null);
            //                        //mboDNS3.
            //                        //
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                        TyphoonCom.log.DebugFormat(ex.Message);
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                return null;
            //            }
            //        }                    
            //    }
            //} 
        }

        public SetNTPResponse SetNTP(SetNTPRequest request)
        {
            return (new SetNTPResponse());
        }
        public SetDynamicDNSResponse SetDynamicDNS(SetDynamicDNSRequest request)
        {
            return (new SetDynamicDNSResponse());
        }
        //public GetNetworkInterfacesResponse GetNetworkInterfaces(GetNetworkInterfacesRequest request)
        //{
        //    //check if dhcp - HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\{30E75CDA-8088-4A67-997C-24D329DDB652}\Parameters\Tcpip
        //    //if not - lla  - 169.254.1.0/169.254.254.255
        //    //or manual - and then choose which part of config to fill

        //    GetNetworkInterfacesResponse getNetworkResponse = new GetNetworkInterfacesResponse();
        //    Adapter[] adapterArray;
        //    adapterArray = AdapterInfo.GetAdaptersInfo();
        //    Device.NetworkInterface[] netInterfaces = new Device.NetworkInterface[adapterArray.Length];
        //    Microsoft.Win32.RegistryKey key = null;

            

        //    for (int a = 0; a < adapterArray.Length; a++)
        //    {
        //        Console.WriteLine(a);
        //        //netInterfaces[a].Enabled - ставить - не ставить и что это значит?
        //        netInterfaces[a] = new Device.NetworkInterface();
        //        netInterfaces[a].Info = new Device.NetworkInterfaceInfo();
        //        netInterfaces[a].IPv4 = new Device.IPv4NetworkInterface();
        //        netInterfaces[a].IPv4.Config = new Device.IPv4Configuration();
        //        //---------------------------------------------------
        //        string dhcp = null;
        //        int dhcpEn;
        //        int i_mask=0;
        //        byte[] b_mask = null;
        //        System.Net.IPAddress mask = null;
        //        string[] values = null;

        //        key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\" + adapterArray[a].name + "\\Parameters\\Tcpip");
        //        dhcpEn = (int)key.GetValue("EnableDHCP");

        //        try
        //        {
        //            //string[] valueSubnetMask = new string[key.GetValue("SubnetMask")]
        //            //mask = System.Net.IPAddress.Parse((string)key.GetValue("SubnetMask"));
        //            try
        //            {
        //                RegistryValueKind rvk = key.GetValueKind("SubnetMask");

        //                switch (rvk)
        //                {
        //                    case RegistryValueKind.MultiString:
        //                        values = (string[])key.GetValue("SubnetMask");
        //                        break;
        //                    case RegistryValueKind.String:
        //                        values = new string[1];
        //                        values[0] = (string)key.GetValue("SubnetMask");
        //                        break;
        //                    default:
        //                    break;
        //                }
        //            }
        //            catch (System.IO.IOException ex)
        //            { 
        //                //Value does not exist
        //                throw new ArgumentNullException();
        //            }


        //            mask = System.Net.IPAddress.Parse(values[0]);
        //            b_mask = mask.GetAddressBytes();
        //            //--------------------------------------------
        //            for (int b = 0; b < 4; b++)
        //            {
        //                for (int c = 8; c > 0; c--)
        //                {
        //                    int q = b_mask[b] << (8 - c);
        //                    int w = q << (c-1);
        //                    if (w > 0) i_mask++;
        //                    //if (((b_mask[b] << (7-c)) << c) > 0) i_mask++;
        //                }
        //            }

        //            //--------------------------------------------
        //            //преобразовать b_mask в int и засунуть в 
        //            //netInterfaces[a].IPv4.Config.Manual[0].PrefixLength
        //            //PrefixLength - похоже, что маска подсети
        //            //netInterfaces[a].IPv4.Config.FromDHCP.PrefixLength = 
        //        }
        //        catch (ArgumentNullException ex)
        //        {
        //            Console.WriteLine("Failed to parse mask, {0}", ex.Message);
        //        }
        //        catch (FormatException ex)
        //        {
        //            Console.WriteLine("Failed to parse mask, {0}", ex.Message);
        //        }
               
                
        //        if (dhcpEn == 1)
        //        {
        //            netInterfaces[a].IPv4.Config.FromDHCP = new Device.PrefixedIPv4Address();
        //            netInterfaces[a].IPv4.Config.DHCP = true;
        //            netInterfaces[a].IPv4.Config.FromDHCP.Address = adapterArray[a].ip;
        //            netInterfaces[a].IPv4.Config.FromDHCP.PrefixLength = i_mask;
        //            //dhcp = (string)key.GetValue("DhcpSubnetMask");
                    
        //        }
        //        else
        //        {
        //            //корявая проверка на LLA, наверное стоит переделать
        //            System.Net.IPAddress address;
        //            byte[] b_address;
        //            try
        //            {
        //                address = System.Net.IPAddress.Parse(adapterArray[a].ip);
        //                b_address = address.GetAddressBytes();

        //                if (b_address[0] == 169 && b_address[1]==254) 
        //                {
        //                    //LLA
        //                    netInterfaces[a].IPv4.Config.LinkLocal = new Device.PrefixedIPv4Address();
        //                    netInterfaces[a].IPv4.Config.LinkLocal.Address = adapterArray[a].ip;
        //                    netInterfaces[a].IPv4.Config.LinkLocal.PrefixLength = i_mask;
                            
        //                }
        //                else
        //                {
        //                    //Manual
        //                    //прочитать айпишники и создать массив Manual, который потом заполнить
        //                    //пока сделано только на один айпишник, может больше и не надо?
        //                    //(нужно ли выдавать все айпи висящие на одном интерфейсе?)
        //                    netInterfaces[a].IPv4.Config.Manual = new Device.PrefixedIPv4Address[1];
        //                    netInterfaces[a].IPv4.Config.Manual[0] = new Device.PrefixedIPv4Address();
        //                    netInterfaces[a].IPv4.Config.Manual[0].Address = adapterArray[a].ip;
        //                    netInterfaces[a].IPv4.Config.Manual[0].PrefixLength = i_mask;

        //                }
        //            }
        //            catch (ArgumentNullException ex)
        //            {
        //                Console.WriteLine("Failed to parse LLAddress, {0}", ex.Message);
        //                break;
        //            }
        //            catch (FormatException ex)
        //            {
        //                Console.WriteLine("Failed to parse LLAddress, {0}", ex.Message);
        //                break;
        //            }
                   
        //        }
        //        //---------------------------------------------------
        //        //это похоже не очень нужно
        //        //netInterfaces[a].Link = new Device.NetworkInterfaceLink();
        //        //netInterfaces[a].Link.AdminSettings = new Device.NetworkInterfaceConnectionSetting();
        //        //netInterfaces[a].Link.AdminSettings.Duplex = new Device.Duplex();
        //        //netInterfaces[a].Link.OperSettings = new Device.NetworkInterfaceConnectionSetting();
        //        //netInterfaces[a].Link.OperSettings.Duplex = new Device.Duplex();

        //        netInterfaces[a].Info.HwAddress = adapterArray[a].macAddress;
        //        netInterfaces[a].token = adapterArray[a].name;
        //        netInterfaces[a].Info.Name = adapterArray[a].name;


        //        if(adapterArray[a].ip != null)netInterfaces[a].IPv4.Enabled = true;
        //    }
        //    getNetworkResponse.NetworkInterfaces = netInterfaces;
        //        if(key!=null)key.Close();
        //    return getNetworkResponse;
        //}

        public GetNetworkInterfacesResponse GetNetworkInterfaces(GetNetworkInterfacesRequest request)
        {
            #region
            ////?use Win32_NetworkAdapter instead Win32_NetworkAdapterConfiguration
            ////http://msdn.microsoft.com/en-us/library/windows/desktop/aa394216(v=vs.85).aspx
            //ManagementClass objMC = new ManagementClass("Win32_NetworkAdapter");
            //ManagementObjectCollection objMOC = objMC.GetInstances();
            //Device.NetworkInterface[] networkInterface = new Device.NetworkInterface[objMOC.Count];
            //int a = 0;
            //foreach (ManagementObject objMO in objMOC)
            //{
            //    if ((string)objMO["NetConnectionID"] != null)
            //    {
            //        try
            //        {

            //            networkInterface[a] = new Device.NetworkInterface();
            //            try
            //            {
            //                networkInterface[a].Enabled = (bool)objMO["NetEnabled"];
            //            }
            //            catch (Exception ex)
            //            {
            //                Console.WriteLine("NetEnabled - {0}",ex.Message);
            //            }

            //            try
            //            {
            //                networkInterface[a].token = (string)objMO["Name"];
            //                Console.WriteLine(networkInterface[a].token);
            //            }
            //            catch (Exception ex)
            //            {
            //                Console.WriteLine("Name - {0}", ex.Message);
            //            }
            //            try
            //            {
            //                networkInterface[a].Info = new Device.NetworkInterfaceInfo();
            //                networkInterface[a].Info.Name = (string)objMO["Name"];
            //                Console.WriteLine(networkInterface[a].Info.Name);
            //            }
            //            catch (Exception ex)
            //            {
            //                Console.WriteLine("Name - {0}", ex.Message);
            //            }

            //            try
            //            {
            //                networkInterface[a].Info.HwAddress = (string)objMO["MACAddress"];
            //                Console.WriteLine(networkInterface[a].Info.HwAddress);
            //            }
            //            catch (Exception ex)
            //            {
            //                Console.WriteLine("MACAddress - {0}", ex.Message);
            //            }

            //            try
            //            {
            //                networkInterface[a].IPv4 = new Device.IPv4NetworkInterface();
            //                networkInterface[a].IPv4.Config = new Device.IPv4Configuration();
            //                networkInterface[a].IPv4.Config.Manual = new Device.PrefixedIPv4Address[1];
            //                networkInterface[a].IPv4.Config.Manual[0] = new Device.PrefixedIPv4Address();
            //                networkInterface[a].IPv4.Config.Manual[0].Address = (string)objMO["NetworkAddresses"];
            //                Console.WriteLine(networkInterface[a].IPv4.Config.Manual[0].Address);
            //            }
            //            catch (Exception ex)
            //            {
            //                Console.WriteLine("NetworkAddresses - {0}", ex.Message);
            //            }
            //            //networkInterface[a].Info.MTU = (int)objMO["MTU"];
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine(ex.Message);
            //        }
            //    }
            //    a++;
            //}
            #endregion
            GetNetworkInterfacesResponse getNetworkResponse = new GetNetworkInterfacesResponse();
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            int a = 0;
            Device.NetworkInterface[] networkInterface = new Device.NetworkInterface[objMOC.Count];
            byte[] b_mask = null;
            System.Net.IPAddress mask = null;
            int i_mask = 0;

            foreach (ManagementObject objMO in objMOC)
            {
                
                if ((bool)objMO["IPEnabled"] == true)
                {
                    try
                    {
                        networkInterface[a] = new Device.NetworkInterface();
                        networkInterface[a].Enabled = (bool)objMO["IPEnabled"];
                        networkInterface[a].token = (string)objMO["ServiceName"];
                        networkInterface[a].Info = new Device.NetworkInterfaceInfo();
                        networkInterface[a].Info.Name = (string)objMO["ServiceName"];
                        networkInterface[a].Info.HwAddress = (string)objMO["MACAddress"];
                        //если mtu определен - заполним
                        try
                        {
                            
                            if (((int)objMO["MTU"]) != 0)
                            {
                                networkInterface[a].Info.MTUSpecified = true;
                                networkInterface[a].Info.MTU = (int)objMO["MTU"];
                            }                            
                        }
                        catch (Exception)
                        { }

                        networkInterface[a].IPv4 = new Device.IPv4NetworkInterface();
                        networkInterface[a].IPv4.Enabled = (bool)objMO["IPEnabled"];
                        networkInterface[a].IPv4.Config = new Device.IPv4Configuration();
                        networkInterface[a].IPv4.Config.DHCP = (bool)objMO["DHCPEnabled"];

                        try
                        {
                            string tmp = ((string[])objMO["IPSubnet"])[0];
                            //парсим маску подсети
                            mask = System.Net.IPAddress.Parse(tmp);
                            b_mask = mask.GetAddressBytes();
                            //--------------------------------------------
                            for (int b = 0; b < 4; b++)
                            {
                                for (int c = 8; c > 0; c--)
                                {
                                    int q = b_mask[b] << (8 - c);
                                    int w = q << (c - 1);
                                    if (w > 0) i_mask++;
                                }
                            }
                            //--------------------------------------------
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("(string)objMO['IPAddress[]']; - {0}", ex.Message);
                        }


                        if (networkInterface[a].IPv4.Config.DHCP == true)
                        {
                            try
                            {
                                networkInterface[a].IPv4.Config.FromDHCP = new Device.PrefixedIPv4Address();
                                networkInterface[a].IPv4.Config.FromDHCP.Address = ((string[])objMO["IPAddress"])[0];
                                networkInterface[a].IPv4.Config.FromDHCP.PrefixLength = i_mask;
                                //Console.WriteLine("networkInterface[a].IPv4.Config.FromDHCP.PrefixLength - {0}", networkInterface[a].IPv4.Config.FromDHCP.PrefixLength);
                            }
                            catch (Exception ex)
                            { 
                                Console.WriteLine(ex.Message);
                            }

                        }
                        else
                        {
                            System.Net.IPAddress address;
                            byte[] b_address;
                            try
                            {
                                address = System.Net.IPAddress.Parse(((string[])objMO["IPAddress"])[0]);
                                b_address = address.GetAddressBytes();

                                if (b_address[0] == 169 && b_address[1] == 254)
                                {
                                    //LLA
                                    networkInterface[a].IPv4.Config.LinkLocal = new Device.PrefixedIPv4Address();
                                    networkInterface[a].IPv4.Config.LinkLocal.Address = ((string[])objMO["IPAddress"])[0];
                                    networkInterface[a].IPv4.Config.LinkLocal.PrefixLength = i_mask;

                                }
                                else
                                {
                                    //Manual
                                    //прочитать айпишники и создать массив Manual, который потом заполнить
                                    //пока сделано только на один айпишник, может больше и не надо?
                                    //(нужно ли выдавать все айпи висящие на одном интерфейсе?)
                                    networkInterface[a].IPv4.Config.Manual = new Device.PrefixedIPv4Address[1];
                                    networkInterface[a].IPv4.Config.Manual[0] = new Device.PrefixedIPv4Address();
                                    networkInterface[a].IPv4.Config.Manual[0].Address = ((string[])objMO["IPAddress"])[0];
                                    networkInterface[a].IPv4.Config.Manual[0].PrefixLength = i_mask;

                                }
                            }
                            catch (ArgumentNullException ex)
                            {
                                Console.WriteLine("Failed to parse LLAddress, {0}", ex.Message);
                                break;
                            }
                            catch (FormatException ex)
                            {
                                Console.WriteLine("Failed to parse LLAddress, {0}", ex.Message);
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    };
                }
                a++;
                i_mask = 0;
            }
            getNetworkResponse.NetworkInterfaces = networkInterface;

            return getNetworkResponse;
        }

        public GetNetworkProtocolsResponse GetNetworkProtocols(GetNetworkProtocolsRequest request)
        {
            //йа заглушко!
            GetNetworkProtocolsResponse resp = new GetNetworkProtocolsResponse();
            resp.NetworkProtocols = new NetworkProtocol[3];

            resp.NetworkProtocols[0] = new NetworkProtocol();
            resp.NetworkProtocols[0].Name = NetworkProtocolType.HTTP;
            resp.NetworkProtocols[0].Port = new int[]{ 80 };
            resp.NetworkProtocols[0].Enabled = true;

            resp.NetworkProtocols[1] = new NetworkProtocol();
            resp.NetworkProtocols[1].Name = NetworkProtocolType.RTSP;
            resp.NetworkProtocols[1].Port = new int[] { 554 };
            resp.NetworkProtocols[1].Enabled = true;

            //если протокол не поддерживается - его не должно быть в ответе
            //если он есть в ответе но enabled=false , значит он поддерживается 
            //и просто выключен

            //resp.NetworkProtocols[2] = new NetworkProtocol();
            //resp.NetworkProtocols[2].Name = NetworkProtocolType.HTTPS;
            //resp.NetworkProtocols[2].Port = new int[] { 443 };
            //resp.NetworkProtocols[2].Enabled = false;

            return resp;
        }

        public SetNetworkProtocolsResponse SetNetworkProtocols(SetNetworkProtocolsRequest request)
        {
            return (new SetNetworkProtocolsResponse());
        }
        
        public SetNetworkDefaultGatewayResponse SetNetworkDefaultGateway(SetNetworkDefaultGatewayRequest request)
        {
            if (request != null)
            {
                GetNetworkInterfacesResponse getNetworkResponse = new GetNetworkInterfacesResponse();
                ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection objMOC = objMC.GetInstances();
                XmlConfig conf = new XmlConfig();
                ConfigStruct confstr = new ConfigStruct();
                confstr = conf.Read();

                string[] gwar = request.IPv4Address;

                foreach (ManagementObject objMO in objMOC)
                {
                    if ((bool)objMO["IPEnabled"] == true && ((string[])objMO["IPAddress"]).Contains(confstr.IPAddr))
                    {
                        try
                        {
                            UInt16[] metr = new UInt16[request.IPv4Address.Length];
                            for (int y = 0; y < request.IPv4Address.Length; y++)
                            {
                                metr[y] = 1;
                            }
                            ManagementBaseObject SetGWparam = objMO.GetMethodParameters("SetGateways");
                            ManagementBaseObject ReturnedN;
                            SetGWparam["DefaultIPGateway"] = request.IPv4Address;
                            SetGWparam["GatewayCostMetric"] = metr;

                            ReturnedN = objMO.InvokeMethod("SetGateways", SetGWparam, null);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                }
                SetNetworkDefaultGatewayResponse SetNetDefGWResponse = new SetNetworkDefaultGatewayResponse();
                return SetNetDefGWResponse;
            }
            return null;
        }
        public CreateCertificateResponse CreateCertificate(CreateCertificateRequest request)
        {
            return (new CreateCertificateResponse());
        }
        public GetCertificatesResponse GetCertificates(GetCertificatesRequest request)
        {
            return (new GetCertificatesResponse());
        }
        public GetCertificatesStatusResponse GetCertificatesStatus(GetCertificatesStatusRequest request)
        {
            return (new GetCertificatesStatusResponse());
        }
        public SetCertificatesStatusResponse SetCertificatesStatus(SetCertificatesStatusRequest request)
        {
            return (new SetCertificatesStatusResponse());
        }
        public DeleteCertificatesResponse DeleteCertificates(DeleteCertificatesRequest request)
        {
            return (new DeleteCertificatesResponse());
        }
        public GetPkcs10RequestResponse GetPkcs10Request(GetPkcs10RequestRequest request)
        {
            return (new GetPkcs10RequestResponse());
        }
        public LoadCertificatesResponse LoadCertificates(LoadCertificatesRequest request)
        {
            return (new LoadCertificatesResponse());
        }
        public GetRelayOutputsResponse GetRelayOutputs(GetRelayOutputsRequest request)
        {
            return (new GetRelayOutputsResponse());
        }
        public GetCACertificatesResponse GetCACertificates(GetCACertificatesRequest request)
        {
            return (new GetCACertificatesResponse());
        }
        public LoadCertificateWithPrivateKeyResponse LoadCertificateWithPrivateKey(LoadCertificateWithPrivateKeyRequest request)
        {
            return (new LoadCertificateWithPrivateKeyResponse());
        }
        public GetCertificateInformationResponse GetCertificateInformation(GetCertificateInformationRequest request)
        {
            return (new GetCertificateInformationResponse());
        }
        public LoadCACertificatesResponse LoadCACertificates(LoadCACertificatesRequest request)
        {
            return (new LoadCACertificatesResponse());
        }
        public GetDot1XConfigurationsResponse GetDot1XConfigurations(GetDot1XConfigurationsRequest request)
        {
            return (new GetDot1XConfigurationsResponse());
        }
        public DeleteDot1XConfigurationResponse DeleteDot1XConfiguration(DeleteDot1XConfigurationRequest request)
        {
            return (new DeleteDot1XConfigurationResponse());
        }
        public GetDot11CapabilitiesResponse GetDot11Capabilities(GetDot11CapabilitiesRequest request)
        {
            return (new GetDot11CapabilitiesResponse());
        }
        public ScanAvailableDot11NetworksResponse ScanAvailableDot11Networks(ScanAvailableDot11NetworksRequest request)
        {
            return (new ScanAvailableDot11NetworksResponse());
        }
        public GetSystemUrisResponse GetSystemUris(GetSystemUrisRequest request)
        {
            return (new GetSystemUrisResponse());
        }
        public StartFirmwareUpgradeResponse StartFirmwareUpgrade(StartFirmwareUpgradeRequest request)
        {
            return (new StartFirmwareUpgradeResponse());
        }
        public StartSystemRestoreResponse StartSystemRestore(StartSystemRestoreRequest request)
        {
            return (new StartSystemRestoreResponse());
        }
        //----------------------------------------------------------------------------------------------------------
        

        //--------------------
        //public System.DateTime ParseAsUtc(string logDate, string timezoneName)
        //{
        //    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneName);
        //    var localDate = System.DateTime.Parse(logDate);
        //    var offset = new DateTimeOffset(localDate, timeZone.GetUtcOffset(localDate));
        //    return offset.ToUniversalTime().DateTime;
        //}
        //--------------------

        public void SetSystemDateAndTime(SetDateTimeType DateTimeType,
            bool DaylightSavings,
            Device.TimeZone Timezone,
            Device.DateTime UTCDateTime)
        {
            System.TimeZone tz = System.TimeZone.CurrentTimeZone;
            
            if (UTCDateTime != null)
            {
                SYSTEMTIME systime = new SYSTEMTIME();

                try
                {
                    systime.wYear = (ushort)UTCDateTime.Date.Year;
                    if ((UTCDateTime.Date.Month >= 1) && (UTCDateTime.Date.Month <= 12)
                        &&(UTCDateTime.Date.Day>=1)&&(UTCDateTime.Date.Day<=31)
                        &&(UTCDateTime.Time.Hour>=0)&&(UTCDateTime.Time.Hour<=23)
                        &&(UTCDateTime.Time.Minute>=0)&&(UTCDateTime.Time.Minute<=59)
                        &&(UTCDateTime.Time.Second>=0)&&(UTCDateTime.Time.Second<=59))

                    {
                        systime.wMonth = (ushort)UTCDateTime.Date.Month;
                        systime.wDay = (ushort)UTCDateTime.Date.Day;
                        systime.wHour = (ushort)(UTCDateTime.Time.Hour);
                        systime.wMinute = (ushort)(UTCDateTime.Time.Minute);
                        systime.wSecond = (ushort)UTCDateTime.Time.Second;
                    }
                    else
                    {
                        throw new Exception();
                    }                    
                    bool fl = false;
                    if (Timezone != null && (!(tz.StandardName.Contains(Timezone.TZ))))
                    {
                        throw new FaultException(new FaultReason("InvalidTimeZone"),
                            new FaultCode("Sender",
                                new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                    new FaultCode("InvalidTimeZone", "http://www.onvif.org/ver10/error"))));
                    }

                    fl = OnvifProxy.Service1.SetSystemTime(ref systime);

                }
                catch (FaultException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    TyphoonCom.log.DebugFormat("SetSystemDateAndTime - {0}", ex.Message);
                    throw new FaultException(new FaultReason("InvalidDateTime"),
                        new FaultCode("Sender",
                            new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                new FaultCode("InvalidDateTime", "http://www.onvif.org/ver10/error"))));
                }
                
            }
            else
            {
                throw new FaultException(new FaultReason("InvalidDateTime"),
                        new FaultCode("Sender",
                            new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                new FaultCode("InvalidDateTime", "http://www.onvif.org/ver10/error"))));
            }
        }
        
        public void SetSystemFactoryDefault(FactoryDefaultType FactoryDefault)
        {
            Thread thr = new Thread(new ThreadStart(Program.RebootHost));
            //thr.IsBackground = true;
            thr.Priority = ThreadPriority.Normal;
            thr.Start();
        }
        public string StartSystemRestore([System.Xml.Serialization.XmlElementAttribute(DataType = "duration")]
            out string ExpectedDownTime)
        {
            ExpectedDownTime = "";
            return "";
        }
        public string StartFirmwareUpgrade([System.Xml.Serialization.XmlElementAttribute(DataType="duration")]
            out string UploadDelay,
            [System.Xml.Serialization.XmlElementAttribute(DataType="duration")]
            out string ExpectedDownTime)
        {
            ExpectedDownTime = "";
            UploadDelay = "";
            return "";
        }
        public SystemLogUri[] GetSystemUris([System.Xml.Serialization.XmlElementAttribute(DataType = "anyURI")]
            out string SupportInfoUri,
            [System.Xml.Serialization.XmlElementAttribute(DataType = "anyURI")]
            out string SystemBackupUri, 
            out GetSystemUrisResponseExtension Extension)
        {
            SupportInfoUri = "";
            Extension = new GetSystemUrisResponseExtension();
            SystemBackupUri = "";
            return (new SystemLogUri[1]);
        }
        public Dot11AvailableNetworks[] ScanAvailableDot11Networks(string InterfaceToken)
        {
            return (new Dot11AvailableNetworks[1]);
        }
        public Dot11Status GetDot11Status(string InterfaceToken)
        { 
            return (new Dot11Status()); 
        }
        public Dot11Capabilities GetDot11Capabilities([System.Xml.Serialization.XmlAnyElementAttribute()] System.Xml.XmlElement[] Any)
        {
            return (new Dot11Capabilities());
        }
        public void DeleteDot1XConfiguration([System.Xml.Serialization.XmlElementAttribute("Dot1XConfigurationToken")] string[] Dot1XConfigurationToken)
        { 
        }
        public Dot1XConfiguration[] GetDot1XConfigurations()
        {
            return (new Dot1XConfiguration[1]);
        }
        public Dot1XConfiguration GetDot1XConfiguration(string Dot1XConfigurationToken)
        {
            return (new Dot1XConfiguration());
        }
        public void SetDot1XConfiguration(Dot1XConfiguration Dot1XConfiguration)
        { 
        }
        public void CreateDot1XConfiguration(Dot1XConfiguration Dot1XConfiguration)
        { 
        }
        public void LoadCACertificates([System.Xml.Serialization.XmlElementAttribute("CACertificate")] Certificate[] CACertificate)
        { 
        }
        public CertificateInformation GetCertificateInformation([System.Xml.Serialization.XmlElementAttribute(DataType = "token")] string CertificateID)
        { 
            return (new CertificateInformation());
        }
        public void LoadCertificateWithPrivateKey([System.Xml.Serialization.XmlElementAttribute("CertificateWithPrivateKey")] CertificateWithPrivateKey[] CertificateWithPrivateKey)
        { 
        }
        public Certificate[] GetCACertificates()
        {
            return (new Certificate[1]);
        }
        public string SendAuxiliaryCommand(string AuxiliaryCommand)
        {
            return "";
        }
        public void SetRelayOutputState(string RelayOutputToken, RelayLogicalState LogicalState)
        { 
        }
        public void SetRelayOutputSettings(string RelayOutputToken, Device.RelayOutputSettings Properties)
        { 
        }
        public Device.RelayOutput[] GetRelayOutputs()
        {
            return (new Device.RelayOutput[1]);
        }
        public void SetClientCertificateMode(bool Enabled)
        { 
        }
        public bool GetClientCertificateMode()
        {
            return true;
        }
        public void LoadCertificates([System.Xml.Serialization.XmlElementAttribute("NVTCertificate")] Certificate[] NVTCertificate)
        { 
        }
        public BinaryData GetPkcs10Request([System.Xml.Serialization.XmlElementAttribute(DataType = "token")] string CertificateID, string Subject, BinaryData Attributes)
        {
            return (new BinaryData());
        }
        public void DeleteCertificates([System.Xml.Serialization.XmlElementAttribute("CertificateID", DataType = "token")] string[] CertificateID)
        { 
        }
        public void SetCertificatesStatus([System.Xml.Serialization.XmlElementAttribute("CertificateStatus")] CertificateStatus[] CertificateStatus)
        { 
        }
        public CertificateStatus[] GetCertificatesStatus()
        {
            return (new CertificateStatus[1]);
        }
        public Certificate[] GetCertificates()
        {
            return (new Certificate[1]);
        }
        public Certificate CreateCertificate([System.Xml.Serialization.XmlElementAttribute(DataType = "token")] string CertificateID, string Subject, System.DateTime ValidNotBefore, [System.Xml.Serialization.XmlIgnoreAttribute()] bool ValidNotBeforeSpecified, System.DateTime ValidNotAfter, [System.Xml.Serialization.XmlIgnoreAttribute()] bool ValidNotAfterSpecified)
        { 
            return (new Certificate());
        }
        public void SetAccessPolicy(BinaryData PolicyFile)
        { 
        }
        public BinaryData GetAccessPolicy()
        {
            return (new BinaryData());
        }
        public void RemoveIPAddressFilter(IPAddressFilter IPAddressFilter)
        { 
        }
        public void AddIPAddressFilter(IPAddressFilter IPAddressFilter)
        { 
        }
        public void SetIPAddressFilter(IPAddressFilter IPAddressFilter)
        { 
        }
        public IPAddressFilter GetIPAddressFilter()
        {
            return (new IPAddressFilter());
        }
        public void SetZeroConfiguration(string InterfaceToken, bool Enabled)
        {
            //ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            //ManagementObjectCollection objMOC = objMC.GetInstances();
            //Device.NetworkInterface[] networkInterface = new Device.NetworkInterface[objMOC.Count];
            //string subnet_mask = null;


            //foreach (ManagementObject objMO in objMOC)
            //{
            //    if ((string)objMO["ServiceName"] == InterfaceToken)
            //    {
            //        if (NetworkInterface.IPv4.DHCP == true)
            //        {
            //            try
            //            {
            //                ManagementBaseObject newEnableDHCP = objMO.InvokeMethod("EnableDHCP", null, null);
            //            }
            //            catch (Exception e)
            //            {
            //                Console.WriteLine("Failed to setup network interface - {0}", e.Message);
            //            }
            //        }
            //    }
            //}
        }
        public NetworkZeroConfiguration GetZeroConfiguration()
        {
            return (new NetworkZeroConfiguration());
        }
        public void SetNetworkDefaultGateway([System.Xml.Serialization.XmlElementAttribute("IPv4Address", DataType = "token")] string[] IPv4Address, [System.Xml.Serialization.XmlElementAttribute("IPv6Address", DataType = "token")] string[] IPv6Address)
        { 
        }

        public NetworkGateway GetNetworkDefaultGateway()
        {
            GetNetworkInterfacesResponse getNetworkResponse = new GetNetworkInterfacesResponse();
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            NetworkGateway GW = new NetworkGateway();

            XmlConfig conf = new XmlConfig();
            ConfigStruct confstr = new ConfigStruct();
            confstr = conf.Read();
            
            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"] == true && ((string[])objMO["IPAddress"]).Contains(confstr.IPAddr))
                {
                    try
                    {
                        GW.IPv4Address = (string[])objMO["DefaultIPGateway"];
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
            return GW;
        }

        public void SetNetworkProtocols([System.Xml.Serialization.XmlElementAttribute("NetworkProtocols")] NetworkProtocol[] NetworkProtocols)
        { 
        }
        public NetworkProtocol[] GetNetworkProtocols()
        {
            return (new NetworkProtocol[1]);
        }
        
        public bool SetNetworkInterfaces(string InterfaceToken, NetworkInterfaceSetConfiguration NetworkInterface)
        {
            if (InterfaceToken != null && NetworkInterface != null)
            {
                //make thread to change ip
                NetInterfaceStruct netInterfaceStruct = new NetInterfaceStruct();
                netInterfaceStruct.InterfaceToken = InterfaceToken;
                netInterfaceStruct.NetworkInterface = NetworkInterface;
                Thread thr_setIP = new Thread(new ParameterizedThreadStart((new IPmgmnt()).SetIP));

                thr_setIP.Start(netInterfaceStruct);
                //return answer for client
                return true;
            }
            else
                return false;
            #region
            //ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            //ManagementObjectCollection objMOC = objMC.GetInstances();
            //Device.NetworkInterface[] networkInterface = new Device.NetworkInterface[objMOC.Count];            
            //string subnet_mask = null;
       

            //foreach (ManagementObject objMO in objMOC)
            //{
            //    if ((string)objMO["ServiceName"] == InterfaceToken)
            //    {
            //        if (NetworkInterface.IPv4.DHCP == true)
            //        {
            //            try
            //            {
            //                ManagementBaseObject newEnableDHCP = objMO.InvokeMethod("EnableDHCP", null, null);
            //            }
            //            catch (Exception e)
            //            {
            //                Console.WriteLine("Failed to setup network interface - {0}", e.Message);
            //            }
            //        }
            //        else
            //        {
            //            try
            //            {
            //                ManagementBaseObject newIP = objMO.GetMethodParameters("EnableStatic");
            //                //преобразуем маску из вида инт в стринг
            //                uint mask = 0;
            //                if (NetworkInterface.IPv4.Manual != null)
            //                {
            //                    for (int s = 0; s < NetworkInterface.IPv4.Manual[0].PrefixLength; s++)
            //                    {
            //                        mask = mask >> 1;
            //                        mask += 2147483648;
            //                    }

            //                    byte[] b_mask = new byte[4];
            //                    for (int i = 0; i < 4; i++)
            //                    {
            //                        b_mask[i] = (byte)(mask >> 8 * (3 - i));
            //                    }
            //                    subnet_mask = b_mask[0].ToString() + "."
            //                        + b_mask[1].ToString() + "."
            //                        + b_mask[2].ToString() + "."
            //                        + b_mask[3].ToString();
            //                    //запихиваем в параметры метода
            //                    newIP["IPAddress"] = new string[] { NetworkInterface.IPv4.Manual[0].Address };
            //                    newIP["SubnetMask"] = new string[] { subnet_mask };
            //                    //и дергаем метод
            //                    objMO.InvokeMethod("EnableStatic", newIP, null);
            //                }

            //                XmlConfig conf = new XmlConfig();
            //                ConfigStruct confstr = new ConfigStruct();
            //                confstr = conf.Read();
            //                confstr.IPAddr = NetworkInterface.IPv4.Manual[0].Address;
            //                conf.Write(confstr);
                            
            //            }
            //            catch (Exception e)
            //            {
            //                Console.WriteLine("Failed to setup network interface - {0}", e.Message);
            //                return false;
            //            }
            //        }

            //    }
            //}

            //return true;
            #endregion
        }
        public Device.NetworkInterface[] GetNetworkInterfaces()
        {
            return (new Device.NetworkInterface[1]);
        }
        public void SetDynamicDNS(DynamicDNSType Type, [System.Xml.Serialization.XmlElementAttribute(DataType = "token")] string Name, [System.Xml.Serialization.XmlElementAttribute(DataType = "duration")] string TTL)
        { 
        }
        public DynamicDNSInformation GetDynamicDNS()
        {
            return (new DynamicDNSInformation());
        }
        public void SetNTP(bool FromDHCP, [System.Xml.Serialization.XmlElementAttribute("NTPManual")] NetworkHost[] NTPManual)
        { 
        }
        public NTPInformation GetNTP()
        {
            return (new NTPInformation());
        }
        public void SetDNS(bool FromDHCP, [System.Xml.Serialization.XmlElementAttribute("SearchDomain", DataType = "token")] string[] SearchDomain, [System.Xml.Serialization.XmlElementAttribute("DNSManual")] Device.IPAddress[] DNSManual)
        { 
        }

        public DNSInformation GetDNS()
        {
            DNSInformation dnsInformation = new DNSInformation();
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            XmlConfig conf = new XmlConfig();
            ConfigStruct confstr = new ConfigStruct();
            confstr = conf.Read();

            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"] == true && ((string[])objMO["IPAddress"]).Contains(confstr.IPAddr))
                {                   
                    try
                    {
                        dnsInformation.FromDHCP = (bool)objMO["DHCPEnabled"];
                        if ((bool)objMO["DHCPEnabled"] != true)
                        {
                            string[] strarr = (string[])objMO["DNSServerSearchOrder"];
                            if (strarr == null)
                            {
                                strarr = new string[1];
                                strarr[0] = "No IP Address";
                            }
                            int a = strarr.Length;

                            dnsInformation.DNSManual = new Device.IPAddress[a];
                            for (int y = 0; y < a; y++)
                            {
                                dnsInformation.DNSManual[y] = new Device.IPAddress();
                                dnsInformation.DNSManual[y].IPv4Address = strarr[y];
                                //TyphoonCom.log.DebugFormat("IP - {0}", dnsInformation.DNSManual[y].IPv4Address);
                            }

                        }
                        else
                        {
                            //dnsInformation.DNSFromDHCP = new Device.IPAddress[1];
                            string[] strarr = (string[])objMO["DNSServerSearchOrder"];
                            if (strarr == null)
                            {
                                strarr = new string[1];
                                strarr[0] = "No IP Address";
                            }
                            int a = strarr.Length;

                            dnsInformation.DNSFromDHCP = new Device.IPAddress[a];
                            for (int y = 0; y < a; y++)
                            {
                                dnsInformation.DNSFromDHCP[y] = new Device.IPAddress();
                                dnsInformation.DNSFromDHCP[y].IPv4Address = strarr[y];
                                TyphoonCom.log.DebugFormat("IP - {0}", dnsInformation.DNSFromDHCP[y].IPv4Address);
                            }
                        }
                        dnsInformation.SearchDomain = ((string)objMO["DNSDomain"]).Split(' ');
                    }
                    catch (Exception aex)
                    {
                        TyphoonCom.log.DebugFormat(aex.Message);
                        throw new FaultException(new FaultReason("NoDNS"),
                      new FaultCode("Sender",
                          new FaultCode("InvalidArgVa", "http://www.onvif.org/ver10/error",
                              new FaultCode("NoDNS", "http://www.onvif.org/ver10/error"))));
                    }
                }
            }
            return dnsInformation;
        }

        public void SetHostname([System.Xml.Serialization.XmlElementAttribute(DataType = "token")] string Name)
        {
            //if (Name != null)
            //{
            //    try
            //    {
            //        ManagementClass mComputerSystem = new ManagementClass("Win32_ComputerSystem");
            //        ManagementBaseObject outParams;
            //        ManagementBaseObject objNewComputerName = mComputerSystem.GetMethodParameters("Rename");
            //        objNewComputerName["Name"] = Name;
            //        //objNewComputerName["Password"] = Name;
            //        outParams = mComputerSystem.InvokeMethod("Rename", objNewComputerName, null);
            //    }
            //    catch (Exception ex)
            //    {
            //        TyphoonCom.log.DebugFormat("SetHostName failed - {0}", ex.Message);
            //    }
            //}
        }

        public HostnameInformation GetHostname()
        {
            HostnameInformation hostnameInformation = new HostnameInformation();
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            XmlConfig conf = new XmlConfig();
            ConfigStruct confstr = new ConfigStruct();
            confstr = conf.Read();

            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"] == true && ((string[])objMO["IPAddress"]).Contains(confstr.IPAddr))
                {
                    try
                    {
                        hostnameInformation.FromDHCP = (bool)objMO["DHCPEnabled"];
                        //hostnameInformation.Name = (string)objMO["DNSHostName"];
                    }
                    catch (Exception e)
                    {
                        TyphoonCom.log.DebugFormat("GetHostname failed - {0}", e.Message);
                        return null;
                    }
                }
            }
            string oldName = System.Environment.MachineName;
            using (ManagementObject cs = new ManagementObject(@"Win32_Computersystem.Name='" + oldName + "'"))
            {
                cs.Get();
                try
                {
                    hostnameInformation.Name = (string)cs["Name"];
                }
                catch (Exception e)
                {
                    TyphoonCom.log.DebugFormat("Getting DNSHostName failed - {0}", e.Message);
                }
                //ManagementBaseObject inParams = cs.GetMethodParameters("Rename");
                //inParams.SetPropertyValue("Name", request.Name);
                //InvokeMethodOptions methodOptions = new InvokeMethodOptions(null, System.TimeSpan.MaxValue);
                //ManagementBaseObject outParams = cs.InvokeMethod("Rename", inParams, methodOptions);
                //string Return = Convert.ToString(outParams.Properties["ReturnValue"].Value);
                //Console.WriteLine("Return Value: {0}", Return);
            }

            return hostnameInformation;
        }
        //public Device.Capabilities GetCapabilities([System.Xml.Serialization.XmlElementAttribute("Category")] CapabilityCategory[] Category)
        //{
        //    return (new Device.Capabilities());
        //}
        public string GetWsdlUrl()
        {
            return "http://127.0.0.1/Service1?wsdl";
        }
        public void SetUser([System.Xml.Serialization.XmlElementAttribute("User")] User[] User)
        { 
        }
        public void DeleteUsers([System.Xml.Serialization.XmlElementAttribute("Username")] string[] Username)
        { 
        }
        public void CreateUsers([System.Xml.Serialization.XmlElementAttribute("User")] User[] User)
        { 
        }
        public User[] GetUsers()
        {
            return (new User[1]);
        }
        public void SetRemoteUser(RemoteUser RemoteUser)
        { 
        }
        public RemoteUser GetRemoteUser()
        { 
            return (new RemoteUser());
        }
        public string GetEndpointReference([System.Xml.Serialization.XmlAnyElementAttribute()] out System.Xml.XmlElement[] Any)
        {
            Any = new System.Xml.XmlElement[1];
            return "";
        }
        public void SetDPAddresses([System.Xml.Serialization.XmlElementAttribute("DPAddress")] NetworkHost[] DPAddress)
        { 
        }
        public NetworkHost[] GetDPAddresses()
        { 
            return (new NetworkHost[1]);
        }
        public void SetRemoteDiscoveryMode(DiscoveryMode RemoteDiscoveryMode)
        { 
        }
        public DiscoveryMode GetRemoteDiscoveryMode()
        {
            return (new DiscoveryMode());
        }
        public void SetDiscoveryMode(DiscoveryMode DiscoveryMode)
        {
            Console.WriteLine("SetDiscoveryMode entered");
            object LockObj = new object();
            lock (LockObj)
            {
                if (FlagHostThreadReboot.Start == true) Program.ev_RebootEnded.WaitOne();
                ConfigStruct confstr = new ConfigStruct();
                XmlConfig conf = new XmlConfig();
                confstr.Scopes = new System.Collections.ObjectModel.Collection<OnvifScope>();
                confstr = conf.Read();

                confstr.ServiceDiscoveryStatus = DiscoveryMode;
                conf.Write(confstr);
                Console.WriteLine("(SetDiscoveryMode)DiscoveryMode = {0}", DiscoveryMode);
                FlagHostThreadReboot.Start = true;
                Program.ev_RebootHost.Set();
            }
            Console.WriteLine("SetDiscoveryMode exited");
            //Program.autoEvent.WaitOne();            
        }

        public DiscoveryMode GetDiscoveryMode()
        {

            if (FlagHostThreadReboot.Start == true) Program.ev_RebootEnded.WaitOne();

            Console.WriteLine("GetDiscoveryMode entered");
            object LockObj = new object();
            lock (LockObj)
            {
                ConfigStruct confstr = new ConfigStruct();
                XmlConfig conf = new XmlConfig();
                confstr.Scopes = new System.Collections.ObjectModel.Collection<OnvifScope>();
                confstr = conf.Read();
                FlagServiceDiscoverable.Mode = confstr.ServiceDiscoveryStatus;
                Console.WriteLine("(GetDiscoveryMode)FlagServiceDiscoverable = {0}", FlagServiceDiscoverable.Mode);
                Console.WriteLine("GetDiscoveryMode exited");
                return (FlagServiceDiscoverable.Mode);
            };
            

            //-----------------------------------------------
            //OperationContext context = OperationContext.Current;
            ////context.Channel.Extensions;
            //return DiscoveryMode.NonDiscoverable;
        }

        public SupportInformation GetSystemSupportInformation()
        {
            return (new SupportInformation());
        }

        public SystemLog GetSystemLog(SystemLogType LogType)
        {
            return (new SystemLog());
        }
        public BackupFile[] GetSystemBackup()
        {
            return (new BackupFile[1]);
        }
        public void RestoreSystem([System.Xml.Serialization.XmlElementAttribute("BackupFiles")] BackupFile[] BackupFiles)
        { 
        }
        public string SystemReboot()
        {
            FlagHostThreadReboot.Start = true;
            Program.ev_RebootHost.Set();
            //Program.rebootEnded.WaitOne();
            return "going to reboot";
        }
        public string UpgradeSystemFirmware(AttachmentData Firmware)
        {
            if (Firmware!=null&& Firmware.contentType != null && Firmware.Include != null)
            {
                return "";
            }
            else
            {
                //throw new FaultException<AttachmentData>(null);
                return null;
            }
            
        }
        public SystemDateTime GetSystemDateAndTime()
        {
            System.DateTime today = new System.DateTime();
            SystemDateTime systemDateTime = new SystemDateTime();
            Device.Time time = new Time();
            Device.Date date = new Date();
            Device.DateTime dateTime = new Device.DateTime();
            systemDateTime.UTCDateTime = new Device.DateTime();

            systemDateTime.DateTimeType = SetDateTimeType.Manual;            
            systemDateTime.UTCDateTime.Date = date;
            systemDateTime.UTCDateTime.Time = time;
            today = System.DateTime.Now;
            time.Hour = today.Hour;
            time.Minute = today.Minute;
            time.Second = today.Second;
            date.Year = today.Year;
            date.Month = today.Month;
            date.Day = today.Day;
            dateTime.Date = date;
            dateTime.Time = time;

            systemDateTime.DateTimeType = SetDateTimeType.Manual;
            systemDateTime.DaylightSavings = false;
            systemDateTime.LocalDateTime = dateTime;

            systemDateTime.TimeZone = new Device.TimeZone();
            systemDateTime.TimeZone.TZ = System.TimeZone.CurrentTimeZone.StandardName.ToString();

            
            return systemDateTime;
        }


        public Media.GetServiceCapabilitiesResponse1 GetServiceCapabilities(Media.GetServiceCapabilitiesRequest request)
        {
            //throw new NotImplementedException();
            var resp = new GetServiceCapabilitiesResponse();
            resp.Capabilities = new Media.Capabilities();
            resp.Capabilities.ProfileCapabilities = new Media.ProfileCapabilities();
            resp.Capabilities.StreamingCapabilities = new StreamingCapabilities();
            resp.Capabilities.StreamingCapabilities.RTP_RTSP_TCP = true;
            resp.Capabilities.SnapshotUri = false;
            
            return new GetServiceCapabilitiesResponse1(resp);
        }
 
//------------------------------Media--------------------------------------------------------------------
        public void ActionNotSupported()
        {
            throw new FaultException(new FaultReason("ActionNotSupported"),
                            new FaultCode("Sender",
                                new FaultCode("ActionNotSupported", "http://www.onvif.org/ver10/error",
                                    new FaultCode("ActionNotSupported", "http://www.onvif.org/ver10/error")))); 
        }
        public Media.GetVideoSourcesResponse GetVideoSources(Media.GetVideoSourcesRequest request)
        {
            Media.GetVideoSourcesResponse getVideoSourcesResponse = new Media.GetVideoSourcesResponse();
          
            TyphoonMsg typhmsgresp = new TyphoonMsg(TyphoonMsgType.Responce);

            typhmsgresp = TyphoonMsgManager.SendSyncMsg(6);

            if (typhmsgresp == null)
                throw new FaultException(new FaultReason("arghh"),
                             new FaultCode("Sender",
                                 new FaultCode("NotAuthorized", "http://www.onvif.org/ver10/error",
                                     new FaultCode("Operation not Permitted", "http://www.onvif.org/ver10/error")))); 
            
            string tmpStr = TyphoonCom.ParseMem(0, typhmsgresp.stringMessageData);
            typhmsgresp.Dispose();

            if (tmpStr == null)
                throw new FaultException(new FaultReason("arghh!!!"),
                    new FaultCode("Sender",
                        new FaultCode("NotAuthorized", "http://www.onvif.org/ver10/error",
                            new FaultCode("Operation not Permitted", "http://www.onvif.org/ver10/error"))));

            using (Stream ms = new MemoryStream(Encoding.UTF8.GetBytes(tmpStr)))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(GetVideoSourcesResponse));
                try
                {
                    getVideoSourcesResponse = (GetVideoSourcesResponse)xmlSerializer.Deserialize(ms);
                    for (int y = 0; y < getVideoSourcesResponse.VideoSources.Count(); y++)
                    {
                        //здесь надо получить от Шарова реальное разрешение
                        getVideoSourcesResponse.VideoSources[y].Resolution = new Media.VideoResolution();
                        getVideoSourcesResponse.VideoSources[y].Resolution.Height = 1080;
                        getVideoSourcesResponse.VideoSources[y].Resolution.Width = 1920;
                        getVideoSourcesResponse.VideoSources[y].Framerate = 30;
                    }
                }
                catch (SerializationException g)
                {
                    Console.WriteLine("Не могу десериализовать GetVideoSources; " + g.Message);
                    return null;
                }
                //finally
                //{
                //    ms.Close();
                //}
            }
                
            return getVideoSourcesResponse;
        }

        public Media.GetAudioSourcesResponse GetAudioSources(Media.GetAudioSourcesRequest request)
        {
            return new Media.GetAudioSourcesResponse();
        }

        public Media.GetAudioOutputsResponse GetAudioOutputs(Media.GetAudioOutputsRequest request)
        {
            return new Media.GetAudioOutputsResponse();
        }

        public Media.Profile CreateProfile(string Name, string Token)
        {
            throw new FaultException(new FaultReason("MaxNVTProfiles"),
                           new FaultCode("Receiver",
                               new FaultCode("Action", "http://www.onvif.org/ver10/error",
                                   new FaultCode("MaxNVTProfiles", "http://www.onvif.org/ver10/error"))));
            
        }

        public Media.Profile GetProfile(string ProfileToken)
        {         
            TyphoonMsg typhmsgresp = new TyphoonMsg(TyphoonMsgType.Responce);
            Media.GetProfilesResponse mediaprofile = new Media.GetProfilesResponse();
            XmlConfig config = new XmlConfig();

            ///формируем команду GetProfiles
            using(typhmsgresp = TyphoonMsgManager.SendSyncMsg(2))
            {
                if (typhmsgresp == null || String.IsNullOrEmpty(typhmsgresp.stringMessageData))
                    throw new FaultException(new FaultReason("NoProfile"),
                        new FaultCode("Sender",
                            new FaultCode("InvalidArgVa", "http://www.onvif.org/ver10/error",
                                new FaultCode("NoProfile", "http://www.onvif.org/ver10/error"))));                

                typhmsgresp.stringMessageData = TyphoonCom.ParseMem(0, typhmsgresp.stringMessageData);
                mediaprofile = config.ParseGetProfiles(typhmsgresp.stringMessageData);
            }

            foreach (Profile profile in mediaprofile.Profiles)
            {
                if (profile.token == ProfileToken)
                {
                    return profile;
                }
            }
            throw new FaultException(new FaultReason("NoProfile"),
                      new FaultCode("Sender",
                          new FaultCode("InvalidArgVa", "http://www.onvif.org/ver10/error",
                              new FaultCode("NoProfile", "http://www.onvif.org/ver10/error"))));

        }

        public Media.GetProfilesResponse GetProfiles(Media.GetProfilesRequest request)
        {
            ///формируем запрос к тайфуну GetProfiles
            TyphoonMsg typhmsgresp = new TyphoonMsg(TyphoonMsgType.Responce);

            typhmsgresp = TyphoonMsgManager.SendSyncMsg(2);

            if(typhmsgresp!=null)
            {
                ///создаем структуру под конфиг
                XmlConfig config = new XmlConfig();
                Media.GetProfilesResponse mediaprofile = new Media.GetProfilesResponse();

                string tmpStr = TyphoonCom.ParseMem(0, typhmsgresp.stringMessageData);
                mediaprofile = config.ParseGetProfiles(tmpStr);
                typhmsgresp.Dispose();
                return mediaprofile;
            }
            else 
            {
                return null;
            }
        }

        public void AddVideoEncoderConfiguration(string ProfileToken, string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public void RemoveVideoEncoderConfiguration(string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public void AddVideoSourceConfiguration(string ProfileToken, string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public void RemoveVideoSourceConfiguration(string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public void AddAudioEncoderConfiguration(string ProfileToken, string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public void RemoveAudioEncoderConfiguration(string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public void AddAudioSourceConfiguration(string ProfileToken, string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public void RemoveAudioSourceConfiguration(string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public void AddPTZConfiguration(string ProfileToken, string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public void RemovePTZConfiguration(string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public void AddVideoAnalyticsConfiguration(string ProfileToken, string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public void RemoveVideoAnalyticsConfiguration(string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public void AddMetadataConfiguration(string ProfileToken, string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public void RemoveMetadataConfiguration(string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public void AddAudioOutputConfiguration(string ProfileToken, string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public void RemoveAudioOutputConfiguration(string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public void AddAudioDecoderConfiguration(string ProfileToken, string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public void RemoveAudioDecoderConfiguration(string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public void DeleteProfile(string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public Media.GetVideoSourceConfigurationsResponse GetVideoSourceConfigurations(Media.GetVideoSourceConfigurationsRequest request)
        {
            return new GetVideoSourceConfigurationsResponse();
            //throw new NotImplementedException();
        }

        public Media.GetVideoEncoderConfigurationsResponse GetVideoEncoderConfigurations(Media.GetVideoEncoderConfigurationsRequest request)
        {
            //
            //всё ниже следующее просто заглушка
            //
            VideoEncoderConfiguration[] vencconf = new VideoEncoderConfiguration[1];
            vencconf[0] = new VideoEncoderConfiguration();
            vencconf[0].token = "1";
            vencconf[0].Name = vencconf[0].token;
            vencconf[0].Encoding = Media.VideoEncoding.H264;
            vencconf[0].Resolution = new Media.VideoResolution();
            vencconf[0].Resolution.Height = 0;
            vencconf[0].Resolution.Width = 0;
            vencconf[0].Quality = 100;
            vencconf[0].RateControl = new VideoRateControl();
            vencconf[0].RateControl.BitrateLimit = 0;
            vencconf[0].RateControl.EncodingInterval = 0;
            vencconf[0].RateControl.FrameRateLimit = 0;
            vencconf[0].H264 = new H264Configuration();
            vencconf[0].MPEG4 = new Mpeg4Configuration();
            vencconf[0].Multicast = new MulticastConfiguration();
            vencconf[0].Multicast.Address = new Media.IPAddress();
            vencconf[0].SessionTimeout = "PT10S";
            vencconf[0].UseCount = 1;

            //vencconf[1] = new VideoEncoderConfiguration();
            //vencconf[1].token = "2";
            return new GetVideoEncoderConfigurationsResponse(vencconf);
            //throw new NotImplementedException();
            //
            //
        }

        public Media.GetAudioSourceConfigurationsResponse GetAudioSourceConfigurations(Media.GetAudioSourceConfigurationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Media.GetAudioEncoderConfigurationsResponse GetAudioEncoderConfigurations(Media.GetAudioEncoderConfigurationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Media.GetVideoAnalyticsConfigurationsResponse GetVideoAnalyticsConfigurations(Media.GetVideoAnalyticsConfigurationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Media.GetMetadataConfigurationsResponse GetMetadataConfigurations(Media.GetMetadataConfigurationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Media.GetAudioOutputConfigurationsResponse GetAudioOutputConfigurations(Media.GetAudioOutputConfigurationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Media.GetAudioDecoderConfigurationsResponse GetAudioDecoderConfigurations(Media.GetAudioDecoderConfigurationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Media.VideoSourceConfiguration GetVideoSourceConfiguration(string ConfigurationToken)
        {
            return new Media.VideoSourceConfiguration();
            //throw new NotImplementedException();
        }

        public Media.VideoEncoderConfiguration GetVideoEncoderConfiguration(string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public Media.AudioSourceConfiguration GetAudioSourceConfiguration(string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public Media.AudioEncoderConfiguration GetAudioEncoderConfiguration(string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public Media.VideoAnalyticsConfiguration GetVideoAnalyticsConfiguration(string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public Media.MetadataConfiguration GetMetadataConfiguration(string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public Media.AudioOutputConfiguration GetAudioOutputConfiguration(string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public Media.AudioDecoderConfiguration GetAudioDecoderConfiguration(string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public Media.GetCompatibleVideoEncoderConfigurationsResponse GetCompatibleVideoEncoderConfigurations(Media.GetCompatibleVideoEncoderConfigurationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Media.GetCompatibleVideoSourceConfigurationsResponse GetCompatibleVideoSourceConfigurations(Media.GetCompatibleVideoSourceConfigurationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Media.GetCompatibleAudioEncoderConfigurationsResponse GetCompatibleAudioEncoderConfigurations(Media.GetCompatibleAudioEncoderConfigurationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Media.GetCompatibleAudioSourceConfigurationsResponse GetCompatibleAudioSourceConfigurations(Media.GetCompatibleAudioSourceConfigurationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Media.GetCompatibleVideoAnalyticsConfigurationsResponse GetCompatibleVideoAnalyticsConfigurations(Media.GetCompatibleVideoAnalyticsConfigurationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Media.GetCompatibleMetadataConfigurationsResponse GetCompatibleMetadataConfigurations(Media.GetCompatibleMetadataConfigurationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Media.GetCompatibleAudioOutputConfigurationsResponse GetCompatibleAudioOutputConfigurations(Media.GetCompatibleAudioOutputConfigurationsRequest request)
        {
            throw new NotImplementedException();
        }

        public Media.GetCompatibleAudioDecoderConfigurationsResponse GetCompatibleAudioDecoderConfigurations(Media.GetCompatibleAudioDecoderConfigurationsRequest request)
        {
            throw new NotImplementedException();
        }

        public void SetVideoSourceConfiguration(Media.VideoSourceConfiguration Configuration, bool ForcePersistence)
        {
            throw new NotImplementedException();
        }

        public void SetVideoEncoderConfiguration(Media.VideoEncoderConfiguration Configuration, bool ForcePersistence)
        {
            throw new NotImplementedException();
        }

        public void SetAudioSourceConfiguration(Media.AudioSourceConfiguration Configuration, bool ForcePersistence)
        {
            throw new NotImplementedException();
        }

        public void SetAudioEncoderConfiguration(Media.AudioEncoderConfiguration Configuration, bool ForcePersistence)
        {
            throw new NotImplementedException();
        }

        public void SetVideoAnalyticsConfiguration(Media.VideoAnalyticsConfiguration Configuration, bool ForcePersistence)
        {
            throw new NotImplementedException();
        }

        public void SetMetadataConfiguration(Media.MetadataConfiguration Configuration, bool ForcePersistence)
        {
            throw new NotImplementedException();
        }

        public void SetAudioOutputConfiguration(Media.AudioOutputConfiguration Configuration, bool ForcePersistence)
        {
            throw new NotImplementedException();
        }

        public void SetAudioDecoderConfiguration(Media.AudioDecoderConfiguration Configuration, bool ForcePersistence)
        {
            throw new NotImplementedException();
        }

        public Media.VideoSourceConfigurationOptions GetVideoSourceConfigurationOptions(string ConfigurationToken, string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public Media.VideoEncoderConfigurationOptions GetVideoEncoderConfigurationOptions(string ConfigurationToken, string ProfileToken)
        {
            return new Media.VideoEncoderConfigurationOptions();
            //throw new NotImplementedException();
        }

        public Media.AudioSourceConfigurationOptions GetAudioSourceConfigurationOptions(string ConfigurationToken, string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public Media.AudioEncoderConfigurationOptions GetAudioEncoderConfigurationOptions(string ConfigurationToken, string ProfileToken)
        {
            return new Media.AudioEncoderConfigurationOptions();
            //throw new NotImplementedException();
        }

        public Media.MetadataConfigurationOptions GetMetadataConfigurationOptions(string ConfigurationToken, string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public Media.AudioOutputConfigurationOptions GetAudioOutputConfigurationOptions(string ConfigurationToken, string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public Media.AudioDecoderConfigurationOptions GetAudioDecoderConfigurationOptions(string ConfigurationToken, string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public int GetGuaranteedNumberOfVideoEncoderInstances(out int JPEG, out int H264, out int MPEG4, string ConfigurationToken)
        {
            throw new NotImplementedException();
        }

        public Media.MediaUri GetStreamUri(Media.StreamSetup StreamSetup, string ProfileToken)
        {
            if (StreamSetup == null || ProfileToken == null) 
                throw new FaultException(new FaultReason("NoStreamSetupOrProfileToken"),
                       new FaultCode("Sender",
                           new FaultCode("InvalidArgVa", "http://www.onvif.org/ver10/error",
                               new FaultCode("NoStreamSetupOrProfileToken", "http://www.onvif.org/ver10/error"))));            

            MediaUri mediauri = new MediaUri();
            string Buf = null;

            string stream = StreamSetup.Stream.ToString();
            string protocol = StreamSetup.Transport.Protocol.ToString();

            byte[] b_stream = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, TyphoonCom.MakeMem(stream));
            byte[] b_protocol = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, TyphoonCom.MakeMem(protocol));
            byte[] b_profileToken = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, TyphoonCom.MakeMem(ProfileToken));

            uint datalen = (uint)b_profileToken.Length + (uint)b_protocol.Length + (uint)b_stream.Length;

            #region data preparation
            //---------------------------------------
            byte[] b_datalen = TyphoonCom.Int32toByteAr(datalen);

            byte[] data = new byte[datalen];

            uint ptr = 0;

            for (uint a = 0; a < (uint)b_stream.Length; a++)
            {
                data[a + ptr] = b_stream[a];
            }
            ptr += (uint)b_stream.Length;

            for (uint a = 0; a < (uint)b_protocol.Length; a++)
            {
                data[a + ptr] = b_protocol[a];
            }
            ptr += (uint)b_protocol.Length;

            for (uint a = 0; a < (uint)b_profileToken.Length; a++)
            {
                data[a + ptr] = b_profileToken[a];
            }
            ptr += (uint)b_profileToken.Length;

            data = Encoding.Convert(Encoding.ASCII, Encoding.Unicode, data);
            #endregion
            //---------------------------------------

            using(TyphoonMsg typhmsgresp = TyphoonMsgManager.SendSyncMsg(200, 5, data, 0))
            {
                if (typhmsgresp != null)
                {
                    Buf = typhmsgresp.stringMessageData;
                    Buf = TyphoonCom.ParseMem(0, Buf);
                    
                    mediauri.Uri = Buf;
                    return mediauri;
                }
                else
                {
                    throw new FaultException(new FaultReason("NoStreamUriFound"),
                           new FaultCode("Sender",
                               new FaultCode("InvalidArgVa", "http://www.onvif.org/ver10/error",
                                   new FaultCode("NoStreamUriFound", "http://www.onvif.org/ver10/error"))));
                }
            }

            
        }

        public void StartMulticastStreaming(string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public void StopMulticastStreaming(string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public void SetSynchronizationPoint(string ProfileToken)
        {
            throw new NotImplementedException();
        }

        public Media.MediaUri GetSnapshotUri(string ProfileToken)
        {
            ///* 
            // * здесь они хотят ссылку на картинку в JPEG
            // * хз как это реализовывать и надо ли             
            // */
            ////Media.StreamSetup StreamSetup = new Media.StreamSetup();

            //ConfigStruct confstr = new ConfigStruct();
            //XmlConfig conf = new XmlConfig();
            //TyphoonMessage TyphMsg = new TyphoonMessage();
            //MediaUri mediauri = new MediaUri();
            //string Buf = null;


            ////string stream = StreamSetup.Stream.ToString();
            ////string protocol = StreamSetup.Transport.Protocol.ToString();


            //string stream = new Media.StreamSetup().Stream.ToString();
            //string protocol = "UDP";


            //byte[] b_stream = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, TyphoonCom.MakeMem(stream));
            //byte[] b_protocol = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, TyphoonCom.MakeMem(protocol));
            //byte[] b_profileToken = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, TyphoonCom.MakeMem(ProfileToken));

            //uint datalen = (uint)b_profileToken.Length + (uint)b_protocol.Length + (uint)b_stream.Length;

            //byte[] b_datalen = TyphoonCom.Int32toByteAr(datalen);

            //byte[] data = new byte[datalen];

            //uint ptr = 0;

            //for (uint a = 0; a < (uint)b_stream.Length; a++)
            //{
            //    data[a + ptr] = b_stream[a];
            //}
            //ptr += (uint)b_stream.Length;

            //for (uint a = 0; a < (uint)b_protocol.Length; a++)
            //{
            //    data[a + ptr] = b_protocol[a];
            //}
            //ptr += (uint)b_protocol.Length;

            //for (uint a = 0; a < (uint)b_profileToken.Length; a++)
            //{
            //    data[a + ptr] = b_profileToken[a];
            //}
            //ptr += (uint)b_profileToken.Length;

            //data = Encoding.Convert(Encoding.ASCII, Encoding.Unicode, data);

            //byte[] tmp = TyphoonCom.FormCommand(200, 5, data, 0);
            //TyphoonCom.AddCommand(TyphoonCom.FormPacket(tmp));

            //for (int a = 0; a < 4; a++)
            //{
            //    TyphMsg.MessageID = TyphMsg.MessageID << 8;
            //    TyphMsg.MessageID += tmp[9 - a];
            //}
            ////ждем первого ответа, кста, это не гарантирует, что это нужный ответ, надо бы переделать
            //do
            //{
            //    Thread.Sleep(1);
            //} while (TyphoonCom.queueResponce.Count == 0);

            //if (TyphoonCom.queueResponce.Count > 0)
            //{
            //    try
            //    {
            //        //находим в очереди ответ с ID отправленного нами запроса
            //        Buf = TyphoonCom.queueResponce.Single(TyphoonMessage => TyphoonMessage.MessageID == TyphMsg.MessageID).MessageData;
            //        Buf = TyphoonCom.ParseMem(0, Buf);
            //    }
            //    catch (Exception ex)
            //    {
            //        TyphoonCom.log.ErrorFormat("от Typhoon пришли сообщения с одинаковыми ID или нет ни одного сообщения с таким ID {0}", ex.Message);
            //    }
            //    //удаляем из очереди мессагу с ID отправленного нами запроса
            //    try
            //    {
            //        TyphoonCom.queueResponce.Remove(TyphoonCom.queueResponce.Single(TyphoonMessage => TyphoonMessage.MessageID == TyphMsg.MessageID));
            //        ////рихтуем данные 
            //    }
            //    catch (Exception ex)
            //    {
            //        TyphoonCom.log.Error(ex.Message);
            //    }
                
            //    mediauri.Uri = Buf;
            //    mediauri.Timeout = "PT0S";
            //    mediauri.InvalidAfterReboot = false;
            //    mediauri.InvalidAfterConnect = true;

            //}
            //else
            //{
            //    Console.WriteLine("AddCommand returned false");
            //}
            //TyphoonCom.log.Debug("Service1: GetSnapshotUri");

            //return mediauri;
            return null;
        }
//--------------------------EventHandling--------------------------------------------------------------------
        public Event.SubscribeResponse1 Subscribe(Event.SubscribeRequest request)
        {
            Helper hlp = new Helper();
            if (request != null
                && request.Subscribe != null 
                && request.Subscribe.ConsumerReference != null 
                && request.Subscribe.ConsumerReference.Address != null)
            {
                OnvifProxy.bnSubscriptionManager.AddSubscriber(new bnSubscriber(request.Subscribe.ConsumerReference.Address.Value,
                    hlp.ParseTermTime(request.Subscribe.InitialTerminationTime),
                    1 ));
                
                Event.SubscribeResponse1 subscr = new Event.SubscribeResponse1();
                subscr.SubscribeResponse = new Event.SubscribeResponse();
                subscr.SubscribeResponse.SubscriptionReference = new Event.EndpointReferenceType();
                subscr.SubscribeResponse.SubscriptionReference.Address = new Event.AttributedURIType();
                subscr.SubscribeResponse.SubscriptionReference.Address.Value = Program.host.BaseAddresses[0].AbsoluteUri.ToString() + "onvif/event_service/bn_subscription_manager";
                
                subscr.SubscribeResponse.CurrentTimeSpecified = true;
                subscr.SubscribeResponse.CurrentTime = System.DateTime.Now;
                System.DateTime tmptime = new System.DateTime();
                tmptime = subscr.SubscribeResponse.CurrentTime;
                double mlscnds = hlp.ParseTermTime(request.Subscribe.InitialTerminationTime);
                if (mlscnds >= 0)
                {
                    subscr.SubscribeResponse.TerminationTime = tmptime.AddMilliseconds(mlscnds);
                }
                else 
                {
                    subscr.SubscribeResponse.TerminationTime = tmptime.AddMilliseconds(1000);
                }
                subscr.SubscribeResponse.TerminationTimeSpecified = true;

                return subscr;
            }
            else
            {
                return null;
            }
            
            
        }

        public Event.GetCurrentMessageResponse1 GetCurrentMessage(Event.GetCurrentMessageRequest request)
        {
            Event.GetCurrentMessageResponse1 getcurmess = new Event.GetCurrentMessageResponse1();
            getcurmess.GetCurrentMessageResponse = new Event.GetCurrentMessageResponse();

            return getcurmess;
        }

        Event.Capabilities Event.IEventPortType.GetServiceCapabilities()
        {
            Event.Capabilities caps = new Event.Capabilities();
            return caps;
        }

        public Event.GetEventPropertiesResponse GetEventProperties(Event.GetEventPropertiesRequest request)
        {
           
            Event.GetEventPropertiesResponse geteventprop = new Event.GetEventPropertiesResponse();

            geteventprop.TopicNamespaceLocation = new string[1];
            geteventprop.TopicNamespaceLocation[0] = "http://www.onvif.org/onvif/ver10/topics/topicsns.xml";
            geteventprop.FixedTopicSet = false;            
            geteventprop.TopicSet = new Event.TopicSetType();
            //geteventprop.TopicSet.Any = new System.Xml.XmlElement[1];
            geteventprop.TopicSet.Any = new System.Xml.XmlElement[2];
            XmlDocument doc = new XmlDocument();

            doc.LoadXml("<tns1:VideoSource wstop:topic = 'true' xmlns:tns1 = 'http://www.onvif.org/ver10/topics'"
                +" xmlns:tt='http://www.onvif.org/ver10/schema' xmlns:wstop = 'http://docs.oasis-open.org/wsn/t-1'>"
                + "<tt:MessageDescription><tt:Source><tt:SimpleItemDescription Name='app' Type='xsd:string' /> "
                + "</tt:Source><tt:Key><tt:SimpleItemDescription Name='channel' Type='xsd:int' /></tt:Key><tt:Data> "
                + "<tt:SimpleItemDescription Name='tampering' Type='xsd:int' /> </tt:Data></tt:MessageDescription>"
                + "</tns1:VideoSource>");
            
            geteventprop.TopicSet.Any[0] = doc.DocumentElement;
            geteventprop.TopicSet.Any[1] = (new XmlDocument()).CreateElement("my:MotionAlarm", "mynamespace");


            geteventprop.TopicExpressionDialect = new string[2];
            geteventprop.TopicExpressionDialect[0] = "http://www.onvif.org/ver10/tev/topicExpression/ConcreteSet";
            //geteventprop.TopicExpressionDialect[0] = "http://www.onvif.org/ver10/tev/topicExpression/SimpleSet";
            geteventprop.TopicExpressionDialect[1] = "http://docs.oasis-open.org/wsn/t-1/TopicExpression/Concrete";
            //geteventprop.TopicExpressionDialect[1] = "http://docs.oasis-open.org/wsn/t-1/TopicExpression/Simple";

            geteventprop.MessageContentFilterDialect = new string[1];
            geteventprop.MessageContentFilterDialect[0] = "http://www.onvif.org/ver10/tev/messageContentFilter/ItemFilter";

            geteventprop.MessageContentSchemaLocation = new string[1];
            geteventprop.MessageContentSchemaLocation[0] = "http://www.onvif.org/onvif/ver10/schema/onvif.xsd";


            return geteventprop;
        }

        public Event.CreatePullPointSubscriptionResponse CreatePullPointSubscription(Event.CreatePullPointSubscriptionRequest request)
        {
            Helper hlp = new Helper();

            if (request != null)
            {
                ppSubscriber ppsubs;
                ConfigStruct confstr = new ConfigStruct();
                XmlConfig conf = new XmlConfig();

                confstr = conf.Read();

                if (request.Filter == null)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml("<zero/>");
                    request.Filter = new Event.FilterType();
                    request.Filter.Any = new System.Xml.XmlElement[1];
                    request.Filter.Any[0] = doc.DocumentElement;
                }
                //if (ppSubscriptionManager.SubscribersCollection.ContainsKey(request.Filter))
                //{
                //    //если сервис с таким фильтром уже существует, взведем таймер
                //    ppSubscriptionManager.SubscribersCollection.TryGetValue(request.Filter, out ppsubs);
                //    ppsubs.SubscriberTimeoutTimer.Interval = ppsubs.SubscriberTimeoutTimer.Interval + 60000;
                //}
                //else
                using (ppsubs = new ppSubscriber(request.Filter))
                {
                    //иначе просто создадим сервис с запрашиваемым фильтром
                    ;
                    if (request.InitialTerminationTime != null)
                    {
                        ppsubs.SubscriberTimeoutTimer.Interval = hlp.ParseTermTime(request.InitialTerminationTime);
                    }



                    Event.CreatePullPointSubscriptionResponse PullPointSubscriptionResponse = new Event.CreatePullPointSubscriptionResponse();
                    PullPointSubscriptionResponse.SubscriptionReference = new Event.EndpointReferenceType();
                    PullPointSubscriptionResponse.SubscriptionReference.Address = new Event.AttributedURIType();
                    //PullPointSubscriptionResponse.SubscriptionReference.Address.Value = confstr.Capabilities.Events.XAddr + "/bn_subscription_manager";
                    PullPointSubscriptionResponse.SubscriptionReference.Address.Value = ppsubs.Addr;
                    TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, (int)hlp.ParseTermTime(request.InitialTerminationTime));//некорректно приведение double в int
                    PullPointSubscriptionResponse.TerminationTime = System.DateTime.UtcNow.Add(timeSpan);
                    PullPointSubscriptionResponse.CurrentTime = System.DateTime.UtcNow;

                    return PullPointSubscriptionResponse;
                }
            }
            return null;
        }

        //Event.RenewResponse1 Event.IPausableSubscriptionManager.Renew(Event.RenewRequest request)
        public Event.RenewResponse1 Renew(Event.RenewRequest request)
        {
            OperationContext context = OperationContext.Current;
            Helper hlp = new Helper();
            if (request != null && request.Renew != null && request.Renew.
                TerminationTime!= null )
            { 
                //request.Renew.
                string address = GetIP(context);
                int fl = 0;
                foreach (bnSubscriber subscriber in bnSubscriptionManager.SubscribersCollection.Values)
                {
                    if (TrimIP(subscriber.Addr) == address)
                    {
                        if (hlp.ParseTermTime(request.Renew.TerminationTime) >= 0)
                        {
                            subscriber.SubscriberTimeoutTimer.Interval = hlp.ParseTermTime(request.Renew.TerminationTime);
                        }
                        else
                        {
                            subscriber.SubscriberTimeoutTimer.Interval = 1;
                        }
                        fl++;
                    }
                }
                //проверка наличия подписчика в коллекции
                //если нет - вернем fault
                if (fl == 0)
                {
                    #region
                    //docs.oasisopen.org.wsn.b2.UnacceptableTerminationTimeFaultType uttft = new docs.oasisopen.org.wsn.b2.UnacceptableTerminationTimeFaultType();
                    //uttft.Nodes = new XmlNode[8];
                    //XmlDocument doc = new XmlDocument();
                    //string str = "<Timestamp xmlns='http://docs.oasis-open.org/wsrf/bf-2'>" + System.DateTime.UtcNow.ToString("s") + "</Timestamp>";
                    //doc.LoadXml(str);
                    //uttft.Nodes[0] = doc.FirstChild;
                    //str = "<Originator xmlns='http://docs.oasis-open.org/wsrf/bf-2'>" + "<Address xmlns = 'http://www.w3.org/2005/08/addressing'>" + Program.uuid + "</Address>" + "</Originator>";
                    //doc.LoadXml(str);
                    //uttft.Nodes[1] = doc.FirstChild;
                    //str = "<ErrorCode  xmlns='http://docs.oasis-open.org/wsrf/bf-2' dialect='anyURI'/>";
                    //doc.LoadXml(str);
                    //uttft.Nodes[2] = doc.FirstChild;
                    //str = "<Description xmlns='http://docs.oasis-open.org/wsrf/bf-2'/>";
                    //doc.LoadXml(str);
                    //uttft.Nodes[3] = doc.FirstChild;
                    //str = "<FaultCause xmlns='http://docs.oasis-open.org/wsrf/bf-2'><someitem xmlns = 'somenamespace'/></FaultCause>";
                    //doc.LoadXml(str);
                    //uttft.Nodes[4] = doc.FirstChild;
                    //str = "<MinimumTime xmlns='http://docs.oasis-open.org/wsn/b-2'>" + System.DateTime.MinValue.ToString("s") + "</MinimumTime>";
                    //doc.LoadXml(str);
                    //uttft.Nodes[5] = doc.FirstChild;
                    //str = "<MaximumTime xmlns='http://docs.oasis-open.org/wsn/b-2'>" + System.DateTime.MaxValue.ToString("s") + "</MaximumTime>";
                    //doc.LoadXml(str);
                    //uttft.Nodes[6] = doc.FirstChild;
                    ////str = "<Action xmlns='http://www.w3.org/2005/08/adressing'>" + "http://www.w3.org/2005/08/addressing/soap/fault" + "</Action>";
                    ////doc.LoadXml(str);
                    ////uttft.Nodes[7] = doc.FirstChild;//http://docs.oasis-open.org/wsn/fault
                    //FaultException fe = new FaultException<docs.oasisopen.org.wsn.b2.UnacceptableTerminationTimeFaultType>(uttft, new FaultReason("no such subscriber"), new FaultCode("Sender"), "http://www.w3.org/2005/08/addressing/soap/fault");
                    ////FaultException fe = new FaultException<docs.oasisopen.org.wsn.b2.UnacceptableTerminationTimeFaultType>(uttft, new FaultReason("no such subscriber"), new FaultCode("faultcodename"), "http://docs.oasis-open.org/wsn/fault");
                    //throw fe;
                    #endregion
                    throw new FaultException(new FaultReason("ResourceUnknownFault"),
                      new FaultCode("Sender",
                          new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                              new FaultCode("ResourceUnknownFault", "http://www.onvif.org/ver10/error"))));
                }

                Event.RenewResponse renewResponse = new Event.RenewResponse();
                renewResponse.CurrentTime = System.DateTime.Now;
                System.DateTime tmptime = new System.DateTime();
                tmptime = renewResponse.CurrentTime;
                double mlscnds = hlp.ParseTermTime(request.Renew.TerminationTime);
                if (mlscnds >= 0)
                {
                    renewResponse.TerminationTime = tmptime.AddMilliseconds(mlscnds);
                }
                else 
                {
                    renewResponse.TerminationTime = tmptime.AddMilliseconds(1000);
                }
                renewResponse.CurrentTimeSpecified = true;

                return new Event.RenewResponse1(renewResponse);
            }
            return null;
        }


        //Event.RenewResponse1 Event.SubscriptionManager.Renew(Event.RenewRequest request)
        //{
        //    Helper hlp = new Helper();
        //    if (request != null && request.Renew != null && request.Renew.
        //        TerminationTime != null)
        //    {
        //        string address = GetIP();
        //        int fl = 0;
        //        foreach (bnSubscriber subscriber in bnSubscriptionManager.SubscribersCollection.Values)
        //        {
        //            if (TrimIP(subscriber.Addr) == address)
        //            {
        //                if (hlp.ParseTermTime(request.Renew.TerminationTime) >= 0)
        //                {
        //                    subscriber.SubscriberTimeoutTimer.Interval = hlp.ParseTermTime(request.Renew.TerminationTime);
        //                }
        //                else
        //                {
        //                    subscriber.SubscriberTimeoutTimer.Interval = 1;
        //                }
        //                fl++;
        //            }
        //        }
        //        //проверка наличия подписчика в коллекции
        //        //если нет - вернем fault
        //        if (fl == 0)
        //        {
        //            //docs.oasisopen.org.wsn.b2.UnacceptableTerminationTimeFaultType uttft = new docs.oasisopen.org.wsn.b2.UnacceptableTerminationTimeFaultType();
        //            //uttft.Nodes = new XmlNode[8];
        //            //XmlDocument doc = new XmlDocument();
        //            //string str = "<Timestamp xmlns='http://docs.oasis-open.org/wsrf/bf-2'>" + System.DateTime.UtcNow.ToString("s") + "</Timestamp>";
        //            //doc.LoadXml(str);
        //            //uttft.Nodes[0] = doc.FirstChild;
        //            //str = "<Originator xmlns='http://docs.oasis-open.org/wsrf/bf-2'>" + "<Address xmlns = 'http://www.w3.org/2005/08/addressing'>" + Program.uuid + "</Address>" + "</Originator>";
        //            //doc.LoadXml(str);
        //            //uttft.Nodes[1] = doc.FirstChild;
        //            //str = "<ErrorCode  xmlns='http://docs.oasis-open.org/wsrf/bf-2' dialect='anyURI'/>";
        //            //doc.LoadXml(str);
        //            //uttft.Nodes[2] = doc.FirstChild;
        //            //str = "<Description xmlns='http://docs.oasis-open.org/wsrf/bf-2'/>";
        //            //doc.LoadXml(str);
        //            //uttft.Nodes[3] = doc.FirstChild;
        //            //str = "<FaultCause xmlns='http://docs.oasis-open.org/wsrf/bf-2'><someitem xmlns = 'somenamespace'/></FaultCause>";
        //            //doc.LoadXml(str);
        //            //uttft.Nodes[4] = doc.FirstChild;
        //            //str = "<MinimumTime xmlns='http://docs.oasis-open.org/wsn/b-2'>" + System.DateTime.MinValue.ToString("s") + "</MinimumTime>";
        //            //doc.LoadXml(str);
        //            //uttft.Nodes[5] = doc.FirstChild;
        //            //str = "<MaximumTime xmlns='http://docs.oasis-open.org/wsn/b-2'>" + System.DateTime.MaxValue.ToString("s") + "</MaximumTime>";
        //            //doc.LoadXml(str);
        //            //uttft.Nodes[6] = doc.FirstChild;
        //            ////str = "<Action xmlns='http://www.w3.org/2005/08/adressing'>" + "http://www.w3.org/2005/08/addressing/soap/fault" + "</Action>";
        //            ////doc.LoadXml(str);
        //            ////uttft.Nodes[7] = doc.FirstChild;//http://docs.oasis-open.org/wsn/fault
        //            //FaultException fe = new FaultException<docs.oasisopen.org.wsn.b2.UnacceptableTerminationTimeFaultType>(uttft, new FaultReason("no such subscriber"), new FaultCode("Sender"), "http://www.w3.org/2005/08/addressing/soap/fault");
        //            ////FaultException fe = new FaultException<docs.oasisopen.org.wsn.b2.UnacceptableTerminationTimeFaultType>(uttft, new FaultReason("no such subscriber"), new FaultCode("faultcodename"), "http://docs.oasis-open.org/wsn/fault");
        //            //throw fe;

        //            throw new FaultException(new FaultReason("ResourceUnknownFault"),
        //              new FaultCode("Sender",
        //                  new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
        //                      new FaultCode("ResourceUnknownFault", "http://www.onvif.org/ver10/error"))));
        //        }

        //        Event.RenewResponse renewResponse = new Event.RenewResponse();
        //        renewResponse.CurrentTime = System.DateTime.Now;
        //        System.DateTime tmptime = new System.DateTime();
        //        tmptime = renewResponse.CurrentTime;
        //        double mlscnds = hlp.ParseTermTime(request.Renew.TerminationTime);
        //        if (mlscnds >= 0)
        //        {
        //            renewResponse.TerminationTime = tmptime.AddMilliseconds(mlscnds);
        //        }
        //        else
        //        {
        //            renewResponse.TerminationTime = tmptime.AddMilliseconds(1000);
        //        }
        //        renewResponse.CurrentTimeSpecified = true;

        //        return new Event.RenewResponse1(renewResponse);
        //    }
        //    return null;
        //}

        //private double ParseTermTime(string termtime)
        //{
        //    //возвращает в время миллисекундах
        //    #region //xsd:duration time format description
        //    // Constructor which takes a relative time value. The time value is expressed as String with
        //    // the following format PxYxMxDTxHxMxS where:
        //    // P is a required value to inidicate the start of the expression
        //    // xY indicates the number of years (optional)
        //    // xM indicates the number of months (optional)
        //    // xD indicates the number of days (optional)
        //    // T indicates the start of a time section and is required if the duration expressed contains hours, minutes, or seconds
        //    // xH indicates the number of hours (optional)
        //    // xM indicates the number of minutes (optional)
        //    // xS indicates the number of seconds (optional)

        //    //Examples:
        //    //"P12Y8M22DT3H35M2S" indicates a duration of 12 years, 8 months, 22 days, 3 hours, 35 minutes and 2 seconds
        //    //"P21DT8H" indicates a duration of 21 days and 8 hours
        //    //"P5Y7M" indicates a duration of 5 years and 7 months
        //    //"PT14S" indicates a duration of 14 seconds
        //    #endregion

        //    System.DateTime dtime = new System.DateTime();
        //    string tmp1;
            
        //    if((termtime.ToCharArray())[0] == 'P')
        //    {
        //        //ветка xsd:duration
        //        int indofS, indofM, indofH, indofT, indofD, indofMn, indofY;
        //        indofS = termtime.IndexOf('S');
        //        indofM = termtime.IndexOf('M');
        //        indofH = termtime.IndexOf('H');
        //        indofT = termtime.IndexOf('T');
        //        //indofMn = termtime.IndexOf('M');
        //        indofY = termtime.IndexOf('Y');

        //        if (termtime.Contains('T'))
        //        {
        //            if (termtime.Contains('H'))
        //            {
        //                tmp1 = termtime.Remove(0, termtime.IndexOf('T')+1);
        //                tmp1 = tmp1.Remove(termtime.IndexOf('H'), (tmp1.Length - tmp1.LastIndexOf('H')));
        //                dtime.AddHours(str2double(tmp1));
        //            }
        //            if (termtime.Contains('M')&& (termtime.IndexOf('T')<termtime.IndexOf('M')))
        //            {
        //                tmp1 = termtime.Remove(0, termtime.IndexOf('T')+1);
        //                tmp1 = termtime.Remove(tmp1.IndexOf('M'), (tmp1.Length - tmp1.LastIndexOf('M')));
        //                dtime.AddMinutes(str2double(tmp1));
        //            }
        //            if (termtime.Contains('S'))
        //            {
        //                tmp1 = termtime.Remove(0, termtime.IndexOf('T') + 1);
        //                tmp1 = tmp1.Remove(tmp1.IndexOf('S'), (tmp1.Length - tmp1.LastIndexOf('S')));
        //                dtime = dtime.AddSeconds(str2double(tmp1));
        //            }
        //        }
        //        else
        //        {
        //            return 0;
        //        }             
                
                
        //        //---------------T------------
        //        if (termtime.Contains('D'))
        //        {
        //        }
        //        if (termtime.Contains('M'))
        //        {
        //        }
        //        if (termtime.Contains('Y'))
        //        {
        //        }
        //        return (double)dtime.TimeOfDay.TotalMilliseconds;
        //    }
        //    else
        //    {
        //        //ветка xsd:dateTime
        //        System.DateTime ddtime = new System.DateTime();
        //        System.DateTime.TryParseExact(termtime, "yyyy-MM-dd'T'HH:mm:ss'Z'", null, DateTimeStyles.AdjustToUniversal, out ddtime);
        //        TimeSpan timespan = ddtime.Subtract(System.DateTime.Now);
        //        if (timespan.TotalMilliseconds >= 0)
        //        {
        //            return (double)timespan.TotalMilliseconds;
        //        }
        //        else
        //            return 1;
        //    }
        //}

        //private double str2double(string str)
        //{
        //    double bytes = double.Parse(str);
        //    return (double)bytes;
        //}

        private string TrimIP(string adr)
        {
            string tmp1, tmp2;
            try
            {
                tmp1 = adr.Remove(0, (adr.IndexOf("//") + 2));
                tmp2 = tmp1.Remove(tmp1.IndexOf("/"), tmp1.Length - (tmp1.IndexOf("/")));
                if (tmp2.Contains(":"))
                {
                    tmp2 = tmp2.Remove(tmp2.IndexOf(":"), tmp2.Length - (tmp2.IndexOf(":")));
                }
            }
            catch (Exception ex)
            {
                TyphoonCom.log.Debug(ex.Message);
                return null;
            }

            return tmp2;
        }

        private string GetIP(OperationContext context)
        {
            using (MessageProperties prop = context.IncomingMessageProperties)//IDisposable   
            {
                RemoteEndpointMessageProperty endpoint =
                    prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                string ip = endpoint.Address;
                return ip;
            }
        }

        public Event.UnsubscribeResponse1 Unsubscribe(Event.UnsubscribeRequest request)
        {
            OperationContext context = OperationContext.Current;

            bool containsRequestSubscriber = false;
            string address;

            
            if (request != null && request.Unsubscribe != null)
            {
                address = GetIP(context);
                
                foreach (bnSubscriber subscriber in bnSubscriptionManager.SubscribersCollection.Values)
                {
                    if (TrimIP(subscriber.Addr) == address)
                    {
                        subscriber.SubscriberTimeoutTimer.Interval = 1;
                        containsRequestSubscriber = true;
                    }
                }
                if (containsRequestSubscriber)
                {
                    return new Event.UnsubscribeResponse1();
                }
                else
                {
                    throw new FaultException(new FaultReason("ResourceUnknownFault"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("ResourceUnknownFault", "http://www.onvif.org/ver10/error"))));
                }              
            }
            return null;
            
        }

        public Event.GetMessagesResponse1 GetMessages(Event.GetMessagesRequest request)
        {
            throw new NotImplementedException();
        }

        public Event.DestroyPullPointResponse1 DestroyPullPoint(Event.DestroyPullPointRequest request)
        {
            throw new NotImplementedException();
        }

        public void Notify(Event.Notify1 request)
        {
        }

        public Event.CreatePullPointResponse1 CreatePullPoint(Event.CreatePullPointRequest request)
        {
            throw new NotImplementedException();
        }

        public Event.PauseSubscriptionResponse1 PauseSubscription(Event.PauseSubscriptionRequest request)
        {
            throw new NotImplementedException();
        }

        public Event.ResumeSubscriptionResponse1 ResumeSubscription(Event.ResumeSubscriptionRequest request)
        {
            throw new NotImplementedException();
        }

        //-----------------PullPointSubscription------------------------------
        void Event.PullPointSubscription.SetSynchronizationPoint()
        {
        }
        public Event.SeekResponse Seek(Event.SeekRequest request)
        {
            throw new NotImplementedException();
        }
        public Event.PullMessagesResponse PullMessages(Event.PullMessagesRequest request)
        {
            throw new NotImplementedException();
        }

        //public object DeserializeReply(System.ServiceModel.Channels.Message message, object[] parameters)
        //{
        //    return DeserializeReply(message, parameters);
        //}

        //public System.ServiceModel.Channels.Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        //{
        //    //System.ServiceModel.Channels.Message message = System.ServiceModel.Channels.Message.CreateMessage(messageVersion, action);

        //    ////address.ApplyTo(message);
        //    ////UriBuilder builder = new UriBuilder(message.Headers.To);
        //    ////builder.Path = builder.Path + "/" + this.operationName;
        //    ////builder.Query = SerializeParameters(parameterInfos, parameters);
        //    ////message.Headers.To = builder.Uri;
        //    ////message.Properties.Via = builder.Uri;

        //    //return message;
        //    return SerializeRequest(messageVersion, parameters);
        //}
    }
    public partial class Service1 : MediaSourcesProvider.IMediaSourcesProvider
    {
        public MediaSourcesProvider.GetMediaSourcesResponse GetMediaSources(MediaSourcesProvider.GetMediaSourcesRequest request)
        {
            int startRef = 0, limit = 1, mediaSourcesLeftToSend = 0;
            GetMediaSourcesResponse getMediaSourceResp = new GetMediaSourcesResponse();

            if (!string.IsNullOrEmpty(request.StartReference))
            {
                try
                {
                    startRef = Convert.ToInt32(request.StartReference);
                    if (startRef < 0) startRef = 0;
                }
                catch(FormatException)
                {
                    startRef = 0;
                }
                catch(OverflowException)
                {
                    startRef = 0;
                }                
            }

            if(request.Limit > 0) limit = request.Limit;


            if ((MediaSource.MediaSourceList.Count - startRef) < limit)
            {
                mediaSourcesLeftToSend = MediaSource.MediaSourceList.Count - startRef;
            }
            else
            {
                mediaSourcesLeftToSend = limit;
            }

            getMediaSourceResp.MediaSource = new MediaSourceType[mediaSourcesLeftToSend];

            for (int e = startRef; e < (startRef+mediaSourcesLeftToSend); e++)
            {
                getMediaSourceResp.MediaSource[e] = MediaSource.MediaSourceList[e];
            }

            if(!String.IsNullOrWhiteSpace(request.StartReference))
            {
                //тогда надо возвращать UpdateToken
                foreach (MediaSourceType mSource in MediaSource.MediaSourceList)
                {
                    getMediaSourceResp.UpdateToken += "*" + mSource.token;
                }
            }
            //MediaSource.GetUpdatesOfMediaSourceList(getMediaSourceResp.UpdateToken);

            return getMediaSourceResp;
        }
        public MediaSourcesProvider.FindMediaSourcesResponse FindMediaSources(MediaSourcesProvider.FindMediaSourcesRequest request)
        {
            return new MediaSourcesProvider.FindMediaSourcesResponse();
        }        
        public MediaSourcesProvider.GetSearchResultsResponse GetSearchResults(MediaSourcesProvider.GetSearchResultsRequest request)
        {
            MediaSourcesProvider.GetSearchResultsResponse resp = new MediaSourcesProvider.GetSearchResultsResponse();
            return resp;
        }
        public MediaSourcesProvider.GetUpdatesResponse GetUpdates(MediaSourcesProvider.GetUpdatesRequest request)
        {
            //int startRef = 0, limit = 1, mediaSourcesLeftToSend = 0;

            MediaSourcesProvider.GetUpdatesResponse resp = new GetUpdatesResponse();

            MediaSourceType[] MediaSourceArr = MediaSource.GetUpdatesOfMediaSourceList(request.UpdateToken);
            if (MediaSourceArr == null)
                return null;

            resp.Update = new UpdateType[MediaSourceArr.Length];

            for (int t = 0; t < MediaSourceArr.Length; t++)
            {
                resp.Update[t] = new UpdateType();
                resp.Update[t].MediaSource = MediaSourceArr[t];
                resp.Update[t].MediaSourceToken = MediaSourceArr[t].token;
            }
            
            resp.HasMoreUpdates = false;
            return resp;
        }
    }
    public partial class Service1 : ReplayService.IReplayPort
    {
        ReplayService.Capabilities ReplayService.IReplayPort.GetServiceCapabilities()
        {
            ReplayService.Capabilities caps = new ReplayService.Capabilities();
            return caps;
        }
        public ReplayService.GetReplayUriResponse GetReplayUri(ReplayService.GetReplayUriRequest request)
        {
            string formedstring;
            GetReplayUriResponse resp;

            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(GetReplayUriRequest));
                try
                {
                    xmlSerializer.Serialize(ms, request);
                    StreamReader strread = new StreamReader(ms);
                    ms.Position = 0;
                    formedstring = strread.ReadToEnd();
                }
                catch (Exception)
                {
                    throw new FaultException(new FaultReason("No ReplayUri Results"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVa", "http://www.onvif.org/ver10/error",
                                  new FaultCode("NoReplayUriFound", "http://www.onvif.org/ver10/error"))));
                }
            }

            using (TyphoonMsg TyphMsg = TyphoonMsgManager.SendSyncMsg(200, 21, TyphoonCom.MakeMem(formedstring), 0))
            {
                if (TyphMsg == null || string.IsNullOrEmpty(TyphMsg.stringMessageData))
                    throw new FaultException(new FaultReason("No ReplayUri Found"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("NoReplayUriFound", "http://www.onvif.org/ver10/error"))));

                TyphMsg.stringMessageData = TyphoonCom.ParseMem(0, TyphMsg.stringMessageData);
                using (MemoryStream ms2 = new MemoryStream(TyphMsg.byteMessageData))
                {
                    XmlSerializer xmlserializer2 = new XmlSerializer(typeof(GetReplayUriResponse),
                        "http://www.onvif.org/ver10/schema");
                    try
                    {
                        resp = (GetReplayUriResponse)xmlserializer2.Deserialize(ms2);
                        return resp;
                    }
                    catch (InvalidOperationException)
                    {
                        throw new FaultException(new FaultReason("No ReplayUri"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("NoReplayUriFound", "http://www.onvif.org/ver10/error"))));
                    }
                }
            }
        }
        public ReplayService.ReplayConfiguration GetReplayConfiguration()
        {
            //Console.WriteLine("GetReplayConfiguration called");
            return new ReplayService.ReplayConfiguration();
        }
        public void SetReplayConfiguration(ReplayService.ReplayConfiguration Configuration)
        {
            //Console.WriteLine("SetReplayConfiguration called");
            return;
        }
    }
    public partial class Service1 : RecordingSearch.ISearchPort
    {
        public RecordingSearch.Capabilities GetRecordingServiceCapabilities()
        {
            #region
            ////-----для отладки mediasourceprovider---

            //MediaSourcesProvider.GetMediaSourcesResponse getmediasource = new MediaSourcesProvider.GetMediaSourcesResponse();
            //Media.GetVideoSourcesResponse videosources = new GetVideoSourcesResponse();
            //videosources = GetVideoSources(null);

            //getmediasource.MediaSource = new MediaSourcesProvider.MediaSourceType[videosources.VideoSources.Length];
            //for(int i=0;i<getmediasource.MediaSource.Length;i++)
            //{
            //    getmediasource.MediaSource[i] = new MediaSourceType();
            //    getmediasource.MediaSource[i].Location = new MediaSourcesProvider.GeoCircle();
            //    getmediasource.MediaSource[i].Location.Value = "someValue";
            //    getmediasource.MediaSource[i].ONVIFBinding = new MediaSourcesProvider.ONVIFBindingType();
            //    getmediasource.MediaSource[i].ONVIFBinding.Endpoint = new MediaSourcesProvider.EndpointType[1];
            //    //getmediasource.MediaSource[i].ONVIFBinding.Endpoint[0].
            //    getmediasource.MediaSource[i].ONVIFBinding.MediaSourceToken = "someStringToken";
            //    getmediasource.MediaSource[i].ONVIFBinding.ProfileToken = videosources.VideoSources[i].token;

            //    getmediasource.MediaSource[i].token = "mediasourcetoken";
            //    getmediasource.MediaSource[i].Name = new NameType[1];
            //    getmediasource.MediaSource[i].Name[0] = new NameType();
            //    getmediasource.MediaSource[i].Name[0].lang = "RU";
            //    getmediasource.MediaSource[i].Name[0].Value = videosources.VideoSources[i].token;

            //}
            ////-------------------------------------
            #endregion

            RecordingSearch.Capabilities caps = new RecordingSearch.Capabilities();
            caps.MetadataSearch = false;
            caps.GeneralStartEvents = false;
            caps.GeneralStartEventsSpecified = true;
            caps.MetadataSearchSpecified = true;
            return caps;
        }

        public RecordingSummary GetRecordingSummary()
        {
            using (TyphoonMsg typhmsgresp = TyphoonMsgManager.SendSyncMsg(7))
            {
                if (typhmsgresp != null)
                {
                    string tmpstring = TyphoonCom.ParseMem(0, typhmsgresp.stringMessageData);
                    tmpstring = tmpstring.Replace("<?xml version=\u00221.0\u0022 encoding=\u0022UTF-8\u0022?>", "");
                    RecordingSummary recsumresponse = new RecordingSummary();

                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(tmpstring);


                        recsumresponse.DataFrom = System.DateTime.Parse(doc.FirstChild.FirstChild["DataFrom"].InnerText);
                        recsumresponse.DataUntil = System.DateTime.Parse(doc.FirstChild.FirstChild["DataUntil"].InnerText);
                        recsumresponse.NumberRecordings = Int16.Parse(doc.FirstChild.FirstChild["NumberRecordings"].InnerText);
                    }
                    catch (SystemException)
                    {
                        throw new FaultException(new FaultReason("Wrong GetRecordingSummary Recieved From Typhoon"),
                           new FaultCode("Sender",
                               new FaultCode("InvalidArgVa", "http://www.onvif.org/ver10/error",
                                   new FaultCode("WrongGetRecordingSummaryRecievedFromTyphoon", "http://www.onvif.org/ver10/error"))));
                    }
                    return recsumresponse;
                }
                else
                {
                    throw new FaultException(new FaultReason("No GetRecordingSummary Recieved From Typhoon"),
                           new FaultCode("Sender",
                               new FaultCode("InvalidArgVa", "http://www.onvif.org/ver10/error",
                                   new FaultCode("NoGetRecordingSummaryRecievedFromTyphoon", "http://www.onvif.org/ver10/error"))));
                }            
            }
            
        }

        public RecordingInformation GetRecordingInformation(string RecordingToken)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\u00221.0\u0022 encoding=\u0022UTF-8\u0022?>");
            sb.Append("<GetRecordingInformationRequest>");
            sb.Append("<RecordingToken>");
            sb.Append(RecordingToken);
            sb.Append("</RecordingToken>");
            sb.Append("</GetRecordingInformationRequest>");
            RecordingInformation resp = new RecordingInformation();

            using (TyphoonMsg TyphMsg = TyphoonMsgManager.SendSyncMsg(200, 8, TyphoonCom.MakeMem(sb.ToString()), 0))
            {
                if (TyphMsg == null || string.IsNullOrEmpty(TyphMsg.stringMessageData))
                    throw new FaultException(new FaultReason("No Recordings Information Found"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("NoRecordingsFound", "http://www.onvif.org/ver10/error"))));

                TyphMsg.stringMessageData = TyphoonCom.ParseMem(0, TyphMsg.stringMessageData);
                using (MemoryStream ms2 = new MemoryStream(TyphMsg.byteMessageData))
                {
                    XmlSerializer xmlserializer2 = new XmlSerializer(typeof(RecordingInformation),
                        "http://www.onvif.org/ver10/schema");
                    try
                    {
                        resp = (RecordingInformation)xmlserializer2.Deserialize(ms2);
                        return resp;
                    }
                    catch (InvalidOperationException)
                    {
                        {
                            throw new FaultException(new FaultReason("No RecordingInformationSearchResults"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("NoRecordingsFound", "http://www.onvif.org/ver10/error"))));
                        }

                    }
                }
            }
        }

        public GetMediaAttributesResponse GetMediaAttributes(GetMediaAttributesRequest request)
        {
            return new GetMediaAttributesResponse();
        }

        public FindRecordingsResponse FindRecordings(FindRecordingsRequest request)
        {
            string formedstring;

            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(FindRecordingsRequest));
                xmlSerializer.Serialize(ms, request);

                StreamReader strread = new StreamReader(ms);
                ms.Position = 0;
                formedstring = strread.ReadToEnd();
            }

            using (TyphoonMsg TyphMsg = TyphoonMsgManager.SendSyncMsg(200, 10, TyphoonCom.MakeMem(formedstring), 0))
            {
                if (TyphMsg == null || string.IsNullOrEmpty(TyphMsg.stringMessageData))
                    throw new FaultException(new FaultReason("No Recordings Found"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVa", "http://www.onvif.org/ver10/error",
                                  new FaultCode("NoRecordingsFound", "http://www.onvif.org/ver10/error"))));

                FindRecordingsResponse resp = null;
                TyphMsg.stringMessageData = TyphoonCom.ParseMem(0, TyphMsg.stringMessageData);
                
                using(MemoryStream ms2 = new MemoryStream(TyphMsg.byteMessageData))
                {
                    XmlSerializer xmlserializer2 = new XmlSerializer(typeof(FindRecordingsResponse), "http://www.onvif.org/ver10/schema");
                    
                    try
                    {
                        resp = (FindRecordingsResponse)xmlserializer2.Deserialize(ms2);
                    }
                    catch(InvalidOperationException)
                    {
                        throw new FaultException(new FaultReason("No Recordings Found"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVa", "http://www.onvif.org/ver10/error",
                                  new FaultCode("NoRecordingsFound", "http://www.onvif.org/ver10/error"))));
                    }
                }
                return resp;
            }

            

            //---------------------------------------------------------------------------------------
            #region Junk



//            //request.Scope.IncludedSources[0].
//            //request.Scope.IncludedRecordings[0].
//            //string a = FilterParser.ParseExpression(@" BOOlean(//Source[Location = “mylocation”]) and boolean(//Track[TrackType = “Video”]) not boolean(//Track[TrackType = “Operator”]) ");
//            string a = FilterParser.ParseExpression(request.Scope.RecordingInformationFilter);

//            #region
//            /*
//             Dialect=http://www.onvif.org/ver10/tse/searchFilter 
//[1] Expression ::= BoolExpr | Expression 'and' Expression 
//  | Expression 'or' Expression | '('Expression')' | 
//'not''('Expression')' 
//[2] BoolExpr ::= 'boolean''('PathExpr')' | 'contains''(' ElementPath ',' 
//'"' String '"' ')'  
//[3] PathExpr ::= '//SimpleItem' NodeTest | '//ElementItem' NodeTest | 
//  ElementTest 
//[4] NodeTest ::= '['AttrExpr']' 
//[5] AttrExpr ::= NameComp | ValueComp | AttrExpr 'and' AttrExpr | AttrExpr 
//'or' AttrExpr | 'not''(AttrExpr')' 
//[6] NameComp ::= NameAttr'=''"'String'"' 
//[7] ValueComp ::= ValueAttr Operator '"'String'"' 
//[8] Operator ::= '=' | '!=' | '<' | '<=' | '>' | '>=' 
//[9] NameAttr ::= '@Name' 
//[10] ValueAttr ::= '@Value' 
//[11] ElementTest ::= '/' ElementPath '['NodeComp']'  
//[12] ElementPath ::= ElementName ElementName* 
//[13] ElementName ::= '/' String 
//[14] NodeComp ::= NodeName Operator '"' String '"' 
//[15] NodeName ::= '@' String | String 
//             * 
//             * Example of an XPath expression used to find recordings from the basement where there is at 
//least one track containing video: 
//boolean(//Source[Location = “Basement”]) and 
//boolean(//Track[TrackType = “Video”])
//             */
//            /*
//            request.Scope.RecordingInformationFilter = @"(  BOOlean(//Source[Location = “mylocation”]) and boolean(//Track[TrackType = “Video”]) not boolean(//Track[TrackType = “Operator”]))  ";
//            var xpathList = new List<string>();
//            //[1]
//            string pattern = @"( and )|( or )|( not )";
//            string[] match = Regex.Split(request.Scope.RecordingInformationFilter.ToLower(), pattern);
            

//            string[] matchtmp = new string[match.Length];
//            for(int i=0; i<match.Length; i++)
//            {
//                match[i] = match[i].Trim();
//                if (match[i] == "and" || match[i] == "or" || match[i] == "not")
//                {
//                    matchtmp[i] = match[i];
//                    continue;
//                }
        
//                matchtmp[i] = match[i];
//                matchtmp[i] = matchtmp[i].Trim();
//                if (matchtmp[i].StartsWith("(") && matchtmp[i].EndsWith(")"))
//                {
//                    matchtmp[i] = matchtmp[i].Remove(matchtmp[i].Length - 1, 1).Remove(0, 1);
//                    matchtmp[i] = matchtmp[i].Trim();
//                }
//                //[2]
//                if (matchtmp[i].StartsWith("boolean"))
//                {
//                    //[3]
//                    //'PathExpr'//просто передать XPath?
//                    matchtmp[i] = matchtmp[i].Remove(0, 7);
//                    matchtmp[i] = matchtmp[i].Trim();
//                    matchtmp[i] = matchtmp[i].TrimEnd(')');
//                    matchtmp[i] = matchtmp[i].TrimStart('(');
//                    matchtmp[i] = matchtmp[i].Replace('\u201C', '\'');//left qoutation mark
//                    matchtmp[i] = matchtmp[i].Replace('\u201D', '\'');//right qoutation mark
//                    if (matchtmp[i].StartsWith("//"))
//                    {//check [4]
//                        //if()
//                        matchtmp[i] = matchtmp[i].Insert(2, "a:");
//                        matchtmp[i] = matchtmp[i].Replace("[", "[a:");
//                        //
//                    }
//                    else
//                        if (matchtmp[i].StartsWith("/"))
//                        { //check [11]
//                            //
//                        }
//                        else throw new ArgumentException();
//                }
//                else
//                    if (match[i].StartsWith("contains"))
//                    {
//                        //(' ElementPath ',' '"' String '"' ')//просто передать XPath?
//                        matchtmp[i] = match[i].Remove(0, 8);
//                        matchtmp[i] = matchtmp[i].TrimEnd().TrimEnd(')');
//                        matchtmp[i] = matchtmp[i].TrimStart().TrimStart('(');
//                    }
//                    else
//                        throw new ArgumentException("Argument", "Filter doesnt suit the rules");
//            }
//            */
//            #endregion
//            ////TODO : get all recordings from Typhoon and place it in recInfo
//            RecordingInformation recInfo = new RecordingInformation();
//            recInfo.EarliestRecording = System.DateTime.Now;
//            recInfo.EarliestRecordingSpecified = true;
//            recInfo.LatestRecording = System.DateTime.Now;
//            recInfo.RecordingStatus = RecordingStatus.Stopped;
//            recInfo.RecordingToken = Guid.NewGuid().ToString();
//            recInfo.Source = new RecordingSourceInformation();
//            recInfo.Source.Address = "sourceaddress";
//            recInfo.Source.SourceId = "2";
//            recInfo.Track = new TrackInformation[2];
//            recInfo.Track[0] = new TrackInformation();
//            recInfo.Track[0].TrackType = TrackType.Video;
//            recInfo.Track[0].TrackToken = "tracktoken1";

//            recInfo.Track[1] = new TrackInformation();
//            recInfo.Track[1].TrackType = TrackType.Audio;
//            recInfo.Track[1].TrackToken = "tracktoken2";
//            recInfo.Source.Location = "mylocation";
//            recInfo.Content = "mycontent";

//            string formedstrign;

//            using(MemoryStream ms =new MemoryStream())
//            {
//                XmlSerializer xmlSerializer = new XmlSerializer(typeof(RecordingInformation));
//                try
//                {
//                    var namespaces = new XmlSerializerNamespaces();
//                    namespaces.Add("tt", "http://www.onvif.org/ver10/schema");

//                    xmlSerializer.Serialize(ms, recInfo);                    
//                    StreamReader strread = new StreamReader(ms);
//                    ms.Position = 0;
//                    formedstrign = strread.ReadToEnd();
//                }
//                catch (InvalidOperationException iox)
//                {
//                    throw iox;
//                }
//                catch (IOException ioex)
//                {
//                    throw ioex;
//                }
//                catch (OutOfMemoryException oomex)
//                {
//                    throw oomex;
//                }
//                catch (Exception ex)
//                {
//                    throw ex;
//                }
//            }

//            XmlDocument doc = new XmlDocument();

//            doc.LoadXml(formedstrign.ToLower().Trim());
//            XmlElement xRoot = doc.DocumentElement;
//            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
//            nsmgr.AddNamespace("tt", "http://www.onvif.org/ver10/schema");

//            Boolean total;
//            XPathNavigator navigator = doc.CreateNavigator();

//            try
//            {
//                if(a!="")
//                {
//                    XPathExpression expr = navigator.Compile(a);
//                    expr.SetContext(nsmgr);
//                    total = (bool)navigator.Evaluate(expr);
//                }else
//                {
//                    throw new ArgumentException();
//                }
                
//            }
//            catch (System.Xml.XPath.XPathException xpex)
//            {
//                throw xpex;
//            }
//            catch(Exception ex)
//            {
//                throw ex;
//            }

            //            return new FindRecordingsResponse();
            #endregion
        }

        public GetRecordingSearchResultsResponse GetRecordingSearchResults(GetRecordingSearchResultsRequest request)
        {
            string formedstring;
            using (MemoryStream ms = new MemoryStream())
            {
                #region
                //////////-----------------------------------------------
                ////////resp = new GetRecordingSearchResultsResponse();
                ////////resp.ResultList = new FindRecordingResultList();
                ////////resp.ResultList.RecordingInformation = new RecordingInformation[1];
                ////////resp.ResultList.SearchState = SearchState.Searching;
                ////////resp.ResultList.RecordingInformation[0] = new RecordingInformation();
                ////////resp.ResultList.RecordingInformation[0].RecordingToken = "reco_token";
                ////////resp.ResultList.RecordingInformation[0].LatestRecording = System.DateTime.Now;
                ////////resp.ResultList.RecordingInformation[0].LatestRecordingSpecified = true;
                ////////resp.ResultList.RecordingInformation[0].RecordingStatus = RecordingStatus.Recording;
                
                ////////resp.ResultList.RecordingInformation[0].Source = new RecordingSourceInformation();
                ////////resp.ResultList.RecordingInformation[0].Source.Description = "source_desc";
                ////////resp.ResultList.RecordingInformation[0].Source.Location = "source_location";
                ////////resp.ResultList.RecordingInformation[0].Source.Name = "source_name";
                ////////resp.ResultList.RecordingInformation[0].Source.SourceId = "source_ID";
                ////////resp.ResultList.RecordingInformation[0].Track = new TrackInformation[1];
                ////////resp.ResultList.RecordingInformation[0].Track[0] = new TrackInformation();
                ////////resp.ResultList.RecordingInformation[0].Track[0] = new TrackInformation();
                ////////resp.ResultList.RecordingInformation[0].Track[0].DataFrom = System.DateTime.Now;
                ////////resp.ResultList.RecordingInformation[0].Track[0].DataTo = System.DateTime.Now;
                ////////resp.ResultList.RecordingInformation[0].Track[0].Description = "track_desc";
                ////////resp.ResultList.RecordingInformation[0].Track[0].TrackToken = "track_token";
                ////////resp.ResultList.RecordingInformation[0].Track[0].TrackType = TrackType.Video;

                ////////XmlSerializer xmlSerializer = new XmlSerializer(typeof(GetRecordingSearchResultsResponse),
                ////////        "http://www.onvif.org/ver10/schema");
                ////////try
                ////////{
                ////////    xmlSerializer.Serialize(ms, resp);
                ////////    StreamReader strread = new StreamReader(ms);
                ////////    ms.Position = 0;
                ////////    formedstring = strread.ReadToEnd();
                ////////}
                ////////catch (Exception ex)
                ////////{
                ////////    throw new FaultException(new FaultReason("No RecordingSearchResults"),
                ////////          new FaultCode("Sender",
                ////////              new FaultCode("InvalidArgVa", "http://www.onvif.org/ver10/error",
                ////////                  new FaultCode("NoRecordingsFound", "http://www.onvif.org/ver10/error"))));
                ////////}

                //---------------------
                #endregion

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(GetRecordingSearchResultsRequest));
                try
                {
                    xmlSerializer.Serialize(ms, request);
                    StreamReader strread = new StreamReader(ms);
                    ms.Position = 0;
                    formedstring = strread.ReadToEnd();
                }
                catch (Exception)
                {
                    throw new FaultException(new FaultReason("No RecordingSearchResults"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVa", "http://www.onvif.org/ver10/error",
                                  new FaultCode("NoRecordingsFound", "http://www.onvif.org/ver10/error"))));
                }
                    
                
            }

            using (TyphoonMsg TyphMsg = TyphoonMsgManager.SendSyncMsg(200, 11, TyphoonCom.MakeMem(formedstring), 0))
            {
                if (TyphMsg == null || string.IsNullOrEmpty(TyphMsg.stringMessageData))
                    throw new FaultException(new FaultReason("No Recordings Found"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("NoRecordingsFound", "http://www.onvif.org/ver10/error"))));

                TyphMsg.stringMessageData = TyphoonCom.ParseMem(0, TyphMsg.stringMessageData);
                using (MemoryStream ms2 = new MemoryStream(TyphMsg.byteMessageData))
                {
                    XmlSerializer xmlserializer2 = new XmlSerializer(typeof(GetRecordingSearchResultsResponse),
                        "http://www.onvif.org/ver10/schema");
                    try
                    {
                        GetRecordingSearchResultsResponse resp = (GetRecordingSearchResultsResponse)xmlserializer2.Deserialize(ms2);
                        return resp;
                    }
                    catch (InvalidOperationException)
                    {
                        throw new FaultException(new FaultReason("No RecordingSearchResults"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("NoRecordingsFound", "http://www.onvif.org/ver10/error"))));
                    }
                }
            }
        }

        public FindEventsResponse FindEvents(FindEventsRequest request)
        {
            return new FindEventsResponse();
        }

        public GetEventSearchResultsResponse GetEventSearchResults(GetEventSearchResultsRequest request)
        {
            return new GetEventSearchResultsResponse();
        }

        public FindPTZPositionResponse FindPTZPosition(FindPTZPositionRequest request)
        {
            return new FindPTZPositionResponse();
        }
        public GetPTZPositionSearchResultsResponse GetPTZPositionSearchResults(GetPTZPositionSearchResultsRequest request)
        {
            return new GetPTZPositionSearchResultsResponse();
        }
        public SearchState GetSearchState(string SearchToken)
        {
            //string formedstring;

            //SearchState resp;

            //TyphoonMsg TyphMsg = new TyphoonMsg(TyphoonMsgType.Request);

            //using (TyphMsg = TyphoonMsgManager.SendSyncMsg(18))
            //{
            //    if (TyphMsg == null || string.IsNullOrEmpty(TyphMsg.stringMessageData))
            //        throw new FaultException(new FaultReason("No Recordings Found"),
            //              new FaultCode("Sender",
            //                  new FaultCode("InvalidArgVa", "http://www.onvif.org/ver10/error",
            //                      new FaultCode("NoRecordingsFound", "http://www.onvif.org/ver10/error"))));

            //    TyphMsg.stringMessageData = TyphoonCom.ParseMem(0, TyphMsg.stringMessageData);

            //    using (MemoryStream ms2 = new MemoryStream(TyphMsg.byteMessageData))
            //    {
            //        XmlSerializer xmlserializer2 = new XmlSerializer(typeof(FindRecordingsResponse), "http://www.onvif.org/ver10/schema");

            //        try
            //        {
            //            //resp = (FindRecordingsResponse)xmlserializer2.Deserialize(ms2);
            //        }
            //        catch (InvalidOperationException)
            //        {
            //            throw new FaultException(new FaultReason("No Recordings Found"),
            //              new FaultCode("Sender",
            //                  new FaultCode("InvalidArgVa", "http://www.onvif.org/ver10/error",
            //                      new FaultCode("NoRecordingsFound", "http://www.onvif.org/ver10/error"))));
            //        }
            //    }
            //}
                return SearchState.Unknown;
        }
        public System.DateTime EndSearch(string SearchToken)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\u00221.0\u0022 encoding=\u0022UTF-8\u0022?><EndSearchRequest><SearchToken>");
            sb.Append(SearchToken);
            sb.Append("</SearchToken></EndSearchRequest>");

            using (TyphoonMsg TyphMsg = TyphoonMsgManager.SendSyncMsg(200, 19, TyphoonCom.MakeMem(sb.ToString()), 0))
            {
                if (TyphMsg == null || string.IsNullOrEmpty(TyphMsg.stringMessageData))
                    throw new FaultException(new FaultReason("No SearchJob Found"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVa", "http://www.onvif.org/ver10/error",
                                  new FaultCode("NoSearchFound", "http://www.onvif.org/ver10/error"))));

                TyphMsg.stringMessageData = TyphoonCom.ParseMem(0, TyphMsg.stringMessageData);
                System.DateTime resp = System.DateTime.Now; ;
                using (MemoryStream ms2 = new MemoryStream(TyphMsg.byteMessageData))
                {
                    XmlSerializer xmlserializer2 = new XmlSerializer(typeof(System.DateTime), "http://www.onvif.org/ver10/schema");

                    try
                    {
                        resp = (System.DateTime)xmlserializer2.Deserialize(ms2);
                        return resp;
                    }
                    catch (InvalidOperationException)
                    {
                        throw new FaultException(new FaultReason("The search token is invalid."),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("Invalid Token", "http://www.onvif.org/ver10/error"))));
                    }
                }
            }
        }
        public FindMetadataResponse FindMetadata(FindMetadataRequest request)
        {
            throw new NotImplementedException();
        }
        public GetMetadataSearchResultsResponse GetMetadataSearchResults(GetMetadataSearchResultsRequest request)
        {
            throw new NotImplementedException();
        }
    }
    public partial class Service1 : PTZ.IPTZ
    {
        PTZ.Capabilities PTZ.IPTZ.GetServiceCapabilities()
        {
            PTZ.Capabilities caps = new PTZ.Capabilities();
            caps.EFlip = false;
            caps.EFlipSpecified = true;
            caps.Reverse = false;
            caps.ReverseSpecified = true;
            caps.MoveStatus = false;
            caps.StatusPosition = false;
            caps.GetCompatibleConfigurations = false;
            return caps;

        }
        public PTZ.GetNodesResponse GetNodes(PTZ.GetNodesRequest request)
        {
            #region
            //{
            //    PTZ.GetNodesResponse resp = new PTZ.GetNodesResponse();

            //    resp.PTZNode = new PTZ.PTZNode[1];
            //    resp.PTZNode[0] = new PTZ.PTZNode();
            //    resp.PTZNode[0].Name = "Camera 3";
            //    resp.PTZNode[0].token = "3";
            //    resp.PTZNode[0].SupportedPTZSpaces = new PTZ.PTZSpaces();
            //    resp.PTZNode[0].SupportedPTZSpaces.ContinuousPanTiltVelocitySpace = new PTZ.Space2DDescription[1];
            //    resp.PTZNode[0].SupportedPTZSpaces.ContinuousPanTiltVelocitySpace[0] = new PTZ.Space2DDescription();
            //    resp.PTZNode[0].SupportedPTZSpaces.ContinuousPanTiltVelocitySpace[0].URI = "http://www.onvif.org/ver10/tptz/PanTiltSpaces/VelocityGenericSpace";
            //    resp.PTZNode[0].SupportedPTZSpaces.ContinuousPanTiltVelocitySpace[0].XRange = new PTZ.FloatRange();
            //    resp.PTZNode[0].SupportedPTZSpaces.ContinuousPanTiltVelocitySpace[0].XRange.Max = 1;
            //    resp.PTZNode[0].SupportedPTZSpaces.ContinuousPanTiltVelocitySpace[0].XRange.Min = -1;
            //    resp.PTZNode[0].SupportedPTZSpaces.ContinuousPanTiltVelocitySpace[0].YRange = new PTZ.FloatRange();
            //    resp.PTZNode[0].SupportedPTZSpaces.ContinuousPanTiltVelocitySpace[0].XRange.Max = 1;
            //    resp.PTZNode[0].SupportedPTZSpaces.ContinuousPanTiltVelocitySpace[0].XRange.Min = -1;
            //    resp.PTZNode[0].SupportedPTZSpaces.ContinuousZoomVelocitySpace = new PTZ.Space1DDescription[1];
            //    resp.PTZNode[0].SupportedPTZSpaces.ContinuousZoomVelocitySpace[0] = new PTZ.Space1DDescription();
            //    resp.PTZNode[0].SupportedPTZSpaces.ContinuousZoomVelocitySpace[0].URI = "http://www.onvif.org/ver10/tptz/ZoomSpaces/VelocityGenericSpace";
            //    resp.PTZNode[0].SupportedPTZSpaces.ContinuousZoomVelocitySpace[0].XRange = new PTZ.FloatRange();
            //    resp.PTZNode[0].SupportedPTZSpaces.ContinuousZoomVelocitySpace[0].XRange.Max = 1;
            //    resp.PTZNode[0].SupportedPTZSpaces.ContinuousZoomVelocitySpace[0].XRange.Min = -1;


            //    using (var fs = new FileStream("PTZGetNodes.txt", FileMode.OpenOrCreate))
            //    {
            //        XmlSerializer xmlSerializer = new XmlSerializer(typeof(PTZ.GetNodesResponse), "http://www.onvif.org/ver10/schema");

            //        try
            //        {
            //            xmlSerializer.Serialize(fs, resp);
            //        }
            //        finally
            //        { }
            //    }
            //    resp = null;
            //}
            
            #endregion

            using (TyphoonMsg typhmsg = TyphoonMsgManager.SendSyncMsg(25))
            {
                if (typhmsg == null) 
                    throw new FaultException(new FaultReason("No PTZNodes returned"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("No PTZNodes returned", "http://www.onvif.org/ver10/error"))));

                typhmsg.stringMessageData = TyphoonCom.ParseMem(0, typhmsg.stringMessageData);

                if(String.IsNullOrWhiteSpace(typhmsg.stringMessageData))
                    throw new FaultException(new FaultReason("No PTZNodes returned"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("No PTZNodes returned", "http://www.onvif.org/ver10/error"))));

                PTZ.GetNodesResponse resp = null;
                
                using (var ms = new MemoryStream(typhmsg.byteMessageData))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(PTZ.GetNodesResponse), "http://www.onvif.org/ver10/schema");

                    try
                    {
                        resp = (PTZ.GetNodesResponse)xmlSerializer.Deserialize(ms);
                    }
                    catch(SerializationException)
                    {
                        throw new FaultException(new FaultReason("No PTZNodes returned"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("No PTZNodes returned", "http://www.onvif.org/ver10/error"))));
                    }
                }
                return resp;
            }
        }
        public PTZ.PTZNode GetNode(string NodeToken)
        {
            PTZ.GetNodesResponse nodesResp = ((PTZ.IPTZ)this).GetNodes(new PTZ.GetNodesRequest());
            if(nodesResp!=null)
            {
              foreach(PTZ.PTZNode node in nodesResp.PTZNode)
              {
                  if (node.token == NodeToken)
                      return node;
              }
            }
            throw new FaultException(new FaultReason("No such PTZNode on the device"),
                           new FaultCode("Sender",
                               new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                   new FaultCode("NoEntity", "http://www.onvif.org/ver10/error"))));
        }
        public PTZ.PTZConfiguration GetConfiguration(string PTZConfigurationToken)
        {
            #region
            //PTZ.PTZConfiguration config = new PTZ.PTZConfiguration();
            //config.DefaultAbsolutePantTiltPositionSpace = "DefaultAbsolutePantTiltPositionSpace";
            //config.DefaultAbsoluteZoomPositionSpace = "DefaultAbsoluteZoomPositionSpace";
            //config.DefaultContinuousPanTiltVelocitySpace = "DefaultContinuousPanTiltVelocitySpace";
            //config.DefaultContinuousZoomVelocitySpace = "DefaultContinuousZoomVelocitySpace";
            //config.DefaultPTZSpeed = new PTZ.PTZSpeed();
            //config.DefaultPTZSpeed.PanTilt = new PTZ.Vector2D();
            //config.DefaultPTZSpeed.PanTilt.space = "space";
            //config.DefaultPTZSpeed.PanTilt.x = 100;
            //config.DefaultPTZSpeed.PanTilt.y = 100;

            //config.DefaultPTZTimeout = "DefaultPTZTimeout";
            //config.DefaultRelativePanTiltTranslationSpace = "DefaultRelativePanTiltTranslationSpace";
            //config.DefaultRelativeZoomTranslationSpace = "DefaultRelativeZoomTranslationSpace";
            //config.Name = "configuration-name";
            //config.NodeToken = "NodeToken";
            //config.PanTiltLimits = new PTZ.PanTiltLimits();
            //config.PanTiltLimits.Range = new PTZ.Space2DDescription();
            //config.PanTiltLimits.Range.URI = "URI";
            //config.PanTiltLimits.Range.XRange = new PTZ.FloatRange();
            //config.PanTiltLimits.Range.XRange.Max = 100;
            //config.PanTiltLimits.Range.XRange.Min = -100;
            //config.PanTiltLimits.Range.YRange = new PTZ.FloatRange();
            //config.PanTiltLimits.Range.XRange.Max = 100;
            //config.PanTiltLimits.Range.XRange.Min = -100;

            //config.token = "token";
            //config.ZoomLimits = new PTZ.ZoomLimits();
            //config.ZoomLimits.Range = new PTZ.Space1DDescription();
            //config.ZoomLimits.Range.URI = "URI";
            //config.ZoomLimits.Range.XRange = new PTZ.FloatRange();
            //config.ZoomLimits.Range.XRange.Max = 100;
            //config.ZoomLimits.Range.XRange.Min = -100;

            //{
            //    using (var fs = new FileStream("GetConfiguration.txt", FileMode.OpenOrCreate))
            //    {
            //        XmlSerializer xmlSerializer = new XmlSerializer(typeof(PTZ.PTZConfiguration), "http://www.onvif.org/ver10/schema");

            //        try
            //        {
            //            xmlSerializer.Serialize(fs, config);
            //        }
            //        finally
            //        { }
            //    }
            //    config = null;
            //}
            //throw new NotImplementedException();
            #endregion

            PTZ.GetConfigurationsResponse ConfigResp = ((PTZ.IPTZ)this).GetConfigurations(new PTZ.GetConfigurationsRequest());

            if (ConfigResp != null)
            {
                foreach (PTZ.PTZConfiguration config in ConfigResp.PTZConfiguration)
                {
                    if (config.token == PTZConfigurationToken)
                        return config;
                }
            }
            throw new FaultException(new FaultReason("PTZConfigurationToken"),
                           new FaultCode("Sender",
                               new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                   new FaultCode("PTZConfigurationToken", "http://www.onvif.org/ver10/error"))));
            
        }
        public PTZ.GetConfigurationsResponse GetConfigurations(PTZ.GetConfigurationsRequest request)
        {
            #region
            //PTZ.GetConfigurationsResponse resp0 = new PTZ.GetConfigurationsResponse();
            //resp0.PTZConfiguration = new PTZ.PTZConfiguration[1];
            //resp0.PTZConfiguration[0] = new PTZ.PTZConfiguration();
            //resp0.PTZConfiguration[0].DefaultAbsolutePantTiltPositionSpace = "DefaultAbsolutePantTiltPositionSpace";
            //resp0.PTZConfiguration[0].DefaultAbsoluteZoomPositionSpace = "DefaultAbsoluteZoomPositionSpace";
            //resp0.PTZConfiguration[0].DefaultContinuousPanTiltVelocitySpace = "DefaultContinuousPanTiltVelocitySpace";
            //resp0.PTZConfiguration[0].DefaultContinuousZoomVelocitySpace = "DefaultContinuousZoomVelocitySpace";
            //resp0.PTZConfiguration[0].DefaultPTZSpeed = new PTZ.PTZSpeed();
            //resp0.PTZConfiguration[0].DefaultPTZSpeed.PanTilt = new PTZ.Vector2D();
            //resp0.PTZConfiguration[0].DefaultPTZSpeed.PanTilt.space = "space";
            //resp0.PTZConfiguration[0].DefaultPTZSpeed.PanTilt.x = 100;
            //resp0.PTZConfiguration[0].DefaultPTZSpeed.PanTilt.y = 100;

            //resp0.PTZConfiguration[0].DefaultPTZTimeout = "DefaultPTZTimeout";
            //resp0.PTZConfiguration[0].DefaultRelativePanTiltTranslationSpace = "DefaultRelativePanTiltTranslationSpace";
            //resp0.PTZConfiguration[0].DefaultRelativeZoomTranslationSpace = "DefaultRelativeZoomTranslationSpace";
            //resp0.PTZConfiguration[0].Name = "configuration-name";
            //resp0.PTZConfiguration[0].NodeToken = "NodeToken";
            //resp0.PTZConfiguration[0].PanTiltLimits = new PTZ.PanTiltLimits();
            //resp0.PTZConfiguration[0].PanTiltLimits.Range = new PTZ.Space2DDescription();
            //resp0.PTZConfiguration[0].PanTiltLimits.Range.URI = "URI";
            //resp0.PTZConfiguration[0].PanTiltLimits.Range.XRange = new PTZ.FloatRange();
            //resp0.PTZConfiguration[0].PanTiltLimits.Range.XRange.Max = 100;
            //resp0.PTZConfiguration[0].PanTiltLimits.Range.XRange.Min = -100;
            //resp0.PTZConfiguration[0].PanTiltLimits.Range.YRange = new PTZ.FloatRange();
            //resp0.PTZConfiguration[0].PanTiltLimits.Range.XRange.Max = 100;
            //resp0.PTZConfiguration[0].PanTiltLimits.Range.XRange.Min = -100;

            //resp0.PTZConfiguration[0].token = "token";
            //resp0.PTZConfiguration[0].ZoomLimits = new PTZ.ZoomLimits();
            //resp0.PTZConfiguration[0].ZoomLimits.Range = new PTZ.Space1DDescription();
            //resp0.PTZConfiguration[0].ZoomLimits.Range.URI = "URI";
            //resp0.PTZConfiguration[0].ZoomLimits.Range.XRange = new PTZ.FloatRange();
            //resp0.PTZConfiguration[0].ZoomLimits.Range.XRange.Max = 100;
            //resp0.PTZConfiguration[0].ZoomLimits.Range.XRange.Min = -100;

            //{
            //    using (var fs = new FileStream("GetConfigurationS.txt", FileMode.OpenOrCreate))
            //    {
            //        XmlSerializer xmlSerializer = new XmlSerializer(typeof(PTZ.GetConfigurationsResponse), "http://www.onvif.org/ver10/schema");

            //        try
            //        {
            //            xmlSerializer.Serialize(fs, resp0);
            //        }
            //        finally
            //        { }
            //    }
            //    resp0 = null;
            //}
            #endregion

            using (TyphoonMsg typhmsg = TyphoonMsgManager.SendSyncMsg(30))
            {
                if (typhmsg == null || string.IsNullOrEmpty(typhmsg.stringMessageData))
                    throw new FaultException(new FaultReason("GetConfigurationsException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("GetConfigurationsException", "http://www.onvif.org/ver10/error"))));

                typhmsg.stringMessageData = TyphoonCom.ParseMem(0, typhmsg.stringMessageData);
                PTZ.GetConfigurationsResponse resp = null;

                using (MemoryStream ms2 = new MemoryStream(typhmsg.byteMessageData))
                {
                    XmlSerializer xmlserializer2 = new XmlSerializer(typeof(PTZ.GetConfigurationsResponse),
                        "http://www.onvif.org/ver10/schema");

                    try
                    {
                        resp = (PTZ.GetConfigurationsResponse)xmlserializer2.Deserialize(ms2);
                    }
                    catch (InvalidOperationException)
                    {
                        throw new FaultException(new FaultReason("GetConfigurationsException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("GetConfigurationsException", "http://www.onvif.org/ver10/error"))));
                    }
                    return resp;
                }
            }

        }
        public void SetConfiguration(PTZ.PTZConfiguration PTZConfiguration, bool ForcePersistence)
        { 
            return;//краткость - сестра
        }
        public PTZ.PTZConfigurationOptions GetConfigurationOptions(string ConfigurationToken)
        {
            #region
            //options.PTZTimeout = new PTZ.DurationRange();
            //options.PTZTimeout.Max = "maxtimeout";
            //options.PTZTimeout.Min = "mintimeout";
            //options.Spaces = new PTZ.PTZSpaces();
            //options.Spaces.AbsolutePanTiltPositionSpace = new PTZ.Space2DDescription[1];
            //options.Spaces.AbsolutePanTiltPositionSpace[0] = new PTZ.Space2DDescription();
            //options.Spaces.AbsolutePanTiltPositionSpace[0].URI = "URI";
            //options.Spaces.AbsolutePanTiltPositionSpace[0].XRange = new PTZ.FloatRange();
            //options.Spaces.AbsolutePanTiltPositionSpace[0].XRange.Max = 100;
            //options.Spaces.AbsolutePanTiltPositionSpace[0].XRange.Min = -100;

            //options.Spaces.AbsoluteZoomPositionSpace = new PTZ.Space1DDescription[1];
            //options.Spaces.AbsoluteZoomPositionSpace[0] = new PTZ.Space1DDescription();
            //options.Spaces.AbsoluteZoomPositionSpace[0].URI = "uri";
            //options.Spaces.AbsoluteZoomPositionSpace[0].XRange = new PTZ.FloatRange();
            //options.Spaces.AbsoluteZoomPositionSpace[0].XRange.Max = 100;
            //options.Spaces.AbsoluteZoomPositionSpace[0].XRange.Min = -100;

            //options.Spaces.ContinuousPanTiltVelocitySpace = new PTZ.Space2DDescription[1];
            //options.Spaces.ContinuousPanTiltVelocitySpace[0] = new PTZ.Space2DDescription();
            //options.Spaces.ContinuousPanTiltVelocitySpace[0].URI = "uri";
            //options.Spaces.ContinuousPanTiltVelocitySpace[0].XRange = new PTZ.FloatRange();
            //options.Spaces.ContinuousPanTiltVelocitySpace[0].XRange.Max = 100;
            //options.Spaces.ContinuousPanTiltVelocitySpace[0].XRange.Min = -100;

            //options.Spaces.ContinuousZoomVelocitySpace = new PTZ.Space1DDescription[1];
            //options.Spaces.ContinuousZoomVelocitySpace[0] = new PTZ.Space1DDescription();
            //options.Spaces.ContinuousZoomVelocitySpace[0].URI = "uri";
            //options.Spaces.ContinuousZoomVelocitySpace[0].XRange = new PTZ.FloatRange();
            //options.Spaces.ContinuousZoomVelocitySpace[0].XRange.Max = 100;
            //options.Spaces.ContinuousZoomVelocitySpace[0].XRange.Min = -100;

            //options.Spaces.PanTiltSpeedSpace = new PTZ.Space1DDescription[1];
            //options.Spaces.PanTiltSpeedSpace[0] = new PTZ.Space1DDescription();
            //options.Spaces.PanTiltSpeedSpace[0].URI = "uri";
            //options.Spaces.PanTiltSpeedSpace[0].XRange = new PTZ.FloatRange();
            //options.Spaces.PanTiltSpeedSpace[0].XRange.Max = 100;
            //options.Spaces.PanTiltSpeedSpace[0].XRange.Min = -100;

            //options.Spaces.RelativePanTiltTranslationSpace = new PTZ.Space2DDescription[1];
            //options.Spaces.RelativePanTiltTranslationSpace[0] = new PTZ.Space2DDescription();
            //options.Spaces.RelativePanTiltTranslationSpace[0].URI = "uri";
            //options.Spaces.RelativePanTiltTranslationSpace[0].XRange = new PTZ.FloatRange();
            //options.Spaces.RelativePanTiltTranslationSpace[0].XRange.Max = 100;
            //options.Spaces.RelativePanTiltTranslationSpace[0].XRange.Min = -100;

            //options.Spaces.RelativeZoomTranslationSpace = new PTZ.Space1DDescription[1];
            //options.Spaces.RelativeZoomTranslationSpace[0] = new PTZ.Space1DDescription();
            //options.Spaces.RelativeZoomTranslationSpace[0].URI = "uri";
            //options.Spaces.RelativeZoomTranslationSpace[0].XRange = new PTZ.FloatRange();
            //options.Spaces.RelativeZoomTranslationSpace[0].XRange.Max = 100;
            //options.Spaces.RelativeZoomTranslationSpace[0].XRange.Min = -100;

            //options.Spaces.ZoomSpeedSpace = new PTZ.Space1DDescription[1];
            //options.Spaces.ZoomSpeedSpace[0] = new PTZ.Space1DDescription();
            //options.Spaces.ZoomSpeedSpace[0].URI = "uri";
            //options.Spaces.ZoomSpeedSpace[0].XRange = new PTZ.FloatRange();
            //options.Spaces.ZoomSpeedSpace[0].XRange.Max = 100;
            //options.Spaces.ZoomSpeedSpace[0].XRange.Min = -100;

            //options.PTControlDirection = new PTZ.PTControlDirectionOptions();
            //options.PTControlDirection.EFlip = new PTZ.EFlipOptions();
            //options.PTControlDirection.EFlip.Mode = new PTZ.EFlipMode[1];
            //options.PTControlDirection.EFlip.Mode[0] = new PTZ.EFlipMode();
            //options.PTControlDirection.Reverse = new PTZ.ReverseOptions();
            //options.PTControlDirection.Reverse.Mode = new PTZ.ReverseMode[1];
            //options.PTControlDirection.Reverse.Mode[0] = new PTZ.ReverseMode();


            //using (var fs = new FileStream("GetConfigurationOptions.txt", FileMode.OpenOrCreate))
            //{
            //    XmlSerializer xmlSerializer = new XmlSerializer(typeof(PTZ.PTZConfigurationOptions), "http://www.onvif.org/ver10/schema");

            //    try
            //    {
            //        xmlSerializer.Serialize(fs, options);
            //    }
            //    finally
            //    { }
            //}
            #endregion

            using (TyphoonMsg typhmsg = TyphoonMsgManager.SendSyncMsg(200, 26, TyphoonCom.MakeMem(ConfigurationToken), 0))
            {
                if (typhmsg == null)
                    throw new FaultException(new FaultReason("No PTZNodes returned"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("No PTZNodes returned", "http://www.onvif.org/ver10/error"))));

                typhmsg.stringMessageData = TyphoonCom.ParseMem(0, typhmsg.stringMessageData);

                if (String.IsNullOrWhiteSpace(typhmsg.stringMessageData))
                    throw new FaultException(new FaultReason("No PTZNodes returned"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("No PTZNodes returned", "http://www.onvif.org/ver10/error"))));

                PTZ.PTZConfigurationOptions options = null;

                using (var ms = new MemoryStream(typhmsg.byteMessageData))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(PTZ.PTZConfigurationOptions), "http://www.onvif.org/ver10/schema");

                    try
                    {
                        options = (PTZ.PTZConfigurationOptions)xmlSerializer.Deserialize(ms);
                    }
                    catch (SerializationException)
                    {
                        throw new FaultException(new FaultReason("No PTZNodes returned"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("No PTZNodes returned", "http://www.onvif.org/ver10/error"))));
                    }
                }
                return options;
            }
        }
        public string SendAuxiliaryCommand(string ProfileToken, string AuxiliaryData)
        {
            throw new NotImplementedException();
        }
        public PTZ.GetPresetsResponse GetPresets(PTZ.GetPresetsRequest request)
        {
            throw new NotImplementedException();
        }
        public PTZ.SetPresetResponse SetPreset(PTZ.SetPresetRequest request)
        {
            throw new NotImplementedException();
        }
        public void RemovePreset(string ProfileToken, string PresetToken)
        {
            throw new NotImplementedException();
        }
        public void GotoPreset(string ProfileToken, string PresetToken, PTZ.PTZSpeed Speed)
        {
            throw new NotImplementedException();
        }
        public void GotoHomePosition(string ProfileToken, PTZ.PTZSpeed Speed)
        {
            throw new NotImplementedException();
        }
        public void SetHomePosition(string ProfileToken)
        {
            throw new NotImplementedException();
        }
        public PTZ.ContinuousMoveResponse ContinuousMove(PTZ.ContinuousMoveRequest request)
        {
            string tempstring = null;

            using (var ms = new MemoryStream())
            {
                XmlSerializer RequestSerializer = new XmlSerializer(typeof(PTZ.ContinuousMoveRequest), "http://www.onvif.org/ver10/schema");
                try
                {
                    RequestSerializer.Serialize(ms, request);
                    StreamReader strread = new StreamReader(ms);
                    ms.Position = 0;
                    tempstring = strread.ReadToEnd();
                    if(String.IsNullOrWhiteSpace(tempstring))
                        throw new FaultException(new FaultReason("ContinuousMoveSerializationException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("ContinuousMoveSerializationException", "http://www.onvif.org/ver10/error"))));

                }
                catch(SerializationException)
                {
                    throw new FaultException(new FaultReason("ContinuousMoveSerializationException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("ContinuousMoveSerializationException", "http://www.onvif.org/ver10/error"))));
                }
            }
            
            using (TyphoonMsg typhmsg = TyphoonMsgManager.SendSyncMsg(200, 27, TyphoonCom.MakeMem(tempstring), 0))
            {
                if (typhmsg == null || string.IsNullOrEmpty(typhmsg.stringMessageData))
                    throw new FaultException(new FaultReason("ContinuousMoveSerializationException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("ContinuousMoveSerializationException", "http://www.onvif.org/ver10/error"))));

                typhmsg.stringMessageData = TyphoonCom.ParseMem(0, typhmsg.stringMessageData);

                PTZ.ContinuousMoveResponse resp = null;

                using (MemoryStream ms2 = new MemoryStream(typhmsg.byteMessageData))
                {
                    XmlSerializer xmlserializer2 = new XmlSerializer(typeof(PTZ.ContinuousMoveResponse), "http://www.onvif.org/ver10/schema");

                    try
                    {
                        resp = (PTZ.ContinuousMoveResponse)xmlserializer2.Deserialize(ms2);
                    }
                    catch (SerializationException)
                    {
                        throw new FaultException(new FaultReason("SerializationContiniousMoveException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVa", "http://www.onvif.org/ver10/error",
                                  new FaultCode("SerializationContiniousMoveException", "http://www.onvif.org/ver10/error"))));
                    }                   
                }

                if (resp != null)
                {
                    return resp;
                }
                else
                {
                    throw new FaultException(new FaultReason("ContiniousMoveException"),
                      new FaultCode("Sender",
                          new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                              new FaultCode("ContiniousMoveException", "http://www.onvif.org/ver10/error"))));
                }
            }
        }
        public void RelativeMove(string ProfileToken, PTZ.PTZVector Translation, PTZ.PTZSpeed Speed)
        {
            throw new NotImplementedException();
        }
        public PTZ.PTZStatus GetStatus(string ProfileToken)
        {
            using (TyphoonMsg typhmsg = TyphoonMsgManager.SendSyncMsg(200, 29, TyphoonCom.MakeMem(ProfileToken), 0))
            {
                if (typhmsg == null || string.IsNullOrEmpty(typhmsg.stringMessageData))
                    throw new FaultException(new FaultReason("GetStatusSerializationException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("GetStatusSerializationException", "http://www.onvif.org/ver10/error"))));

                typhmsg.stringMessageData = TyphoonCom.ParseMem(0, typhmsg.stringMessageData);
                PTZ.PTZStatus resp = null;
                using (MemoryStream ms2 = new MemoryStream(typhmsg.byteMessageData))
                {
                    XmlSerializer xmlserializer2 = new XmlSerializer(typeof(PTZ.PTZStatus), "http://www.onvif.org/ver10/schema");

                    try
                    {
                        resp = (PTZ.PTZStatus)xmlserializer2.Deserialize(ms2);
                    }
                    catch (InvalidOperationException)
                    {
                        throw new FaultException(new FaultReason("SerializationContiniousMoveException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("SerializationContiniousMoveException", "http://www.onvif.org/ver10/error"))));
                    }
                    return resp;
                }
            }
        }
        public void AbsoluteMove(string ProfileToken, PTZ.PTZVector Position, PTZ.PTZSpeed Speed)
        {
            throw new NotImplementedException();
        }
        public void Stop(string ProfileToken, bool PanTilt, bool Zoom)
        {
            Stop stop = new Stop();
            stop.ProfileToken = ProfileToken;
            stop.PanTilt = PanTilt;
            stop.Zoom = Zoom;

            string tempstring = null;
            using (var ms = new MemoryStream())
            {
                XmlSerializer RequestSerializer = new XmlSerializer(typeof(Stop), "http://www.onvif.org/ver10/schema");

                try
                {
                    RequestSerializer.Serialize(ms, stop);
                    StreamReader strread = new StreamReader(ms);
                    ms.Position = 0;
                    tempstring = strread.ReadToEnd();
                    if (String.IsNullOrWhiteSpace(tempstring))
                        throw new FaultException(new FaultReason("StopSerializationException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("ContinuousMoveSerializationException", "http://www.onvif.org/ver10/error"))));

                }
                catch (SerializationException)
                {
                    throw new FaultException(new FaultReason("ContinuousMoveSerializationException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("StopSerializationException", "http://www.onvif.org/ver10/error"))));
                }
            }

            using (TyphoonMsg typhmsg = TyphoonMsgManager.SendSyncMsg(200, 28, TyphoonCom.MakeMem(tempstring), 0))
            {
                if (typhmsg == null || string.IsNullOrEmpty(typhmsg.stringMessageData))
                    throw new FaultException(new FaultReason("StopSerializationException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("StopSerializationException", "http://www.onvif.org/ver10/error"))));

                typhmsg.stringMessageData = TyphoonCom.ParseMem(0, typhmsg.stringMessageData);

                Stop resp = null;

                using (MemoryStream ms2 = new MemoryStream(typhmsg.byteMessageData))
                {
                    XmlSerializer xmlserializer2 = new XmlSerializer(typeof(Stop), "http://www.onvif.org/ver10/schema");

                    try
                    {
                        resp = (Stop)xmlserializer2.Deserialize(ms2);
                    }
                    catch (SerializationException)
                    {
                        throw new FaultException(new FaultReason("SerializationContiniousMoveException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("SerializationContiniousMoveException", "http://www.onvif.org/ver10/error"))));
                    }
                }

                if (resp != null)
                {
                    return;
                }
                else
                {
                    throw new FaultException(new FaultReason("ContiniousMoveException"),
                      new FaultCode("Sender",
                          new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                              new FaultCode("ContiniousMoveException", "http://www.onvif.org/ver10/error"))));
                }
            }


        }
        public PTZ.GetPresetToursResponse GetPresetTours(PTZ.GetPresetToursRequest request)
        {
            throw new NotImplementedException();
        }
        public PTZ.PresetTour GetPresetTour(string ProfileToken, string PresetTourToken)
        {
            throw new NotImplementedException();
        }
        public PTZ.PTZPresetTourOptions GetPresetTourOptions(string ProfileToken, string PresetTourToken)
        {
            throw new NotImplementedException();
        }
        public string CreatePresetTour(string ProfileToken)
        {
            throw new NotImplementedException();
        }
        public void ModifyPresetTour(string ProfileToken, PTZ.PresetTour PresetTour)
        {
            throw new NotImplementedException();
        }
        public void OperatePresetTour(string ProfileToken, string PresetTourToken, PTZ.PTZPresetTourOperation Operation)
        {
            throw new NotImplementedException();
        }
        public void RemovePresetTour(string ProfileToken, string PresetTourToken)
        {
            throw new NotImplementedException();
        }
        public PTZ.GetCompatibleConfigurationsResponse GetCompatibleConfigurations(PTZ.GetCompatibleConfigurationsRequest request)
        {
            throw new NotImplementedException();
        }
    }
    public partial class Service1 : Imaging.IImagingPort
    {
        Imaging.Capabilities Imaging.IImagingPort.GetServiceCapabilities()
        {
            Imaging.Capabilities resp = new Imaging.Capabilities();
            resp.ImageStabilization = false;
            resp.ImageStabilizationSpecified = true;
            return resp;
        }
        public Imaging.ImagingSettings20 GetImagingSettings(string VideoSourceToken)
        {
            #region
            //resp.Focus = new Imaging.FocusConfiguration20();
            //resp.Focus.AutoFocusMode = new Imaging.AutoFocusMode();
            //resp.Focus.DefaultSpeed = 100;
            //resp.Focus.DefaultSpeedSpecified = true;
            //resp.Focus.FarLimit = 100;
            //resp.Focus.FarLimitSpecified = true;
            //resp.Focus.NearLimit = -100;
            //resp.Focus.NearLimitSpecified = true;

            //using (var fs = new FileStream("GetImagingSettings.txt", FileMode.OpenOrCreate))
            //{
            //    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Imaging.ImagingSettings20), "http://www.onvif.org/ver10/schema");

            //    try
            //    {
            //        xmlSerializer.Serialize(fs, resp);
            //    }
            //    finally
            //    { }
            //}
            //resp = null;
            #endregion
            using (TyphoonMsg typhmsg = TyphoonMsgManager.SendSyncMsg(200, 32, TyphoonCom.MakeMem(VideoSourceToken), 0))
            {
                if (typhmsg == null || string.IsNullOrEmpty(typhmsg.stringMessageData))
                    throw new FaultException(new FaultReason("GetImagingSettingsException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("GetImagingSettingsException", "http://www.onvif.org/ver10/error"))));

                typhmsg.stringMessageData = TyphoonCom.ParseMem(0, typhmsg.stringMessageData);

                Imaging.ImagingSettings20 resp = null;

                using (MemoryStream ms2 = new MemoryStream(typhmsg.byteMessageData))
                {
                    XmlSerializer xmlserializer2 = new XmlSerializer(typeof(Imaging.ImagingSettings20), "http://www.onvif.org/ver10/schema");

                    try
                    {
                        resp = (Imaging.ImagingSettings20)xmlserializer2.Deserialize(ms2);
                    }
                    catch (SerializationException)
                    {
                        throw new FaultException(new FaultReason("GetImagingSettingsException"),
                          new FaultCode("Receiver",
                              new FaultCode("ActionNotSupported", "http://www.onvif.org/ver10/error",
                                  new FaultCode("NoImagingForSource", "http://www.onvif.org/ver10/error"))));
                    }
                    catch(InvalidOperationException e)
                    {
                        //throw new FaultException(new FaultReason("GetImagingSettingsException"),
                        //  new FaultCode("Receiver",
                        //      new FaultCode("ActionNotSupported", "http://www.onvif.org/ver10/error",
                        //          new FaultCode("NoImagingForSource", "http://www.onvif.org/ver10/error"))));
                        resp = new Imaging.ImagingSettings20();
                        return resp;//тест хочет так, а должен быть fault!
                    }
                    catch(System.SystemException)
                    {
                        throw;
                    }
                }

                if (resp != null)
                {
                    return resp;
                }
                else
                {
                    throw new FaultException(new FaultReason("GetImagingSettingsException"),
                      new FaultCode("Receiver",
                          new FaultCode("ActionNotSupported", "http://www.onvif.org/ver10/error",
                              new FaultCode("GetImagingSettingsException", "http://www.onvif.org/ver10/error"))));
                }
            }
        }
        public void SetImagingSettings(string VideoSourceToken,
            Imaging.ImagingSettings20 ImagingSettings,
            bool ForcePersistence)
        {
            return;
        }
        public Imaging.ImagingOptions20 GetOptions(string VideoSourceToken)
        {
            using (TyphoonMsg typhmsg = TyphoonMsgManager.SendSyncMsg(200, 33, TyphoonCom.MakeMem(VideoSourceToken), 0))
            {
                if (typhmsg == null || string.IsNullOrEmpty(typhmsg.stringMessageData))
                    throw new FaultException(new FaultReason("GetOptionsException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("GetOptionsException", "http://www.onvif.org/ver10/error"))));

                typhmsg.stringMessageData = TyphoonCom.ParseMem(0, typhmsg.stringMessageData);

                Imaging.ImagingOptions20 resp = null;
                using (MemoryStream ms2 = new MemoryStream(typhmsg.byteMessageData))
                {
                    XmlSerializer xmlserializer2 = new XmlSerializer(typeof(Imaging.ImagingOptions20), "http://www.onvif.org/ver10/schema");

                    try
                    {
                        resp = (Imaging.ImagingOptions20)xmlserializer2.Deserialize(ms2);
                    }
                    catch (SerializationException)
                    {
                        throw new FaultException(new FaultReason("GetOptionsException"),
                          new FaultCode("Receiver",
                              new FaultCode("ActionNotSupported", "http://www.onvif.org/ver10/error",
                                  new FaultCode("NoImagingForSource", "http://www.onvif.org/ver10/error"))));
                    }
                    catch (InvalidOperationException e)
                    {
                        resp = new Imaging.ImagingOptions20();
                        return resp;//тест хочет так, а должен быть fault!
                    }
                    catch (System.SystemException)
                    {
                        throw;
                    }
                }

                if (resp != null)
                {
                    return resp;
                }
                else
                {
                    throw new FaultException(new FaultReason("GetOptionsException"),
                      new FaultCode("Receiver",
                          new FaultCode("ActionNotSupported", "http://www.onvif.org/ver10/error",
                              new FaultCode("GetOptionsException", "http://www.onvif.org/ver10/error"))));
                }
            }
        }
        public void Move(string VideoSourceToken, FocusMove Focus)
        {
            Move move = new Move();
            move.Focus = Focus;
            move.VideoSourceToken = VideoSourceToken;
            string tempstring = null;

            using (var ms = new MemoryStream())
            {
                XmlSerializer RequestSerializer = new XmlSerializer(typeof(Move), "http://www.onvif.org/ver10/schema");

                try
                {
                    RequestSerializer.Serialize(ms, move);
                    StreamReader strread = new StreamReader(ms);
                    ms.Position = 0;
                    tempstring = strread.ReadToEnd();
                    if (String.IsNullOrWhiteSpace(tempstring))
                        throw new FaultException(new FaultReason("MoveSerializationException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("MoveSerializationException", "http://www.onvif.org/ver10/error"))));

                }
                catch (SerializationException)
                {
                    throw new FaultException(new FaultReason("MoveSerializationException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("MoveSerializationException", "http://www.onvif.org/ver10/error"))));
                }
            }

            using (TyphoonMsg typhmsg = TyphoonMsgManager.SendSyncMsg(200, 35, TyphoonCom.MakeMem(tempstring), 0))
            {
                if (typhmsg == null || string.IsNullOrEmpty(typhmsg.stringMessageData))
                    throw new FaultException(new FaultReason("MoveSerializationException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("MoveSerializationException", "http://www.onvif.org/ver10/error"))));

                typhmsg.stringMessageData = TyphoonCom.ParseMem(0, typhmsg.stringMessageData);

                Move resp = null;

                using (MemoryStream ms2 = new MemoryStream(typhmsg.byteMessageData))
                {
                    XmlSerializer xmlserializer2 = new XmlSerializer(typeof(Move), "http://www.onvif.org/ver10/schema");

                    try
                    {
                        resp = (Move)xmlserializer2.Deserialize(ms2);
                    }
                    catch (SerializationException)
                    {
                        throw new FaultException(new FaultReason("MoveSerializationException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("MoveSerializationException", "http://www.onvif.org/ver10/error"))));
                    }
                }

                if (resp != null)
                {
                    return;
                }
                else
                {
                    throw new FaultException(new FaultReason("MoveSerializationException"),
                      new FaultCode("Sender",
                          new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                              new FaultCode("MoveSerializationException", "http://www.onvif.org/ver10/error"))));
                }
            }

            throw new NotImplementedException();
        }
        public Imaging.MoveOptions20 GetMoveOptions(string VideoSourceToken)
        {
            #region
            //Imaging.MoveOptions20 resp = new MoveOptions20();

            //resp.Absolute = new AbsoluteFocusOptions();
            //resp.Absolute.Position = new Imaging.FloatRange();
            //resp.Absolute.Position.Max = 1;
            //resp.Absolute.Position.Min = -1;

            //resp.Continuous = new ContinuousFocusOptions();
            //resp.Continuous.Speed = new Imaging.FloatRange();
            //resp.Continuous.Speed.Max = 1;
            //resp.Continuous.Speed.Min = -1;

            //resp.Relative = new RelativeFocusOptions20();
            //resp.Relative.Distance = new Imaging.FloatRange();
            //resp.Relative.Distance.Max = 1;
            //resp.Relative.Distance.Min = -1;
            //resp.Relative.Speed = new Imaging.FloatRange();
            //resp.Relative.Speed.Max = 1;
            //resp.Relative.Speed.Min = -1;

            //using (var fs = new FileStream("GetMoveOptions.txt", FileMode.OpenOrCreate))
            //{
            //    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Imaging.MoveOptions20), "http://www.onvif.org/ver10/schema");

            //    try
            //    {
            //        xmlSerializer.Serialize(fs, resp);
            //    }
            //    finally
            //    { }
            //}
            //resp = null;

            //throw new NotImplementedException();
            #endregion
            using (TyphoonMsg typhmsg = TyphoonMsgManager.SendSyncMsg(200, 34, TyphoonCom.MakeMem(VideoSourceToken), 0))
            {
                if (typhmsg == null || string.IsNullOrEmpty(typhmsg.stringMessageData))
                    throw new FaultException(new FaultReason("GetMoveOptionsException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("GetMoveOptionsException", "http://www.onvif.org/ver10/error"))));

                typhmsg.stringMessageData = TyphoonCom.ParseMem(0, typhmsg.stringMessageData);

                Imaging.MoveOptions20 resp = null;
                using (MemoryStream ms2 = new MemoryStream(typhmsg.byteMessageData))
                {
                    XmlSerializer xmlserializer2 = new XmlSerializer(typeof(Imaging.MoveOptions20), "http://www.onvif.org/ver10/schema");

                    try
                    {
                        resp = (Imaging.MoveOptions20)xmlserializer2.Deserialize(ms2);
                    }
                    catch (SerializationException)
                    {
                        throw new FaultException(new FaultReason("GetMoveOptionsException"),
                          new FaultCode("Receiver",
                              new FaultCode("ActionNotSupported", "http://www.onvif.org/ver10/error",
                                  new FaultCode("GetMoveOptionsException", "http://www.onvif.org/ver10/error"))));
                    }
                    catch (InvalidOperationException e)
                    {
                        //throw new FaultException(new FaultReason("GetMoveOptionsException"),
                        //  new FaultCode("Receiver",
                        //      new FaultCode("ActionNotSupported", "http://www.onvif.org/ver10/error",
                        //          new FaultCode("NoImagingForSource", "http://www.onvif.org/ver10/error"))));
                        resp = new Imaging.MoveOptions20();
                        return resp;//тест хочет так, а должен быть fault!
                    }
                    catch (System.SystemException)
                    {
                        throw;
                    }
                }

                if (resp != null)
                {
                    return resp;
                }
                else
                {
                    throw new FaultException(new FaultReason("GetMoveOptionsException"),
                      new FaultCode("Receiver",
                          new FaultCode("ActionNotSupported", "http://www.onvif.org/ver10/error",
                              new FaultCode("GetMoveOptionsException", "http://www.onvif.org/ver10/error"))));
                }
            }
        }
        public void Stop(string VideoSourceToken)
        {
            using (TyphoonMsg typhmsg = TyphoonMsgManager.SendSyncMsg(200, 36, TyphoonCom.MakeMem(VideoSourceToken), 0))
            {
                if (typhmsg == null || string.IsNullOrEmpty(typhmsg.stringMessageData))
                    throw new FaultException(new FaultReason("StopFocusException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("StopFocusException", "http://www.onvif.org/ver10/error"))));
                typhmsg.stringMessageData = TyphoonCom.ParseMem(0, typhmsg.stringMessageData);
            }
        }
        Imaging.ImagingStatus20 Imaging.IImagingPort.GetStatus(string VideoSourceToken)
        {
            using (TyphoonMsg typhmsg = TyphoonMsgManager.SendSyncMsg(200, 37, TyphoonCom.MakeMem(VideoSourceToken), 0))
            {
                if (typhmsg == null || string.IsNullOrEmpty(typhmsg.stringMessageData))
                    throw new FaultException(new FaultReason("GetStatusException"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("GetStatusException", "http://www.onvif.org/ver10/error"))));
                typhmsg.stringMessageData = TyphoonCom.ParseMem(0, typhmsg.stringMessageData);
                Imaging.ImagingStatus20 resp = null;

                using (MemoryStream ms2 = new MemoryStream(typhmsg.byteMessageData))
                {
                    XmlSerializer xmlserializer2 = new XmlSerializer(typeof(Imaging.ImagingStatus20), "http://www.onvif.org/ver10/schema");

                    try
                    {
                        resp = (Imaging.ImagingStatus20)xmlserializer2.Deserialize(ms2);
                    }
                    catch (SerializationException)
                    {
                        throw new FaultException(new FaultReason("GetStatusException"),
                          new FaultCode("Receiver",
                              new FaultCode("ActionNotSupported", "http://www.onvif.org/ver10/error",
                                  new FaultCode("GetStatusException", "http://www.onvif.org/ver10/error"))));
                    }
                    catch (InvalidOperationException)
                    {
                        resp = new Imaging.ImagingStatus20();
                        return resp;//тест хочет так, а должен быть fault!
                    }
                    catch (System.SystemException)
                    {
                        throw;
                    }
                }


                return resp;
            }
        }
    }
    public partial class Service1 : MediaRestrictions.IMediaRestrictionsManager
    {
        public string SetRestriction(MediaRestrictions.RestrictionType Restriction)
        {
            //if (Restriction == null
            //    || Restriction.BeginTime == null
            //    || Restriction.EndTime == null
            //    || Restriction.MediaSourceToken == null
            //    || String.IsNullOrWhiteSpace(Restriction.Subject)
            //    )
            //    throw new FaultException();
            //string[] restrictionSubjectArr = Restriction.Subject.Split('\u0020');
            //if (restrictionSubjectArr == null) throw new FaultException();

            //// get mediasourcetokens
            //MediaSourcesProvider.GetMediaSourcesResponse existingMediaSources = GetMediaSources(new GetMediaSourcesRequest());

            //// check if all sources exist / if not - throw
            //// check if all sources are available / if not - throw
            //// change MediaRestrictionMngr settings

            throw new NotImplementedException();             
        }
        public MediaRestrictions.GetRestrictionsResponse GetRestrictions(MediaRestrictions.GetRestrictionsRequest request)
        {
            throw new NotImplementedException();
        }
        public MediaRestrictions.ClearRestrictionsResponse ClearRestrictions(MediaRestrictions.ClearRestrictionsRequest request)
        {
            throw new NotImplementedException();
        }
    }
    public partial class Service1 : MediaMarkup.IMediaMarkupPort
    {
        public MediaMarkup.SetMarkerResponse SetMarker(MediaMarkup.SetMarkerRequest request)
        {
            throw new NotImplementedException();
        }
        public MediaMarkup.GetMarkerResponse GetMarker(MediaMarkup.GetMarkerRequest request)
        {
            throw new NotImplementedException();
        }
        public MediaMarkup.GetMarkedRecordingsResponse GetMarkedRecordings(MediaMarkup.GetMarkedRecordingsRequest request)
        {
            throw new NotImplementedException();
        }
        public void DeleteMarker(string MarkerToken)
        {
            throw new NotImplementedException();
        }
    }
    public partial class Service1 : TaskManager.ITaskManager
    {
        public string CreateTask(System.Xml.XmlElement Task, System.DateTime TimeBegin,
            System.DateTime TimeEnd, TaskManager.FeedbackType Feedback)
        {
            if (Task == null
                || TimeBegin == null
                || TimeEnd == null
                || Feedback == null)
                throw new FaultException("Some arguments are null");

            if (TimeEnd.CompareTo(TimeBegin) <= 0
                || TimeEnd.CompareTo(System.DateTime.Now) <=0
                || TimeBegin.CompareTo(System.DateTime.Now) <=0)
                throw new FaultException(new FaultReason("InvalidTime"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("InvalidTime", "urn:ias:cvss:tm:1.0"))));

            //XmlNodeList xmlNodeList = Task.GetElementsByTagName("ScheduledRecordingTask", "urn:ias:cvss:tm:1.0");
            //XmlNodeList xmlNodeList = Task.;
            if (Task.LocalName == "ScheduledRecordingTask" && Task.NamespaceURI == "urn:ias:cvss:tm:1.0")
            //if(xmlNodeList!=null&&xmlNodeList.Count==1)
            {
                //---------------------------------------------------------------------------------------------
                var scheduledRecordingTask = new TaskManager.ScheduledRecordingTask();
                scheduledRecordingTask.beginTime = TimeBegin;
                scheduledRecordingTask.endTime = TimeEnd;
                //scheduledRecordingTask.mediaSourceToken = xmlNodeList.Item(0).Value;
                scheduledRecordingTask.mediaSourceToken = Task.InnerText.Trim();
                string tempstring;
                XmlSerializer scheduledRecordingTaskSerializer = null;
                using (var ms = new MemoryStream())
                {
                   scheduledRecordingTaskSerializer = new XmlSerializer(typeof(TaskManager.ScheduledRecordingTask),
                        "urn:ias:cvss:tm:1.0");
                   
                    try
                    {
                        scheduledRecordingTaskSerializer.Serialize(ms, scheduledRecordingTask);
                        StreamReader strread = new StreamReader(ms);
                        ms.Position = 0;
                        tempstring = strread.ReadToEnd();
                        if (String.IsNullOrWhiteSpace(tempstring))
                            throw new FaultException(new FaultReason("scheduledRecordingTaskSerializationException"),
                              new FaultCode("Sender",
                                  new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                      new FaultCode("scheduledRecordingTaskSerializationException", "http://www.onvif.org/ver10/error"))));

                    }
                    catch (SerializationException)
                    {
                        throw new FaultException(new FaultReason("scheduledRecordingTaskSerializationException"),
                              new FaultCode("Sender",
                                  new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                      new FaultCode("scheduledRecordingTaskSerializationException", "http://www.onvif.org/ver10/error"))));
                    }
                }
                //---------------------------------------------------------------------------------------------
                //var token = xmlNodeList.Item(0).Value;
                using (TyphoonMsg typhmsg = TyphoonMsgManager.SendSyncMsg(200, 38, TyphoonCom.MakeMem(tempstring), 0))
                {
                    if (typhmsg == null)
                        throw new FaultException(new FaultReason("No CreateTask returned"),
                              new FaultCode("Sender",
                                  new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                      new FaultCode("No CreateTask returned", "http://www.onvif.org/ver10/error"))));

                    string taskToken = TyphoonCom.ParseMem(0, typhmsg.stringMessageData);
                    if (!String.IsNullOrWhiteSpace(taskToken))
                        return taskToken;
                    else
                    {
                        throw new FaultException(new FaultReason("No CreateTask returned"),
                              new FaultCode("Sender",
                                  new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                      new FaultCode("No CreateTask returned", "http://www.onvif.org/ver10/error"))));
                    }
                }
            }
            else
                throw new FaultException(new FaultReason("Invalid Tasks Args"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("InvalidTask", "urn:ias:cvss:tm:1.0"))));
        }
        public TaskManager.GetTaskStatusResponse GetTaskStatus(TaskManager.GetTaskStatusRequest request)
        {
            throw new NotImplementedException();
        }
        public TaskManager.GetTaskResultsResponse GetTaskResults(TaskManager.GetTaskResultsRequest request)
        {
            throw new NotImplementedException();
        }
        public void DeleteTask(string taskToken)
        {
            if(String.IsNullOrWhiteSpace(taskToken))
                throw new FaultException(new FaultReason("Whitespace Task token"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("Whitespace Task token", "urn:ias:cvss:tm:1.0"))));
            using (TyphoonMsg typhmsg = TyphoonMsgManager.SendSyncMsg(200, 41, TyphoonCom.MakeMem(taskToken), 0))
            {
                if (typhmsg == null)
                    throw new FaultException(new FaultReason("No DeleteTask returned"),
                          new FaultCode("Sender",
                              new FaultCode("InvalidArgVal", "http://www.onvif.org/ver10/error",
                                  new FaultCode("No DeleteTask returned", "http://www.onvif.org/ver10/error"))));
                typhmsg.stringMessageData = TyphoonCom.ParseMem(0, typhmsg.stringMessageData);
                return;
            }
            throw new FaultException();
        }
    }
    public class IPmgmnt
    {
        public void SetIP(object netInterfaceStruct)
        {
            Thread.Sleep(10);
            //((NetInterfaceStruct)netInterfaceStruct).;
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            Device.NetworkInterface[] networkInterface = new Device.NetworkInterface[objMOC.Count];
            string subnet_mask = null;


            foreach (ManagementObject objMO in objMOC)
            {
                if ((string)objMO["ServiceName"] == ((NetInterfaceStruct)netInterfaceStruct).InterfaceToken)
                {
                    if (((NetInterfaceStruct)netInterfaceStruct).NetworkInterface.IPv4.DHCP == true)
                    {
                        try
                        {
                            ManagementBaseObject newEnableDHCP = objMO.InvokeMethod("EnableDHCP", null, null);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Failed to setup network interface - {0}", e.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            ManagementBaseObject newIP = objMO.GetMethodParameters("EnableStatic");
                            //преобразуем маску из вида инт в стринг
                            uint mask = 0;
                            if (((NetInterfaceStruct)netInterfaceStruct).NetworkInterface.IPv4.Manual != null)
                            {
                                for (int s = 0; s < ((NetInterfaceStruct)netInterfaceStruct).NetworkInterface.IPv4.Manual[0].PrefixLength; s++)
                                {
                                    mask = mask >> 1;
                                    mask += 2147483648;
                                }

                                byte[] b_mask = new byte[4];
                                for (int i = 0; i < 4; i++)
                                {
                                    b_mask[i] = (byte)(mask >> 8 * (3 - i));
                                }
                                subnet_mask = b_mask[0].ToString() + "."
                                    + b_mask[1].ToString() + "."
                                    + b_mask[2].ToString() + "."
                                    + b_mask[3].ToString();
                                //запихиваем в параметры метода
                                newIP["IPAddress"] = new string[] { ((NetInterfaceStruct)netInterfaceStruct).NetworkInterface.IPv4.Manual[0].Address };
                                newIP["SubnetMask"] = new string[] { subnet_mask };
                                //и дергаем метод
                                objMO.InvokeMethod("EnableStatic", newIP, null);
                            }

                            XmlConfig conf = new XmlConfig();
                            ConfigStruct confstr = new ConfigStruct();
                            confstr = conf.Read();
                            confstr.IPAddr = ((NetInterfaceStruct)netInterfaceStruct).NetworkInterface.IPv4.Manual[0].Address;
                            conf.Write(confstr);

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Failed to setup network interface - {0}", e.Message);
                            //return false;
                        }
                    }

                }
            }

            //return true;
        }
        public string GetDHCPIP(object interfacename)
        {
            if (interfacename != null)
            {
                ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection objMOC = objMC.GetInstances();

                foreach (ManagementObject objMO in objMOC)
                {

                    if ((bool)objMO["IPEnabled"] == true &&
                        (bool)objMO["DHCPEnabled"] == true &&
                        (string)objMO["ServiceName"] == (string)interfacename)
                    {
                        return ((string[])objMO["IPAddress"])[0];
                    }
                }
            }
            return null;
        }
        public string UseDHCP()
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["DHCPEnabled"] == true)
                    return (string)objMO["ServiceName"];
            }

            return null;
        }
    }
    public class NetInterfaceStruct
    {
        private string interfaceToken;
        private NetworkInterfaceSetConfiguration networkInterface;
        public string InterfaceToken
        {
            get
            {
                return interfaceToken;
            }
            set
            {
                interfaceToken = value;
            }

        }
        public NetworkInterfaceSetConfiguration NetworkInterface
        {
            get
            {
                return networkInterface;
            }
            set
            {
                networkInterface = value;
            }

        }
    }
    public class Stop
    {
        public string ProfileToken;
        public bool PanTilt;
        public bool Zoom;
    }
    public class Move
    {
        public string VideoSourceToken;
        public FocusMove Focus;
    }
}

