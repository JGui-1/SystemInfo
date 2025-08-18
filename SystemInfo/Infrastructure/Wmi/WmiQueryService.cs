using System.Collections.Generic;
using System.Management;
using SystemInfo.Core.Abstractions;
using SystemInfo.Configuration;

namespace SystemInfo.Infrastructure.Wmi
{
    public class WmiQueryService : IWmiQueryService
    {
        public IEnumerable<IDictionary<string, object>> QueryAll(string alias, string whereClause = null)
        {
            var (ns, cls) = WmiClassMap.Get(alias);
            string q = $"SELECT * FROM {cls}" + (whereClause != null ? " WHERE " + whereClause : "");
            using var searcher = new ManagementObjectSearcher(ns, q);
            foreach (ManagementObject obj in searcher.Get())
            {
                var dict = new Dictionary<string, object>();
                foreach (PropertyData p in obj.Properties)
                    dict[p.Name] = p.Value;
                yield return dict;
            }
        }

        public IEnumerable<IDictionary<string, object>> QuerySelect(string alias, IEnumerable<string> properties, string whereClause = null)
        {
            var (ns, cls) = WmiClassMap.Get(alias);
            string select = string.Join(",", properties);
            string q = $"SELECT {select} FROM {cls}" + (whereClause != null ? " WHERE " + whereClause : "");
            using var searcher = new ManagementObjectSearcher(ns, q);
            foreach (ManagementObject obj in searcher.Get())
            {
                var dict = new Dictionary<string, object>();
                foreach (var p in properties)
                    dict[p] = obj[p];
                yield return dict;
            }
        }

        public string GetScalar(string alias, string property, string whereClause = null)
        {
            var (ns, cls) = WmiClassMap.Get(alias);
            string q = $"SELECT {property} FROM {cls}" + (whereClause != null ? " WHERE " + whereClause : "");
            using var searcher = new ManagementObjectSearcher(ns, q);
            foreach (ManagementObject obj in searcher.Get())
                return obj[property]?.ToString();
            return null;
        }
    }
}
