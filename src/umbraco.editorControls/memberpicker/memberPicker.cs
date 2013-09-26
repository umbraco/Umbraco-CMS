using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Data;

using umbraco.BusinessLogic;
using umbraco.DataLayer;

namespace umbraco.editorControls
{
	/// <summary>
	/// Summary description for memberPicker.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
	public class memberPicker : System.Web.UI.WebControls.DropDownList, interfaces.IDataEditor
	{

		//private String _text;

		interfaces.IData _data;
		public memberPicker(interfaces.IData Data)
		{
			_data = Data;
		}

		public virtual bool TreatAsRichTextEditor 
		{
			get {return false;}
		}
		public virtual bool ShowLabel 
		{
			get {return true;}
		}

		public Control Editor {
			get {return this;}
		}
		
		//public override string Text
		//{
		//    get
		//    {
		//        if (!Page.IsPostBack) 
		//        {
		//            _text = _data.Value.ToString();
		//        }
		//        return _text;
		//    }
		//}
		public void Save() 
		{
				_data.Value = base.SelectedValue;
				
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);
			IRecordsReader dropdownData = Application.SqlHelper.ExecuteReader("select id, text from umbracoNode where nodeObjectType = '39EB0F98-B348-42A1-8662-E7EB18487560' order by text");
			base.DataValueField = "id";
			base.DataTextField = "text";
			base.DataSource = dropdownData;
			base.DataBind();
			base.Items.Insert(0, new ListItem(ui.Text("choose") + "...",""));

            base.SelectedValue = _data.Value != null ? _data.Value.ToString() : "";

			// Iterate on the control items and mark fields by match them with the Text property!
			//foreach(ListItem li in base.Items) 
			//{
			//	if ((","+base.SelectedValue+",").IndexOf(","+li.Value.ToString()+",") > -1)
			//		li.Selected = true;
			//}

			dropdownData.Close();
		}
	
		/// <summary> 
		/// Render this control to the output parameter specified.
		/// </summary>
		/// <param name="output"> The HTML writer to write out to </param>
		protected override void Render(HtmlTextWriter output)
		{
			base.Render(output);
		}
	}
}
