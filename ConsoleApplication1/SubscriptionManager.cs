using System;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;

using Event;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Activation;
using System.ServiceModel.Description;
using System.Xml;
using System.Collections.ObjectModel;

//-----------------------------------
using System.Linq;
using System.Globalization;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Concurrent;
//-----------------------------------


namespace OnvifProxy
{
    public static class bnSubscriptionManager
    {
        //public static Dictionary<Guid, bnSubscriber> SubscribersCollection = new Dictionary<Guid, bnSubscriber>();
        public static ConcurrentDictionary<Guid, bnSubscriber> SubscribersCollection = new ConcurrentDictionary<Guid, bnSubscriber>();

        public static void AddSubscriber(bnSubscriber subscriber)
        {
            SubscribersCollection.TryAdd(subscriber.id,subscriber);
            Console.WriteLine("one basic notification subscriber added, subs number = {0}", SubscribersCollection.Count);
        }
        
    }

    public class bnSubscriber
    {
        public string Addr = null;
        public double Timeout = 0;
        public int Eventtype = 0;
        public Guid id;

        public static CustomBinding binding = new CustomBinding(
                    new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8),
                    new HttpTransportBindingElement());

        private static ChannelFactory<INotificationConsumer> myChannelFactory = new ChannelFactory<INotificationConsumer>(binding);
        
        public INotificationConsumer channel;

        public System.Timers.Timer SubscriberTimeoutTimer;

        public bnSubscriber(string addr, double timeout, int eventtype)
        {
            if ((addr == null) || (timeout <= 0) || (eventtype <= 0))
            {
                throw new Exception("wrong data for subscriber");
            }
            
            if (SubscriberTimeoutTimer == null)
            {
                SubscriberTimeoutTimer = new System.Timers.Timer(timeout);
                SubscriberTimeoutTimer.Elapsed += new ElapsedEventHandler(OnSubscriptionTimeoutEvent);
                SubscriberTimeoutTimer.Enabled = true;
                SubscriberTimeoutTimer.AutoReset = false;

                //to start OnSubscriptionTimeoutEvent in ThreadPool - to lock the collection
                SubscriberTimeoutTimer.SynchronizingObject = null;
            }

            Addr = addr;
            Timeout = timeout;
            Eventtype = eventtype;
            id = Guid.NewGuid();
            
            channel = myChannelFactory.CreateChannel(new EndpointAddress(Addr));
        }      

        private void OnSubscriptionTimeoutEvent(object source, ElapsedEventArgs e)
        {
            OnSubscriptionTimeout(source, e);
        }

