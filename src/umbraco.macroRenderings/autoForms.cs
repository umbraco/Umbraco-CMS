using System;

namespace umbraco.macroRenderings
{
	/// <summary>
	/// Summary description for autoForms.
	/// </summary>
	public class autoForms : System.Web.UI.HtmlControls.HtmlInputHidden, interfaces.IMacroGuiRendering 
	{

		private string _value; 
		public autoForms()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			writer.WriteLine("<input type=\"hidden\" name=\"" + this.ID + "\" value=\"" + this.Value + "\"/>");
		}

		#region IMacroGuiRendering Members

		public override string Value
		{
			set
			{
				_value = value;
			}
		}

		public bool ShowCaption
		{
			get
			{
				return false;
			}
		}

		#endregion
	}
}
