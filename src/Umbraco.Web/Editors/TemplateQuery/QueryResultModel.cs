using System.Collections.Generic;

namespace Umbraco.Web.Editors
{
    
    public class QueryResultModel
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
        public IEnumerable<TemplateQueryResult> SampleResults { get; set; }
        public int ResultCount { get; set; }
        public double ExecutionTime { get; set; }
        public int Take { get; set; }
    }
}