// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Device;
using Media;
using System.ServiceModel;
using System.ServiceModel.Channels;

using System.ServiceModel.Description;
using System.IdentityModel;
using System.IdentityModel.Tokens;
using System.IdentityModel.Selectors;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.Security.Cryptography;



namespace OnvifProxy
{
    [System.ServiceModel.ServiceContractAttribute(Namespace = "http://www.onvif.org/ver10/device/wsdl", ConfigurationName = "NetworkVideoTransmitter")]
    public interface INVTDeviceServiceChannel : Device.IDevice, System.ServiceModel.IClientChannel
    {
    }
    public interface INVTMediaServiceChannel : Media.IMedia, System.ServiceModel.IClientChannel
    {
    }
    public partial class NVTDeviceServiceClient : System.ServiceModel.ClientBase<INVTDeviceServiceChannel>, IDevice
    {
        public NVTDeviceServiceClient()
        { 
        }

        public NVTDeviceServiceClient(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        public NVTDeviceServiceClient(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint) :
            base(serviceEndpoint)
        {
        }

        public NVTDeviceServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
            //need to create CustomTokenSerializer binding to make Digest and Nonce

            //base.ClientCredentials.HttpDigest.ClientCredential.Password = "root";
            //base.ClientCredentials.HttpDigest.ClientCredential.UserName = "root";
            //base.ClientCredentials.UserName.UserName = "root";
            //base.ClientCredentials.UserName.Password = "root";

        }

        public string GetDeviceInformation(out string Model, out string FirmwareVersion,
            out string SerialNumber, out string HardwareId)
        {
            try
            {
                return base.Channel.GetDeviceInformation(out Model, out FirmwareVersion, out SerialNumber, out HardwareId);
            }
            catch (Exception ex)
            {
                Model = FirmwareVersion = SerialNumber = HardwareId = String.Empty;
            }
            return String.Empty;

        }

        public GetCapabilitiesResponse GetCapabilities(GetCapabilitiesRequest request)
        {
            //йа заглушко!
            //нужно авторизоваться при GetDeviceInformation
            //на Axis'ах

            //set
           
            try
            {
                return base.Channel.GetCapabilities(request);
            }
            catch (ProtocolException)
            {

            }
            return new GetCapabilitiesResponse();

        }

        public void UnauthorizedAccessFault()
        {
            throw new FaultException(new FaultReason("The requested operation is not permitted by the device"),
                          new FaultCode("Sender",
                              new FaultCode("NotAuthorized", "http://www.onvif.org/ver10/error",
                                  new FaultCode("Operation not Permitted", "http://www.onvif.org/ver10/error"))));
        }
        

        public StartSystemRestoreResponse StartSystemRestore(StartSystemRestoreRequest request)
        {
            return new StartSystemRestoreResponse();
        }
        public StartFirmwareUpgradeResponse StartFirmwareUpgrade(StartFirmwareUpgradeRequest request)
        {
            return new StartFirmwareUpgradeResponse();
        }
        public GetSystemUrisResponse GetSystemUris(GetSystemUrisRequest request)
        {
            return new GetSystemUrisResponse();
        }
        public ScanAvailableDot11NetworksResponse ScanAvailableDot11Networks(ScanAvailableDot11NetworksRequest request)
        {
            return new ScanAvailableDot11NetworksResponse();
        }


        public Dot11Status GetDot11Status(string InterfaceToken)
        {
            return new Dot11Status();
        }
        public GetDot11CapabilitiesResponse GetDot11Capabilities(GetDot11CapabilitiesRequest request)
        {
            return new GetDot11CapabilitiesResponse();
        }
        public DeleteDot1XConfigurationResponse DeleteDot1XConfiguration(DeleteDot1XConfigurationRequest request)
        {
            return new DeleteDot1XConfigurationResponse();
        }

        public GetDot1XConfigurationsResponse GetDot1XConfigurations(GetDot1XConfigurationsRequest request)
        {
            return new GetDot1XConfigurationsResponse();
        }

        public Dot1XConfiguration GetDot1XConfiguration(string Dot1XConfigurationToken)
        {
            return new Dot1XConfiguration();
        }

        public void SetDot1XConfiguration(Dot1XConfiguration Dot1XConfiguration)
        {
        }

        public void CreateDot1XConfiguration(Dot1XConfiguration Dot1XConfiguration)
        {
        }

        public LoadCACertificatesResponse LoadCACertificates(LoadCACertificatesRequest request)
        {
            return new LoadCACertificatesResponse();
        }

        public GetCertificateInformationResponse GetCertificateInformation(GetCertificateInformationRequest request)
        {
            return new GetCertificateInformationResponse();
        }

        public LoadCertificateWithPrivateKeyResponse LoadCertificateWithPrivateKey(LoadCertificateWithPrivateKeyRequest request)
        {
            return new LoadCertificateWithPrivateKeyResponse();
        }

