using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;

using System.Management; 

namespace OnvifProxy
{
    public sealed class AdapterInfo
    {

        const int MAX_ADAPTER_NAME_LENGTH = 256;

        const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;

        const int MAX_ADAPTER_ADDRESS_LENGTH = 8;

        const int ERROR_BUFFER_OVERFLOW = 111;

        const int ERROR_SUCCESS = 0;

        [DllImport("iphlpapi.dll", CharSet = CharSet.Ansi)]

        private static extern int GetAdaptersInfo(IntPtr pAdapterInfo, ref Int64 pBufOutLen);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]

        private struct IP_ADDRESS_STRING
        {

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]

            public string Address;

        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]

        private struct IP_ADDR_STRING
        {

            public IntPtr Next;

            public IP_ADDRESS_STRING IpAddress;

            public IP_ADDRESS_STRING Mask;

            public Int32 Context;

        }



        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]

        private struct IP_ADAPTER_INFO
        {

            public IntPtr Next;

            public Int32 ComboIndex;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_NAME_LENGTH + 4)]

            public string AdapterName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_DESCRIPTION_LENGTH + 4)]

            public string AdapterDescription;

            public UInt32 AddressLength;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]

            public byte[] Address;

            public Int32 Index;

            public UInt32 Type;

            public UInt32 DhcpEnabled;

            public IntPtr CurrentIpAddress;

            public IP_ADDR_STRING IpAddressList;

            public IP_ADDR_STRING GatewayList;

            public IP_ADDR_STRING DhcpServer;

            public bool HaveWins;

            public IP_ADDR_STRING PrimaryWinsServer;

            public IP_ADDR_STRING SecondaryWinsServer;

            public Int32 LeaseObtained;

            public Int32 LeaseExpires;

        }

        public AdapterInfo()
        {

        }

        public static Adapter[] GetAdaptersInfo()
        {

            Adapter[] adaptersList;

            ArrayList adapters = new ArrayList();

            long structSize = Marshal.SizeOf(typeof(IP_ADAPTER_INFO));

            IntPtr pArray = Marshal.AllocHGlobal((int)structSize);

            int ret = GetAdaptersInfo(pArray, ref structSize);

            if (ret == ERROR_BUFFER_OVERFLOW) // ERROR_BUFFER_OVERFLOW == 111
            {

                // Buffer was too small, reallocate the correct size for the buffer.

                pArray = Marshal.ReAllocHGlobal(pArray, new IntPtr(structSize));

                ret = GetAdaptersInfo(pArray, ref structSize);

            } // if

            if (ret == 0)
            {

                // Call Succeeded

                IntPtr pEntry = pArray;

                do
                {

                    // Retrieve the adapter info from the memory address

                    IP_ADAPTER_INFO entry = (IP_ADAPTER_INFO)Marshal.PtrToStructure(pEntry, typeof(IP_ADAPTER_INFO));

                    Adapter adapter = new Adapter();

                    adapter.index = entry.Index.ToString();

                    adapter.name = entry.AdapterName;

                    adapter.description = entry.AdapterDescription;

                    adapter.ip = entry.IpAddressList.IpAddress.Address;

                    // MAC Address (data is in a byte[])

                    string tmpString = string.Empty;

                    for (int i = 0; i < entry.AddressLength; i++)
                    {

                        tmpString += string.Format("{0:X2}", entry.Address[i]);

                    }

                    adapter.macAddress = tmpString;

                    adapters.Add(adapter);

                    // Get next adapter (if any)

                    pEntry = entry.Next;

                }

                while (pEntry != IntPtr.Zero);

                Marshal.FreeHGlobal(pArray);

                adaptersList = new Adapter[adapters.Count];

                adapters.CopyTo(adaptersList);

            }

            else
            {

                adaptersList = new Adapter[0];

                Marshal.FreeHGlobal(pArray);

                throw new InvalidOperationException("GetAdaptersInfo failed with " + ret);

            }

            return adaptersList;

        }

    }

    public class Adapter
    {

        public string index;

        public string name;

        public string description;

        public string ip;

        public string macAddress;

    }

    class NetworkManagement
    {
        /// <summary> 
        /// Set's a new IP Address and it's Submask of the local machine 
        /// </summary> 
        /// <param name="ip_address">The IP Address</param> 
        /// <param name="subnet_mask">The Submask IP Address</param> 
        /// <remarks>Requires a reference to the System.Management namespace</remarks> 
        public void setIP(string ip_address, string subnet_mask)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                //string[] tmp = (string[])objMO["IPAddress"];
                if ((bool)objMO["IPEnabled"])
                {
                    try
                    {
                        ManagementBaseObject setIP;
                        ManagementBaseObject newIP =
                            objMO.GetMethodParameters("EnableStatic");

                        newIP["IPAddress"] = new string[] { ip_address };
                        newIP["SubnetMask"] = new string[] { subnet_mask };

                        setIP = objMO.InvokeMethod("EnableStatic", newIP, null);
                    }
                    catch (Exception)
                    {
                        throw;
                    }


                }
            }
        }
     
        public string[] getIP()
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            
            foreach (ManagementObject objMO in objMOC)
            {
                //string[] tmp = (string[])objMO["IPAddress"];
                if ((bool)objMO["IPEnabled"])
                {
                    try
                    {
                        ManagementBaseObject setIP;
                        ManagementBaseObject newIP =
                            objMO.GetMethodParameters("EnableStatic");

                        //newIP["IPAddress"] = new string[] { ip_address };
                        //newIP["SubnetMask"] = new string[] { subnet_mask };

                        setIP = objMO.InvokeMethod("EnableStatic", newIP, null);
                    }
                    catch (Exception)
                    {
                        throw;
                    }


                }
            }
            return null;
        }
        /// <summary> 
        /// Set's a new Gateway address of the local machine 
        /// </summary> 
        /// <param name="gateway">The Gateway IP Address</param> 
        /// <remarks>Requires a reference to the System.Management namespace</remarks> 
        public void setGateway(string gateway)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    try
                    {
                        ManagementBaseObject setGateway;
                        ManagementBaseObject newGateway =
                            objMO.GetMethodParameters("SetGateways");

                        newGateway["DefaultIPGateway"] = new string[] { gateway };
                        newGateway["GatewayCostMetric"] = new int[] { 1 };

                        setGateway = objMO.InvokeMethod("SetGateways", newGateway, null);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }
        /// <summary> 
        /// Set's the DNS Server of the local machine 
        /// </summary> 
        /// <param name="NIC">NIC address</param> 
        /// <param name="DNS">DNS server address</param> 
        /// <remarks>Requires a reference to the System.Management namespace</remarks> 
        public void setDNS(string NIC, string DNS)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    if (objMO["Caption"].Equals(NIC))
                    {
                        try
                        {
                            ManagementBaseObject newDNS =
                                objMO.GetMethodParameters("SetDNSServerSearchOrder");
                            newDNS["DNSServerSearchOrder"] = DNS.Split(',');
                            ManagementBaseObject setDNS =
                                objMO.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }
        }
        /// <summary> 
        /// Set's WINS of the local machine 
        /// </summary> 
        /// <param name="NIC">NIC Address</param> 
        /// <param name="priWINS">Primary WINS server address</param> 
        /// <param name="secWINS">Secondary WINS server address</param> 
        /// <remarks>Requires a reference to the System.Management namespace</remarks> 
        public void setWINS(string NIC, string priWINS, string secWINS)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    if (objMO["Caption"].Equals(NIC))
                    {
                        try
                        {
                            ManagementBaseObject setWINS;
                            ManagementBaseObject wins =
                            objMO.GetMethodParameters("SetWINSServer");
                            wins.SetPropertyValue("WINSPrimaryServer", priWINS);
                            wins.SetPropertyValue("WINSSecondaryServer", secWINS);

                            setWINS = objMO.InvokeMethod("SetWINSServer", wins, null);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }
        }
    } 
}

