//  Copyright (c) Microsoft Corporation.  All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Description;
using System.Xml;
using System.ServiceModel.Dispatcher;

namespace OnvifProxy
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    sealed class DispatchByBodyElementBehaviorAttribute : Attribute, IContractBehavior
    {
        #region IContractBehavior Members

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            //System.ServiceModel.Channels.HttpRequestMessageProperty.Name.
            // no binding parameters need to be set here
            //TyphoonCom.log.Debug("AddBindingParameters");
            return;
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            // this is a dispatch-side behavior which doesn't require
            // any action on the client
            return;
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime)
        {
            Dictionary<XmlQualifiedName,string> dispatchDictionary = 
                             new Dictionary<XmlQualifiedName,string>();
            foreach( OperationDescription operationDescription in 
                                      contractDescription.Operations )
            {
               DispatchBodyElementAttribute dispatchBodyElement = 
           operationDescription.Behaviors.Find<DispatchBodyElementAttribute>();
                if (dispatchBodyElement != null )
                {
                     dispatchDictionary.Add(dispatchBodyElement.QName, 
                                      operationDescription.Name);
                     dispatchRuntime.OperationSelector =
                     new DispatchByBodyElementOperationSelector(
                        dispatchDictionary,
                        dispatchRuntime.UnhandledDispatchOperation.Name);
                     //break;
                }
            }
            
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
            // 
        }

        #endregion
    }
}