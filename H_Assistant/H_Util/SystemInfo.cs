using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using Microsoft.Win32;

namespace H_Util
{
    public class SystemInfo
    {
        //private void test()
        //{
        //    Console.WriteLine("主板编号 GetBoardID:" + GetBoardID());
        //    Console.WriteLine("主板制造厂商 GetBoardManufacturer:" + GetBoardManufacturer());
        //    Console.WriteLine("主板型号 GetBoardType:" + GetBoardType());
        //    Console.WriteLine("获取计算机名 GetComputerName:" + GetComputerName);
        //    Console.WriteLine("获得CPU编号 GetCPUID:" + GetCPUID());
        //    Console.WriteLine("CPU制造厂商 GetCPUManufacturer:" + GetCPUManufacturer());
        //    Console.WriteLine("CPU名称信息 GetCPUName:" + GetCPUName());
        //    Console.WriteLine("获取CPU序列号 GetCPUSerialNumber:" + GetCPUSerialNumber());
        //    Console.WriteLine("CPU版本信息 GetCPUVersion:" + GetCPUVersion());
        //    Console.WriteLine("获取硬盘序列号 GetDiskSerialNumber:" + GetDiskSerialNumber());
        //    Console.WriteLine("获取硬盘序列号 GetHardDiskSerialNumber:" + GetHardDiskSerialNumber());
        //    Console.WriteLine("获取IP地址 GetIPAddress:" + GetIPAddress());
        //    Console.WriteLine("获取网卡硬件地址 GetMacAddress:" + GetMacAddress());
        //    Console.WriteLine("获取网卡地址 GetNetCardMACAddress:" + GetNetCardMACAddress());
        //    Console.WriteLine("物理内存  GetPhysicalMemory:" + GetPhysicalMemory());
        //    Console.WriteLine("声卡PNPDeviceID  GetSoundPNPID:" + GetSoundPNPID());
        //    Console.WriteLine("操作系统类型  GetSystemType:" + GetSystemType());
        //    Console.WriteLine("操作系统的登录用户名 GetUserName:" + GetUserName);
        //    Console.WriteLine("显卡PNPDeviceID GetVideoPNPID:" + GetVideoPNPID());
        //}
        /// <summary>
        /// 操作系统的登录用户名
        /// </summary>
        public string GetUserName { get { return Environment.UserName; } }

        /// <summary>
        /// 获取计算机名
        /// </summary>
        /// <returns></returns>
        public string GetComputerName { get { return Environment.MachineName; } }

