using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.IO;
using Umbraco.Core.IO;
using umbraco.presentation.webservices;

namespace umbraco.developer
{
	/// <summary>
	/// Summary description for getXsltStatus.
	/// </summary>
	[WebService(Namespace="http://umbraco.org/webservices")]
	public class getXsltStatus : System.Web.Services.WebService
	{
		public getXsltStatus()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		// TODO: Security-check
		[WebMethod]
		public XmlDocument FilesFromDirectory(string dir) 
		{
            legacyAjaxCalls.Authorize();

			XmlDocument xd = new XmlDocument();
			xd.LoadXml("<files/>");
			foreach (string file in System.IO.Directory.GetFiles(IOHelper.MapPath(SystemDirectories.Umbraco + "/xslt/" + dir), "*.xsl*"))
			{
				FileInfo fi = new FileInfo(file);
				FileAttributes fa = fi.Attributes;
				XmlElement fileXml = xd.CreateElement("file");
				fileXml.SetAttribute("name", fi.Name);
				//fileXml.SetAttribute("created", fi.CreationTimeUtc);
				fileXml.SetAttribute("modified", fi.LastWriteTimeUtc.ToString());
				fileXml.SetAttribute("size", fi.Length.ToString());
				xd.DocumentElement.AppendChild((XmlNode) fileXml);
			}
			return xd;
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

		// WEB SERVICE EXAMPLE
		// The HelloWorld() example service returns the string Hello World
		// To build, uncomment the following lines then save and build the project
		// To test this web service, press F5

//		[WebMethod]
//		public string HelloWorld()
//		{
//			return "Hello World";
//		}
	}
}
