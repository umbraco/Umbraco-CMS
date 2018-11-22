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

        private string _indexPath = "";

        /// <summary>
        /// The folder path of where the lucene index is stored
        /// </summary>
        /// <value>The index path.</value>
        /// <remarks>
        /// This can be set at runtime but will not be persisted to the configuration file
        /// </remarks>
        [ConfigurationProperty("IndexPath", IsRequired = true, IsKey = false)]
        public string IndexPath
        {
            get
            {
                if (string.IsNullOrEmpty(_indexPath))
                    _indexPath = (string)this["IndexPath"];

                return _indexPath;
            }
            set => _indexPath = value;
        }

        /// <summary>
        /// Returns the DirectoryInfo object for the index path.
        /// </summary>
        /// <value>The index directory.</value>
        public DirectoryInfo IndexDirectory
        {
            get
            {
                //TODO: Get this out of the index set. We need to use the Indexer's DataService to lookup the folder so it can be unit tested. Probably need DataServices on the searcher then too

                //we need to de-couple the context
                if (HttpContext.Current != null)
                    return new DirectoryInfo(HttpContext.Current.Server.MapPath(this.IndexPath));
                else if (HostingEnvironment.ApplicationID != null)
                    return new DirectoryInfo(HostingEnvironment.MapPath(this.IndexPath));
                else
                    return new DirectoryInfo(this.IndexPath);
            }
        }

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
