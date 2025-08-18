using System.Collections.Generic;

namespace SystemInfo.Core.Abstractions
{
    public interface IWmiQueryService
    {
        IEnumerable<IDictionary<string, object>> QueryAll(string alias, string whereClause = null);
        IEnumerable<IDictionary<string, object>> QuerySelect(string alias, IEnumerable<string> properties, string whereClause = null);
        string GetScalar(string alias, string property, string whereClause = null);
    }
}
