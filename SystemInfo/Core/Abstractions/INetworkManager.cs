using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace SystemInfo.Core.Abstractions
{
    public interface INetworkManager
    {
        Dictionary<string, InterfaceInfo> LoadInterfaces();
        void SaveInterfacesToFile(Dictionary<string, InterfaceInfo> interfaceData);
        NetworkInterface GetMainNetworkInterface();
    }

    public class InterfaceInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Index { get; set; }
        public string Mtu { get; set; }
    }
}
