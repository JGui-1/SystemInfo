using Microsoft.Win32;
using System.Collections.Generic;

namespace SystemInfo.Infrastructure.System
{
    public static class PolicyDefinitions
    {
        public class Target
        {
            public RegistryHive Hive { get; init; }
            public string Path { get; init; } = "";
            public string ValueName { get; init; } = "";
            public RegistryValueKind Kind { get; init; } = RegistryValueKind.DWord;
            public int? EnabledValue { get; init; }
            public int? DisabledValue { get; init; }
            public bool DeleteOnNotConfigured { get; init; } = true;
        }

        public class PolicyDef
        {
            public string Key { get; init; } = "";
            public List<Target> Targets { get; init; } = new();
            public bool EnabledRequiresValue { get; init; }
        }

        public static readonly Dictionary<string, PolicyDef> Known = new()
        {
            // QoS: NonBestEffortLimit
            ["NonBestEffortLimit"] = new PolicyDef
            {
                Key = "NonBestEffortLimit",
                EnabledRequiresValue = true,
                Targets = new()
                {
                    new Target
                    {
                        Hive = RegistryHive.LocalMachine,
                        Path = @"SOFTWARE\Policies\Microsoft\Windows\Psched",
                        ValueName = "NonBestEffortLimit",
                        Kind = RegistryValueKind.DWord,
                        EnabledValue = 0,
                        DisabledValue = 80
                    }
                }
            },

            // TimerResolution
            ["TimerResolution"] = new PolicyDef
            {
                Key = "TimerResolution",
                EnabledRequiresValue = true,
                Targets = new()
                {
                    new Target
                    {
                        Hive = RegistryHive.LocalMachine,
                        Path = @"SOFTWARE\Policies\Microsoft\Windows\System",
                        ValueName = "TimerResolution",
                        Kind = RegistryValueKind.DWord,
                        EnabledValue = 10,
                        DisabledValue = 0
                    }
                }
            },

            // Telemetria
            ["AllowTelemetry"] = new PolicyDef
            {
                Key = "AllowTelemetry",
                EnabledRequiresValue = true,
                Targets = new()
                {
                    new Target
                    {
                        Hive = RegistryHive.LocalMachine,
                        Path = @"SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                        ValueName = "AllowTelemetry",
                        Kind = RegistryValueKind.DWord,
                        EnabledValue = 0,
                        DisabledValue = 3
                    }
                }
            },

            // Advertising ID
            ["AdvertisingId"] = new PolicyDef
            {
                Key = "AdvertisingId",
                Targets = new()
                {
                    new Target
                    {
                        Hive = RegistryHive.CurrentUser,
                        Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo",
                        ValueName = "Enabled",
                        Kind = RegistryValueKind.DWord,
                        EnabledValue = 0,
                        DisabledValue = 1
                    }
                }
            },

            // Cortana
            ["Cortana"] = new PolicyDef
            {
                Key = "Cortana",
                Targets = new()
                {
                    new Target
                    {
                        Hive = RegistryHive.LocalMachine,
                        Path = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
                        ValueName = "AllowCortana",
                        Kind = RegistryValueKind.DWord,
                        EnabledValue = 0,
                        DisabledValue = 1
                    }
                }
            },

            // Sugestões e dicas
            ["CloudContent.TipsAndSuggestions"] = new PolicyDef
            {
                Key = "CloudContent.TipsAndSuggestions",
                Targets = new()
                {
                    new Target
                    {
                        Hive = RegistryHive.CurrentUser,
                        Path = @"SOFTWARE\Policies\Microsoft\Windows\CloudContent",
                        ValueName = "DisableSoftLanding",
                        Kind = RegistryValueKind.DWord,
                        EnabledValue = 1,
                        DisabledValue = 0
                    }
                }
            },

            // Windows Update opções automáticas
            ["WindowsUpdate.AUOptions"] = new PolicyDef
            {
                Key = "WindowsUpdate.AUOptions",
                EnabledRequiresValue = true,
                Targets = new()
                {
                    new Target
                    {
                        Hive = RegistryHive.LocalMachine,
                        Path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                        ValueName = "AUOptions",
                        Kind = RegistryValueKind.DWord,
                        EnabledValue = 2,
                        DisabledValue = 3
                    }
                }
            },

            // Não reiniciar com usuário logado
            ["WindowsUpdate.NoAutoRebootWithLoggedOnUsers"] = new PolicyDef
            {
                Key = "WindowsUpdate.NoAutoRebootWithLoggedOnUsers",
                Targets = new()
                {
                    new Target
                    {
                        Hive = RegistryHive.LocalMachine,
                        Path = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU",
                        ValueName = "NoAutoRebootWithLoggedOnUsers",
                        Kind = RegistryValueKind.DWord,
                        EnabledValue = 1,
                        DisabledValue = 0
                    }
                }
            },

            // Delivery Optimization
            ["DeliveryOptimization.Mode"] = new PolicyDef
            {
                Key = "DeliveryOptimization.Mode",
                EnabledRequiresValue = true,
                Targets = new()
                {
                    new Target
                    {
                        Hive = RegistryHive.LocalMachine,
                        Path = @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization",
                        ValueName = "DODownloadMode",
                        Kind = RegistryValueKind.DWord,
                        EnabledValue = 0,
                        DisabledValue = 1
                    }
                }
            },

            // BITS - limite de banda
            ["BITS.MaxBandwidthKBps"] = new PolicyDef
            {
                Key = "BITS.MaxBandwidthKBps",
                EnabledRequiresValue = true,
                Targets = new()
                {
                    new Target
                    {
                        Hive = RegistryHive.LocalMachine,
                        Path = @"SOFTWARE\Policies\Microsoft\Windows\BITS",
                        ValueName = "MaxDownloadBandwidth",
                        Kind = RegistryValueKind.DWord,
                        EnabledValue = 50,
                        DisabledValue = null
                    }
                }
            }
        };

        public static IEnumerable<string> KnownKeys => Known.Keys;
    }
}
