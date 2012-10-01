using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Dynamics;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// The View that front-end templates inherit from
	/// </summary>
	public abstract class RenderViewPage : WebViewPage<RenderModel>
	{
		protected RenderViewPage()
		{

		}

		protected override void InitializePage()
		{
			//set the model to the current node if it is not set, this is generally not the case
			if (Model != null)
			{
				////this.ViewData.Model = Model;
				//var backingItem = new DynamicBackingItem(Model.CurrentNode);
				var dynamicNode = new DynamicPublishedContent(Model.CurrentContent);
				CurrentPage = dynamicNode.AsDynamic();
			}
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

		/// <summary>
		/// Returns the a DynamicPublishedContent object
		/// </summary>
		public dynamic CurrentPage { get; private set; }

		/// <summary>
		/// Returns the dictionary value for the key specified
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public string GetDictionaryValue(string key)
		{
			return Umbraco.GetDictionaryValue(key);
		}

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