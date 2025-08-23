using System;
using System.Diagnostics;
using System.Linq;
using SystemInfo.Core.Interop;

namespace SystemInfo.Core.Models
{
    public static class MemoryInfo
    {
        public static (ulong Total, ulong Available, ulong Used, int LoadPercent) Get()
        {
            var st = new NativeMethods.MEMORYSTATUSEX();
            if (!NativeMethods.GlobalMemoryStatusEx(st))
                throw new InvalidOperationException("GlobalMemoryStatusEx failed.");
            ulong used = st.ullTotalPhys - st.ullAvailPhys;
            return (st.ullTotalPhys, st.ullAvailPhys, used, (int)st.dwMemoryLoad);
        }

        public static void Show()
        {
            var (total, avail, used, load) = Get();
            Console.WriteLine($"Memória: Uso={Format(used)} / Total={Format(total)} | Livre={Format(avail)} | Carga={load}%");
        }

        public static void ShowTopProcesses(int top = 10)
        {
            var procs = Process.GetProcesses()
                .Select(p => {
                    try { return (p, p.WorkingSet64); } catch { return (p, 0L); }
                })
                .OrderByDescending(t => t.Item2)
                .Take(top);

            Console.WriteLine($"Top {top} processos por WorkingSet:");
            foreach (var (p, ws) in procs)
            {
                Console.WriteLine($"{p.ProcessName} (PID {p.Id}) -> {Format((ulong)ws)}");
            }
        }

        private static string Format(ulong bytes)
        {
            const double KB = 1024, MB = 1024*1024, GB = 1024*1024*1024;
            if (bytes >= GB) return $"{bytes/GB:0.00} GB";
            if (bytes >= MB) return $"{bytes/MB:0.00} MB";
            if (bytes >= KB) return $"{bytes/KB:0.00} KB";
            return $"{bytes} B";
        }
    }
}
