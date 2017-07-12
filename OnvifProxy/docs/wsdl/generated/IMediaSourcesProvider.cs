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
[System.ServiceModel.ServiceContractAttribute(Namespace="urn:ias:cvss:msp:1.0", ConfigurationName="MediaSourcesProvider")]
public interface MediaSourcesProvider
{
    
    // CODEGEN: Generating message contract since message part namespace (http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd) does not match the default value (urn:ias:cvss:msp:1.0)
    [System.ServiceModel.OperationContractAttribute(Action="urn:ias:cvss:msp:1.0/GetMediaSources", ReplyAction="*")]
    [System.ServiceModel.XmlSerializerFormatAttribute()]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(EndpointReferenceType))]
    [return: System.ServiceModel.MessageParameterAttribute(Name="NextStartReference")]
    GetMediaSourcesResponse GetMediaSources(GetMediaSourcesRequest request);
    
    // CODEGEN: Parameter 'KeepAliveTime' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'System.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action="urn:ias:cvss:msp:1.0/FindMediaSources", ReplyAction="*")]
    [System.ServiceModel.XmlSerializerFormatAttribute()]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(EndpointReferenceType))]
    [return: System.ServiceModel.MessageParameterAttribute(Name="SearchToken")]
    FindMediaSourcesResponse FindMediaSources(FindMediaSourcesRequest request);
    
    // CODEGEN: Parameter 'WaitTime' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'System.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action="urn:ias:cvss:msp:1.0/GetSearchResults", ReplyAction="*")]
    [System.ServiceModel.XmlSerializerFormatAttribute()]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(EndpointReferenceType))]
    [return: System.ServiceModel.MessageParameterAttribute(Name="SearchState")]
    GetSearchResultsResponse GetSearchResults(GetSearchResultsRequest request);
    
    // CODEGEN: Generating message contract since message part namespace (http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd) does not match the default value (urn:ias:cvss:msp:1.0)
    [System.ServiceModel.OperationContractAttribute(Action="urn:ias:cvss:msp:1.0/GetUpdates", ReplyAction="*")]
    [System.ServiceModel.XmlSerializerFormatAttribute()]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(EndpointReferenceType))]
    GetUpdatesResponse GetUpdates(GetUpdatesRequest request);
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd" +
    "")]
public partial class SecurityHeaderType
{
    
    private System.Xml.XmlElement[] anyField;
    
    private System.Xml.XmlAttribute[] anyAttrField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute(Order=0)]
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
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:ias:cvss:msp:1.0")]
public partial class UpdateType
{
    
    private string mediaSourceTokenField;
    
