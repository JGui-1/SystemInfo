using System.Collections.Generic;
using System.Diagnostics;

namespace SystemInfo.Core.Abstractions
{
    public interface IPriorityAdjuster
    {
        // Observa��o: implementa��es devem DISPOSE dos Process recebidos ap�s uso.
        void SetHighPriority(IEnumerable<Process> processes);
        void SetLowPriority(IEnumerable<Process> processes);

        // Define uma prioridade espec�fica (a implementa��o cria e descarta o Process).
        void SetPriority(int pid, ProcessPriorityClass priority);
        void SetPriorityByName(string processName, ProcessPriorityClass priority);
    }
}
