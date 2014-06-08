using System.Collections.Generic;

namespace Umbraco.Web.Editors
{
    public interface IQueryModel
    {

        string ContentTypeAlias { get; set; }

        /// <summary>
        /// IPublishedContent Id
        /// </summary>
        int Id { get; set; }

        /// <summary>
        ///  Wheres.FirstOrDefault()
        /// </summary>
        IEnumerable<IQueryCondition> Wheres { get; set; }

        /// <summary>
        /// enum?
        /// </summary>
        string SortDirection { get; set; }

        //IEnumerable<string> Wheres { get; set; }

        ISortExpression SortExpression { get; set; }
    }
}