        public GetCACertificatesResponse GetCACertificates(GetCACertificatesRequest request)
        {
            return new GetCACertificatesResponse();
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

        public GetRelayOutputsResponse GetRelayOutputs(GetRelayOutputsRequest request)
        {
            return new GetRelayOutputsResponse();
        }

        public void SetClientCertificateMode(bool Enabled)
        {
        }
        public bool GetClientCertificateMode()
        {
            return false;
        }
        public LoadCertificatesResponse LoadCertificates(LoadCertificatesRequest request)
        {
            return new LoadCertificatesResponse();
        }
        public GetPkcs10RequestResponse GetPkcs10Request(GetPkcs10RequestRequest request)
        {
            return new GetPkcs10RequestResponse();
        }
        public DeleteCertificatesResponse DeleteCertificates(DeleteCertificatesRequest request)
        {
            return new DeleteCertificatesResponse();
        }
        public SetCertificatesStatusResponse SetCertificatesStatus(SetCertificatesStatusRequest request)
        {
            return new SetCertificatesStatusResponse();
        }
        public GetCertificatesStatusResponse GetCertificatesStatus(GetCertificatesStatusRequest request)
        {
            return new GetCertificatesStatusResponse();
        }
        public GetCertificatesResponse GetCertificates(GetCertificatesRequest request)
        {
            return new GetCertificatesResponse();
        }
        public CreateCertificateResponse CreateCertificate(CreateCertificateRequest request)
        {
            return new CreateCertificateResponse();
        }
        public void SetAccessPolicy(BinaryData PolicyFile)
        {
        }
        public BinaryData GetAccessPolicy()
        {
            return new BinaryData();
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
            return new IPAddressFilter();
        }
        public void SetZeroConfiguration(string InterfaceToken, bool Enabled)
        {
        }
        public NetworkZeroConfiguration GetZeroConfiguration()
        {
            return new NetworkZeroConfiguration();
        }
        public SetNetworkDefaultGatewayResponse SetNetworkDefaultGateway(SetNetworkDefaultGatewayRequest request)
        {
            return new SetNetworkDefaultGatewayResponse();
        }
        public NetworkGateway GetNetworkDefaultGateway()
        {
            return new NetworkGateway();
        }
        public SetNetworkProtocolsResponse SetNetworkProtocols(SetNetworkProtocolsRequest request)
        {
            return new SetNetworkProtocolsResponse();
        }
        public GetNetworkProtocolsResponse GetNetworkProtocols(GetNetworkProtocolsRequest request)
        {
            return new GetNetworkProtocolsResponse();
        }
        public bool SetNetworkInterfaces(string InterfaceToken, NetworkInterfaceSetConfiguration NetworkInterface)
        {
            return false;
        }

        public GetNetworkInterfacesResponse GetNetworkInterfaces(GetNetworkInterfacesRequest request)
        {
            return new GetNetworkInterfacesResponse();
        }

        public SetDynamicDNSResponse SetDynamicDNS(SetDynamicDNSRequest request)
        {
            return new SetDynamicDNSResponse();
        }

        public DynamicDNSInformation GetDynamicDNS()
        {
            return new DynamicDNSInformation();
        }

        public SetNTPResponse SetNTP(SetNTPRequest request)
        {
            return new SetNTPResponse();
        }

        public NTPInformation GetNTP()
        {
            return new NTPInformation();
        }

        public SetDNSResponse SetDNS(SetDNSRequest request)
        {
            return new SetDNSResponse();
        }

        public DNSInformation GetDNS()
        {
            return new DNSInformation();
        }

        public bool SetHostnameFromDHCP(bool FromDHCP)
        {
            return false;
        }

        public SetHostnameResponse SetHostname(SetHostnameRequest request)
        {
            return new SetHostnameResponse();
        }

        public HostnameInformation GetHostname()
        {
            return new HostnameInformation();
        }

        public GetWsdlUrlResponse GetWsdlUrl(GetWsdlUrlRequest request)
        {
            return new GetWsdlUrlResponse();
        }

        public SetUserResponse SetUser(SetUserRequest request)
        {
            return new SetUserResponse();
        }

        public DeleteUsersResponse DeleteUsers(DeleteUsersRequest request)
        {
            return new DeleteUsersResponse();
        }

        public CreateUsersResponse CreateUsers(CreateUsersRequest request)
        {
            return new CreateUsersResponse();
        }

        public GetUsersResponse GetUsers(GetUsersRequest request)
        {
            return new GetUsersResponse();
        }

        public void SetRemoteUser(RemoteUser RemoteUser)
        {
        }

        public RemoteUser GetRemoteUser()
        {
            return new RemoteUser();
        }

        public GetEndpointReferenceResponse GetEndpointReference(GetEndpointReferenceRequest request)
        {
            return new GetEndpointReferenceResponse();
        }

        public SetDPAddressesResponse SetDPAddresses(SetDPAddressesRequest request)
        {
            return new SetDPAddressesResponse();
        }

        public GetDPAddressesResponse GetDPAddresses(GetDPAddressesRequest request)
        {
            return new GetDPAddressesResponse();
        }

        public void SetRemoteDiscoveryMode(DiscoveryMode RemoteDiscoveryMode)
        {
        }

        public DiscoveryMode GetRemoteDiscoveryMode()
        {
            return new DiscoveryMode();
        }

        public void SetDiscoveryMode(DiscoveryMode DiscoveryMode)
        {
        }

        public DiscoveryMode GetDiscoveryMode()
        {
            return new DiscoveryMode();
        }

        public RemoveScopesResponse RemoveScopes(RemoveScopesRequest request)
        {
            return new RemoveScopesResponse();
        }
        public AddScopesResponse AddScopes(AddScopesRequest request)
        {
            return new AddScopesResponse();
        }
        public SetScopesResponse SetScopes(SetScopesRequest request)
        {
            return new SetScopesResponse();
        }
        public GetScopesResponse GetScopes(GetScopesRequest request)
        {
            return new GetScopesResponse();
        }
        public SupportInformation GetSystemSupportInformation()
        {
            return new SupportInformation();
        }
        public SystemLog GetSystemLog(SystemLogType LogType)
        {
            return new SystemLog();
        }
        public GetSystemBackupResponse GetSystemBackup(GetSystemBackupRequest request)
        {
            return new GetSystemBackupResponse();
        }
        public RestoreSystemResponse RestoreSystem(RestoreSystemRequest request)
        {
            return new RestoreSystemResponse();
        }
        public string SystemReboot()
        {
            return ("");
        }
        public string UpgradeSystemFirmware(AttachmentData Firmware)
        {
            return ("");
        }
        public void SetSystemFactoryDefault(FactoryDefaultType FactoryDefault)
        {
        }
        public SystemDateTime GetSystemDateAndTime()
        {
            return new SystemDateTime();
        }
        public void SetSystemDateAndTime(SetDateTimeType DateTimeType, bool DaylightSavings, Device.TimeZone TimeZone, Device.DateTime UTCDateTime)
        {
        }
        public DeviceServiceCapabilities GetServiceCapabilities()
        {
            return new DeviceServiceCapabilities();
        }
        public GetServicesResponse GetServices(GetServicesRequest request)
        {
            return new GetServicesResponse();
        }
    }
    public partial class NVTMediaServiceClient : System.ServiceModel.ClientBase<INVTMediaServiceChannel>, IMedia
    {
        public NVTMediaServiceClient()
        { 
        }