    private MediaSourceType mediaSourceField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order=0)]
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
    [System.Xml.Serialization.XmlElementAttribute(Order=1)]
    public MediaSourceType MediaSource
    {
        get
        {
            return this.mediaSourceField;
        }
        set
        {
            this.mediaSourceField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:ias:cvss:msp:1.0")]
public partial class MediaSourceType
{
    
    private ONVIFBindingType oNVIFBindingField;
    
    private NameType[] nameField;
    
    private DescriptionType[] descriptionField;
    
    private GeoCircle locationField;
    
    private GeoPolygon viewAreaField;
    
    private string tokenField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order=0)]
    public ONVIFBindingType ONVIFBinding
    {
        get
        {
            return this.oNVIFBindingField;
        }
        set
        {
            this.oNVIFBindingField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Name", Order=1)]
    public NameType[] Name
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
    [System.Xml.Serialization.XmlElementAttribute("Description", Order=2)]
    public DescriptionType[] Description
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
    public GeoCircle Location
    {
        get
        {
            return this.locationField;
        }
        set
        {
            this.locationField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order=4)]
    public GeoPolygon ViewArea
    {
        get
        {
            return this.viewAreaField;
        }
        set
        {
            this.viewAreaField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string token
    {
        get
        {
            return this.tokenField;
        }
        set
        {
            this.tokenField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:ias:cvss:msp:1.0")]
public partial class ONVIFBindingType
{
    
    private EndpointType[] endpointField;
    
    private string mediaSourceTokenField;
    
    private string profileTokenField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Endpoint", Order=0)]
    public EndpointType[] Endpoint
    {
        get
        {
            return this.endpointField;
        }
        set
        {
            this.endpointField = value;
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
    public string ProfileToken
    {
        get
        {
            return this.profileTokenField;
        }
        set
        {
            this.profileTokenField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:ias:cvss:msp:1.0")]
public partial class EndpointType : EndpointReferenceType
{
    
    private string[] purposeField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string[] purpose
    {
        get
        {
            return this.purposeField;
        }
        set
        {
            this.purposeField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlIncludeAttribute(typeof(EndpointType))]
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.w3.org/2005/08/addressing")]
public partial class EndpointReferenceType
{
    
    private AttributedURIType addressField;
    
    private ReferenceParametersType referenceParametersField;
    
    private MetadataType metadataField;
    
    private System.Xml.XmlElement[] anyField;
    
    private System.Xml.XmlAttribute[] anyAttrField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order=0)]
    public AttributedURIType Address
    {
        get
        {
            return this.addressField;
        }
        set
        {
            this.addressField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order=1)]
    public ReferenceParametersType ReferenceParameters
    {
        get
        {
            return this.referenceParametersField;
        }
        set
        {
            this.referenceParametersField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order=2)]
    public MetadataType Metadata
    {
        get
        {
            return this.metadataField;
        }
        set
        {
            this.metadataField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute(Order=3)]
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
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.w3.org/2005/08/addressing")]
public partial class AttributedURIType
{
    
    private System.Xml.XmlAttribute[] anyAttrField;
    
    private string valueField;
    
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
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
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

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.w3.org/2005/08/addressing")]
public partial class ReferenceParametersType
{
    
    private System.Xml.XmlElement[] anyField;
    
    private System.Xml.XmlAttribute[] anyAttrField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute(Order=0)]
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
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.w3.org/2005/08/addressing")]
public partial class MetadataType
{
    
    private System.Xml.XmlElement[] anyField;
    
    private System.Xml.XmlAttribute[] anyAttrField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute(Order=0)]
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
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:ias:cvss:msp:1.0")]
public partial class NameType
{
    
    private string langField;
    
    private string valueField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified, Namespace="http://www.w3.org/XML/1998/namespace")]
    public string lang
    {
        get
        {
            return this.langField;
        }
        set
        {
            this.langField = value;
        }
    }
    
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

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:ias:cvss:msp:1.0")]
public partial class DescriptionType
{
    
    private string langField;
    
    private string valueField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified, Namespace="http://www.w3.org/XML/1998/namespace")]
    public string lang
    {
        get
        {
            return this.langField;
        }
        set
        {
            this.langField = value;
        }
    }
    
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

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:ias:cvss:1.0")]
public partial class GeoPolygon
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

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:ias:cvss:msp:1.0")]
public partial class SearchScopeType
{
    
    private System.Xml.XmlElement[] anyField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute(Order=0)]
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
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.MessageContractAttribute(WrapperName="GetMediaSources", WrapperNamespace="urn:ias:cvss:msp:1.0", IsWrapped=true)]
public partial class GetMediaSourcesRequest
{
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=0)]
    public int Limit;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=1)]
    public string StartReference;
    
    public GetMediaSourcesRequest()
    {
    }
    
    public GetMediaSourcesRequest(int Limit, string StartReference)
    {
        this.Limit = Limit;
        this.StartReference = StartReference;
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.MessageContractAttribute(WrapperName="GetMediaSourcesResponse", WrapperNamespace="urn:ias:cvss:msp:1.0", IsWrapped=true)]
public partial class GetMediaSourcesResponse
{
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=0)]
    public string NextStartReference;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=1)]
    public string UpdateToken;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd" +
        "", Order=2)]
    public SecurityHeaderType Security;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=3)]
    [System.Xml.Serialization.XmlElementAttribute("MediaSource")]
    public MediaSourceType[] MediaSource;
    
    public GetMediaSourcesResponse()
    {
    }
    
    public GetMediaSourcesResponse(string NextStartReference, string UpdateToken, SecurityHeaderType Security, MediaSourceType[] MediaSource)
    {
        this.NextStartReference = NextStartReference;
        this.UpdateToken = UpdateToken;
        this.Security = Security;
        this.MediaSource = MediaSource;
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.MessageContractAttribute(WrapperName="FindMediaSources", WrapperNamespace="urn:ias:cvss:msp:1.0", IsWrapped=true)]
public partial class FindMediaSourcesRequest
{
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=0)]
    public SearchScopeType Scope;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=1)]
    public int MaxMatches;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=2)]
    [System.Xml.Serialization.XmlElementAttribute(DataType="duration")]
    public string KeepAliveTime;
    
    public FindMediaSourcesRequest()
    {
    }
    
    public FindMediaSourcesRequest(SearchScopeType Scope, int MaxMatches, string KeepAliveTime)
    {
        this.Scope = Scope;
        this.MaxMatches = MaxMatches;
        this.KeepAliveTime = KeepAliveTime;
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.MessageContractAttribute(WrapperName="FindMediaSourcesResponse", WrapperNamespace="urn:ias:cvss:msp:1.0", IsWrapped=true)]
public partial class FindMediaSourcesResponse
{
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=0)]
    public string SearchToken;
    
    public FindMediaSourcesResponse()
    {
    }
    
    public FindMediaSourcesResponse(string SearchToken)
    {
        this.SearchToken = SearchToken;
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:ias:cvss:msp:1.0")]
public enum SearchStateEnum
{
    
    /// <remarks/>
    Searching,
    
    /// <remarks/>
    Completed,
    
    /// <remarks/>
    Unknown,
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.MessageContractAttribute(WrapperName="GetSearchResults", WrapperNamespace="urn:ias:cvss:msp:1.0", IsWrapped=true)]
public partial class GetSearchResultsRequest
{
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=0)]
    public string SearchToken;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=1)]
    public int MinResults;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=2)]
    public int MaxResults;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=3)]
    [System.Xml.Serialization.XmlElementAttribute(DataType="duration")]
    public string WaitTime;
    
    public GetSearchResultsRequest()
    {
    }
    
    public GetSearchResultsRequest(string SearchToken, int MinResults, int MaxResults, string WaitTime)
    {
        this.SearchToken = SearchToken;
        this.MinResults = MinResults;
        this.MaxResults = MaxResults;
        this.WaitTime = WaitTime;
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.MessageContractAttribute(WrapperName="GetSearchResultsResponse", WrapperNamespace="urn:ias:cvss:msp:1.0", IsWrapped=true)]
public partial class GetSearchResultsResponse
{
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=0)]
    public SearchStateEnum SearchState;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd" +
        "", Order=1)]
    public SecurityHeaderType Security;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=2)]
    [System.Xml.Serialization.XmlElementAttribute("MediaSource")]
    public MediaSourceType[] MediaSource;
    
    public GetSearchResultsResponse()
    {
    }
    
    public GetSearchResultsResponse(SearchStateEnum SearchState, SecurityHeaderType Security, MediaSourceType[] MediaSource)
    {
        this.SearchState = SearchState;
        this.Security = Security;
        this.MediaSource = MediaSource;
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.MessageContractAttribute(WrapperName="GetUpdates", WrapperNamespace="urn:ias:cvss:msp:1.0", IsWrapped=true)]
public partial class GetUpdatesRequest
{
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=0)]
    public string UpdateToken;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=1)]
    public int Limit;
    
    public GetUpdatesRequest()
    {
    }
    
    public GetUpdatesRequest(string UpdateToken, int Limit)
    {
        this.UpdateToken = UpdateToken;
        this.Limit = Limit;
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
[System.ServiceModel.MessageContractAttribute(WrapperName="GetUpdatesResponse", WrapperNamespace="urn:ias:cvss:msp:1.0", IsWrapped=true)]
public partial class GetUpdatesResponse
{
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=0)]
    public string UpdateToken;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=1)]
    public bool HasMoreUpdates;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd" +
        "", Order=2)]
    public SecurityHeaderType Security;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:ias:cvss:msp:1.0", Order=3)]
    [System.Xml.Serialization.XmlElementAttribute("Update")]
    public UpdateType[] Update;
    
    public GetUpdatesResponse()
    {
    }
    
    public GetUpdatesResponse(string UpdateToken, bool HasMoreUpdates, SecurityHeaderType Security, UpdateType[] Update)
    {
        this.UpdateToken = UpdateToken;
        this.HasMoreUpdates = HasMoreUpdates;
        this.Security = Security;
        this.Update = Update;
    }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public interface MediaSourcesProviderChannel : MediaSourcesProvider, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
