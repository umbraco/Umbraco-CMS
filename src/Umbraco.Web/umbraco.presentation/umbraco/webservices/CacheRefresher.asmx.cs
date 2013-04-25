using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Xml;
using Umbraco.Core;

namespace umbraco.presentation.webservices
{
	/// <summary>
	/// Summary description for CacheRefresher.
	/// </summary>
	[WebService(Namespace="http://umbraco.org/webservices/")]
	public class CacheRefresher : WebService
	{		
		[WebMethod]
		public void RefreshAll(Guid uniqueIdentifier, string Login, string Password)
		{
			if (BusinessLogic.User.validateCredentials(Login, Password))
			{
				var cr = CacheRefreshersResolver.Current.GetById(uniqueIdentifier);
				cr.RefreshAll();
				
			}
		}

		[WebMethod]
		public void RefreshByGuid(Guid uniqueIdentifier, Guid Id, string Login, string Password)
		{
			if (BusinessLogic.User.validateCredentials(Login, Password))
			{
				var cr = CacheRefreshersResolver.Current.GetById(uniqueIdentifier);
				cr.Refresh(Id);
				
			}
		}

		[WebMethod]
		public void RefreshById(Guid uniqueIdentifier, int Id, string Login, string Password)
		{
			if (BusinessLogic.User.validateCredentials(Login, Password))
			{
				var cr = CacheRefreshersResolver.Current.GetById(uniqueIdentifier);
				cr.Refresh(Id);
				
			}
		}

        [WebMethod]
        public void RemoveById(Guid uniqueIdentifier, int Id, string Login, string Password) {

            if (BusinessLogic.User.validateCredentials(Login, Password)) {
				var cr = CacheRefreshersResolver.Current.GetById(uniqueIdentifier);
                cr.Remove(Id);
            }
        }

		[WebMethod]
		public XmlDocument GetRefreshers(string Login, string Password) 
		{
			if (BusinessLogic.User.validateCredentials(Login, Password))
			{
				var xd = new XmlDocument();
				xd.LoadXml("<cacheRefreshers/>");
				foreach (var cr in CacheRefreshersResolver.Current.CacheResolvers) 
				{
					var n = xmlHelper.addTextNode(xd, "cacheRefresher", cr.Name);
					n.Attributes.Append(xmlHelper.addAttribute(xd, "uniqueIdentifier", cr.UniqueIdentifier.ToString()));
					xd.DocumentElement.AppendChild(n);
				}
				return xd;
						
				
			}
			return null;
		}

	}
}
