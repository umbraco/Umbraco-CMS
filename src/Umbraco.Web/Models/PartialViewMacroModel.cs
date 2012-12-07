using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models
{
	/// <summary>
	/// The model used when rendering Partial View Macros
	/// </summary>
	public class PartialViewMacroModel
	{

		public PartialViewMacroModel(IPublishedContent page, IDictionary<string, object> macroParams)
		{
			CurrentPage = page;
			MacroParameters = macroParams;
		}

		public IPublishedContent CurrentPage { get; private set; }

		public IDictionary<string, object> MacroParameters { get; private set; }

	}
}