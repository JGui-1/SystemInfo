using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SystemInfo.Core.Abstractions;

namespace SystemInfo.Infrastructure.System
{
    public class PriorityAdjuster : IPriorityAdjuster
    {
        // Evita mexer em processos de sistema críticos
        private static readonly HashSet<string> Protected =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "System", "Idle", "csrss", "wininit", "lsass", "services", "smss", "svchost", "winlogon" };

        public void SetHighPriority(IEnumerable<Process> processes)
        {
            foreach (var p in processes)
            {
                try
                {
                    if (p == null || p.HasExited) continue;
                    if (Protected.Contains(p.ProcessName)) continue;
                    p.PriorityClass = ProcessPriorityClass.High;
                }
                catch { /* acesso negado/sem permissão */ }
                finally { p?.Dispose(); } // importante: evitar leak
            }
        }

        public void SetLowPriority(IEnumerable<Process> processes)
        {
            foreach (var p in processes)
            {
                try
                {
                    if (p == null || p.HasExited) continue;
                    if (Protected.Contains(p.ProcessName)) continue;
                    p.PriorityClass = ProcessPriorityClass.BelowNormal;
                }
                catch { }
                finally { p?.Dispose(); } // importante: evitar leak
            }
        }

        public void SetPriority(int pid, ProcessPriorityClass priority)
        {
            try
            {
                using var p = Process.GetProcessById(pid);
                if (p.HasExited) return;
                if (Protected.Contains(p.ProcessName)) return;
                p.PriorityClass = priority;
            }
            catch
            {
                // ignorar erros de acesso/perm
            }
        }

        public void SetPriorityByName(string processName, ProcessPriorityClass priority)
        {
            if (string.IsNullOrWhiteSpace(processName)) return;

            Process[] list = Array.Empty<Process>();
            try { list = Process.GetProcessesByName(processName); }
            catch { }

            foreach (var p in list)
            {
                try
                {
                    if (p.HasExited) continue;
                    if (Protected.Contains(p.ProcessName)) continue;
                    p.PriorityClass = priority;
                }
                catch { }
                finally { p?.Dispose(); }
            }
        }
    }
}
