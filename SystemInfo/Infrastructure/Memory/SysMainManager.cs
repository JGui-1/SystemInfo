using System;
using System.ServiceProcess;
using SystemInfo.Core.Abstractions;

namespace SystemInfo.Infrastructure.Memory
{
    public class SysMainManager : IMemoryOptimizer
    {
        public string Name => "SysMain Manager";

        public void Optimize()
        {
            try
            {
                using var sc = new ServiceController("SysMain");

                if (sc.Status == ServiceControllerStatus.Running)
                {
                    Console.WriteLine("[SysMainManager] Parando SysMain...");
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(15));
                }

                Console.WriteLine("[SysMainManager] SysMain parado (pode reduzir pressão de memória em alguns cenários).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SysMainManager] Falha ao gerenciar SysMain: {ex.Message}");
            }
        }
    }
}
