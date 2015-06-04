using System;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Net;
using System.Timers;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;

using log4net;

using Device;
using Media;


namespace OnvifProxy
{
    public delegate void TyphoonDisconnectEventHandler(object sender, EventArgs ev);
    
    public static class TyphoonCom 
    {
        public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static event TyphoonDisconnectEventHandler TyphoonDisconnect;

        //очередь запросов к тайфуну
        private static Queue queueRequest = null;
        //очередь ответов от тайфуна (кодировка cp1251)
        public static Collection<TyphoonMessage> queueResponce = null;
        //очередь команд от тайфуна
        public static Queue queueCmd = null;


        private static TcpClient client = null;
        private static LingerOption lingerOpt = null;
        private static NetworkStream stream = null;
        //тред, в котором крутится обработка очереди команд
        //static Thread thr_process = null;
        //тред для парсера очереди команд от тайфуна - queueCmd
        private static Thread thr_parseQueueCmd = null;

        static AutoResetEvent ev_CommandParseQuit;
        static AutoResetEvent ev_ParseOutBufEnded;

        public static ManualResetEvent ev_TyphComStarted;
        public static AutoResetEvent ev_TyphComStoped;
        private static AutoResetEvent ev_TyphComConstructorPassed;

        //таймер таймаута коннекта к тайфуну
        private static System.Timers.Timer tmr_ConnectionTimeout;
        //таймер посылки зонда тайфуну
        private static System.Timers.Timer tmr_SendZond; 

        //true - если очередь комманд крутится постоянно, иначе false
        public static bool flg_SendToNet;

        //true - если клиент подцеплен к тайфуну
        private static bool flg_Connected = false;

        private static uint intPacketPtr;
        private static uint intCommandPtr = 0;

        private static byte[] commandBuffer = null;
        //ipaddr - на каком адресе расположен Тайфун
        private static string ipaddr = null;
        //флажок, указывающий, что отработка действий при OnConnectionFailed началась
        private static bool flg_ConnectionFailedActive = false;    
        //флаг, указывающий, что как минимум один раз коннекшн падал
        private static bool flg_OnConnectionFailed = false;
        private static Object obj;

        private static TyphoonMessage tmpTyphMsg;
        //private static UTF8Encoding utf8;
        
        private static byte[] Data;
        
        //счетчик MessageID
        private static UInt32 msgIDCounter;
        private static WSHttpBinding binding;
        //private static BasicHttpBinding binding;
        private static NVTServiceClient nvtClient;
        private static System.Collections.ObjectModel.Collection<NVTServiceClient> nvtClientCollection = null;

        
        

        public static void TyphoonComInit (object ip)
        {

            ipaddr = (string)ip;

            flg_ConnectionFailedActive = false;

            if(queueRequest == null)queueRequest = new Queue();
            if(queueResponce == null)queueResponce = new Collection<TyphoonMessage>();
            if(queueCmd == null)queueCmd = new Queue();

            if(nvtClientCollection ==null)nvtClientCollection = new Collection<NVTServiceClient>();

            commandBuffer = new byte[10240];

            //Program.ev_RebootHost.Reset();//???????????????????????????????????нахуй он тут?

            //зарегестрировать только один обработчик на событие TyphoonDisconnect
            if (!flg_OnConnectionFailed)
            {
                TyphoonCom.TyphoonDisconnect += OnConnectionFailed;
                flg_OnConnectionFailed = true;
            }

            if (tmr_ConnectionTimeout == null)
            {
                tmr_ConnectionTimeout = new System.Timers.Timer(10000);//конфигурируем таймер таймаута коннекта к тайфуну
                tmr_ConnectionTimeout.Elapsed += new ElapsedEventHandler(OnConnectionTimeoutEvent);
                tmr_ConnectionTimeout.Enabled = true;
                tmr_ConnectionTimeout.Stop();
            }
            
            if (tmr_SendZond == null)
            {
                tmr_SendZond = new System.Timers.Timer(2000);//конфигурируем таймер отправки зонда тайфуну
                tmr_SendZond.Elapsed += new ElapsedEventHandler(Send_Zond);
                tmr_SendZond.Enabled = true;
                tmr_SendZond.Stop();
            }
            
            if (ev_CommandParseQuit == null) ev_CommandParseQuit = new AutoResetEvent(false);
            if (ev_ParseOutBufEnded == null) ev_ParseOutBufEnded = new AutoResetEvent(false);
            if (ev_TyphComStarted == null) ev_TyphComStarted = new ManualResetEvent(false);
            if (ev_TyphComStoped == null) ev_TyphComStoped = new AutoResetEvent(false);
            if (ev_TyphComConstructorPassed == null) ev_TyphComConstructorPassed = new AutoResetEvent(false);

            if(obj == null) obj = new Object();

            flg_SendToNet = true;

            intPacketPtr = 0;
            ev_TyphComStoped.Reset();
            log.Debug("Connecting Typhoon...\n");

            Thread thr_Connect = new Thread(new ThreadStart(Connect));
            thr_Connect.IsBackground = true;
            thr_Connect.Start();
            //Connect();
            log.Debug("done?");
            
        }

        static void OnConnectionFailed(object sender, EventArgs e)
        {
            flg_ConnectionFailedActive = true;
            tmr_ConnectionTimeout.Stop();
            tmr_SendZond.Stop();
            ev_TyphComStoped.Set();

            Console.ForegroundColor = ConsoleColor.DarkRed;
            log.Debug("Connection Failed");
            Console.ResetColor();

            if (client != null)
            {
                client.Close();
                client = null;
            }
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
            }
            lingerOpt = null;           

            msgIDCounter = 1;
            TyphoonComStop();

            //зачистим все очереди
            while (queueRequest.Count != 0) queueRequest.Dequeue();
            while (queueResponce.Count != 0) queueResponce.Clear();
            while (queueCmd.Count != 0) queueCmd.Dequeue();

            Thread.Sleep(1000);        
            
            //проверить, что все закрыто
            //????????????
            TyphoonComInit(ipaddr);
        }


        static void OnTyphoonDisconnect()
        {
            TyphoonDisconnectEventHandler temp = TyphoonDisconnect;
            if (temp != null)
                temp(typeof(TyphoonCom), new EventArgs());
        }


