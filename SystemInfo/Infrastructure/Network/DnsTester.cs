using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace SystemInfo.Infrastructure.Network
{
    public class DnsTester
    {
        private readonly DnsList dnsList;
        private int currentDnsIndex;
        private int totalDnsCount;
        public event EventHandler<string> ProgressUpdated;

        public DnsTester(DnsList dnsList)
        {
            this.dnsList = dnsList;
        }

        public async Task<List<(string DnsName, string Address, bool Success, IPStatus Status, long? RoundtripTime, string Error)>> TestDnsConnectivityAsync(NetworkInterface iface)
        {
            var results = new List<(string DnsName, string Address, bool Success, IPStatus Status, long? RoundtripTime, string Error)>();
            var dnsDictionary = dnsList.GetDnsList();
            currentDnsIndex = 0;
            totalDnsCount = dnsDictionary.Count;

            foreach (var dns in dnsDictionary)
            {
                currentDnsIndex++;
                var addresses = new List<string>();
                if (!string.IsNullOrEmpty(dns.Value.Primary)) addresses.Add(dns.Value.Primary);
                if (!string.IsNullOrEmpty(dns.Value.Secondary)) addresses.Add(dns.Value.Secondary);

                foreach (var address in addresses)
                {
                    try
                    {
                        var ping = new Ping();
                        var reply = await Task.Run(() => ping.Send(address, 1000));
                        results.Add((dns.Key, address, reply.Status == IPStatus.Success, reply.Status, reply.RoundtripTime, null));
                    }
                    catch (Exception ex)
                    {
                        results.Add((dns.Key, address, false, IPStatus.Unknown, null, ex.Message));
                    }
                }

                OnProgressUpdated($"DNSs Testadas [{currentDnsIndex}/{totalDnsCount}]");
                await Task.Yield(); // Libera a thread para permitir atualizações em tempo real
            }

            return results;
        }

        protected virtual void OnProgressUpdated(string status)
        {
            ProgressUpdated?.Invoke(this, status);
        }

        public void SavePingResultsToFile(List<(string DnsName, string Address, bool Success, IPStatus Status, long? RoundtripTime, string Error)> pingResults, NetworkInterface mainInterface)
        {
            var orderedResults = pingResults
                .OrderBy(p => !p.Success)
                .ThenBy(p => p.RoundtripTime ?? long.MaxValue)
                .ToList();

            var bestPing = pingResults.Where(p => p.Success)
                                     .OrderBy(p => p.RoundtripTime ?? long.MaxValue)
                                     .FirstOrDefault();

            try
            {
                using (StreamWriter writer = new StreamWriter("ping_result.txt"))
                {
                    writer.WriteLine($"Interface: {mainInterface.Name}");
                    writer.WriteLine($"Index: {mainInterface.GetIPProperties().GetIPv4Properties().Index}");
                    writer.WriteLine($"Best Performing DNS: {(bestPing.Success ? $"{bestPing.DnsName} ({bestPing.Address}, {bestPing.RoundtripTime} ms)" : "None")}");
                    writer.WriteLine("---");
                    writer.WriteLine("Ping Results (Ordered by Performance):");
                    writer.WriteLine("---");

                    foreach (var result in orderedResults)
                    {
                        writer.WriteLine($"DNS: {result.DnsName}");
                        writer.WriteLine($"Address: {result.Address}");
                        writer.WriteLine($"Status: {result.Status}");
                        writer.WriteLine($"Roundtrip Time: {(result.RoundtripTime.HasValue ? result.RoundtripTime.Value + " ms" : "N/A")}");
                        if (result.Error != null)
                            writer.WriteLine($"Error: {result.Error}");
                        writer.WriteLine("---");
                    }
                }
                Console.WriteLine("Resultados de ping salvos em ping_result.txt.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar ping_result.txt: {ex.Message}");
            }
        }
    }
}