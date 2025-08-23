using Microsoft.Win32;
using System;

namespace SystemInfo.Infrastructure.QoS
{
    public static class RegBackend
    {
        private const string BaseKey = @"SOFTWARE\Policies\Microsoft\Windows\QoS";

        public static void List()
        {
            using var key = Registry.LocalMachine.OpenSubKey(BaseKey);
            if (key == null)
            {
                Console.WriteLine("[QoS][Registry] Nenhuma política encontrada.");
                return;
            }

            foreach (var sub in key.GetSubKeyNames())
            {
                Console.WriteLine($"[QoS][Registry] {sub}");
                using var p = key.OpenSubKey(sub);
                if (p != null)
                {
                    foreach (var v in p.GetValueNames())
                        Console.WriteLine($"   {v} = {p.GetValue(v)}");
                }
            }
        }

        public static void Create(QoSOptions opt, bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(opt?.Name))
                throw new ArgumentException("QoSOptions.Name é obrigatório.");

            using var baseKey = Registry.LocalMachine.CreateSubKey(BaseKey, true);
            if (baseKey == null) throw new Exception("Falha ao abrir/criar chave base QoS.");

            if (!overwrite && baseKey.OpenSubKey(opt.Name) != null)
                throw new Exception($"Política '{opt.Name}' já existe (use overwrite=true).");

            using var policyKey = baseKey.CreateSubKey(opt.Name, true);
            if (policyKey == null) throw new Exception("Falha ao criar subchave da política.");

            // Campos compatíveis com GPO QoS
            if (!string.IsNullOrWhiteSpace(opt.AppPath))
                policyKey.SetValue("Application Name", opt.AppPath, RegistryValueKind.String);

            if (opt.DSCPValue >= 0 && opt.DSCPValue <= 63)
                policyKey.SetValue("DSCP Value", opt.DSCPValue.ToString(), RegistryValueKind.String);

            if (opt.ThrottleRate.HasValue && opt.ThrottleRate.Value > 0)
                policyKey.SetValue("Throttle Rate", opt.ThrottleRate.Value.ToString(), RegistryValueKind.String);

            if (!string.IsNullOrWhiteSpace(opt.Protocol))
                policyKey.SetValue("Protocol", opt.Protocol, RegistryValueKind.String);
            else
                policyKey.SetValue("Protocol", "* , *", RegistryValueKind.String);

            if (opt.LocalPort.HasValue)
                policyKey.SetValue("Local Port", opt.LocalPort.Value.ToString(), RegistryValueKind.String);

            if (opt.RemotePort.HasValue)
                policyKey.SetValue("Remote Port", opt.RemotePort.Value.ToString(), RegistryValueKind.String);

            if (!string.IsNullOrWhiteSpace(opt.LocalIP))
                policyKey.SetValue("Local IP", opt.LocalIP, RegistryValueKind.String);

            if (!string.IsNullOrWhiteSpace(opt.RemoteIP))
                policyKey.SetValue("Remote IP", opt.RemoteIP, RegistryValueKind.String);

            Console.WriteLine($"[QoS][Registry] Criada/Atualizada política '{opt.Name}'. (HKLM\\{BaseKey}\\{opt.Name})");
        }

        public static void Delete(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("[QoS][Registry] Nome inválido."); return; }

            using var baseKey = Registry.LocalMachine.OpenSubKey(BaseKey, writable: true);
            if (baseKey == null) { Console.WriteLine("[QoS][Registry] Nenhuma política para deletar."); return; }

            try
            {
                baseKey.DeleteSubKeyTree(name);
                Console.WriteLine($"[QoS][Registry] Política '{name}' deletada.");
            }
            catch
            {
                Console.WriteLine($"[QoS][Registry] Política '{name}' não encontrada.");
            }
        }
    }
}
