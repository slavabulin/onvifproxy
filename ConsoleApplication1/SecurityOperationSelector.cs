
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
            //!!!!can leak here (with buffer, message1, message2)!!!!!
            //using (
            MessageBuffer buffer = message.CreateBufferedCopy(Int32.MaxValue);
            //)
            {
                List<XmlQualifiedName> methodList = new List<XmlQualifiedName>();
                Usertype usertypefromfile = Usertype.wrongpass;
                //Message msgcopy1 = buffer.CreateMessage();// using
                
                Message msgcopy2 = buffer.CreateMessage();//
                XmlDictionaryReader bodyReader = msgcopy2.GetReaderAtBodyContents();
                XmlQualifiedName lookupQName = new XmlQualifiedName(bodyReader.LocalName, bodyReader.NamespaceURI);
                Message msgcopy1 = CreateMessageCopy(message, bodyReader);// using

                //---------------------------------------
                foreach (MessageHeaderInfo mheadinfo in message.Headers)
                //foreach (MessageHeaderInfo mheadinfo in msgcopy2.Headers)
                {
                    //check if security header exists
                    #region check security header

                    //todo:try to make getting values over XPath or something like that
                    if (mheadinfo.Name == "Security" || mheadinfo.Name == "security")
                    {
                        Console.WriteLine("Security Header found!");

                        // - check if method needs security
                        //message.Action
                        //         else select operation
                        // - cut sec header
                        // - deserialize sec header
                        // - check if credentials are valid
                        // - select operation 

                        String msg = message.ToString();
                        //String msg = msgcopy2.ToString();
                        int startindex = msg.IndexOf("<UsernameToken");// and if in lower case?
                        int endindex = msg.IndexOf("</UsernameToken");
                        String securityheaderstring = msg.Substring(startindex, (endindex - startindex + 16));
                        if (!securityheaderstring.EndsWith(">"))
                            securityheaderstring.Insert(securityheaderstring.Length, ">");

                        Security secheader = new Security();
                        secheader.Token = new UsernameToken();
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(UsernameToken));
                        securityheaderstring = "<?xml version='1.0' encoding='utf-8' ?>" + securityheaderstring;
                        int strtcutindexcreate;
                        if (securityheaderstring.Contains("Created"))
                        {
                            strtcutindexcreate = securityheaderstring.IndexOf("<Created");
                        }
                        else
                        {
                            strtcutindexcreate = securityheaderstring.IndexOf("<created");
                        }
                        int endcutindexcreate = securityheaderstring.IndexOf(">", strtcutindexcreate);
                        securityheaderstring = securityheaderstring.Remove(strtcutindexcreate + 9, (endcutindexcreate - strtcutindexcreate - 9));
                        using (Stream ms = new MemoryStream(Encoding.UTF8.GetBytes(securityheaderstring)))
                        {
                            try
                            {
                                secheader.Token = (UsernameToken)xmlSerializer.Deserialize(ms);
                                CheckPasswordDigest checkpass = new CheckPasswordDigest(secheader.Token.Password,
                                    secheader.Token.Username,
                                    secheader.Token.Nonce,
                                    secheader.Token.Created
                                    );
                                // get usertype from file
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
                                    Console.WriteLine("Invalid password!");
                                    message = msgcopy1;
                                    return defaultOperationName;
                                }

                                #endregion check if creds are wright
                            }
                            catch (SerializationException g)
                            {
                                Console.WriteLine("Не могу десериализовать файл конфигурации; " + g.Message);
                                return defaultOperationName;
                            }
                            finally
                            {
                                ms.Close();
                            }
                        }
                    }
                    //if security header doesnt exists
                    //check anon user branch for called method
                    else
                    {
                        //case if no security header
                        try
                        {
                            // - check if desired method is in the allowed without auth method list
                            dispatchDictionary.TryGetValue("anon", out methodList);
                            foreach (XmlQualifiedName methodname in methodList)
                            {
                                string tmpstring = methodname.Namespace + "/" + methodname.Name;
                                //   if true - return methodname
                                //   else defaultOpertionname
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
                        message = msgcopy1;
                        return defaultOperationName;
                    }
                    #endregion check security header
                }
                message = msgcopy1;
                return defaultOperationName;
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

    public class CheckPasswordDigest
    {
        public string RecievedPasswordDigest;// = "admin";
        public string Name;// = "admin";
        public string Nonce;// = "aN6VJLJZfkCkkjpXT6GbX1UAAAAAAA==";
        public string Created;// = "2015-05-13T13:44:47.631Z";
        string PasswordFromFile;// = "admin";//read from file
        static UserList userlist;
        int UsertypeFromFile;

        public CheckPasswordDigest(string Pass, string Name, string Nonce, string Created)
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