using System.Collections.Generic;

namespace Umbraco.Web.Editors
{
    public class QueryModel : IQueryModel
    {
        public string ContentTypeAlias { get; set; }
        public int Id { get; set; }
        public IEnumerable<IQueryCondition> Wheres { get; set; }
        public string SortDirection { get; set; }
        public ISortExpression SortExpression { get; set; }
    }
}