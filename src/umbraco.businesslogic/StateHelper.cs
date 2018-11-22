using System;
using System.Reflection;
using System.Web;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace umbraco.BusinessLogic
{
    /// <summary>
    /// The StateHelper class provides general helper methods for handling sessions, context, viewstate and cookies.
    /// </summary>
    [Obsolete("DO NOT USE THIS ANYMORE! REPLACE ALL CALLS WITH NEW EXTENSION METHODS")]
    public class StateHelper
    {

        private static HttpContextBase _customHttpContext;

        /// <summary>
        /// Gets/sets the HttpContext object, this is generally used for unit testing. By default this will 
        /// use the HttpContext.Current
        /// </summary>
        internal static HttpContextBase HttpContext
        {
            get
            {
                if (_customHttpContext == null && System.Web.HttpContext.Current != null)
                {
                    //return the current HttpContxt, do NOT store this in the _customHttpContext field
                    //as it will persist across reqeusts!
                    return new HttpContextWrapper(System.Web.HttpContext.Current);
                }

                if (_customHttpContext == null && System.Web.HttpContext.Current == null)
                {
                    throw new NullReferenceException("The HttpContext property has not been set or the object execution is not running inside of an HttpContext");
                }
                return _customHttpContext;
            }
            set { _customHttpContext = value; }
        }

        #region Session Helpers

        /// <summary>
        /// Gets the session value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static T GetSessionValue<T>(string key)
        {
            return GetSessionValue<T>(HttpContext, key);
        }

        /// <summary>
        /// Gets the session value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [Obsolete("Use the GetSessionValue accepting an HttpContextBase instead")]
        public static T GetSessionValue<T>(HttpContext context, string key)
        {
            return GetSessionValue<T>(new HttpContextWrapper(context), key);
        }

        /// <summary>
        /// Gets the session value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static T GetSessionValue<T>(HttpContextBase context, string key)
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
            SetSessionValue(HttpContext, key, value);
        }

        /// <summary>
        /// Sets the session value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        [Obsolete("Use the SetSessionValue accepting an HttpContextBase instead")]
        public static void SetSessionValue(HttpContext context, string key, object value)
        {
            SetSessionValue(new HttpContextWrapper(context), key, value);
        }

        /// <summary>
        /// Sets the session value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetSessionValue(HttpContextBase context, string key, object value)
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
            return GetContextValue<T>(HttpContext, key);
        }

        /// <summary>
        /// Gets the context value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [Obsolete("Use the GetContextValue accepting an HttpContextBase instead")]
        public static T GetContextValue<T>(HttpContext context, string key)
        {
            return GetContextValue<T>(new HttpContextWrapper(context), key);
        }

        /// <summary>
        /// Gets the context value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static T GetContextValue<T>(HttpContextBase context, string key)
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
            SetContextValue(HttpContext, key, value);
        }

        /// <summary>
        /// Sets the context value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        [Obsolete("Use the SetContextValue accepting an HttpContextBase instead")]
        public static void SetContextValue(HttpContext context, string key, object value)
        {
            SetContextValue(new HttpContextWrapper(context), key, value);
        }

        /// <summary>
        /// Sets the context value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void SetContextValue(HttpContextBase context, string key, object value)
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
            //if (HttpContext.Current == null)
            //    return null;

            var page = HttpContext.CurrentHandler as Page;
            if (page == null)
                return null;

            var pageType = typeof(Page);
            var viewState = pageType.GetProperty("ViewState", BindingFlags.GetProperty | BindingFlags.Instance);
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
        [Obsolete("Use !string.IsNullOrEmpty(GetCookieValue(key))", false)]
        public static bool HasCookieValue(string key)
        {
            return !string.IsNullOrEmpty(GetCookieValue(key));
        }



        /// <summary>
        /// Gets the cookie value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetCookieValue(string key)
        {
            if (!Cookies.HasCookies)
                return null;
            var cookie = HttpContext.Request.Cookies[key];
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
            var context = HttpContext;

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
            static readonly Cookie _preview = new Cookie(Constants.Web.PreviewCookieName, TimeSpan.Zero); // was "PreviewSet"
            static readonly Cookie _userContext = new Cookie(UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName, 30d); // was "UserContext"
            static readonly Cookie _member = new Cookie("UMB_MEMBER", 30d); // was "umbracoMember"

            public static Cookie Preview { get { return _preview; } }
            public static Cookie UserContext { get { return _userContext; } }
            public static Cookie Member { get { return _member; } }

            public static bool HasCookies
            {
                get
                {
                    var context = HttpContext;
                    // although just checking context should be enough?!
                    // but in some (replaced) umbraco code, everything is checked...
                    return context != null
                        && context.Request != null & context.Request.Cookies != null
                        && context.Response != null && context.Response.Cookies != null;
                }
            }

            public static void ClearAll()
            {
                HttpContext.Response.Cookies.Clear();
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
                    SetValueWithDate(value, _expires == TimeSpan.Zero ? DateTime.MinValue : DateTime.Now + _expires);
                }

                public void SetValue(string value, double days)
                {
                    SetValue(value, DateTime.Now.AddDays(days));
                }

                public void SetValue(string value, TimeSpan expires)
                {
                    SetValue(value, expires == TimeSpan.Zero ? DateTime.MinValue : DateTime.Now + expires);
                }

                public void SetValue(string value, DateTime expires)
                {
                    SetValueWithDate(value, expires);
                }

                private void SetValueWithDate(string value, DateTime expires)
                {
                    var cookie = new HttpCookie(_key, value);

                    if (GlobalSettings.UseSSL)
                        cookie.Secure = true;

                    //ensure http only, this should only be able to be accessed via the server
                    cookie.HttpOnly = true;

                    //set an expiry date if not min value, otherwise leave it as a session cookie.
                    if (expires != DateTime.MinValue)
                    {
                        cookie.Expires = expires;    
                    }
                    
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
                        var cookie = new HttpCookie(_key);
                        cookie.Expires = DateTime.Now.AddDays(-1);
                        ResponseCookie = cookie;
                    }
                }

                public void Remove()
                {
                    // beware! will not clear browser's cookie
                    // you probably want to use .Clear()
                    HttpContext.Response.Cookies.Remove(_key);
                }

                public HttpCookie RequestCookie
                {
                    get
                    {
                        return HttpContext.Request.Cookies[_key];
                    }
                }

                public HttpCookie ResponseCookie
                {
                    get
                    {
                        return HttpContext.Response.Cookies[_key];
                    }
                    set
                    {
                        // .Set() ensures the uniqueness of cookies in the cookie collection
                        // ie it is the same as .Remove() + .Add() -- .Add() allows duplicates
                        HttpContext.Response.Cookies.Set(value);
                    }
                }
            }
        }

        #endregion
    }
}
