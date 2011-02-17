using System;
using System.Reflection;
using System.Web;
using System.Web.UI;

namespace umbraco.BusinessLogic
{
    /// <summary>
    /// The StateHelper class provides general helper methods for handling sessions, context, viewstate and cookies.
    /// </summary>
	public class StateHelper
	{
		#region Session Helpers

        /// <summary>
        /// Gets the session value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
		public static T GetSessionValue<T>(string key)
		{
			return GetSessionValue<T>(HttpContext.Current, key);
		}

        /// <summary>
        /// Gets the session value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
		public static T GetSessionValue<T>(HttpContext context, string key)
		{
			if (context == null)
				return default(T);
			object o = context.Session[key];
			if (o == null)
				return default(T);
			return (T)o;
		}

        /// <summary>
        /// Sets a session value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
		public static void SetSessionValue(string key, object value)
		{
			SetSessionValue(HttpContext.Current, key, value);
		}

        /// <summary>
        /// Sets the session value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
		public static void SetSessionValue(HttpContext context, string key, object value)
		{
			if (context == null)
				return;
			context.Session[key] = value;
		}

		#endregion

		#region Context Helpers

        /// <summary>
        /// Gets the context value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
		public static T GetContextValue<T>(string key)
		{
			return GetContextValue<T>(HttpContext.Current, key);
		}

        /// <summary>
        /// Gets the context value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
		public static T GetContextValue<T>(HttpContext context, string key)
		{
			if (context == null)
				return default(T);
			object o = context.Items[key];
			if (o == null)
				return default(T);
			return (T)o;
		}

        /// <summary>
        /// Sets the context value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
		public static void SetContextValue(string key, object value)
		{
			SetContextValue(HttpContext.Current, key, value);
		}

        /// <summary>
        /// Sets the context value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
		public static void SetContextValue(HttpContext context, string key, object value)
		{
			if (context == null)
				return;
			context.Items[key] = value;
		}

		#endregion

		#region ViewState Helpers

        /// <summary>
        /// Gets the state bag.
        /// </summary>
        /// <returns></returns>
		private static StateBag GetStateBag()
		{
			if (HttpContext.Current == null)
				return null;

			Page page = HttpContext.Current.CurrentHandler as Page;
			if (page == null)
				return null;

			Type pageType = typeof(Page);
			PropertyInfo viewState = pageType.GetProperty("ViewState", BindingFlags.GetProperty | BindingFlags.Instance);
			if (viewState == null)
				return null;

			return viewState.GetValue(page, null) as StateBag;
		}

        /// <summary>
        /// Gets the view state value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
		public static T GetViewStateValue<T>(string key)
		{
			return GetViewStateValue<T>(GetStateBag(), key);
		}

        /// <summary>
        /// Gets a view-state value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bag">The bag.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
		public static T GetViewStateValue<T>(StateBag bag, string key)
		{
			if (bag == null)
				return default(T);
			object o = bag[key];
			if (o == null)
				return default(T);
			return (T)o;
		}

        /// <summary>
        /// Sets the view state value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
		public static void SetViewStateValue(string key, object value)
		{
			SetViewStateValue(GetStateBag(), key, value);
		}

        /// <summary>
        /// Sets the view state value.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
		public static void SetViewStateValue(StateBag bag, string key, object value)
		{
			if (bag != null)
				bag[key] = value;
		}

		#endregion

		#region Cookie Helpers

        /// <summary>
        /// Gets the cookie value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
		public static string GetCookieValue(string key)
		{
			if (!Cookies.HasCookies)
				return null;
			var cookie = HttpContext.Current.Request.Cookies[key];
			return cookie == null ? null : cookie.Value;
		}

        /// <summary>
        /// Sets the cookie value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetCookieValue(string key, string value)
        {
			SetCookieValue(key, value, 30d); // default Umbraco expires is 30 days
        }

        /// <summary>
        /// Sets the cookie value including the number of days to persist the cookie
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="daysToPersist">How long the cookie should be present in the browser</param>
        public static void SetCookieValue(string key, string value, double daysToPersist)
        {
			if (!Cookies.HasCookies)
				return;
			var context = HttpContext.Current;

			HttpCookie cookie = new HttpCookie(key, value);
			cookie.Expires = DateTime.Now.AddDays(daysToPersist);
			context.Response.Cookies.Set(cookie);

			cookie = context.Request.Cookies[key];
			if (cookie != null)
				cookie.Value = value;
		}

