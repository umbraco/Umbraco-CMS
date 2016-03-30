using System.Text;
using Umbraco.Core.IO;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;

namespace Umbraco.Web.Trees
{
	/// <summary>
	/// Tree for displaying partial view macros in the developer app
	/// </summary>
	[Tree(Constants.Applications.Developer, Constants.Trees.PartialViewMacros, "Partial View Macro Files", sortOrder: 6)]
	public class PartialViewMacrosTreeController : PartialViewsTreeController
	{
		protected override string FilePath
		{
			get { return SystemDirectories.MvcViews + "/MacroPartials/"; }
		}

	    protected override string EditFormUrl
	    {
	        get { return "Settings/Views/EditView.aspx?treeType=partialViewMacros&file={0}"; }
	    }
	}
}