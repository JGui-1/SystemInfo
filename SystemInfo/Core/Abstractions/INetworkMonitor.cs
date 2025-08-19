using System.Collections.Generic;

namespace SystemInfo.Core.Abstractions
{
    public interface INetworkMonitor
    {
        IEnumerable<(string Adapter, float BytesSentPerSec, float BytesReceivedPerSec)> GetTrafficSnapshot();
        IEnumerable<(string Local, string Remote, string State)> GetActiveConnections();
    }
}
