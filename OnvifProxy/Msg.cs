using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OnvifProxy
{
    public class Msg
    {
        public UInt32 MsgID;
        public UInt32 MsgSubComNum;
        public TyphoonMsgType MsgType;

        private string _messageData;
        private static UInt32 _msgIDCounter;

        public Msg(TyphoonMsgType msgType)
        {
            MsgType = msgType;
            
            if ((_msgIDCounter == 0
                || _msgIDCounter == UInt32.MaxValue) 
               )
            {
                _msgIDCounter = 1;
            }

            MsgID = _msgIDCounter;
            _msgIDCounter++;

            if(msgType == TyphoonMsgType.Zond)
            {
                MsgID = 0;
            }
        }
        public byte[] byteMessageData
        {
            get
            {
                if (_messageData == null)
                {
                    return null;
                }
                else
                {
                    return Encoding.GetEncoding(1251).GetBytes(_messageData);
                }

            }
            set
            {
                this._messageData = Encoding.GetEncoding(1251).GetString(value);
            }
        }
        public string stringMessageData
        {
            get
            {
                return this._messageData;
            }
            set
            {
                this._messageData = value;
            }
        }

    }

    public static class MsgQueue
    {
        private const double TYPHOON_MSG_EXIST_TIMEOUT = 5000;

        internal static ConcurrentBag<System.Timers.Timer> timerList = new ConcurrentBag<System.Timers.Timer>();
        // очередь запросов к Тайфуну
        internal static ConcurrentBag<Msg> queueRequestToTyphoon = new ConcurrentBag<Msg>();
        // очередь ответов от Тайфуна
        internal static ConcurrentBag<Msg> queueResponseFromTyphoon = new ConcurrentBag<Msg>();
        // очередь команд от Тайфуна
        internal static ConcurrentBag<Msg> queueCommandsFromTyphoon = new ConcurrentBag<Msg>();

        public static void Add(Msg msg)
        {
            //--------------------------------------------
            switch (msg.MsgType)
            {
                case TyphoonMsgType.Command:
                    queueCommandsFromTyphoon.Add(msg);
                    break;
                case TyphoonMsgType.Request:
                    queueRequestToTyphoon.Add(msg);
                    break;
                case TyphoonMsgType.Responce:
                    queueResponseFromTyphoon.Add(msg);
                    break;
                case TyphoonMsgType.Zond:
                    queueRequestToTyphoon.Add(msg);
                    break;
                default:
                    Console.WriteLine("MsgQueue.Add - unknown type msg came.");
                    return;
            }

            System.Timers.Timer msgTimer = new System.Timers.Timer(TYPHOON_MSG_EXIST_TIMEOUT);
            msgTimer.Elapsed += delegate { OnMsgTimeout(msg.MsgID, msg.MsgType, ref msgTimer); };
            msgTimer.AutoReset = true;
            msgTimer.Enabled = true;
            //starts OnMsgTimeout in ThreadPool - to lock the collection
            msgTimer.SynchronizingObject = null;

            timerList.Add(msgTimer);
        }
        private static void OnMsgTimeout(UInt32 msgID, TyphoonMsgType msgType, ref System.Timers.Timer timer)
        {
            ConcurrentBag<Msg> queue = null;

            switch (msgType)
            {
                case TyphoonMsgType.Command:
                    queue = queueCommandsFromTyphoon;
                    break;
                case TyphoonMsgType.Request:
                    queue = queueRequestToTyphoon;
                    break;
                case TyphoonMsgType.Responce:
                    queue = queueResponseFromTyphoon;
                    break;
                case TyphoonMsgType.Zond:
                    queue = queueRequestToTyphoon;
                    break;
            }
            try
            {
                Msg msg1 = queue.SingleOrDefault<Msg>(msg => msg.MsgID == msgID);
                queue.TryTake(out msg1);
            }
            catch (NullReferenceException)
            {
            }
            finally
            {
                timerList.TryTake(out timer);
                timer.Close();
                timer.Dispose();                
            }
        }
    }

    public static class MsgQueueManager
    {
        private const int TYPHOON_CHECK_FOR_RESPONSE_PERIOD = 100;
        private const int TYPHOON_RESPONSE_TIMEOUT = 4500;


        public static void SendAsyncMsg(Msg msg)
        {
            MsgQueue.Add(msg);
        }
        public static Msg SendSyncMsg(Msg msg)
        {
            #region
            /*
              public static TyphoonMsg SendSyncMsg(ushort comNum, int subComNum, byte[] data, uint msgID)
        {
            TyphoonMsg TyphMsg = new TyphoonMsg(TyphoonMsgType.Request);
            
            byte[] tmp = TyphoonCom.FormCommand(comNum, subComNum, data, msgID);

            for (int a = 0; a < 4; a++)
            {
                TyphMsg.msgID = TyphMsg.msgID << 8;
                TyphMsg.msgID += tmp[9 - a];
            }
            TyphMsg.byteMessageData = TyphoonCom.FormPacket(tmp);
            TyphoonMsgManager.EnqueueMsg(TyphMsg);

            // дожидается ответа 4,5 секунды или возвращает нулл
            GetResponceMsg(ref TyphMsg);
            
            //if (TyphMsg == null) return null;// наверное надо бы fault выкинуть
            return TyphMsg;
        }
            */
            #endregion

            SendAsyncMsg(msg);
            var respMsg = GetResponceMsg(msg.MsgID);
            return respMsg;
        }
        private static Msg GetResponceMsg(UInt32 msgID)
        {
            //TyphoonCom.log.DebugFormat("msgID- {0}", msg.msgID.ToString());
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            Task<Msg> task = null;
            Msg msg = null;
            try
            {
                var token = cancelTokenSource.Token;
                task = Task<Msg>.Factory.StartNew(() => TaskGetResponceMsg(msgID, token), token);
                task.Wait(TYPHOON_RESPONSE_TIMEOUT);
                cancelTokenSource.Cancel();
                if(task!=null) msg = task.Result;//??????????? 
            }
            catch (ObjectDisposedException ode)
            {
                TyphoonCom.log.ErrorFormat("msgID- {0}, Exception - {1}", msgID.ToString(), ode.StackTrace.ToString());
            }
            catch (ArgumentNullException ane)
            {
                TyphoonCom.log.ErrorFormat("msgID- {0}, Exception - {1}", msgID.ToString(), ane.StackTrace.ToString());
            }
            catch (AggregateException ae)
            {
                TyphoonCom.log.ErrorFormat("msgID- {0}, Exception - {1}", msgID.ToString(), ae.StackTrace.ToString());
            }
            finally
            {
                if (task != null) task.Dispose();
                if (cancelTokenSource != null) cancelTokenSource.Dispose();
            }
            return msg;
        }
        private static void PutMsg(Msg msg)
        {
            MsgQueue.Add(msg);
        }
        private static Msg TaskGetResponceMsg(UInt32 msgID, CancellationToken token)
        {
            Msg msg;
            int i = 0;

            while (!token.IsCancellationRequested)
            {
                if (MsgQueue.queueResponseFromTyphoon.Count > 0)
                {
                    try
                    {
                        msg = MsgQueue.queueResponseFromTyphoon.Single(Msg => Msg.MsgID == msgID);
                        if (MsgQueue.queueResponseFromTyphoon.TryTake(out msg))
                        {
                            return msg;
                        }
                        else
                        {
                            if (i < 5)
                            {
                                Thread.Sleep(TYPHOON_CHECK_FOR_RESPONSE_PERIOD);
                                i++;
                            }
                            else
                                break;
                        }
                    }
                    catch (ArgumentNullException ane)
                    {
                        Console.WriteLine("TaskGetMsg() - TryTake - Key=null {0}", ane.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("TaskGetMsg() - TryTake - {0}", ex.Message);
                    }
                }
                else
                {
                    Thread.Sleep(TYPHOON_CHECK_FOR_RESPONSE_PERIOD);
                }
            }
            return null;
        }
    }

    public static class MsgCreator
    {
        public static Msg Create(ushort comNum, int subComNum, byte[] data, TyphoonMsgType msgType)
        {
            var msg = new Msg(msgType);
            var byteArr = FormCommand(comNum, subComNum, data, msg.MsgID);
            if (byteArr != null)
            {
                for (int a = 0; a < 4; a++)
                {
                    msg.MsgID = msg.MsgID << 8;
                    msg.MsgID += byteArr[9 - a];
                }

                msg.byteMessageData = FormPacket(byteArr);
            }
            else
            {
                msg = null;
            }                
            return msg;
        }

        //---------------------------------------------------------------------------------------
        // формирование команды для дальнейшего формирования пакета и отправки Тайфуну
        // на входе - номер команды и данные для неё, выход - массив байт для вставки в пакет
        //---------------------------------------------------------------------------------------
        public static byte[] FormCommand(ushort comNum, int subComNum, byte[] data, uint msgID)
        {
            byte[] dataCommandBlock;
            byte[] subComNumAr;
            byte[] dataLengthAr;
            byte[] msgIDAr;
            //uint MsgID;

            //MsgID = msgID;

            if (data != null)
            {
                //конвертим данные из юникода в cp1251
                Encoding unicode = Encoding.Unicode;
                Encoding cp1251 = Encoding.GetEncoding(1251);
                byte[] unicodeBytes = data;
                byte[] ASCIIBytes = Encoding.Convert(unicode, cp1251, unicodeBytes);
                //создаем массив в котором будет лежать сформированная команда
                dataCommandBlock = new byte[(ASCIIBytes.Length + 14)];
                //вставляем значение кода субкоманды
                subComNumAr = Int32toByteAr((uint)subComNum);
                for (int y = 0; y < 4; y++)
                {
                    dataCommandBlock[2 + y] = subComNumAr[y];
                }

                ////если msgID при вызове метода 0, используем в качестве msgID внутренний счетчик 
                ////(сделано для формирования ответов на команды тайфуна с msgID запроса)
                //if (msgID == 0)
                //{
                //    //MsgID = 
                //}
                ////{
                ////    if (msgIDCounter == uint.MaxValue || msgIDCounter == 0) msgIDCounter = 1;
                ////    MsgID = msgIDCounter;
                ////    msgIDCounter++;
                ////}
                //// если не 0, то указанное значение
                //else
                //{
                //    //MsgID = msgID;
                //}

                //превращаем его в массив байт
                msgIDAr = Int32toByteAr((uint)(msgID));

                //и засовываем в массив с 6 по 9 байты
                for (int t = 0; t < 4; t++)
                {
                    dataCommandBlock[6 + t] = msgIDAr[t];
                }
                //делаем из длины данных массив байт
                //dataLengthAr = Int32toByteAr((uint)(data.Length));
                dataLengthAr = Int32toByteAr((uint)(ASCIIBytes.Length));
                //и засовываем в массив с 10 по 13 байты
                for (int t = 0; t < 4; t++)
                {
                    dataCommandBlock[10 + t] = dataLengthAr[t];
                }
                //засовываем в массив сами данные с 14 байта 
                //и до упора
                for (int a = 0; a < ASCIIBytes.Length; a++)
                {
                    dataCommandBlock[14 + a] = ASCIIBytes[a];
                }
            }
            else
            {
                //создаем массив в котором будет лежать сформированная команда
                dataCommandBlock = new byte[14];
                //вставляем значение кода субкоманды
                subComNumAr = Int32toByteAr((uint)subComNum);
                for (int y = 0; y < 4; y++)
                {
                    dataCommandBlock[2 + y] = subComNumAr[y];
                }

                //генерим значение для ID команды
                //if (msgIDCounter == uint.MaxValue || msgIDCounter == 0) msgIDCounter = 100;
                //MsgID = msgIDCounter;
                //msgIDCounter++;
                //превращаем его в массив байт
                msgIDAr = Int32toByteAr((uint)(msgID));

                //и засовываем в массив с 6 по 9 байты
                for (int t = 0; t < 4; t++)
                {
                    dataCommandBlock[6 + t] = msgIDAr[t];
                }
                dataLengthAr = Int32toByteAr(0);
                for (int t = 0; t < 4; t++)
                {
                    dataCommandBlock[10 + t] = dataLengthAr[t];
                }
            }
            //пишем номер команды в 0-1 байты массива
            dataCommandBlock[0] = (byte)comNum;
            dataCommandBlock[1] = (byte)(comNum >> 8);

            return dataCommandBlock;
        }

        //---------------------------------------------------------------------------------------
        //формирование пакета содержащего команду для отправки его тайфуну
        //на входе массив данных сформированный в FormCommand, на выходе пакет
        //готовый для отправки тайфуну
        //---------------------------------------------------------------------------------------
        public static byte[] FormPacket(byte[] dataCommandBlock)
        {
            if (dataCommandBlock != null)
            {
                byte[] Packet = new byte[(13 + dataCommandBlock.Length)];

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

                for (int a = 0; a < dataCommandBlock.Length; a++)
                {
                    Packet[a + 12] = dataCommandBlock[a];
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
        private static uint ByteArtoInt32(byte[] arr)
        {
            if (arr.Length != 4) return 0;
            uint bytes = 0;
            for (int a = 0; a < 4; a++)
            {
                bytes = bytes << 8;
                bytes += arr[3 - a];
            }
            return bytes;
        }

        //---------------------------------------------------------------------------------------
        //подсчет контрольной суммы по CRC8
        //на входе массив по которому считается crc, на выходе байт crc
        //---------------------------------------------------------------------------------------
        private static byte CRC8(byte[] data)
        {
            byte Checksum;

            Checksum = 0xc7;

            for (int a = 0; a < data.Length; a++)
            {
                Checksum ^= data[a];
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
        public static string ParseMem(int ptr, string msgData)
        {
            if (msgData == null) return null;
            byte[] outAr = null;
            uint len = 0;
            byte[] b_len = new byte[4];
            string outString = null;

            if (ptr > msgData.Length) return null;

            Encoding cp1251 = Encoding.GetEncoding(1251);
            byte[] cp1251Bytes = cp1251.GetBytes(msgData);

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
    }
}



