using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SystemInfo.Infrastructure.System
{
    public class StealthRegistryApplier
    {
        private readonly Dictionary<string, object?> _backup = new();

        // 🔑 Decodifica Base64 → string
        private string Decode(string enc) =>
            Encoding.UTF8.GetString(Convert.FromBase64String(enc));

        // 🔐 Salva valor atual antes de mexer
        private void BackupValue(RegistryHive hive, string path, string name)
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = baseKey.OpenSubKey(path);
            if (key != null)
            {
                var val = key.GetValue(name, null);
                _backup[$"{hive}:{path}:{name}"] = val;
            }
            else
            {
                _backup[$"{hive}:{path}:{name}"] = null;
            }
        }

        // 📦 Restaura valores originais
        public void Rollback()
        {
            foreach (var kv in _backup)
            {
                var parts = kv.Key.Split(':');
                var hive = (RegistryHive)Enum.Parse(typeof(RegistryHive), parts[0]);
                var path = parts[1];
                var name = parts[2];

                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                using var key = baseKey.CreateSubKey(path, true);

                if (kv.Value == null)
                    key.DeleteValue(name, false);
                else
                    key.SetValue(name, kv.Value);
            }
        }

        // 🚀 Aplica valor de forma stealth
        public void ApplyStealth(string hiveEnc, string pathEnc, string nameEnc, object value)
        {
            var hiveStr = Decode(hiveEnc);
            var path = Decode(pathEnc);
            var name = Decode(nameEnc);

            var hive = hiveStr == "HKLM" ? RegistryHive.LocalMachine : RegistryHive.CurrentUser;

            // backup antes
            BackupValue(hive, path, name);

            // gera arquivo .reg temporário
            string tempFile = Path.GetTempFileName() + ".reg";
            using (var sw = new StreamWriter(tempFile, false, Encoding.Unicode))
            {
                sw.WriteLine("Windows Registry Editor Version 5.00");
                sw.WriteLine();
                sw.WriteLine($"[{(hive == RegistryHive.LocalMachine ? "HKEY_LOCAL_MACHINE" : "HKEY_CURRENT_USER")}\\{path}]");
                sw.WriteLine($"\"{name}\"={RegValue(value)}");
            }

            // importa via reg.exe
            var psi = new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = $"import \"{tempFile}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            var proc = Process.Start(psi);
            proc?.WaitForExit();

            // apaga arquivo
            try { File.Delete(tempFile); } catch { }
        }

        // 🔧 Converte valor em formato de .reg
        private string RegValue(object value)
        {
            return value switch
            {
                int i => "dword:" + i.ToString("x8"),
                string s => $"\"{s}\"",
                _ => throw new NotSupportedException("Tipo de valor não suportado")
            };
        }
    }
}
