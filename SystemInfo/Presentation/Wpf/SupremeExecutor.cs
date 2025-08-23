using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using SystemInfo.Configuration;
using SystemInfo.Core.Abstractions;
using SystemInfo.Features.Fingerprint;
using SystemInfo.Features.Memory;
using SystemInfo.Infrastructure.Memory;
using SystemInfo.Infrastructure.Network;
using SystemInfo.Infrastructure.Output;
using SystemInfo.Infrastructure.System;
using SystemInfo.Infrastructure.Wmi;
using SystemInfo.Infrastructure.Tweaks;

namespace SystemInfo.Presentation.Wpf
{
    /// <summary>
    /// Executor “supremo” pensado para integração com WPF.
    /// - Só expõe métodos VOID para você ligar direto em botões/comandos.
    /// - Aceita um IOutput (ou Action&lt;string&gt; via WpfActionOutput) para logar no UI.
    /// - Usa as implementações existentes do projeto (nada “mágico” escondido).
    /// </summary>
    public class SupremeExecutor
    {
        // Infra compartilhada
        private readonly IOutput _output;
        private readonly IWmiQueryService _wmiQuery;
        private readonly IWmiMethodInvoker _wmiInvoke;
        private readonly IServiceManager _services;
        private readonly INetworkManager _netMgr;
        private readonly IMtuOptimizer _mtu;
        private readonly DnsConfigurer _dnsCfg;
        private readonly ITempCleaner _tempCleaner;
        private readonly IDiskDefragmenter _defrag;
        private readonly IProcessAnalyzer _procAnalyzer;
        private readonly FingerprintGenerator _fingerprint;

        // Policies (QoS/Desempenho/Privacidade/Update)
        private readonly PolicyManager _policyMgr = new PolicyManager();

        public SupremeExecutor(IOutput? output = null)
        {
            _output = output ?? new ConsoleOutput();

            // WMI
            _wmiQuery = new WmiQueryService();
            _wmiInvoke = new WmiMethodInvoker();

            // Sistema / Rede
            _services = new ServiceManager(_wmiQuery, _wmiInvoke);
            _netMgr = new NetworkManager();
            _mtu = new MtuOptimizer();
            _dnsCfg = new DnsConfigurer(new DnsList());
            _tempCleaner = new TempCleaner();
            _defrag = new DiskDefragmenter();
            _procAnalyzer = new ProcessAnalyzer();

            // Fingerprint (usa WMI + saída)
            _fingerprint = new FingerprintGenerator(_wmiQuery, _output);
        }

        public SupremeExecutor(Action<string> logSink) : this(new WpfActionOutput(logSink)) { }

        // ------------- MEMÓRIA -------------

        public void ApplyAllMemoryTweaks()
        {
            try
            {
                _output.WriteLine("[Memory] Iniciando orquestração padrão...");
                var orch = MemoryOrchestrator.Default();
                orch.RunAll();
                _output.WriteLine("[Memory] Concluído.");
            }
            catch (Exception ex)
            {
                _output.WriteLine("[Memory][ERRO] " + ex.Message);
            }
        }

        public void Memory_SelfOptimize() => Try(() => new SelfOptimizer().Optimize(), "[Memory] SelfOptimizer");
        public void Memory_CompressWorkingSets() => Try(() => new CompressionManager().Optimize(), "[Memory] CompressionManager");
        public void Memory_ClearProcessWorkingSets() => Try(() => new ProcessCleaner().Optimize(), "[Memory] ProcessCleaner (EmptyWorkingSet)");
        public void Memory_ManageSysMain() => Try(() => new SysMainManager().Optimize(), "[Memory] SysMain Manager");
        public void Memory_FlushTlb() => Try(() => new TlbFlusher().Optimize(), "[Memory] TLB Flusher");

        // ------------- SISTEMA -------------

        public void System_CleanTemp()
        {
            Try(() =>
            {
                var freed = _tempCleaner.CleanAll();
                _output.WriteLine($"[TempCleaner] Liberado ~{freed:F1} MB.");
            }, "[TempCleaner]");
        }

        public void System_Defrag(string driveLetter = "C")
        {
            Try(() =>
            {
                var text = _defrag.Optimize(driveLetter);
                if (!string.IsNullOrWhiteSpace(text)) _output.WriteLine(text);
            }, $"[Defrag] Unidade {driveLetter}");
        }

        public void System_StopUselessServices()
        {
            Try(() =>
            {
                var result = _services.StopManyByName(UselessServices.Names);
                foreach (var kv in result)
                    _output.WriteLine($"[Service] {kv.Key} → Return={kv.Value}");
            }, "[Service] StopMany(UselessServices)");
        }

