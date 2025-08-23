using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SystemInfo.Core.Abstractions;
using SystemInfo.Core.Models;
using SystemInfo.Infrastructure.Memory;

namespace SystemInfo.Features.Memory
{
    public class MemoryOrchestrator
    {
        private readonly List<IMemoryOptimizer> _optimizers;
        public MemoryOrchestrator(IEnumerable<IMemoryOptimizer> optimizers) => _optimizers = optimizers.ToList();

        public void RunAll()
        {
            Console.WriteLine("=== Otimização de Memória ===");
            MemoryInfo.Show();
            foreach (var opt in _optimizers)
            {
                Console.WriteLine($"-> {opt.Name}");
                opt.Optimize();
                Thread.Sleep(200);
            }
            MemoryInfo.Show();
            Console.WriteLine("=== Concluído ===");
        }

        public static MemoryOrchestrator Default() => new MemoryOrchestrator(new IMemoryOptimizer[] {
            new SelfOptimizer(),
            new ProcessCleaner(),
            new CompressionManager(),
            new SysMainManager(),
            new TlbFlusher()
        });
    }
}