        private void OnSubscriptionTimeout(object sender, EventArgs e)
        {
            bnSubscriber subs;
            ((IClientChannel)channel).Close();
            bnSubscriptionManager.SubscribersCollection.TryRemove(this.id, out subs);
            Console.WriteLine("one basic notification subscriber deleted, subs number = {0}", OnvifProxy.bnSubscriptionManager.SubscribersCollection.Count);
        }
    }

    public class EventData
    {
        public Alarmstatus Status;
        public uint Devicenumber;
        public uint Eventtype;
    }

    public class TyphoonEvent
    {
        public EventData eventData;
        public System.DateTime eventTime;
        public System.Timers.Timer EventTimeoutTimer;

        public TyphoonEvent(EventData curData, double timeout)
        {

            if (curData != null && timeout != 0)
            {
                System.DateTime curTime = DateTime.UtcNow;
                eventTime = curTime;
                eventData = curData;

                EventTimeoutTimer = new System.Timers.Timer(timeout);
                EventTimeoutTimer.Elapsed += new ElapsedEventHandler(OnSubscriptionTimeoutEvent);
                EventTimeoutTimer.Enabled = true;
                EventTimeoutTimer.AutoReset = false;
            }
            else
            {
                throw new ArgumentNullException("аргументы конструктора TyphoonEvent равны нулю");
            }
        }

        private void OnSubscriptionTimeoutEvent(object source, ElapsedEventArgs e)
        {
            lock (((ICollection)EventStorage.storage).SyncRoot)
            {
                try
                {
                    foreach (TyphoonEvent evnt in EventStorage.storage)
                    {
                        if (evnt == this)
                        {
                            EventStorage.storage.Remove(evnt);
                            Console.WriteLine("TyphoonEvent.OnSubscriptionTimeoutEvent - event removed");
                        }
                    }
                }
                catch (InvalidOperationException ioe)
                {
                    OnSubscriptionTimeoutEvent(source, e);
                    TyphoonCom.log.DebugFormat("TyphoonEvent - OnSubscriptionTimeoutEvent - exception raised - {0}", ioe.Message);
                }
            }
        }
    }

    public static class EventStorage
    {
        public static Collection<TyphoonEvent> storage = new Collection<TyphoonEvent>();
        //public static ConcurrentDictionary<TyphoonEvent.,TyphoonEvent> storage = new ConcurrentBag<TyphoonEvent>();

        public static void AddEvent(TyphoonEvent typhoonEvent)
        {
            storage.Add(typhoonEvent);
            Console.WriteLine("EventStorage.AddEvent - event added");
        }
    }

    public class ppSubscriber
    {
        public string Addr;
        public Event.FilterType Filter;
        public System.Timers.Timer SubscriberTimeoutTimer;
        Guid guid;
        private ServiceHost servhost;
        private double timeout = 60000;//минута в миллисекундах
        private Uri[] addr = new Uri[1];

        public ppSubscriber(Event.FilterType filter)
        {
            ppSubscriber tmpsubs;
            if (filter == null)
            { 
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<zero/>");
                filter.Any = new XmlElement[1];
                filter.Any[0] = doc.DocumentElement; 
            }
            guid = Guid.NewGuid();
            EndpointAddress epaddr;
            XmlConfig conf = new XmlConfig();
            ConfigStruct confstr = new ConfigStruct();
            confstr = conf.Read();

            this.Filter = filter;
            addr[0] = new Uri("http://" + confstr.IPAddr + "/onvif/pp_subscription_manager/");

            //if filter doesnt exits already
            //взведем таймер таймаута
            SubscriberTimeoutTimer = new System.Timers.Timer(timeout);
            SubscriberTimeoutTimer.Elapsed += new ElapsedEventHandler(OnSubscriptionTimeoutEvent);
            SubscriberTimeoutTimer.Enabled = true;
            SubscriberTimeoutTimer.AutoReset = false;

            // to lock the timer
            SubscriberTimeoutTimer.SynchronizingObject = null;

            servhost = new ServiceHost(typeof(PullPointNotificationService), addr);
            //epaddr = new EndpointAddress(addr[0] + "pp_subscription_manager");
            epaddr = new EndpointAddress(new Uri(addr[0].ToString() + guid.ToString()));
            Addr = epaddr.Uri.ToString();

            HttpTransportBindingElement httpTransportBindingElement = new HttpTransportBindingElement();
            httpTransportBindingElement.KeepAliveEnabled = false;
            
            CustomBinding binding = new CustomBinding(
                    new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8),
                    httpTransportBindingElement);
            binding.Namespace = "http://www.onvif.org/ver10/event/wsdl";

            servhost.AddServiceEndpoint(typeof(IPullPointService), binding, guid.ToString());

            try
            {
                ppSubscriptionManager.AddSubscriber(this);
                servhost.Open();
                Console.WriteLine("ep - {0}", Addr);
            }
            catch (CommunicationObjectFaultedException cofe)
            {
                Console.WriteLine("additional service host openning failed - {0}", cofe.Message);
                Console.WriteLine("another try ...");
                servhost.Close();
                servhost = null;
                ppSubscriptionManager.SubscribersCollection.TryRemove(this.Filter, out tmpsubs);
                new ppSubscriber(filter);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("additional service host openning failed - {0}", ex.Message);
                Console.WriteLine("another try ...");
                ppSubscriptionManager.SubscribersCollection.TryRemove(this.Filter, out tmpsubs);
                servhost = null;
                new ppSubscriber(filter);

                Console.WriteLine("!!!!");
                Console.WriteLine("additional service host openning failed - {0}", ex.Message);
            }

        }

        private void OnSubscriptionTimeoutEvent(object source, ElapsedEventArgs e)
        {
            ppSubscriber tmpsubs;
            if (servhost.State != CommunicationState.Closed ||
                servhost.State != CommunicationState.Closing ||
                servhost.State != CommunicationState.Faulted)
            {
                try
                {
                    ppSubscriptionManager.SubscribersCollection.TryRemove(this.Filter, out tmpsubs);
                    servhost.Close();
                    Console.WriteLine("one pull-point subscriber deleted, subs number = {0}", OnvifProxy.ppSubscriptionManager.SubscribersCollection.Count);
                }
                
                catch (Exception ex)
                {
                    Console.WriteLine("failed to close pull-point event service host - {0}", ex.Message);
                }

            }
            servhost = null;
        }
    }

    public static class ppSubscriptionManager
    {
        public static ConcurrentDictionary<Event.FilterType, ppSubscriber> SubscribersCollection =
            new ConcurrentDictionary<Event.FilterType, ppSubscriber>();

        public static void AddSubscriber(ppSubscriber subscriber)
        {
            SubscribersCollection.TryAdd(subscriber.Filter, subscriber);
            Console.WriteLine("one pull-point subscriber added, subs number = {0}", SubscribersCollection.Count);
        }
    }

    public class PullPointNotificationService : IPullPointService
    {
        void Event.PullPointSubscription.SetSynchronizationPoint()
        {
        }

        public Event.SeekResponse Seek(Event.SeekRequest request)
        {
            throw new NotImplementedException();
        }

        public Event.PullMessagesResponse PullMessages(Event.PullMessagesRequest request)
        {
            int a;
            XmlDocument doc = new XmlDocument();
            XmlDocument doc1 = new XmlDocument();
            doc.LoadXml("<Message/>");
            doc1.LoadXml("<Top/>");

            PullMessagesResponse pmresp = new PullMessagesResponse();
            OperationContext context = OperationContext.Current;
            //определяем адрес  сервиса на который пришёл запрос
            string addr = context.EndpointDispatcher.EndpointAddress.Uri.AbsoluteUri.ToString();
            //просматриваем коллекцию сервисов и ищем в ней сервис с нашим адресом
            foreach (System.Collections.Generic.KeyValuePair<Event.FilterType, ppSubscriber> subs in ppSubscriptionManager.SubscribersCollection)
            {
                //если нашли - снова взводим таймер таймаута
                if (subs.Value.Addr == addr)
                {
                    subs.Value.SubscriberTimeoutTimer.Interval = 60000;
                    pmresp.TerminationTime = System.DateTime.UtcNow.AddMilliseconds(subs.Value.SubscriberTimeoutTimer.Interval);
                    //check if there are events for this filtertype
                    //пока фильтрация не сделана, возвращаем все события
                    if (request.MessageLimit >= EventStorage.storage.Count)
                    {
                        a = EventStorage.storage.Count;
                    }
                    else
                    {
                        a = request.MessageLimit;
                    }

                    pmresp.NotificationMessage = new NotificationMessageHolderType[EventStorage.storage.Count];

                    for (int y = 0; y < a; y++)
                    {
                        pmresp.NotificationMessage[y] = new NotificationMessageHolderType();
                        pmresp.NotificationMessage[y].Topic = new TopicExpressionType();
                        pmresp.NotificationMessage[y].Topic.Any = new XmlNode[1];

                        pmresp.NotificationMessage[y].Message = doc.DocumentElement;
                        pmresp.NotificationMessage[y].Message.SetAttribute("UtcTime", System.DateTime.UtcNow.ToString("s"));
                        pmresp.NotificationMessage[y].Topic.Dialect = "http://www.onvif.org/ver10/tev/topicExpression/ConcreteSet";
                        pmresp.NotificationMessage[y].Topic.Any[0] = doc1.CreateNode(XmlNodeType.Text, "", "");
                        pmresp.NotificationMessage[y].Topic.Any[0].Value = "tns1:VideoSource";
                       
                    }
                }
            }
            //if no events
            
            pmresp.CurrentTime = System.DateTime.UtcNow;
            //else get events from event storage
            return pmresp;
        }

        public Event.UnsubscribeResponse1 Unsubscribe(Event.UnsubscribeRequest request)
        {
            PullMessagesResponse pmresp = new PullMessagesResponse();

            OperationContext context = OperationContext.Current;

            //Console.WriteLine(context.EndpointDispatcher.EndpointAddress.Uri.AbsoluteUri.ToString());

            //определяем адрес  сервиса на который пришёл запрос
            string addr = context.EndpointDispatcher.EndpointAddress.Uri.AbsoluteUri.ToString();

            foreach (System.Collections.Generic.KeyValuePair<Event.FilterType, ppSubscriber> subs in ppSubscriptionManager.SubscribersCollection)
            {
                //просматриваем коллекцию сервисов и ищем в ней сервис с нашим адресом
                if (subs.Value.Addr == addr)
                {
                    subs.Value.SubscriberTimeoutTimer.Interval = 1;
                }
            }
            return new Event.UnsubscribeResponse1(new UnsubscribeResponse());
        }

        public RenewResponse1 Renew(RenewRequest request)
        {
            RenewResponse renewresp = new RenewResponse();
            RenewResponse1 renewresp1 = new RenewResponse1();
            PullMessagesResponse pmresp = new PullMessagesResponse();
            XmlConfig conf = new XmlConfig();
            ConfigStruct confstr = new ConfigStruct();
            Helper hlp = new Helper();
            confstr = conf.Read();

            OperationContext context = OperationContext.Current;
            int a = context.Channel.LocalAddress.Uri.Port;
            //string addr = "http://" + confstr.IPAddr + ":" + a.ToString() + "/pp_subscription_manager";
            string addr = "http://" + confstr.IPAddr + ":" + a.ToString() +
                "/pp_subscription_manager/" + context.Channel.LocalAddress.Uri.ToString();

            foreach (System.Collections.Generic.KeyValuePair<Event.FilterType, ppSubscriber> subs
                in ppSubscriptionManager.SubscribersCollection)
            {
                if (subs.Value.Addr == context.Channel.LocalAddress.Uri.ToString())
                {
                    subs.Value.SubscriberTimeoutTimer.Interval = hlp.ParseTermTime(request.Renew.TerminationTime);
                    Console.WriteLine("timeout added");

                    pmresp.TerminationTime = System.DateTime.UtcNow.AddMilliseconds(subs.Value.SubscriberTimeoutTimer.Interval);
                }
            }
            renewresp.CurrentTime = System.DateTime.UtcNow;
            renewresp.CurrentTimeSpecified = true;
            renewresp.TerminationTime = pmresp.TerminationTime;
            renewresp1.RenewResponse = renewresp;
            return renewresp1;
        }
    }

    public class Helper
    {
        public double ParseTermTime(string termtime)
        {
            if (termtime == null)
                return 0;
            //возвращает в время миллисекундах
            #region //xsd:duration time format description
            // Constructor which takes a relative time value. The time value is expressed as String with
            // the following format PxYxMxDTxHxMxS where:
            // P is a required value to inidicate the start of the expression
            // xY indicates the number of years (optional)
            // xM indicates the number of months (optional)
            // xD indicates the number of days (optional)
            // T indicates the start of a time section and is required if the duration expressed contains hours, minutes, or seconds
            // xH indicates the number of hours (optional)
            // xM indicates the number of minutes (optional)
            // xS indicates the number of seconds (optional)

            //Examples:
            //"P12Y8M22DT3H35M2S" indicates a duration of 12 years, 8 months, 22 days, 3 hours, 35 minutes and 2 seconds
            //"P21DT8H" indicates a duration of 21 days and 8 hours
            //"P5Y7M" indicates a duration of 5 years and 7 months
            //"PT14S" indicates a duration of 14 seconds
            #endregion

            System.DateTime dtime = new System.DateTime();
            string tmp1;

            if ((termtime.ToCharArray())[0] == 'P')
            {
                //ветка xsd:duration
                int indofS, indofM, indofH, indofT, indofD, indofMn, indofY;
                indofS = termtime.IndexOf('S');
                indofM = termtime.IndexOf('M');
                indofH = termtime.IndexOf('H');
                indofT = termtime.IndexOf('T');
                //indofMn = termtime.IndexOf('M');
                indofY = termtime.IndexOf('Y');

                if (termtime.Contains('T'))
                {
                    if (termtime.Contains('H'))
                    {
                        tmp1 = termtime.Remove(0, termtime.IndexOf('T') + 1);
                        tmp1 = tmp1.Remove(termtime.IndexOf('H'), (tmp1.Length - tmp1.LastIndexOf('H')));
                        dtime.AddHours(str2double(tmp1));
                    }
                    if (termtime.Contains('M') && (termtime.IndexOf('T') < termtime.IndexOf('M')))
                    {
                        tmp1 = termtime.Remove(0, termtime.IndexOf('T') + 1);
                        tmp1 = termtime.Remove(tmp1.IndexOf('M'), (tmp1.Length - tmp1.LastIndexOf('M')));
                        dtime.AddMinutes(str2double(tmp1));
                    }
                    if (termtime.Contains('S'))
                    {
                        tmp1 = termtime.Remove(0, termtime.IndexOf('T') + 1);
                        tmp1 = tmp1.Remove(tmp1.IndexOf('S'), (tmp1.Length - tmp1.LastIndexOf('S')));
                        dtime = dtime.AddSeconds(str2double(tmp1));
                    }
                }
                else
                {
                    return 0;
                }


                //---------------T------------
                if (termtime.Contains('D'))
                {
                }
                if (termtime.Contains('M'))
                {
                }
                if (termtime.Contains('Y'))
                {
                }
                return (double)dtime.TimeOfDay.TotalMilliseconds;
            }
            else
            {
                //ветка xsd:dateTime
                System.DateTime ddtime = new System.DateTime();
                System.DateTime.TryParseExact(termtime, "yyyy-MM-dd'T'HH:mm:ss'Z'", null, DateTimeStyles.AdjustToUniversal, out ddtime);
                TimeSpan timespan = ddtime.Subtract(System.DateTime.Now);
                if (timespan.TotalMilliseconds >= 0)
                {
                    return (double)timespan.TotalMilliseconds;
                }
                else
                    return 1;
            }
        }

        public double str2double(string str)
        {
            double bytes = double.Parse(str);
            return (double)bytes;
        }
    }

    public enum Alarmstatus
    {
        AlarmOff = 0,
        AlarmOn = 1,
    }

    [ServiceContractAttribute]
    public interface IPullPointService : Event.PullPointSubscription, Event.SubscriptionManager
    {
    }
  
}