        public NVTMediaServiceClient(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        public NVTMediaServiceClient(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint) :
            base(serviceEndpoint)
        {
        }

        public NVTMediaServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        public void ActionNotSupported()
        {
            throw new FaultException(new FaultReason("ActionNotSupported"),
                            new FaultCode("Sender",
                                new FaultCode("ActionNotSupported", "http://www.onvif.org/ver10/error",
                                    new FaultCode("ActionNotSupported", "http://www.onvif.org/ver10/error"))));
        }

        public GetServiceCapabilitiesResponse1 GetServiceCapabilities(Media.GetServiceCapabilitiesRequest request)
        {
            throw new NotImplementedException();
        }
        public MediaUri GetSnapshotUri(string ProfileToken)
        {
            return base.Channel.GetSnapshotUri(ProfileToken);
        }

        GetAudioOutputConfigurationsResponse GetAudioOutputConfigurations(GetAudioOutputConfigurationsRequest request)
        {
            return base.Channel.GetAudioOutputConfigurations(request);
        }

        public void AddVideoEncoderConfiguration(string ProfileToken, string ConfigurationToken)
        {
            base.Channel.AddVideoEncoderConfiguration(ProfileToken, ConfigurationToken);
        }

        public void RemoveVideoEncoderConfiguration(string ProfileToken)
        {
            base.Channel.RemoveVideoEncoderConfiguration(ProfileToken);
        }

        public void AddVideoSourceConfiguration(string ProfileToken, string ConfigurationToken)
        {
            base.Channel.AddVideoSourceConfiguration(ProfileToken, ConfigurationToken);
        }

        public void RemoveVideoSourceConfiguration(string ProfileToken)
        {
            base.Channel.RemoveVideoSourceConfiguration(ProfileToken);
        }

        public void AddAudioEncoderConfiguration(string ProfileToken, string ConfigurationToken)
        {
            base.Channel.AddAudioEncoderConfiguration(ProfileToken, ConfigurationToken);
        }

        public void RemoveAudioEncoderConfiguration(string ProfileToken)
        {
            base.Channel.RemoveAudioEncoderConfiguration(ProfileToken);
        }

        public void AddAudioSourceConfiguration(string ProfileToken, string ConfigurationToken)
        {
            base.Channel.AddAudioSourceConfiguration(ProfileToken, ConfigurationToken);
        }

        public void RemoveAudioSourceConfiguration(string ProfileToken)
        {
            base.Channel.RemoveAudioSourceConfiguration(ProfileToken);
        }

        public void AddPTZConfiguration(string ProfileToken, string ConfigurationToken)
        {
            base.Channel.AddPTZConfiguration(ProfileToken, ConfigurationToken);
        }

        public void RemovePTZConfiguration(string ProfileToken)
        {
            base.Channel.RemovePTZConfiguration(ProfileToken);
        }

        public void AddVideoAnalyticsConfiguration(string profiletoken, string configurationtoken)
        {
            base.Channel.AddVideoAnalyticsConfiguration(profiletoken, configurationtoken);
        }

        public void RemoveVideoAnalyticsConfiguration(string ProfileToken)
        {
            base.Channel.RemoveVideoAnalyticsConfiguration(ProfileToken);
        }

        public void AddMetadataConfiguration(string ProfileToken, string ConfigurationToken)
        {
            base.Channel.AddMetadataConfiguration(ProfileToken, ConfigurationToken);
        }

        public void RemoveMetadataConfiguration(string ProfileToken)
        {
            base.Channel.RemoveMetadataConfiguration(ProfileToken);
        }

        public void AddAudioOutputConfiguration(string ProfileToken, string ConfigurationToken)
        {
            base.Channel.AddAudioOutputConfiguration(ProfileToken, ConfigurationToken);
        }

        public void RemoveAudioOutputConfiguration(string ProfileToken)
        {
            base.Channel.RemoveAudioOutputConfiguration(ProfileToken);
        }

        public void AddAudioDecoderConfiguration(string ProfileToken, string ConfigurationToken)
        {
            base.Channel.AddAudioDecoderConfiguration(ProfileToken, ConfigurationToken);
        }

        public void RemoveAudioDecoderConfiguration(string ProfileToken)
        {
            base.Channel.RemoveAudioDecoderConfiguration(ProfileToken);
        }

        public void DeleteProfile(string ProfileToken)
        {
            base.Channel.DeleteProfile(ProfileToken);
        }

        //[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public GetVideoSourceConfigurationsResponse GetVideoSourceConfigurations(GetVideoSourceConfigurationsRequest request)
            //GetVideoSourceConfigurationsResponse IMedia.GetVideoSourceConfigurations(GetVideoSourceConfigurationsRequest request)
        {
            return base.Channel.GetVideoSourceConfigurations(request);
        }

        //public GetCapabilitiesResponse GetCapabilities(GetCapabilitiesRequest request)
        //{
        //    //йа заглушко!
        //    //нужно авторизоваться при GetDeviceInformation
        //    //на Axis'ах
        //    try
        //    {
        //        return base.Channel.GetCapabilities(request);
        //    }
        //    catch (ProtocolException)
        //    {

        //    }
        //    return new GetCapabilitiesResponse();

        //}

        public GetProfilesResponse GetProfiles(GetProfilesRequest request)
        {
            return base.Channel.GetProfiles(request);
        }


        //#region
        public void UnauthorizedAccessFault()
        {
            throw new FaultException(new FaultReason("The requested operation is not permitted by the device"),
                          new FaultCode("Sender",
                              new FaultCode("NotAuthorized", "http://www.onvif.org/ver10/error",
                                  new FaultCode("Operation not Permitted", "http://www.onvif.org/ver10/error"))));
        }
       
        //[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        GetVideoEncoderConfigurationsResponse IMedia.GetVideoEncoderConfigurations(GetVideoEncoderConfigurationsRequest request)
        {
            return base.Channel.GetVideoEncoderConfigurations(request);
        }

        //public VideoEncoderConfiguration[] GetVideoEncoderConfigurations()
        //{
        //    GetVideoEncoderConfigurationsRequest inValue = new GetVideoEncoderConfigurationsRequest();
        //    GetVideoEncoderConfigurationsResponse retVal = ((IMedia)(this)).GetVideoEncoderConfigurations(inValue);
        //    return retVal.Configurations;
        //}

        //[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        GetAudioSourceConfigurationsResponse IMedia.GetAudioSourceConfigurations(GetAudioSourceConfigurationsRequest request)
        {
            return base.Channel.GetAudioSourceConfigurations(request);
        }

        //public AudioSourceConfiguration[] GetAudioSourceConfigurations()
        //{
        //    GetAudioSourceConfigurationsRequest inValue = new GetAudioSourceConfigurationsRequest();
        //    GetAudioSourceConfigurationsResponse retVal = ((IMedia)(this)).GetAudioSourceConfigurations(inValue);
        //    return retVal.Configurations;
        //}

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        GetAudioEncoderConfigurationsResponse IMedia.GetAudioEncoderConfigurations(GetAudioEncoderConfigurationsRequest request)
        {
            return base.Channel.GetAudioEncoderConfigurations(request);
        }

        //public AudioEncoderConfiguration[] GetAudioEncoderConfigurations()
        //{
        //    GetAudioEncoderConfigurationsRequest inValue = new GetAudioEncoderConfigurationsRequest();
        //    GetAudioEncoderConfigurationsResponse retVal = ((IMedia)(this)).GetAudioEncoderConfigurations(inValue);
        //    return retVal.Configurations;
        //}

        //[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        GetVideoAnalyticsConfigurationsResponse IMedia.GetVideoAnalyticsConfigurations(GetVideoAnalyticsConfigurationsRequest request)
        {
            return base.Channel.GetVideoAnalyticsConfigurations(request);
        }

        //public VideoAnalyticsConfiguration[] GetVideoAnalyticsConfigurations()
        //{
        //    GetVideoAnalyticsConfigurationsRequest inValue = new GetVideoAnalyticsConfigurationsRequest();
        //    GetVideoAnalyticsConfigurationsResponse retVal = ((IMedia)(this)).GetVideoAnalyticsConfigurations(inValue);
        //    return retVal.Configurations;
        //}

        //[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        GetMetadataConfigurationsResponse IMedia.GetMetadataConfigurations(GetMetadataConfigurationsRequest request)
        {
            return base.Channel.GetMetadataConfigurations(request);
        }

        //public MetadataConfiguration[] GetMetadataConfigurations()
        //{
        //    GetMetadataConfigurationsRequest inValue = new GetMetadataConfigurationsRequest();
        //    GetMetadataConfigurationsResponse retVal = ((IMedia)(this)).GetMetadataConfigurations(inValue);
        //    return retVal.Configurations;
        //}

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        GetAudioOutputConfigurationsResponse IMedia.GetAudioOutputConfigurations(GetAudioOutputConfigurationsRequest request)
        {
            return base.Channel.GetAudioOutputConfigurations(request);
        }

        //public AudioOutputConfiguration[] GetAudioOutputConfigurations()
        //{
        //    GetAudioOutputConfigurationsRequest inValue = new GetAudioOutputConfigurationsRequest();
        //    GetAudioOutputConfigurationsResponse retVal = ((IMedia)(this)).GetAudioOutputConfigurations(inValue);
        //    return retVal.Configurations;
        //}

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        GetAudioDecoderConfigurationsResponse IMedia.GetAudioDecoderConfigurations(GetAudioDecoderConfigurationsRequest request)
        {
            return base.Channel.GetAudioDecoderConfigurations(request);
        }

        public AudioDecoderConfiguration[] GetAudioDecoderConfigurations()
        {
            GetAudioDecoderConfigurationsRequest inValue = new GetAudioDecoderConfigurationsRequest();
            GetAudioDecoderConfigurationsResponse retVal = ((IMedia)(this)).GetAudioDecoderConfigurations(inValue);
            return retVal.Configurations;
        }

        public VideoSourceConfiguration GetVideoSourceConfiguration(string ConfigurationToken)
        {
            return base.Channel.GetVideoSourceConfiguration(ConfigurationToken);
        }

        public VideoEncoderConfiguration GetVideoEncoderConfiguration(string ConfigurationToken)
        {
            return base.Channel.GetVideoEncoderConfiguration(ConfigurationToken);
        }

        public AudioSourceConfiguration GetAudioSourceConfiguration(string ConfigurationToken)
        {
            return base.Channel.GetAudioSourceConfiguration(ConfigurationToken);
        }

        public AudioEncoderConfiguration GetAudioEncoderConfiguration(string ConfigurationToken)
        {
            return base.Channel.GetAudioEncoderConfiguration(ConfigurationToken);
        }

        public VideoAnalyticsConfiguration GetVideoAnalyticsConfiguration(string ConfigurationToken)
        {
            return base.Channel.GetVideoAnalyticsConfiguration(ConfigurationToken);
        }

        public MetadataConfiguration GetMetadataConfiguration(string ConfigurationToken)
        {
            return base.Channel.GetMetadataConfiguration(ConfigurationToken);
        }

        public AudioOutputConfiguration GetAudioOutputConfiguration(string ConfigurationToken)
        {
            return base.Channel.GetAudioOutputConfiguration(ConfigurationToken);
        }

        public AudioDecoderConfiguration GetAudioDecoderConfiguration(string ConfigurationToken)
        {
            return base.Channel.GetAudioDecoderConfiguration(ConfigurationToken);
        }

        //[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public GetCompatibleVideoEncoderConfigurationsResponse GetCompatibleVideoEncoderConfigurations(GetCompatibleVideoEncoderConfigurationsRequest request)
        {
            return base.Channel.GetCompatibleVideoEncoderConfigurations(request);
        }

        //public VideoEncoderConfiguration[] GetCompatibleVideoEncoderConfigurations(string ProfileToken)
        //{
        //    GetCompatibleVideoEncoderConfigurationsRequest inValue = new GetCompatibleVideoEncoderConfigurationsRequest();
        //    inValue.ProfileToken = ProfileToken;
        //    GetCompatibleVideoEncoderConfigurationsResponse retVal = ((IMedia)(this)).GetCompatibleVideoEncoderConfigurations(inValue);
        //    return retVal.Configurations;
        //}

        //[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public GetCompatibleVideoSourceConfigurationsResponse GetCompatibleVideoSourceConfigurations(GetCompatibleVideoSourceConfigurationsRequest request)
        {
            return base.Channel.GetCompatibleVideoSourceConfigurations(request);
        }

        //public VideoSourceConfiguration[] GetCompatibleVideoSourceConfigurations(string ProfileToken)
        //{
        //    GetCompatibleVideoSourceConfigurationsRequest inValue = new GetCompatibleVideoSourceConfigurationsRequest();
        //    inValue.ProfileToken = ProfileToken;
        //    GetCompatibleVideoSourceConfigurationsResponse retVal = ((IMedia)(this)).GetCompatibleVideoSourceConfigurations(inValue);
        //    return retVal.Configurations;
        //}

        //[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public GetCompatibleAudioEncoderConfigurationsResponse GetCompatibleAudioEncoderConfigurations(GetCompatibleAudioEncoderConfigurationsRequest request)
        {
            return base.Channel.GetCompatibleAudioEncoderConfigurations(request);
        }

        //public AudioEncoderConfiguration[] GetCompatibleAudioEncoderConfigurations(string ProfileToken)
        //{
        //    GetCompatibleAudioEncoderConfigurationsRequest inValue = new GetCompatibleAudioEncoderConfigurationsRequest();
        //    inValue.ProfileToken = ProfileToken;
        //    GetCompatibleAudioEncoderConfigurationsResponse retVal = ((IMedia)(this)).GetCompatibleAudioEncoderConfigurations(inValue);
        //    return retVal.Configurations;
        //}

        //[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public GetCompatibleAudioSourceConfigurationsResponse GetCompatibleAudioSourceConfigurations(GetCompatibleAudioSourceConfigurationsRequest request)
        {
            return base.Channel.GetCompatibleAudioSourceConfigurations(request);
        }

        //public AudioSourceConfiguration[] GetCompatibleAudioSourceConfigurations(string ProfileToken)
        //{
        //    GetCompatibleAudioSourceConfigurationsRequest inValue = new GetCompatibleAudioSourceConfigurationsRequest();
        //    inValue.ProfileToken = ProfileToken;
        //    GetCompatibleAudioSourceConfigurationsResponse retVal = ((IMedia)(this)).GetCompatibleAudioSourceConfigurations(inValue);
        //    return retVal.Configurations;
        //}

        //[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public GetCompatibleVideoAnalyticsConfigurationsResponse GetCompatibleVideoAnalyticsConfigurations(GetCompatibleVideoAnalyticsConfigurationsRequest request)
        {
            return base.Channel.GetCompatibleVideoAnalyticsConfigurations(request);
        }

        //public VideoAnalyticsConfiguration[] GetCompatibleVideoAnalyticsConfigurations(string ProfileToken)
        //{
        //    GetCompatibleVideoAnalyticsConfigurationsRequest inValue = new GetCompatibleVideoAnalyticsConfigurationsRequest();
        //    inValue.ProfileToken = ProfileToken;
        //    GetCompatibleVideoAnalyticsConfigurationsResponse retVal = ((IMedia)(this)).GetCompatibleVideoAnalyticsConfigurations(inValue);
        //    return retVal.Configurations;
        //}

        //[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public GetCompatibleMetadataConfigurationsResponse GetCompatibleMetadataConfigurations(GetCompatibleMetadataConfigurationsRequest request)
        {
            return base.Channel.GetCompatibleMetadataConfigurations(request);
        }

        //public MetadataConfiguration[] GetCompatibleMetadataConfigurations(string ProfileToken)
        //{
        //    GetCompatibleMetadataConfigurationsRequest inValue = new GetCompatibleMetadataConfigurationsRequest();
        //    inValue.ProfileToken = ProfileToken;
        //    GetCompatibleMetadataConfigurationsResponse retVal = ((IMedia)(this)).GetCompatibleMetadataConfigurations(inValue);
        //    return retVal.Configurations;
        //}

        //[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public GetCompatibleAudioOutputConfigurationsResponse GetCompatibleAudioOutputConfigurations(GetCompatibleAudioOutputConfigurationsRequest request)
        {
            return base.Channel.GetCompatibleAudioOutputConfigurations(request);
        }

        //public AudioOutputConfiguration[] GetCompatibleAudioOutputConfigurations(string ProfileToken)
        //{
        //    GetCompatibleAudioOutputConfigurationsRequest inValue = new GetCompatibleAudioOutputConfigurationsRequest();
        //    inValue.ProfileToken = ProfileToken;
        //    GetCompatibleAudioOutputConfigurationsResponse retVal = ((IMedia)(this)).GetCompatibleAudioOutputConfigurations(inValue);
        //    return retVal.Configurations;
        //}

        //[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public GetCompatibleAudioDecoderConfigurationsResponse GetCompatibleAudioDecoderConfigurations(GetCompatibleAudioDecoderConfigurationsRequest request)
        {
            return base.Channel.GetCompatibleAudioDecoderConfigurations(request);
        }

        //public AudioDecoderConfiguration[] GetCompatibleAudioDecoderConfigurations(string ProfileToken)
        //{
        //    GetCompatibleAudioDecoderConfigurationsRequest inValue = new GetCompatibleAudioDecoderConfigurationsRequest();
        //    inValue.ProfileToken = ProfileToken;
        //    GetCompatibleAudioDecoderConfigurationsResponse retVal = ((IMedia)(this)).GetCompatibleAudioDecoderConfigurations(inValue);
        //    return retVal.Configurations;
        //}

        public void SetVideoSourceConfiguration(VideoSourceConfiguration Configuration, bool ForcePersistence)
        {
            base.Channel.SetVideoSourceConfiguration(Configuration, ForcePersistence);
        }

        public void SetVideoEncoderConfiguration(VideoEncoderConfiguration Configuration, bool ForcePersistence)
        {
            base.Channel.SetVideoEncoderConfiguration(Configuration, ForcePersistence);
        }

        public void SetAudioSourceConfiguration(AudioSourceConfiguration Configuration, bool ForcePersistence)
        {
            base.Channel.SetAudioSourceConfiguration(Configuration, ForcePersistence);
        }

        public void SetAudioEncoderConfiguration(AudioEncoderConfiguration Configuration, bool ForcePersistence)
        {
            base.Channel.SetAudioEncoderConfiguration(Configuration, ForcePersistence);
        }

        public void SetVideoAnalyticsConfiguration(VideoAnalyticsConfiguration Configuration, bool ForcePersistence)
        {
            base.Channel.SetVideoAnalyticsConfiguration(Configuration, ForcePersistence);
        }

        public void SetMetadataConfiguration(MetadataConfiguration Configuration, bool ForcePersistence)
        {
            base.Channel.SetMetadataConfiguration(Configuration, ForcePersistence);
        }

        public void SetAudioOutputConfiguration(AudioOutputConfiguration Configuration, bool ForcePersistence)
        {
            base.Channel.SetAudioOutputConfiguration(Configuration, ForcePersistence);
        }

        public void SetAudioDecoderConfiguration(AudioDecoderConfiguration Configuration, bool ForcePersistence)
        {
            base.Channel.SetAudioDecoderConfiguration(Configuration, ForcePersistence);
        }

        public VideoSourceConfigurationOptions GetVideoSourceConfigurationOptions(string ConfigurationToken, string ProfileToken)
        {
            return base.Channel.GetVideoSourceConfigurationOptions(ConfigurationToken, ProfileToken);
        }

        public VideoEncoderConfigurationOptions GetVideoEncoderConfigurationOptions(string ConfigurationToken, string ProfileToken)
        {
            return base.Channel.GetVideoEncoderConfigurationOptions(ConfigurationToken, ProfileToken);
        }

        public AudioSourceConfigurationOptions GetAudioSourceConfigurationOptions(string ConfigurationToken, string ProfileToken)
        {
            return base.Channel.GetAudioSourceConfigurationOptions(ConfigurationToken, ProfileToken);
        }

        public AudioEncoderConfigurationOptions GetAudioEncoderConfigurationOptions(string ConfigurationToken, string ProfileToken)
        {
            return base.Channel.GetAudioEncoderConfigurationOptions(ConfigurationToken, ProfileToken);
        }

        public MetadataConfigurationOptions GetMetadataConfigurationOptions(string ConfigurationToken, string ProfileToken)
        {
            return base.Channel.GetMetadataConfigurationOptions(ConfigurationToken, ProfileToken);
        }

        public AudioOutputConfigurationOptions GetAudioOutputConfigurationOptions(string ConfigurationToken, string ProfileToken)
        {
            return base.Channel.GetAudioOutputConfigurationOptions(ConfigurationToken, ProfileToken);
        }

        public AudioDecoderConfigurationOptions GetAudioDecoderConfigurationOptions(string ConfigurationToken, string ProfileToken)
        {
            return base.Channel.GetAudioDecoderConfigurationOptions(ConfigurationToken, ProfileToken);
        }

        public int GetGuaranteedNumberOfVideoEncoderInstances(out int JPEG, out int H264, out int MPEG4, string ConfigurationToken)
        {
            return base.Channel.GetGuaranteedNumberOfVideoEncoderInstances(out JPEG, out H264, out MPEG4, ConfigurationToken);
        }

        public MediaUri GetStreamUri(StreamSetup StreamSetup, string ProfileToken)
        {
            return base.Channel.GetStreamUri(StreamSetup, ProfileToken);
        }

        public void StartMulticastStreaming(string ProfileToken)
        {
            base.Channel.StartMulticastStreaming(ProfileToken);
        }

        public void StopMulticastStreaming(string ProfileToken)
        {
            base.Channel.StopMulticastStreaming(ProfileToken);
        }

        public void SetSynchronizationPoint(string ProfileToken)
        {
            base.Channel.SetSynchronizationPoint(ProfileToken);
        }
        public Media.AudioOutput[] GetAudioOutputs()
        {
            GetAudioOutputsRequest inValue = new GetAudioOutputsRequest();
            GetAudioOutputsResponse retVal = ((IMedia)(this)).GetAudioOutputs(inValue);
            return retVal.AudioOutputs;
        }

        public Profile CreateProfile(string Name, string Token)
        {
            return base.Channel.CreateProfile(Name, Token);
        }

        public Profile GetProfile(string ProfileToken)
        {
            return base.Channel.GetProfile(ProfileToken);
        }

        GetAudioOutputsResponse IMedia.GetAudioOutputs(GetAudioOutputsRequest request)
        {
            return base.Channel.GetAudioOutputs(request);
        }

        GetAudioSourcesResponse IMedia.GetAudioSources(GetAudioSourcesRequest request)
        {
            return base.Channel.GetAudioSources(request);
        }
        public GetVideoSourcesResponse GetVideoSources(GetVideoSourcesRequest request)
        {
            return base.Channel.GetVideoSources(request);
        }
    }

