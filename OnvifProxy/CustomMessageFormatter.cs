using System;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.Xml;
using System.ServiceModel.Description;


namespace OnvifProxy
{
    public class CustomClientMessageFormatter : /*IDispatchMessageFormatter, */IClientMessageFormatter
    {
        private readonly IClientMessageFormatter formatter;
        //private readonly IDispatchMessageFormatter formatter;

        //public MyCustomMessageFormatter(IDispatchMessageFormatter formatter)
        //{
        //    this.formatter = formatter;
        //}

        //public void DeserializeRequest(Message message, object[] parameters)
        //{
        //    this.formatter.DeserializeRequest(message, parameters);
        //}

        //public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        //{
        //    var message = this.formatter.SerializeReply(messageVersion, parameters, result);
        //    return new MyCustomMessage(message);
        //}
        //--------------------------------------
        public CustomClientMessageFormatter(IClientMessageFormatter formatter)
        {
            this.formatter = formatter;
        }

        public object DeserializeReply(Message message, object[] parameters)
        {
            return this.formatter.DeserializeReply(message, parameters);
        }

        public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            var message = this.formatter.SerializeRequest(messageVersion, parameters);
            CustomMessage cmsg = new CustomMessage(message);
            return cmsg;
        }
    }

    public class CustomServiceMessageFormatter : IDispatchMessageFormatter //IClientMessageFormatter
    {
        //private readonly IClientMessageFormatter formatter;
        private readonly IDispatchMessageFormatter formatter;

        //public CustomServiceMessageFormatter(IDispatchMessageFormatter formatter)
        //{
        //    this.formatter = formatter;
        //}

        public void DeserializeRequest(Message message, object[] parameters)
        {
            this.formatter.DeserializeRequest(message, parameters);
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            var message = this.formatter.SerializeReply(messageVersion, parameters, result);
            return new CustomMessage(message);
        }
        //--------------------------------------
        public CustomServiceMessageFormatter(IDispatchMessageFormatter formatter)
        {
            this.formatter = formatter;
        }

        //public object DeserializeReply(Message message, object[] parameters)
        //{
        //    return this.formatter.DeserializeReply(message, parameters);
        //}

        //public Message SerializeRequest(MessageVersion messageVersion, object[] parameters)
        //{
        //    var message = this.formatter.SerializeRequest(messageVersion, parameters);
        //    CustomMessage cmsg = new CustomMessage(message);
        //    return cmsg;
        //}
    }

    public class CustomMessage : Message
    {
        private readonly Message message;

        public CustomMessage(Message message)
        {
            this.message = message;
        }
        public override MessageHeaders Headers
        {
            get { return this.message.Headers; }
        }
        public override MessageProperties Properties
        {
            get { return this.message.Properties; }
        }
        public override MessageVersion Version
        {
            get { return this.message.Version; }
        }
        //protected override void OnWriteStartBody(XmlDictionaryWriter writer)
        //{
        //    writer.WriteStartElement("Body", "http://www.w3.org/2003/05/soap-envelope");

        //}
        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            try
            {
                this.message.WriteBodyContents(writer);
                //TyphoonCom.log.DebugFormat("WriteBodyContents XmlDictionaryWriter - {0}", writer.ToString());
            }
            catch (Exception ex)
            {
                TyphoonCom.log.DebugFormat("OnWriteBodyContents - {0}", ex.Message);
            }
        }
        protected override void OnWriteStartEnvelope(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement("s", "Envelope", "http://www.w3.org/2003/05/soap-envelope");
            writer.WriteAttributeString("xmlns", "a", null, "http://www.w3.org/2005/08/addressing");
            writer.WriteAttributeString("xmlns", "tns1", null, "http://www.onvif.org/ver10/topics");
            writer.WriteAttributeString("xmlns", "tt", null, "http://www.onvif.org/ver10/schema");
            writer.WriteAttributeString("xmlns", "wstop", null, "http://docs.oasis-open.org/wsn/t-1");
            writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
        }
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class CustomClientMessageFormatAttribute : Attribute, IOperationBehavior
    {
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            var deserializerBehavior = operationDescription.Behaviors.Find<XmlSerializerOperationBehavior>();

            if (clientOperation.Formatter == null)
            {
                ((IOperationBehavior)deserializerBehavior).ApplyClientBehavior(operationDescription, clientOperation);
            }

            IClientMessageFormatter innerClientFormatter = clientOperation.Formatter;
            clientOperation.Formatter = new CustomClientMessageFormatter(innerClientFormatter);
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            //var serializerBehavior = operationDescription.Behaviors.Find<DataContractSerializerOperationBehavior>();

            //if (dispatchOperation.Formatter == null)
            //{
            //    ((IOperationBehavior)serializerBehavior).ApplyDispatchBehavior(operationDescription, dispatchOperation);
            //}

            //IDispatchMessageFormatter innerDispatchFormatter = dispatchOperation.Formatter;

            //dispatchOperation.Formatter = new CustomMessageFormatAttribute(innerDispatchFormatter);
        }

        public void Validate(OperationDescription operationDescription) { }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CustomServiceMessageFormatAttribute : Attribute, IOperationBehavior
    {
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            //var deserializerBehavior = operationDescription.Behaviors.Find<XmlSerializerOperationBehavior>();

            //if (clientOperation.Formatter == null)
            //{
            //    ((IOperationBehavior)deserializerBehavior).ApplyClientBehavior(operationDescription, clientOperation);
            //}

            //IClientMessageFormatter innerClientFormatter = clientOperation.Formatter;
            //clientOperation.Formatter = new CustomServiceMessageFormatter(innerClientFormatter);
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            var serializerBehavior = operationDescription.Behaviors.Find<XmlSerializerOperationBehavior>();

            if (dispatchOperation.Formatter == null)
            {
                ((IOperationBehavior)serializerBehavior).ApplyDispatchBehavior(operationDescription, dispatchOperation);
            }

            IDispatchMessageFormatter innerDispatchFormatter = dispatchOperation.Formatter;

            dispatchOperation.Formatter = new CustomServiceMessageFormatter(innerDispatchFormatter);
        }

        public void Validate(OperationDescription operationDescription) { }
    }

}
