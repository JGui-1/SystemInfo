using System;
using System.Linq;
using System.Diagnostics;
using SystemInfo.Core.Abstractions;
using SystemInfo.Infrastructure.System;
using SystemInfo.Features.Fingerprint;
using SystemInfo.Configuration;

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

        public MainMenu(FingerprintGenerator fingerprint, IServiceManager services, IDiskOptimizer disk,
                        IProcessAnalyzer analyzer, IPriorityAdjuster adjuster, IOutput output)
        {
            _fingerprint = fingerprint; _services = services; _disk = disk;
            _analyzer = analyzer; _adjuster = adjuster; _output = output;
        }

        public void Run()
        {
            while (true)
            {
                _output.WriteLine("\n=== SystemInfo – Menu Principal ===");
                _output.WriteLine("1) Fingerprint interativo");
                _output.WriteLine("2) Serviços (listar / parar sugeridos / parar por nome)");
                _output.WriteLine("3) Processos (analisar consumo e ajustar prioridades)");
                _output.WriteLine("4) Disco (CHKDSK + PowerState)");
                _output.WriteLine("5) Sair");
                _output.Write("Escolha uma opção: ");
                var op = _output.ReadLine()?.Trim();

                if (op == "1") Fingerprint();
                else if (op == "2") Services();
                else if (op == "3") Processes();
                else if (op == "4") Disk();
                else if (op == "5" || string.IsNullOrEmpty(op)) break;
                else _output.WriteLine("Opção inválida.");
            }
        }

        private void Fingerprint()
        {
            _fingerprint.InteractiveShow();
        }

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
            _output.WriteLine("Amostrando uso de CPU por 2s...");            
            var usage = _analyzer.SampleCpuUsage(2000);
            var top = usage.OrderByDescending(u => u.CpuMs).Take(5).ToList();
            var bottom = usage.OrderBy(u => u.CpuMs).Take(5).ToList();

            _output.WriteLine("Top 5 processos (mais uso):");
            foreach (var u in top) _output.WriteLine($"{u.Proc.ProcessName} (PID {u.Proc.Id}): {u.CpuMs:F0} ms");

            _output.WriteLine("Bottom 5 processos (menos uso):");
            foreach (var u in bottom) _output.WriteLine($"{u.Proc.ProcessName} (PID {u.Proc.Id}): {u.CpuMs:F0} ms");

            _output.Write("Elevar prioridade dos mais usados? (s/n): ");
            if (_output.ReadLine()?.Trim().ToLower() == "s")
                _adjuster.SetHighPriority(top.Select(t => t.Proc));

            _output.Write("Reduzir prioridade dos menos usados? (s/n): ");
            if (_output.ReadLine()?.Trim().ToLower() == "s")
                _adjuster.SetLowPriority(bottom.Select(b => b.Proc));
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
    }
}
