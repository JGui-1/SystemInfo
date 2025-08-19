using System;
using System.Text;
using System.Collections.Generic;

namespace SystemInfo.Infrastructure.System
{
    public class PolicyManager
    {
        private readonly StealthRegistryApplier _applier = new();

        // Tabela de políticas → Hive, Path, Name
        private readonly Dictionary<string, (string hiveEnc, string pathEnc, string nameEnc)> _policies
            = new()
            {
                ["NonBestEffortLimit"] = (
                Encode("HKLM"),
                Encode(@"SOFTWARE\Policies\Microsoft\Windows\Psched"),
                Encode("NonBestEffortLimit")
            ),

                ["TimerResolution"] = (
                Encode("HKLM"),
                Encode(@"SOFTWARE\Policies\Microsoft\Windows\System"),
                Encode("TimerResolution")
            ),

                ["AllowTelemetry"] = (
                Encode("HKLM"),
                Encode(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection"),
                Encode("AllowTelemetry")
            ),

                ["AdvertisingId"] = (
                Encode("HKLM"),
                Encode(@"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo"),
                Encode("DisabledByGroupPolicy")
            ),

                ["Cortana"] = (
                Encode("HKLM"),
                Encode(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search"),
                Encode("AllowCortana")
            ),

                ["CloudContent.TipsAndSuggestions"] = (
                Encode("HKLM"),
                Encode(@"SOFTWARE\Policies\Microsoft\Windows\CloudContent"),
                Encode("DisableSoftLanding")
            ),

                ["WindowsUpdate.AUOptions"] = (
                Encode("HKLM"),
                Encode(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU"),
                Encode("AUOptions")
            ),

                ["WindowsUpdate.NoAutoRebootWithLoggedOnUsers"] = (
                Encode("HKLM"),
                Encode(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU"),
                Encode("NoAutoRebootWithLoggedOnUsers")
            ),

                ["DeliveryOptimization.Mode"] = (
                Encode("HKLM"),
                Encode(@"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization"),
                Encode("DODownloadMode")
            ),

                ["BITS.MaxBandwidthKBps"] = (
                Encode("HKLM"),
                Encode(@"SOFTWARE\Policies\Microsoft\Windows\BITS"),
                Encode("MaxInternetBandwidthKBps")
            )
            };

        // 🔐 Aplica uma política
        public void ApplyPolicy(string policy, PolicyState state, int? value = null)
        {
            if (!_policies.ContainsKey(policy))
            {
                Console.WriteLine($"[x] Política '{policy}' não registrada.");
                return;
            }

            var (hiveEnc, pathEnc, nameEnc) = _policies[policy];

            object regValue = state switch
            {
                PolicyState.Enabled when value.HasValue => value.Value,
                PolicyState.Enabled => 1,
                PolicyState.Disabled => 0,
                PolicyState.NotConfigured => 0,
                _ => 0
            };

            Console.WriteLine($"[✔] Aplicando {policy} = {regValue} ({state})");

            _applier.ApplyStealth(hiveEnc, pathEnc, nameEnc, regValue);
        }

        // ♻️ Rollback de todas as alterações
        public void Rollback()
        {
            Console.WriteLine("[↩] Restaurando valores originais...");
            _applier.Rollback();
        }

        // Helper para Base64
        private static string Encode(string s) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
    }

    public enum PolicyState
    {
        Enabled,
        Disabled,
        NotConfigured
    }
}
