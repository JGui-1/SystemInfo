using System.Collections.Generic;
using System.Diagnostics;

namespace SystemInfo.Core.Abstractions
{
    public interface IPriorityAdjuster
    {
        // Observação: implementações devem DISPOSE dos Process recebidos após uso.
        void SetHighPriority(IEnumerable<Process> processes);
        void SetLowPriority(IEnumerable<Process> processes);

        // Define uma prioridade específica (a implementação cria e descarta o Process).
        void SetPriority(int pid, ProcessPriorityClass priority);
        void SetPriorityByName(string processName, ProcessPriorityClass priority);
    }
}
