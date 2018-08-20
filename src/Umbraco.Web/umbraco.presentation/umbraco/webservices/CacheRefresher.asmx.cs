using System;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;
using umbraco.interfaces;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.Web.Security;

namespace umbraco.presentation.webservices
{
	/// <summary>
	/// CacheRefresher web service.
	/// </summary>
	[WebService(Namespace="http://umbraco.org/webservices/")]
	public class CacheRefresher : WebService
    {
        #region Helpers

        // is the server originating from this server - ie are we self-messaging?
        // in which case we should ignore the message because it's been processed locally already
        internal static bool SelfMessage(string hash)
        {
            if (string.IsNullOrEmpty(hash)) return false; // no hash = don't know = not self
            if (hash != WebServiceServerMessenger.GetCurrentServerHash()) return false;

            LogHelper.Debug<CacheRefresher>(
                "Ignoring self-message. (server: {0}, appId: {1}, hash: {2})",
                () => NetworkHelper.MachineName,
                () => HttpRuntime.AppDomainAppId,
                () => hash);

            return true;
        }

        private static ICacheRefresher GetRefresher(Guid id)
        {
            var refresher = CacheRefreshersResolver.Current.GetById(id);
            if (refresher == null)
                throw new InvalidOperationException("Cache refresher with ID \"" + id + "\" does not exist.");
            return refresher;
        }

        private static IJsonCacheRefresher GetJsonRefresher(Guid id)
        {
            return GetJsonRefresher(GetRefresher(id));
        }

        private static IJsonCacheRefresher GetJsonRefresher(ICacheRefresher refresher)
        {
            var jsonRefresher = refresher as IJsonCacheRefresher;
            if (jsonRefresher == null)
                throw new InvalidOperationException("Cache refresher with ID \"" + refresher.UniqueIdentifier + "\" does not implement " + typeof(IJsonCacheRefresher) + ".");
            return jsonRefresher;
        }

        private bool Authorized(string login, string rawPassword)
        {
            //TODO: This technique of passing the raw password in is a legacy idea and isn't really 
            // a very happy way to secure this webservice. To prevent brute force attacks, we need 
            // to ensure that the lockout policies are applied, though because we are not authenticating
            // the user with their real password, we need to do this a bit manually.

            var userMgr = Context.GetOwinContext().GetBackOfficeUserManager();
            
            var user = ApplicationContext.Current.Services.UserService.GetByUsername(login);
            if (user == null) return false;

            var u = userMgr.FindById(user.Id);
            if (u == null) return false;

            if (u.IsLockedOut) return false;

            if (user.RawPasswordValue != rawPassword)
            {
                //this performs the lockout and/or increments the access failed count
                userMgr.AccessFailed(u.Id);
                return false;
            }

            return true;
        }

        #endregion

        [WebMethod]
        public void BulkRefresh(RefreshInstruction[] instructions, string appId, string login, string password)
        {
            if (Authorized(login, password) == false) return;
            if (SelfMessage(appId)) return; // do not process self-messages

            // only execute distinct instructions - no sense in running the same one more than once
            foreach (var instruction in instructions.Distinct())
            {
                var refresher = GetRefresher(instruction.RefresherId);
                switch (instruction.RefreshType)
                {
                    case RefreshMethodType.RefreshAll:
                        refresher.RefreshAll();
                        break;
                    case RefreshMethodType.RefreshByGuid:
                        refresher.Refresh(instruction.GuidId);
                        break;
                    case RefreshMethodType.RefreshById:
                        refresher.Refresh(instruction.IntId);
                        break;
                    case RefreshMethodType.RefreshByIds: // not directly supported by ICacheRefresher
                        foreach (var id in JsonConvert.DeserializeObject<int[]>(instruction.JsonIds))
	                        refresher.Refresh(id);
                        break;
                    case RefreshMethodType.RefreshByJson:
                        GetJsonRefresher(refresher).Refresh(instruction.JsonPayload);
                        break;
                    case RefreshMethodType.RemoveById:
                        refresher.Remove(instruction.IntId);
                        break;
                    //case RefreshMethodType.RemoveByIds: // not directly supported by ICacheRefresher
                    //    foreach (var id in JsonConvert.DeserializeObject<int[]>(instruction.JsonIds))
                    //        refresher.Remove(id);
                    //    break;
                }
            }
        }

		[WebMethod]
		public void RefreshAll(Guid uniqueIdentifier, string Login, string Password)
		{
		    if (Authorized(Login, Password) == false) return;
			GetRefresher(uniqueIdentifier).RefreshAll();
		}

	    [WebMethod]
		public void RefreshByGuid(Guid uniqueIdentifier, Guid Id, string Login, string Password)
		{
            if (Authorized(Login, Password) == false) return;
            GetRefresher(uniqueIdentifier).Refresh(Id);
		}

		[WebMethod]
		public void RefreshById(Guid uniqueIdentifier, int Id, string Login, string Password)
		{
		    if (Authorized(Login, Password) == false) return;
			GetRefresher(uniqueIdentifier).Refresh(Id);
		}

        [WebMethod]
        public void RefreshByIds(Guid uniqueIdentifier, string jsonIds, string Login, string Password)
        {
            if (Authorized(Login, Password) == false) return;
            var refresher = GetRefresher(uniqueIdentifier);
	        foreach (var id in JsonConvert.DeserializeObject<int[]>(jsonIds))
	            refresher.Refresh(id);
        }

        [WebMethod]
        public void RefreshByJson(Guid uniqueIdentifier, string jsonPayload, string Login, string Password)
        {
            if (Authorized(Login, Password) == false) return;
            GetJsonRefresher(uniqueIdentifier).Refresh(jsonPayload);
        }

	    [WebMethod]
        public void RemoveById(Guid uniqueIdentifier, int Id, string Login, string Password) 
        {
            if (Authorized(Login, Password) == false) return;
            GetRefresher(uniqueIdentifier).Remove(Id);
        }

	    [WebMethod]
		public XmlDocument GetRefreshers(string Login, string Password)
	    {
	        if (Authorized(Login, Password) == false) return null;

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
	}
}
