using System;
using System.Collections.Generic;

namespace SystemInfo.Configuration
{
    public static class WmiClassMap
    {
        private static readonly Dictionary<string, (string Namespace, string ClassName)> ClassMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "CPU", ("root\\CIMV2", "Win32_Processor") },
                { "GPU", ("root\\CIMV2", "Win32_VideoController") },
                { "Memory", ("root\\CIMV2", "Win32_PhysicalMemory") },
                { "OperatingSystem", ("root\\CIMV2", "Win32_OperatingSystem") },
                { "LogicalDisk", ("root\\CIMV2", "Win32_LogicalDisk") },
                { "DiskDrive", ("root\\CIMV2", "Win32_DiskDrive") },
                { "PhysicalMedia", ("root\\CIMV2", "Win32_PhysicalMedia") },
                { "Network", ("root\\CIMV2", "Win32_NetworkAdapterConfiguration") },
                { "NetworkAdapter", ("root\\CIMV2", "Win32_NetworkAdapterConfiguration") },
                { "Battery", ("root\\CIMV2", "Win32_Battery") },
                { "BaseBoard", ("root\\CIMV2", "Win32_BaseBoard") },
                { "BIOS", ("root\\CIMV2", "Win32_BIOS") },
                { "ComputerSystem", ("root\\CIMV2", "Win32_ComputerSystem") },
                { "DiskPartition", ("root\\CIMV2", "Win32_DiskPartition") },
                { "Service", ("root\\CIMV2", "Win32_Service") },
                { "BaseService", ("root\\CIMV2", "Win32_Service") },
            };

        public static (string Namespace, string ClassName) Get(string alias)
        {
            if (ClassMap.TryGetValue(alias, out var m)) return m;
            throw new ArgumentException($"Alias WMI '{alias}' não mapeado.");
        }

        public static IEnumerable<string> GetAllAliases() => ClassMap.Keys;
    }
}
