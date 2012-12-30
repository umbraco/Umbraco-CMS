using System;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using umbraco.BasePages;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// Ensures authorization occurs for the installer if it has already completed. If install has not yet occured
	/// then the authorization is successful
	/// </summary>
	internal class UmbracoAuthorizeAttribute : AuthorizeAttribute
	{
		private readonly ApplicationContext _applicationContext;

		public UmbracoAuthorizeAttribute(ApplicationContext appContext)
		{
			if (appContext == null) throw new ArgumentNullException("appContext");
			_applicationContext = appContext;
		}

		public UmbracoAuthorizeAttribute()
			: this(ApplicationContext.Current)
		{

		}

		/// <summary>
		/// Ensures that the user must be in the Administrator or the Install role
		/// </summary>
		/// <param name="httpContext"></param>
		/// <returns></returns>
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			if (httpContext == null)
			{
				throw new ArgumentNullException("httpContext");
			}

			try
			{						
				//we need to that the app is configured and that a user is logged in
				if (!_applicationContext.IsConfigured)
					return false;
				var isLoggedIn = BasePage.ValidateUserContextID(BasePage.umbracoUserContextID);
				return isLoggedIn;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Override the OnAuthorization so that we can return a custom response.
		/// </summary>
		/// <param name="filterContext"></param>
		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			Mandate.ParameterNotNull(filterContext, "filterContext");
			if (OutputCacheAttribute.IsChildActionCacheActive(filterContext))
				throw new InvalidOperationException("Cannot use " + typeof(UmbracoAuthorizeAttribute).FullName +  " on a child action");
			if (AuthorizeCore(filterContext.HttpContext))
			{
				//with a little help from dotPeek... this is what it normally would do
				var cache = filterContext.HttpContext.Response.Cache;
				cache.SetProxyMaxAge(new TimeSpan(0L));
				cache.AddValidationCallback(CacheValidateHandler, null);
			}
			else
			{
				//they aren't authorized 
				throw new HttpException((int)global::System.Net.HttpStatusCode.Unauthorized, "You must login to view this resource.");
			}
		}

		private void CacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
		{
			validationStatus = OnCacheAuthorization(new HttpContextWrapper(context));
		}
	}
}