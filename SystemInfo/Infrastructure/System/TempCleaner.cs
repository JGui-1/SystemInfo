using System;
using System.IO;
using System.Linq;
using SystemInfo.Core.Abstractions;

namespace SystemInfo.Infrastructure.System
{
    public class TempCleaner : ITempCleaner
    {
        public double CleanAll()
        {
            long totalFreed = 0;

            // %TEMP% do usuário
            totalFreed += CleanFolder(Path.GetTempPath());

            // %SystemRoot%\Temp
            var systemTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
            totalFreed += CleanFolder(systemTemp);

            // Logs antigos
            var logs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Logs");
            totalFreed += CleanFolder(logs);

            // Cache do Windows Update
            var updateCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");
            totalFreed += CleanFolder(updateCache);

            return Math.Round(totalFreed / 1024d / 1024d, 2); // MB
        }

        private long CleanFolder(string path)
        {
            long freed = 0;
            if (!Directory.Exists(path)) return 0;

            try
            {
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var info = new FileInfo(file);
                        freed += info.Length;
                        info.IsReadOnly = false;
                        info.Delete();
                    }
                    catch { }
                }

                foreach (var dir in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
                {
                    try { Directory.Delete(dir, true); }
                    catch { }
                }
            }
            catch { }

            return freed;
        }
    }
}
