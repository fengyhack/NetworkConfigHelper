//////////////////////////////////////////////////////////////////////////////////////
//
//  NetworkConfigurationHelper by fengyh
//  This comment MUST be included in any copy of this code.
//  Author: https://fengyh.cn
//
//////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using NetworkAdapter;

namespace Demo
{
    static class Program
    {
        static void Main(string[] args)
        {
            var configs = NetworkConfigHelper.GetAdapterConfigurations();
            int i = 0;
            Console.WriteLine("Network adapters:");
            foreach(var cfg in configs.Values)
            {
                Console.WriteLine($"[{++i}] {cfg.AdapterName}: {cfg.IP}");
            }
            Console.WriteLine();

            Console.Write($"Which adapter you want to config? Index(1~{i}):");
            var s = Console.ReadLine();
            int idx = int.Parse(s);

            if (idx >= 1 && idx <= i)
            {
                var name = configs.ElementAt(idx - 1).Key;
                Console.WriteLine($"You choosed: {name}");

                NetworkConfigHelper.DisableAdapter(name);
                Console.WriteLine("The chosen adapter is now disabled");

                Console.WriteLine("Please input the configurations as below");
                Console.Write("IP:");
                var ip = Console.ReadLine();
                Console.Write("Mask:");
                var mask = Console.ReadLine();
                Console.Write("Gateway:");
                var gateway = Console.ReadLine();

                NetworkConfigHelper.EnableAdapter(name);
                Console.WriteLine("The chosen adapter is now enabled");
                var config = new NetworkConfig() { IP = ip, Mask = mask, Gateway = gateway };
                NetworkConfigHelper.SetAdapterConfiguration(name, config);
                Console.WriteLine();

                Console.WriteLine("New configuration:");
                var cfg = NetworkConfigHelper.GetAdapterConfiguration(name);
                Console.WriteLine($"{cfg.AdapterName}: {cfg.IP}");
            }
            else
            {
                Console.WriteLine("Invalid index");
            }

            Console.ReadLine();
        }
    }
}
