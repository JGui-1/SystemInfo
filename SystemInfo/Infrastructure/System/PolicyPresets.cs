using System.Collections.Generic;

namespace SystemInfo.Infrastructure.System
{
    /// <summary>
    /// Presets de políticas (coleções pré-definidas).
    /// </summary>
    public static class PolicyPresets
    {
        public static readonly Dictionary<string, Dictionary<string, (PolicyState, int?)>> Presets
            = new()
            {
                ["Desempenho"] = new()
                {
                    ["NonBestEffortLimit"] = (PolicyState.Enabled, 0),
                    ["TimerResolution"] = (PolicyState.Enabled, 0)
                },

                ["Privacidade"] = new()
                {
                    ["AllowTelemetry"] = (PolicyState.Enabled, 0),
                    ["AdvertisingId"] = (PolicyState.Enabled, null),
                    ["Cortana"] = (PolicyState.Enabled, null),
                    ["CloudContent.TipsAndSuggestions"] = (PolicyState.Enabled, null)
                },

                ["UpdateRestrito"] = new()
                {
                    ["WindowsUpdate.AUOptions"] = (PolicyState.Enabled, 2),
                    ["WindowsUpdate.NoAutoRebootWithLoggedOnUsers"] = (PolicyState.Enabled, null),
                    ["DeliveryOptimization.Mode"] = (PolicyState.Enabled, 0),
                    ["BITS.MaxBandwidthKBps"] = (PolicyState.Enabled, 50)
                }
            };
    }
}
