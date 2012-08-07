using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// The View that front-end templates inherit from
	/// </summary>
	public abstract class RenderViewPage : WebViewPage<RenderModel>
	{
		//protected RenderViewPage()
		//{

		//}

		//protected override void InitializePage()
		//{
		//    //set the model to the current node if it is not set, this is generally not the case
		//    if (Model != null)
		//    {
		//        //this.ViewData.Model = Model;
		//        var backingItem = new DynamicBackingItem(Model.CurrentNode);
		//        var dynamicNode = new DynamicNode(backingItem);
		//        CurrentPage = dynamicNode;
		//    }
		//}


		//public dynamic CurrentPage { get; private set; }

		//private ICultureDictionary _cultureDictionary;
		//public string GetDictionary(string key)
		//{
		//    if (_cultureDictionary == null)
		//    {
		//        _cultureDictionary = new UmbracoCultureDictionary();
		//    }
		//    return _cultureDictionary[key];
		//}

		//private RazorLibraryCore _library;
		//public RazorLibraryCore Library
		//{
		//    get { return _library ?? (_library = new RazorLibraryCore(Model.CurrentNode)); }
		//}

	}
}