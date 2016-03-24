using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Xml;
using Device;
using Media;


namespace OnvifProxy
{
    [Serializable]
    public class XmlConfig
    {
        public GetProfilesResponse ParseGetProfiles(String InputString)
        {
            String OutputString = "<?xml version=\u00221.0\u0022 encoding=\u0022utf-8\u0022 ?>";
            GetProfilesResponse profile = new GetProfilesResponse();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(GetProfilesResponse));//,"http://www.onvif.org/ver10/schema");

            OutputString = String.Concat(OutputString, InputString);

            using (Stream ms = new MemoryStream(Encoding.UTF8.GetBytes(OutputString)))
            {
                try
                {
                    profile = (GetProfilesResponse)xmlSerializer.Deserialize(ms);
                }
                catch (SerializationException g)
                {
                    Console.WriteLine("Не могу десериализовать файл конфигурации; " + g.Message);
                    return null;
                }
                finally
                {
                    ms.Close();                    
                }
            }
            //profile.Profiles[0].VideoSourceConfiguration.
            return profile;
        }

        public void tmpWrite()
        {
            //откроем поток для записи в файл
            #region создание и наполнение конфигурашки config.xml
            //OnvifScope we = new OnvifScope();
            //OnvifScope we2 = new OnvifScope();
            //config.Scopes = new Collection<OnvifScope>();
            ////we.ScopeType = OnvifScope.ScopeTypes.Fixed;
            ////we2.ScopeType = OnvifScope.ScopeTypes.Variable;
            //we.Data = "onvif://www.onvif.org/type/video_encoder";
            //we2.Data = "onvif://www.onvif.org/hardware/TyphoonPC";

            //config.Scopes.Add(we);
            //config.Scopes.Add(we2);
            //config.IPAddr = "172.16.38.1";

            //config.Capabilities = new Capabilities();
            //config.Capabilities.Analytics = new AnalyticsCapabilities();
            //config.Capabilities.Analytics.Any = new XmlElement[0];
            //config.Capabilities.Analytics.AnyAttr = new XmlAttribute[0];
            //config.Capabilities.Analytics.XAddr = "/";
            //config.Capabilities.Analytics.AnalyticsModuleSupport = false;
            //config.Capabilities.Analytics.RuleSupport = false;

            //config.Capabilities.Device = new DeviceCapabilities();
            //config.Capabilities.Device.XAddr = "/";

            //config.Capabilities.Device.Extension = new DeviceCapabilitiesExtension();

            //config.Capabilities.Device.IO = new IOCapabilities();
            //config.Capabilities.Device.IO.InputConnectors = 1;
            //config.Capabilities.Device.IO.InputConnectorsSpecified = true;
            //config.Capabilities.Device.IO.RelayOutputs = 1;
            //config.Capabilities.Device.IO.RelayOutputsSpecified = true;

            //config.Capabilities.Device.Network = new NetworkCapabilities1();
            //config.Capabilities.Device.Network.DynDNS = true;
            //config.Capabilities.Device.Network.DynDNSSpecified = true;
            //config.Capabilities.Device.Network.IPFilter = true;
            //config.Capabilities.Device.Network.IPFilterSpecified = true;
            //config.Capabilities.Device.Network.IPVersion6 = true;
            //config.Capabilities.Device.Network.IPVersion6Specified = true;
            //config.Capabilities.Device.Network.ZeroConfiguration = true;
            //config.Capabilities.Device.Network.ZeroConfigurationSpecified = true;

            //config.Capabilities.Device.Network.Extension = new NetworkCapabilitiesExtension();
            //config.Capabilities.Device.Network.Extension.Any = new XmlElement[0];
            //config.Capabilities.Device.Network.Extension.Dot11Configuration = true;
            //config.Capabilities.Device.Network.Extension.Dot11ConfigurationSpecified = true;
            //config.Capabilities.Device.Network.Extension.Extension = new NetworkCapabilitiesExtension2();
            //config.Capabilities.Device.Network.Extension.Extension.Any = new XmlElement[0];


            //config.Capabilities.Device.Security = new SecurityCapabilities1();
            //config.Capabilities.Device.Security.AccessPolicyConfig = false;
            //config.Capabilities.Device.Security.Extension = new SecurityCapabilitiesExtension();
            //config.Capabilities.Device.Security.Extension.Extension = new SecurityCapabilitiesExtension2();
            //config.Capabilities.Device.Security.Extension.Extension.Dot1X = false;
            //config.Capabilities.Device.Security.Extension.Extension.RemoteUserHandling = false;
            //config.Capabilities.Device.Security.Extension.Extension.SupportedEAPMethod = null;
            //config.Capabilities.Device.Security.Extension.TLS10 = false;
            //config.Capabilities.Device.Security.KerberosToken = false;
            //config.Capabilities.Device.Security.OnboardKeyGeneration = false;
            //config.Capabilities.Device.Security.RELToken = false;
            //config.Capabilities.Device.Security.SAMLToken = false;
            //config.Capabilities.Device.Security.TLS11 = false;
            //config.Capabilities.Device.Security.TLS12 = false;
            //config.Capabilities.Device.Security.X509Token = false;

            //config.Capabilities.Device.System = new SystemCapabilities1();
            //config.Capabilities.Device.System.DiscoveryBye = false;
            //config.Capabilities.Device.System.DiscoveryResolve = false;
            //config.Capabilities.Device.System.FirmwareUpgrade = false;
            //config.Capabilities.Device.System.RemoteDiscovery = false;
            //config.Capabilities.Device.System.SupportedVersions = new OnvifVersion[0];
            //config.Capabilities.Device.System.SystemBackup = false;
            //config.Capabilities.Device.System.SystemLogging = false;


            //config.Capabilities.Events = new EventCapabilities();
            //config.Capabilities.Events.WSPausableSubscriptionManagerInterfaceSupport = false;
            //config.Capabilities.Events.WSPullPointSupport = false;
            //config.Capabilities.Events.WSSubscriptionPolicySupport = false;

            //config.Capabilities.Extension = new CapabilitiesExtension();
            //config.Capabilities.Extension.AnalyticsDevice = new AnalyticsDeviceCapabilities();
            //config.Capabilities.Extension.AnalyticsDevice.Extension = new AnalyticsDeviceExtension();
            //config.Capabilities.Extension.AnalyticsDevice.RuleSupport = false;
            //config.Capabilities.Extension.AnalyticsDevice.RuleSupportSpecified = false;

            //config.Capabilities.Extension.DeviceIO = new DeviceIOCapabilities();
            //config.Capabilities.Extension.DeviceIO.AudioOutputs = 1;
            //config.Capabilities.Extension.DeviceIO.RelayOutputs = 1;
            //config.Capabilities.Extension.DeviceIO.VideoOutputs = 1;
            //config.Capabilities.Extension.DeviceIO.VideoSources = 1;

            //config.Capabilities.Extension.Display = new DisplayCapabilities();
            //config.Capabilities.Extension.Display.FixedLayout = false;

            //config.Capabilities.Extension.Receiver = new ReceiverCapabilities();
            //config.Capabilities.Extension.Receiver.MaximumRTSPURILength = 0;
            //config.Capabilities.Extension.Receiver.RTP_Multicast = false;
            //config.Capabilities.Extension.Receiver.RTP_RTSP_TCP = false;
            //config.Capabilities.Extension.Receiver.RTP_TCP = false;
            //config.Capabilities.Extension.Receiver.SupportedReceivers = 0;

            //config.Capabilities.Extension.Recording = new RecordingCapabilities();
            //config.Capabilities.Extension.Recording.DynamicRecordings = false;
            //config.Capabilities.Extension.Recording.DynamicTracks = false;
            //config.Capabilities.Extension.Recording.MaxStringLength = 0;
            //config.Capabilities.Extension.Recording.MediaProfileSource = false;
            //config.Capabilities.Extension.Recording.ReceiverSource = false;

            //config.Capabilities.Extension.Replay = new ReplayCapabilities();

            //config.Capabilities.Extension.Search = new SearchCapabilities();
            //config.Capabilities.Extension.Search.MetadataSearch = false;

            //config.Capabilities.Imaging = new ImagingCapabilities();

            //config.Capabilities.Media = new MediaCapabilities();
            //config.Capabilities.Media.StreamingCapabilities = new RealTimeStreamingCapabilities();
            //config.Capabilities.Media.Extension = new MediaCapabilitiesExtension();

            //config.Capabilities.PTZ = new PTZCapabilities();
            //config.Capabilities.PTZ.XAddr = new 

            #endregion
            Media.GetProfilesResponse getProf = new Media.GetProfilesResponse();
            XmlSerializer bf = new XmlSerializer(typeof(Media.GetProfilesResponse));
            TextWriter writer = new StreamWriter("Media.GetProfilesResponse.xml");
            #region
            getProf.Profiles = new Profile[1];
            getProf.Profiles[0] = new Profile();
            getProf.Profiles[0].AudioEncoderConfiguration = new Media.AudioEncoderConfiguration();
            getProf.Profiles[0].AudioEncoderConfiguration.Bitrate = 0;
            getProf.Profiles[0].AudioEncoderConfiguration.Encoding = new Media.AudioEncoding();
            getProf.Profiles[0].AudioEncoderConfiguration.Multicast = new Media.MulticastConfiguration();
            getProf.Profiles[0].AudioEncoderConfiguration.Multicast.Address = new Media.IPAddress();
            getProf.Profiles[0].AudioEncoderConfiguration.Multicast.Address.IPv4Address = "string";
            getProf.Profiles[0].AudioEncoderConfiguration.Multicast.Address.IPv6Address = "string";
            getProf.Profiles[0].AudioEncoderConfiguration.Multicast.Address.Type = new Media.IPType();
            getProf.Profiles[0].AudioEncoderConfiguration.Multicast.AutoStart = true;
            getProf.Profiles[0].AudioEncoderConfiguration.Multicast.Port = 0;
            getProf.Profiles[0].AudioEncoderConfiguration.Multicast.TTL = 0;
            getProf.Profiles[0].AudioEncoderConfiguration.Name = "profileName";
            getProf.Profiles[0].AudioEncoderConfiguration.SampleRate = 0;
            getProf.Profiles[0].AudioEncoderConfiguration.SessionTimeout = "string";
            getProf.Profiles[0].AudioEncoderConfiguration.token = "string";
            getProf.Profiles[0].AudioEncoderConfiguration.UseCount = 0;
            getProf.Profiles[0].AudioSourceConfiguration = new Media.AudioSourceConfiguration();
            getProf.Profiles[0].AudioSourceConfiguration.Name = "string";
            getProf.Profiles[0].AudioSourceConfiguration.SourceToken = "string";
            getProf.Profiles[0].AudioSourceConfiguration.token = "string";
            getProf.Profiles[0].AudioSourceConfiguration.UseCount = 0;
            getProf.Profiles[0].Extension = new ProfileExtension();
            getProf.Profiles[0].Extension.AudioDecoderConfiguration = new Media.AudioDecoderConfiguration();
            getProf.Profiles[0].Extension.AudioDecoderConfiguration.Name = "string";
            getProf.Profiles[0].Extension.AudioDecoderConfiguration.token = "string";
            getProf.Profiles[0].Extension.AudioDecoderConfiguration.UseCount = 0;
            getProf.Profiles[0].Extension.AudioOutputConfiguration = new Media.AudioOutputConfiguration();
            getProf.Profiles[0].Extension.AudioOutputConfiguration.Name = "string";
            getProf.Profiles[0].Extension.AudioOutputConfiguration.OutputLevel = 0;
            getProf.Profiles[0].Extension.AudioOutputConfiguration.OutputToken = "string";
            getProf.Profiles[0].Extension.AudioOutputConfiguration.SendPrimacy = "string";
            getProf.Profiles[0].Extension.AudioOutputConfiguration.token = "string";
            getProf.Profiles[0].Extension.AudioOutputConfiguration.UseCount = 0;
            getProf.Profiles[0].Extension.Extension = new ProfileExtension2();
            getProf.Profiles[0].@fixed = false;
            getProf.Profiles[0].fixedSpecified = false;
            getProf.Profiles[0].MetadataConfiguration = new Media.MetadataConfiguration();
            getProf.Profiles[0].MetadataConfiguration.Analytics = false;
            getProf.Profiles[0].MetadataConfiguration.AnalyticsEngineConfiguration = new Media.AnalyticsEngineConfiguration();
            getProf.Profiles[0].MetadataConfiguration.AnalyticsEngineConfiguration.AnalyticsModule = new Media.Config[1];
            getProf.Profiles[0].MetadataConfiguration.AnalyticsEngineConfiguration.Extension = new Media.AnalyticsEngineConfigurationExtension();
            getProf.Profiles[0].MetadataConfiguration.AnalyticsSpecified = false;
            getProf.Profiles[0].MetadataConfiguration.Events = new Media.EventSubscription();
            getProf.Profiles[0].MetadataConfiguration.Events.Filter = new Media.FilterType();
            getProf.Profiles[0].MetadataConfiguration.Events.SubscriptionPolicy = new Media.EventSubscriptionSubscriptionPolicy();
            getProf.Profiles[0].Name = "string";
            getProf.Profiles[0].PTZConfiguration = new Media.PTZConfiguration();
            getProf.Profiles[0].token = "string";
            getProf.Profiles[0].VideoAnalyticsConfiguration = new Media.VideoAnalyticsConfiguration();
            getProf.Profiles[0].VideoAnalyticsConfiguration.AnalyticsEngineConfiguration = new Media.AnalyticsEngineConfiguration();
            getProf.Profiles[0].VideoEncoderConfiguration = new Media.VideoEncoderConfiguration();
            getProf.Profiles[0].VideoEncoderConfiguration.Encoding = new Media.VideoEncoding();
            getProf.Profiles[0].VideoEncoderConfiguration.H264 = new Media.H264Configuration();
            getProf.Profiles[0].VideoEncoderConfiguration.H264.GovLength = 0;
            getProf.Profiles[0].VideoEncoderConfiguration.H264.H264Profile = new Media.H264Profile();
            getProf.Profiles[0].VideoEncoderConfiguration.MPEG4 = new Media.Mpeg4Configuration();
            getProf.Profiles[0].VideoEncoderConfiguration.MPEG4.GovLength = 0;
            getProf.Profiles[0].VideoEncoderConfiguration.MPEG4.Mpeg4Profile = new Media.Mpeg4Profile();
            getProf.Profiles[0].VideoEncoderConfiguration.Multicast = new Media.MulticastConfiguration();
            getProf.Profiles[0].VideoEncoderConfiguration.Multicast.Address = new Media.IPAddress();
            getProf.Profiles[0].VideoEncoderConfiguration.Multicast.AutoStart = false;
            getProf.Profiles[0].VideoEncoderConfiguration.Multicast.Port = 0;
            getProf.Profiles[0].VideoEncoderConfiguration.Multicast.TTL = 0;
            getProf.Profiles[0].VideoEncoderConfiguration.Name = "string";
            getProf.Profiles[0].VideoEncoderConfiguration.Quality = 0;
            getProf.Profiles[0].VideoEncoderConfiguration.RateControl = new Media.VideoRateControl();
            getProf.Profiles[0].VideoEncoderConfiguration.RateControl.BitrateLimit = 0;
            getProf.Profiles[0].VideoEncoderConfiguration.RateControl.EncodingInterval = 0;
            getProf.Profiles[0].VideoEncoderConfiguration.RateControl.FrameRateLimit = 0;
            getProf.Profiles[0].VideoEncoderConfiguration.Resolution = new Media.VideoResolution();
            getProf.Profiles[0].VideoEncoderConfiguration.Resolution.Height = 0;
            getProf.Profiles[0].VideoEncoderConfiguration.Resolution.Width = 0;
            getProf.Profiles[0].VideoEncoderConfiguration.SessionTimeout = "string";
            getProf.Profiles[0].VideoEncoderConfiguration.token = "string";
            getProf.Profiles[0].VideoEncoderConfiguration.UseCount = 0;
            getProf.Profiles[0].VideoSourceConfiguration = new Media.VideoSourceConfiguration();
            getProf.Profiles[0].VideoSourceConfiguration.Bounds = new Media.IntRectangle();
            getProf.Profiles[0].VideoSourceConfiguration.Bounds.height = 0;
            getProf.Profiles[0].VideoSourceConfiguration.Bounds.width = 0;
            getProf.Profiles[0].VideoSourceConfiguration.Bounds.x = 0;
            getProf.Profiles[0].VideoSourceConfiguration.Bounds.y = 0;
            getProf.Profiles[0].VideoSourceConfiguration.Name = "string";
            getProf.Profiles[0].VideoSourceConfiguration.SourceToken = "string";
            getProf.Profiles[0].VideoSourceConfiguration.token = "string";
            getProf.Profiles[0].VideoSourceConfiguration.UseCount = 0;
            #endregion
            //сериализация
            try
            {
                bf.Serialize(writer, getProf);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Не могу сериализовать файл конфигурации; " + e.Message);
            }
            catch
            {
                Console.WriteLine("Не могу сериализовать файл конфигурации; ");
            }
            finally
            {
                writer.Close();
            }
        }