        public void System_ListRunningServices()
        {
            Try(() =>
            {
                foreach (var dict in _services.ListRunning())
                {
                    var name = dict.TryGetValue("Name", out var n) ? n?.ToString() : "?";
                    var state = dict.TryGetValue("State", out var s) ? s?.ToString() : "?";
                    _output.WriteLine($"[Service] {name} → {state}");
                }
            }, "[Service] ListRunning()");
        }

        // ------------- REDE -------------

        public void Network_OptimizeMtu()
        {
            Try(() =>
            {
                var iface = _netMgr.GetMainNetworkInterface();
                if (iface == null) { _output.WriteLine("[MTU] Nenhuma interface IPv4 UP detectada."); return; }

                var bestMtu = _mtu.CalculateOptimalMtu(iface);
                var idx = iface.GetIPProperties().GetIPv4Properties().Index.ToString();
                _mtu.SetMTU(idx, bestMtu.ToString());
                _output.WriteLine($"[MTU] Interface={iface.Name} → MTU aplicado: {bestMtu}");
            }, "[MTU] Optimize");
        }

        public void Network_ApplyBestDns()
        {
            Try(() =>
            {
                var iface = _netMgr.GetMainNetworkInterface();
                if (iface == null) { _output.WriteLine("[DNS] Nenhuma interface principal encontrada."); return; }

                var best = PickBestDnsByPing();
                if (best == null) { _output.WriteLine("[DNS] Não foi possível determinar o melhor DNS."); return; }

                if (_dnsCfg.SetBestDns(iface.Name, best.Value.Name, best.Value.Primary))
                    _output.WriteLine($"[DNS] Aplicado: {best.Value.Name} → {best.Value.Primary}/{best.Value.Secondary} em {iface.Name}");
                else
                    _output.WriteLine("[DNS] Falha ao aplicar DNS.");
            }, "[DNS] ApplyBest");
        }

        public void Network_RevertDns()
        {
            Try(() =>
            {
                var iface = _netMgr.GetMainNetworkInterface();
                if (iface == null) { _output.WriteLine("[DNS] Nenhuma interface principal encontrada."); return; }
                _dnsCfg.RevertDns(iface.Name);
                _output.WriteLine($"[DNS] Revertido para automático em {iface.Name}.");
            }, "[DNS] Revert");
        }

        /// <summary>🚀 Novo Executor: SpeedDuplex</summary>
        public void SetSpeedDuplex(NetworkDuplexManager.DuplexMode mode = NetworkDuplexManager.DuplexMode._1GbpsFull)
        {
            try
            {
                NetworkDuplexManager.SetSpeedDuplex(mode);
                _output.WriteLine($"✔ SpeedDuplex configurado para {mode}.");
            }
            catch (Exception ex)
            {
                _output.WriteLine("❌ Falha ao configurar SpeedDuplex: " + ex.Message);
            }
        }

        // ------------- MONITORAMENTO / DIAGNÓSTICOS -------------

        public void Diagnostics_FingerprintInteractive() => Try(() => _fingerprint.InteractiveShow(), "[Fingerprint] InteractiveShow");

        public void Diagnostics_SampleCpuUsage(int sampleMs = 2000, int topN = 5, int bottomN = 5)
        {
            Try(() =>
            {
                var rows = _procAnalyzer.SampleCpuUsage(sampleMs, topN, bottomN);
                foreach (var r in rows)
                    _output.WriteLine($"[CPU] PID={r.Pid} Name={r.Name} CPU={r.CpuPercent:F1}% WS={r.WorkingSetMB:F1}MB");
            }, "[Diagnostics] SampleCpuUsage");
        }

        // ------------- POLÍTICAS (QoS/Presets) -------------

        public void Policies_ApplyPreset(string presetName, bool gpupdate = true)
        {
            Try(() =>
            {
                if (!PolicyPresets.Presets.TryGetValue(presetName, out var set) || set.Count == 0)
                {
                    _output.WriteLine($"[Policy] Preset '{presetName}' não encontrado.");
                    return;
                }

                foreach (var kv in set)
                {
                    var key = kv.Key;
                    var (state, value) = kv.Value;
                    _policyMgr.ApplyPolicy(key, state, value);
                }

                if (gpupdate)
                {
                    _output.WriteLine("[Policy] Executando gpupdate /force...");
                    RunGpupdate();
                }
            }, $"[Policy] ApplyPreset '{presetName}'");
        }