    public class NVTClient : IDisposable
    {
        public NVTDeviceServiceClient DeviceClient;
        public NVTMediaServiceClient MediaClient;

        private bool _isDisposed = false;


        public NVTClient(Binding binding, EndpointAddress remoteAddress)
        {
            DeviceClient = new NVTDeviceServiceClient(binding, remoteAddress);
            MediaClient = new NVTMediaServiceClient(binding, remoteAddress);
        }

        public NVTClient(Binding binding, EndpointAddress remoteAddress, string username, string password)
        {
            //var security = TransportSecurityBindingElement.CreateUserNameOverTransportBindingElement();
            //security.IncludeTimestamp = false;
            //security.DefaultAlgorithmSuite = SecurityAlgorithmSuite.Basic256;
            //security.MessageSecurityVersion = MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;
            //var encoding = new TextMessageEncodingBindingElement();
            //encoding.MessageVersion = MessageVersion.Soap11;

            //var transport = new HttpTransportBindingElement();
            //transport.MaxReceivedMessageSize = 20000000; // 20 megs
            
            //binding = new CustomBinding(encoding, security, transport);


            var security = TransportSecurityBindingElement.CreateUserNameOverTransportBindingElement();
            security.AllowInsecureTransport = true;
            security.IncludeTimestamp = false;
            security.DefaultAlgorithmSuite = SecurityAlgorithmSuite.Basic256;
            //security.MessageSecurityVersion = MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;
            security.MessageSecurityVersion = MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;

            var encoding = new TextMessageEncodingBindingElement();
            //encoding.MessageVersion = MessageVersion.Soap11;
            encoding.MessageVersion = MessageVersion.Soap12;

            var transport = new HttpTransportBindingElement();
            transport.MaxReceivedMessageSize = 20000000; // 20 megs

            var cBinding = new CustomBinding();//encoding, security, transport);
            cBinding.Elements.Add(security);
            cBinding.Elements.Add(encoding);
            cBinding.Elements.Add(transport);


            DeviceClient = new NVTDeviceServiceClient(cBinding, remoteAddress);
            MediaClient = new NVTMediaServiceClient(cBinding, remoteAddress);

            DeviceClient.ChannelFactory.Endpoint.Behaviors.Remove<System.ServiceModel.Description.ClientCredentials>();
            DeviceClient.ChannelFactory.Endpoint.Behaviors.Add(new CustomCredentials());

            DeviceClient.ClientCredentials.UserName.UserName = username;
            DeviceClient.ClientCredentials.UserName.Password = password;

            MediaClient.ChannelFactory.Endpoint.Behaviors.Remove<System.ServiceModel.Description.ClientCredentials>();
            MediaClient.ChannelFactory.Endpoint.Behaviors.Add(new CustomCredentials());

            MediaClient.ClientCredentials.UserName.UserName = username;
            MediaClient.ClientCredentials.UserName.Password = password;

        }

