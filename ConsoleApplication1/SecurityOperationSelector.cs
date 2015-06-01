﻿
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
using System.Xml.XPath;

namespace OnvifProxy
{
    class SecurityOperationSelector : IDispatchOperationSelector
    {
        Dictionary<string, List<XmlQualifiedName>> dispatchDictionary;
        string defaultOperationName;

        public SecurityOperationSelector(Dictionary<string, List<XmlQualifiedName>> dispatchDictionary, string defaultOperationName)
        {
            this.dispatchDictionary = dispatchDictionary;
            this.defaultOperationName = defaultOperationName;
        }
        
        //makes copy of message excluding security header
        private Message CreateMessageCopy(Message message, XmlDictionaryReader body)
        {
            Message copy;

            try
            {
                copy = Message.CreateMessage(message.Version, message.Headers.Action, body);
            }
            catch (ArgumentNullException e)
            {
                throw e;
            }
            if (message.Headers.Action == null)
            {
                message.Headers.Action = body.LocalName;
                copy.Headers.CopyHeaderFrom(message, 0);///!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                copy.Headers.Action = body.NamespaceURI + "/" + body.LocalName;

                copy.Properties.CopyProperties(message.Properties);
            }
            return copy;
        }

        Security GetCredesFromMessageBuffer(MessageBuffer msgBuf)
        {
            XPathNavigator navigator = msgBuf.CreateNavigator();
            Security secheader = new Security();
            secheader.Token = new UsernameToken();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(UsernameToken));

