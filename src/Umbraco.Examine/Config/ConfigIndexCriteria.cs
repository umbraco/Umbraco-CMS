using System.Collections.Generic;
using System.Linq;
using Examine;

namespace Umbraco.Examine.Config
{
    /// <summary>
    /// a data structure for storing indexing/searching instructions based on config based indexers
    /// </summary>
    public class ConfigIndexCriteria
    {
        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="standardFields"></param>
        ///<param name="userFields"></param>
        ///<param name="includeNodeTypes"></param>
        ///<param name="excludeNodeTypes"></param>
        ///<param name="parentNodeId"></param>
        public ConfigIndexCriteria(IEnumerable<FieldDefinition> standardFields, IEnumerable<FieldDefinition> userFields, IEnumerable<string> includeNodeTypes, IEnumerable<string> excludeNodeTypes, int? parentNodeId)
        {
            UserFields = userFields.ToList();
            StandardFields = standardFields.ToList();
            IncludeItemTypes = includeNodeTypes;
            ExcludeItemTypes = excludeNodeTypes;
            ParentNodeId = parentNodeId;
        }

        public IEnumerable<FieldDefinition> StandardFields { get; internal set; }
        public IEnumerable<FieldDefinition> UserFields { get; internal set; }

        public IEnumerable<string> IncludeItemTypes { get; internal set; }
        public IEnumerable<string> ExcludeItemTypes { get; internal set; }
        public int? ParentNodeId { get; internal set; }
    }
}