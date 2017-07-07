// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.IO;
using System.Xml.Serialization;



namespace OnvifProxy
{
    public static class MediaSource
    {
        public static List<MediaSourcesProvider.MediaSourceType> MediaSourceList;
        public static System.Timers.Timer RenewTimer;
        private const double TYPHOON_MEDIASOURCELIST_RENEW_TIMEOUT = 60000;//renew each minute

        static MediaSource()
        {
            MediaSourceList = new List<MediaSourcesProvider.MediaSourceType>();
            GetMediaSourcesFromTyphoon(null,null);

            RenewTimer = new System.Timers.Timer(TYPHOON_MEDIASOURCELIST_RENEW_TIMEOUT);
            RenewTimer.Elapsed += new ElapsedEventHandler(GetMediaSourcesFromTyphoon);
            RenewTimer.AutoReset = true;
            RenewTimer.Enabled = true;
        }
        private static void GetMediaSourcesFromTyphoon(object source, ElapsedEventArgs e)
        {
            TyphoonCom.log.Debug("GetMediaSourcesFromTyphoon called");
            
            MediaSourcesProvider.GetMediaSourcesResponse getMediaSourceResp =
                new MediaSourcesProvider.GetMediaSourcesResponse();

            using (TyphoonMsg TyphMsg = TyphoonMsgManager.SendSyncMsg(24))
            {
                if (TyphMsg == null)
                {
                    TyphoonCom.log.DebugFormat("GetMediaSourcesFromTyphoon - Typhoon return null");
                    return;
                }
                TyphMsg.stringMessageData = TyphoonCom.ParseMem(0, TyphMsg.stringMessageData);

                using (MemoryStream ms = new MemoryStream(TyphMsg.byteMessageData))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(MediaSourcesProvider.GetMediaSourcesResponse));
                    try
                    {
                        getMediaSourceResp = (MediaSourcesProvider.GetMediaSourcesResponse)xmlSerializer.Deserialize(ms);
                        MediaSourceList.Clear();
                        TyphoonCom.log.DebugFormat("MediaSourceList - {0}", MediaSourceList.Count);
                        foreach (MediaSourcesProvider.MediaSourceType mediaSource in getMediaSourceResp.MediaSource)
                        {
                            MediaSourceList.Add(mediaSource);
                        }
                        TyphoonCom.log.DebugFormat("MediaSourceList - {0}", MediaSourceList.Count);
                    }
                    catch (System.Runtime.Serialization.SerializationException)
                    {
                        return;//do nothing
                    }
                }
            }            
        }
        public static MediaSourcesProvider.MediaSourceType[] GetUpdatesOfMediaSourceList(string tokenstring)
        {
            if (String.IsNullOrEmpty(tokenstring)) return null;

            string[] tokenArray = tokenstring.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> tokenList = tokenArray.ToList<string>();
            bool containsToken = false;

            if (tokenList.Count != 0)
            {
                for (int y = 0; y < tokenList.Count; y++)
                {
                    containsToken = false;
                    for (var r = 0; r < MediaSourceList.Count;r++ )
                    {
                        if (MediaSourceList.ElementAt(r).token == tokenList.ElementAt(y))
                        {
                            containsToken = true;
                            break;
                        }
                    }
                    if (!containsToken)
                    {
                        MediaSourcesProvider.MediaSourceType MediaSourceToDelete = new MediaSourcesProvider.MediaSourceType();
                        MediaSourceToDelete.token = tokenList.ElementAt(y);
                        MediaSourceList.Add(MediaSourceToDelete);                        
                    }
                }
            }
            if (MediaSourceList != null)
                return MediaSourceList.ToArray();
            else
                return null;
            
        }
    }

    public  class TestMediaSource
    {
        public TestMediaSource()
        {
            Thread eventThread = new Thread(new ThreadStart(Test));
            eventThread.IsBackground = true;
            eventThread.Start();
        }

        public void Test()
        {
            while (true)
            {
                //var Mediasrc = new MediaSource();
                //Thread.Sleep(1);
                //Mediasrc.Dispose();
            }
        }
            
    }

    public class MediaSourceListType
    {
        public string Number;
        public System.DateTime TimeStamp {get;set;}
        public MediaSourcesProvider.MediaSourceType MediaSource;

    }
}