        public void Write( ConfigStruct config)
        {
            //откроем поток для записи в файл
            #region создание и наполнение конфигурашки config.xml
            //OnvifScope we = new OnvifScope();
            //OnvifScope we2 = new OnvifScope();
            //config.Scopes = new Collection<OnvifScope>();
            ////we.ScopeType = OnvifScope.ScopeTypes.Fixed;
            ////we2.ScopeType = OnvifScope.ScopeTypes.Variable;
            //we.Data = "onvif://www.onvif.org/type/video_encoder";
            //we2.Data = "onvif://www.onvif.org/hardware/TyphoonPC";

            //config.Scopes.Add(we);
            //config.Scopes.Add(we2);
            //config.IPAddr = "172.16.38.1";

            //config.Capabilities = new Capabilities();
            //config.Capabilities.Analytics = new AnalyticsCapabilities();
            //config.Capabilities.Analytics.Any = new XmlElement[0];
            //config.Capabilities.Analytics.AnyAttr = new XmlAttribute[0];
            //config.Capabilities.Analytics.XAddr = "/";
            //config.Capabilities.Analytics.AnalyticsModuleSupport = false;
            //config.Capabilities.Analytics.RuleSupport = false;

            //config.Capabilities.Device = new DeviceCapabilities();
            //config.Capabilities.Device.XAddr = "/";

            //config.Capabilities.Device.Extension = new DeviceCapabilitiesExtension();

            //config.Capabilities.Device.IO = new IOCapabilities();
            //config.Capabilities.Device.IO.InputConnectors = 1;
            //config.Capabilities.Device.IO.InputConnectorsSpecified = true;
            //config.Capabilities.Device.IO.RelayOutputs = 1;
            //config.Capabilities.Device.IO.RelayOutputsSpecified = true;

            //config.Capabilities.Device.Network = new NetworkCapabilities1();
            //config.Capabilities.Device.Network.DynDNS = true;
            //config.Capabilities.Device.Network.DynDNSSpecified = true;
            //config.Capabilities.Device.Network.IPFilter = true;
            //config.Capabilities.Device.Network.IPFilterSpecified = true;
            //config.Capabilities.Device.Network.IPVersion6 = true;
            //config.Capabilities.Device.Network.IPVersion6Specified = true;
            //config.Capabilities.Device.Network.ZeroConfiguration = true;
            //config.Capabilities.Device.Network.ZeroConfigurationSpecified = true;

            //config.Capabilities.Device.Network.Extension = new NetworkCapabilitiesExtension();
            //config.Capabilities.Device.Network.Extension.Any = new XmlElement[0];
            //config.Capabilities.Device.Network.Extension.Dot11Configuration = true;
            //config.Capabilities.Device.Network.Extension.Dot11ConfigurationSpecified = true;
            //config.Capabilities.Device.Network.Extension.Extension = new NetworkCapabilitiesExtension2();
            //config.Capabilities.Device.Network.Extension.Extension.Any = new XmlElement[0];


            //config.Capabilities.Device.Security = new SecurityCapabilities1();
            //config.Capabilities.Device.Security.AccessPolicyConfig = false;
            //config.Capabilities.Device.Security.Extension = new SecurityCapabilitiesExtension();
            //config.Capabilities.Device.Security.Extension.Extension = new SecurityCapabilitiesExtension2();
            //config.Capabilities.Device.Security.Extension.Extension.Dot1X = false;
            //config.Capabilities.Device.Security.Extension.Extension.RemoteUserHandling = false;
            //config.Capabilities.Device.Security.Extension.Extension.SupportedEAPMethod = null;
            //config.Capabilities.Device.Security.Extension.TLS10 = false;
            //config.Capabilities.Device.Security.KerberosToken = false;
            //config.Capabilities.Device.Security.OnboardKeyGeneration = false;
            //config.Capabilities.Device.Security.RELToken = false;
            //config.Capabilities.Device.Security.SAMLToken = false;
            //config.Capabilities.Device.Security.TLS11 = false;
            //config.Capabilities.Device.Security.TLS12 = false;
            //config.Capabilities.Device.Security.X509Token = false;

            //config.Capabilities.Device.System = new SystemCapabilities1();
            //config.Capabilities.Device.System.DiscoveryBye = false;
            //config.Capabilities.Device.System.DiscoveryResolve = false;
            //config.Capabilities.Device.System.FirmwareUpgrade = false;
            //config.Capabilities.Device.System.RemoteDiscovery = false;
            //config.Capabilities.Device.System.SupportedVersions = new OnvifVersion[0];
            //config.Capabilities.Device.System.SystemBackup = false;
            //config.Capabilities.Device.System.SystemLogging = false;


            //config.Capabilities.Events = new EventCapabilities();
            //config.Capabilities.Events.WSPausableSubscriptionManagerInterfaceSupport = false;
            //config.Capabilities.Events.WSPullPointSupport = false;
            //config.Capabilities.Events.WSSubscriptionPolicySupport = false;

            //config.Capabilities.Extension = new CapabilitiesExtension();
            //config.Capabilities.Extension.AnalyticsDevice = new AnalyticsDeviceCapabilities();
            //config.Capabilities.Extension.AnalyticsDevice.Extension = new AnalyticsDeviceExtension();
            //config.Capabilities.Extension.AnalyticsDevice.RuleSupport = false;
            //config.Capabilities.Extension.AnalyticsDevice.RuleSupportSpecified = false;

            //config.Capabilities.Extension.DeviceIO = new DeviceIOCapabilities();
            //config.Capabilities.Extension.DeviceIO.AudioOutputs = 1;
            //config.Capabilities.Extension.DeviceIO.RelayOutputs = 1;
            //config.Capabilities.Extension.DeviceIO.VideoOutputs = 1;
            //config.Capabilities.Extension.DeviceIO.VideoSources = 1;

            //config.Capabilities.Extension.Display = new DisplayCapabilities();
            //config.Capabilities.Extension.Display.FixedLayout = false;

            //config.Capabilities.Extension.Receiver = new ReceiverCapabilities();
            //config.Capabilities.Extension.Receiver.MaximumRTSPURILength = 0;
            //config.Capabilities.Extension.Receiver.RTP_Multicast = false;
            //config.Capabilities.Extension.Receiver.RTP_RTSP_TCP = false;
            //config.Capabilities.Extension.Receiver.RTP_TCP = false;
            //config.Capabilities.Extension.Receiver.SupportedReceivers = 0;

            //config.Capabilities.Extension.Recording = new RecordingCapabilities();
            //config.Capabilities.Extension.Recording.DynamicRecordings = false;
            //config.Capabilities.Extension.Recording.DynamicTracks = false;
            //config.Capabilities.Extension.Recording.MaxStringLength = 0;
            //config.Capabilities.Extension.Recording.MediaProfileSource = false;
            //config.Capabilities.Extension.Recording.ReceiverSource = false;

            //config.Capabilities.Extension.Replay = new ReplayCapabilities();

            //config.Capabilities.Extension.Search = new SearchCapabilities();
            //config.Capabilities.Extension.Search.MetadataSearch = false;

            //config.Capabilities.Imaging = new ImagingCapabilities();

            //config.Capabilities.Media = new MediaCapabilities();
            //config.Capabilities.Media.StreamingCapabilities = new RealTimeStreamingCapabilities();
            //config.Capabilities.Media.Extension = new MediaCapabilitiesExtension();

            //config.Capabilities.PTZ = new PTZCapabilities();
            //config.Capabilities.PTZ.XAddr = new 

            #endregion
            #region создание и наполнение GetProfilesResponse

            #endregion

            if (config != null)
            {
                XmlSerializer bf = new XmlSerializer(typeof(ConfigStruct));
                using (TextWriter writer = new StreamWriter("config.xml"))
                {
                    //config.Capabilities.Analytics.XAddr = "http://" + config.IPAddr
                    //    + config.Capabilities.Analytics.XAddr.Remove(0, 7 + TrimIP(config.Capabilities.Analytics.XAddr).Length);
                    config.Capabilities.Device.XAddr = "http://" + config.IPAddr
                        + config.Capabilities.Device.XAddr.Remove(0, 7 + TrimIP(config.Capabilities.Device.XAddr).Length);
                    config.Capabilities.Events.XAddr = "http://" + config.IPAddr
                        + config.Capabilities.Events.XAddr.Remove(0, 7 + TrimIP(config.Capabilities.Events.XAddr).Length);
                    //config.Capabilities.Imaging.XAddr = "http://" + config.IPAddr
                    //    + config.Capabilities.Imaging.XAddr.Remove(0, 7 + TrimIP(config.Capabilities.Imaging.XAddr).Length);
                    config.Capabilities.Media.XAddr = "http://" + config.IPAddr
                        + config.Capabilities.Media.XAddr.Remove(0, 7 + TrimIP(config.Capabilities.Media.XAddr).Length);
                    //config.Capabilities.PTZ.XAddr = "http://" + config.IPAddr
                    //    + config.Capabilities.PTZ.XAddr.Remove(0, 7 + TrimIP(config.Capabilities.PTZ.XAddr).Length);

                    //сериализация
                    try
                    {
                        bf.Serialize(writer, config);
                    }
                    catch (SerializationException e)
                    {
                        Console.WriteLine("Не могу сериализовать файл конфигурации; " + e.Message);
                    }
                    catch
                    {
                        Console.WriteLine("Не могу сериализовать файл конфигурации; ");
                    }
                    finally
                    {
                        writer.Close();
                    }
                }
            }
            else
            {
                TyphoonCom.log.Debug("XmlConfig.Write got null as input");
            }
        }

