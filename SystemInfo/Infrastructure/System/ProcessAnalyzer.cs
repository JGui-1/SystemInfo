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
        public IReadOnlyList<(Process Proc, double CpuMs)> SampleCpuUsage(int sampleMilliseconds = 2000, int topN = 5, int bottomN = 5)
        {
            var processes = Process.GetProcesses();
            var startTimes = processes.ToDictionary(p => p, p => p.TotalProcessorTime);
            Thread.Sleep(sampleMilliseconds);

            var usage = processes.Select(p =>
            {
                try
                {
                    var delta = p.TotalProcessorTime - startTimes[p];
                    return (Proc: p, CpuMs: delta.TotalMilliseconds);
                }
                catch { return (Proc: p, CpuMs: 0.0); }
            }).ToList();

            return usage.OrderByDescending(u => u.CpuMs).ToList();
        }
    }
}
