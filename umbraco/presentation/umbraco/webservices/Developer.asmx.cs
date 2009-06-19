using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;

using System.Xml;

namespace umbraco.webservices
{
	/// <summary>
	/// Summary description for Developer.
	/// </summary>
	[WebService(Namespace="http://umbraco.org/webservices/")]
	public class Developer : System.Web.Services.WebService
	{
		public Developer()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		[WebMethod]
		public string BootStrapTidy(string html, string ContextID) 
		{
			return cms.helpers.xhtml.BootstrapTidy(html);
		}

		[WebMethod]
		public XmlNode GetMacros(string Login, string Password) 
		{
			if (BusinessLogic.User.validateCredentials(Login, Password))  
			{
				XmlDocument xmlDoc = new XmlDocument();
				XmlElement macros = xmlDoc.CreateElement("macros");
				foreach (cms.businesslogic.macro.Macro m in cms.businesslogic.macro.Macro.GetAll()) 
				{
					XmlElement mXml = xmlDoc.CreateElement("macro");
					mXml.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "id", m.Id.ToString()));
					mXml.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "alias", m.Alias));
					mXml.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "name", m.Name));
					macros.AppendChild(mXml);
				}
				return macros;
			} else
				return null;
		}

		[WebMethod]
		public XmlNode GetMacro(int Id, string Login, string Password) 
		{
			if (BusinessLogic.User.validateCredentials(Login, Password)) 
			{
				XmlDocument xmlDoc = new XmlDocument();
				XmlElement macro = xmlDoc.CreateElement("macro");
				cms.businesslogic.macro.Macro m = new cms.businesslogic.macro.Macro(Id);
				macro.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "id", m.Id.ToString()));
				macro.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "refreshRate", m.RefreshRate.ToString()));
				macro.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "useInEditor", m.UseInEditor.ToString()));
				macro.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "alias", m.Alias));
				macro.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "name", m.Name));
				macro.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "assembly", m.Assembly));
				macro.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "type", m.Type));
				macro.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "xslt", m.Xslt));
				XmlElement Properties = xmlDoc.CreateElement("properties");
				foreach (cms.businesslogic.macro.MacroProperty mp in m.Properties) 
				{
					XmlElement pXml = xmlDoc.CreateElement("property");
					pXml.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "alias", mp.Alias));
					pXml.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "name", mp.Name));
					pXml.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "public", mp.Public.ToString()));
					Properties.AppendChild(pXml);
				}
				macro.AppendChild(Properties);
				return macro;
			} else
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
