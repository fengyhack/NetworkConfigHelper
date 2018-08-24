using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Management;
using System.Text.RegularExpressions;

namespace NetworkAdapter
{
    public static class NetworkConfigHelper
    {
        /// <summary>
        /// 获取网卡信息列表（名称:网络配置）
        /// </summary>
        public static Dictionary<string, NetworkConfig> GetAdapterConfigurations()
        {
            var nwInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            var adapterNames = GetAdapterNames(nwInterfaces);
            return GetAdapterConfigurations(nwInterfaces, adapterNames);
        }

        /// <summary>
        /// 对指定网卡进行配置
        /// </summary>
        /// <param name="adapterName">网卡名称</param>
        /// <param name="config">网络配置</param>
        public static void SetAdapterConfiguration(string adapterName, NetworkConfig config)
        {
            var wmi = new ManagementClass("Win32_NetworkAdapterConfiguration");
            var moc = wmi.GetInstances();
            var found = false;
            foreach (var m in moc)
            {
                var mo = m as ManagementObject;

                foreach (var p in mo.Properties)
                {
                    if (p.Value != null)
                    {
                        var desc = p.Value.ToString();
                        if (p.Name.Equals("Description") && adapterName.Equals(desc))
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (found)
                {
                    //if (!(bool)mo["IPEnabled"])
                    //{
                    //}

                    if (IsValidIPAddr(config.IP) && IsValidMask(config.Mask))
                    {
                        var parameter = mo.GetMethodParameters("EnableStatic");
                        parameter["IPAddress"] = new string[] { config.IP };
                        parameter["SubnetMask"] = new string[] { config.Mask };
                        mo.InvokeMethod("EnableStatic", parameter, null);
                    }
                    else
                    {
                        // IP地址/子网掩码 是必须设置的
                        throw new Exception($"Invalid IP/Mask: IP = {config.IP}, Mask = {config.Mask}");
                    }

                    if (IsValidIPAddr(config.Gateway))
                    {
                        var parameter = mo.GetMethodParameters("SetGateways");
                        parameter["DefaultIPGateway"] = new string[] { config.Gateway };
                        mo.InvokeMethod("SetGateways", parameter, null);
                    }
                    else
                    {
                        //throw new Exception($"Invalid Gateway: Gateway = {config.Gateway}");
                    }

                    if (IsValidIPAddr(config.DNS) && IsValidIPAddr(config.DNS2))
                    {
                        var parameter = mo.GetMethodParameters("SetDNSServerSearchOrder");
                        parameter["DNSServerSearchOrder"] = new string[] { config.DNS, config.DNS2 };
                        mo.InvokeMethod("SetDNSServerSearchOrder", parameter, null);
                    }
                    else
                    {
                        //throw new Exception($"Invalid DNS: DNS = {config.DNS}, DNS2 = {config.DNS2}");
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// 是否合法的IP地址(点分十进制格式，例如127.0.0.1等)
        /// </summary>
        public static bool IsValidIPAddr(string ip)
        {
            var regx = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");
            return (!string.IsNullOrEmpty(ip)) && regx.IsMatch(ip);
        }

        /// <summary>
        /// 是否合法的子网掩码（点分十进制格式，例如255.255.255.0等）
        /// </summary>
        public static bool IsValidMask(string mask)
        {
            string[] vList = mask.Split('.');
            if (vList.Length != 4)
            {
                return false;
            }

            bool vZero = false; // 出现0 
            for (int j = 0; j < vList.Length; j++)
            {
                int i;
                if (!int.TryParse(vList[j], out i))
                {
                    return false;
                }

                if ((i < 0) || (i > 255))
                {
                    return false;
                }

                if (vZero)
                {
                    if (i != 0)
                    {
                        return false;
                    }
                }
                else
                {
                    for (int k = 7; k >= 0; k--)
                    {
                        if (((i >> k) & 1) == 0) // 出现0 
                        {
                            vZero = true;
                        }
                        else
                        {
                            if (vZero)
                            {
                                return false; // 不为0 
                            }
                        }
                    }
                }
            }

            return true;
        }

        #region Privates

        private static List<string> GetAdapterNames(NetworkInterface[] nwInterfaces)
        {
            var adapterNames = new List<string>();

            var adapterDescriptions = new List<string>();
            foreach (var adapter in nwInterfaces)
            {
                adapterDescriptions.Add(adapter.Description);
            }
            var wmi = new ManagementClass("Win32_NetworkAdapterConfiguration");
            var moc = wmi.GetInstances();
            foreach (var m in moc)
            {
                var mo = m as ManagementObject;
                foreach (var p in mo.Properties)
                {
                    if (p.Name.Equals("Description") && p.Value != null)
                    {
                        var desc = p.Value.ToString();
                        if (adapterDescriptions.Contains(desc))
                        {
                            adapterNames.Add(desc);
                            break;
                        }
                    }
                }

            }

            return adapterNames;
        }

        private static Dictionary<string, NetworkConfig> GetAdapterConfigurations(NetworkInterface[] nwInterfaces, List<string> adapterNames)
        {
            var configurations = new Dictionary<string, NetworkConfig>();

            foreach (var adapter in nwInterfaces)
            {
                if (adapterNames.Contains(adapter.Description))
                {
                    var config = new NetworkConfig();
                    config.SetAdapterName(adapter.Description);
                    config.SetInterfaceType(adapter.NetworkInterfaceType);
                    var ipp = adapter.GetIPProperties();
                    var ipCollection = ipp.UnicastAddresses;
                    foreach (var ipAddr in ipCollection)
                    {
                        if (ipAddr.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            config.IP = ipAddr.Address.ToString();
                            config.Mask = ipAddr.IPv4Mask.ToString();
                            if (ipp.GatewayAddresses.Count > 0)
                            {
                                config.Gateway = ipp.GatewayAddresses[0].Address.ToString();
                            }
                            break;
                        }
                    }
                    configurations.Add(config.AdapterName, config);
                }
            }

            return configurations;
        }

        #endregion Privates
    }
}
