using System.Collections.Generic;

namespace Umbraco.Web.Editors
{
    public interface IQueryResultModel
    {
        string QueryExpression { get; set; }

        IEnumerable<ITemplateQueryResult> SampleResults { get; set; }

        int ResultCount { get; set; }

        double ExecutionTime { get; set; }

        int Take { get; set; }
    }
}