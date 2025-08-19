using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using SystemInfo.Core.Abstractions;
using SystemInfo.Features.Fingerprint;
using SystemInfo.Configuration;
using SystemInfo.Infrastructure.System;

namespace SystemInfo.Presentation.Menus
{
    public class MainMenu
    {
        private readonly FingerprintGenerator _fingerprint;
        private readonly IServiceManager _services;
        private readonly IDiskOptimizer _disk;
        private readonly IProcessAnalyzer _analyzer;
        private readonly IPriorityAdjuster _adjuster;
        private readonly IOutput _output;
        private readonly PolicyManager _policies;

        public MainMenu(
            FingerprintGenerator fingerprint, IServiceManager services,
            IDiskOptimizer disk, IProcessAnalyzer analyzer,
            IPriorityAdjuster adjuster, IOutput output)
        {
            _fingerprint = fingerprint;
            _services = services;
            _disk = disk;
            _analyzer = analyzer;
            _adjuster = adjuster;
            _output = output;
            _policies = new PolicyManager();
        }

        public void Run()
        {
            while (true)
            {
                _output.WriteLine("\n=== SystemInfo – Menu Principal ===");
                _output.WriteLine("1) Fingerprint interativo");
                _output.WriteLine("2) Serviços (listar / parar sugeridos / parar por nome)");
                _output.WriteLine("3) Processos (gerenciar prioridades)");
                _output.WriteLine("4) Disco (CHKDSK + PowerState)");
                _output.WriteLine("5) Sair");
                _output.WriteLine("6) Rede (tráfego e conexões em tempo real)");
                _output.WriteLine("7) Limpeza de arquivos temporários");
                _output.WriteLine("8) Otimizar / Desfragmentar disco");
                _output.WriteLine("9) Políticas de Sistema (Group Policy / Presets)");
                _output.Write("Escolha uma opção: ");
                var op = _output.ReadLine()?.Trim();

                if (op == "1") Fingerprint();
                else if (op == "2") Services();
                else if (op == "3") Processes();
                else if (op == "4") Disk();
                else if (op == "6") Network();
                else if (op == "7") TempClean();
                else if (op == "8") Defrag();
                else if (op == "9") Policies();
                else if (op == "5" || string.IsNullOrEmpty(op)) break;
                else _output.WriteLine("Opção inválida.");
            }
        }

        private void Fingerprint() => _fingerprint.InteractiveShow();

