using System;

namespace SystemInfo.Infrastructure.QoS
{
    public class QoSOptions
    {
        public string Name { get; set; }
        public int DSCPValue { get; set; }
        public long? ThrottleRate { get; set; }
        public string AppPath { get; set; }
        public string Protocol { get; set; }
        public int? LocalPort { get; set; }
        public int? RemotePort { get; set; }
        public string LocalIP { get; set; }
        public string RemoteIP { get; set; }

        public override string ToString()
        {
            return $"Policy={Name}, DSCP={DSCPValue}, Throttle={(ThrottleRate.HasValue ? ThrottleRate.Value + " bps" : "N/A")}, App={AppPath}, Proto={Protocol}, LPort={LocalPort}, RPort={RemotePort}, LIP={LocalIP}, RIP={RemoteIP}";
        }
    }
}
