using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core;
using umbraco.interfaces;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using System.Collections.Generic;

namespace umbraco.cms.businesslogic.datatype
{

    [Obsolete("This class is no longer used and will be removed from the codebase in the future.")]
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


        private Dictionary<string, DataEditorSettingType> dtSettings = new Dictionary<string, DataEditorSettingType>();


        public static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        /// <summary>
        /// The default editor for editing the built-in pre values in umbraco
        /// </summary>
        /// <param name="DataType">The DataType to be parsed</param>
        /// <param name="DisplayTextBox">Whether to use the default text box</param>
        public DefaultPreValueEditor(cms.businesslogic.datatype.BaseDataType DataType, bool DisplayTextBox)
        {
            // state it knows its datatypedefinitionid
            _displayTextBox = DisplayTextBox;
            _datatype = DataType;
            //setupChildControls();
        }


        //private void setupChildControls()
        //{
        //    DataEditorPropertyPanel pnlType = new DataEditorPropertyPanel();
        //    pnlType.Text = ui.Text("dataBaseDatatype");

        //    _dropdownlist = new DropDownList();
        //    _dropdownlist.ID = "dbtype";

        //    _dropdownlist.Items.Add(DBTypes.Date.ToString());
        //    _dropdownlist.Items.Add(DBTypes.Integer.ToString());
        //    _dropdownlist.Items.Add(DBTypes.Ntext.ToString());
        //    _dropdownlist.Items.Add(DBTypes.Nvarchar.ToString());

        //    DataEditorPropertyPanel pnlPrevalue = new DataEditorPropertyPanel();
        //    pnlPrevalue.Text = ui.Text("prevalue");

        //    _textbox = new TextBox();
        //    _textbox.ID = "prevalues";
        //    _textbox.Visible = _displayTextBox;

        //    // put the childcontrols in context - ensuring that
        //    // the viewstate is persisted etc.

        //    pnlType.Controls.Add(_dropdownlist);
        //    Controls.Add(pnlType);

        //    if (_displayTextBox)
        //    {
        //        pnlPrevalue.Controls.Add(_textbox);
        //        Controls.Add(pnlPrevalue);
        //    }

            
        //}

        protected override void OnLoad(EventArgs e)
        {
            LoadSettings();

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
            bool hasErrors = false;
            foreach (KeyValuePair<string, DataEditorSettingType> k in dtSettings)
            {
                var result = k.Value.Validate();
                Label lbl = this.FindControlRecursive<Label>("lbl" + k.Key);
                if (result == null && lbl != null)
                {
                    if (lbl != null)
                        lbl.Text = string.Empty;
                }
                else
                {
                    if (hasErrors == false)
                        hasErrors = true;

                    if (lbl != null)
                        lbl.Text = " " + result.ErrorMessage;
                }
            }

            if (!hasErrors)
            {
                // save the prevalue data and get on with you life ;)
                if (_datatype != null)
                    _datatype.DBType =
                        (cms.businesslogic.datatype.DBTypes)
                        Enum.Parse(typeof (cms.businesslogic.datatype.DBTypes), _dropdownlist.SelectedValue, true);
                else if (_datatypeOld != null)
                    _datatypeOld.DBType = (DBTypes) Enum.Parse(typeof (DBTypes), _dropdownlist.SelectedValue, true);


                if (_displayTextBox)
                {
                    // If the prevalue editor has an prevalue textbox - save the textbox value as the prevalue
                    Prevalue = _textbox.Text;
                }

                DataEditorSettingsStorage ss = new DataEditorSettingsStorage();

                ss.ClearSettings(_datatype.DataTypeDefinitionId);

                int i = 0;
                foreach (KeyValuePair<string, DataEditorSettingType> k in dtSettings)
                {
                    ss.InsertSetting(_datatype.DataTypeDefinitionId, k.Key, k.Value.Value, i);
                    i++;

                }

                ss.Dispose();
            }
        }

        //protected override void Render(HtmlTextWriter writer)
        //{
        //    writer.Write("<div class='propertyItem'><div class='propertyItemheader'>" + ui.Text("dataBaseDatatype") + "</div>");
        //    _dropdownlist.RenderControl(writer);
        //    writer.Write("<br style='clear: both'/></div>");


        //    if (_displayTextBox)
        //    {
        //        writer.Write("<div class='propertyItem'><div class='propertyItemheader'>" + ui.Text("prevalue") + "</div>");
        //        _textbox.RenderControl(writer);
        //        writer.Write("<br style='clear: both'/></div>");
        //    }