            if (
            navigator.MoveToChild("Envelope", "http://www.w3.org/2003/05/soap-envelope") &&
            navigator.MoveToChild("Header", "http://www.w3.org/2003/05/soap-envelope") &&
            navigator.MoveToChild("Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"))
            {
                if (navigator.MoveToChild("UsernameToken", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd") &&
                    navigator.MoveToChild("Username", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"))
                {
                    secheader.Token.Username = navigator.Value;
                    navigator.MoveToParent();
                }
                else return null;
                if (navigator.MoveToChild("Password", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"))
                {
                    secheader.Token.Password = navigator.Value;
                    navigator.MoveToParent();
                }
                else return null;
                if (navigator.MoveToChild("Nonce", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"))
                {
                    secheader.Token.Nonce = navigator.Value;
                    navigator.MoveToParent();
                }
                else return null;
                if (navigator.MoveToChild("Created", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"))
                {
                    secheader.Token.Created = navigator.Value;
                    navigator.MoveToParent();
                }
                else return null;
                return secheader;
            }
            return null;
        }

        public string SelectOperation(ref System.ServiceModel.Channels.Message message)
        {
            //!!!!can leak here (in message1, message2)!!!!!
            using (MessageBuffer buffer = message.CreateBufferedCopy(Int32.MaxValue))
            {
                List<XmlQualifiedName> methodList = new List<XmlQualifiedName>();
                Usertype usertypefromfile = Usertype.wrongpass;
                
                Message msgcopy2 = buffer.CreateMessage();//
                XmlDictionaryReader bodyReader = msgcopy2.GetReaderAtBodyContents();
                XmlQualifiedName lookupQName = new XmlQualifiedName(bodyReader.LocalName, bodyReader.NamespaceURI);
                Message msgcopy1 = CreateMessageCopy(message, bodyReader);// using

                //---------------------------------------
                Security secheader = new Security();
                secheader.Token = new UsernameToken();
                secheader = GetCredesFromMessageBuffer(buffer);
                #region check if there is security header
                if (secheader != null)
                {
                    SecurityHeader checkpass = new SecurityHeader(secheader.Token.Password,
                                    secheader.Token.Username,
                                    secheader.Token.Nonce,
                                    secheader.Token.Created
                                    );
                    usertypefromfile = checkpass.CheckPassword();

                    #region check if creds are wright
                    if (usertypefromfile != Usertype.wrongpass)
                    {
                        Console.WriteLine("Pass is valid!");
                        //-------------------------------------------------                                    
                        // check if it exists in dictionary
                        // get list of apropriate usertype
                        // and compare it with lookupQName
                        //-------------------------------------------------
                        #region get allowed methods list
                        foreach (string usrtype in dispatchDictionary.Keys)
                        {
                            if (usrtype == usertypefromfile.ToString())
                            {
                                try
                                {
                                    dispatchDictionary.TryGetValue(usrtype, out methodList);
                                    //check if allowed method requested
                                    foreach (XmlQualifiedName methodname in methodList)
                                    {
                                        //string tmpstring = methodname.Namespace + "/" + methodname.Name;
                                        if (methodname == lookupQName)
                                        {
                                            message = msgcopy1;
                                            return methodname.Name;
                                        }
                                    }
                                }
                                catch (ArgumentNullException ane)
                                {
                                    throw ane;
                                }
                            }
                        }
                        message = msgcopy1;
                        return defaultOperationName;
                        #endregion get allowed methods list
                    }
                    else
                    {
                        //credentials are wrong
                        message = msgcopy1;
                        //return defaultOperationName;
                        return null;
                    }
                    #endregion check if creds are wright
                }
                else
                {
                    //no security header
                    message = msgcopy1;
                    return defaultOperationName;
                }
                #endregion check if there is security header

            }
        }
    }
        

    public enum Usertype
    {
        anon,
        admin,
        oper,
        user,
        wrongpass
    }

    public class Security
    {
        private UsernameToken _token;

        public UsernameToken Token
        {
            get
            {
                return this._token;
            }
            set
            {
                this._token = value;
            }
        }

    }

    public class UsernameToken
    {
        private string _name;
        private string _pass;
        private string _nonce;
        private string _created;

        public string Username
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        public string Password
        {
            get
            {
                return this._pass;
            }
            set
            {
                this._pass = value;
            }
        }

        public string Nonce
        {
            get
            {
                return this._nonce;
            }
            set
            {
                this._nonce = value;
            }
        }

        public string Created
        {
            get
            {
                return this._created;
            }
            set
            {
                this._created = value;
            }
        }
    }

    public class User
    {
        public string username;
        public string password;
        public int usertype;
    }

    public class UserList : Collection<User>
    {
    }

    public class SecurityHeader
    {
        public string RecievedPasswordDigest;// = "admin";
        public string Name;// = "admin";
        public string Nonce;// = "aN6VJLJZfkCkkjpXT6GbX1UAAAAAAA==";
        public string Created;// = "2015-05-13T13:44:47.631Z";
        string PasswordFromFile;// = "admin";//read from file
        static UserList userlist;
        int UsertypeFromFile;

        public SecurityHeader(string Pass, string Name, string Nonce, string Created)
        {
            this.RecievedPasswordDigest = Pass;
            this.Name = Name;
            this.Nonce = Nonce;
            this.Created = Created;
            UsertypeFromFile = -1;

            //open file and read passwords and usernames
            //put them in a dictionary
            using (FileStream fs = new FileStream("pwd.xml", FileMode.Open, FileAccess.Read, FileShare.Read))
            {

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserList));

                try
                {
                    userlist = (UserList)xmlSerializer.Deserialize(fs);
                    foreach (User usr in userlist)
                    {
                        if (usr.username == Name)
                        {
                            PasswordFromFile = usr.password;
                            UsertypeFromFile = usr.usertype;
                            break;
                        }
                    }
                }
                catch (SerializationException g)
                {
                    Console.WriteLine("Не могу десериализовать файл конфигурации; " + g.Message);
                    throw g;
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        public Usertype CheckPassword()
        {
            byte[] bytearrpass;
            byte[] bytearrnonce;
            byte[] bytearrcreated;
            //choose password by username from dictionary and put in PasswordFromFile
            if (PasswordFromFile != null && Nonce != null && Created != null)
            {
                bytearrnonce = System.Convert.FromBase64String(Nonce);
                bytearrcreated = Encoding.UTF8.GetBytes(Created);
                bytearrpass = Encoding.UTF8.GetBytes(PasswordFromFile);

                byte[] barr = new byte[bytearrnonce.Length + bytearrcreated.Length + bytearrpass.Length];
                for (int r = 0; r < bytearrnonce.Length; r++)
                {
                    barr[r] = bytearrnonce[r];
                }
                for (int t = 0; t < bytearrcreated.Length; t++)
                {
                    barr[bytearrnonce.Length + t] = bytearrcreated[t];
                }
                for (int y = 0; y < bytearrpass.Length; y++)
                {
                    barr[bytearrnonce.Length + bytearrcreated.Length + y] = bytearrpass[y];
                }

                string DerivedPasswordDigest = System.Convert.ToBase64String(SHA1.Create().ComputeHash(barr));
                if (DerivedPasswordDigest == RecievedPasswordDigest)
                {
                    switch (UsertypeFromFile)
                    {
                        case -1:
                            return Usertype.wrongpass;
                        case 0:
                            return Usertype.admin;
                        case 1:
                            return Usertype.oper;
                        case 2:
                            return Usertype.user;
                        case 3:
                            return Usertype.anon;
                        default:
                            return Usertype.wrongpass;
                    }
                }
                else
                {
                    return Usertype.wrongpass;
                }
            }
            else
            {
                return Usertype.wrongpass;
            }
        }
    }
}