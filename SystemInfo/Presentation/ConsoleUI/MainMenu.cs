using System;
using System.Collections.Generic;
using SystemInfo.Presentation.Wpf;       // SupremeExecutor
using SystemInfo.Infrastructure.Tweaks; // Executor
using SystemInfo.Infrastructure.Output; // ConsoleOutput
using SystemInfo.Infrastructure.System; // PolicyPresets
using SystemInfo.Infrastructure.Network; // NetworkDuplexManager

namespace SystemInfo.Presentation.ConsoleUI
{
    public class MainMenu
    {
        private readonly SupremeExecutor _executor;

        public MainMenu()
        {
            // Console precisa de um IOutput interativo (ConsoleOutput).
            _executor = new SupremeExecutor(new ConsoleOutput());
        }

        public void Show()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== SystemInfo Executor Menu ===");
                Console.WriteLine("1  - Compress Working Sets");
                Console.WriteLine("2  - Clean Temp Files");
                Console.WriteLine("3  - Stop Useless Services");
                Console.WriteLine("4  - Apply Best DNS");
                Console.WriteLine("5  - Optimize MTU");
                Console.WriteLine("6  - Full Memory Optimization");
                Console.WriteLine("7  - Defrag Disk C:");
                Console.WriteLine("8  - List Running Services");
                Console.WriteLine("9  - Show System Fingerprint");
                Console.WriteLine("10 - Sample CPU Usage");
                Console.WriteLine("11 - Windows Tweaks");
                Console.WriteLine("12 - Apply QoS Policy (Desempenho)");
                Console.WriteLine("13 - Apply Privacy Policies");
                Console.WriteLine("14 - Apply Update Restrito Policies");
                Console.WriteLine("15 - Apply ALL Presets (interactive confirm)");
                Console.WriteLine("16 - Rollback Policies");
                Console.WriteLine("17 - Configurar SpeedDuplex (1Gbps Full)");
                Console.WriteLine("18 - QoS Manager");
                Console.WriteLine("0  - Exit");
                Console.Write("\nEscolha: ");

