using System.Diagnostics;
using SystemInfo.Core.Abstractions;

namespace SystemInfo.Infrastructure.System
{
    public class DiskOptimizer : IDiskOptimizer
    {
        private readonly IWmiMethodInvoker _invoker;
        public DiskOptimizer(IWmiMethodInvoker invoker) => _invoker = invoker;

        public void Optimize(string driveLetter, int powerState)
        {
            // Executa CHKDSK
            Process.Start("chkdsk", $"{driveLetter}: /f /r");

            // Altera estado de energia do disco
            _invoker.Invoke("DiskDrive", "SetPowerState", null,
                new Dictionary<string, object> { { "PowerState", powerState }, { "Time", 0 } });
        }
    }
}
