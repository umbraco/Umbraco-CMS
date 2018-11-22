using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;
using umbraco.uicontrols.TreePicker;
using Constants = Umbraco.Core.Constants;

namespace umbraco.editorControls.MultiNodeTreePicker
{
    /// <summary>
    /// The pre-value editor for the multi node tree picker.
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class MNTP_PrevalueEditor : Control, IDataPrevalue
    {
        private readonly umbraco.cms.businesslogic.datatype.BaseDataType m_DataType;
        private static readonly object Locker = new object();
        private SortedList m_PreValues = null;

        #region Public properties

        /// <summary>
        /// The chosen tree type to render
        /// </summary>
        public string SelectedTreeType
        {
            get
            {
                return GetPreValue(PropertyIndex.TreeType, x => x.Value, Constants.Applications.Content);
            }
        }

        /// <summary>
        /// An xpath filter to disable nodes to be selectable
        /// </summary>
        public string XPathFilter
        {
            get
            {
                return GetPreValue(PropertyIndex.XPathFilter, x => x.Value, string.Empty);
            }
        }

        /// <summary>
        /// The number of nodes this picker will support picking
        /// </summary>
        public int MaxNodeCount
        {
            get
            {
                return GetPreValue(PropertyIndex.MaxNodeCount, x =>
                {
                    var max = -1;
                    return int.TryParse(x.Value, out max) ? max : -1;
                }, -1);
            }
        }

        /// <summary>
        /// The minimum number of nodes this picker will support picking
        /// </summary>
        public int MinNodeCount
        {
            get
            {
                return GetPreValue(PropertyIndex.MinNodeCount, x =>
                {
                    var min = 0;
                    return int.TryParse(x.Value, out min) ? min : 0;
                }, 0);
            }
        }

        /// <summary>
        /// A boolean value indicating whether or not to show the informational tool tips 
        /// </summary>
        public bool ShowToolTip
        {
            get
            {
                return GetPreValue(PropertyIndex.ShowToolTip, x =>
                {
                    var show = true;
                    return bool.TryParse(x.Value, out show) ? show : true;
                }, true);

            }
        }

        /// <summary>
        /// Value to check if the data should be stored as CSV or XML
        /// </summary>
        public bool StoreAsCommaDelimited
        {
            get
            {
                return GetPreValue(PropertyIndex.StoreAsCommaDelimited, x =>
                {
                    var asComma = 0;
                    return int.TryParse(x.Value, out asComma) ? asComma == 1 : false;
                }, false);

            }
        }

        /// <summary>
        /// The XPath expression used when the node type selection is Xpath
        /// </summary>
        public string StartNodeXPathExpression
        {
            get
            {
                return GetPreValue(PropertyIndex.StartNodeXPathExpression, x => x.Value, string.Empty);
            }
        }

        /// <summary>
        /// The type of xpath expression used for the xpathexpressiontext if using an xpath node selection
        /// </summary>
        public XPathExpressionType StartNodeXPathExpressionType
        {
            get
            {
                return GetPreValue(PropertyIndex.StartNodeXPathExpressionType,
                    x => (XPathExpressionType)Enum.ToObject(typeof(XPathExpressionType), int.Parse(x.Value)),
                    XPathExpressionType.Global);
            }
        }

        /// <summary>
        /// The type of selection type to use for the start node
        /// </summary>
        public NodeSelectionType StartNodeSelectionType
        {
            get
            {
                return GetPreValue(PropertyIndex.StartNodeSelectionType,
                    x => (NodeSelectionType)Enum.ToObject(typeof(NodeSelectionType), int.Parse(x.Value)),
                    NodeSelectionType.Picker);
            }
        }

        /// <summary>
        /// The type of xpath filter applied
        /// </summary>
        public XPathFilterType XPathFilterMatchType
        {
            get
            {
                return GetPreValue(PropertyIndex.XPathFilterType,
                    x => (XPathFilterType)Enum.ToObject(typeof(XPathFilterType), int.Parse(x.Value)),
                    XPathFilterType.Disable);
            }
        }

        /// <summary>
        /// The start node id used when the node selection is a picker
        /// </summary>
        public int StartNodeId
        {
            get
            {
                return GetPreValue(PropertyIndex.StartNodeId, x =>
                {
                    var max = 0;
                    return int.TryParse(x.Value, out max) ? max : uQuery.RootNodeId;
                }, uQuery.RootNodeId);
            }
        }

        /// <summary>
        /// A boolean value indicating whether or not to show the thumbnails for media 
        /// </summary>
        public bool ShowThumbnailsForMedia
        {
            get
            {
                return GetPreValue(PropertyIndex.ShowThumbnails, x =>
                {
                    var show = true;
                    bool.TryParse(x.Value, out show);
                    return show;
                }, true);
            }
        }

        ///<summary>
        /// Returns the control height in pixels
        ///</summary>
        public int ControlHeight
        {
            get
            {
                return GetPreValue(PropertyIndex.ControlHeight, x =>
                {
                    var max = 0;
                    return int.TryParse(x.Value, out max) ? max : 200;
                }, 200);
            }
        }

        #endregion

        #region Protected properties

        /// <summary>
        /// The control height text box
        /// </summary>
        protected TextBox ControlHeightTextBox;

        /// <summary>
        /// 
        /// </summary>
        protected RadioButtonList StartNodeSelectionTypeRadioButtons;

        /// <summary>
        /// The start node id content picker
        /// </summary>
        protected SimpleContentPicker StartContentNodeIdPicker;

        /// <summary>
        /// The start node id media picker
        /// </summary>
        protected SimpleMediaPicker StartMediaNodeIdPicker;

        /// <summary>
        /// XPath expression type radio button list
        /// </summary>
        protected RadioButtonList StartNodeXPathExpressionTypeRadioButtons;

        /// <summary>
        /// 
        /// </summary>
        protected TextBox StartNodeXPathExpressionTextBox;

        /// <summary>
        /// 
        /// </summary>
        protected CheckBox ShowThumbnailsForMediaCheckBox;

        /// <summary>
        /// 
        /// </summary>
        protected DropDownList TreeTypeDropDown;

        /// <summary>
        /// 
        /// </summary>
        protected TextBox XPathFilterTextBox;

        /// <summary>
        /// Text box for maximum amount of items
        /// </summary>
        protected TextBox MaxItemsTextBox;

        /// <summary>
        /// Text box for minimum amount of items
        /// </summary>
        protected TextBox MinItemsTextBox;

        /// <summary>
        /// Minimum items validator
        /// </summary>
        protected RegularExpressionValidator NumbersMinItemsValidator;

        /// <summary>
        /// Validator for validating relative xpath expressions
        /// </summary>
        protected CustomValidator RelativeXpathValidator;

        /// <summary>
        /// 
        /// </summary>
        protected RegularExpressionValidator NumbersMaxItemsValidator;

        /// <summary>
        /// 
        /// </summary>
        protected RegularExpressionValidator ControlHeightValidatator;

        /// <summary>
        /// 
        /// </summary>
        protected CheckBox ShowItemInfoTooltipCheckBox;

        /// <summary>
        /// 
        /// </summary>
        protected RadioButtonList StoreAsCommaDelimitedRadioButtons;

        /// <summary>
        /// 
        /// </summary>
        protected RadioButtonList XPathFilterTypeRadioButtons;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MNTP_PrevalueEditor"/> class.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        public MNTP_PrevalueEditor(umbraco.cms.businesslogic.datatype.BaseDataType dataType)
        {
            this.m_DataType = dataType;
        }

        /// <summary>
        /// Override on init to ensure child controls
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.EnsureChildControls();

            this.RegisterEmbeddedClientResource("umbraco.editorControls.PrevalueEditor.css", ClientDependencyType.Css);
        }

