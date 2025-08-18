using System.Collections.Generic;

namespace SystemInfo.Core.Abstractions
{
    public interface IServiceManager
    {
        IEnumerable<IDictionary<string, object>> ListRunning();
        IDictionary<string, object> StopByName(string serviceName);
        IDictionary<string, IDictionary<string, object>> StopManyByName(IEnumerable<string> serviceNames);
    }
}
