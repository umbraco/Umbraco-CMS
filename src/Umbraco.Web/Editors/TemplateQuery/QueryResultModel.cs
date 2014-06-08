using System.Collections.Generic;

namespace Umbraco.Web.Editors
{
    using System.Web.UI;

    public class QueryResultModel : IQueryResultModel
    {
        public QueryResultModel()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            QueryExpression = "CurrentPage.Site()";
        }

        public string QueryExpression { get; set; }
        public IEnumerable<ITemplateQueryResult> SampleResults { get; set; }
        public int ResultCount { get; set; }
        public double ExecutionTime { get; set; }
        public int Take { get; set; }
    }
}