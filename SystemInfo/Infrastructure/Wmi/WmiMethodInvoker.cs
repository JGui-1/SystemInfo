using System.Collections.Generic;
using System.Management;
using SystemInfo.Core.Abstractions;
using SystemInfo.Configuration;

namespace SystemInfo.Infrastructure.Wmi
{
    public class WmiMethodInvoker : IWmiMethodInvoker
    {
        public IDictionary<string, object> Invoke(string alias, string methodName, string whereClause, IDictionary<string, object> parameters = null)
        {
            var (ns, cls) = WmiClassMap.Get(alias);
            string q = $"SELECT * FROM {cls}" + (whereClause != null ? " WHERE " + whereClause : "");
            var results = new Dictionary<string, object>();
            using var searcher = new ManagementObjectSearcher(ns, q);
            foreach (ManagementObject obj in searcher.Get())
            {
                ManagementBaseObject inParams = null;
                if (parameters != null)
                {
                    try
                    {
                        inParams = obj.GetMethodParameters(methodName);
                        foreach (var kv in parameters)
                        {
                            if (inParams.Properties[kv.Key] != null)
                                inParams[kv.Key] = kv.Value;
                        }
                    }
                    catch
                    {
                        inParams = null;
                    }
                }

                var outParams = obj.InvokeMethod(methodName, inParams, null);
                object returnValue = null;
                try { returnValue = outParams?["ReturnValue"]; } catch { }

                var key = obj.Properties["Name"]?.Value?.ToString() ?? obj.Path?.RelativePath ?? cls;
                results[key] = returnValue ?? outParams;
            }
            return results;
        }
    }
}