        /// <summary>
        /// 获取CPU序列号
        /// </summary>
        /// <returns></returns>
        public string GetCPUSerialNumber()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Processor");
                string sCPUSerialNumber = "";
                foreach (ManagementObject mo in searcher.Get()) { sCPUSerialNumber = mo["ProcessorId"].ToString().Trim(); }
                return sCPUSerialNumber;
            }
            catch { return ""; }
        }

        /// <summary>
        /// 获取硬盘序列号
        /// </summary>
        /// <returns></returns>
        public string GetHardDiskSerialNumber()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
                string sHardDiskSerialNumber = "";
                foreach (ManagementObject mo in searcher.Get()) { sHardDiskSerialNumber = mo["SerialNumber"].ToString().Trim(); break; }
                return sHardDiskSerialNumber;
            }
            catch { return ""; }
        }

        /// <summary>
        /// 获取网卡地址
        /// </summary>
        /// <returns></returns>
        public string GetNetCardMACAddress()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE ((MACAddress Is Not NULL) AND (Manufacturer <> 'Microsoft'))");
                string NetCardMACAddress = "";
                foreach (ManagementObject mo in searcher.Get()) { NetCardMACAddress = mo["MACAddress"].ToString().Trim(); }
                return NetCardMACAddress;
            }
            catch { return ""; }
        }

        /// <summary>
        /// 获得CPU编号
        /// </summary>
        /// <returns></returns>
        public string GetCPUID()
        {
            string cpuid = "";
            try
            {
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc) { cpuid = mo.Properties["ProcessorId"].Value.ToString(); }
            }
            catch { return ""; }
            return cpuid;
        }

        /// <summary>
        /// 获取硬盘序列号
        /// </summary>
        /// <returns></returns>
        public string GetDiskSerialNumber()
        {
            //这种模式在插入一个U盘后可能会有不同的结果，如插入我的手机时  
            String HDid = "";
            ManagementClass mc = new ManagementClass("Win32_DiskDrive");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                HDid = (string)mo.Properties["Model"].Value;//SerialNumber  
                break;//这名话解决有多个物理盘时产生的问题，只取第一个物理硬盘  
            }
            return HDid;
            /*ManagementClass mc = new ManagementClass("Win32_PhysicalMedia");
            ManagementObjectCollection moc = mc.GetInstances();
            string str = "";
            foreach (ManagementObject mo in moc)
            {
                str = mo.Properties["SerialNumber"].Value.ToString();
                break;
            }
            return str;*/
        }

        /// <summary>
        /// 获取网卡硬件地址
        /// </summary>
        /// <returns></returns>
        public string GetMacAddress1()
        {
            string mac = "";
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"] == true)
                { mac = mo["MacAddress"].ToString(); break; }
            }
            return mac;
        }

        /// <summary>
        /// 获取IP地址
        /// </summary>
        /// <returns></returns>
        public string GetIPAddress()
        {
            string st = "";
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"] == true)
                {
                    //st=mo["IpAddress"].ToString();
                    System.Array ar;
                    ar = (System.Array)(mo.Properties["IpAddress"].Value);
                    st = ar.GetValue(0).ToString();
                    break;
                }
            }
            return st;
        }

        /// <summary>
        /// 操作系统类型
        /// </summary>
        /// <returns></returns>
        public string GetSystemType()
        {
            string st = "";
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc) { st = mo["SystemType"].ToString(); }
            return st;
        }

        /// <summary>
        /// 物理内存
        /// </summary>
        /// <returns></returns>
        public string GetPhysicalMemory()
        {
            string st = "";
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc) { st = mo["TotalPhysicalMemory"].ToString(); }
            return st;
        }

        /// <summary>
        /// 显卡PNPDeviceID
        /// </summary>
        /// <returns></returns>
        public string GetVideoPNPID()
        {
            string st = "";
            ManagementObjectSearcher mos = new ManagementObjectSearcher("Select * from Win32_VideoController");
            foreach (ManagementObject mo in mos.Get()) { st = mo["PNPDeviceID"].ToString(); }
            return st;
        }

        /// <summary>
        /// 声卡PNPDeviceID
        /// </summary>
        /// <returns></returns>
        public string GetSoundPNPID()
        {
            string st = "";
            ManagementObjectSearcher mos = new ManagementObjectSearcher("Select * from Win32_SoundDevice");
            foreach (ManagementObject mo in mos.Get()) { st = mo["PNPDeviceID"].ToString(); }
            return st;
        }

        /// <summary>
        /// CPU版本信息
        /// </summary>
        /// <returns></returns>
        public static string GetCPUVersion()
        {
            string st = "";
            ManagementObjectSearcher mos = new ManagementObjectSearcher("Select * from Win32_Processor");
            foreach (ManagementObject mo in mos.Get()) { st = mo["Version"].ToString(); }
            return st;
        }

        /// <summary>
        /// CPU名称信息
        /// </summary>
        /// <returns></returns>
        public string GetCPUName()
        {
            string st = "";
            ManagementObjectSearcher driveID = new ManagementObjectSearcher("Select * from Win32_Processor");
            foreach (ManagementObject mo in driveID.Get()) { st = mo["Name"].ToString(); }
            return st;
        }

        /// <summary>
        /// CPU制造厂商
        /// </summary>
        /// <returns></returns>
        public string GetCPUManufacturer()
        {
            string st = "";
            ManagementObjectSearcher mos = new ManagementObjectSearcher("Select * from Win32_Processor");
            foreach (ManagementObject mo in mos.Get()) { st = mo["Manufacturer"].ToString(); }
            return st;
        }

        /// <summary>
        /// 主板制造厂商
        /// </summary>
        /// <returns></returns>
        public string GetBoardManufacturer()
        {
            SelectQuery query = new SelectQuery("Select * from Win32_BaseBoard");
            ManagementObjectSearcher mos = new ManagementObjectSearcher(query);
            ManagementObjectCollection.ManagementObjectEnumerator data = mos.Get().GetEnumerator();
            data.MoveNext();
            ManagementBaseObject board = data.Current;
            return board.GetPropertyValue("Manufacturer").ToString();
        }

        /// <summary>
        /// 主板编号
        /// </summary>
        /// <returns></returns>
        public string GetBoardID()
        {
            string st = "";
            ManagementObjectSearcher mos = new ManagementObjectSearcher("Select * from Win32_BaseBoard");
            foreach (ManagementObject mo in mos.Get()) { st = mo["SerialNumber"].ToString(); }
            return st;
        }

        /// <summary>
        /// 主板型号
        /// </summary>
        /// <returns></returns>
        public string GetBoardType()
        {
            string st = "";
            ManagementObjectSearcher mos = new ManagementObjectSearcher("Select * from Win32_BaseBoard");
            foreach (ManagementObject mo in mos.Get()) { st = mo["Product"].ToString(); }
            return st;
        }

        /// <summary>
        /// 获取Windows文件时间 132156874462559390
        /// </summary>
        /// <returns></returns>
        public long GetTimeNo()
        {
            try { return DateTime.Now.ToFileTimeUtc(); }
            catch (Exception ex) { return 0; }
        }
        /// <summary>
        /// 获取与当前线程关联的人员的用户名
        /// </summary>
        /// <returns></returns>
        public string ChatListBox()
        {
            try { return Environment.UserName; }
            catch (Exception ex) { return ""; }
        }
        /// <summary>
        /// 获取与当前用户关联的网络域名
        /// </summary>
        /// <returns></returns>
        public string GetUserDomainName()
        {
            try { return Environment.UserDomainName; }
            catch (Exception ex) { return ""; }
        }
        /// <summary>
        /// 获取本地计算机的主机名
        /// </summary>
        /// <returns></returns>
        public string GetHostName()
        {
            try { return Dns.GetHostName(); }
            catch (Exception ex) { return ""; }
        }
        /// <summary>
        /// 获取ip
        /// </summary>
        /// <returns></returns>
        public string GetLocalIP()
        {
            try
            {
                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork) { return IpEntry.AddressList[i].ToString(); }
                }
                return "";
            }
            catch (Exception ex) { return ""; }
        }
        /// <summary>  
        /// 获取本机MAC地址  
        /// </summary>  
        /// <returns>本机MAC地址</returns>  
        public string GetMacAddress()
        {
            try
            {
                string strMac = string.Empty;
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true) { strMac = mo["MacAddress"].ToString(); }
                }
                moc = null;
                mc = null;
                return strMac;
            }
            catch { return "unknown"; }
        }
        /// <summary>  
        /// 获取本机的物理地址  
        /// </summary>  
        /// <returns></returns>  
        public string getMacAddr_Local()
        {
            string madAddr = null;
            try
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc2 = mc.GetInstances();
                foreach (ManagementObject mo in moc2)
                {
                    if (Convert.ToBoolean(mo["IPEnabled"]) == true)
                    {
                        madAddr = mo["MacAddress"].ToString();
                        madAddr = madAddr.Replace(':', '-');
                    }
                    mo.Dispose();
                }
                if (madAddr == null) { return "unknown"; }
                else { return madAddr; }
            }
            catch (Exception) { return "unknown"; }
        }
        /// <summary>  
        /// 获取客户端内网IPv6地址  
        /// </summary>  
        /// <returns>客户端内网IPv6地址</returns>  
        public string GetClientLocalIPv6Address()
        {
            string strLocalIP = string.Empty;
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHost.AddressList[0];
                strLocalIP = ipAddress.ToString();
                return strLocalIP;
            }
            catch { return "unknown"; }
        }
        /// <summary>  
        /// 获取客户端内网IPv4地址  
        /// </summary>  
        /// <returns>客户端内网IPv4地址</returns>  
        public string GetClientLocalIPv4Address()
        {
            string strLocalIP = string.Empty;
            try
            {
                IPHostEntry ipHost = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddress = ipHost.AddressList[0];
                strLocalIP = ipAddress.ToString();
                return strLocalIP;
            }
            catch { return "unknown"; }
        }
        /// <summary>  
        /// 获取客户端内网IPv4地址集合  
        /// </summary>  
        /// <returns>返回客户端内网IPv4地址集合</returns>  
        public List<string> GetClientLocalIPv4AddressList()
        {
            List<string> ipAddressList = new List<string>();
            try
            {
                IPHostEntry ipHost = Dns.Resolve(Dns.GetHostName());
                foreach (IPAddress ipAddress in ipHost.AddressList)
                {
                    if (!ipAddressList.Contains(ipAddress.ToString()))
                    {
                        ipAddressList.Add(ipAddress.ToString());
                    }
                }
            }
            catch { return null; }
            return ipAddressList;
        }

        /// <summary>  
        /// 获取本机公网IP地址  
        /// </summary>  
        /// <returns>本机公网IP地址</returns>  
        private string GetClientInternetIPAddress2()
        {
            //string tempip = "";
            //try
            //{
            //    //http://iframe.ip138.com/ic.asp 返回的是：您的IP是：[220.231.17.99] 来自：北京市 光环新网  
            //    WebRequest wr = WebRequest.Create("http://iframe.ip138.com/ic.asp");
            //    Stream s = wr.GetResponse().GetResponseStream();
            //    StreamReader sr = new StreamReader(s, Encoding.Default);
            //    string all = sr.ReadToEnd(); //读取网站的数据  

            //    int start = all.IndexOf("[") + 1;
            //    int end = all.IndexOf("]", start);
            //    tempip = all.Substring(start, end - start);
            //    sr.Close();
            //    s.Close();
            //    return tempip;
            //}
            //catch
            //{
            return "unknown";
            //}
        }
        /// <summary>  
        /// 获取硬盘序号  
        /// </summary>  
        /// <returns>硬盘序号</returns>  
        public string GetDiskID()
        {
            try
            {
                string strDiskID = string.Empty;
                ManagementClass mc = new ManagementClass("Win32_DiskDrive");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    strDiskID = mo.Properties["Model"].Value.ToString();
                }
                moc = null;
                mc = null;
                return strDiskID;
            }
            catch { return "unknown"; }
        }
        /// <summary>  
        /// 获取CpuID  
        /// </summary>  
        /// <returns>CpuID</returns>  
        public string GetCpuID()
        {
            try
            {
                string strCpuID = string.Empty;
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    strCpuID = mo.Properties["ProcessorId"].Value.ToString();
                }
                moc = null;
                mc = null;
                return strCpuID;
            }
            catch { return "unknown"; }
        }
        /// <summary>  
        /// 获取操作系统类型  
        /// </summary>  
        /// <returns>操作系统类型</returns>  
        public string GetSystemType1()
        {
            try
            {
                string strSystemType = string.Empty;
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    strSystemType = mo["SystemType"].ToString();
                }
                moc = null;
                mc = null;
                return strSystemType;
            }
            catch { return "unknown"; }
        }
        /// <summary>  
        /// 获取操作系统名称  
        /// </summary>  
        /// <returns>操作系统名称</returns>  
        public string GetSystemName()
        {
            try
            {
                string strSystemName = string.Empty;
                ManagementObjectSearcher mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT PartComponent FROM Win32_SystemOperatingSystem");
                foreach (ManagementObject mo in mos.Get())
                {
                    strSystemName = mo["PartComponent"].ToString();
                }
                mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT Caption FROM Win32_OperatingSystem");
                foreach (ManagementObject mo in mos.Get())
                {
                    strSystemName = mo["Caption"].ToString();
                }
                return strSystemName;
            }
            catch { return "unknown"; }
        }
        /// <summary>  
        /// 获取物理内存信息  
        /// </summary>  
        /// <returns>物理内存信息</returns>  
        public string GetTotalPhysicalMemory()
        {
            try
            {
                string strTotalPhysicalMemory = string.Empty;
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    strTotalPhysicalMemory = mo["TotalPhysicalMemory"].ToString();
                }
                moc = null;
                mc = null;
                return strTotalPhysicalMemory;
            }
            catch { return "unknown"; }
        }

        /// <summary>  
        /// 获取主板id  
        /// </summary>  
        /// <returns></returns>  
        public string GetMotherBoardID()
        {
            try
            {
                ManagementClass mc = new ManagementClass("Win32_BaseBoard");
                ManagementObjectCollection moc = mc.GetInstances();
                string strID = null;
                foreach (ManagementObject mo in moc)
                {
                    strID = mo.Properties["SerialNumber"].Value.ToString();
                    break;
                }
                return strID;
            }
            catch { return "unknown"; }
        }

    }
}
