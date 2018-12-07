using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace Umbraco.Examine.Config
{
    public sealed class IndexSet : ConfigurationElement
    {

        [ConfigurationProperty("SetName", IsRequired = true, IsKey = true)]
        public string SetName => (string)this["SetName"];

        /// <summary>
        /// When this property is set, the indexing will only index documents that are descendants of this node.
        /// </summary>
        [ConfigurationProperty("IndexParentId", IsRequired = false, IsKey = false)]
        public int? IndexParentId
        {
            get
            {
                if (this["IndexParentId"] == null)
                    return null;

                return (int)this["IndexParentId"];
            }
        }

        /// <summary>
        /// The collection of node types to index, if not specified, all node types will be indexed (apart from the ones specified in the ExcludeNodeTypes collection).
        /// </summary>
        [ConfigurationCollection(typeof(IndexFieldCollection))]
        [ConfigurationProperty("IncludeNodeTypes", IsDefaultCollection = false, IsRequired = false)]
        public IndexFieldCollection IncludeNodeTypes => (IndexFieldCollection)base["IncludeNodeTypes"];

        /// <summary>
        /// The collection of node types to not index. If specified, these node types will not be indexed.
        /// </summary>
        [ConfigurationCollection(typeof(IndexFieldCollection))]
        [ConfigurationProperty("ExcludeNodeTypes", IsDefaultCollection = false, IsRequired = false)]
        public IndexFieldCollection ExcludeNodeTypes => (IndexFieldCollection)base["ExcludeNodeTypes"];

        /// <summary>
        /// A collection of user defined umbraco fields to index
        /// </summary>
        /// <remarks>
        /// If this property is not specified, or if it's an empty collection, the default user fields will be all user fields defined in Umbraco
        /// </remarks>
        [ConfigurationCollection(typeof(IndexFieldCollection))]
        [ConfigurationProperty("IndexUserFields", IsDefaultCollection = false, IsRequired = false)]
        public IndexFieldCollection IndexUserFields => (IndexFieldCollection)base["IndexUserFields"];

        /// <summary>
        /// The fields umbraco values that will be indexed. i.e. id, nodeTypeAlias, writer, etc...
        /// </summary>
        /// <remarks>
        /// If this is not specified, or if it's an empty collection, the default optins will be specified:
        /// - id
        /// - version
        /// - parentID
        /// - level
        /// - writerID
        /// - creatorID
        /// - nodeType
        /// - template
        /// - sortOrder
        /// - createDate
        /// - updateDate
        /// - nodeName
        /// - urlName
        /// - writerName
        /// - creatorName
        /// - nodeTypeAlias
        /// - path
        /// </remarks>
        [ConfigurationCollection(typeof(IndexFieldCollection))]
        [ConfigurationProperty("IndexAttributeFields", IsDefaultCollection = false, IsRequired = false)]
        public IndexFieldCollection IndexAttributeFields => (IndexFieldCollection)base["IndexAttributeFields"];
    }
}
