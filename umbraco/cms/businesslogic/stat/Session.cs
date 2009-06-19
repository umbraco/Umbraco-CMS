using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

using umbraco.cms.businesslogic.member;
using umbraco.DataLayer;
using umbraco.BusinessLogic;

namespace umbraco.cms.businesslogic.stat
{
	/// <summary>
	/// Summary description for Session.
	/// </summary>
	/// 
	public class Session
	{
		#region Declarations

		private static Hashtable _sessions = Hashtable.Synchronized(new Hashtable());

		private ArrayList _entries = new ArrayList();
		private Guid _sessionId = Guid.NewGuid();
		private Guid _member;
		private int _newsletter = 0;
		private bool _returningUser = false;
		private bool _cookies = false;
		private DateTime _sessionStart;
		private string _language = "";
		private string _userAgent = "";
		private string _browserBrand = "unknown";
		private string _browserVersion = "unknown";
		private string _browserType = "unknown";
		private string _os = "unknown";
		private string _ip = "";
		private string _referrer = "";
		private string _referrerKeyword = "";
		private int _sqlSessionId;
		private string _visitorId;
		private bool _isHuman = true;

		#endregion

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }
        
        public static string Request(string key)
		{
			if(HttpContext.Current.Request[key] != null)
				return HttpContext.Current.Request[key];
			else
				return "";
		}

		public Session()
		{

			HttpRequest currentRequest = HttpContext.Current.Request;

			// Update member id
			if(Member.IsLoggedOn())
				_member = new Member(Member.CurrentMemberId()).UniqueId;

			// TODO: Newsletter id

			if(Request("umbNl") != "")
				_newsletter = int.Parse(Request("umbNl"));

			// Add session start time
			_sessionStart = DateTime.Now;

			// Returning user
			if(currentRequest.Cookies["umbracoReturningUser"] != null)
				if(bool.Parse(currentRequest.Cookies["umbracoReturningUser"].Value))
					_returningUser = true;

			// User id
			if(currentRequest.Cookies["umbracoVisitorId"] != null && !string.IsNullOrEmpty(currentRequest.Cookies["umbracoVisitorId"].Value))
				_visitorId = currentRequest.Cookies["umbracoVisitorId"].Value;
			else
				_visitorId = Guid.NewGuid().ToString();

			// Useragent
			if(currentRequest.UserAgent == null)
				_userAgent = "Unknown";
			else
				_userAgent = currentRequest.UserAgent.ToLower();

			// System OS and Browser (based on useragent)
			if(currentRequest.Browser != null)
			{
				_os = currentRequest.Browser.Platform;
				_browserBrand = currentRequest.Browser.Browser;
				_browserVersion = currentRequest.Browser.Version;
				_browserType = currentRequest.Browser.Type;

				// Cookies
				if(currentRequest.Browser.Cookies)
					_cookies = true;
			}

			// Detect bots
			if(_userAgent.IndexOf("bot") > -1 || _browserBrand.ToLower() == "unknown")
				_isHuman = false;

			// Language
			if(currentRequest.UserLanguages != null)
			{
				if(currentRequest.UserLanguages.Length > -1)
					_language = currentRequest.UserLanguages[0];
			}

			// IP
			_ip = currentRequest.UserHostAddress;

			// Referer and keyword
			if(currentRequest.UrlReferrer != null)
			{
				_referrer = currentRequest.UrlReferrer.ToString();
				_referrerKeyword = findKeyword(_referrer);
			}

			this.LogStatSession();

			// Add cookies
			if(Array.IndexOf(HttpContext.Current.Response.Cookies.AllKeys, "umbracoVisitorId") > -1)
				HttpContext.Current.Response.Cookies.Remove("umbracoVisitorId");
			if(Array.IndexOf(HttpContext.Current.Response.Cookies.AllKeys, "umbracoReturningUser") > -1)
				HttpContext.Current.Response.Cookies.Remove("umbracoReturningUser");

			// Add cookie with visitor-id
			HttpCookie c = new HttpCookie("umbracoVisitorId", _visitorId);
			c.Value = _visitorId;
			c.Expires = DateTime.Now.AddYears(1);
			HttpContext.Current.Response.Cookies.Add(c);

			// Add cookie with returning user
			HttpCookie c2 = new HttpCookie("umbracoReturningUser", "true");
			c2.Value = "true";
			c2.Expires = DateTime.Now.AddYears(1);
			HttpContext.Current.Response.Cookies.Add(c2);

			// Check for old session id
			if(HttpContext.Current.Session["umbracoSessionId"] != null)
			{
				_sessionId = new Guid(HttpContext.Current.Session["umbracoSessionId"].ToString());

				// If it exists in the servers session collection, we need to remove it
				if(_sessions.ContainsKey(_sessionId))
					_sessions.Remove(_sessionId);
			}
			else
			{
				// Add guid to visitor
				HttpContext.Current.Session.Add("umbracoSessionId", _sessionId.ToString());
				/*				System.Web.HttpCookie c = new System.Web.HttpCookie("umbracoSessionId");
								c.Expires = DateTime.Now.AddDays(1);
								c.Value = _sessionId.ToString();
								System.Web.HttpContext.Current.Response.Cookies.Add(c);
				*/
			}

			_sessions.Add(_sessionId, this);
		}

