using System.Collections.Generic;

namespace SystemInfo.Core.Abstractions
{
    public interface IWmiMethodInvoker
    {
        IDictionary<string, object> Invoke(string alias, string methodName, string whereClause, IDictionary<string, object> parameters = null);
    }
}
