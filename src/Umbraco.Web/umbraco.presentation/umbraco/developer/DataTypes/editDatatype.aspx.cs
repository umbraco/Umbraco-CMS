using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using umbraco.cms.presentation.Trees;
using umbraco.interfaces;

namespace umbraco.cms.presentation.developer
{
    public partial class editDatatype : BasePages.UmbracoEnsuredPage
    {
        public editDatatype()
        {
            CurrentApp = BusinessLogic.DefaultApps.developer.ToString();

        }
        protected ImageButton save;
        
        private int _id = 0;
        private IDataPrevalue _prevalue;
        private IDataTypeDefinition _dataTypeDefinition;

        protected void Page_Load(object sender, EventArgs e)
        {
            pp_name.Text = ui.Text("name");
            pp_renderControl.Text = ui.Text("renderControl");
            pane_settings.Text = ui.Text("settings");
            pp_guid.Text = ui.Text("guid");

            _id = int.Parse(Request.QueryString["id"]);

            _dataTypeDefinition = ApplicationContext.Services.DataTypeService.GetDataTypeDefinitionById(_id);
            
            if (IsPostBack == false)
            {
                txtName.Text = _dataTypeDefinition.Name;

                //get the legacy data types
                var datatypes = DataTypesResolver.Current.DataTypes
                    .ToDictionary(df => df.Id, df => "(legacy) " + df.DataTypeName);

                //get the new property editors
                var propEditors = PropertyEditorResolver.Current.PropertyEditors
                    .ToDictionary(pe => pe.Id, pe => pe.Name);

                //join the lists
                var combined = propEditors.Concat(datatypes);
                
                foreach (var item in combined)
                {
                    var li = new ListItem
                        {

                            Text = item.Value,
                            Value = item.Key.ToString()
                        };

                    //SJ Fixes U4-2488 Edit datatype: Media Picker appears incorrectly
                    //Apparently in some installs the media picker rendercontrol is installed twice with 
                    //the exact same ID so we need to check for duplicates
                    if (ddlRenderControl.Items.Contains(li))
                        continue; 
                        
                    if (_dataTypeDefinition.ControlId != default(Guid) && li.Value == _dataTypeDefinition.ControlId.ToString())
                    {
                        li.Selected = true;
                    }

                    ddlRenderControl.Items.Add(li);
                }     

                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadDataTypes>().Tree.Alias)
                    .SyncTree("-1,init," + _id.ToString(CultureInfo.InvariantCulture), false);

            }

            if (_dataTypeDefinition.ControlId != default(Guid))
            {
                litGuid.Text = _dataTypeDefinition.ControlId.ToString();
            }

            Panel1.Text = ui.Text("edit") + " datatype: " + _dataTypeDefinition.Name;
            InsertPrevalueEditor();
        }

        private void InsertPrevalueEditor()
        {
            try
            {
                if (ddlRenderControl.SelectedIndex >= 0)
                {
                    var o = DataTypesResolver.Current.GetById(new Guid(ddlRenderControl.SelectedValue));

                    o.DataTypeDefinitionId = _dataTypeDefinition.Id;
                    _prevalue = o.PrevalueEditor;

                    if (o.PrevalueEditor.Editor != null)
                        plcEditorPrevalueControl.Controls.Add(o.PrevalueEditor.Editor);
                }
                else
                {
                    plcEditorPrevalueControl.Controls.Add(new LiteralControl("No editor control selected"));
                }
            }
            catch { }

        }

        protected void save_click(object sender, ImageClickEventArgs e)
        {
            // save prevalues;
            if (_prevalue != null)
                _prevalue.Save();

            _dataTypeDefinition.ControlId = new Guid(ddlRenderControl.SelectedValue);
            _dataTypeDefinition.Name = txtName.Text;

            ApplicationContext.Services.DataTypeService.Save(_dataTypeDefinition, UmbracoUser.Id);

            ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "dataTypeSaved"), "");
            ClientTools.SyncTree("-1,init," + _id.ToString(CultureInfo.InvariantCulture), true);
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
    }
}