        public static void TyphoonComStop()
        {
            nvtClientCollection = null;
            commandBuffer = null;
        }


        private static void OnConnectionTimeoutEvent(object source, ElapsedEventArgs e)
        {
            OnConnectionFailed(source, e);
        }


        private static void Connect()
        {
            try
            {
                client = new TcpClient();
                lingerOpt = new LingerOption(true, 0);
                client.LingerState = lingerOpt;
                client.Connect(ipaddr, 7520);

                stream = client.GetStream();

                flg_Connected = true;
                //----------------------------
                tmr_ConnectionTimeout.Enabled = true; //включаем таймер таймаута коннекта к тайфуну
                tmr_ConnectionTimeout.Stop();
                tmr_ConnectionTimeout.Start();

                ev_TyphComConstructorPassed.Set();

                ev_TyphComStarted.Set(); //отсылаем событие "Тайфун приконнекчен" ??
                Thread.Sleep(1);
                tmr_SendZond.Start();
                Send_HelloTyphoon();
                log.Debug("Done.\n");
                Process();
            }
            catch (ArgumentNullException ane)
            {
                log.DebugFormat("Connect() - client.Connect ArgumentNullException {0}", ane.Message);
            }
            catch (ArgumentOutOfRangeException aor)
            {
                log.DebugFormat("Connect() - client.Connect ArgumentOutOfRangeException {0}", aor.Message);
            }
            catch (ArgumentException ex)
            {
                log.DebugFormat("Connect() - ArgumentException {0}", ex.Message);
                if (!flg_ConnectionFailedActive) OnTyphoonDisconnect();
            }
            catch (SocketException es)
            {
                log.DebugFormat("Connect() - SocketException {0}", es.Message);
                if (!flg_ConnectionFailedActive) OnTyphoonDisconnect();
            }
            catch (ObjectDisposedException ed)
            {
                log.DebugFormat("Connect() - ObjectDisposedException {0}", ed.Message);
                if (!flg_ConnectionFailedActive) OnTyphoonDisconnect();
            }
            catch (InvalidOperationException ei)
            {
                log.DebugFormat("Connect() - client.GetStream() - InvalidOperationException {0}", ei.Message);
                if (!flg_ConnectionFailedActive) OnTyphoonDisconnect();
            }
        }


        private static void Process()
        {
            do
            {
                Thread.Sleep(1);
                try
                {
                    //----------------------------
                    //чтение данных из потока
                    //----------------------------
                    while (stream.DataAvailable)
                    {
                        try
                        {
                            stream.BeginRead(commandBuffer,
                                0,
                                commandBuffer.Length,
                                PacketParse,
                                stream);
                        }
                        catch (Exception ex)//The buffer parameter is null.
                        {
                            log.DebugFormat("stream.BeginRead - Exception {0}", ex.Message);
                            if (!flg_ConnectionFailedActive) OnTyphoonDisconnect();
                            break;
                        }
                    }
                }
                catch (Exception ex)//The buffer parameter is null.
                {
                    log.DebugFormat("stream.DataAvailable - Exception {0}", ex.Message);
                    if (!flg_ConnectionFailedActive) OnTyphoonDisconnect();
                    break;
                }
                //----------------------------
                //отсылка команд для тайфуна
                //----------------------------
                lock (obj)
                {
                    if (queueRequest.Count > 0)
                    {
                        try
                        {
                            stream.Write((byte[])(queueRequest.Peek()),
                                0,
                                ((byte[])(queueRequest.Peek())).Length);

                            queueRequest.Dequeue();
                        }
                        catch (Exception ex)
                        {
                            log.DebugFormat("stream.Write - Exception {0}", ex.Message);
                            if (!flg_ConnectionFailedActive) OnTyphoonDisconnect();
                            break;
                        }
                    }
                }
            }
            while (flg_SendToNet);
        }


        #region



        //---------------------------------------------------------
        //колбек для таймера добавляющего зонд в очередь команд
        //---------------------------------------------------------
        private static void Send_Zond(object source, ElapsedEventArgs e)
        {
            if (TyphoonCom.flg_Connected)
            {
                queueRequest.Enqueue(FormPacket(null));
            }
        }


