using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;

using umbraco.DataLayer;
using umbraco.BusinessLogic;

using umbraco.editorControls;

namespace umbraco.editorControls.ultimatepicker
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class ultimatePickerPrevalueEditor : System.Web.UI.WebControls.PlaceHolder, umbraco.interfaces.IDataPrevalue
    {
        public ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        #region IDataPrevalue Members

        // referenced datatype
        private umbraco.cms.businesslogic.datatype.BaseDataType _datatype;

        private DropDownList _dropdownlist;
        private DropDownList _dropdownlistType;
        private TextBox _textboxParentNode;
        private TextBox _textboxDocumentTypeFilter;
        private CheckBox _checkboxShowGrandChildren;

        public ultimatePickerPrevalueEditor(umbraco.cms.businesslogic.datatype.BaseDataType DataType)
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

            _dropdownlistType = new DropDownList();
            _dropdownlistType.ID = "type";
            _dropdownlistType.Items.Add("AutoComplete");
            _dropdownlistType.Items.Add("CheckBoxList");
            _dropdownlistType.Items.Add("DropDownList");
            _dropdownlistType.Items.Add("ListBox");
            _dropdownlistType.Items.Add("RadioButtonList");

            _textboxParentNode = new TextBox();
            _textboxParentNode.ID = "parentnode";
            _textboxParentNode.CssClass = "umbEditorTextField";

            _textboxDocumentTypeFilter = new TextBox();
            _textboxDocumentTypeFilter.ID = "documentTypeFilter";
            _textboxDocumentTypeFilter.CssClass = "umbEditorTextField";

            _checkboxShowGrandChildren = new CheckBox();
            _checkboxShowGrandChildren.ID = "showgrandchildren";

            // put the childcontrols in context - ensuring that
            // the viewstate is persisted etc.
            Controls.Add(_dropdownlist);
            Controls.Add(_dropdownlistType);
            Controls.Add(_textboxParentNode);
            Controls.Add(_textboxDocumentTypeFilter);
            Controls.Add(_checkboxShowGrandChildren);
        }



        public Control Editor
        {
            get
            {
                return this;
            }
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                string[] config = Configuration.Split("|".ToCharArray());
                if (config.Length > 1)
                {
                    _dropdownlistType.SelectedValue = config[0];
                    _textboxParentNode.Text = config[1];
                    _textboxDocumentTypeFilter.Text = config[2];
                    _checkboxShowGrandChildren.Checked = Convert.ToBoolean(config[3]);

                }
                _dropdownlist.SelectedValue = _datatype.DBType.ToString();

            }


        }

        public void Save()
        {
            _datatype.DBType = (umbraco.cms.businesslogic.datatype.DBTypes)Enum.Parse(typeof(umbraco.cms.businesslogic.datatype.DBTypes), _dropdownlist.SelectedValue, true);


            string validatedFilter = validateFilterInput(_textboxDocumentTypeFilter.Text);

            // Generate data-string
            string data = _dropdownlistType.SelectedValue + "|" + _textboxParentNode.Text + "|" + validatedFilter + "|" + _checkboxShowGrandChildren.Checked.ToString();

            // If the add new prevalue textbox is filled out - add the value to the collection.
            IParameter[] SqlParams = new IParameter[] {
			            SqlHelper.CreateParameter("@value",data),
						SqlHelper.CreateParameter("@dtdefid",_datatype.DataTypeDefinitionId)};
            SqlHelper.ExecuteNonQuery("delete from cmsDataTypePreValues where datatypenodeid = @dtdefid", SqlParams);
            // need to unlock the parameters (for SQL CE compat)
            SqlParams = new IParameter[] {
										SqlHelper.CreateParameter("@value",data),
										SqlHelper.CreateParameter("@dtdefid",_datatype.DataTypeDefinitionId)};
            SqlHelper.ExecuteNonQuery("insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')", SqlParams);


        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.WriteLine("<table>");
            writer.WriteLine("<tr><th>Database datatype:</th><td>");
            _dropdownlist.RenderControl(writer);
            writer.Write("</td></tr>");
            writer.Write("<tr><th>Type:</th><td>");
            _dropdownlistType.RenderControl(writer);
            writer.Write("</td></tr>");
            writer.Write("<tr><th>Parent nodeid:</th><td>");
            _textboxParentNode.RenderControl(writer);
            writer.Write("</td></tr>");
            writer.Write("<tr><th>Document Alias filter (comma-separated):</th><td>");
            _textboxDocumentTypeFilter.RenderControl(writer);
            writer.Write("</td></tr>");
            writer.Write("<tr><th>Show grandchildren:</th><td>");
            _checkboxShowGrandChildren.RenderControl(writer);
            writer.Write("</td></tr>");
            writer.Write("</table>");
        }

        public string Configuration
        {
            get
            {
                object conf =
                    SqlHelper.ExecuteScalar<object>("select value from cmsDataTypePreValues where datatypenodeid = @datatypenodeid",
                                            SqlHelper.CreateParameter("@datatypenodeid", _datatype.DataTypeDefinitionId));
                if (conf != null)
                    return conf.ToString();
                else
                    return "";

            }
        }

        #endregion

        /// <summary>
        /// Validates and clears the filter input from errorneous entries
        /// </summary>
        /// <param name="filterInput">The filter string to be validated</param>
        /// <returns>A validated filtered comma separated string</returns>
        public string validateFilterInput(string filterInput)
        {
            string[] filters = filterInput.Split(",".ToCharArray());
            string validatedFilter = string.Empty;
            List<string> validatedFilters = new List<string>();

            foreach (string filter in filters)
            {
                string trimmedFilter = filter.TrimStart(" ".ToCharArray());
                trimmedFilter = trimmedFilter.TrimEnd(" ".ToCharArray());

                if (trimmedFilter != string.Empty)
                {
                    validatedFilters.Add(trimmedFilter);
                }
            }

            for (int i = 0; i < validatedFilters.Count; i++)
            {
                if (i > 0)
                    validatedFilter += ",";
                validatedFilter += validatedFilters[i];
            }
            return validatedFilter;
        }
    }
}
