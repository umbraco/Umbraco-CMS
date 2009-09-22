using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;

namespace umbraco.Linq.Core.Node
{
    /// <summary>
    /// Data Provider for LINQ to umbraco via umbraco ndoes
    /// </summary>
    /// <remarks>
    /// This class provides a data access model for the umbraco XML cache.
    /// It is responsible for the access to the XML and construction of nodes from it.
    /// 
    /// The <see cref="umbraco.Linq.Core.Node.NodeDataProvider"/> is capable of reading the XML cache from either the path provided in the umbraco settings or from a specified location on the file system.
    /// </remarks>
    public sealed class NodeDataProvider : UmbracoDataProvider
    {
        private string _xmlPath;
        private Dictionary<UmbracoInfoAttribute, IContentTree> _trees;
        private bool _enforceSchemaValidation;
        private XDocument _xml;
        private const string UMBRACO_XSD_PATH = "umbraco.Linq.Core.Node.UmbracoConfig.xsd";
        private Dictionary<string, Type> _knownTypes;

        internal XDocument Xml
        {
            get
            {
                if (this._xml == null)
                {
                    this._xml = XDocument.Load(this._xmlPath);

                    if (this._enforceSchemaValidation)
                    {
                        XmlSchemaSet schemas = new XmlSchemaSet();
                        //read the resorce for the XSD to validate against
                        schemas.Add("", System.Xml.XmlReader.Create(this.GetType().Assembly.GetManifestResourceStream(UMBRACO_XSD_PATH)));

                        //we'll have a list of all validation exceptions to put them to the screen
                        List<XmlSchemaException> exList = new List<XmlSchemaException>();

                        //some funky in-line event handler. Lambda loving goodness ;)
                        this._xml.Validate(schemas, (o, e) => { exList.Add(e.Exception); });

                        if (exList.Count > 0)
                        {
                            //dump out the exception list
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("The following validation errors occuring with the XML:");
                            foreach (var item in exList)
                            {
                                sb.AppendLine(" * " + item.Message + " - " + item.StackTrace);
                            }
                            throw new XmlSchemaException(sb.ToString());
                        }
                    }
                }

                return this._xml; //cache the XML in memory to increase performance and force the disposable pattern
            }
        }

        private void Init(string xmlPath)
        {
            if (string.IsNullOrEmpty(xmlPath))
            {
                throw new ArgumentNullException("xmlPath");
            }

            if (!File.Exists(xmlPath))
            {
                throw new FileNotFoundException("The XML used by the provider must exist", xmlPath);
            }
            this._xmlPath = xmlPath;

            this._trees = new Dictionary<UmbracoInfoAttribute, IContentTree>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeDataProvider"/> class using umbraco settings as XML path
        /// </summary>
        public NodeDataProvider()
            : this(umbraco.presentation.UmbracoContext.Current.Server.MapPath(umbraco.presentation.UmbracoContext.Current.Server.ContentXmlPath))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeDataProvider"/> class
        /// </summary>
        /// <remarks>
        /// This constructor is ideal for unit testing as it allows for the XML to be located anywhere
        /// </remarks>
        /// <param name="xmlPath">The path of the umbraco XML</param>
        public NodeDataProvider(string xmlPath)
            : this(xmlPath, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeDataProvider"/> class.
        /// </summary>
        /// <param name="xmlPath">The XML path.</param>
        /// <param name="enforceValidation">if set to <c>true</c> when the XML document is accessed validation against the umbraco XSD will be done.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the xmlPath is null</exception>
        /// <exception cref="System.IO.FileNotFoundException">Thrown when the xmlPath does not resolve to a physical file</exception>
        public NodeDataProvider(string xmlPath, bool enforceValidation)
        {
            this.Init(xmlPath);
            this._enforceSchemaValidation = enforceValidation;
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

            var attr = ReflectionAssistance.GetumbracoInfoAttribute(typeof(TDocType));

            if (!this._trees.ContainsKey(attr))
            {
                SetupNodeTree<TDocType>(attr);
            }

            return (NodeTree<TDocType>)this._trees[attr];
        }

        internal void SetupNodeTree<TDocType>(UmbracoInfoAttribute attr) where TDocType : DocTypeBase, new()
        {
            var tree = new NodeTree<TDocType>(this);
            if (!this._trees.ContainsKey(attr))
            {
                this._trees.Add(attr, tree); //cache so it's faster to get next time 
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

            var parentXml = this.Xml.Descendants("node").SingleOrDefault(d => (int)d.Attribute("id") == id);

            if (!ReflectionAssistance.CompareByAlias(typeof(TDocType), parentXml))
            {
                throw new DocTypeMissMatchException((string)parentXml.Attribute("nodeTypeAlias"), ReflectionAssistance.GetumbracoInfoAttribute(typeof(TDocType)).Alias);
            }

            if (parentXml == null) //really shouldn't happen!
            {
                throw new ArgumentException("Parent ID \"" + id + "\" cannot be found in the loaded XML. Ensure that the umbracoDataContext is being disposed of once it is no longer needed");
            }

            var parent = new TDocType();
            parent.LoadFromXml(parentXml);

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

            var startElement = this.Xml.Descendants("node").Single(x => (int)x.Attribute("id") == startNodeId);
            var ancestorElements = startElement.Ancestors("node");

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
                var alias = (string)ancestor.Attribute("nodeTypeAlias");
                var t = KnownTypes[alias];
                var instaceOfT = (DocTypeBase)Activator.CreateInstance(t); //create an instance of the type and down-cast so we can use it
                instaceOfT.LoadFromXml(ancestor);
                instaceOfT.Provider = this;
                ancestors.Add(instaceOfT);
                yield return instaceOfT;
            }

            //return ancestors;
        }

        //Will this cause a problem with trust levels...?
        internal Dictionary<string, Type> KnownTypes
        {
            get
            {
                if (this._knownTypes == null)
                {
                    this._knownTypes = (from a in AppDomain.CurrentDomain.GetAssemblies()
                                       from t in a.GetTypes()
                                       where t.GetCustomAttributes(typeof(DocTypeAttribute), true).Length == 1
                                       select t).ToDictionary(k => ((UmbracoInfoAttribute)k.GetCustomAttributes(typeof(UmbracoInfoAttribute), true)[0]).Alias);
                }

                return this._knownTypes;
            }
        }
    }
}