		private void LogStatSession()
		{
			// Save to db
			IParameter[] sqlParams = {
				SqlHelper.CreateParameter("@MemberId", this._member),
				SqlHelper.CreateParameter("@NewsletterId", this._newsletter),
				SqlHelper.CreateParameter("@ReturningUser", this._returningUser),
				SqlHelper.CreateParameter("@SessionStart", this._sessionStart),
				SqlHelper.CreateParameter("@Language", this._language),
				SqlHelper.CreateParameter("@UserAgent", this._userAgent),
				SqlHelper.CreateParameter("@Browser", this._browserBrand),
				SqlHelper.CreateParameter("@BrowserVersion", this._browserVersion),
				SqlHelper.CreateParameter("@OperatingSystem", this._os),
				SqlHelper.CreateParameter("@Cookies", this._cookies),
				SqlHelper.CreateParameter("@Ip", this._ip),
				SqlHelper.CreateParameter("@Referrer", this._referrer),
				SqlHelper.CreateParameter("@ReferrerKeyword", this._referrerKeyword),
				SqlHelper.CreateParameter("@visitorId", this._visitorId),
				SqlHelper.CreateParameter("@browserType", this._browserType),
				SqlHelper.CreateParameter("@isHuman", this._isHuman)
			};

			WaitCallback callback =
				delegate
                {
                	try
                	{
                        // cannot synchronize inline delegate, so lock on type
                        lock (GetType())
                        {
                            SqlHelper.ExecuteNonQuery("INSERT INTO umbracoStatSession (MemberId, NewsletterId, ReturningUser, SessionStart, [Language], UserAgent, Browser, BrowserVersion, OperatingSystem, Ip, Referrer, ReferrerKeyword, allowCookies, browserType, visitorId, isHuman) VALUES (@MemberId, @NewsletterId, @ReturningUser, @SessionStart, @Language, @UserAgent, @Browser, @BrowserVersion, @OperatingSystem, @Ip, @Referrer, @ReferrerKeyword, @Cookies, @browserType, @visitorId, @isHuman)", sqlParams);
                            this._sqlSessionId = SqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM umbracoStatSession");
                        }
                	}
                	catch(Exception e)
                	{
						Debug.WriteLine("Error writing to db - umbracoStatSession: " + e);
                	}
                };
			if (GlobalSettings.EnableAsyncStatLogging)
			{
				ThreadPool.QueueUserWorkItem(callback);
			}
			else
			{
				callback(null);
			}
		}

		public void EndSession()
		{
			updateSessionTime();

			// Remove from hashtable
			_sessions.Remove(this._sessionId);
		}

		private void updateSessionTime()
		{
			WaitCallback callback =
				delegate
				{
					try
					{
						SqlHelper.ExecuteNonQuery(
							"update umbracoStatSession set sessionEnd = @sessionEnd where id = @id",
							SqlHelper.CreateParameter("@sessionEnd", DateTime.Now),
							SqlHelper.CreateParameter("@id", _sqlSessionId));
					}
					catch(Exception e)
					{
						Debug.WriteLine("Error writing to db - umbracoStatSession: " + e);
					}
				};
			// Save end time to db
			if(GlobalSettings.EnableAsyncStatLogging)
				ThreadPool.QueueUserWorkItem(callback);
			else
				callback(null);
		}

		private string findKeyword(string refString)
		{
			Match keyword = Regex.Match(refString, @"[\?|\&]q=([^\&]*)|[\?|\&]query=([^\&]*)|[\?|\&]p=([^\&]*)",
					RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
			if(keyword.Success)
				if(keyword.Groups[0].Value != "")
					return keyword.Groups[0].Value.Trim().Replace("?q=", "").Replace("&q=", "");
				else if(keyword.Groups[1].Value != "")
					return keyword.Groups[1].Value.Trim().Replace("?query=", "").Replace("&query=", "");
				else if(keyword.Groups[2].Value != "")
					return keyword.Groups[2].Value.Trim().Replace("?p=", "").Replace("&p=", "");
				else
					// will never be called
					return "";
			else
				return "";
		}

		public void AddEntry(int CurrentPage)
		{
			int LastPage;
			if(_entries.Count > 0)
				LastPage = ((Entry)_entries[_entries.Count - 1]).CurrentPage;
			else
				LastPage = 0;

			Entry e = new Entry(CurrentPage, LastPage);
			_entries.Add(e);

			WaitCallback callback =
				delegate
				{
					try
					{
						SqlHelper.ExecuteNonQuery(
							"insert into umbracoStatEntry (SessionId, EntryTime, RefNodeId, NodeId) values (@SessionId, @EntryTime, @RefNodeId, @NodeId)",
							SqlHelper.CreateParameter("@SessionId", _sqlSessionId),
							SqlHelper.CreateParameter("@EntryTime", e.EntryTime),
							SqlHelper.CreateParameter("@RefNodeId", e.LastPage),
							SqlHelper.CreateParameter("@NodeId", e.CurrentPage));
					}
					catch(Exception e1)
					{
						Debug.WriteLine("Error writing to db - umbracoStatEntry: " + e1);
					}
				};

			// Save to db
			if (GlobalSettings.EnableAsyncStatLogging)
				ThreadPool.QueueUserWorkItem(callback);
			else
				callback(null);

			updateSessionTime();
		}

		public static void AddEntry(Guid SessionId, int CurrentPage)
		{
			// If session doesn't exists then add it
			if(_sessions.ContainsKey(SessionId))
			{
				Session session = _sessions[SessionId] as Session;
				if(session != null)
					session.AddEntry(CurrentPage);
			}
		}

		public static void EndSession(Guid SessionId)
		{
			// If session doesn't exists then add it
			if(_sessions.ContainsKey(SessionId))
			{
				Session session = _sessions[SessionId] as Session;
				if(session != null)
					session.EndSession();
			}
		}
	}
}