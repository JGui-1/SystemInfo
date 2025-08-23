using System;
using System.Diagnostics;
using SystemInfo.Core.Abstractions;
using SystemInfo.Core.Interop;

namespace SystemInfo.Infrastructure.Memory
{
    public class ProcessCleaner : IMemoryOptimizer
    {
        public string Name => "Process Cleaner";

        public void Optimize()
        {
            int ok = 0, fail = 0;
            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    NativeMethods.EmptyWorkingSet(proc.Handle);
                    ok++;
                }
                catch
                {
                    fail++;
                }
            }
            Console.WriteLine($"[ProcessCleaner] EmptyWorkingSet aplicado. Sucesso={ok}, Falhas={fail}.");
        }
    }
}
