using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using SystemInfo.Core.Abstractions;

namespace SystemInfo.Infrastructure.System
{
    public class NetworkMonitor : INetworkMonitor
    {
        private sealed class Sample
        {
            public long Sent;
            public long Received;
            public DateTime When;
        }

        // chave = NIC.Id (estável), valor = último snapshot
        private readonly Dictionary<string, Sample> _last = new(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<(string Adapter, float BytesSentPerSec, float BytesReceivedPerSec)> GetTrafficSnapshot()
        {
            var now = DateTime.UtcNow;

            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Ignora interfaces inativas/loopback
                if (nic.OperationalStatus != OperationalStatus.Up ||
                    nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                float sentPerSec = 0, recvPerSec = 0;

                try
                {
                    var stats = nic.GetIPv4Statistics();
                    var key = nic.Id;

                    if (_last.TryGetValue(key, out var prev))
                    {
                        var elapsed = (float)(now - prev.When).TotalSeconds;
                        if (elapsed > 0.0001f)
                        {
                            var dSent = stats.BytesSent - prev.Sent;
                            var dRecv = stats.BytesReceived - prev.Received;

                            sentPerSec = dSent < 0 ? 0 : dSent / elapsed;
                            recvPerSec = dRecv < 0 ? 0 : dRecv / elapsed;
                        }

                        // Atualiza baseline
                        prev.Sent = stats.BytesSent;
                        prev.Received = stats.BytesReceived;
                        prev.When = now;
                    }
                    else
                    {
                        // Primeira medição dessa NIC → cadastra baseline
                        _last[key] = new Sample
                        {
                            Sent = stats.BytesSent,
                            Received = stats.BytesReceived,
                            When = now
                        };
                    }
                }
                catch
                {
                    // Alguns drivers/NICs podem lançar erro → ignora
                }

                yield return (nic.Description, sentPerSec, recvPerSec);
            }
        }

        public IEnumerable<(string Local, string Remote, string State)> GetActiveConnections()
        {
            List<(string, string, string)> result = new();

            try
            {
                var ipProps = IPGlobalProperties.GetIPGlobalProperties();
                var conns = ipProps.GetActiveTcpConnections();

                foreach (var c in conns)
                {
                    result.Add(($"{c.LocalEndPoint}", $"{c.RemoteEndPoint}", c.State.ToString()));
                }
            }
            catch
            {
                // Ambiente restrito/sem permissão → retorna vazio
            }

            foreach (var item in result)
                yield return item;
        }
    }
}
