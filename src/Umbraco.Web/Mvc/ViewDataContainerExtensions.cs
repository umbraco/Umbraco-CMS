using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
    internal static class ViewDataContainerExtensions
	{
		private class ViewDataContainer : IViewDataContainer
		{
			public ViewDataContainer()
			{
				ViewData = new ViewDataDictionary();
			}
			public ViewDataDictionary ViewData { get; set; }
		}

		/// <summary>
		/// Creates a new IViewDataContainer but with a filtered ModelState
		/// </summary>
		/// <param name="container"></param>
		/// <param name="prefix"></param>
		/// <returns></returns>
		public static IViewDataContainer FilterContainer(this IViewDataContainer container, string prefix)
		{
			var newContainer = new ViewDataContainer();
			newContainer.ViewData.ModelState.Merge(container.ViewData.ModelState, prefix);
            //change the html field name too
		    newContainer.ViewData.TemplateInfo.HtmlFieldPrefix = prefix;
			return newContainer;
		}

		/// <summary>
		/// Returns a new IViewContainer based on the current one but supplies a different model to the ViewData
		/// </summary>
		/// <param name="container"></param>
		/// <param name="model"></param>
		/// <returns></returns>
		public static IViewDataContainer CopyWithModel(this IViewDataContainer container, object model)
		{
			return new ViewDataContainer
				{
					ViewData = new ViewDataDictionary(container.ViewData)
						{
							Model = model
						}
				};
		}

	}
}