using System;
using System.Collections;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.presentation.Trees;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace umbraco.cms.presentation.developer
{
    public partial class editDatatype : BasePages.UmbracoEnsuredPage
    {
        public editDatatype()
        {
            CurrentApp = BusinessLogic.DefaultApps.developer.ToString();

        }
        protected ImageButton save;
        private cms.businesslogic.datatype.DataTypeDefinition dt;
        cms.businesslogic.datatype.controls.Factory f;
        private int _id = 0;
        private interfaces.IDataPrevalue _prevalue;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            pp_name.Text = ui.Text("name");
            pp_renderControl.Text = ui.Text("renderControl");
            pane_settings.Text = ui.Text("settings");
            pp_guid.Text = ui.Text("guid");


            _id = int.Parse(Request.QueryString["id"]);
            dt = cms.businesslogic.datatype.DataTypeDefinition.GetDataTypeDefinition(_id);


            f = new cms.businesslogic.datatype.controls.Factory();

            if (!IsPostBack)
            {
                txtName.Text = dt.Text;

                SortedList datatypes = new SortedList();

                foreach (interfaces.IDataType df in f.GetAll())
                    datatypes.Add(df.DataTypeName + "|" + Guid.NewGuid().ToString(), df.Id);

                IDictionaryEnumerator ide = datatypes.GetEnumerator();

                string datatTypeId = dt.DataType != null ? dt.DataType.Id.ToString() : String.Empty;
                while (ide.MoveNext())
                {
                    ListItem li = new ListItem();
                    li.Text = ide.Key.ToString().Substring(0, ide.Key.ToString().IndexOf("|"));
                    li.Value = ide.Value.ToString();

                    //SJ Fixes U4-2488 Edit datatype: Media Picker appears incorrectly
                    //Apparently in some installs the media picker rendercontrol is installed twice with 
                    //the exact same ID so we need to check for duplicates
                    if (ddlRenderControl.Items.Contains(li))
                        continue;

                    if (!String.IsNullOrEmpty(datatTypeId) && li.Value.ToString() == datatTypeId) li.Selected = true;
                    ddlRenderControl.Items.Add(li);
                }

                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadDataTypes>().Tree.Alias)
                    .SyncTree("-1,init," + _id, false);

            }

            if (dt.DataType != null)
                litGuid.Text = dt.DataType.Id.ToString();
            Panel1.Text = ui.Text("edit") + " datatype: " + dt.Text;
            InsertPrevalueEditor();
        }

        private void InsertPrevalueEditor()
        {
            try
            {
                if (ddlRenderControl.SelectedIndex >= 0)
                {
                    interfaces.IDataType o = f.DataType(new Guid(ddlRenderControl.SelectedValue));

                    o.DataTypeDefinitionId = dt.Id;
                    _prevalue = o.PrevalueEditor;

                    if (o.PrevalueEditor.Editor != null)
                        plcEditorPrevalueControl.Controls.Add(o.PrevalueEditor.Editor);
                }
                else
                {
                    plcEditorPrevalueControl.Controls.Add(new LiteralControl("No editor control selected"));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<editDatatype>("An error occurred inserting pre-value editor", ex);
            }

        }

        protected void save_click(object sender, ImageClickEventArgs e)
        {
            // save prevalues;
            if (_prevalue != null)
                _prevalue.Save();

            dt.Text = txtName.Text;

            dt.DataType = f.DataType(new Guid(ddlRenderControl.SelectedValue));

            try
            {
                dt.Save();
                ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "dataTypeSaved", null), "");
                ClientTools.SyncTree(dt.Path, true);
            }
            catch (DuplicateNameException)
            {
                DuplicateNameValidator.IsValid = false;
                ClientTools.ShowSpeechBubble(speechBubbleIcon.error, "A data type with the name " + dt.Text + " already exists", "");
            }            
        }

        override protected void OnInit(EventArgs e)
        {
            save = Panel1.Menu.NewImageButton();
            save.ID = "save";
            save.Click += save_click;
            save.ImageUrl = SystemDirectories.Umbraco + "/images/editor/save.gif";

            Panel1.hasMenu = true;

            base.OnInit(e);
        }

        protected CustomValidator DuplicateNameValidator;

        /// <summary>
        /// Panel1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.UmbracoPanel Panel1;

        /// <summary>
        /// pane_control control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane pane_control;

        /// <summary>
        /// pp_name control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_name;

        /// <summary>
        /// txtName control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox txtName;

        /// <summary>
        /// pp_renderControl control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_renderControl;

        /// <summary>
        /// ddlRenderControl control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.DropDownList ddlRenderControl;

        /// <summary>
        /// pp_guid control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_guid;

        /// <summary>
        /// litGuid control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal litGuid;

        /// <summary>
        /// pane_settings control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane pane_settings;

        /// <summary>
        /// plcEditorPrevalueControl control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder plcEditorPrevalueControl;
    }
}
