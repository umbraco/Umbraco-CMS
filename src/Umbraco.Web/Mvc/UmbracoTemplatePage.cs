using Umbraco.Core.Dictionary;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// The View that front-end templates inherit from
	/// </summary>
	public abstract class UmbracoTemplatePage : UmbracoViewPage<RenderModel>
	{
		protected UmbracoTemplatePage()
		{

		}

		protected override void InitializePage()
		{
			base.InitializePage();
			//set the model to the current node if it is not set, this is generally not the case
			if (Model != null)
			{
				////this.ViewData.Model = Model;
				//var backingItem = new DynamicBackingItem(Model.CurrentNode);
				var dynamicNode = new DynamicPublishedContent(Model.Content);
				CurrentPage = dynamicNode.AsDynamic();
			}
		}

		/// <summary>
		/// Returns the a DynamicPublishedContent object
		/// </summary>
		public dynamic CurrentPage { get; private set; }		

	}
}