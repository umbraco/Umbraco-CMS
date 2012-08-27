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
				var dynamicNode = new DynamicDocument(Model.CurrentDocument);
				CurrentPage = dynamicNode.AsDynamic();
			}
			DocumentRequest = (DocumentRequest)ViewContext.RouteData.DataTokens.GetRequiredObject("umbraco-doc-request");
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
		/// Returns the current DocumentRequest
		/// </summary>
		internal DocumentRequest DocumentRequest { get; private set; }

		/// <summary>
		/// Returns the a DynamicDocument object
		/// </summary>
		public dynamic CurrentPage { get; private set; }

		private ICultureDictionary _cultureDictionary;
		public string GetDictionaryValue(string key)
		{
			if (_cultureDictionary == null)
			{
				var factory = CultureDictionaryFactoryResolver.Current.Factory;
				_cultureDictionary = factory.CreateDictionary();
			}
			return _cultureDictionary[key];
		}

		//private RazorLibraryCore _library;
		//public RazorLibraryCore Library
		//{
		//    get { return _library ?? (_library = new RazorLibraryCore(Model.CurrentNode)); }
		//}

	}
}