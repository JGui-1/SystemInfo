using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using SystemInfo.Core.Abstractions;

namespace SystemInfo.Infrastructure.Network
{
    public class NetworkManager : SystemInfo.Core.Abstractions.INetworkManager
    {
        public Dictionary<string, InterfaceInfo> LoadInterfaces()
        {
            var interfaceData = new Dictionary<string, InterfaceInfo>();
            var ifaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.GetIPProperties() != null
                         && x.Supports(NetworkInterfaceComponent.IPv4)
                         && x.GetIPProperties().GetIPv4Properties() != null);

            foreach (var iface in ifaces)
            {
                var ipProps = iface.GetIPProperties().GetIPv4Properties();
                string mtu = ipProps.Mtu.ToString();

                Process p = new Process();
                p.StartInfo = new ProcessStartInfo("cmd", "/c netsh interface ipv4 show subinterface " + ipProps.Index)
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Verb = "runas",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                try
                {
                    p.Start();
                    var res = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    var lines = res.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].StartsWith("------"))
                        {
                            mtu = lines[i + 1].Trim().Substring(0, lines[i + 1].Trim().IndexOf(' '));
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao obter MTU para {iface.Name}: {ex.Message}");
                    mtu = "1500"; // Default
                }

                interfaceData.Add(iface.Name, new InterfaceInfo
                {
                    Name = iface.Name,
                    Type = iface.NetworkInterfaceType.ToString(),
                    Index = ipProps.Index,
                    Mtu = mtu
                });
            }
            return interfaceData;
        }

        public void SaveInterfacesToFile(Dictionary<string, InterfaceInfo> interfaceData)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter("interfaces.txt"))
                {
                    foreach (var pair in interfaceData)
                    {
                        var info = pair.Value;
                        writer.WriteLine($"Interface: {info.Name}");
                        writer.WriteLine($"Type: {info.Type}");
                        writer.WriteLine($"Index: {info.Index}");
                        writer.WriteLine($"MTU: {info.Mtu}");
                        writer.WriteLine("---");
                    }
                }
                Console.WriteLine("Interfaces salvas em interfaces.txt.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar interfaces.txt: {ex.Message}");
            }
        }

        public NetworkInterface GetMainNetworkInterface()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.OperationalStatus == OperationalStatus.Up
                         && x.Supports(NetworkInterfaceComponent.IPv4)
                         && x.GetIPProperties() != null
                         && x.GetIPProperties().GetIPv4Properties() != null);

            foreach (var iface in interfaces)
            {
                var ipProps = iface.GetIPProperties();
                var gateways = ipProps.GatewayAddresses
                    .Where(g => g.Address != null && !g.Address.Equals(IPAddress.Any))
                    .ToList();

                if (gateways.Any() && !IsVpnInterface(iface))
                {
                    try
                    {
                        var ping = new Ping();
                        var reply = ping.Send("8.8.8.8", 1000);
                        if (reply.Status == IPStatus.Success)
                            return iface;
                    }
                    catch { }
                }
            }
            return null;
        }

        private bool IsVpnInterface(NetworkInterface iface)
        {
            var vpnTypes = new[] { NetworkInterfaceType.Ppp, NetworkInterfaceType.Tunnel };
            if (vpnTypes.Contains(iface.NetworkInterfaceType))
                return true;

            var description = iface.Description.ToLower();
            return description.Contains("vpn") || description.Contains("virtual") || description.Contains("tap-windows") || description.Contains("openvpn");
        }
    }

    public class LegacyInterfaceInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Index { get; set; }
        public string Mtu { get; set; }
    }
}