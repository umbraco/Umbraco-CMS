using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using umbraco.BusinessLogic;
using umbraco.editorControls;

namespace umbraco.editorControls.relatedlinks
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class RelatedLinksPrevalueEditor : System.Web.UI.WebControls.PlaceHolder, umbraco.interfaces.IDataPrevalue
    {
        #region IDataPrevalue Members

        // referenced datatype
        private umbraco.cms.businesslogic.datatype.BaseDataType _datatype;

        //private DropDownList _dropdownlist;
        //private CheckBox _showUrls;

        public RelatedLinksPrevalueEditor(umbraco.cms.businesslogic.datatype.BaseDataType DataType)
        {

            _datatype = DataType;
            setupChildControls();

        }

        private void setupChildControls()
        {
            //_dropdownlist = new DropDownList();
            //_dropdownlist.ID = "dbtype";
            ////_dropdownlist.Items.Add(DBTypes.Date.ToString());
            ////_dropdownlist.Items.Add(DBTypes.Integer.ToString());
            //_dropdownlist.Items.Add(DBTypes.Ntext.ToString());
            //_dropdownlist.Items.Add(DBTypes.Nvarchar.ToString());

            ////_checkboxShowGrandChildren = new CheckBox();
            ////_checkboxShowGrandChildren.ID = "showurls";


            //Controls.Add(_dropdownlist);
            //Controls.Add(_showUrls);
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
            //if (!Page.IsPostBack)
            //{
            //    string[] config = Configuration.Split("|".ToCharArray());
            //    if (config.Length > 1)
            //    {
            //        //_showUrls.Checked = Convert.ToBoolean(config[0]);

            //    }
            //    _dropdownlist.SelectedValue = _datatype.DBType.ToString();
            //}
        }

        public void Save()
        {
            _datatype.DBType = (umbraco.cms.businesslogic.datatype.DBTypes)Enum.Parse(typeof(umbraco.cms.businesslogic.datatype.DBTypes), DBTypes.Ntext.ToString(), true);


        }

        protected override void Render(HtmlTextWriter writer)
        {
            //writer.WriteLine("<table>");
            //writer.WriteLine("<tr><th>Database datatype:</th><td>");
            //_dropdownlist.RenderControl(writer);
            //writer.Write("</td></tr>");

            //writer.Write("</table>");
        }

        public string Configuration
        {
            get
            {
              return "";

            }
        }

        #endregion
    }
}
