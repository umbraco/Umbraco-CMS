using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;

using umbraco.BusinessLogic;
using umbraco.DataLayer;

namespace umbraco.editorControls.tinymce
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class tinyMCEPreValueConfigurator : System.Web.UI.WebControls.PlaceHolder, interfaces.IDataPrevalue
    {
		// UI controls
		private CheckBoxList _editorButtons;
        private CheckBox _enableRightClick;
        private DropDownList _dropdownlist;
        private CheckBoxList _advancedUsersList;
        private CheckBoxList _stylesheetList;
        private TextBox _width = new TextBox();
        private TextBox _height = new TextBox();
        private TextBox _maxImageWidth = new TextBox();
        private CheckBox _fullWidth = new CheckBox();
        private CheckBox _showLabel = new CheckBox();
        private RegularExpressionValidator _widthValidator = new RegularExpressionValidator();
        private RegularExpressionValidator _heightValidator = new RegularExpressionValidator();
        private RegularExpressionValidator _maxImageWidthValidator = new RegularExpressionValidator();
				
		// referenced datatype
		private cms.businesslogic.datatype.BaseDataType _datatype;
        private string _selectedButtons = "";
        private string _advancedUsers = "";
        private string _stylesheets = "";

        public static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        public tinyMCEPreValueConfigurator(cms.businesslogic.datatype.BaseDataType DataType) 
		{
			// state it knows its datatypedefinitionid
			_datatype = DataType;
			setupChildControls();
		}
		
		private void setupChildControls() 
		{
            _dropdownlist = new DropDownList();
            _dropdownlist.ID = "dbtype";
            _dropdownlist.Items.Add(DBTypes.Date.ToString());
            _dropdownlist.Items.Add(DBTypes.Integer.ToString());
            _dropdownlist.Items.Add(DBTypes.Ntext.ToString());
            _dropdownlist.Items.Add(DBTypes.Nvarchar.ToString());

            _editorButtons = new CheckBoxList();
            _editorButtons.ID = "editorButtons";
            _editorButtons.RepeatColumns = 4;
            _editorButtons.CellPadding = 3;

            _enableRightClick = new CheckBox();
		    _enableRightClick.ID = "enableRightClick";

            _advancedUsersList = new CheckBoxList();
            _advancedUsersList.ID = "advancedUsersList";

            _stylesheetList = new CheckBoxList();
            _stylesheetList.ID = "stylesheetList";

            _showLabel = new CheckBox();
            _showLabel.ID = "showLabel";

            _maxImageWidth = new TextBox();
            _maxImageWidth.ID = "maxImageWidth";

            // put the childcontrols in context - ensuring that
			// the viewstate is persisted etc.
            Controls.Add(_dropdownlist);
            Controls.Add(_enableRightClick);
            Controls.Add(_editorButtons);
            Controls.Add(_advancedUsersList);
            Controls.Add(_stylesheetList);
            Controls.Add(_width);
		    Controls.Add(_widthValidator);
            Controls.Add(_height);
            Controls.Add(_heightValidator);
            Controls.Add(_showLabel);
            Controls.Add(_maxImageWidth);
            Controls.Add(_maxImageWidthValidator);
            //            Controls.Add(_fullWidth);

		}
		
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);
            // add ids to controls
            _width.ID = "width";
            _height.ID = "height";


            // initialize validators
            _widthValidator.ValidationExpression = "0*[1-9][0-9]*";
            _widthValidator.ErrorMessage = ui.Text("errorHandling", "errorIntegerWithoutTab", ui.Text("width"), new BasePages.BasePage().getUser());
		    _widthValidator.Display = ValidatorDisplay.Dynamic;
            _widthValidator.ControlToValidate = _width.ID;
            _heightValidator.ValidationExpression = "0*[1-9][0-9]*";
            _heightValidator.ErrorMessage = ui.Text("errorHandling", "errorIntegerWithoutTab", ui.Text("height"), new BasePages.BasePage().getUser());
            _heightValidator.ControlToValidate = _height.ID;
            _heightValidator.Display = ValidatorDisplay.Dynamic;
            _maxImageWidthValidator.ValidationExpression = "0*[1-9][0-9]*";
            _maxImageWidthValidator.ErrorMessage = ui.Text("errorHandling", "errorIntegerWithoutTab","'" +  ui.Text("rteMaximumDefaultImgSize") + "'", new BasePages.BasePage().getUser());
            _maxImageWidthValidator.ControlToValidate = _maxImageWidth.ID;
            _maxImageWidthValidator.Display = ValidatorDisplay.Dynamic;
            
            if (!Page.IsPostBack)
			{
                if (Configuration != null)
                {
                    string[] config = Configuration.Split("|".ToCharArray());
                    if (config.Length > 0)
                    {
                        _selectedButtons = config[0];

                        if (config.Length > 1)
                            if (config[1] == "1")
                                _enableRightClick.Checked = true;

                        if (config.Length > 2)
                            _advancedUsers = config[2];

                        if (config.Length > 4 && config[4].Split(',').Length > 1)
                        {
//                        if (config[3] == "1")
//                            _fullWidth.Checked = true;
//                        else
//                        {
                            _width.Text = config[4].Split(',')[0];
                            _height.Text = config[4].Split(',')[1];
//                        }
                        }

                        // if width and height are empty or lower than 0 then set default sizes:
                        int tempWidth, tempHeight;
                        int.TryParse(_width.Text, out tempWidth);
                        int.TryParse(_height.Text, out tempHeight);
                        if (_width.Text.Trim() == "" || tempWidth < 1)
                            _width.Text = "500";
                        if (_height.Text.Trim() == "" || tempHeight < 1)
                            _height.Text = "400";

                        if (config.Length > 5)
                            _stylesheets = config[5];
                        if (config.Length > 6 && config[6] != "")
                            _showLabel.Checked = bool.Parse(config[6]);

                        if (config.Length > 7 && config[7] != "")
                            _maxImageWidth.Text = config[7];
                        else
                            _maxImageWidth.Text = "500";
                    }

                    // add editor buttons
                    IDictionaryEnumerator ide = tinyMCEConfiguration.SortedCommands.GetEnumerator();
                    while (ide.MoveNext())
                    {
                        tinyMCECommand cmd = (tinyMCECommand) ide.Value;
                        ListItem li =
                            new ListItem(
                                string.Format("<img src=\"{0}\" class=\"tinymceIcon\" alt=\"{1}\" />&nbsp;", cmd.Icon,
                                              cmd.Alias), cmd.Alias);
                        if (_selectedButtons.IndexOf(cmd.Alias) > -1)
                            li.Selected = true;

                        _editorButtons.Items.Add(li);
                    }

                    // add users
                    foreach (BusinessLogic.UserType ut in BusinessLogic.UserType.getAll)
                    {
                        ListItem li = new ListItem(ut.Name, ut.Id.ToString());
                        if (("," + _advancedUsers + ",").IndexOf("," + ut.Id.ToString() + ",") > -1)
                            li.Selected = true;

                        _advancedUsersList.Items.Add(li);
                    }

                    // add stylesheets
                    foreach (cms.businesslogic.web.StyleSheet st in cms.businesslogic.web.StyleSheet.GetAll())
                    {
                        ListItem li = new ListItem(st.Text, st.Id.ToString());
                        if (("," + _stylesheets + ",").IndexOf("," + st.Id.ToString() + ",") > -1)
                            li.Selected = true;

                        _stylesheetList.Items.Add(li);
                    }
                }

			    // Mark the current db type
                _dropdownlist.SelectedValue = _datatype.DBType.ToString();
                    
            }
		}
		
		public Control Editor 
		{
			get
			{
				return this;
			}
		}

		public virtual void Save() 
		{
            _datatype.DBType = (cms.businesslogic.datatype.DBTypes)Enum.Parse(typeof(cms.businesslogic.datatype.DBTypes), _dropdownlist.SelectedValue, true);

			// Generate data-string
            string data = ",";

            foreach (ListItem li in _editorButtons.Items)
                if (li.Selected)
                    data += li.Value + ",";

            data += "|";

            if (_enableRightClick.Checked)
                data += "1";
            else
                data += "0";

            data += "|";

            foreach (ListItem li in _advancedUsersList.Items)
                if (li.Selected)
                    data += li.Value + ",";

            data += "|";

            
		    data += "0|";
            /*
            if (_fullWidth.Checked)
                data += "1|";
            else
                data += "0|";
            */

            data += _width.Text + "," + _height.Text + "|";

            foreach (ListItem li in _stylesheetList.Items)
                if (li.Selected)
                    data += li.Value + ",";
		    data += "|";
            data += _showLabel.Checked.ToString() + "|";
            data += _maxImageWidth.Text + "|";


            // If the add new prevalue textbox is filled out - add the value to the collection.
			IParameter[] SqlParams = new IParameter[] {
										    SqlHelper.CreateParameter("@value",data),
											SqlHelper.CreateParameter("@dtdefid",_datatype.DataTypeDefinitionId)};
			SqlHelper.ExecuteNonQuery("delete from cmsDataTypePreValues where datatypenodeid = @dtdefid",SqlParams);
            // we need to populate the parameters again due to an issue with SQL CE
            SqlParams = new IParameter[] {
										    SqlHelper.CreateParameter("@value",data),
											SqlHelper.CreateParameter("@dtdefid",_datatype.DataTypeDefinitionId)};
			SqlHelper.ExecuteNonQuery("insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')",SqlParams);
		}

		protected override void Render(HtmlTextWriter writer)
		{
			writer.WriteLine("<table>");
            writer.WriteLine("<tr><th>" + ui.Text("editdatatype", "dataBaseDatatype") + ":</th><td>");
			_dropdownlist.RenderControl(writer);
			writer.Write("</td></tr>");
            writer.Write("<tr><th>" + ui.Text("editdatatype", "rteButtons") + ":</th><td>");
            _editorButtons.RenderControl(writer);
            writer.Write("</td></tr>");
            writer.Write("<tr><th>" + ui.Text("editdatatype", "rteRelatedStylesheets") + ":</th><td>");
            _stylesheetList.RenderControl(writer);
            writer.Write("</td></tr>");
            writer.Write("<tr><th>" + ui.Text("editdatatype", "rteEnableContextMenu") + ":</th><td>");
            _enableRightClick.RenderControl(writer);
            writer.Write("</td></tr>");
            writer.Write("<tr><th>" + ui.Text("editdatatype", "rteEnableAdvancedSettings") + ":</th><td>");
            _advancedUsersList.RenderControl(writer);
            writer.Write("</td></tr>");
            writer.Write("<tr><th>"); 
            //"Size:</th><td>Maximum width and height: ");
            //            _fullWidth.RenderControl(writer);

            writer.Write(ui.Text("editdatatype", "rteWidthAndHeight") + ":</th><td>");
            _width.RenderControl(writer);
            _widthValidator.RenderControl(writer);
            writer.Write(" x ");
            _height.RenderControl(writer);
            _heightValidator.RenderControl(writer);
            writer.Write("</td></tr>");
            writer.Write("<tr><th>");
            writer.Write(ui.Text("editdatatype", "rteMaximumDefaultImgSize") + ":</th><td>");
            _maxImageWidth.RenderControl(writer);
            _maxImageWidthValidator.RenderControl(writer);
            writer.Write("</td></tr>");
            writer.Write("<tr><th>" + ui.Text("editdatatype", "rteShowLabel") + ":</th><td>");
            _showLabel.RenderControl(writer);
            writer.Write("</td></tr>");
            writer.Write("</table>");
        }

		public string Configuration 
		{
			get 
			{
                try
                {
                    return SqlHelper.ExecuteScalar<string>("select value from cmsDataTypePreValues where datatypenodeid = @datatypenodeid", SqlHelper.CreateParameter("@datatypenodeid", _datatype.DataTypeDefinitionId));
                }
                catch
                {
                    return "";
                }
			}
		}

	}
}
