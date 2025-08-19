using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using SystemInfo.Core.Abstractions;

namespace SystemInfo.Infrastructure.System
{
    public class ProcessAnalyzer : IProcessAnalyzer
    {
        public IReadOnlyList<(int Pid, string Name, double CpuPercent, double WorkingSetMB)> SampleCpuUsage(
            int sampleMilliseconds = 2000, int topN = 5, int bottomN = 5)
        {
            var cpuCount = Math.Max(1, Environment.ProcessorCount);

            // 1) Snapshot inicial (por PID) — não manter objetos Process vivos
            var all = Process.GetProcesses();
            var start = DateTime.UtcNow;
            var startTimes = new Dictionary<int, TimeSpan>(capacity: all.Length);
            foreach (var p in all)
            {
                try { startTimes[p.Id] = p.TotalProcessorTime; }
                catch { /* acesso negado ou já saiu */ }
                finally { p.Dispose(); }
            }

            // 2) Espera do intervalo
            Thread.Sleep(sampleMilliseconds);
            var end = DateTime.UtcNow;
            var elapsedMs = Math.Max(1.0, (end - start).TotalMilliseconds);

            // 3) Snapshot final e cálculo
            var results = new List<(int Pid, string Name, double CpuPercent, double WorkingSetMB)>();

            foreach (var pid in startTimes.Keys)
            {
                try
                {
                    using var p = Process.GetProcessById(pid);
                    // pode ter saído desde o snapshot inicial
                    var startCpu = startTimes[pid];
                    var endCpu = p.TotalProcessorTime;
                    var deltaMs = (endCpu - startCpu).TotalMilliseconds;

                    // %CPU = (tempo CPU no intervalo) / (tempo real * núcleos) * 100
                    var cpuPercent = Math.Max(0.0, (deltaMs / (elapsedMs * cpuCount)) * 100.0);

                    double wsMb = 0;
                    try { wsMb = Math.Round(p.WorkingSet64 / 1024.0 / 1024.0, 2); }
                    catch { /* alguns processos negam acesso */ }

                    string name;
                    try { name = string.IsNullOrWhiteSpace(p.ProcessName) ? "unknown" : p.ProcessName; }
                    catch { name = "unknown"; }

                    results.Add((pid, name, cpuPercent, wsMb));
                }
                catch
                {
                    // processo pode ter morrido; ignore
                }
            }

            // 4) Ordena por maior uso; topN/bottomN são usados no menu
            return results.OrderByDescending(r => r.CpuPercent).ToList();
        }
    }
}