        public ConfigStruct Read()
        {
            ConfigStruct config = new ConfigStruct();
            config.Scopes = new Collection<OnvifScope>();

            try
            {
                using (FileStream fs = new FileStream("config.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ConfigStruct));
                    try
                    {
                        config = (ConfigStruct)xmlSerializer.Deserialize(fs);
                    }
                    catch (SerializationException g)
                    {
                        Console.WriteLine("Не могу десериализовать файл конфигурации; " + g.Message);
                        //return null;
                        throw g;
                    }
                    
                    finally
                    {
                        fs.Close();
                    }
                }
            }
            catch (FileNotFoundException fnf)
            {
                Console.WriteLine("Не найден файл конфигурации - config.xml");
            }            
            return config;
        }


        public ConfigStruct DeserializeString(ConfigStruct cfgStruct,String InputString)
        {
            if (InputString != null)
            {
                //String DataString = "<?xml version=\u00221.0\u0022 encoding=\u0022utf-8\u0022 ?><ConfigStruct xmlns:xsd=\u0022http://www.w3.org/2001/XMLSchema\u0022 xmlns:xsi=\u0022http://www.w3.org/2001/XMLSchema-instance\u0022>";
                ////String DataString = "<ConfigStruct xmlns:xsi=\u0022http://www.w3.org/2001/XMLSchema-instance\u0022 xmlns:xsd=\u0022http://www.w3.org/2001/XMLSchema\u0022>";
                //DataString = String.Concat(DataString, "<IPAddr>172.16.38.1</IPAddr>");
                //InputString = InputString.Replace("<Device>", "<Device  xmlns=\u0022http://www.onvif.org/ver10/schema\u0022>");
                //DataString = String.Concat(DataString, InputString);
                //DataString = String.Concat(DataString, "</ConfigStruct>");

                using (Stream ms = new MemoryStream(Encoding.UTF8.GetBytes(InputString)))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ConfigStruct));
                    //cfgStruct.Scopes = new Collection<OnvifScope>();            
                    #region
                    //-----------
                    //cfgStruct.Capabilities.Device.IO.InputConnectors = 2;
                    //TextWriter writer = new StreamWriter("config.xml");
                    //try
                    //{
                    //    xmlSerializer.Serialize(writer, cfgStruct);
                    //}
                    //catch (SerializationException e)
                    //{
                    //    Console.WriteLine("Не могу сериализовать файл конфигурации; " + e.Message);
                    //}
                    //catch
                    //{
                    //    Console.WriteLine("Не могу сериализовать файл конфигурации; ");
                    //}
                    //finally
                    //{
                    //    writer.Close();
                    //}
                    //-------------

                    #endregion

                    try
                    {
                        cfgStruct = (ConfigStruct)xmlSerializer.Deserialize(ms);
                    }
                    catch (SerializationException g)
                    {
                        Console.WriteLine("Не могу десериализовать файл конфигурации; " + g.Message);
                        return null;
                    }
                    finally
                    {
                        ms.Close();
                    }
                }
            }
            return cfgStruct;
        }

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
    }

    [Serializable]
    public class OnvifScope
    {
        private string ScopeData;
        public string Data
        {
            get
            {
                return this.ScopeData;
            }
            set
            {
                this.ScopeData = value;
            }
        }
        public ScopeDefinition ScopeType;
    }

    [Serializable]
    public class ConfigStruct
    {
        public Collection<OnvifScope> Scopes;

        private DiscoveryMode serviceDiscoveryStatus;
        public DiscoveryMode ServiceDiscoveryStatus
        {
            get
            {
                return serviceDiscoveryStatus;
            }
            set
            {
                serviceDiscoveryStatus = value;
            }

        }


        private string iPAddr;
        public string IPAddr
        {
            get
            {
                return this.iPAddr;
            }
            set
            {
                this.iPAddr = value;
            }
        }
        private string typhoonIP;
        public string TyphoonIP
        {
            get
            {
                return this.typhoonIP;
            }
            set
            {
                this.typhoonIP = value;
            }
        }
        private Device.Capabilities capabilities;
        public Device.Capabilities Capabilities 
        {
            get
            {
                //----------
                //capabilities.Analytics.XAddr = "http://" + iPAddr + capabilities.Analytics.XAddr;
                //capabilities.Device.XAddr = "http://" + iPAddr + capabilities.Device.XAddr;
                //capabilities.Events.XAddr = "http://" + iPAddr + capabilities.Events.XAddr;
                //capabilities.Imaging.XAddr = "http://" + iPAddr + capabilities.Imaging.XAddr;
                //capabilities.Media.XAddr = "http://" + iPAddr + capabilities.Media.XAddr;
                //capabilities.PTZ.XAddr = "http://" + iPAddr + capabilities.PTZ.XAddr;
                //----------
                return capabilities;
            }
            set
            {                
                capabilities = value;
            }
        }
    }
}
    