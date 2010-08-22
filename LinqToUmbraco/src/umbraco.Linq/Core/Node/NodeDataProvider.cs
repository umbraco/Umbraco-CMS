using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Reflection;
using umbraco.presentation;
using umbraco.cms.helpers;
using umbraco.BusinessLogic.Utils;

namespace umbraco.Linq.Core.Node
{
    /// <summary>
    /// Data Provider for LINQ to umbraco via umbraco ndoes
    /// </summary>
    /// <remarks>
    /// <para>This class provides a data access model for the umbraco XML cache.
    /// It is responsible for the access to the XML and construction of nodes from it.</para>
    /// <para>The <see cref="umbraco.Linq.Core.Node.NodeDataProvider"/> is capable of reading the XML cache from either the path provided in the umbraco settings or from a specified location on the file system.</para>
    /// </remarks>
    public sealed class NodeDataProvider : UmbracoDataProvider
    {
        private object lockObject = new object();
        private string _xmlPath;
        private Dictionary<UmbracoInfoAttribute, IContentTree> _trees;
        private XDocument _xml;
        private Dictionary<string, Type> _knownTypes;

        private bool _tryMemoryCache = false;

        internal XDocument Xml
        {
            get
            {
                if (this._xml == null)
                {
                    if (this._tryMemoryCache)
                    {
                        var doc = UmbracoContext.Current.Server.ContentXml;
                        if (doc != null)
                        {
                            this._xml = doc;
                        }
                        else
                        {
                            this._xml = XDocument.Load(this._xmlPath);
                        }
                    }
                    else
                    {
                        this._xml = XDocument.Load(this._xmlPath);
                    }
                }

                return this._xml; //cache the XML in memory to increase performance and force the disposable pattern
            }
        }

        /// <summary>
        /// Initializes the NodeDataProvider, performing validation
        /// </summary>
        /// <param name="xmlPath">The XML path.</param>
        /// <param name="newSchemaMode">if set to <c>true</c> [new schema mode].</param>
        protected void Init(string xmlPath, bool newSchemaMode)
        {
            if (!newSchemaMode)
                throw new NotSupportedException(this.Name + " only supports the new XML schema. Change the umbracoSettings.config to implement this and republish");

            if (string.IsNullOrEmpty(xmlPath))
                throw new ArgumentNullException("xmlPath");

            if (!File.Exists(xmlPath))
                throw new FileNotFoundException("The XML used by the provider must exist", xmlPath);

            this._xmlPath = xmlPath;
            this._trees = new Dictionary<UmbracoInfoAttribute, IContentTree>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeDataProvider"/> class using umbraco settings as XML path
        /// </summary>
        public NodeDataProvider()
            : this(UmbracoContext.Current.Server.MapPath(UmbracoContext.Current.Server.ContentXmlPath), UmbracoContext.Current.NewSchemaMode)
        {
            this._tryMemoryCache = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeDataProvider"/> class
        /// </summary>
        /// <param name="xmlPath">The path of the umbraco XML</param>
        /// <param name="newSchemaMode">Indicates which Schema mode is used for the XML file</param>
        /// <remarks>
        /// This constructor is ideal for unit testing as it allows for the XML to be located anywhere
        /// </remarks>
        public NodeDataProvider(string xmlPath, bool newSchemaMode)
        {
            this.Init(xmlPath, newSchemaMode);
        }

        #region IDisposable Members

        private bool _disposed;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected internal override void Dispose(bool disposing)
        {
            if (!this._disposed && disposing)
            {
                this._xmlPath = null;

                this._disposed = true;
            }
        }

        internal void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(null);
            }
        }

        #endregion

        /// <summary>
        /// Gets the name of the provider
        /// </summary>
        /// <value>The name of the provider.</value>
        public override string Name
        {
            get
            {
                return "NodeDataProvider";
            }
        }

