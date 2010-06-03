using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using umbraco.interfaces;
using umbraco.BusinessLogic;
using umbraco.DataLayer;

namespace umbraco.cms.businesslogic.datatype
{
    public class DefaultPreValueEditor : PlaceHolder, interfaces.IDataPrevalue
    {
        // UI controls
        private TextBox _textbox;
        private DropDownList _dropdownlist;

        // referenced datatype
        private cms.businesslogic.datatype.BaseDataType _datatype;
        
        //WHY IS THIS HERE... IT IS NEVER SET!?
        private BaseDataType _datatypeOld;

        private bool _isEnsured = false;
        private string _prevalue;
        private bool _displayTextBox;

        public static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        /// <summary>
        /// The default editor for editing the build-in pre values in umbraco
        /// </summary>
        /// <param name="DataType">The DataType to be parsed</param>
        /// <param name="DisplayTextBox">Whether to use the default text box</param>
        public DefaultPreValueEditor(cms.businesslogic.datatype.BaseDataType DataType, bool DisplayTextBox)
        {
            // state it knows its datatypedefinitionid
            _displayTextBox = DisplayTextBox;
            _datatype = DataType;
            setupChildControls();
        }


        private void setupChildControls()
        {
            _dropdownlist = new DropDownList();
            _dropdownlist.ID = "dbtype";

            _textbox = new TextBox();
            _textbox.ID = "prevalues";
            _textbox.Visible = _displayTextBox;

            // put the childcontrols in context - ensuring that
            // the viewstate is persisted etc.
            Controls.Add(_textbox);
            Controls.Add(_dropdownlist);

            _dropdownlist.Items.Add(DBTypes.Date.ToString());
            _dropdownlist.Items.Add(DBTypes.Integer.ToString());
            _dropdownlist.Items.Add(DBTypes.Ntext.ToString());
            _dropdownlist.Items.Add(DBTypes.Nvarchar.ToString());
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                if (_datatype != null)
                    _dropdownlist.SelectedValue = _datatype.DBType.ToString();
                else
                    _dropdownlist.SelectedValue = _datatypeOld.DBType.ToString();

                _textbox.Text = Prevalue;
            }
        }

        public string Prevalue
        {
            get
            {
                ensurePrevalue();
                if (_prevalue == null)
                {
                    int defId;
                    if (_datatype != null)
                        defId = _datatype.DataTypeDefinitionId;
                    else if (_datatypeOld != null)
                        defId = _datatypeOld.DataTypeDefinitionId;
                    else
                        throw new ArgumentException("Datatype is not initialized");

                    _prevalue =
                        SqlHelper.ExecuteScalar<string>("Select [value] from cmsDataTypePreValues where DataTypeNodeId = " + defId);
                }
                return _prevalue;
            }
            set
            {
                int defId;
                if (_datatype != null)
                    defId = _datatype.DataTypeDefinitionId;
                else if (_datatypeOld != null)
                    defId = _datatypeOld.DataTypeDefinitionId;
                else
                    throw new ArgumentException("Datatype is not initialized");

                _prevalue = value;
                ensurePrevalue();
                IParameter[] SqlParams = new IParameter[]
                    {
                        SqlHelper.CreateParameter("@value", _textbox.Text),
                        SqlHelper.CreateParameter("@dtdefid", defId)
                    };
                // update prevalue
                SqlHelper.ExecuteNonQuery("update cmsDataTypePreValues set [value] = @value where datatypeNodeId = @dtdefid", SqlParams);
            }
        }

        private void ensurePrevalue()
        {
            if (!_isEnsured)
            {

                int defId;
                if (_datatype != null)
                    defId = _datatype.DataTypeDefinitionId;
                else if (_datatypeOld != null)
                    defId = _datatypeOld.DataTypeDefinitionId;
                else
                    throw new ArgumentException("Datatype is not initialized");

                bool hasPrevalue = PreValues.CountOfPreValues(defId) > 0;
                
                if (!hasPrevalue)
                {
                    PreValue.MakeNew(defId, _textbox.Text);
                }
                _isEnsured = true;
            }
        }

        public Control Editor
        {
            get { return this; }
        }

        public void Save()
        {
            // save the prevalue data and get on with you life ;)
            if (_datatype != null)
                _datatype.DBType = (cms.businesslogic.datatype.DBTypes)Enum.Parse(typeof(cms.businesslogic.datatype.DBTypes), _dropdownlist.SelectedValue, true);
            else if (_datatypeOld != null)
                _datatypeOld.DBType = (DBTypes)Enum.Parse(typeof(DBTypes), _dropdownlist.SelectedValue, true);


            if (_displayTextBox)
            {
                // If the prevalue editor has an prevalue textbox - save the textbox value as the prevalue
                Prevalue = _textbox.Text;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write("<div class='propertyItem'><div class='propertyItemheader'>" + ui.Text("dataBaseDatatype") + "</div>");
            _dropdownlist.RenderControl(writer);
            writer.Write("<br style='clear: both'/></div>");


            if (_displayTextBox)
            {
                writer.Write("<div class='propertyItem'><div class='propertyItemheader'>" + ui.Text("prevalue") + "</div>");
                _textbox.RenderControl(writer);
                writer.Write("<br style='clear: both'/></div>");
            }

            /*
            writer.WriteLine("<div class='propertyItem'>");
            writer.WriteLine("<tr><td>Database datatype</td><td>");
            _dropdownlist.RenderControl(writer);
            writer.Write("</td></tr>");
            if (_displayTextBox)
                writer.WriteLine("<tr><td>Prevalue: </td><td>");
            _textbox.RenderControl(writer);
            writer.WriteLine("</td></tr>");
            writer.Write("</div>");
             */

        }

        [Obsolete("Use the PreValues class for data access instead")]
        public static string GetPrevalueFromId(int Id)
        {
            return SqlHelper.ExecuteScalar<string>("Select [value] from cmsDataTypePreValues where id = @id",
                                                   SqlHelper.CreateParameter("@id", Id));
        }
    }
}
