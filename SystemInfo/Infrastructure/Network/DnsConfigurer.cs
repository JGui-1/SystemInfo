using System;
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace SystemInfo.Infrastructure.Network
{
    public class DnsConfigurer
    {
        private readonly DnsList dnsList;

        public DnsConfigurer(DnsList dnsList)
        {
            this.dnsList = dnsList;
        }

        public bool SetBestDns(string interfaceName, string bestDnsName, string bestAddress)
        {
            string primary = dnsList.GetDnsList()[bestDnsName].Primary;
            string secondary = dnsList.GetDnsList()[bestDnsName].Secondary;

            // Ajustar para usar o melhor endereço testado
            if (bestAddress == secondary && !string.IsNullOrEmpty(secondary))
            {
                primary = bestAddress;
                secondary = dnsList.GetDnsList()[bestDnsName].Primary; // Troca se o melhor for o secundário
            }
            else if (bestAddress == primary)
            {
                // Mantém a ordem original se o melhor for o primário
            }

            // Definir DNS primária
            Process p1 = new Process();
            p1.StartInfo = new ProcessStartInfo("cmd", $"/c netsh interface ipv4 set dns \"{interfaceName}\" static {primary}")
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            p1.Start();
            string output1 = p1.StandardOutput.ReadToEnd();
            p1.WaitForExit();

            // Definir DNS secundária (se disponível)
            if (!string.IsNullOrEmpty(secondary))
            {
                Process p2 = new Process();
                p2.StartInfo = new ProcessStartInfo("cmd", $"/c netsh interface ipv4 add dns \"{interfaceName}\" {secondary} index=2")
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Verb = "runas",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                p2.Start();
                string output2 = p2.StandardOutput.ReadToEnd();
                p2.WaitForExit();
            }

            // Flush DNS cache
            Process flush = new Process();
            flush.StartInfo = new ProcessStartInfo("cmd", "/c ipconfig /flushdns")
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            flush.Start();
            flush.WaitForExit();

            // Verificar se foi setada corretamente
            return VerifyDnsSet(interfaceName, primary, secondary);
        }

        public bool VerifyDnsSet(string interfaceName, string expectedPrimary, string expectedSecondary)
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo("cmd", $"/c netsh interface ipv4 show dnsservers \"{interfaceName}\"")
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            // Verificar se os endereços estão presentes na saída
            bool hasPrimary = output.Contains(expectedPrimary);
            bool hasSecondary = string.IsNullOrEmpty(expectedSecondary) || output.Contains(expectedSecondary);

            if (hasPrimary && hasSecondary)
            {
                Console.WriteLine($"DNS aplicada corretamente: Primary {expectedPrimary}, Secondary {expectedSecondary}");
                return true;
            }
            else
            {
                Console.WriteLine("Falha na verificação da DNS aplicada.");
                return false;
            }
        }

        public void RevertDns(string interfaceName)
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo("cmd", $"/c netsh interface ipv4 set dns \"{interfaceName}\" dhcp")
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            Console.WriteLine(output);
            Console.WriteLine("DNS revertida para automático.");

            // Flush DNS cache
            Process flush = new Process();
            flush.StartInfo = new ProcessStartInfo("cmd", "/c ipconfig /flushdns")
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            flush.Start();
            flush.WaitForExit();
        }
    }
}