        public void Policies_ApplyQoSPolicy() => Policies_ApplyPreset("Desempenho");

        public void Policies_ApplyAllPresetsInteractive(bool gpupdate = true)
        {
            Try(() =>
            {
                bool anyApplied = false;
                foreach (var preset in PolicyPresets.Presets.Keys)
                {
                    var yn = AskYesNo($"> Aplicar preset '{preset}'? [s/N]: ", defaultNo: true);
                    if (!yn) continue;

                    Policies_ApplyPreset(preset, gpupdate: false);
                    anyApplied = true;
                }

                if (anyApplied && gpupdate)
                {
                    _output.WriteLine("[Policy] Executando gpupdate /force (consolidado)...");
                    RunGpupdate();
                }
                else if (!anyApplied)
                {
                    _output.WriteLine("[Policy] Nenhum preset aplicado.");
                }
            }, "[Policy] ApplyAllPresetsInteractive");
        }

        public void Policies_ListPresets()
        {
            Try(() =>
            {
                _output.WriteLine("[Policy] Presets disponíveis:");
                foreach (var preset in PolicyPresets.Presets)
                {
                    _output.WriteLine($"- {preset.Key} ({preset.Value.Count} políticas)");
                }
            }, "[Policy] ListPresets");
        }

        public void Policies_Rollback()
        {
            Try(() =>
            {
                _policyMgr.Rollback();
                _output.WriteLine("[Policy] Rollback concluído.");
            }, "[Policy] Rollback");
        }

        // ------------- WINDOWS TWEAKS -------------

