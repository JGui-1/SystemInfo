using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Management;

namespace SystemInfo.Core.Utilities
{
    public static class ValueFormatter
    {
        public static string Format(object value)
        {
            if (value == null) return "N/A";

            if (value is string s)
            {
                if (IsWmiDateTime(s, out var dt))
                    return dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                return s;
            }

            if (value is ManagementBaseObject mbo)
            {
                try
                {
                    var parts = mbo.Properties
                        .Cast<PropertyData>()
                        .Select(p => $"{p.Name}={Format(p.Value)}");
                    return "{ " + string.Join("; ", parts) + " }";
                }
                catch
                {
                    return mbo.ToString();
                }
            }

            if (value is ManagementObjectCollection moc)
            {
                var items = moc.Cast<object>().Select(Format);
                return string.Join(", ", items);
            }

            if (value is IEnumerable enu && value is not string)
            {
                var list = enu.Cast<object?>().Select(v => Format(v));
                return string.Join(", ", list);
            }

            return value.ToString();
        }

        private static bool IsWmiDateTime(string s, out DateTime dt)
        {
            dt = default;
            try
            {
                // Expect format like 20200101010203.000000+000
                if (s.Length >= 21 && char.IsDigit(s[0]) && s[14] == '.')
                {
                    var year = int.Parse(s.Substring(0, 4));
                    var mon = int.Parse(s.Substring(4, 2));
                    var day = int.Parse(s.Substring(6, 2));
                    var hour = int.Parse(s.Substring(8, 2));
                    var min = int.Parse(s.Substring(10, 2));
                    var sec = int.Parse(s.Substring(12, 2));
                    dt = new DateTime(year, mon, day, hour, min, sec, DateTimeKind.Utc);
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}
