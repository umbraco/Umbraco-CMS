using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Umbraco.Core.IO;
using Umbraco.Core;
using umbraco.cms.businesslogic.macro;

namespace Umbraco.Web.UI.Umbraco.Developer.Macros
{
	public partial class EditMacro : global::umbraco.cms.presentation.developer.editMacro
	{

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			PopulatePartialViewFiles();
		}

		/// <summary>
		/// This ensures that the SelectedPartialView txt box value is set correctly when the m_macro object's 
		/// ScriptingFile property contains a full virtual path beginning with the MacroPartials path
		/// </summary>
		/// <param name="macro"> </param>
		/// <param name="macroAssemblyValue"></param>
		/// <param name="macroTypeValue"></param>
		protected override void PopulateFieldsOnLoad(Macro macro, string macroAssemblyValue, string macroTypeValue)
		{
			base.PopulateFieldsOnLoad(macro, macroAssemblyValue, macroTypeValue);
			//check if the ScriptingFile property contains the MacroPartials path
			if (macro.ScriptingFile.StartsWith(SystemDirectories.MvcViews + "/MacroPartials/"))
			{
				macroPython.Text = "";
				SelectedPartialView.Text = Path.GetFileName(macro.ScriptingFile);
			}
		}

		/// <summary>
		/// This changes the macro type to a PartialViewMacro if the SelectedPartialView txt box has a value.
		/// This then also updates the file path saved for the partial view to be the full virtual path, not just the file name.
		/// </summary>
		/// <param name="macro"> </param>
		/// <param name="macroCachePeriod"></param>
		/// <param name="macroAssemblyValue"></param>
		/// <param name="macroTypeValue"></param>
		protected override void SetMacroValuesFromPostBack(Macro macro, int macroCachePeriod, string macroAssemblyValue, string macroTypeValue)
		{
			base.SetMacroValuesFromPostBack(macro, macroCachePeriod, macroAssemblyValue, macroTypeValue);
			if (!SelectedPartialView.Text.IsNullOrWhiteSpace())
			{
				macro.ScriptingFile = SystemDirectories.MvcViews + "/MacroPartials/" + SelectedPartialView.Text;
			}
		}

		/// <summary>
		/// Populate the drop down list for partial view files
		/// </summary>
		private void PopulatePartialViewFiles()
		{
			var partialsDir = IOHelper.MapPath(SystemDirectories.MvcViews + "/MacroPartials");
			var views = GetPartialViewFiles(partialsDir, partialsDir);
			PartialViewList.DataSource = views;
			PartialViewList.DataBind();
			PartialViewList.Items.Insert(0, new ListItem("Browse partial view files on server...", string.Empty));			
		}

		/// <summary>
		/// Get the list of partial view files in the ~/Views/MacroPartials folder
		/// </summary>
		/// <param name="orgPath"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		private IEnumerable<string> GetPartialViewFiles(string orgPath, string path)
		{
			var files = new List<string>();
			var dirInfo = new DirectoryInfo(path);

			// Populate subdirectories
			var dirInfos = dirInfo.GetDirectories();
			foreach (var dir in dirInfos)
			{
				files.AddRange(GetPartialViewFiles(orgPath, path + "/" + dir.Name));
			}

			var fileInfo = dirInfo.GetFiles("*.*");

			files.AddRange(fileInfo.Select(file => (path.Replace(orgPath, string.Empty).Trim('/') + "/" + file.Name).Trim('/')));
			return files;
		}

	}
}