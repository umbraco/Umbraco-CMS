using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Xml;


namespace umbraco
{
	/// <summary>
	/// Summary description for webService.
	/// </summary>
	/// 

	[WebService(Namespace="http://umbraco.org/webservices/")]
	public class webService : System.Web.Services.WebService
	{
		public webService()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		[WebMethod]
		public XmlNode GetNode(int NodeId, string ContextID) 
		{
			XmlDocument xd = new XmlDocument();
			if (BasePages.BasePage.ValidateUserContextID(ContextID)) 
			{
				return new cms.businesslogic.CMSNode(NodeId).ToXml(xd, false);
			} 
			else
				return null;
		}

		[WebMethod]
		public XmlNode GetNodeValidate(int NodeId, string Login, string Password) 
		{
			XmlDocument xd = new XmlDocument();
			if (BusinessLogic.User.validateCredentials(Login, Password)) 
			{
				return new cms.businesslogic.CMSNode(NodeId).ToXml(xd, false);
			} 
			else
				return null;
		}

		[WebMethod]
		public XmlNode GetDocument(int NodeId, string ContextID) 
		{
			XmlDocument xd = new XmlDocument();
			if (BasePages.BasePage.ValidateUserContextID(ContextID)) 
			{
				return new cms.businesslogic.web.Document(NodeId).ToXml(xd, false);
			} 
			else
				return null;
		}

		[WebMethod]
		public XmlNode GetMedia(int NodeId, string ContextID) 
		{
			XmlDocument xd = new XmlDocument();
			if (BasePages.BasePage.ValidateUserContextID(ContextID)) 
			{
				return new cms.businesslogic.media.Media(NodeId).ToXml(xd, false);
			} 
			else
				return null;
		}

		[WebMethod]
		public XmlNode GetMediaValidate(int NodeId, string Login, string Password) 
		{
			XmlDocument xd = new XmlDocument();
			if (BusinessLogic.User.validateCredentials(Login, Password)) 
			{
				return new cms.businesslogic.media.Media(NodeId).ToXml(xd, false);
			} 
			else
				return null;
		}


		[WebMethod]
		public XmlNode GetDocumentValidate(int NodeId, string Login, string Password) 
		{
			XmlDocument xd = new XmlDocument();
			if (BusinessLogic.User.validateCredentials(Login, Password)) 
			{
				return new cms.businesslogic.web.Document(NodeId).ToXml(xd, false);
			} 
			else
				return null;
		}

		[WebMethod]
        public XmlNode GetDocumentsBySearchValidate(string Query, int StartNodeId, string Login, string Password) 
		{
			XmlDocument xd = new XmlDocument();
			if (BusinessLogic.User.validateCredentials(Login, Password)) 
			{
				return doQuery(Query, xd, StartNodeId);
			} 
			else 
			{
				XmlNode result = xd.CreateNode(XmlNodeType.Element, "error", "");
				result.AppendChild(xmlHelper.addTextNode(xd, "error", "Not a valid login"));
				return result;
			}
		}

		[WebMethod]
		public XmlNode GetDocumentsBySearch(string Query, int StartNodeId, string ContextID) 
		{
			XmlDocument xd = new XmlDocument();
			if (BasePages.BasePage.ValidateUserContextID(ContextID)) 
			{

				Query = Query.Trim();

				// Check for fulltext or title search
				string prefix = "";
				if (Query.Length > 0 && Query.Substring(0,1) != "*")
					prefix = "Text:";
				else
					Query = Query.Substring(1, Query.Length-1);


				// Check for spaces
				if (Query.IndexOf("\"") == -1 && Query.Trim().IndexOf(" ") > 0) 
				{
					string[] queries = Query.Split(" ".ToCharArray());
					Query = "";
					for (int i=0;i<queries.Length;i++)
						Query += prefix + queries[i] + "* AND ";
					Query = Query.Substring(0, Query.Length-5);
				} else
					Query = prefix + Query + "*";

				return doQuery(Query, xd, StartNodeId);
			} 
			else 
			{
				XmlNode result = xd.CreateNode(XmlNodeType.Element, "error", "");
				result.AppendChild(xmlHelper.addTextNode(xd, "error", "Not a valid login"));
				return result;
			}
}

		private XmlNode doQuery(string Query, XmlDocument xd, int StartNodeId) 
		{
			XmlNode result = xd.CreateNode(XmlNodeType.Element, "documents", "");
			try 
			{
				System.Collections.Generic.List<cms.businesslogic.index.SearchItem> results = cms.businesslogic.index.searcher.Search(
					cms.businesslogic.web.Document._objectType, Query, 20);
				foreach(cms.businesslogic.index.SearchItem si in results)
				{
					//				string parent = "";
					//				if (!dr.IsNull(dr.GetOrdinal("parentText")))
					//					parent = " (" + dr["parentText"].ToString() + ")";
					XmlElement x = xd.CreateElement("document");
                    x.SetAttribute("id", si.NodeId.ToString());
					x.SetAttribute("nodeName", si.Title);
					result.AppendChild(x);
				}
			} 
			catch (Exception ee)
			{
				XmlElement x = xd.CreateElement("document");
				x.SetAttribute("id", "0");
				x.SetAttribute("nodeName", "Error in search: " + ee.ToString());
				result.AppendChild(x);
			}
			return result;
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
