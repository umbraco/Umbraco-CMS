using System;
using System.Xml;

using umbraco.BasePages;
using umbraco.BusinessLogic;

namespace umbraco.js
{
	public partial class language : UmbracoEnsuredPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
            Response.ContentType = "text/javascript";
            User u = base.getUser();
			if(u == null)
				return;
			string lang = u.Language;
			XmlDocument all = ui.getLanguageFile(lang);
			if(all == null)
				return;
			Response.Write("\nvar uiKeys = new Array();\n");
			foreach(XmlNode x in all.DocumentElement.ChildNodes)
			{
				if(x == null)
					continue;
				foreach(XmlNode key in x.ChildNodes)
				{
					if (key.FirstChild == null || string.IsNullOrEmpty(key.FirstChild.Value))
						continue;

					XmlNode n1 = x.Attributes.GetNamedItem("alias");
					if(n1 == null)
						continue;
					XmlNode n2 = key.Attributes.GetNamedItem("alias");
					if(n2 == null)
						continue;
					string _tempKey = string.Format("{0}_{1}", n1.Value, n2.Value);

                    // we need to remove linie breaks as they can't break js
					string tmpStr = key.FirstChild.Value.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "").Replace("\n", "");
					Response.Write(string.Format("uiKeys['{0}'] = '{1}';\n", _tempKey, tmpStr));
				}
			}
		}

		#region Web Form Designer generated code

		protected override void OnInit(EventArgs e)
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