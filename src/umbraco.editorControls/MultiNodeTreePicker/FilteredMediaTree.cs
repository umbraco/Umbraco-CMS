using System;
using System.Xml;
using System.Xml.Linq;
using umbraco.cms.businesslogic.media;
using umbraco.cms.presentation.Trees;

namespace umbraco.editorControls.MultiNodeTreePicker
{
    /// <summary>
    /// FilteredMediaTree for the MultiNodeTreePicker.
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class FilteredMediaTree : BaseMediaTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilteredMediaTree"/> class.
        /// </summary>
        /// <param name="app">The app.</param>
        public FilteredMediaTree(string app)
            : base(app)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        private Media m_UserStartNodeMedia;

        /// <summary>
        /// 
        /// </summary>
        private Media m_DefinedStartNodeMedia;

        /// <summary>
        /// Returns the Media object of the starting node for the current User. This ensures
        /// that the Media object is only instantiated once.
        /// </summary>
        protected Media UserStartNodeDoc
        {
            get
            {
                if (m_UserStartNodeMedia == null)
                {
                    if (CurrentUser.StartMediaId <= 0)
                    {
                        return new Media(-1, true);
                    }
                    m_UserStartNodeMedia = new Media(CurrentUser.StartMediaId);
                }
                return m_UserStartNodeMedia;
            }
        }

        /// <summary>
        /// Returns the Media object of the starting node that is defined in the prevalue editor. This ensures
        /// that the Media object is only instantiated once.
        /// </summary>
        protected Media DefinedStartNodeMedia
        {
            get
            {
                if (m_DefinedStartNodeMedia == null)
                {
                    var definedId = this.GetPersistedCookieValue(x => x.MntpGetStartNodeId(this.GetDataTypeId()), -1);
                    if (definedId <= 0)
                    {
                        return new Media(-1, true);
                    }
                    m_DefinedStartNodeMedia = new Media(definedId);
                }
                return m_DefinedStartNodeMedia;
            }
        }

        /// <summary>
        /// The start node id determined by the defined id and by the user's defined id
        /// </summary>
        private int? m_DeterminedStartNodeId = null;

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

                m_DeterminedStartNodeId = this.GetStartNodeId(DefinedStartNodeMedia, UserStartNodeDoc);

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
                    var startNode = new Media(StartNodeID);
                    rootNode.Text = startNode.Text;
                    rootNode.Icon = startNode.ContentTypeIcon;
                }
            }
        }

        /// <summary>
        /// Called when [before node render].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="node">The node.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnBeforeNodeRender(ref XmlTree sender, ref XmlTreeNode node, EventArgs e)
        {

            var xpath = this.GetXPathFromCookie(this.GetDataTypeId());
            var xPathType = this.GetXPathFilterTypeFromCookie(this.GetDataTypeId());
            var xDoc = new XmlDocument();

            var xmlNode = umbraco.library.GetMedia(int.Parse(node.NodeID), false).Current.OuterXml;

            var xmlString = "<root>" + xmlNode + "</root>";
            var xml = XElement.Parse(xmlString);

            node.DetermineClickable(xpath, xPathType, xml);

            //ensure that the NodeKey is passed through
            node.Source = this.GetTreeServiceUrlWithParams(int.Parse(node.NodeID), this.GetDataTypeId());

            base.OnBeforeNodeRender(ref sender, ref node, e);
        }
        #endregion
    }
}