        //---------------------------------------------------------
        //при коннекте к тайфуну кидает ему hello и собственны  IP, чтобы тайфун
        //знал, что это onvifproxy прицепился
        //---------------------------------------------------------
        private static void Send_HelloTyphoon()
        {
            byte[] b_ip = null;
            byte[] b_ip_ascii;
            byte[] tmpData;

            XmlConfig conf = new XmlConfig();
            conf = new XmlConfig();
            ConfigStruct confstr = new ConfigStruct();
            confstr = new ConfigStruct();
            confstr = conf.Read();

            if (TyphoonCom.flg_Connected)
            {
                try
                {
                    b_ip = Encoding.Unicode.GetBytes(confstr.IPAddr);
                }
                catch (Exception e)
                {
                    log.ErrorFormat("Failed to send HelloTyphoon", e.Message);
                }

                b_ip_ascii = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, b_ip);                
                tmpData = TyphoonCom.MakeMem(Encoding.ASCII.GetString(b_ip_ascii));
               
                try
                {
                    queueRequest.Enqueue(FormPacket(FormCommand(200, 3, tmpData, 0)));
                }
                catch (Exception e)
                {
                    log.ErrorFormat("Send_HelloTyphoon - {0}", e.Message);
                    throw new Exception("Send_HelloTyphoon failed to send") ;
                }
            }
        }



        //---------------------------------------------------------
        //добавление сформированного пакета команды в очередь для 
        //отправки тайфуну
        //---------------------------------------------------------
        public static bool AddCommand(byte[] CommandPacket)
        {
            object obj = new object();
            if ((CommandPacket != null) && (queueRequest != null))
            {
                lock (obj)
                {
                    queueRequest.Enqueue(CommandPacket);
                    //log.Debug("TyphoonCom.AddCommand commandQueue.Enqueued");
                    //ev_ParseOutBufEnded.WaitOne();
                    ev_ParseOutBufEnded.WaitOne(500);
                    //log.Debug("TyphoonCom.AddCommand passed");
                }
                return true;
            }
            else
            {
                //Console.WriteLine("AddCommand argument null");
                log.Debug("TyphoonCom:AddCommand argument null");
            }
            return false;
        }



        //---------------------------------------------------------
        //колбек, разбирающий, что пришло на запрос, соответствие CRC
        //отделяет данные от зондов и взводит флаг flagConnected
        //---------------------------------------------------------
        private static void PacketParse(IAsyncResult ar)
        {            
            PacketParseEx(ar);
            ev_CommandParseQuit.Set();
        }


        //---------------------------------------------------------
        //разбирает команды, пришедшие в пакете
        //---------------------------------------------------------
        private static void CommandParse(byte[] CommandBuff)
        {
            Encoding cp1251 = Encoding.GetEncoding(1251);
            byte[] tmpByteAr = new byte[4];
            uint dataBlockLen = 0;


            if (CommandBuff != null)
            {
                try
                {

                    switch (CommandBuff[intCommandPtr])//здесь эксепшен аут оф рэндж
                    {
                        case 0:
                            intCommandPtr += 2;
                            break;
                        case 200:
                            ///если номер команды 200 - это ответ на запросы от сервиса,
                            ///поместим данные в очередь ответов на запросы сервиса
                            tmpTyphMsg = new TyphoonMessage();

                            ///вынимаем номер субкоманды
                            for (int a = 0; a < 4; a++)
                            {
                                tmpByteAr[a] = CommandBuff[2 + a + intCommandPtr];
                            }
                            //и кладем в TyphMsg.MessageSubComNum
                            tmpTyphMsg.MessageSubComNum = ByteArtoInt32(tmpByteAr);

                            //вынимаем ID команды 
                            for (int a = 0; a < 4; a++)
                            {
                                tmpByteAr[a] = CommandBuff[6 + a + intCommandPtr];
                            }
                            //и кладем в TyphMsg.MessageID
                            tmpTyphMsg.MessageID = ByteArtoInt32(tmpByteAr);


                            //вынимаем длину команды
                            for (int a = 0; a < 4; a++)
                            {
                                tmpByteAr[a] = CommandBuff[10 + a + intCommandPtr];
                            }
                            //и кладем в dataBlockLen
                            dataBlockLen = ByteArtoInt32(tmpByteAr);
                            Data = new byte[dataBlockLen];
                            for (int a = 0; a < dataBlockLen; a++)
                            {
                                Data[a] = CommandBuff[14 + a + intCommandPtr];
                            }
                            intCommandPtr += (dataBlockLen + 13);

                            tmpTyphMsg.MessageData += cp1251.GetString(Data);

                            queueResponce.Add(tmpTyphMsg);

                            break;
                        case 201:
                            ///если 201 - запрос от тайфуна на действие
                            ///поместим данные в очередь запросов от тайфуна
                            tmpTyphMsg = new TyphoonMessage();

                            ///вынимаем номер субкоманды
                            ///по нему мы будем определять, чего хочет тайфун
                            for (int a = 0; a < 4; a++)
                            {
                                tmpByteAr[a] = CommandBuff[2 + a + intCommandPtr];
                            }

                            //и кладем в TyphMsg.MessageSubComNum
                            tmpTyphMsg.MessageSubComNum = ByteArtoInt32(tmpByteAr);

                            //вынимаем ID команды 
                            for (int a = 0; a < 4; a++)
                            {
                                tmpByteAr[a] = CommandBuff[6 + a + intCommandPtr];
                            }

                            //и кладем в TyphMsg.MessageID
                            tmpTyphMsg.MessageID = ByteArtoInt32(tmpByteAr);

                            //вынимаем длину набора команд
                            for (int a = 0; a < 4; a++)
                            {
                                tmpByteAr[a] = CommandBuff[10 + a + intCommandPtr];
                            }

                            //и кладем в dataBlockLen
                            dataBlockLen = ByteArtoInt32(tmpByteAr);

                            ////вынимаем длину команды
                            //for (int a = 0; a < 4; a++)
                            //{
                            //    tmpByteAr[a] = CommandBuff[14 + a + intCommandPtr];
                            //}
                            //commandBlockLen = ByteArtoInt32(tmpByteAr);

                            Data = new byte[dataBlockLen];
                            for (int a = 0; a < dataBlockLen; a++)
                            {
                                Data[a] = CommandBuff[14 + a + intCommandPtr];
                            }

                            ///сдвигаем указатель внутри блока данных
                            intCommandPtr += (dataBlockLen + 13);

                            tmpTyphMsg.MessageData += cp1251.GetString(Data);
                            queueCmd.Enqueue(tmpTyphMsg);
                            //--------------------------------------------------------------------
                            //Console.WriteLine("queueCmd.Count - {0}", queueCmd.Count);
                            //--------------------------------------------------------------------

                            //здесь пнуть разборку очереди команд
                            thr_parseQueueCmd = new Thread(new ThreadStart(ParseQueueCmd));
                            thr_parseQueueCmd.IsBackground = true;
                            thr_parseQueueCmd.Start();

                            break;
                        default:
                            ///если ни то ни другое - никуда эти данные не кладем
                            ///и идем дальше
                            log.InfoFormat("От Тайфуна пришли данные с несоответствующим номером команды , данные отброшены, как неклассифицированные");
                            //
                            byte[] tmpAr = new byte[4];
                            for (int a = 0; a < 4; a++)
                            {
                                tmpAr[a] = CommandBuff[10 + a + intCommandPtr];
                            }
                            //и кладем в dataBlockLen
                            uint dataLen = ByteArtoInt32(tmpAr);
                            intCommandPtr += dataLen;
                            break;
                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat(e.Message);
                }
            }
            else
            {
                log.Error("CommandParse - на вход пришел ноль");
            }
            ///если не дошли до конца - продолжаем
            if (intCommandPtr < CommandBuff.Length-1)
            {
                TyphoonCom.log.Debug("слипшиеся команды");
                CommandParse(CommandBuff);
            }
            else
            {
                intCommandPtr = 0;
            }
        }


        //разбор очереди команд, пришедших от тайфуна, вертится в отдельном потоке
        private static void ParseQueueCmd()
        {
            TyphoonMessage typhMsg = new TyphoonMessage();
            Object obj = new object();
            lock (obj)
            {
                if (queueCmd.Count != 0)
                {
                    typhMsg = (TyphoonMessage)queueCmd.Peek();
                    //выкинем обработанную команду из очереди
                    queueCmd.Dequeue();
                }
                else
                {
                    typhMsg.MessageSubComNum = 0;
                }
            }
            
            binding = new WSHttpBinding(SecurityMode.None);
            //binding = new BasicHttpBinding();


            switch (typhMsg.MessageSubComNum)
            { 
                case 1:
                    #region
                    ///пнуть UdpDiscoClient
                    new NVTDiscoClient(typhMsg.MessageID);
                    break;
                #endregion
                case 2:
                    #region
                    ///дернуть GetDeviceInformation()
                    ///у NVTClient
                    Console.WriteLine("SubCom - 2 - GetDeviceInformation");
                    typhMsg.MessageData = typhMsg.MessageData.Remove(0,4);
                    
                    try
                    {
                        nvtClient = TyphoonCom.nvtClientCollection.Single(NVTServiceClient => NVTServiceClient.Endpoint.ListenUri == (new Uri(typhMsg.MessageData)));
                    }
                    catch (Exception ex)
                    {
                        nvtClient = new NVTServiceClient(binding, (new EndpointAddress(typhMsg.MessageData)));
                        if(nvtClient!=null)
                        {
                            nvtClientCollection.Add(nvtClient);
                        }
                        else
                        {
                            Console.WriteLine("Не удалось создать NVTServiceClient");
                            break;
                        }                       
                    }

                    string model, firmware, serial, hardwareid, manufacturer;
                    byte[] b_model, b_firmware, b_serial, b_hardwareid, b_manufacturer, b_totalData;
                    int ptr = 0;
                    try
                    {
                        manufacturer = nvtClient.GetDeviceInformation(out model, out firmware, out serial, out hardwareid);
                    }
                    catch (FaultException ex)
                    {
                        log.ErrorFormat("failed to GetDeviceInformation from {0}", typhMsg.MessageData);
                        break;
                    }
                    b_model = MakeMem(model);
                    b_firmware = MakeMem(firmware);
                    b_serial = MakeMem(serial);
                    b_hardwareid = MakeMem(hardwareid);
                    b_manufacturer = MakeMem(manufacturer);

                    b_totalData = new byte[(b_model.Length + b_firmware.Length + b_serial.Length + b_hardwareid.Length + b_manufacturer.Length)];
                    for (int a = 0; a < b_manufacturer.Length; a++)
                    {
                        b_totalData[a + ptr] = b_manufacturer[a];
                    }
                    ptr += b_manufacturer.Length;

                    for (int a = 0; a < b_model.Length; a++)
                    {
                        b_totalData[a + ptr] = b_model[a];
                    }
                    ptr += b_model.Length;
                    for (int a = 0; a < b_firmware.Length; a++)
                    {
                        b_totalData[a + ptr] = b_firmware[a];
                    }
                    ptr += b_firmware.Length;
                    for (int a = 0; a < b_serial.Length; a++)
                    {
                        b_totalData[a + ptr] = b_serial[a];
                    }
                    ptr += b_serial.Length;
                    for (int a = 0; a < b_hardwareid.Length; a++)
                    {
                        b_totalData[a + ptr] = b_hardwareid[a];
                    }
                    ptr += b_hardwareid.Length;

                    //потом отдать тайфуну
                    AddCommand(FormPacket(FormCommand(201, 2, b_totalData, typhMsg.MessageID)));                    

                    break;
                    #endregion

                case 3:
                    #region
                    ///дернуть GetMediaCapabilities()
                    ///у NVTClient        
                    //---------------
                    Console.WriteLine("SubCom - 3 - GetMediaCapabilities");
                    typhMsg.MessageData = typhMsg.MessageData.Remove(0, 4);
                    ptr = 0;
                    string xaddr, rtpmulticast, rtp_tcp, rtp_rtsp_tcp;
                    byte[] b_xaddr, b_rtpmulticast, b_rtp_tcp, b_rtp_rtsp_tcp;

                    try
                    {
                        nvtClient = TyphoonCom.nvtClientCollection.Single(NVTServiceClient => NVTServiceClient.Endpoint.ListenUri == (new Uri(typhMsg.MessageData)));
                    }
                    catch (Exception ex)
                    {
                        nvtClient = new NVTServiceClient(binding, (new EndpointAddress(typhMsg.MessageData)));
                        if (nvtClient != null)
                        {
                            nvtClientCollection.Add(nvtClient);
                        }
                        else
                        {
                            Console.WriteLine("Не удалось создать NVTServiceClient");
                            break;
                        }
                    }
                    GetCapabilitiesResponse nvtCapabilitiesResponse;
                    try
                    {
                        nvtCapabilitiesResponse = nvtClient.GetCapabilities(new GetCapabilitiesRequest());
                    }
                    catch (FaultException ex)
                    {
                        log.ErrorFormat("failed to GetCapabilities from {0}", typhMsg.MessageData);
                        break;
                    }


                    xaddr = nvtCapabilitiesResponse.Capabilities.Media.XAddr;
                    rtpmulticast = nvtCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTPMulticast.ToString();
                    rtp_tcp = nvtCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_TCP.ToString();
                    rtp_rtsp_tcp = nvtCapabilitiesResponse.Capabilities.Media.StreamingCapabilities.RTP_RTSP_TCP.ToString();

                    b_xaddr = MakeMem(xaddr);
                    b_rtpmulticast = MakeMem(rtpmulticast);
                    b_rtp_tcp = MakeMem(rtp_tcp);
                    b_rtp_rtsp_tcp = MakeMem(rtp_rtsp_tcp);

                    b_totalData = new byte[(b_xaddr.Length + b_rtpmulticast.Length + b_rtp_tcp.Length + b_rtp_rtsp_tcp.Length)];

                    for (int a = 0; a < b_xaddr.Length; a++)
                    {
                        b_totalData[a + ptr] = b_xaddr[a];
                    }
                    ptr += b_xaddr.Length;

                    for (int a = 0; a < b_rtpmulticast.Length; a++)
                    {
                        b_totalData[a + ptr] = b_rtpmulticast[a];
                    }
                    ptr += b_rtpmulticast.Length;

                    for (int a = 0; a < b_rtp_tcp.Length; a++)
                    {
                        b_totalData[a + ptr] = b_rtp_tcp[a];
                    }
                    ptr += b_rtp_tcp.Length;

                    for (int a = 0; a < b_rtp_rtsp_tcp.Length; a++)
                    {
                        b_totalData[a + ptr] = b_rtp_rtsp_tcp[a];
                    }
                    ptr += b_rtp_rtsp_tcp.Length;
                    //потом отдать тайфуну
                    AddCommand(FormPacket(FormCommand(201, 3, b_totalData, typhMsg.MessageID)));
                    
                    break;
                    #endregion

                case 4:
                    #region
                    ///дернуть GetMediaProfiles()
                    ///у NVTClient
                    Console.WriteLine("SubCom - 4 - GetMediaProfiles");
                    typhMsg.MessageData = typhMsg.MessageData.Remove(0, 4);
                    try
                    {
                        nvtClient = TyphoonCom.nvtClientCollection.Single(NVTServiceClient => NVTServiceClient.Endpoint.ListenUri == (new Uri(typhMsg.MessageData)));
                    }
                    catch (Exception ex)
                    {
                        nvtClient = new NVTServiceClient(binding, (new EndpointAddress(typhMsg.MessageData)));
                        if (nvtClient != null)
                        {
                            nvtClientCollection.Add(nvtClient);
                        }
                        else
                        {
                            Console.WriteLine("Не удалось создать NVTServiceClient");
                            break;
                        }
                    }

                    GetProfilesResponse nvtProfilesResponse;
                    try
                    {
                        nvtProfilesResponse = nvtClient.GetProfiles(new GetProfilesRequest());
                    }
                    catch (FaultException ex)
                    {
                        log.ErrorFormat("failed to GetProfiles from {0}", typhMsg.MessageData);
                        break;
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(GetProfilesResponse));

                        try
                        {
                            serializer.Serialize(ms, nvtProfilesResponse);
                        }
                        catch (SerializationException e)
                        {
                            Console.WriteLine("Не могу сериализовать nvtProfilesResponse " + e.Message);
                        }
                        catch
                        {
                            Console.WriteLine("Не могу сериализовать nvtProfilesResponse ");
                        }
                        byte[] str = ms.ToArray();

                        Encoding unicode = Encoding.Unicode;
                        Encoding cp1251 = Encoding.GetEncoding(1251);
                        string OutStr = unicode.GetString(Encoding.Convert(cp1251, unicode, str));
                        int t = OutStr.IndexOf(">");
                        OutStr = OutStr.Insert(t+1, "<Envelope><Body>");
                        OutStr += "</Body></Envelope>";
                        byte[] OutAr = MakeMem(OutStr);
                        //потом отдать тайфуну
                        AddCommand(FormPacket(FormCommand(201, 4, OutAr, typhMsg.MessageID)));
                    }

                    break;
                    #endregion

                case 5:
                    #region
                    ///дернуть SetVideoEncoderConfiguration и StreamUri()
                    ///у NVTClient
                    Console.WriteLine("SubCom - 5 - GetStreamUri");
                    byte[] b_streamUri = null;
                    StreamSetup streamSetup = new StreamSetup();
                    
                    int tmpptr = 0;
                    string xaddrs = ParseMem(tmpptr, typhMsg.MessageData);
                    tmpptr += xaddrs.Length+4;
                    string stream = ParseMem(tmpptr, typhMsg.MessageData);
                    tmpptr += stream.Length + 4;
                    string protocol = ParseMem(tmpptr, typhMsg.MessageData);
                    tmpptr += protocol.Length + 4;
                    string profileToken = ParseMem(tmpptr, typhMsg.MessageData);
                    tmpptr += profileToken.Length + 4;

                    try
                    {
                        nvtClient = TyphoonCom.nvtClientCollection.Single(NVTServiceClient => NVTServiceClient.Endpoint.ListenUri == (new Uri(xaddrs)));
                    }
                    catch (Exception ex)
                    {
                        nvtClient = new NVTServiceClient(binding, (new EndpointAddress(xaddrs)));
                        if (nvtClient != null)
                        {
                            nvtClientCollection.Add(nvtClient);
                        }
                        else
                        {
                            Console.WriteLine("Не удалось создать NVTServiceClient");
                            break;
                        }
                    }

                    
                    switch (stream)
                    { 
                        case "RTP-Multicast":
                            streamSetup.Stream = StreamType.RTPMulticast;
                            break;
                        case "RTP-Unicast":
                            streamSetup.Stream = StreamType.RTPUnicast;
                            break;
                        default:
                            log.InfoFormat("GetStreamUri для streamSetup пришло {0}", stream);
                            break;
                    }
                    streamSetup.Transport = new Transport();
                    switch (protocol)
                    {
                        case "HTTP":
                            streamSetup.Transport.Protocol = TransportProtocol.HTTP;
                            break;
                        case "RTSP":
                            streamSetup.Transport.Protocol = TransportProtocol.RTSP;
                            break;
                        case "TCP":
                            streamSetup.Transport.Protocol = TransportProtocol.TCP;
                            break;
                        case "UDP":
                            streamSetup.Transport.Protocol = TransportProtocol.UDP;
                            break;
                        default:
                            log.InfoFormat("GetStreamUri для streamSetup.Transport.Protocol пришло {0}", protocol);
                            break;
                    }
                    
                    //запросить uri соответсующую конфигурации
                    MediaUri nvtStreamUri = nvtClient.GetStreamUri(streamSetup, profileToken);
                    //потом отдать тайфуну
                    b_streamUri = MakeMem(nvtStreamUri.Uri);
                    AddCommand(FormPacket(FormCommand(201, 5, b_streamUri, typhMsg.MessageID)));
                    break;
                    #endregion

                case 6:
                    #region
                    //пришло событие от тайфуна
                    Console.WriteLine("SubCom - 6 - Event came from Typhoon");
                    
                    byte[] bytes = Encoding.UTF8.GetBytes(typhMsg.MessageData.ToCharArray());

                    byte[] bytes1= {0,0,0,0};

                    EventData eventdata = new EventData();

                    for (int y = 0; y < 4; y++)
                    {
                        bytes1[y] = bytes[y];
                    }
                    eventdata.Eventtype = ByteArtoInt32(bytes1);
                    //

                    for (int y = 4; y < 8; y++)
                    {
                        bytes1[y - 4] = bytes[y];
                    }
                    eventdata.Devicenumber = ByteArtoInt32(bytes1);
                    //

                    for (int y = 8; y < 12; y++)
                    {
                        bytes1[y - 8] = bytes[y];
                    }

                    uint alst  = ByteArtoInt32(bytes1);
                    if (alst != 0)
                    {
                        eventdata.Status = Alarmstatus.AlarmOff;
                    }
                    else
                    {
                        eventdata.Status = Alarmstatus.AlarmOn;
                    }
                    //------------------------------------------
                    //здесь добавим в EventStorage
                    TyphoonEvent tevent = new TyphoonEvent(eventdata, 60000);
                    EventStorage.AddEvent(tevent);
                    //------------------------------------------
                    //
                    object objj  = new object();
                    lock (bnSubscriptionManager.SubscribersCollection)
                    {
                        try
                        {
                            foreach (bnSubscriber subscriber in bnSubscriptionManager.SubscribersCollection.Values)
                            {
                                bnSubscriptionManager.SubscribersCollection.Remove(bnSubscriptionManager.SubscribersCollection.First().Key);
                                #region
                                if (subscriber.Eventtype == eventdata.Eventtype)
                                {
                                    Event.Notify1 notify = new Event.Notify1(new Event.Notify());
                                    notify.Notify.NotificationMessage = new Event.NotificationMessageHolderType[1];
                                    notify.Notify.NotificationMessage[0] = new Event.NotificationMessageHolderType();
                                    XmlDocument doc = new XmlDocument();
                                    doc.LoadXml("<Notify><NotificationMessage xmlns = 'http://docs.oasis-open.org/wsn/b-2' >"
                                    + "<Topic  Dialect='http://docs.oasis-open.org/wsn/t-1/TopicExpression/ConcreteTopic'>"
                                        //+ "tns1:VideoSource/MotionAlarm</Topic>"
                                    + "tns1:VideoSource</Topic>"
                                    + "<Message><tt:Message UtcTime='" + (System.DateTime.UtcNow).ToString("s") + "' "
                                    + "PropertyOperation='Initialized' xmlns:tt='http://www.onvif.org/ver10/schema'><tt:Source>"
                                    + "<tt:SimpleItem Name='app' Value='changed' /></tt:Source><tt:Key>"
                                    + "<tt:SimpleItem Name='channel' Value='0' /></tt:Key><tt:Data><tt:SimpleItem Name='tampering' Value='0' />"
                                    + "</tt:Data></tt:Message></Message>"
                                    + "</NotificationMessage></Notify>"
                                    );
                               
                                    string strs = "<?xml version='1.0' encoding='utf-8' ?>" + doc.InnerXml.ToString();
                                    using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(strs)))
                                    {
                                        XmlSerializer serializer = new XmlSerializer(typeof(Event.Notify));

                                        try
                                        {
                                            notify.Notify = (Event.Notify)serializer.Deserialize(ms);  //                                          
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("TyphoonCom: Event: Не могу сериализовать " + ex.Message);
                                        }

                                    }
                                    //--------------------------------------
                                #endregion
                                    try
                                    {
                                        subscriber.channel.Notify(notify);
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Warn(ex.Message.ToString());
                                    }
                                    //break;
                                }
                    #endregion
                            }
                        }
                        catch (InvalidOperationException ioe)
                        {
                            //try to form notify again
                            Console.WriteLine("пыщыпщпыщ");
                        }
                        catch (Exception ex)
                        {
                            log.Warn(ex.Message.ToString());
                        }
                        
                    }
                    break;
                    #endregion

                default:
                    ///сообщить о неизвестном номере субкоманды
                    log.ErrorFormat("В очередь команд от тайфуна (queueCmd) пришла команда с неизвестным номером субкоманды - {0}", typhMsg.MessageSubComNum);
                    break;
            }            
        }

        private static Event.Notify1 FormNotify(EventData eventdata, bnSubscriber subscriber)
        {
            if (eventdata != null && subscriber != null)
            {
                Event.Notify1 notify = new Event.Notify1(new Event.Notify());
                if (subscriber.Eventtype == eventdata.Eventtype)
                {                    
                    notify.Notify.NotificationMessage = new Event.NotificationMessageHolderType[1];
                    notify.Notify.NotificationMessage[0] = new Event.NotificationMessageHolderType();

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml("<Notify><NotificationMessage xmlns = 'http://docs.oasis-open.org/wsn/b-2' >"
                    + "<Topic  Dialect='http://docs.oasis-open.org/wsn/t-1/TopicExpression/ConcreteTopic'>"
                    + "tns1:VideoSource</Topic>"
                    + "<Message><tt:Message UtcTime='" + (System.DateTime.UtcNow).ToString("s") + "' "
                    + "PropertyOperation='Initialized' xmlns:tt='http://www.onvif.org/ver10/schema'><tt:Source>"
                    + "<tt:SimpleItem Name='app' Value='changed' /></tt:Source><tt:Key>"
                    + "<tt:SimpleItem Name='channel' Value='0' /></tt:Key><tt:Data><tt:SimpleItem Name='tampering' Value='0' />"
                    + "</tt:Data></tt:Message></Message>"
                    + "</NotificationMessage></Notify>"
                    );

                    string strs = "<?xml version='1.0' encoding='utf-8' ?>" + doc.InnerXml.ToString();
                    using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(strs)))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Event.Notify));

                        try
                        {
                            notify.Notify = (Event.Notify)serializer.Deserialize(ms);  //                                          
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("TyphoonCom: Event: Не могу сериализовать " + ex.Message);
                        }

                    }
                }

                return notify;
            }
            else return null;
        }

        private static void PacketParseEx(IAsyncResult ar)
        {
            uint datalength = 0;
            int packlen = 0;
            bool flg = false;


            try
            {
                packlen = stream.EndRead(ar);
                flg = true;
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine("ArgumentException - {0}",ae.Message);
                log.DebugFormat("ArgumentException - {0}", ae.Message);
            }
            catch (IOException ae)
            {
                Console.WriteLine("IOException - {0}", ae.Message);
                log.DebugFormat("IOException - {0}", ae.Message);
            }
            catch (ObjectDisposedException ae)
            {
                Console.WriteLine("ObjectDisposedException - {0}", ae.Message);
                log.DebugFormat("ObjectDisposedException - {0}", ae.Message);
            }

            if (flg)
            {
                //читаю первые 4 байта пакета - его размер
                //и складываю в datalength
                for (uint y = intPacketPtr; y < (4 + intPacketPtr); y++)
                {
                    datalength = (datalength << 8);
                    datalength += (uint)commandBuffer[3 - y + intPacketPtr];
                }

                //создаю массив на данные без 4х байт общей длины пакета
                //и без 1го байта crc чтобы посчитать crc8
                Byte[] tmpBuff = new byte[datalength - 5];

                //копирую туда данные из пакета
                for (uint t = 0; t < (datalength - 5); t++)
                {
                    tmpBuff[t] = commandBuffer[4 + t + intPacketPtr];
                }

                //считаю crc от того массива и сверяю с crc пришедшего пакета
                if (commandBuffer[datalength - 1 + intPacketPtr] == TyphoonCom.CRC8(tmpBuff))
                {
                    //crc8 сошелся - сбросим таймер таймаута
                    tmr_ConnectionTimeout.Stop();
                    tmr_ConnectionTimeout.Start();

                    //--------------------------------
                    ev_TyphComStarted.Set(); //отсылаем событие "Тайфун приконнекчен" ??
                    //--------------------------------


                    Byte[] pureData = new byte[tmpBuff.Length - 8];
                    for (int a = 0; a < pureData.Length; a++)
                    {
                        pureData[a] = tmpBuff[a + 8];
                    }
                    //анализируем пришедший набор команд
                    CommandParse(pureData);

                }
                intPacketPtr += datalength;
                //если есть еще пакеты в буфере - продолжаем обработку
                if (intPacketPtr < packlen)
                {
                    PacketParseEx(ar);
                }

                intPacketPtr = 0;
            }
        }
        


        //---------------------------------------------------------------------------------------
        //формирование команды для дальнейшего формирования пакета и отправки Тайфуну
        // на входе - номер команды и данные для неё, выход - массив байт для вставки в пакет
        //---------------------------------------------------------------------------------------
        public static byte[] FormCommand(ushort ComNum, int SubComNum, byte[] Data, uint MessageID)
        {
            byte[] Data_Command_Block;
            byte[] SubComNumAr;
            byte[] DataLengthAr;
            byte[] MsgIDAr;
            uint MsgID;

            if (Data != null)
            {
                //конвертим данные из юникода в cp1251
                Encoding unicode = Encoding.Unicode;
                Encoding cp1251 = Encoding.GetEncoding(1251);
                byte[] unicodeBytes = Data;
                byte[] ASCIIBytes = Encoding.Convert(unicode, cp1251, unicodeBytes);
                //создаем массив в котором будет лежать сформированная команда
                Data_Command_Block = new byte[(ASCIIBytes.Length + 14)];
                //вставляем значение кода субкоманды
                SubComNumAr = Int32toByteAr((uint)SubComNum);
                for (int y = 0; y < 4; y++)
                {
                    Data_Command_Block[2 + y] = SubComNumAr[y];
                }

                //если MessageID при вызове метода 0, используем в качестве MessageID внутренний счетчик 
                //(сделано для формирования ответов на команды тайфуна с MessageID запроса)
                if (MessageID == 0)
                {
                    if (msgIDCounter == uint.MaxValue || msgIDCounter == 0) msgIDCounter = 1;
                    MsgID = msgIDCounter;
                    msgIDCounter++;
                }
                // если не 0, то указанное значение
                else
                {
                    MsgID = MessageID;
                }

                //превращаем его в массив байт
                MsgIDAr = Int32toByteAr((uint)(MsgID));

                //и засовываем в массив с 6 по 9 байты
                for (int t = 0; t < 4; t++)
                {
                    Data_Command_Block[6 + t] = MsgIDAr[t];
                }
                //делаем из длины данных массив байт
                //DataLengthAr = Int32toByteAr((uint)(Data.Length));
                DataLengthAr = Int32toByteAr((uint)(ASCIIBytes.Length));
                //и засовываем в массив с 10 по 13 байты
                for (int t = 0; t < 4; t++)
                {
                    Data_Command_Block[10 + t] = DataLengthAr[t];
                }
                //засовываем в массив сами данные с 14 байта 
                //и до упора
                for (int a = 0; a < ASCIIBytes.Length; a++)
                {
                    Data_Command_Block[14 + a] = ASCIIBytes[a];
                }
            }
            else
            {
                //создаем массив в котором будет лежать сформированная команда
                Data_Command_Block = new byte[14];
                //вставляем значение кода субкоманды
                SubComNumAr = Int32toByteAr((uint)SubComNum);
                for (int y = 0; y < 4; y++)
                {
                    Data_Command_Block[2 + y] = SubComNumAr[y];
                }

                //генерим случайное значение для ID команды
                //MsgID = rnd.Next(System.Int32.MaxValue);
                if (msgIDCounter == uint.MaxValue||msgIDCounter == 0) msgIDCounter = 1;
                MsgID = msgIDCounter;
                msgIDCounter++;
                //превращаем его в массив байт
                MsgIDAr = Int32toByteAr((uint)(MsgID));

                //и засовываем в массив с 6 по 9 байты
                for (int t = 0; t < 4; t++)
                {
                    Data_Command_Block[6 + t] = MsgIDAr[t];
                }
                DataLengthAr = Int32toByteAr(0);
                for (int t = 0; t < 4; t++)
                {
                    Data_Command_Block[10 + t] = DataLengthAr[t];
                }
            }
            //пишем номер команды в 0-1 байты массива
            Data_Command_Block[0] = (byte)ComNum;
            Data_Command_Block[1] = (byte)(ComNum >> 8);

            return Data_Command_Block;
        }


        

        //---------------------------------------------------------------------------------------
        //формирование пакета содержащего команду для отправки его тайфуну
        //на входе массив данных сформированный в FormCommand, на выходе пакет
        //готовый для отправки тайфуну
        //---------------------------------------------------------------------------------------
        public static byte[] FormPacket(byte[] Data_Command_Block)
        {
            if (Data_Command_Block != null)
            {
                byte[] Packet = new byte[(13 + Data_Command_Block.Length)];

                byte[] tmp = new byte[4];
                tmp = Int32toByteAr((uint)Packet.Length);
                //Начинаем заполнять пакет 
                for (int b = 0; b < 4; b++)
                // заполняем общую длину пакета
                {
                    Packet[b] = tmp[b];
                }
                tmp = Int32toByteAr(0xa5a5a5a5);
                for (int a = 0; a < 4; a++)
                {
                    //заполняем маркер начала пакета
                    Packet[a + 4] = tmp[a];
                }

                for (int a = 0; a < Data_Command_Block.Length; a++)
                {
                    Packet[a + 12] = Data_Command_Block[a];
                }

                //выделяем кусок данных, подлежащих обсчету crc
                byte[] datatocrc = new byte[Packet.Length - 5];
                for (int a = 0; a < datatocrc.Length; a++)
                {
                    datatocrc[a] = Packet[a + 4];
                }
                //засовываем crc последним байтом в пакет
                Packet[Packet.Length - 1] = CRC8(datatocrc);
                return Packet;
            }
            else
            {
                byte[] Packet = new byte[15];
                byte[] tmp = new byte[4];

                tmp = Int32toByteAr((uint)Packet.Length);
                //Начинаем заполнять пакет 
                for (int b = 0; b < 4; b++)
                // заполняем общую длину пакета
                {
                    Packet[b] = tmp[b];
                }
                tmp = Int32toByteAr(0xa5a5a5a5);
                for (int a = 0; a < 4; a++)
                {
                    //заполняем маркер начала пакета
                    Packet[a + 4] = tmp[a];
                }

                byte[] datatocrc = new byte[Packet.Length - 5];
                for (int a = 0; a < datatocrc.Length; a++)
                {
                    datatocrc[a] = Packet[a + 4];
                }
                //засовываем crc последним байтом в пакет
                Packet[Packet.Length - 1] = CRC8(datatocrc);
                return Packet;
            }
        }



        //---------------------------------------------------------------------------------------
        //преобразование Int32 в массив из 4 byte
        //---------------------------------------------------------------------------------------
        public static byte[] Int32toByteAr(uint length)
        {
            byte[] ByteAr = new byte[4];
            for (int a = 0; a < 4; a++)
            {
                ByteAr[a] = (byte)(length >> a * 8);
            }
            return ByteAr;
        }



        //---------------------------------------------------------------------------------------
        //преобразование массива из 4 byte в Int32
        //---------------------------------------------------------------------------------------
        //преобразование обратное Int32toByteAr
        private static uint ByteArtoInt32(byte[] Arr)
        {
            if (Arr.Length != 4) return 0;
            uint bytes = 0;
            for (int a = 0; a < 4; a++)
            {
                bytes = bytes << 8;
                bytes += Arr[3-a];                
            }
            return bytes;
        }


        //---------------------------------------------------------------------------------------
        //подсчет контрольной суммы по CRC8
        //на входе массив по которому считается crc, на выходе байт crc
        //---------------------------------------------------------------------------------------
        private static byte CRC8(byte[] Data)
        {
            byte Checksum;

            Checksum = 0xc7;

            for (int a = 0; a < Data.Length; a++)
            {
                Checksum ^= Data[a];
                for (int b = 0; b < 8; b++)
                {
                    if ((Checksum & 0x80) != 0)
                    {
                        Checksum = (byte)((Checksum << 1) ^ 0x1d);
                    }
                    else
                    {
                        Checksum = (byte)(Checksum << 1);
                    }
                }
            }
            return Checksum;
        }


        //---------------------------------------------------------------------------------------
        //метод - хелпер, преобразующий строчку на входе в кусочек команды, в виде 
        //перевернутых четырех байт хранящих длину строчки и строчки данных
        //метод для создания массива данных для метода FormCommand
        //---------------------------------------------------------------------------------------
        public static byte[] MakeMem(string inputString)
        {
            byte[] b_inputString;
            uint len_inputString;

            Encoding unicode = Encoding.Unicode;
            Encoding cp1251 = Encoding.GetEncoding(1251);

            if (inputString == null)
            {
                return new byte[4];
            }

            byte[] unicodeBytes = unicode.GetBytes(inputString);
            b_inputString = Encoding.Convert(unicode, cp1251, unicodeBytes);
            //длина в формате cp1251
            len_inputString = (uint)b_inputString.Length;

            char[] hlpCharAr = cp1251.GetChars(TyphoonCom.Int32toByteAr(len_inputString));
            char[] char_inputString = { '0', '0', '0', '0' };
            for (int a = 0; a < hlpCharAr.Length; a++)
            {
                char_inputString[a] = hlpCharAr[a];
            }
            //положим в строчку перевернутую длину address отконвертированную в юникод
            string OutStr = unicode.GetString(Encoding.Convert(cp1251, unicode, cp1251.GetBytes(char_inputString)));
            //добавим туда же значение поля address
            OutStr += inputString;

            return unicode.GetBytes(OutStr);
        }


        //---------------------------------------------------------------------------------------
        // метод - хелпер,обратный MakeMem
        // на входе строчка в формате cp1251 - как я её получаю от Тайфуна
        //---------------------------------------------------------------------------------------
        public static string ParseMem(int ptr, string MsgData)
        {
            byte[] outAr = null;
            uint len = 0;
            byte[] b_len = new byte[4];
            string outString = null; 

            if (ptr > MsgData.Length) return null;

            Encoding cp1251 = Encoding.GetEncoding(1251);
            byte[] cp1251Bytes = cp1251.GetBytes(MsgData);

            //копируем 4 байта последовательности с указанного места
            //в b_len
            for (int a = 0; a < 4; a++)
            {
                b_len[a] = cp1251Bytes[a + ptr];
            }

            //и вычисляем длину данных мема
            len = ByteArtoInt32(b_len);

            //копируем данные мема в выходной массив
            outAr = new byte[len];
            for (int a = 0; a < len; a++)
            {
                outAr[a] = cp1251Bytes[a + 4 + ptr];
            }
            outString = cp1251.GetString(outAr);

            return outString;
        }


        //#endregion
    }

    public class TyphoonMessage
    {
        private string messageData;
        public UInt32 MessageID;
        public UInt32 MessageSubComNum;

        public TyphoonMessage()
        { 
        }


        public TyphoonMessage(char[] msg)
        {
            this.messageData = msg.ToString();
        }


        public string MessageData
        {
            get
            {
                return this.messageData;
            }
            set
            {
                this.messageData = value;
            }
        }
    }

}