        /// <summary>
        /// Ensures the css to render this control is included.
        /// Binds the saved value to the drop down.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //add the css required
            //// this.AddCssMNTPClientDependencies();

            //let view state handle the rest
            if (!Page.IsPostBack)
            {
                TreeTypeDropDown.SelectedValue = SelectedTreeType;
                XPathFilterTextBox.Text = XPathFilter;
                MaxItemsTextBox.Text = MaxNodeCount.ToString();
                ShowItemInfoTooltipCheckBox.Checked = ShowToolTip;
                StoreAsCommaDelimitedRadioButtons.SelectedIndex = StoreAsCommaDelimited ? 1 : 0;
                XPathFilterTypeRadioButtons.SelectedIndex = XPathFilterMatchType == XPathFilterType.Disable ? 0 : 1;
                ShowThumbnailsForMediaCheckBox.Checked = ShowThumbnailsForMedia;

                StartNodeXPathExpressionTextBox.Text = StartNodeXPathExpression;
                StartNodeXPathExpressionTypeRadioButtons.SelectedIndex = StartNodeXPathExpressionType == XPathExpressionType.Global ? 0 : 1;
                StartNodeSelectionTypeRadioButtons.SelectedIndex = StartNodeSelectionType == NodeSelectionType.Picker ? 0 : 1;

                switch (SelectedTreeType.ToLower())
                {
                    case Constants.Applications.Content:
                        StartContentNodeIdPicker.Value = StartNodeId.ToString();
                        break;
                    case Constants.Applications.Media:
                    default:
                        StartMediaNodeIdPicker.Value = StartNodeId.ToString();
                        break;
                }

                ControlHeightTextBox.Text = ControlHeight.ToString();
                MinItemsTextBox.Text = MinNodeCount.ToString();
            }


        }

        /// <summary>
        /// Creates child controls for this control
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();


            TreeTypeDropDown = new DropDownList { ID = "TreeTypeList" };
            TreeTypeDropDown.Items.Add(new ListItem("Content", Constants.Applications.Content));
            TreeTypeDropDown.Items.Add(new ListItem("Media", Constants.Applications.Media));
            TreeTypeDropDown.AutoPostBack = true;
            AddPreValueRow(MNTPResources.Lbl_SelectTreeType, "", TreeTypeDropDown);


            StartNodeSelectionTypeRadioButtons = new RadioButtonList { ID = "NodeSelectionTypeRadioButtons" };
            StartNodeSelectionTypeRadioButtons.Items.Add(MNTPResources.Item_NodeSelectionType_Picker);
            StartNodeSelectionTypeRadioButtons.Items.Add(new ListItem(MNTPResources.Item_NodeSelectionType_XPath, MNTPResources.Item_NodeSelectionType_XPath.Replace(" ", "")));
            StartNodeSelectionTypeRadioButtons.RepeatDirection = RepeatDirection.Horizontal;
            StartNodeSelectionTypeRadioButtons.AutoPostBack = true;
            AddPreValueRow(MNTPResources.Lbl_NodeSelectionType, MNTPResources.Desc_NodeSelectionType, StartNodeSelectionTypeRadioButtons);


            StartContentNodeIdPicker = new SimpleContentPicker { ID = "StartNodeIdTextBox" };
            AddPreValueRow(MNTPResources.Lbl_StartNodeId, MNTPResources.Desc_StartNodeId, StartContentNodeIdPicker);

            StartMediaNodeIdPicker = new SimpleMediaPicker { ID = "StartMediaNodeIdPicker" };
            AddPreValueRow(MNTPResources.Lbl_StartNodeId, MNTPResources.Desc_StartNodeId, StartMediaNodeIdPicker);


            ShowThumbnailsForMediaCheckBox = new CheckBox { ID = "ShowThumbnailsForMedia" };
            AddPreValueRow(MNTPResources.Lbl_ShowThumbnails, MNTPResources.Desc_ShowThumbnails, ShowThumbnailsForMediaCheckBox);

            StartNodeXPathExpressionTypeRadioButtons = new RadioButtonList { ID = "XPathExpressionTypeRadioButtons" };
            StartNodeXPathExpressionTypeRadioButtons.Items.Add(MNTPResources.Item_XPathExpressionType_Global);
            StartNodeXPathExpressionTypeRadioButtons.Items.Add(new ListItem(MNTPResources.Item_XPathExpressionType_CurrentNode, MNTPResources.Item_XPathExpressionType_CurrentNode.Replace(" ", "")));
            StartNodeXPathExpressionTypeRadioButtons.RepeatDirection = RepeatDirection.Horizontal;
            AddPreValueRow(MNTPResources.Lbl_XPathExpressionType, MNTPResources.Desc_XPathExpressionType, StartNodeXPathExpressionTypeRadioButtons);

            StartNodeXPathExpressionTextBox = new TextBox { ID = "XPathExpressionTextBox", Width = Unit.Pixel(400) };
            RelativeXpathValidator = new CustomValidator()
                                         {
                                             ID = "RelativeXpathValidator",
                                             ControlToValidate = "XPathExpressionTextBox",
                                             ErrorMessage = MNTPResources.Val_RelativeXpath,
                                             CssClass = "validator"
                                         };
            RelativeXpathValidator.ServerValidate += new ServerValidateEventHandler(RelativeXpathValidator_ServerValidate);
            AddPreValueRow(MNTPResources.Lbl_XPathExpression, MNTPResources.Desc_XPathExpression, StartNodeXPathExpressionTextBox, RelativeXpathValidator);

            XPathFilterTypeRadioButtons = new RadioButtonList { ID = "XPathMatchTypeRadioButtons" };
            XPathFilterTypeRadioButtons.Items.Add(MNTPResources.Item_XPathMatchType_Disable);
            XPathFilterTypeRadioButtons.Items.Add(MNTPResources.Item_XPathMatchType_Enable);
            XPathFilterTypeRadioButtons.RepeatDirection = RepeatDirection.Horizontal;
            AddPreValueRow(MNTPResources.Lbl_XPathFilterType, MNTPResources.Desc_XPathFilterType, XPathFilterTypeRadioButtons);

            XPathFilterTextBox = new TextBox { ID = "XPathFilter", Width = Unit.Pixel(400) };
            AddPreValueRow(MNTPResources.Lbl_XPathFilter, MNTPResources.Desc_XPathFilter, XPathFilterTextBox);

            MaxItemsTextBox = new TextBox { ID = "MaxItemsCount", Width = Unit.Pixel(50) };
            NumbersMaxItemsValidator = new RegularExpressionValidator
                {
                    ID = "NumbersMaxItemsValidator",
                    ControlToValidate = "MaxItemsCount",
                    CssClass = "validator",
                    ErrorMessage = MNTPResources.Val_MaxItemsMsg,
                    ValidationExpression = @"^-{0,1}\d*\.{0,1}\d+$"
                };
            AddPreValueRow(MNTPResources.Lbl_MaxItemsAllowed, MNTPResources.Desc_MaxItemsAllowed, MaxItemsTextBox, NumbersMaxItemsValidator);

            MinItemsTextBox = new TextBox { ID = "MinItemsCount", Width = Unit.Pixel(50) };
            NumbersMinItemsValidator = new RegularExpressionValidator
            {
                ID = "NumbersMinItemsValidator",
                ControlToValidate = "MinItemsCount",
                CssClass = "validator",
                ErrorMessage = MNTPResources.Val_MinItemsMsg,
                ValidationExpression = @"^\d{1,3}$"
            };
            AddPreValueRow(MNTPResources.Lbl_MinItemsAllowed, MNTPResources.Desc_MinItemsAllowed, MinItemsTextBox, NumbersMinItemsValidator);

            ShowItemInfoTooltipCheckBox = new CheckBox { ID = "ShowItemInfoTooltipCheckBox" };
            AddPreValueRow(MNTPResources.Lbl_ShowItemInfoTooltipCheckBox, MNTPResources.Desc_ShowTooltips, ShowItemInfoTooltipCheckBox);

            StoreAsCommaDelimitedRadioButtons = new RadioButtonList { ID = "StoreAsCommaDelimitedRadioButtons" };
            StoreAsCommaDelimitedRadioButtons.Items.Add("XML");
            StoreAsCommaDelimitedRadioButtons.Items.Add("CSV");
            StoreAsCommaDelimitedRadioButtons.RepeatDirection = RepeatDirection.Horizontal;
            AddPreValueRow(MNTPResources.Lbl_StoreAsComma, MNTPResources.Desc_StoreAsComma, StoreAsCommaDelimitedRadioButtons);

            ControlHeightTextBox = new TextBox() { ID = "ControlHeightTextBox", Width = Unit.Pixel(50) };
            ControlHeightValidatator = new RegularExpressionValidator
                {
                    ID = "ControlHeightValidator",
                    ControlToValidate = "ControlHeightTextBox",
                    CssClass = "validator",
                    ErrorMessage = MNTPResources.Val_ControlHeightMsg,
                    ValidationExpression = @"^\d{1,3}$"
                };
            AddPreValueRow(MNTPResources.Lbl_ControlHeight, "", ControlHeightTextBox, ControlHeightValidatator);
        }

        void RelativeXpathValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = true;
            if (TreeTypeDropDown.SelectedIndex == 0 && StartNodeXPathExpressionTypeRadioButtons.SelectedIndex == 1 && StartNodeXPathExpressionTextBox.Text.StartsWith("/"))
            {
                args.IsValid = false;
            }
        }

        /// <summary>
        /// Helper method to add a server side pre value row
        /// </summary>
        /// <param name="lbl"></param>
        /// <param name="description"></param>
        /// <param name="ctl"></param>
        /// <remarks>
        /// Using server side syntax because of the post backs and because i don't want to manage the view state manually
        /// </remarks>
        private void AddPreValueRow(string lbl, string description, params Control[] ctl)
        {
            var div = new HtmlGenericControl("div");
            div.Attributes.Add("class", "row clearfix");
            var label = new HtmlGenericControl("div");
            label.Attributes.Add("class", "label");
            var span = new HtmlGenericControl("span");
            span.InnerText = lbl;
            label.Controls.Add(span);
            var field = new HtmlGenericControl("div");
            field.Attributes.Add("class", "field");
            foreach (var c in ctl)
            {
                field.Controls.Add(c);
            }
            div.Controls.Add(label);
            div.Controls.Add(field);

            if (!string.IsNullOrEmpty(description))
            {
                var desc = new HtmlGenericControl("div");
                var descSpan = new HtmlGenericControl("span");
                descSpan.InnerHtml = description;
                desc.Attributes.Add("class", "description");
                desc.Controls.Add(descSpan);
                div.Controls.Add(desc);
            }

            this.Controls.Add(div);
        }

        /// <summary>
        /// Hides/Shows controls based on the selection of other controls
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //we can only show the node selection type based on content
            if (TreeTypeDropDown.SelectedIndex == 0)
            {
                this.StartNodeSelectionTypeRadioButtons.Parent.Parent.Visible = true;
            }
            else
            {
                this.StartNodeSelectionTypeRadioButtons.Parent.Parent.Visible = false;
            }

            //if media is selected, or the node type selection is a picker type
            if (TreeTypeDropDown.SelectedIndex == 1 || StartNodeSelectionTypeRadioButtons.SelectedIndex == 0)
            {
                StartNodeXPathExpressionTypeRadioButtons.Parent.Parent.Visible = false;
                StartNodeXPathExpressionTextBox.Parent.Parent.Visible = false;

                switch (TreeTypeDropDown.SelectedIndex)
                {
                    case 0:
                        //content selected
                        StartContentNodeIdPicker.Parent.Parent.Visible = true;

                        StartMediaNodeIdPicker.Parent.Parent.Visible = false;
                        ShowThumbnailsForMediaCheckBox.Parent.Parent.Visible = false;
                        break;
                    case 1:
                    default:
                        //media selected:
                        StartContentNodeIdPicker.Parent.Parent.Visible = false;

                        StartMediaNodeIdPicker.Parent.Parent.Visible = true;
                        ShowThumbnailsForMediaCheckBox.Parent.Parent.Visible = true;
                        break;
                }

            }
            else
            {
                //since it's an xpath expression, not node picker, hide all node pickers
                StartContentNodeIdPicker.Parent.Parent.Visible = false;
                StartMediaNodeIdPicker.Parent.Parent.Visible = false;
                ShowThumbnailsForMediaCheckBox.Parent.Parent.Visible = false;

                StartNodeXPathExpressionTypeRadioButtons.Parent.Parent.Visible = true;
                StartNodeXPathExpressionTextBox.Parent.Parent.Visible = true;
            }
        }

        /// <summary>
        /// render our own custom markup
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "PrevalueEditor");
            writer.RenderBeginTag(HtmlTextWriterTag.Div); //start 'PrevalueEditor'

            base.Render(writer);

            writer.RenderEndTag(); //end 'PrevalueEditor'

        }

        /// <summary>
        /// Lazy loads the prevalues for this data type
        /// </summary>
        /// <returns></returns>
        private SortedList GetPreValues()
        {
            if (m_PreValues == null)
            {
                m_PreValues = PreValues.GetPreValues(m_DataType.DataTypeDefinitionId);
            }
            return m_PreValues;
        }

        #region IDataPrevalue Members

        ///<summary>
        /// returns this as it's own editor
        ///</summary>
        public Control Editor
        {
            get { return this; }
        }

        /// <summary>
        /// Saves data to Umbraco
        /// </summary>
        public void Save()
        {
            if (!Page.IsValid) { return; }

            //it will always be text since people may save a huge amount of selected nodes and serializing to xml could be large.
            m_DataType.DBType = umbraco.cms.businesslogic.datatype.DBTypes.Ntext;

            //need to lock this operation since multiple inserts are happening and if 2 threads reach here at the same time, there 
            //could be issues.
            lock (Locker)
            {
                var vals = GetPreValues();

                //store the tree type
                SavePreValue(PropertyIndex.TreeType, TreeTypeDropDown.SelectedValue, vals);

                //store the xpath
                SavePreValue(PropertyIndex.XPathFilter, XPathFilterTextBox.Text, vals);

                //store the max node count
                SavePreValue(PropertyIndex.MaxNodeCount, string.IsNullOrEmpty(MaxItemsTextBox.Text) ? "-1" : MaxItemsTextBox.Text, vals);

                //store the 'show tooltips'
                SavePreValue(PropertyIndex.ShowToolTip, ShowItemInfoTooltipCheckBox.Checked.ToString(), vals);

                //store the 'as comma'
                SavePreValue(PropertyIndex.StoreAsCommaDelimited, StoreAsCommaDelimitedRadioButtons.SelectedIndex.ToString(), vals);

                //the xpath filter type
                SavePreValue(PropertyIndex.XPathFilterType, XPathFilterTypeRadioButtons.SelectedIndex.ToString(), vals);

                //based on the media type selected, we need to get the correct start node id
                //from the correct control.
                var startNodeId = -1;
                switch (TreeTypeDropDown.SelectedIndex)
                {
                    case 0:
                        int.TryParse(StartContentNodeIdPicker.Value, out startNodeId);
                        break;
                    case 1:
                    default:
                        int.TryParse(StartMediaNodeIdPicker.Value, out startNodeId);
                        break;
                }

                //store the start node id
                SavePreValue(PropertyIndex.StartNodeId, startNodeId.ToString(), vals);

                //store the 'show thumbnails'
                SavePreValue(PropertyIndex.ShowThumbnails, ShowThumbnailsForMediaCheckBox.Checked.ToString(), vals);

                //store the 'node selection type'
                SavePreValue(PropertyIndex.StartNodeSelectionType, StartNodeSelectionTypeRadioButtons.SelectedIndex.ToString(), vals);

                //store the 'xpath expression type'
                SavePreValue(PropertyIndex.StartNodeXPathExpressionType, StartNodeXPathExpressionTypeRadioButtons.SelectedIndex.ToString(), vals);

                //save the 'xpath expression'
                SavePreValue(PropertyIndex.StartNodeXPathExpression, StartNodeXPathExpressionTextBox.Text, vals);

                //save the control height
                SavePreValue(PropertyIndex.ControlHeight, ControlHeightTextBox.Text, vals);

                //save the min amount
                SavePreValue(PropertyIndex.MinNodeCount, MinItemsTextBox.Text, vals);
            }

            //once everything is saved, clear the cookie vals
            MNTP_DataType.ClearCookiePersistence();

        }

        /// <summary>
        /// Used to determine the index number of where the property is saved in the pre values repository
        /// </summary>
        private enum PropertyIndex
        {
            TreeType,
            XPathFilter,
            MaxNodeCount,
            ShowToolTip,
            StoreAsCommaDelimited,
            XPathFilterType,
            StartNodeId,
            ShowThumbnails,
            StartNodeSelectionType,
            StartNodeXPathExpressionType,
            StartNodeXPathExpression,
            ControlHeight,
            MinNodeCount
        }

        /// <summary>
        /// Helper method to save/create pre value values in the db
        /// </summary>
        /// <param name="propIndex"></param>
        /// <param name="value"></param>
        /// <param name="currentVals"></param>
        private void SavePreValue(PropertyIndex propIndex, string value, SortedList currentVals)
        {
            var index = (int)propIndex;
            if (currentVals.Count >= ((int)propIndex + 1))
            {
                //update
                ((PreValue)currentVals[index]).Value = value;
                ((PreValue)currentVals[index]).Save();
            }
            else
            {
                //insert
                PreValue.MakeNew(m_DataType.DataTypeDefinitionId, value);
            }
        }

        ///<summary>
        /// Generic method to return a strongly typed object from the pre value bucket
        ///</summary>
        ///<param name="index"></param>
        ///<param name="output"></param>
        ///<param name="defaultVal"></param>
        ///<typeparam name="T"></typeparam>
        ///<returns></returns>
        private T GetPreValue<T>(PropertyIndex index, Func<PreValue, T> output, T defaultVal)
        {
            var vals = GetPreValues();
            return vals.Count >= (int)index + 1 ? output((PreValue)vals[(int)index]) : defaultVal;
        }

        #endregion
    }
}
