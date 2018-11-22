using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.interfaces;
using datatype = umbraco.cms.businesslogic.datatype;

    
namespace umbraco.editorControls.tags
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class PrevalueEditor : PlaceHolder, IDataPrevalue
    {
        private TextBox _group;

        /// <summary>
        /// Unused, please do not use
        /// </summary>
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        public ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        // referenced datatype
        private datatype.BaseDataType _datatype;

        public PrevalueEditor(datatype.BaseDataType DataType) 
		{
            _datatype = DataType;
            setupChildControls();

    	}

        public Control Editor
        {
            get
            {
                return this;
            }
        }

        public void Save()
        {
            //clear all datatype data first...
            using (var sqlHelper = Application.SqlHelper)
                sqlHelper.ExecuteNonQuery("delete from cmsDataTypePrevalues where datatypeNodeId = " + _datatype.DataTypeDefinitionId);

            //Save datatype
            _datatype.DBType = datatype.DBTypes.Ntext;

            // Save path and control type..
            using (var sqlHelper = Application.SqlHelper)
                sqlHelper.ExecuteNonQuery("insert into cmsDataTypePrevalues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,@alias)",
                    sqlHelper.CreateParameter("@value", _group.Text),
                    sqlHelper.CreateParameter("@alias", "group"),
                    sqlHelper.CreateParameter("@dtdefid", _datatype.DataTypeDefinitionId));
        }

        private void setupChildControls()
        {
            _group = new TextBox();
            _group.ID = "group";
            _group.CssClass = "umbEditorTextField";

            // put the childcontrols in context - ensuring that
            // the viewstate is persisted etc.
            this.Controls.Add(_group);

            SortedList _prevalues = Prevalues;

            if (_prevalues.ContainsKey("group"))
                _group.Text = _prevalues["group"].ToString();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.WriteLine("<table>");

            writer.WriteLine("<tr><td>Tag group:</td><td>");
            _group.RenderControl(writer);
            writer.Write("</td></tr>");
            writer.Write("</table>");
        }

        public SortedList Prevalues
        {
            get
            {
                SortedList retval = new SortedList();
                using (var sqlHelper = Application.SqlHelper)
                using (var dr = sqlHelper.ExecuteReader(
                    "Select alias, [value] from cmsDataTypeprevalues where DataTypeNodeId = " + _datatype.DataTypeDefinitionId + " order by sortorder"))
                {
                    while (dr.Read())
                    {
                        if (!retval.ContainsKey(dr.GetString("alias")))
                        {
                            retval.Add(dr.GetString("alias"), dr.GetString("value"));
                        }
                    }

                    return retval;
                }
            }
        }
    }
}
