//  Copyright (c) Microsoft Corporation.  All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace OnvifProxy
{
    class DispatchByBodyElementOperationSelector : IDispatchOperationSelector
    {
        Dictionary<XmlQualifiedName, string> dispatchDictionary;
        string defaultOperationName;

        public DispatchByBodyElementOperationSelector(Dictionary<XmlQualifiedName, string> dispatchDictionary, string defaultOperationName)
        {
            this.dispatchDictionary = dispatchDictionary;
            this.defaultOperationName = defaultOperationName;
        }

        #region IDispatchOperationSelector Members

        private Message CreateMessageCopy(Message message, XmlDictionaryReader body)
        {
            Message copy;

            try
            {
                copy = Message.CreateMessage(message.Version, message.Headers.Action, body);
                //copy = Message.CreateMessage(message.Version, null, body);//19.08
            }
            catch (ArgumentNullException e)
            {
                throw e;
            }
            if (message.Headers.Action == null)
            {
                message.Headers.Action = body.LocalName;
                copy.Headers.CopyHeaderFrom(message, 0);///!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //
                //copy.Headers.Action = "http://www.onvif.org/ver10/device/wsdl/" + body.LocalName;
                copy.Headers.Action = body.NamespaceURI + "/" + body.LocalName;

                copy.Properties.CopyProperties(message.Properties);
                //Console.WriteLine(copy.Headers.Action.ToString());

                //return copy;
            }
            //return message;
            //Console.WriteLine("------------");
            //Console.WriteLine(copy.Headers.Action.ToString());
            return copy;
        }



        public string SelectOperation(ref System.ServiceModel.Channels.Message message)
        {
            XmlDictionaryReader bodyReader = message.GetReaderAtBodyContents();
            XmlQualifiedName lookupQName = new XmlQualifiedName(bodyReader.LocalName, bodyReader.NamespaceURI);
            message = CreateMessageCopy(message, bodyReader);
            if (dispatchDictionary.ContainsKey(lookupQName))
            {
                return dispatchDictionary[lookupQName];
            }
            else
            {
                return defaultOperationName;
            }
        }

        #endregion
    }
}