        //    /*
        //    writer.WriteLine("<div class='propertyItem'>");
        //    writer.WriteLine("<tr><td>Database datatype</td><td>");
        //    _dropdownlist.RenderControl(writer);
        //    writer.Write("</td></tr>");
        //    if (_displayTextBox)
        //        writer.WriteLine("<tr><td>Prevalue: </td><td>");
        //    _textbox.RenderControl(writer);
        //    writer.WriteLine("</td></tr>");
        //    writer.Write("</div>");
        //     */

        //}

        [Obsolete("Use the PreValues class for data access instead")]
        public static string GetPrevalueFromId(int Id)
        {
            return SqlHelper.ExecuteScalar<string>("Select [value] from cmsDataTypePreValues where id = @id",
                                                   SqlHelper.CreateParameter("@id", Id));
        }



        protected void LoadSettings()
        {
            DataEditorPropertyPanel pnlType = new DataEditorPropertyPanel();
            pnlType.Text = ui.Text("dataBaseDatatype");

            _dropdownlist = new DropDownList();
            _dropdownlist.ID = "dbtype";

            _dropdownlist.Items.Add(DBTypes.Date.ToString());
            _dropdownlist.Items.Add(DBTypes.Integer.ToString());
            _dropdownlist.Items.Add(DBTypes.Ntext.ToString());
            _dropdownlist.Items.Add(DBTypes.Nvarchar.ToString());

            DataEditorPropertyPanel pnlPrevalue = new DataEditorPropertyPanel();
            pnlPrevalue.Text = ui.Text("prevalue");

            _textbox = new TextBox();
            _textbox.ID = "prevalues";
            _textbox.Visible = _displayTextBox;

            // put the childcontrols in context - ensuring that
            // the viewstate is persisted etc.

            pnlType.Controls.Add(_dropdownlist);
            Controls.Add(pnlType);

            if (_displayTextBox)
            {
                pnlPrevalue.Controls.Add(_textbox);
                Controls.Add(pnlPrevalue);
            }

           
            foreach (KeyValuePair<string, DataEditorSetting> kv in _datatype.Settings())
            {
                DataEditorSettingType dst = kv.Value.GetDataEditorSettingType();
                dtSettings.Add(kv.Key, dst);

                DataEditorPropertyPanel panel = new DataEditorPropertyPanel();
                panel.Text = kv.Value.GetName();
                panel.Text += "<br/><small>" + kv.Value.description + "</small>";


                if (_datatype.HasSettings())
                {
                    DataEditorSettingsStorage ss = new DataEditorSettingsStorage();

                    List<Setting<string, string>> s = ss.GetSettings(_datatype.DataTypeDefinitionId);
                    ss.Dispose();

                    if (s.Find(set => set.Key == kv.Key).Value != null)
                        dst.Value = s.Find(set => set.Key == kv.Key).Value;

                }

                panel.Controls.Add(dst.RenderControl(kv.Value));

                Label invalid = new Label();
                invalid.Attributes.Add("style", "color:#8A1F11");
                invalid.ID = "lbl" + kv.Key;
                panel.Controls.Add(invalid);

                this.Controls.Add(panel);
            }
            
        }
    }


    public class DataEditorPropertyPanel : System.Web.UI.WebControls.Panel
    {
        public DataEditorPropertyPanel()
        {

        }

        private string m_Text = string.Empty;
        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; }
        }


        protected override void OnLoad(System.EventArgs EventArguments)
        {
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {

            this.CreateChildControls();
            string styleString = "";

            foreach (string key in this.Style.Keys)
            {
                styleString += key + ":" + this.Style[key] + ";";
            }

            writer.WriteLine("<div class=\"propertyItem\" style='" + styleString + "'>");
            if (m_Text != string.Empty)
            {
                writer.WriteLine("<div class=\"propertyItemheader\">" + m_Text + "</div>");
                writer.WriteLine("<div class=\"propertyItemContent\">");
            }

            try
            {
                this.RenderChildren(writer);
            }
            catch (Exception ex)
            {
                writer.WriteLine("Error creating control <br />");
                writer.WriteLine(ex.ToString());
            }

            if (m_Text != string.Empty)
                writer.WriteLine("</div>");

            writer.WriteLine("</div>");


        }

    }
}
