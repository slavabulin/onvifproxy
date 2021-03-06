﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.5420
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------



[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(Namespace="urn:ias:cvss:ri:1.0", ConfigurationName="RecordingImporter")]
public interface RecordingImporter
{
    
    // CODEGEN: Parameter 'UploadUri' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'System.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action="urn:ias:cvss:ri:1.0/GetUploadUri", ReplyAction="urn:ias:cvss:ri:1.0/GetUploadUriResponse")]
    [System.ServiceModel.XmlSerializerFormatAttribute()]
    [return: System.ServiceModel.MessageParameterAttribute(Name="UploadUri")]
    GetUploadUriResponse GetUploadUri(GetUploadUriRequest request);
    
    [System.ServiceModel.OperationContractAttribute(Action="urn:ias:cvss:ri:1.0/ImportRecording", ReplyAction="urn:ias:cvss:ri:1.0/ImportRecordingResponse")]
    [System.ServiceModel.XmlSerializerFormatAttribute()]
    void ImportRecording(RecordingInformation Recording);
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.MessageContractAttribute(WrapperName="GetUploadUri", WrapperNamespace="urn:ias:cvss:ri:1.0", IsWrapped=true)]
public partial class GetUploadUriRequest
{
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:ri:1.0", Order=0)]
    public int DataSize;
    
    public GetUploadUriRequest()
    {
    }
    
    public GetUploadUriRequest(int DataSize)
    {
        this.DataSize = DataSize;
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.MessageContractAttribute(WrapperName="GetUploadUriResponse", WrapperNamespace="urn:ias:cvss:ri:1.0", IsWrapped=true)]
public partial class GetUploadUriResponse
{
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:ri:1.0", Order=0)]
    [System.Xml.Serialization.XmlElementAttribute("UploadUri", DataType="anyURI")]
    public string[] UploadUri;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:ri:1.0", Order=1)]
    [System.Xml.Serialization.XmlElementAttribute(DataType="anyURI")]
    public string FileUri;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:ri:1.0", Order=2)]
    public System.DateTime InvalidationTime;
    
    public GetUploadUriResponse()
    {
    }
    
    public GetUploadUriResponse(string[] UploadUri, string FileUri, System.DateTime InvalidationTime)
    {
        this.UploadUri = UploadUri;
        this.FileUri = FileUri;
        this.InvalidationTime = InvalidationTime;
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:ias:cvss:ri:1.0")]
public partial class RecordingInformation
{
    
    private string mediaUriField;
    
    private string mediaSourceTokenField;
    
    private string nameField;
    
    private string contentField;
    
    private TrackInformation[] trackField;
    
    private object itemField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType="anyURI", Order=0)]
    public string MediaUri
    {
        get
        {
            return this.mediaUriField;
        }
        set
        {
            this.mediaUriField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order=1)]
    public string MediaSourceToken
    {
        get
        {
            return this.mediaSourceTokenField;
        }
        set
        {
            this.mediaSourceTokenField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order=2)]
    public string Name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order=3)]
    public string Content
    {
        get
        {
            return this.contentField;
        }
        set
        {
            this.contentField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Track", Order=4)]
    public TrackInformation[] Track
    {
        get
        {
            return this.trackField;
        }
        set
        {
            this.trackField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Location", typeof(GeoCircle), Order=5)]
    [System.Xml.Serialization.XmlElementAttribute("LocationUri", typeof(string), DataType="anyURI", Order=5)]
    public object Item
    {
        get
        {
            return this.itemField;
        }
        set
        {
            this.itemField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.onvif.org/ver10/schema")]
public partial class TrackInformation
{
    
    private string trackTokenField;
    
    private TrackType trackTypeField;
    
    private string descriptionField;
    
    private System.DateTime dataFromField;
    
    private System.DateTime dataToField;
    
    private System.Xml.XmlElement[] anyField;
    
    private System.Xml.XmlAttribute[] anyAttrField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order=0)]
    public string TrackToken
    {
        get
        {
            return this.trackTokenField;
        }
        set
        {
            this.trackTokenField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order=1)]
    public TrackType TrackType
    {
        get
        {
            return this.trackTypeField;
        }
        set
        {
            this.trackTypeField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order=2)]
    public string Description
    {
        get
        {
            return this.descriptionField;
        }
        set
        {
            this.descriptionField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order=3)]
    public System.DateTime DataFrom
    {
        get
        {
            return this.dataFromField;
        }
        set
        {
            this.dataFromField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order=4)]
    public System.DateTime DataTo
    {
        get
        {
            return this.dataToField;
        }
        set
        {
            this.dataToField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute(Order=5)]
    public System.Xml.XmlElement[] Any
    {
        get
        {
            return this.anyField;
        }
        set
        {
            this.anyField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
        get
        {
            return this.anyAttrField;
        }
        set
        {
            this.anyAttrField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.onvif.org/ver10/schema")]
public enum TrackType
{
    
    /// <remarks/>
    Video,
    
    /// <remarks/>
    Audio,
    
    /// <remarks/>
    Metadata,
    
    /// <remarks/>
    Extended,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:ias:cvss:1.0")]
public partial class GeoCircle
{
    
    private string valueField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public interface RecordingImporterChannel : RecordingImporter, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public partial class RecordingImporterClient : System.ServiceModel.ClientBase<RecordingImporter>, RecordingImporter
{
    
    public RecordingImporterClient()
    {
    }
    
    public RecordingImporterClient(string endpointConfigurationName) : 
            base(endpointConfigurationName)
    {
    }
    
    public RecordingImporterClient(string endpointConfigurationName, string remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public RecordingImporterClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public RecordingImporterClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(binding, remoteAddress)
    {
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    GetUploadUriResponse RecordingImporter.GetUploadUri(GetUploadUriRequest request)
    {
        return base.Channel.GetUploadUri(request);
    }
    
    public string[] GetUploadUri(int DataSize, out string FileUri, out System.DateTime InvalidationTime)
    {
        GetUploadUriRequest inValue = new GetUploadUriRequest();
        inValue.DataSize = DataSize;
        GetUploadUriResponse retVal = ((RecordingImporter)(this)).GetUploadUri(inValue);
        FileUri = retVal.FileUri;
        InvalidationTime = retVal.InvalidationTime;
        return retVal.UploadUri;
    }
    
    public void ImportRecording(RecordingInformation Recording)
    {
        base.Channel.ImportRecording(Recording);
    }
}
