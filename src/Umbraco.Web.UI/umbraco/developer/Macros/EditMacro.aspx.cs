using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Umbraco.Core.IO;

namespace Umbraco.Web.UI.Umbraco.Developer.Macros
{
	public partial class EditMacro : global::umbraco.cms.presentation.developer.editMacro
	{

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			PopulatePartialViewFiles();
		}

		private void PopulatePartialViewFiles()
		{
			var partialsDir = IOHelper.MapPath(SystemDirectories.MvcViews + "/MacroPartials");
			var views = GetPartialViewFiles(partialsDir, partialsDir);
			PartialViewList.DataSource = views;
			PartialViewList.DataBind();
			PartialViewList.Items.Insert(0, new ListItem("Browse partial view files on server...", string.Empty));			
		}

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