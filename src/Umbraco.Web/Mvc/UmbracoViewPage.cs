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

		protected override void InitializePage()
		{
			base.InitializePage();
			PublishedContentRequest = (PublishedContentRequest)ViewContext.RouteData.DataTokens.GetRequiredObject("umbraco-doc-request");
			UmbracoContext = (UmbracoContext)ViewContext.RouteData.DataTokens.GetRequiredObject("umbraco-context");
			ApplicationContext = UmbracoContext.Application;
		}

		/// <summary>
		/// Returns the current UmbracoContext
		/// </summary>
		public UmbracoContext UmbracoContext { get; private set; }

		/// <summary>
		/// Returns the current ApplicationContext
		/// </summary>
		public ApplicationContext ApplicationContext { get; private set; }

		/// <summary>
		/// Returns the current PublishedContentRequest
		/// </summary>
		internal PublishedContentRequest PublishedContentRequest { get; private set; }		

		private UmbracoHelper _helper;

		/// <summary>
		/// Gets an UmbracoHelper
		/// </summary>
		public UmbracoHelper Umbraco
		{
			get { return _helper ?? (_helper = new UmbracoHelper(UmbracoContext)); }
		}

	}
}