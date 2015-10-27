using System;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;

namespace umbraco.editorControls.MultiNodeTreePicker
{
    /// <summary>
    /// Multi-node tree picker data type
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class MNTP_DataType : AbstractDataEditor
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MNTP_DataType"/> class.
        /// </summary>
        public MNTP_DataType()
        {
            RenderControl = m_Tree;
            m_Tree.Init += Tree_Init;
            m_Tree.Load += Tree_Load;
            DataEditorControl.OnSave += DataEditorControl_OnSave;

        }

        private umbraco.interfaces.IData m_Data;

        /// <summary>
        /// The internal tree picker control to render
        /// </summary>
        private readonly MNTP_DataEditor m_Tree = new MNTP_DataEditor();

        /// <summary>
        /// Internal pre value editor to render
        /// </summary>
        private MNTP_PrevalueEditor m_PreValues;

        ///<summary>
        ///</summary>
        public override Guid Id
        {
            get
            {
                return new Guid(DataTypeGuids.MultiNodeTreePickerId);
            }
        }

        ///<summary>
        ///</summary>
        public override string DataTypeName
        {
            get
            {
                return "Multi-Node Tree Picker";
            }
        }

        ///<summary>
        ///</summary>
        public override IData Data
        {
            get
            {
                if (this.m_Data == null)
                {
                    m_Data = StoreAsCommaDelimited
                                 ? new umbraco.cms.businesslogic.datatype.DefaultData(this)
                                 : new XmlData(this);
                }

                return m_Data;
            }

        }

        /// <summary>
        /// Value indicating whether to store as comma separated or Xml
        /// </summary>
        public bool StoreAsCommaDelimited
        {
            get
            {
                return ((MNTP_PrevalueEditor)PrevalueEditor).StoreAsCommaDelimited;
            }
        }

        /// <summary>
        /// Initialize the tree, here's where we can set some initial properties for the tree
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tree_Init(object sender, EventArgs e)
        {
            var preVal = ((MNTP_PrevalueEditor)PrevalueEditor);
            m_Tree.TreeToRender = preVal.SelectedTreeType;
            m_Tree.XPathFilter = preVal.XPathFilter;
            m_Tree.MaxNodeCount = preVal.MaxNodeCount;
            m_Tree.ShowToolTips = preVal.ShowToolTip;
            m_Tree.XPathFilterMatchType = preVal.XPathFilterMatchType;
            m_Tree.StartNodeId = preVal.StartNodeId;
            m_Tree.DataTypeDefinitionId = DataTypeDefinitionId;
            m_Tree.ShowThumbnailsForMedia = preVal.ShowThumbnailsForMedia;
            m_Tree.PropertyName = this.DataTypeName;
            m_Tree.StartNodeSelectionType = preVal.StartNodeSelectionType;
            m_Tree.StartNodeXPathExpression = preVal.StartNodeXPathExpression;
            m_Tree.StartNodeXPathExpressionType = preVal.StartNodeXPathExpressionType;
            m_Tree.ControlHeight = preVal.ControlHeight;
            m_Tree.MinNodeCount = preVal.MinNodeCount;


        }

        /// <summary>
        /// Set the data source for the editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tree_Load(object sender, EventArgs e)
        {
            if (!m_Tree.Page.IsPostBack)
            {
                // set the value of the control
                if (Data.Value != null && !string.IsNullOrEmpty(Data.Value.ToString()))
                {
                    var strVal = this.Data.Value.ToString();

                    //check how the data is stored
                    if (StoreAsCommaDelimited)
                    {
                        //need to check if it was XML, if so, we'll clear the data (Error checkign)
                        if (!strVal.StartsWith("<"))
                        {
                            m_Tree.SelectedIds = this.Data.Value.ToString().Split(',');
                        }
                        else
                        {
                            //this must have been saved as XML before, we'll reset it but this will cause a loss of data
                            m_Tree.SelectedIds = new string[] { };
                        }
                    }
                    else
                    {
                        try
                        {
                            var xmlDoc = XDocument.Parse(this.Data.Value.ToString());
                            m_Tree.XmlValue = xmlDoc;
                        }
                        catch (XmlException)
                        {
                            //not a valid xml doc, must have been saved as csv before.
                            //set to empty val, this will cause a loss of data
                            m_Tree.XmlValue = null;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Handle the saving event, need to give data to Umbraco
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void DataEditorControl_OnSave(EventArgs e)
        {
            string val;
            if (StoreAsCommaDelimited)
            {
                val = string.Join(",", m_Tree.SelectedIds);
            }
            else
            {
                val = m_Tree.XmlValue == null ? string.Empty : m_Tree.XmlValue.ToString();
            }

            Data.Value = !string.IsNullOrEmpty(val) ? val : null;
        }

        /// <summary>
        /// return a custom pre value editor
        /// </summary>
        public override IDataPrevalue PrevalueEditor
        {
            get
            {
                return m_PreValues ?? (m_PreValues = new MNTP_PrevalueEditor(this));
            }
        }

        internal const string PersistenceCookieName = "MultiNodeTreePicker";

        /// <summary>
        /// Helper method to ensure the pesistence cookie is cleared.
        /// This is used on app startup and after editing the pre-value editor
        /// </summary>
        internal static void ClearCookiePersistence()
        {
            if (HttpContext.Current == null)
            {
                return;
            }

            if (HttpContext.Current.Response.Cookies[PersistenceCookieName] != null)
            {
                HttpContext.Current.Response.Cookies[PersistenceCookieName].Expires = DateTime.Now.AddDays(-1);
            }
        }
    }
}
