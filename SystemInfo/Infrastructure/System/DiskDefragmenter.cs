using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using SystemInfo.Core.Abstractions;

namespace SystemInfo.Infrastructure.System
{
    public class DiskDefragmenter : IDiskDefragmenter
    {
        public string Optimize(string driveLetter)
        {
            if (string.IsNullOrWhiteSpace(driveLetter))
                throw new ArgumentException("Letra de unidade inválida.");

            driveLetter = driveLetter.Trim().TrimEnd(':');

            bool isSSD = IsSSD(driveLetter);

            string args = isSSD
                ? $"{driveLetter}: /L"   // /L = ReTrim (otimização SSD)
                : $"{driveLetter}: /U /V"; // /U = progresso, /V = detalhes (HDD)

            var psi = new ProcessStartInfo
            {
                FileName = "defrag.exe",
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc == null) return "Falha ao iniciar defrag.exe";

            string output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            return output;
        }

        private bool IsSSD(string driveLetter)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT MediaType FROM Win32_DiskDrive");

                foreach (var d in searcher.Get())
                {
                    var mediaType = d["MediaType"]?.ToString() ?? "";
                    if (mediaType.Contains("SSD", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            catch
            {
                // fallback simples → assume HDD
            }
            return false;
        }
    }
}
