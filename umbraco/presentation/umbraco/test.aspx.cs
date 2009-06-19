using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using umbraco.cms.businesslogic.template;
using System.Xml.Xsl;
using System.Reflection;
using System.Xml;
using System.IO;


using umbraco.cms.businesslogic.web;
using umbraco.presentation.nodeFactory;

namespace umbraco.cms.presentation
{
	/// <summary>
	/// Summary description for test.
	/// </summary>
	public partial class test : BasePages.UmbracoEnsuredPage
	{
        protected controls.macroParameterControl mp = new umbraco.controls.macroParameterControl();

		protected void Page_Load(object sender, System.EventArgs e) 
		{
            Assembly a = Assembly.Load("__code");
            Type[] t = a.GetTypes();
		}

		#region Web Form Designer generated code
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    

		}
		#endregion

        protected void button_Click(object sender, EventArgs e)
        {
            result.Controls.Add(new LiteralControl("Macro tag: " + Server.HtmlEncode(mp.GetMacroTag()) + " <br/>"));
            IDictionaryEnumerator ide = mp.ParameterValues.GetEnumerator();
            while (ide.MoveNext())
                result.Controls.Add(
                    new LiteralControl("<li>" + ide.Key.ToString() + ": " + ide.Value.ToString() + "</li>"));
        }

	}
}

