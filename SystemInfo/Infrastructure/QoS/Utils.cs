using System;

namespace SystemInfo.Infrastructure.QoS
{
    public static class QoSUtils
    {
        public static long? ParseRateToBitsPerSec(string rate)
        {
            if (string.IsNullOrWhiteSpace(rate)) return null;
            rate = rate.Trim().ToLowerInvariant();

            double multiplier = 1;
            if (rate.EndsWith("kbps")) { multiplier = 1000; rate = rate[..^4]; }
            else if (rate.EndsWith("mbps")) { multiplier = 1000_000; rate = rate[..^4]; }
            else if (rate.EndsWith("gbps")) { multiplier = 1000_000_000; rate = rate[..^4]; }
            else if (rate.EndsWith("bps")) { multiplier = 1; rate = rate[..^3]; }

            if (double.TryParse(rate, out double value))
                return (long)(value * multiplier);

            return null;
        }
    }
}