        public void Tweaks_ApplyAlignTaskbarLeft() { Try(() => Executor.ApplyAlignTaskbarLeft(), "[Tweak] AlignTaskbarLeft (Apply)"); }
        public void Tweaks_UnapplyAlignTaskbarLeft() { Try(() => Executor.UnapplyAlignTaskbarLeft(), "[Tweak] AlignTaskbarLeft (Unapply)"); }
        public void Tweaks_ApplyDebloatWindows() { Try(() => Executor.ApplyDebloatWindows(), "[Tweak] DebloatWindows (Apply)"); }
        public void Tweaks_ApplyDetailedBsod() { Try(() => Executor.ApplyDetailedBsod(), "[Tweak] DetailedBsod (Apply)"); }
        public void Tweaks_UnapplyDetailedBsod() { Try(() => Executor.UnapplyDetailedBsod(), "[Tweak] DetailedBsod (Unapply)"); }
        public void Tweaks_ApplyDisableBackgroundMsStoreApps() { Try(() => Executor.ApplyDisableBackgroundMsStoreApps(), "[Tweak] DisableBackgroundMsStoreApps (Apply)"); }
        public void Tweaks_UnapplyDisableBackgroundMsStoreApps() { Try(() => Executor.UnapplyDisableBackgroundMsStoreApps(), "[Tweak] DisableBackgroundMsStoreApps (Unapply)"); }
        public void Tweaks_ApplyDisableCopilot() { Try(() => Executor.ApplyDisableCopilot(), "[Tweak] DisableCopilot (Apply)"); }
        public void Tweaks_UnapplyDisableCopilot() { Try(() => Executor.UnapplyDisableCopilot(), "[Tweak] DisableCopilot (Unapply)"); }
        public void Tweaks_ApplyDisableCoreIsolation() { Try(() => Executor.ApplyDisableCoreIsolation(), "[Tweak] DisableCoreIsolation (Apply)"); }
        public void Tweaks_UnapplyDisableCoreIsolation() { Try(() => Executor.UnapplyDisableCoreIsolation(), "[Tweak] DisableCoreIsolation (Unapply)"); }
        public void Tweaks_ApplyDisableDefenderRtp() { Try(() => Executor.ApplyDisableDefenderRtp(), "[Tweak] DisableDefenderRtp (Apply)"); }
        public void Tweaks_UnapplyDisableDefenderRtp() { Try(() => Executor.UnapplyDisableDefenderRtp(), "[Tweak] DisableDefenderRtp (Unapply)"); }
        public void Tweaks_ApplyDisableDynamicTicking() { Try(() => Executor.ApplyDisableDynamicTicking(), "[Tweak] DisableDynamicTicking (Apply)"); }
        public void Tweaks_UnapplyDisableDynamicTicking() { Try(() => Executor.UnapplyDisableDynamicTicking(), "[Tweak] DisableDynamicTicking (Unapply)"); }
        public void Tweaks_ApplyDisableFastStartup() { Try(() => Executor.ApplyDisableFastStartup(), "[Tweak] DisableFastStartup (Apply)"); }
        public void Tweaks_UnapplyDisableFastStartup() { Try(() => Executor.UnapplyDisableFastStartup(), "[Tweak] DisableFastStartup (Unapply)"); }
        public void Tweaks_ApplyDisableGamebar() { Try(() => Executor.ApplyDisableGamebar(), "[Tweak] DisableGamebar (Apply)"); }
        public void Tweaks_UnapplyDisableGamebar() { Try(() => Executor.UnapplyDisableGamebar(), "[Tweak] DisableGamebar (Unapply)"); }
        public void Tweaks_ApplyDisableHibernation() { Try(() => Executor.ApplyDisableHibernation(), "[Tweak] DisableHibernation (Apply)"); }
        public void Tweaks_UnapplyDisableHibernation() { Try(() => Executor.UnapplyDisableHibernation(), "[Tweak] DisableHibernation (Unapply)"); }
        public void Tweaks_ApplyDisableLocationTracking() { Try(() => Executor.ApplyDisableLocationTracking(), "[Tweak] DisableLocationTracking (Apply)"); }
        public void Tweaks_UnapplyDisableLocationTracking() { Try(() => Executor.UnapplyDisableLocationTracking(), "[Tweak] DisableLocationTracking (Unapply)"); }
        public void Tweaks_ApplyDisableLockscreenTips() { Try(() => Executor.ApplyDisableLockscreenTips(), "[Tweak] DisableLockscreenTips (Apply)"); }
        public void Tweaks_UnapplyDisableLockscreenTips() { Try(() => Executor.UnapplyDisableLockscreenTips(), "[Tweak] DisableLockscreenTips (Unapply)"); }
        public void Tweaks_ApplyDisableMouseAcceleration() { Try(() => Executor.ApplyDisableMouseAcceleration(), "[Tweak] DisableMouseAcceleration (Apply)"); }
        public void Tweaks_UnapplyDisableMouseAcceleration() { Try(() => Executor.UnapplyDisableMouseAcceleration(), "[Tweak] DisableMouseAcceleration (Unapply)"); }
        public void Tweaks_ApplyDisableWifiSense() { Try(() => Executor.ApplyDisableWifiSense(), "[Tweak] DisableWifiSense (Apply)"); }
        public void Tweaks_UnapplyDisableWifiSense() { Try(() => Executor.UnapplyDisableWifiSense(), "[Tweak] DisableWifiSense (Unapply)"); }
        public void Tweaks_ApplyEnableDarkMode() { Try(() => Executor.ApplyEnableDarkMode(), "[Tweak] EnableDarkMode (Apply)"); }
        public void Tweaks_UnapplyEnableDarkMode() { Try(() => Executor.UnapplyEnableDarkMode(), "[Tweak] EnableDarkMode (Unapply)"); }
        public void Tweaks_ApplyEnableEndTaskRightClick() { Try(() => Executor.ApplyEnableEndTaskRightClick(), "[Tweak] EnableEndTaskRightClick (Apply)"); }
        public void Tweaks_UnapplyEnableEndTaskRightClick() { Try(() => Executor.UnapplyEnableEndTaskRightClick(), "[Tweak] EnableEndTaskRightClick (Unapply)"); }
        public void Tweaks_ApplyEnableHpet() { Try(() => Executor.ApplyEnableHpet(), "[Tweak] EnableHpet (Apply)"); }
        public void Tweaks_UnapplyEnableHpet() { Try(() => Executor.UnapplyEnableHpet(), "[Tweak] EnableHpet (Unapply)"); }
        public void Tweaks_ApplyOptimizeNetworkSettings() { Try(() => Executor.ApplyOptimizeNetworkSettings(), "[Tweak] OptimizeNetworkSettings (Apply)"); }
        public void Tweaks_UnapplyOptimizeNetworkSettings() { Try(() => Executor.UnapplyOptimizeNetworkSettings(), "[Tweak] OptimizeNetworkSettings (Unapply)"); }
        public void Tweaks_ApplyOptimizeNvidiaSettings() { Try(() => Executor.ApplyOptimizeNvidiaSettings(), "[Tweak] OptimizeNvidiaSettings (Apply)"); }
        public void Tweaks_ApplyRemoveGamingApps() { Try(() => Executor.ApplyRemoveGamingApps(), "[Tweak] RemoveGamingApps (Apply)"); }
        public void Tweaks_ApplyRemoveOnedrive() { Try(() => Executor.ApplyRemoveOnedrive(), "[Tweak] RemoveOnedrive (Apply)"); }
        public void Tweaks_UnapplyRemoveOnedrive() { Try(() => Executor.UnapplyRemoveOnedrive(), "[Tweak] RemoveOnedrive (Unapply)"); }
        public void Tweaks_ApplyRevertContextMenu() { Try(() => Executor.ApplyRevertContextMenu(), "[Tweak] RevertContextMenu (Apply)"); }
        public void Tweaks_UnapplyRevertContextMenu() { Try(() => Executor.UnapplyRevertContextMenu(), "[Tweak] RevertContextMenu (Unapply)"); }
        public void Tweaks_ApplyRunDiskCleanup() { Try(() => Executor.ApplyRunDiskCleanup(), "[Tweak] RunDiskCleanup (Apply)"); }
        public void Tweaks_ApplySetServicesToManual() { Try(() => Executor.ApplySetServicesToManual(), "[Tweak] SetServicesToManual (Apply)"); }
        public void Tweaks_UnapplySetServicesToManual() { Try(() => Executor.UnapplySetServicesToManual(), "[Tweak] SetServicesToManual (Unapply)"); }

        
        // ------------- QOS MANAGER -------------