        /// <summary>
        /// Loads the tree with the relivent DocTypes from the XML
        /// </summary>
        /// <typeparam name="TDocType">The type of the DocType to load.</typeparam>
        /// <returns><see cref="umbraco.Linq.Core.Node.NodeTree&lt;TDocType&gt;"/> representation of the content tree</returns>
        /// <exception cref="System.ObjectDisposedException">When the data provider has been disposed of</exception>
        public override Tree<TDocType> LoadTree<TDocType>()
        {
            CheckDisposed();

            var attr = ReflectionAssistance.GetUmbracoInfoAttribute(typeof(TDocType));

            if (!this._trees.ContainsKey(attr))
                SetupNodeTree<TDocType>(attr);

            return (NodeTree<TDocType>)this._trees[attr];
        }

        internal void SetupNodeTree<TDocType>(UmbracoInfoAttribute attr) where TDocType : DocTypeBase, new()
        {
            var tree = new NodeTree<TDocType>(this);
            if (!this._trees.ContainsKey(attr))
            {
                lock (lockObject)
                {
                    this._trees.Add(attr, tree); //cache so it's faster to get next time  
                }
            }
            else
            {
                this._trees[attr] = tree;
            }
        }

        /// <summary>
        /// Loads the specified id.
        /// </summary>
        /// <typeparam name="TDocType">The type of the doc type.</typeparam>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <exception cref="umbraco.Linq.Core.DocTypeMissMatchException">If the type of the parent does not match the provided type</exception>
        /// <exception cref="System.ArgumentException">No node found matching the provided ID for the parent</exception>
        /// <exception cref="System.ObjectDisposedException">When the data provider has been disposed of</exception>
        public override TDocType Load<TDocType>(int id)
        {
            CheckDisposed();

            var parentXml = this.Xml.Descendants().SingleOrDefault(d => d.Attribute("isDoc") != null && (int)d.Attribute("id") == id);

            if (!ReflectionAssistance.CompareByAlias(typeof(TDocType), parentXml))
                throw new DocTypeMissMatchException(parentXml.Name.LocalName, ReflectionAssistance.GetUmbracoInfoAttribute(typeof(TDocType)).Alias);

            if (parentXml == null) //really shouldn't happen!
                throw new ArgumentException("Parent ID \"" + id + "\" cannot be found in the loaded XML. Ensure that the umbracoDataContext is being disposed of once it is no longer needed");

            var parent = new TDocType();
            this.LoadFromXml(parentXml, parent);

            return parent;
        }

        /// <summary>
        /// Loads the associated (children) nodes with the relivent DocTypes
        /// </summary>
        /// <param name="parentNodeId">The parent node id.</param>
        /// <returns></returns>
        /// <exception cref="System.ObjectDisposedException">When the data provider has been disposed of</exception>
        public override AssociationTree<DocTypeBase> LoadAssociation(int parentNodeId)
        {
            CheckDisposed();

            NodeAssociationTree<DocTypeBase> associationTree = new NodeAssociationTree<DocTypeBase>(parentNodeId, this);

            return associationTree;
        }


        /// <summary>
        /// Loads the associated nodes with the relivent DocTypes
        /// </summary>
        /// <typeparam name="TDocType">The type of the DocType to load.</typeparam>
        /// <param name="nodes">The nodes.</param>
        /// <returns></returns>
        /// <exception cref="System.ObjectDisposedException">When the data provider has been disposed of</exception>
        public override AssociationTree<TDocType> LoadAssociation<TDocType>(IEnumerable<TDocType> nodes)
        {
            CheckDisposed();

            return new NodeAssociationTree<TDocType>(nodes);
        }

        /// <summary>
        /// Loads the ancestors for a node
        /// </summary>
        /// <param name="startNodeId">The start node id.</param>
        /// <returns></returns>
        /// <exception cref="System.ObjectDisposedException">When the data provider has been disposed of</exception>
        public override IEnumerable<DocTypeBase> LoadAncestors(int startNodeId)
        {
            CheckDisposed();

            var startElement = this.Xml.Descendants().Single(x => x.Attribute("isDoc") != null && (int)x.Attribute("id") == startNodeId);
            var ancestorElements = startElement.Ancestors();

            IEnumerable<DocTypeBase> ancestors = DynamicNodeCreation(ancestorElements);

            return ancestors;
        }

