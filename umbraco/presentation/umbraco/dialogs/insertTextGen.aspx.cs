using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;

namespace umbraco.dialogs
{
	/// <summary>
	/// Summary description for insertTextGen.
	/// </summary>
	public partial class insertTextGen : BasePages.UmbracoEnsuredPage
	{

		


		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here

			// Load form config file
			XmlDocument fontConfig = new XmlDocument();
			fontConfig.Load(System.Web.HttpContext.Current.Server.MapPath(GlobalSettings.Path + "/config/fonts/fonts.xml"));
			foreach (XmlNode font in fontConfig.SelectSingleNode("//fonts")) 
				fontList.Items.Add(new ListItem(font.FirstChild.Value, font.Attributes.GetNamedItem("filename").Value));
			foreach (XmlNode fontSize in fontConfig.SelectSingleNode("//sizes"))
				fontSizeList.Items.Add(new ListItem(fontSize.FirstChild.Value));

			ArrayList _colors = new ArrayList();
			ArrayList _bgColors = new ArrayList();
			foreach (XmlNode color in fontConfig.SelectSingleNode("//colors"))
				_colors.Add(new ListItem(color.FirstChild.Value));
			foreach (XmlNode bgColor in fontConfig.SelectSingleNode("//bgcolors"))
				_bgColors.Add(new ListItem(bgColor.FirstChild.Value));
			PlaceholderColor.Controls.Add(colorRange(_colors, "pickerColor"));
			PlaceHolderBgColor.Controls.Add(colorRange(_bgColors, "pickerBgColor"));

		}

		private Control colorRange(ArrayList Colors, string Alias) 
		{
			Control _colors = new Control();
			_colors.Controls.Add(new LiteralControl("<input type=\"hidden\" name=\"" + Alias + "\" value=\"FFFFFF\"/>"));
			_colors.Controls.Add(new LiteralControl("<span id=\"" + Alias + "holder\" style=\"border: 1px solid black; background-color: #FFFFFF\"><img src=\"../images/nada.gif\" width=15 height=15 border=0></span>&nbsp; " + ui.Text("choose") + ": "));
			foreach (object color in Colors) 
			{
				_colors.Controls.Add(
					new LiteralControl("<span style=\"margin: 2px; border: 1px solid black; background-color: #" + color.ToString() + "\"><a href=\"#\" onClick=\"document.forms[0]['" + Alias + "'].value = '" + color.ToString() + "'; document.all['" + Alias + "holder'].style.backgroundColor = '#" + color.ToString() + "'\"><img src=\"../images/nada.gif\" width=15 height=15 border=0></a></span>"));
			}

			return _colors;
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    

		}
		#endregion
	}
}
