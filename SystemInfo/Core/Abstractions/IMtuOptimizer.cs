using System.Net.NetworkInformation;

namespace SystemInfo.Core.Abstractions
{
    public interface IMtuOptimizer
    {
        int CalculateOptimalMtu(NetworkInterface iface);
        void SetMTU(string index, string mtu);
    }
}
