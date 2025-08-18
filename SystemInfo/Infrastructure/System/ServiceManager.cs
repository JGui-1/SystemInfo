using System.Collections.Generic;
using SystemInfo.Core.Abstractions;

namespace SystemInfo.Infrastructure.System
{
    public class ServiceManager : IServiceManager
    {
        private readonly IWmiQueryService _wmi;
        private readonly IWmiMethodInvoker _invoker;
        public ServiceManager(IWmiQueryService wmi, IWmiMethodInvoker invoker)
        {
            _wmi = wmi; _invoker = invoker;
        }

        public IEnumerable<IDictionary<string, object>> ListRunning() =>
            _wmi.QueryAll("Service", "State = 'Running'");

        public IDictionary<string, object> StopByName(string serviceName) =>
            _invoker.Invoke("Service", "StopService", $"Name = '{serviceName}'");

        public IDictionary<string, IDictionary<string, object>> StopManyByName(IEnumerable<string> serviceNames)
        {
            var result = new Dictionary<string, IDictionary<string, object>>();
            foreach (var name in serviceNames)
                result[name] = _invoker.Invoke("Service", "StopService", $"Name = '{name}'");
            return result;
        }
    }
}