                var key = Console.ReadLine();
                Console.WriteLine();
                switch (key)
                {
                    case "1": _executor.Memory_CompressWorkingSets(); break;
                    case "2": _executor.System_CleanTemp(); break;
                    case "3": _executor.System_StopUselessServices(); break;
                    case "4": _executor.Network_ApplyBestDns(); break;
                    case "5": _executor.Network_OptimizeMtu(); break;
                    case "6": _executor.ApplyAllMemoryTweaks(); break;
                    case "7": _executor.System_Defrag("C"); break;
                    case "8": _executor.System_ListRunningServices(); break;
                    case "9": _executor.Diagnostics_FingerprintInteractive(); break;
                    case "10": _executor.Diagnostics_SampleCpuUsage(); break;
                    case "11": ShowTweaksMenu(); break;
                    case "12": _executor.Policies_ApplyQoSPolicy(); break;
                    case "13": _executor.Policies_ApplyPreset("Privacidade"); break;
                    case "14": _executor.Policies_ApplyPreset("UpdateRestrito"); break;
                    case "15": ApplyAllPresetsInteractive(); break;
                    case "16": _executor.Policies_Rollback(); break;
                    case "17": _executor.SetSpeedDuplex(NetworkDuplexManager.DuplexMode._1GbpsFull); break;
                    case "18": QoS_SubMenu(); break;
                    case "0": return;
                    default: Console.WriteLine("Opção inválida."); break;
                }
                Console.WriteLine("\nPressione qualquer tecla para continuar...");
                Console.ReadKey();
            }
        }

        private void ShowTweaksMenu()
        {
            var entries = GetTweaksIndex();
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Windows Tweaks ===");
                foreach (var kv in entries)
                {
                    Console.WriteLine($"{kv.Key} - {kv.Value}");
                }
                Console.WriteLine("0 - Voltar");
                Console.Write("\nEscolha: ");

                var key = Console.ReadLine();
                if (key == "0") return;
                if (int.TryParse(key, out var idx) && entries.TryGetValue(idx, out var baseName))
                {
                    ShowApplyUnapplyMenu(baseName);
                }
                else
                {
                    Console.WriteLine("Opção inválida.");
                    Console.ReadKey();
                }
            }
        }

        private void ShowApplyUnapplyMenu(string baseName)
        {
            bool hasApply = typeof(Executor).GetMethod($"Apply{baseName}") != null;
            bool hasUnapply = typeof(Executor).GetMethod($"Unapply{baseName}") != null;

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"== {baseName} ==");
                if (hasApply) Console.WriteLine("1 - Apply");
                if (hasUnapply) Console.WriteLine("2 - Unapply");
                Console.WriteLine("0 - Voltar");
                Console.Write("\nEscolha: ");
                var key = Console.ReadLine();
                if (key == "0") return;
                if (key == "1" && hasApply)
                {
                    var m = typeof(SupremeExecutor).GetMethod($"Tweaks_{"Apply" + baseName}");
                    m?.Invoke(_executor, null);
                    return;
                }
                if (key == "2" && hasUnapply)
                {
                    var m = typeof(SupremeExecutor).GetMethod($"Tweaks_{"Unapply" + baseName}");
                    m?.Invoke(_executor, null);
                    return;
                }
            }
        }

        private Dictionary<int, string> GetTweaksIndex()
        {
            var list = new List<string> {
                "AlignTaskbarLeft", "DebloatWindows", "DetailedBsod", "DisableBackgroundMsStoreApps",
                "DisableCopilot", "DisableCoreIsolation", "DisableDefenderRtp", "DisableDynamicTicking",
                "DisableFastStartup", "DisableGamebar", "DisableHibernation", "DisableLocationTracking",
                "DisableLockscreenTips", "DisableMouseAcceleration", "DisableWifiSense", "EnableDarkMode",
                "EnableEndTaskRightClick", "EnableHpet", "OptimizeNetworkSettings", "OptimizeNvidiaSettings",
                "RemoveGamingApps", "RemoveOnedrive", "RevertContextMenu", "RunDiskCleanup",
                "SetServicesToManual"
            };
            var dict = new Dictionary<int, string>();
            for (int i = 0; i < list.Count; i++)
                dict[i + 1] = list[i];
            return dict;
        }

        private void ApplyAllPresetsInteractive()
        {
            Console.WriteLine("⚠ Isso vai aplicar TODAS as políticas (Desempenho, Privacidade e UpdateRestrito).");
            Console.Write("Deseja continuar? (s/n): ");
            var resp = Console.ReadLine();
            if (!string.Equals(resp, "s", StringComparison.OrdinalIgnoreCase))
                return;

            foreach (var preset in PolicyPresets.Presets.Keys)
            {
                Console.WriteLine($"\n== Aplicando preset {preset} ==");
                _executor.Policies_ApplyPreset(preset);
            }
        }
    

        private void QoS_SubMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== QoS Manager ===");
                Console.WriteLine("1 - List Policies");
                Console.WriteLine("2 - Create Policy");
                Console.WriteLine("3 - Delete Policy");
                Console.WriteLine("0 - Voltar");
                Console.Write("\nEscolha: ");
                var key = Console.ReadLine();
                if (key == "0") return;
                switch (key)
                {
                    case "1":
                        _executor.QoS_ListPolicies();
                        break;
                    case "2":
                        var opt = new SystemInfo.Infrastructure.QoS.QoSOptions();
                        Console.Write("Policy Name: "); opt.Name = Console.ReadLine();
                        Console.Write("DSCP Value (0-63): "); int.TryParse(Console.ReadLine(), out var dscp); opt.DSCPValue = dscp;
                        Console.Write("Throttle (ex: 10mbps, vazio para nenhum): "); opt.ThrottleRate = SystemInfo.Infrastructure.QoS.QoSUtils.ParseRateToBitsPerSec(Console.ReadLine());
                        Console.Write("App Path (opcional): "); opt.AppPath = Console.ReadLine();
                        Console.Write("Protocol (TCP/UDP, opcional): "); opt.Protocol = Console.ReadLine();
                        Console.Write("Local Port (opcional): "); if (int.TryParse(Console.ReadLine(), out var lp)) opt.LocalPort = lp;
                        Console.Write("Remote Port (opcional): "); if (int.TryParse(Console.ReadLine(), out var rp)) opt.RemotePort = rp;
                        Console.Write("Local IP (opcional): "); opt.LocalIP = Console.ReadLine();
                        Console.Write("Remote IP (opcional): "); opt.RemoteIP = Console.ReadLine();
                        _executor.QoS_CreatePolicy(opt);
                        break;
                    case "3":
                        Console.Write("Nome da policy para remover: ");
                        var name = Console.ReadLine();
                        _executor.QoS_DeletePolicy(name);
                        break;
                    default:
                        Console.WriteLine("Opção inválida.");
                        break;
                }
                Console.WriteLine("\nPressione qualquer tecla para continuar...");
                Console.ReadKey();
            }
        }
    }
}