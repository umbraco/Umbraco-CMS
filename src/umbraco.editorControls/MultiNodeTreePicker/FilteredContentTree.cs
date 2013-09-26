using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using umbraco.cms.businesslogic.web;
using umbraco.cms.presentation.Trees;

namespace umbraco.editorControls.MultiNodeTreePicker
{
    /// <summary>
    /// FilteredContentTree for the MultiNodeTreePicker
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class FilteredContentTree : BaseContentTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilteredContentTree"/> class.
        /// </summary>
        /// <param name="app">The app.</param>
        public FilteredContentTree(string app)
            : base(app)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        private Document m_UserStartNodeDoc;

        /// <summary>
        /// 
        /// </summary>
        private Document m_DefinedStartNodeDoc;

        /// <summary>
        /// The start node id determined by the defined id and by the user's defined id
        /// </summary>
        private int? m_DeterminedStartNodeId = null;

        protected override bool LoadMinimalDocument
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the Document object of the starting node for the current User. This ensures
        /// that the Document object is only instantiated once.
        /// </summary>
        protected Document UserStartNodeDoc
        {
            get
            {
                if (m_UserStartNodeDoc == null)
                {
                    if (CurrentUser.StartNodeId <= 0)
                    {
                        return new Document(uQuery.RootNodeId, true);
                    }
                    m_UserStartNodeDoc = new Document(CurrentUser.StartNodeId);
                }
                return m_UserStartNodeDoc;
            }
        }

        /// <summary>
        /// Returns the Document object of the starting node that is defined in the prevalue editor. This ensures
        /// that the Document object is only instantiated once.
        /// </summary>        
        protected Document DefinedStartNodeDoc
        {
            get
            {
                if (m_DefinedStartNodeDoc == null)
                {
                    var startNodeSelectionType =
                        this.GetPersistedCookieValue(x => x.MntpGetStartNodeSelectionType(this.GetDataTypeId()),
                                                     NodeSelectionType.Picker);
                    switch (startNodeSelectionType)
                    {
                        case NodeSelectionType.Picker:
                            //if it is a picker, then find the start node id
                            var definedId = this.GetPersistedCookieValue(
                                x => x.MntpGetStartNodeId(this.GetDataTypeId()), uQuery.RootNodeId);
                            //return a document with id -1 (don't set this up as it will exception!)
                            m_DefinedStartNodeDoc = (definedId > 0) ? new Document(definedId) : new Document(uQuery.RootNodeId, true);
                            break;
                        case NodeSelectionType.XPathExpression:
                            //if it is an expression, then we need to find the start node based on the xpression type, etc...
                            var expressionType =
                                this.GetPersistedCookieValue(
                                    x => x.MntpGetStartNodeXPathExpressionType(this.GetDataTypeId()),
                                    XPathExpressionType.Global);
                            //the default expression should match both schema types
                            var xpath =
                                this.GetPersistedCookieValue(
                                    x => x.MntpGetStartNodeXPathExpression(this.GetDataTypeId()), "//*[number(@id)>0]");
                            switch (expressionType)
                            {
                                case XPathExpressionType.Global:
                                    //if its a global expression, then we need to run the xpath against the entire tree
                                    var nodes = uQuery.GetNodesByXPath(xpath);
                                    //we'll just use the first node found (THERE SHOULD ONLY BE ONE)
                                    m_DefinedStartNodeDoc = nodes.Any() ? new Document(nodes.First().Id) : null;
                                    break;
                                case XPathExpressionType.FromCurrent:
                                    //if it's a relative query, then it cannot start with //!
                                    if (xpath.StartsWith("/"))
                                    {
                                        throw new InvalidOperationException("A relative xpath query cannot start with a slash");
                                    }

                                    //if it's a FromCurrent expression, then we need to run the xpath from this node and below
                                    var currId =
                                        this.GetPersistedCookieValue(
                                            x => x.MntpGetCurrentEditingNode(this.GetDataTypeId()), uQuery.RootNodeId);
                                    var currNode = umbraco.library.GetXmlNodeById(currId.ToString());
                                    if (currNode.MoveNext())
                                    {
                                        if (currNode.Current != null)
                                        {
                                            var result = currNode.Current.Select(xpath);
                                            //set it to the first node found (if there is one), otherwise to -1
                                            if (result.Current != null)
                                                m_DefinedStartNodeDoc = result.MoveNext() ? uQuery.GetDocument(result.Current.GetAttribute("id", string.Empty)) : null;
                                            else
                                                m_DefinedStartNodeDoc = null;
                                        }
                                        else
                                            m_DefinedStartNodeDoc = null;
                                    }
                                    else
                                    {
                                        m_DefinedStartNodeDoc = null;
                                    }
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                }
                return m_DefinedStartNodeDoc;
            }
        }

        #region Overridden methods
        /// <summary>
        /// Determines the allowed start node id based on the users start node id and the 
        /// defined start node id in the data type.
        /// </summary>
        public override int StartNodeID
        {
            get
            {
                //we only need to determine the id once
                if (m_DeterminedStartNodeId.HasValue)
                {
                    return m_DeterminedStartNodeId.Value;
                }

                //based on security principles, get the actual start node id
                m_DeterminedStartNodeId = this.GetStartNodeId(DefinedStartNodeDoc, UserStartNodeDoc);

                return m_DeterminedStartNodeId.Value;
            }
        }

        /// <summary>
        /// Creates the root node.
        /// </summary>
        /// <param name="rootNode">The root node.</param>
        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            if (!this.SetNullTreeRootNode(StartNodeID, ref rootNode, app))
            {
                rootNode.Action = "javascript:openContent(-1);";
                rootNode.Source = this.GetTreeServiceUrlWithParams(StartNodeID, this.GetDataTypeId());
                if (StartNodeID > 0)
                {
                    var startNode = new Document(StartNodeID);
                    rootNode.Text = startNode.Text;
                    rootNode.Icon = startNode.ContentTypeIcon;
                }
            }
        }

        /// <summary>
        /// Called when [render node].
        /// </summary>
        /// <param name="xNode">The x node.</param>
        /// <param name="doc">The doc.</param>
        protected override void OnRenderNode(ref XmlTreeNode xNode, Document doc)
        {
            base.OnRenderNode(ref xNode, doc);

            var dataTypeId = this.GetDataTypeId();
            var xpath = this.GetXPathFromCookie(dataTypeId);
            var xPathType = this.GetXPathFilterTypeFromCookie(dataTypeId);

            // resolves any Umbraco params in the XPath
            xpath = uQuery.ResolveXPath(xpath);

            var xDoc = new XmlDocument();
            XmlNode xmlDoc;
            if (!doc.Published)
            {
                xmlDoc = doc.ToPreviewXml(xDoc);
            }
            else
            {
                xmlDoc = doc.ToXml(xDoc, false);
            }

            var xmlString = "<root>" + xmlDoc.OuterXml + "</root>";
            var xml = XElement.Parse(xmlString);

            xNode.DetermineClickable(xpath, xPathType, xml);

            //ensure that the NodeKey is passed through
            xNode.Source = this.GetTreeServiceUrlWithParams(int.Parse(xNode.NodeID), dataTypeId);
        }
        #endregion
    }
}
