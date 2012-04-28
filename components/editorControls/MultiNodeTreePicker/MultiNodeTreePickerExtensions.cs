using System.Web.UI;
using umbraco.cms.businesslogic.datatype;

[assembly: WebResource("umbraco.editorControls.MultiNodeTreePicker.jquery.tooltip.min.js", "application/x-javascript")]
[assembly: WebResource("umbraco.editorControls.MultiNodeTreePicker.MultiNodePickerScripts.js", "application/x-javascript")]
[assembly: WebResource("umbraco.editorControls.MultiNodeTreePicker.MultiNodePickerStyles.css", "text/css", PerformSubstitution = true)]

namespace umbraco.editorControls.MultiNodeTreePicker
{
	/// <summary>
	/// Extension methods for this namespace
	/// </summary>
	public static class MultiNodeTreePickerExtensions
	{
		/// <summary>
		/// Adds the JS/CSS required for the MultiNodeTreePicker
		/// </summary>
		/// <param name="ctl"></param>
		public static void AddAllMNTPClientDependencies(this Control ctl)
		{
			//get the urls for the embedded resources
			AddCssMNTPClientDependencies(ctl);
			AddJsMNTPClientDependencies(ctl);
		}

		/// <summary>
		/// Adds the CSS required for the MultiNodeTreePicker
		/// </summary>
		/// <param name="ctl"></param>
		public static void AddCssMNTPClientDependencies(this Control ctl)
		{
			ctl.AddResourceToClientDependency("umbraco.editorControls.MultiNodeTreePicker.MultiNodePickerStyles.css", ClientDependencyType.Css);
		}

		/// <summary>
		/// Adds the JS required for the MultiNodeTreePicker
		/// </summary>
		/// <param name="ctl"></param>
		public static void AddJsMNTPClientDependencies(this Control ctl)
		{
			ctl.AddResourceToClientDependency("umbraco.editorControls.MultiNodeTreePicker.MultiNodePickerScripts.js", ClientDependencyType.Javascript);
			ctl.AddResourceToClientDependency("umbraco.editorControls.MultiNodeTreePicker.jquery.tooltip.min.js", ClientDependencyType.Javascript);
		}
	}
}