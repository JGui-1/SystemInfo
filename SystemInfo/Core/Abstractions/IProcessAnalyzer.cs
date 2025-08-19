using System.Collections.Generic;

namespace SystemInfo.Core.Abstractions
{
    // Retorna snapshot por PID/Name evitando expor Process (previne leaks)
    public interface IProcessAnalyzer
    {
        // Mede % de CPU por processo no intervalo informado.
        // Retorna já ordenado por maior uso (desc).
        IReadOnlyList<(int Pid, string Name, double CpuPercent, double WorkingSetMB)> SampleCpuUsage(
            int sampleMilliseconds = 2000, int topN = 5, int bottomN = 5);
    }
}
