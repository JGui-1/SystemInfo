using System.Collections;
using System.Linq;

namespace SystemInfo.Core.Utilities
{
    public static class ValueFormatter
    {
        public static string Format(object value)
        {
            if (value == null) return "null";
            if (value is string s) return s;
            if (value is IEnumerable enu && value is not string)
            {
                var list = enu.Cast<object?>().Select(v => v?.ToString() ?? "null");
                return string.Join(", ", list);
            }
            return value.ToString();
        }
    }
}
