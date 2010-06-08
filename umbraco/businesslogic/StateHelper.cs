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
        /// Determines whether a cookie has a value with a specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// 	<c>true</c> if the cookie has a  value  with the specified key; otherwise, <c>false</c>.
        /// </returns>
		public static bool HasCookieValue(string key)
		{
			return !string.IsNullOrEmpty(GetCookieValue(HttpContext.Current, key));
		}

        /// <summary>
        /// Gets the cookie value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
		public static string GetCookieValue(string key)
		{
			return GetCookieValue(HttpContext.Current, key);
		}

        /// <summary>
        /// Gets the cookie value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
		public static string GetCookieValue(HttpContext context, string key)
		{
            // Updated by NH to check against session values as well, which is an optional switch used by members
		    string tempValue = null;
			if (context == null || context.Request == null)
				return null;
			
            HttpCookie cookie = context.Request.Cookies[key];
            if (cookie == null) {
                // Check for session
                if (context.Session != null && context.Session[key] != null)
                    if (context.Session[key].ToString() != "0")
                        tempValue = context.Session[key].ToString();
            }
            else
                tempValue = cookie.Value;

			return tempValue;
		}

        /// <summary>
        /// Sets the cookie value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetCookieValue(string key, string value)
        {
            SetCookieValue(HttpContext.Current, key, value);
        }

        /// <summary>
        /// Sets the cookie value including the number of days to persist the cookie
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="daysToPersist">How long the cookie should be present in the browser</param>
        public static void SetCookieValue(string key, string value, double daysToPersist)
        {
            SetCookieValue(HttpContext.Current, key, value, daysToPersist);
        }

        public static void ClearCookie(string key)
        {
            HttpContext ctx = HttpContext.Current;

            if (ctx.Request.Cookies[key] != null)
                ctx.Response.Cookies[key].Expires = DateTime.Now;
        }

        /// <summary>
        /// Sets the cookie value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetCookieValue(HttpContext context, string key, string value)
        {
            SetCookieValue(context, key, value, 30);
        }

        /// <summary>
        /// Sets the cookie value including the number of days to persist the cookie
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="daysToPersist">How long the cookie should be present in the browser</param>
        public static void SetCookieValue(HttpContext context, string key, string value, double daysToPersist)
        {
            if (context == null || context.Request == null)
                return;
            HttpCookie cookie = context.Request.Cookies[key];

            if (cookie == null)
                cookie = new HttpCookie(key);

            cookie.Value = value;

            // add default exp on a month
            cookie.Expires = DateTime.Now.AddDays(daysToPersist);

            // if cookie exists, remove
            context.Response.Cookies.Add(cookie);
        }

        #endregion
	}
}
