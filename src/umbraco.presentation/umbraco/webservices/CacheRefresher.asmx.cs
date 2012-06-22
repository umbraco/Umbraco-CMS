using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Xml;

namespace umbraco.presentation.webservices
{
	/// <summary>
	/// Summary description for CacheRefresher.
	/// </summary>
	[WebService(Namespace="http://umbraco.org/webservices/")]
	public class CacheRefresher : System.Web.Services.WebService
	{
		public CacheRefresher()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
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

		[WebMethod]
		public void RefreshAll(Guid uniqueIdentifier, string Login, string Password)
		{
			if (BusinessLogic.User.validateCredentials(Login, Password))
			{
				interfaces.ICacheRefresher cr = new cache.Factory().GetNewObject(uniqueIdentifier);
				cr.RefreshAll();
				
			}
		}

		[WebMethod]
		public void RefreshByGuid(Guid uniqueIdentifier, Guid Id, string Login, string Password)
		{
			if (BusinessLogic.User.validateCredentials(Login, Password))
			{
				interfaces.ICacheRefresher cr = new cache.Factory().GetNewObject(uniqueIdentifier);
				cr.Refresh(Id);
				
			}
		}

		[WebMethod]
		public void RefreshById(Guid uniqueIdentifier, int Id, string Login, string Password)
		{
			if (BusinessLogic.User.validateCredentials(Login, Password))
			{
				interfaces.ICacheRefresher cr = new cache.Factory().GetNewObject(uniqueIdentifier);
				cr.Refresh(Id);
				
			}
		}

        [WebMethod]
        public void RemoveById(Guid uniqueIdentifier, int Id, string Login, string Password) {

            if (BusinessLogic.User.validateCredentials(Login, Password)) {
                interfaces.ICacheRefresher cr = new cache.Factory().GetNewObject(uniqueIdentifier);
                cr.Remove(Id);
            }
        }

		[WebMethod]
		public XmlDocument GetRefreshers(string Login, string Password) 
		{
			if (BusinessLogic.User.validateCredentials(Login, Password))
			{
				XmlDocument xd = new XmlDocument();
				xd.LoadXml("<cacheRefreshers/>");
				foreach (interfaces.ICacheRefresher cr in new cache.Factory().GetAll()) 
				{
					XmlNode n = xmlHelper.addTextNode(xd, "cacheRefresher", cr.Name);
					n.Attributes.Append(xmlHelper.addAttribute(xd, "uniqueIdentifier", cr.UniqueIdentifier.ToString()));
					xd.DocumentElement.AppendChild(n);
				}
				return xd;
						
				
			}
			return null;
		}

	}
}
