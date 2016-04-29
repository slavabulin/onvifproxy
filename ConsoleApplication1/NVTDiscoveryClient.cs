using System;
using System.Collections.Generic;
using System.ServiceModel.Discovery;
using System.Text;
using NVT;
using System.ServiceModel;
using System.Threading;

namespace OnvifProxy
{
    class NVTDiscoClient
    {

        private DiscoveryClient discoveryClient;
        private UdpDiscoveryEndpoint discEP;
        private FindCriteria findCriteria;
        private XmlConfig conf;
        private ConfigStruct confstr;
        private uint msgID;

        public NVTDiscoClient(uint MsgID)
        {
            //---------------------------------------------- 
            conf = new XmlConfig();
            confstr = new ConfigStruct();
            confstr = conf.Read();

            msgID = MsgID;
            discEP = new UdpDiscoveryEndpoint(DiscoveryVersion.WSDiscoveryApril2005);

            discoveryClient = new DiscoveryClient(discEP);
            discoveryClient.FindCompleted += new EventHandler<FindCompletedEventArgs>(ClientFindCompleted);
            discoveryClient.FindProgressChanged += new EventHandler<FindProgressChangedEventArgs>(ClientFindProgressChanged);

            findCriteria = new FindCriteria(typeof(NetworkVideoTransmitter));
            findCriteria.ScopeMatchBy = FindCriteria.ScopeMatchByUuid;//см. http://www.ietf.org/rfc/rfc2396.txt
            findCriteria.ScopeMatchBy = new Uri("onvif://www.onvif.org/type/NetworkVideoTransmitter");
            findCriteria.Duration = TimeSpan.FromSeconds(5);

            discoveryClient.FindAsync(findCriteria);
            //---------------------------------------------- 
        }

        void ClientFindProgressChanged(object sender, FindProgressChangedEventArgs e)
        {            
            Console.WriteLine("Found NVTService endpoint at {0}\n", e.EndpointDiscoveryMetadata.ListenUris[0].OriginalString);
            if (confstr != null)
            {
                if (confstr.Capabilities.Device.XAddr != e.EndpointDiscoveryMetadata.ListenUris[0].OriginalString)
                {
                    TyphoonMsgManager.SendAsyncMsg(201, 1, FormNVTResponse(e.EndpointDiscoveryMetadata), 0);
                }
            }
            else
            {
                //но блядь как?!!
                throw new Exception("NVTDiscoClient - couldnt read config");
            }
        }

        void ClientFindCompleted(object sender, FindCompletedEventArgs e)
        {
            // Implement this method to access the FindResponse object through e.Result, which includes all the endpoints found
            try
            {
                this.discoveryClient.InnerChannel.Close();
                this.discoveryClient.ChannelFactory.Close();
            }
            catch (Exception ee)
            {
                TyphoonCom.log.DebugFormat("ClientFindComlete raised exception - {0}", ee.Message);
            }

            this.discoveryClient.FindCompleted -= ClientFindCompleted;
            this.discoveryClient.FindProgressChanged -= ClientFindProgressChanged;
            this.discoveryClient.Close();            
            this.discoveryClient = null;
            this.discEP = null;
            findCriteria = null;
            confstr = null;
            conf = null;

            GC.Collect();
        }


