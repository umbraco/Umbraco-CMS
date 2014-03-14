using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Cache;

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

        /// <summary>
        /// Refreshes objects for all Ids matched in the json string
        /// </summary>
        /// <param name="uniqueIdentifier"></param>
        /// <param name="jsonIds">A JSON Serialized string of ids to match</param>
        /// <param name="Login"></param>
        /// <param name="Password"></param>
        [WebMethod]
        public void RefreshByIds(Guid uniqueIdentifier, string jsonIds, string Login, string Password)
        {
            var serializer = new JavaScriptSerializer();
            var ids = serializer.Deserialize<int[]>(jsonIds);

            if (BusinessLogic.User.validateCredentials(Login, Password))
            {
                var cr = CacheRefreshersResolver.Current.GetById(uniqueIdentifier);
                foreach (var i in ids)
                {
                    cr.Refresh(i);   
                }                
            }
        }

        /// <summary>
        /// Refreshes objects using the passed in Json payload, it will be up to the cache refreshers to deserialize
        /// </summary>
        /// <param name="uniqueIdentifier"></param>
        /// <param name="jsonPayload">A custom JSON payload used by the cache refresher</param>
        /// <param name="Login"></param>
        /// <param name="Password"></param>
        /// <remarks>
        /// NOTE: the cache refresher defined by the ID MUST be of type IJsonCacheRefresher or an exception will be thrown
        /// </remarks>
        [WebMethod]
        public void RefreshByJson(Guid uniqueIdentifier, string jsonPayload, string Login, string Password)
        {            
            if (BusinessLogic.User.validateCredentials(Login, Password))
            {
                var cr = CacheRefreshersResolver.Current.GetById(uniqueIdentifier) as IJsonCacheRefresher;
                if (cr == null)
                {
                    throw new InvalidOperationException("The cache refresher: " + uniqueIdentifier + " is not of type " + typeof (IJsonCacheRefresher));
                }
                cr.Refresh(jsonPayload);
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
				foreach (var cr in CacheRefreshersResolver.Current.CacheRefreshers) 
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