public partial class MediaSourcesProviderClient : System.ServiceModel.ClientBase<MediaSourcesProvider>, MediaSourcesProvider
{
    
    public MediaSourcesProviderClient()
    {
    }
    
    public MediaSourcesProviderClient(string endpointConfigurationName) : 
            base(endpointConfigurationName)
    {
    }
    
    public MediaSourcesProviderClient(string endpointConfigurationName, string remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public MediaSourcesProviderClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public MediaSourcesProviderClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(binding, remoteAddress)
    {
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    GetMediaSourcesResponse MediaSourcesProvider.GetMediaSources(GetMediaSourcesRequest request)
    {
        return base.Channel.GetMediaSources(request);
    }
    
    public string GetMediaSources(int Limit, string StartReference, out string UpdateToken, out SecurityHeaderType Security, out MediaSourceType[] MediaSource)
    {
        GetMediaSourcesRequest inValue = new GetMediaSourcesRequest();
        inValue.Limit = Limit;
        inValue.StartReference = StartReference;
        GetMediaSourcesResponse retVal = ((MediaSourcesProvider)(this)).GetMediaSources(inValue);
        UpdateToken = retVal.UpdateToken;
        Security = retVal.Security;
        MediaSource = retVal.MediaSource;
        return retVal.NextStartReference;
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    FindMediaSourcesResponse MediaSourcesProvider.FindMediaSources(FindMediaSourcesRequest request)
    {
        return base.Channel.FindMediaSources(request);
    }
    
    public string FindMediaSources(SearchScopeType Scope, int MaxMatches, string KeepAliveTime)
    {
        FindMediaSourcesRequest inValue = new FindMediaSourcesRequest();
        inValue.Scope = Scope;
        inValue.MaxMatches = MaxMatches;
        inValue.KeepAliveTime = KeepAliveTime;
        FindMediaSourcesResponse retVal = ((MediaSourcesProvider)(this)).FindMediaSources(inValue);
        return retVal.SearchToken;
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    GetSearchResultsResponse MediaSourcesProvider.GetSearchResults(GetSearchResultsRequest request)
    {
        return base.Channel.GetSearchResults(request);
    }
    
    public SearchStateEnum GetSearchResults(string SearchToken, int MinResults, int MaxResults, string WaitTime, out SecurityHeaderType Security, out MediaSourceType[] MediaSource)
    {
        GetSearchResultsRequest inValue = new GetSearchResultsRequest();
        inValue.SearchToken = SearchToken;
        inValue.MinResults = MinResults;
        inValue.MaxResults = MaxResults;
        inValue.WaitTime = WaitTime;
        GetSearchResultsResponse retVal = ((MediaSourcesProvider)(this)).GetSearchResults(inValue);
        Security = retVal.Security;
        MediaSource = retVal.MediaSource;
        return retVal.SearchState;
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    GetUpdatesResponse MediaSourcesProvider.GetUpdates(GetUpdatesRequest request)
    {
        return base.Channel.GetUpdates(request);
    }
    
    public bool GetUpdates(ref string UpdateToken, int Limit, out SecurityHeaderType Security, out UpdateType[] Update)
    {
        GetUpdatesRequest inValue = new GetUpdatesRequest();
        inValue.UpdateToken = UpdateToken;
        inValue.Limit = Limit;
        GetUpdatesResponse retVal = ((MediaSourcesProvider)(this)).GetUpdates(inValue);
        UpdateToken = retVal.UpdateToken;
        Security = retVal.Security;
        Update = retVal.Update;
        return retVal.HasMoreUpdates;
    }
}