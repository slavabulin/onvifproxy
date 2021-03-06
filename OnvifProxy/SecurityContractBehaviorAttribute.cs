﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

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
    sealed class SecurityContractBehaviorAttribute : Attribute, IContractBehavior
    {
        #region IContractBehavior Members

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            // no binding parameters need to be set here
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
            Dictionary<string, List<XmlQualifiedName>> dispatchDictionary = new Dictionary<string, List<XmlQualifiedName>>();

            SecurityOperationBehavoirAttribute securityOperationBehavoirAttribute =
                    contractDescription.Operations[0].Behaviors.Find<SecurityOperationBehavoirAttribute>();
            if (securityOperationBehavoirAttribute != null && securityOperationBehavoirAttribute.QName != null)
            {
                dispatchDictionary = securityOperationBehavoirAttribute.QName;
                dispatchRuntime.OperationSelector =
                   new SecurityOperationSelector(
                      dispatchDictionary,
                      dispatchRuntime.UnhandledDispatchOperation.Name
                      );
            }
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }

        #endregion
    }

}