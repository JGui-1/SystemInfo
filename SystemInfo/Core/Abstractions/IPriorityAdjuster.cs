using System.Collections.Generic;
using System.Diagnostics;

namespace SystemInfo.Core.Abstractions
{
    public interface IPriorityAdjuster
    {
        void SetHighPriority(IEnumerable<Process> processes);
        void SetLowPriority(IEnumerable<Process> processes);
    }
}
