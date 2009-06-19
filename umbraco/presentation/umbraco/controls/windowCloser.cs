using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace umbraco.controls
{
	/// <summary>
	/// Summary description for windowCloser.
	/// </summary>
	[DefaultProperty("Text"), 
		ToolboxData("<{0}:windowCloser runat=server></{0}:windowCloser>")]
	public class windowCloser : System.Web.UI.WebControls.WebControl
	{
		private string text;
		private string secondText;
		private string seconds;
	
		[Bindable(true), 
		Category("Appearance"), 
		DefaultValue("")] 
		public string Text 
		{
			get
			{
				return text;
			}

			set
			{
				text = value;
			}
		}
		[Bindable(true), 
		Category("Appearance"), 
		DefaultValue("")] 
		public string SecondText 
		{
			get
			{
				return secondText;
			}

			set
			{
				secondText = value;
			}
		}
		[Bindable(true), 
		Category("Appearance"), 
		DefaultValue("")] 
		public string Seconds 
		{
			get
			{
				return seconds;
			}

			set
			{
				seconds = value;
			}
		}

		protected override void OnInit(EventArgs e)
		{
			Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "windowCloser", "<script language=\"javascript\">\nfunction windowCloser(elementId, sec) {\n	if (sec < 2) \n		window.close();\n	else {\n		sec--;\n		document.getElementById(elementId).innerHTML = sec;\n		setTimeout('windowCloser(\\'' + elementId + '\\', ' + sec + ');', 1000);\n	}\n}\n</script>");
			base.OnInit (e);
		}

		/// <summary> 
		/// Render this control to the output parameter specified.
		/// </summary>
		/// <param name="output"> The HTML writer to write out to </param>
		protected override void Render(HtmlTextWriter output)
		{
output.Write(text + " <span id=\"timer\">" + seconds + "</span> " + secondText + "...\n</form>\n<script>\n	windowCloser('timer', " + seconds + ");\n</script>");
		}
	}
}
