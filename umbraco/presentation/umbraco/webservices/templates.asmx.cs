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
	/// Summary description for templates.
	/// </summary>
	[WebService(Namespace="http://umbraco.org/webservices/")]
	public class templates : System.Web.Services.WebService
	{
		public templates()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		[WebMethod]
		public XmlNode GetTemplates(string Login, string Password) 
		{
			if (BusinessLogic.User.validateCredentials(Login, Password)) 
			{
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml("<templates/>");
				foreach (cms.businesslogic.template.Template t in cms.businesslogic.template.Template.GetAllAsList()) 
				{
					XmlElement tt = xmlDoc.CreateElement("template");
					tt.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "id", t.Id.ToString()));
					tt.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "name", t.Text));
					xmlDoc.DocumentElement.AppendChild(tt);
				}
				return xmlDoc.DocumentElement;
			} else
				return null;
		}

		[WebMethod]
		public XmlNode GetTemplate(int Id, string Login, string Password) 
		{
			if (BusinessLogic.User.validateCredentials(Login, Password)) 
			{
				cms.businesslogic.template.Template t = new cms.businesslogic.template.Template(Id);
				XmlDocument xmlDoc = new XmlDocument();
				XmlElement tXml = xmlDoc.CreateElement("template");
				tXml.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "id", t.Id.ToString()));
				tXml.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "master", t.MasterTemplate.ToString()));
				tXml.Attributes.Append(xmlHelper.addAttribute(xmlDoc, "name", t.Text));
				tXml.AppendChild(xmlHelper.addCDataNode(xmlDoc, "design", t.Design));
				return tXml;
			} else
				return null;
			
		}

		[WebMethod]
		public bool UpdateTemplate(int Id, int Master, string Design, string Login, string Password) 
		{
			if (BusinessLogic.User.validateCredentials(Login, Password)) 
			{
				cms.businesslogic.template.Template t = new cms.businesslogic.template.Template(Id);
				if (t != null) 
				{
					t.MasterTemplate = Master;
					t.Design = Design;
					return true;
				} 
				else
					return false;
			} else
				return false;
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