		// zb-00004 #29956 : refactor cookies names & handling
		public static class Cookies
		{
			/*
			 * helper class to manage cookies
			 * 
			 * beware! SetValue(string value) does _not_ set expires, unless the cookie has been
			 * configured to have one. This allows us to have cookies w/out an expires timespan.
			 * However, default behavior in Umbraco was to set expires to 30days by default. This
			 * must now be managed in the Cookie constructor or by using an overriden SetValue(...).
			 * 
			 * we currently reproduce this by configuring each cookie with a 30d expires, but does
			 * that actually make sense? shouldn't some cookie have _no_ expires?
			 */
			static readonly Cookie _preview = new Cookie("UMB_PREVIEW", 30d); // was "PreviewSet"
			static readonly Cookie _userContext = new Cookie("UMB_UCONTEXT", 30d); // was "UserContext"
			static readonly Cookie _member = new Cookie("UMB_MEMBER", 30d); // was "umbracoMember"

			public static Cookie Preview { get { return _preview; } }
			public static Cookie UserContext { get { return _userContext; } }
			public static Cookie Member { get { return _member; } }

			public static bool HasCookies
			{
				get
				{
					System.Web.HttpContext context = HttpContext.Current;
					// although just checking context should be enough?!
					// but in some (replaced) umbraco code, everything is checked...
					return context != null 
						&& context.Request != null & context.Request.Cookies != null 
						&& context.Response != null && context.Response.Cookies != null;
				}
			}

			public static void ClearAll()
			{
				HttpContext.Current.Response.Cookies.Clear();
			}

			public class Cookie
			{
				const string cookiesExtensionConfigKey = "umbracoCookiesExtension";

				static readonly string _ext;
				TimeSpan _expires;
				string _key;

				static Cookie()
				{
					var appSettings = System.Configuration.ConfigurationManager.AppSettings;
					_ext = appSettings[cookiesExtensionConfigKey] == null ? "" : "_" + (string)appSettings[cookiesExtensionConfigKey];
				}

				public Cookie(string key)
					: this(key, TimeSpan.Zero, true)
				{ }

				public Cookie(string key, double days)
					: this(key, TimeSpan.FromDays(days), true)
				{ }

				public Cookie(string key, TimeSpan expires)
					: this(key, expires, true)
				{ }

				public Cookie(string key, bool appendExtension)
					: this(key, TimeSpan.Zero, appendExtension)
				{ }

				public Cookie(string key, double days, bool appendExtension)
					: this(key, TimeSpan.FromDays(days), appendExtension)
				{ }

				public Cookie(string key, TimeSpan expires, bool appendExtension)
				{
					_key = appendExtension ? key + _ext : key;
					_expires = expires;
				}

				public string Key
				{
					get { return _key; }
				}

				public bool HasValue
				{
					get { return RequestCookie != null; }
				}

				public string GetValue()
				{
					return RequestCookie == null ? null : RequestCookie.Value;
				}

				public void SetValue(string value)
				{
					HttpCookie cookie = new HttpCookie(_key, value);
					if (!TimeSpan.Zero.Equals(_expires))
						cookie.Expires = DateTime.Now + _expires;
					ResponseCookie = cookie;

					// original Umbraco code also does this
					// so we can GetValue() back what we previously set
					cookie = RequestCookie;
					if (cookie != null)
						cookie.Value = value;
				}

				public void SetValue(string value, double days)
				{
					SetValue(value, DateTime.Now.AddDays(days));
				}

				public void SetValue(string value, TimeSpan expires)
				{
					SetValue(value, DateTime.Now + expires);
				}

				public void SetValue(string value, DateTime expires)
				{
					HttpCookie cookie = new HttpCookie(_key, value);
					cookie.Expires = expires;
					ResponseCookie = cookie;

					// original Umbraco code also does this
					// so we can GetValue() back what we previously set
					cookie = RequestCookie;
					if (cookie != null)
						cookie.Value = value;
				}

				public void Clear()
				{
					if (RequestCookie != null || ResponseCookie != null)
					{
						HttpCookie cookie = new HttpCookie(_key);
						cookie.Expires = DateTime.Now.AddDays(-1);
						ResponseCookie = cookie;
					}
				}

				public void Remove()
				{
					// beware! will not clear browser's cookie
					// you probably want to use .Clear()
					HttpContext.Current.Response.Cookies.Remove(_key);
				}

				public HttpCookie RequestCookie
				{
					get
					{
						return HttpContext.Current.Request.Cookies[_key];
					}
				}

				public HttpCookie ResponseCookie
				{
					get
					{
						return HttpContext.Current.Response.Cookies[_key];
					}
					set
					{
						// .Set() ensures the uniqueness of cookies in the cookie collection
						// ie it is the same as .Remove() + .Add() -- .Add() allows duplicates
						HttpContext.Current.Response.Cookies.Set(value);
					}
				}
			}
		}

        #endregion
	}
}
