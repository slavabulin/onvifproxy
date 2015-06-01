using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Net;
using System.Web;

namespace OnvifProxy
{
    class HttpErrorsAttirbute
    {
    }

    public class HttpErrorsAttribute : Attribute, IEndpointBehavior
    {
        public void AddBindingParameters(
            ServiceEndpoint endpoint,
            BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(
            ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(
            ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            var handlers = endpointDispatcher.ChannelDispatcher.ErrorHandlers;
            handlers.Clear();
            handlers.Add(new HttpErrorHandler());
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        public class HttpErrorHandler : IErrorHandler
        {
            public bool HandleError(Exception error)
            {
                return true;
            }

            public void ProvideFault(
                Exception error, MessageVersion version, ref Message fault)
            {
                HttpStatusCode status;
                if (error is HttpException)
                {
                    var httpError = error as HttpException;
                    status = (HttpStatusCode)httpError.GetHttpCode();
                }
                else if (error is ArgumentException)
                {
                    status = HttpStatusCode.BadRequest;
                }
                else
                {
                    status = HttpStatusCode.InternalServerError;
                }

                // return custom error code.
                fault = Message.CreateMessage(version, "", error.Message);
                fault.Properties.Add(
                    HttpResponseMessageProperty.Name,
                    new HttpResponseMessageProperty
                    {
                        StatusCode = status,
                        StatusDescription = error.Message
                    }
                );
            }
        }
    }
}
