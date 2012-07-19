using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace umbraco.layoutControls
{
	/// <summary>
	/// Summary description for ShowField.
	/// </summary>
	[DefaultProperty("FieldName"), 
		ToolboxData("<{0}:ShowField runat=server></{0}:ShowField>")]
	public class ShowField : System.Web.UI.WebControls.WebControl
	{
		private string _fieldName;
	
		[Bindable(true), 
			Category("umbraco"), 
			DefaultValue("PageName")] 
		public string FieldName 
		{
			get
			{
				return _fieldName;
			}

			set
			{
				_fieldName = value;
			}
		}

		/// <summary> 
		/// Render this control to the output parameter specified.
		/// </summary>
		/// <param name="output"></param>
		protected override void Render(HtmlTextWriter output)
		{
			layoutControls.umbracoPageHolder umbPageHolder = 
				(layoutControls.umbracoPageHolder) Page.FindControl("umbPageHolder");
			try 
			{
				output.Write(umbPageHolder.Elements[_fieldName].ToString());
			} 
			catch 
			{
				output.Write("<span style=\"Color: red\">Field not found ('" + _fieldName + "')</span>");
			}
		}
	}
}
