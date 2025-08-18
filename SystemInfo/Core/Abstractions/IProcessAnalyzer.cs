using System.Collections.Generic;
using System.Diagnostics;

namespace SystemInfo.Core.Abstractions
{
    public interface IProcessAnalyzer
    {
        IReadOnlyList<(Process Proc, double CpuMs)> SampleCpuUsage(int sampleMilliseconds = 2000, int topN = 5, int bottomN = 5);
    }
}
