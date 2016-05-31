using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;
using System.ServiceModel.Dispatcher;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Collections.ObjectModel;

namespace OnvifProxy
{

    public class SecurityOperationBehavoirAttribute : Attribute, IOperationBehavior
    {
        static List<XmlQualifiedName> adminList = new List<XmlQualifiedName>();
        static List<XmlQualifiedName> anonList = new List<XmlQualifiedName>();
        static List<XmlQualifiedName> operatorList = new List<XmlQualifiedName>();
        static List<XmlQualifiedName> userList = new List<XmlQualifiedName>();
        static Dictionary<string, List<XmlQualifiedName>> summartList = new Dictionary<string, List<XmlQualifiedName>>();


        public SecurityOperationBehavoirAttribute(string name)
        {
        }

        public SecurityOperationBehavoirAttribute(string name, string ns)
        {
        }

        public SecurityOperationBehavoirAttribute(string name, string ns, int usertype)
        {
            adminList.Add(new XmlQualifiedName(name, ns));
            switch (usertype)
            {
                case 0:
                    break;
                case 1:
                    operatorList.Add(new XmlQualifiedName(name, ns));
                    break;
                case 2:
                    operatorList.Add(new XmlQualifiedName(name, ns));
                    userList.Add(new XmlQualifiedName(name, ns));
                    break;
                case 3:
                    operatorList.Add(new XmlQualifiedName(name, ns));
                    userList.Add(new XmlQualifiedName(name, ns));
                    anonList.Add(new XmlQualifiedName(name, ns));
                    break;
                default:
                    break;
            }

            //-------------------------------------------------------------
            // отключение авторизации ws-usertokenname
            // чтобы включить авторизацию обратно - закоментить
            // всё что ниже и раскоментить всё что выше
            //adminList.Add(new XmlQualifiedName(name, ns));
            //operatorList.Add(new XmlQualifiedName(name, ns));
            //userList.Add(new XmlQualifiedName(name, ns));
            //anonList.Add(new XmlQualifiedName(name, ns));
            //------------------------------------------------------------- 
        }

        internal Dictionary<string, List<XmlQualifiedName>> QName
        {
            get
            {
                summartList = new Dictionary<string, List<XmlQualifiedName>>();
                summartList.Add("anon", anonList);
                summartList.Add("admin", adminList);
                summartList.Add("user", userList);
                summartList.Add("oper", operatorList);
                return summartList;
            }
            set { summartList = value; }
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
        }

        public void Validate(OperationDescription operationDescription)
        {
        }
    }

}