        public void QoS_ListPolicies(string backend = "registry", string scope = "machine")
        {
            Try(() => SystemInfo.Infrastructure.QoS.Manager.List(backend, scope), "[QoS] ListPolicies");
        }

        public void QoS_CreatePolicy(SystemInfo.Infrastructure.QoS.QoSOptions opt, string backend = "registry", bool overwrite = false)
        {
            Try(() => SystemInfo.Infrastructure.QoS.Manager.Create(backend, opt, overwrite), "[QoS] CreatePolicy");
        }

        public void QoS_DeletePolicy(string name, string backend = "registry", string scope = "machine")
        {
            Try(() => SystemInfo.Infrastructure.QoS.Manager.Delete(backend, name, scope), "[QoS] DeletePolicy");
        }
    
// ------------- HELPERS -------------

        private (string Name, string Primary, string Secondary)? PickBestDnsByPing(int perHostPings = 3, int timeoutMs = 500)
        {
            var list = new DnsList().GetDnsList();
            (string Name, string Primary, string Secondary)? best = null;
            double bestAvg = double.MaxValue;

            using var ping = new Ping();
            foreach (var kv in list)
            {
                var name = kv.Key;
                var prim = kv.Value.Primary;
                var sec = kv.Value.Secondary;

                double SumPing(string ip)
                {
                    double sum = 0;
                    for (int i = 0; i < perHostPings; i++)
                    {
                        try
                        {
                            var reply = ping.Send(ip, timeoutMs);
                            if (reply.Status == IPStatus.Success) sum += reply.RoundtripTime;
                            else sum += timeoutMs;
                        }
                        catch { sum += timeoutMs; }
                    }
                    return sum / perHostPings;
                }

                var avgPrim = SumPing(prim);
                var avgSec = SumPing(sec);
                var avg = (avgPrim + avgSec) / 2.0;

                _output.WriteLine($"[DNS][Ping] {name}: {avg:F1} ms");
                if (avg < bestAvg)
                {
                    bestAvg = avg;
                    best = (name, prim, sec);
                }
            }

            return best;
        }

        private void Try(Action act, string label)
        {
            try { act(); }
            catch (Exception ex) { _output.WriteLine($"{label}[ERRO] {ex.Message}"); }
        }

        private void RunGpupdate()
        {
            try
            {
                var psi = new ProcessStartInfo("gpupdate.exe", "/force")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(15000);
            }
            catch (Exception ex)
            {
                _output.WriteLine("[Policy][ERRO] gpupdate → " + ex.Message);
            }
        }

        private bool AskYesNo(string prompt, bool defaultNo = true)
        {
            _output.Write(prompt);
            var resp = _output.ReadLine()?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(resp)) return !defaultNo;
            return resp == "s" || resp == "y" || resp == "yes";
        }
    }

    /// <summary>
    /// Adaptador simples para ligar Action&lt;string&gt; (ex.: atualizar TextBox no WPF) a IOutput.
    /// </summary>
    public sealed class WpfActionOutput : IOutput
    {
        private readonly Action<string> _sink;
        public WpfActionOutput(Action<string> sink) => _sink = sink ?? (_ => { });
        public void Write(string text) => _sink(text);
        public void WriteLine(string text = "") => _sink(text + Environment.NewLine);
        public string ReadLine() => string.Empty;
        public ConsoleKeyInfo ReadKey() => default;
    }
}
