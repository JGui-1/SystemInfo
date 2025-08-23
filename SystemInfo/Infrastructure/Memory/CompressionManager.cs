using System;
using System.Diagnostics;

using SystemInfo.Core.Abstractions;

namespace SystemInfo.Infrastructure.Memory
{
    // Habilita Memory Compression via PowerShell (Windows 10+)
    public class CompressionManager : IMemoryOptimizer
    {
        public string Name => "Compression Manager";

        public void Optimize()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = @"-NoProfile -ExecutionPolicy Bypass -Command ""Enable-MMAgent -MemoryCompression""",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                p.WaitForExit(15000);
                Console.WriteLine("[CompressionManager] Memory Compression habilitado (requer Windows 10+).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CompressionManager] Falha ao habilitar Memory Compression: {ex.Message}");
            }
        }
    }
}
