using System.Collections.Generic;
using System.Diagnostics;
using SystemInfo.Core.Abstractions;

namespace SystemInfo.Infrastructure.System
{
    public class PriorityAdjuster : IPriorityAdjuster
    {
        public void SetHighPriority(IEnumerable<Process> processes)
        {
            foreach (var p in processes)
            {
                try { p.PriorityClass = ProcessPriorityClass.High; } catch { }
            }
        }

        public void SetLowPriority(IEnumerable<Process> processes)
        {
            foreach (var p in processes)
            {
                try { p.PriorityClass = ProcessPriorityClass.BelowNormal; } catch { }
            }
        }
    }
}
