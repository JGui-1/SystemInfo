using System;
using System.Runtime; 
using SystemInfo.Core.Abstractions;

namespace SystemInfo.Infrastructure.Memory
{
    public class SelfOptimizer : IMemoryOptimizer
    {
        public string Name => "Self Optimizer";

        public void Optimize()
        {
            try
            {
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Console.WriteLine("[SelfOptimizer] GC coletado e LOH compactado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SelfOptimizer] Erro: {ex.Message}");
            }
        }
    }
}
