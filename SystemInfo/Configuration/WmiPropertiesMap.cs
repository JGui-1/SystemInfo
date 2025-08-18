using System.Collections.Generic;

namespace SystemInfo.Configuration
{
    public static class WmiPropertiesMap
    {
        public static readonly Dictionary<string, string[]> Main = new()
        {
            { "CPU", new[] { "Name", "NumberOfCores", "NumberOfLogicalProcessors", "MaxClockSpeed", "ProcessorId" } },
            { "GPU", new[] { "Name", "AdapterRAM", "DriverVersion", "VideoProcessor" } },
            { "Memory", new[] { "Capacity", "Speed", "Manufacturer", "SerialNumber", "SMBIOSMemoryType" } },
            { "OperatingSystem", new[] { "Caption", "Version", "OSArchitecture", "SerialNumber", "LastBootUpTime" } },
            { "LogicalDisk", new[] { "Name", "Size", "FreeSpace", "FileSystem" } },
            { "DiskDrive", new[] { "Model", "Size", "InterfaceType", "SerialNumber" } },
            { "PhysicalMedia", new[] { "SerialNumber" } },
            { "Network", new[] { "Description", "MACAddress", "IPEnabled", "DHCPEnabled", "IPAddress", "ServiceName", "SettingID" } },
            { "NetworkAdapter", new[] { "Description", "MACAddress", "IPEnabled", "DHCPEnabled", "IPAddress", "ServiceName", "SettingID" } },
            { "Battery", new[] { "Name", "BatteryStatus", "EstimatedChargeRemaining", "EstimatedRunTime" } },
            { "BaseBoard", new[] { "Product", "Manufacturer", "SerialNumber", "Version" } },
            { "BIOS", new[] { "Manufacturer", "SMBIOSBIOSVersion", "SerialNumber", "ReleaseDate" } },
            { "ComputerSystem", new[] { "Manufacturer", "Model", "TotalPhysicalMemory", "UserName" } },
            { "DiskPartition", new[] { "DeviceID", "Index", "BootPartition", "PrimaryPartition", "DiskIndex", "Size", "Name" } },
            { "Service", new[] { "Name", "DisplayName", "State", "Status", "StartMode", "AcceptStop", "ProcessId", "PathName" } },
            { "BaseService", new[] { "Name", "DisplayName", "State", "Status", "StartMode", "AcceptStop", "ProcessId", "PathName" } },
        };
    }
}