        //----implementing IDisposable-------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);//чтобы при ошибке не вывалиться в деструктор
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (this._isDisposed)
                return;

            if (isDisposing)
            {
                // Release only managed resources.
                DeviceClient.Close();
                MediaClient.Close();
            }
            // Always release unmanaged resources here.

            // Indicate that the object has been disposed.
            this._isDisposed = true;
        }

        ~NVTClient()
        {
            Dispose(false);
        }
        //-----------------------------------------------
    }


    public class CustomCredentials : ClientCredentials
    {
        public CustomCredentials()
        { }

        protected CustomCredentials(CustomCredentials cc)
            : base(cc)
        { }

        public override System.IdentityModel.Selectors.SecurityTokenManager CreateSecurityTokenManager()
        {
            return new CustomSecurityTokenManager(this);
        }

        protected override ClientCredentials CloneCore()
        {
            return new CustomCredentials(this);
        }
    }

    public class CustomSecurityTokenManager : ClientCredentialsSecurityTokenManager
    {
        public CustomSecurityTokenManager(CustomCredentials cred)
            : base(cred)
        { }

        public override System.IdentityModel.Selectors.SecurityTokenSerializer CreateSecurityTokenSerializer(System.IdentityModel.Selectors.SecurityTokenVersion version)
        {
            return new CustomTokenSerializer(System.ServiceModel.Security.SecurityVersion.WSSecurity11);
        }
    }