        public byte[] FormNVTResponse(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            uint len_Address, len_Scopes, len_XAddrs, len_Metadata;
            string address, scopes = null, xAddrs, metadata;
            char[] tmpAr = new char[4];
            byte[] b_address, b_scopes, b_xAddrs, b_metadata;
            string OutStr = null;
            
            Encoding unicode = Encoding.Unicode;
            Encoding cp1251 = Encoding.GetEncoding(1251);


            try
            {
                //-----------------------------------------------------------
                //заполним строчку address
                address = endpointDiscoveryMetadata.Address.ToString();
                byte[] unicodeBytes = unicode.GetBytes(address);
                b_address = Encoding.Convert(unicode, cp1251, unicodeBytes);
                //длина в формате cp1251
                len_Address = (uint)b_address.Length;

                char[] hlpCharAr = cp1251.GetChars(TyphoonCom.Int32toByteAr(len_Address));
                char[] ch_address = { '0', '0', '0', '0' };
                for (int a = 0; a < hlpCharAr.Length; a++)
                {
                    ch_address[a] = hlpCharAr[a];
                }
                //положим в строчку перевернутую длину address отконвертированную в юникод
                OutStr = unicode.GetString(Encoding.Convert(cp1251, unicode, cp1251.GetBytes(ch_address)));
                //добавим туда же значение поля address
                OutStr += address;

                //слепим все scope в одну строку и положим в scopes
                for (int a = 0; a < endpointDiscoveryMetadata.Scopes.Count; a++)
                {
                    scopes += String.Concat(endpointDiscoveryMetadata.Scopes[a], " ");
                }

                unicodeBytes = null;
                unicodeBytes = unicode.GetBytes(scopes);
                b_scopes = Encoding.Convert(unicode, cp1251, unicodeBytes);
                //длина в формате cp1251
                len_Scopes = (uint)b_scopes.Length;

                hlpCharAr = cp1251.GetChars(TyphoonCom.Int32toByteAr(len_Scopes));
                char[] ch_scopes = { '0', '0', '0', '0' };
                for (int a = 0; a < hlpCharAr.Length; a++)
                {
                    ch_scopes[a] = hlpCharAr[a];
                }
                //добавим в строчку перевернутую длину scopes отконвертированную в юникод
                OutStr += unicode.GetString(Encoding.Convert(cp1251, unicode, cp1251.GetBytes(ch_scopes)));
                //добавим туда же значение поля scopes
                OutStr += scopes;

                //заполним xAddrs
                xAddrs = endpointDiscoveryMetadata.ListenUris[0].OriginalString;
                unicodeBytes = null;
                unicodeBytes = unicode.GetBytes(xAddrs);
                b_xAddrs = Encoding.Convert(unicode, cp1251, unicodeBytes);
                //длина в формате cp1251
                len_XAddrs = (uint)b_xAddrs.Length;

                hlpCharAr = cp1251.GetChars(TyphoonCom.Int32toByteAr(len_XAddrs));
                char[] ch_xaddrs = { '0', '0', '0', '0' };
                for (int a = 0; a < hlpCharAr.Length; a++)
                {
                    ch_xaddrs[a] = hlpCharAr[a];
                }

                //добавим в строчку перевернутую длину xAddrs отконвертированную в юникод
                OutStr += unicode.GetString(Encoding.Convert(cp1251, unicode, cp1251.GetBytes(ch_xaddrs)));
                //добавим туда же значение поля xAddrs
                OutStr += xAddrs;

                metadata = endpointDiscoveryMetadata.Version.ToString();
                unicodeBytes = null;
                unicodeBytes = unicode.GetBytes(metadata);
                b_metadata = Encoding.Convert(unicode, cp1251, unicodeBytes);
                //длина в формате cp1251
                len_Metadata = (uint)b_metadata.Length;

                hlpCharAr = cp1251.GetChars(TyphoonCom.Int32toByteAr(len_Metadata));
                char[] ch_metadata = { '0', '0', '0', '0' };
                for (int a = 0; a < hlpCharAr.Length; a++)
                {
                    ch_metadata[a] = hlpCharAr[a];
                }
                OutStr += unicode.GetString(Encoding.Convert(cp1251, unicode, cp1251.GetBytes(ch_metadata)));
                OutStr += metadata;

                //-----------------------------------------------------------
            }
            catch (Exception ex)
            {
                TyphoonCom.log.DebugFormat("FormNVTResponse - {0}", ex.Message);
            }
            return unicode.GetBytes(OutStr);
        }

    }
}
