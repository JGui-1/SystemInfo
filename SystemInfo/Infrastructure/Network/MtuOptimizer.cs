using System;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace SystemInfo.Infrastructure.Network
{
    public class MtuOptimizer : SystemInfo.Core.Abstractions.IMtuOptimizer
    {
        public int CalculateOptimalMtu(NetworkInterface iface)
        {
            var ipProps = iface.GetIPProperties().GetIPv4Properties();
            string index = ipProps.Index.ToString();
            string tempMtu = GetCurrentMtu(index);

            SetMTU(index, "9000");

            int mtu = 0;
            try
            {
                for (int packetSize = 1500; packetSize >= 0; packetSize--)
                {
                    if (!PingTooBig("t-online.de", packetSize, 1))
                    {
                        mtu = 28 + packetSize;
                        break;
                    }
                }
            }
            catch
            {
                try
                {
                    for (int packetSize = 1500; packetSize >= 0; packetSize--)
                    {
                        if (!PingTooBig("google.de", packetSize, 4))
                        {
                            mtu = 28 + packetSize;
                            break;
                        }
                    }
                }
                catch (Exception ee)
                {
                    Console.WriteLine($"Erro ao testar MTU: {ee.Message}");
                }
            }

            SetMTU(index, tempMtu);
            return mtu;
        }

        public void SetMTU(string index, string mtu)
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo("cmd", "/c netsh interface ipv4 set subinterface " + index + " mtu=" + mtu + " store=persistent")
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            p.Start();
            p.WaitForExit();
        }

        private string GetCurrentMtu(string index)
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo("cmd", "/c netsh interface ipv4 show subinterface " + index)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            p.Start();
            var res = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            var lines = res.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("------"))
                {
                    return lines[i + 1].Trim().Substring(0, lines[i + 1].Trim().IndexOf(' '));
                }
            }
            return "1500"; // Default
        }

        private bool PingTooBig(string host, int packetSize, int packetCount)
        {
            bool tooBig = false;
            int timeout = 3000;
            byte[] packet = new byte[packetSize];
            Ping pinger = new Ping();
            for (int i = 0; i < packetCount; ++i)
            {
                try
                {
                    var rep = pinger.Send(host, timeout, packet, new PingOptions(timeout, true));
                    if (rep.Status == IPStatus.PacketTooBig)
                        tooBig = true;
                }
                catch { }
            }
            return tooBig;
        }
    }
}