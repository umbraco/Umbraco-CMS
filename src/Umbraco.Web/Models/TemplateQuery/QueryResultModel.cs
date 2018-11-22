using System.Collections.Generic;

namespace Umbraco.Web.Models.TemplateQuery
{
    
    public class QueryResultModel
    {
        
        public string QueryExpression { get; set; }
        public IEnumerable<TemplateQueryResult> SampleResults { get; set; }
        public int ResultCount { get; set; }
        public double ExecutionTime { get; set; }
        public int Take { get; set; }
    }
}