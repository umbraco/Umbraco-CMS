using System.IO;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Web.Macros;

namespace umbraco
{
	[UmbracoWillObsolete("http://issues.umbraco.org/issue/U4-1373", "This will one day be removed when we overhaul the create process")]
	public class MacroPartialViewTasks : PartialViewTasks
	{
		protected override string ParentFolderName
		{
			get { return "MacroPartials"; }
		}

		protected override void WriteTemplateHeader(StreamWriter sw)
		{
			//write out the template header
			sw.Write("@inherits ");
			sw.Write(typeof(PartialViewMacroPage).FullName);
		}
	}
}