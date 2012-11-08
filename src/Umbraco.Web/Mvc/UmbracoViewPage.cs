using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// The View that umbraco front-end views inherit from
	/// </summary>
	public abstract class UmbracoViewPage<T> : WebViewPage<T>
	{
		protected UmbracoViewPage()
		{

		}
		
		/// <summary>
		/// Returns the current UmbracoContext
		/// </summary>
		public UmbracoContext UmbracoContext
		{
			get { return (UmbracoContext) ViewContext.RouteData.DataTokens.GetRequiredObject("umbraco-context"); }
		}

		/// <summary>
		/// Returns the current ApplicationContext
		/// </summary>
		public ApplicationContext ApplicationContext
		{
			get { return UmbracoContext.Application; }
		}

		/// <summary>
		/// Returns the current PublishedContentRequest
		/// </summary>
		internal PublishedContentRequest PublishedContentRequest
		{
			get { return (PublishedContentRequest)ViewContext.RouteData.DataTokens.GetRequiredObject("umbraco-doc-request"); }
		}

		private UmbracoHelper _helper;

		/// <summary>
		/// Gets an UmbracoHelper
		/// </summary>
		/// <remarks>
		/// This constructs the UmbracoHelper with the content model of the page routed to
		/// </remarks>
		public virtual UmbracoHelper Umbraco
		{
			get { return _helper ?? (_helper = new UmbracoHelper(UmbracoContext)); }
		}

	}
}