        /// <summary>
        /// Creates a collection of nodes with the type specified from the XML
        /// </summary>
        /// <param name="elements">The elements.</param>
        /// <returns>Collecton of .NET types from the XML</returns>
        internal IEnumerable<DocTypeBase> DynamicNodeCreation(IEnumerable<XElement> elements)
        {
            // TODO: investigate this method for performance bottlenecks
            // TODO: dataContext knows the types, maybe can load from there?
            List<DocTypeBase> ancestors = new List<DocTypeBase>();

            foreach (var ancestor in elements)
            {
                var alias = Casing.SafeAliasWithForcingCheck(ancestor.Name.LocalName);
                var t = KnownTypes[alias];
                var instaceOfT = (DocTypeBase)Activator.CreateInstance(t); //create an instance of the type and down-cast so we can use it
                this.LoadFromXml(ancestor, instaceOfT);
                instaceOfT.Provider = this;
                ancestors.Add(instaceOfT);
                yield return instaceOfT;
            }
        }

        internal Dictionary<string, Type> KnownTypes
        {
            get
            {
                if (this._knownTypes == null)
                {
                    this._knownTypes = new Dictionary<string, Type>();
                    var types = TypeFinder
                        .FindClassesOfType<DocTypeBase>()
                        .Where(t => t != typeof(DocTypeBase))
                        .ToDictionary(k =>
                        {
                            return ((UmbracoInfoAttribute)k.GetCustomAttributes(typeof(UmbracoInfoAttribute), true)[0]).Alias;
                        });

                    foreach (var type in types)
                        this._knownTypes.Add(Casing.SafeAliasWithForcingCheck(type.Key), type.Value);

                }

                return this._knownTypes;
            }
        }

        /// <summary>
        /// Flushes the cache for this provider
        /// </summary>
        public void Flush()
        {
            this.CheckDisposed();

            this._xml = null;
            this._trees.Clear();
        }

        /// <summary>
        /// Loads from XML.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml">The XML.</param>
        /// <param name="node">The node.</param>
        public void LoadFromXml<T>(XElement xml, T node) where T : DocTypeBase
        {
            if (!ReflectionAssistance.CompareByAlias(node.GetType(), xml))
            {
                throw new DocTypeMissMatchException(xml.Name.LocalName, ReflectionAssistance.GetUmbracoInfoAttribute(node.GetType()).Alias);
            }

            node.Id = (int)xml.Attribute("id");
            node.ParentNodeId = (int)xml.Attribute("parentID");
            node.NodeName = (string)xml.Attribute("nodeName");
            node.Version = (string)xml.Attribute("version");
            node.CreateDate = (DateTime)xml.Attribute("createDate");
            node.SortOrder = (int)xml.Attribute("sortOrder");
            node.UpdateDate = (DateTime)xml.Attribute("updateDate");
            node.CreatorID = (int)xml.Attribute("creatorID");
            node.CreatorName = (string)xml.Attribute("creatorName");
            node.WriterID = (int)xml.Attribute("writerID");
            node.WriterName = (string)xml.Attribute("writerName");
            node.Level = (int)xml.Attribute("level");
            node.TemplateId = (int)xml.Attribute("template");

            var properties = node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetCustomAttributes(typeof(PropertyAttribute), true).Count() > 0);
            foreach (var p in properties)
            {
                var attr = ReflectionAssistance.GetUmbracoInfoAttribute(p);

                XElement propertyXml = xml.Element(Casing.SafeAliasWithForcingCheck(attr.Alias));
                string data = null;
                //if the XML doesn't contain the property it means that the node hasn't been re-published with the property
                //so then we'll leave the data at null, otherwise let's grab it
                if (propertyXml != null)
                    data = propertyXml.Value;

                if (p.PropertyType.IsValueType && typeof(Nullable<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition()))
                {
                    if (string.IsNullOrEmpty(data))
                    {
                        //non-mandatory structs which have no value will be null
                        p.SetValue(node, null, null);
                    }
                    else
                    {
                        //non-mandatory structs which do have a value have to be cast based on the type of their Nullable<T>, found from the first (well, only) GenericArgument
                        p.SetValue(node, Convert.ChangeType(data, p.PropertyType.GetGenericArguments()[0]), null);
                    }
                }
                else
                {
                    // TODO: Address how Convert.ChangeType works in globalisation
                    p.SetValue(node, Convert.ChangeType(data, p.PropertyType), null);
                }
            }
        }
    }
}