    public class CustomTokenSerializer : WSSecurityTokenSerializer
    {
        public CustomTokenSerializer(SecurityVersion sv)
            : base(sv)
        { }
        #region
        //protected override void WriteTokenCore(System.Xml.XmlWriter writer,
        //                                        System.IdentityModel.Tokens.SecurityToken token)
        //{
        //    UserNameSecurityToken userToken = token as UserNameSecurityToken;

        //    string tokennamespace = "o";

        //    var created = System.DateTime.Now;
        //    string createdStr = created.ToString("yyyy-MM-ddThh:mm:ss.fffZ");

        //    // unique Nonce value - encode with SHA-1 for 'randomness'
        //    // in theory the nonce could just be the GUID by itself
        //    string phrase = Guid.NewGuid().ToString();
        //    var nonce = GetSHA1String(phrase);

        //    // in this case password is plain text
        //    // for digest mode password needs to be encoded as:
        //    // PasswordAsDigest = Base64(SHA-1(Nonce + Created + Password))
        //    // and profile needs to change to
        //    //string password = GetSHA1String(nonce + createdStr + userToken.Password);

        //    string password = userToken.Password;

        //    writer.WriteRaw(string.Format(
        //    "<{0}:UsernameToken u:Id=\"" + token.Id +
        //    "\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">" +
        //    "<{0}:Username>" + userToken.UserName + "</{0}:Username>" +
        //    "<{0}:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText\">" +
        //    password + "</{0}:Password>" +
        //    "<{0}:Nonce EncodingType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\">" +
        //    nonce + "</{0}:Nonce>" +
        //    "<u:Created>" + createdStr + "</u:Created></{0}:UsernameToken>", tokennamespace));
        //}
        #endregion

