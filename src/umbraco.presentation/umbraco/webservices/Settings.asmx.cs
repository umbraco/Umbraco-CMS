using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Linq;
using System.Xml;

namespace umbraco.webservices
{
	/// <summary>
	/// Summary description for Settings.
	/// </summary>
	public class Settings : System.Web.Services.WebService
	{
		public Settings()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		[WebMethod]
		public XmlNode GetTabs(string ContextID, int ContentTypeId) 
		{
			if (BasePages.BasePage.ValidateUserContextID(ContextID)) 
			{
				XmlDocument xmlDoc = new XmlDocument();
				XmlElement tabs = xmlDoc.CreateElement("tabs");
                foreach (cms.businesslogic.ContentType.TabI t in new cms.businesslogic.ContentType(ContentTypeId).getVirtualTabs.ToList()) 
				{
					XmlElement mXml = xmlDoc.CreateElement("tab");
					mXml.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "id", t.Id.ToString()));
					mXml.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "caption", t.Caption));
					tabs.AppendChild(mXml);
				}
				return tabs;
			} 
			else
				return null;
		}



		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;
				
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

	}
}
