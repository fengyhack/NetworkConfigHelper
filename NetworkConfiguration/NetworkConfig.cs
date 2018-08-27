//////////////////////////////////////////////////////////////////////////////////////
//
//  NetworkConfigurationHelper by fengyh
//  This comment MUST be included in any copy of this code.
//  Author: https://fengyh.cn
//
//////////////////////////////////////////////////////////////////////////////////////

using System.Net.NetworkInformation;

namespace NetworkAdapter
{
    /// <summary>
    /// 网卡配置信息
    /// </summary>
    public class NetworkConfig
    {
        /// <summary>
        /// 网卡名称(NIC Description)
        /// </summary>
        public string AdapterName { get; private set; }

        /// <summary>
        /// 连接类型：是否Ethernet
        /// </summary>
        public bool IsEthernet { get; private set; }

        /// <summary>
        /// IP地址[必需]
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 子网掩码[必需]
        /// </summary>
        public string Mask { get; set; }

        /// <summary>
        /// 默认网关
        /// </summary>
        public string Gateway { get; set; }

        /// <summary>
        /// 主要DNS
        /// </summary>
        public string DNS { get; set; }

        /// <summary>
        /// 备用DNS
        /// </summary>
        public string DNS2 { get; set; }

        /// <param name="name">网卡名称</param>
        public void SetAdapterName(string name)
        {
            AdapterName = name;
        }

        /// <param name="type">连接类型</param>
        public void SetInterfaceType(NetworkInterfaceType type)
        {
            IsEthernet = (type == NetworkInterfaceType.Ethernet);
        }
    }
}