        protected override void WriteTokenCore(System.Xml.XmlWriter writer,
                                        System.IdentityModel.Tokens.SecurityToken token)
        {
            UserNameSecurityToken userToken = token as UserNameSecurityToken;

            string tokennamespace = "o";
            var created = System.DateTime.UtcNow;
            string createdStr = created.ToString("yyyy-MM-ddTHH:mm:ssZ");

            // unique Nonce value - encode with SHA-1 for 'randomness'
            // in theory the nonce could just be the GUID by itself
            string phrase = Guid.NewGuid().ToString();
            var nonce = GetSHA1String(phrase);
            string password = GetPasswordDigest(nonce, createdStr, userToken.Password); 
            
            writer.WriteRaw(string.Format(
            "<{0}:UsernameToken " +
            " xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">" +
            "<{0}:Username>" + userToken.UserName + "</{0}:Username>" +
            "<{0}:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest\">" +
            password + "</{0}:Password>" +
            "<{0}:Nonce>" +
            nonce + "</{0}:Nonce>" +
            "<u:Created>" + createdStr + "</u:Created></{0}:UsernameToken>", tokennamespace));
        }

        protected string GetSHA1String(string phrase)
        {
            SHA1CryptoServiceProvider sha1Hasher = new SHA1CryptoServiceProvider();
            byte[] hashedDataBytes = sha1Hasher.ComputeHash(Encoding.UTF8.GetBytes(phrase));
            return Convert.ToBase64String(hashedDataBytes);
        }

        public static string GetPasswordDigest(string nonce, string created, string pass)
        {
            byte[] bNonce = Convert.FromBase64String(nonce);
            byte[] bCreated = Encoding.UTF8.GetBytes(created);
            byte[] bPass = Encoding.UTF8.GetBytes(pass);
            byte[] bAll = new byte[bNonce.Length + bCreated.Length + bPass.Length];

            Buffer.BlockCopy(bNonce, 0, bAll, 0, bNonce.Length);
            Buffer.BlockCopy(bCreated, 0, bAll, bNonce.Length, bCreated.Length);
            Buffer.BlockCopy(bPass, 0, bAll, bNonce.Length + bCreated.Length, bPass.Length);

            return Sha1Base64Digest(bAll);
        }

        public static String Sha1Base64Digest(byte[] phrase)
        {
            SHA1CryptoServiceProvider sha1Hasher = new SHA1CryptoServiceProvider();
            byte[] hashedDataBytes = sha1Hasher.ComputeHash(phrase);
            return Convert.ToBase64String(hashedDataBytes);
        }

    }
}