        private void Services()
        {
            _output.WriteLine("\n-- Serviços --");
            _output.WriteLine("1) Listar serviços em execução");
            _output.WriteLine("2) Parar serviços sugeridos (inúteis)");
            _output.WriteLine("3) Parar serviços por nome");
            _output.Write("Escolha: ");
            var op = _output.ReadLine()?.Trim();

            if (op == "1")
            {
                foreach (var svc in _services.ListRunning())
                    _output.WriteLine($"{svc["Name"]} - {svc["DisplayName"]} (State={svc["State"]}, StartMode={svc["StartMode"]})");
            }
            else if (op == "2")
            {
                _output.WriteLine("Sugeridos para parada (verifique antes!):");
                _output.WriteLine(string.Join(", ", UselessServices.Names));
                _output.Write("Deseja realmente tentar parar TODOS os sugeridos? (s/n): ");
                if (_output.ReadLine()?.Trim().ToLower() == "s")
                {
                    var results = _services.StopManyByName(UselessServices.Names);
                    foreach (var kv in results)
                        _output.WriteLine($"{kv.Key}: retorno={kv.Value.Values.FirstOrDefault()}");
                }
            }
            else if (op == "3")
            {
                _output.Write("Digite os nomes dos serviços (separados por vírgula): ");
                var line = _output.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var names = line.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s));
                    var results = _services.StopManyByName(names);
                    foreach (var kv in results)
                        _output.WriteLine($"{kv.Key}: retorno={kv.Value.Values.FirstOrDefault()}");
                }
            }
        }

        private void Processes()
        {
            while (true)
            {
                _output.WriteLine("\n-- Gerenciamento de Processos --");
                _output.WriteLine("1) Analisar e ajustar automaticamente (top/bottom por %CPU)");
                _output.WriteLine("2) Definir prioridade por NOME");
                _output.WriteLine("3) Definir prioridade por PID");
                _output.WriteLine("0) Voltar");
                _output.Write("Escolha: ");
                var op = _output.ReadLine()?.Trim();

                if (op == "0" || string.IsNullOrEmpty(op)) return;

                if (op == "1")
                {
                    _output.WriteLine("Amostrando uso de CPU por 2s...");
                    var data = _analyzer.SampleCpuUsage(2000);

                    var top = data.Take(5).ToList();
                    var bottom = data.OrderBy(r => r.CpuPercent).Take(5).ToList();

                    _output.WriteLine("\nTop 5 (mais uso):");
                    foreach (var r in top)
                        _output.WriteLine($"{r.Name} (PID {r.Pid}) - CPU {r.CpuPercent:F2}% | Mem {r.WorkingSetMB:F2} MB");

                    _output.WriteLine("\nBottom 5 (menos uso):");
                    foreach (var r in bottom)
                        _output.WriteLine($"{r.Name} (PID {r.Pid}) - CPU {r.CpuPercent:F2}% | Mem {r.WorkingSetMB:F2} MB");

                    _output.Write("Elevar prioridade dos Top 5 para HIGH? (s/n): ");
                    if (_output.ReadLine()?.Trim().ToLower() == "s")
                    {
                        var procs = top.Select(t =>
                        {
                            try { return Process.GetProcessById(t.Pid); } catch { return null; }
                        }).Where(p => p != null);
                        _adjuster.SetHighPriority(procs!);
                    }

                    _output.Write("Reduzir prioridade dos Bottom 5 para LOW? (s/n): ");
                    if (_output.ReadLine()?.Trim().ToLower() == "s")
                    {
                        var procs = bottom.Select(t =>
                        {
                            try { return Process.GetProcessById(t.Pid); } catch { return null; }
                        }).Where(p => p != null);
                        _adjuster.SetLowPriority(procs!);
                    }
                }
                else if (op == "2")
                {
                    _output.Write("Nome do processo (ex: chrome, notepad): ");
                    var name = _output.ReadLine()?.Trim();
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var priority = AskPriority();
                    if (priority == null) continue;

                    _adjuster.SetPriorityByName(name, priority.Value);
                    _output.WriteLine("✔ Prioridade aplicada (por nome).");
                }
                else if (op == "3")
                {
                    _output.Write("PID do processo: ");
                    if (!int.TryParse(_output.ReadLine(), out var pid)) continue;

                    var priority = AskPriority();
                    if (priority == null) continue;

                    _adjuster.SetPriority(pid, priority.Value);
                    _output.WriteLine("✔ Prioridade aplicada (por PID).");
                }
                else
                {
                    _output.WriteLine("Opção inválida.");
                }
            }
        }

        private ProcessPriorityClass? AskPriority()
        {
            _output.WriteLine("Escolha a prioridade:");
            _output.WriteLine("1. Baixa (Idle)");
            _output.WriteLine("2. Abaixo do Normal");
            _output.WriteLine("3. Normal");
            _output.WriteLine("4. Acima do Normal");
            _output.WriteLine("5. Alta");
            return _output.ReadLine() switch
            {
                "1" => ProcessPriorityClass.Idle,
                "2" => ProcessPriorityClass.BelowNormal,
                "3" => ProcessPriorityClass.Normal,
                "4" => ProcessPriorityClass.AboveNormal,
                "5" => ProcessPriorityClass.High,
                _ => null
            };
        }

        private void Disk()
        {
            _output.Write("Digite a letra do drive (ex: C): ");
            var drive = _output.ReadLine()?.Trim().TrimEnd(':');
            if (string.IsNullOrWhiteSpace(drive))
            {
                _output.WriteLine("Drive inválido.");
                return;
            }

            _output.Write("PowerState (ex: 3=Sleep, 4=Hibernate, 6=PowerCycle): ");
            var ps = _output.ReadLine();
            if (!int.TryParse(ps, out var powerState)) powerState = 3;

            _disk.Optimize(drive, powerState);
            _output.WriteLine("Comandos enviados (CHKDSK + SetPowerState).");
        }

        private void Network()
        {
            var net = new SystemInfo.Infrastructure.System.NetworkMonitor();

            _output.WriteLine("\n=== Monitor de Rede (aperte ENTER para sair) ===");

            while (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Enter)
            {
                Console.Clear();
                _output.WriteLine("=== Monitor de Rede ===\n");

                _output.WriteLine("Tráfego por adaptador:");
                foreach (var (Adapter, Sent, Recv) in net.GetTrafficSnapshot())
                    _output.WriteLine($"{Adapter} | Enviados: {Sent / 1024:F1} KB/s | Recebidos: {Recv / 1024:F1} KB/s");

                _output.WriteLine("\nConexões TCP ativas (primeiras 10):");
                foreach (var (Local, Remote, State) in net.GetActiveConnections().Take(10))
                    _output.WriteLine($"{Local} -> {Remote} [{State}]");

                _output.WriteLine("\nPressione ENTER para voltar...");
                Thread.Sleep(2000);
            }

            while (Console.KeyAvailable) Console.ReadKey(true);
        }

        private void TempClean()
        {
            var cleaner = new SystemInfo.Infrastructure.System.TempCleaner();
            var freed = cleaner.CleanAll();
            _output.WriteLine($"✔ Limpeza concluída. Espaço liberado: {freed} MB");
        }

        private void Defrag()
        {
            _output.Write("Digite a letra do drive (ex: C): ");
            var drive = _output.ReadLine()?.Trim().TrimEnd(':');
            if (string.IsNullOrWhiteSpace(drive))
            {
                _output.WriteLine("Drive inválido.");
                return;
            }

            var defragger = new SystemInfo.Infrastructure.System.DiskDefragmenter();
            var result = defragger.Optimize(drive);

            _output.WriteLine("\n=== Resultado do Defrag/Otimização ===");
            _output.WriteLine(result);
        }

        private void Policies()
        {
            while (true)
            {
                _output.WriteLine("\n-- Políticas de Sistema --");
                _output.WriteLine("1) Aplicar política individual");
                _output.WriteLine("2) Aplicar preset (gaming/performance)");
                _output.WriteLine("3) Rollback (restaurar originais)");
                _output.WriteLine("0) Voltar");
                _output.Write("Escolha: ");
                var op = _output.ReadLine()?.Trim();

                if (op == "0" || string.IsNullOrEmpty(op)) return;

                if (op == "1")
                {
                    _output.Write("Nome da política: ");
                    var name = _output.ReadLine()?.Trim();

                    _output.Write("Estado (enabled/disabled/notconfigured): ");
                    var st = _output.ReadLine()?.Trim().ToLower();

                    PolicyState state = st switch
                    {
                        "enabled" => PolicyState.Enabled,
                        "disabled" => PolicyState.Disabled,
                        _ => PolicyState.NotConfigured
                    };

                    int? val = null;
                    if (state == PolicyState.Enabled)
                    {
                        _output.Write("Valor (ou ENTER p/ padrão 1): ");
                        var txt = _output.ReadLine();
                        if (int.TryParse(txt, out var parsed)) val = parsed;
                    }

                    _policies.ApplyPolicy(name, state, val);
                }
                else if (op == "2")
                {
                    _output.WriteLine("Escolha o preset:");
                    _output.WriteLine("1) Gaming (latência baixa, otimização máxima)");
                    _output.WriteLine("2) Performance Balanceada");
                    _output.Write("Escolha: ");
                    var p = _output.ReadLine();

                    if (p == "1")
                    {
                        _policies.ApplyPolicy("NonBestEffortLimit", PolicyState.Enabled, 0);
                        _policies.ApplyPolicy("TimerResolution", PolicyState.Enabled, 10);
                        _policies.ApplyPolicy("AllowTelemetry", PolicyState.Disabled);
                        _policies.ApplyPolicy("AdvertisingId", PolicyState.Disabled);
                        _policies.ApplyPolicy("Cortana", PolicyState.Disabled);
                        _policies.ApplyPolicy("CloudContent.TipsAndSuggestions", PolicyState.Disabled);
                        _policies.ApplyPolicy("DeliveryOptimization.Mode", PolicyState.Enabled, 0);
                        _policies.ApplyPolicy("BITS.MaxBandwidthKBps", PolicyState.Enabled, 1);
                    }
                    else if (p == "2")
                    {
                        _policies.ApplyPolicy("NonBestEffortLimit", PolicyState.Enabled, 10);
                        _policies.ApplyPolicy("TimerResolution", PolicyState.Enabled, 15);
                        _policies.ApplyPolicy("AllowTelemetry", PolicyState.Enabled, 1);
                        _policies.ApplyPolicy("Cortana", PolicyState.Disabled);
                    }
                }
                else if (op == "3")
                {
                    _policies.Rollback();
                }
            }
        }
    }
}
