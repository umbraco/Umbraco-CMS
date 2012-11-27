using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.template
{
    class ViewHelper
    {
		internal static bool ViewExists(Template t)
		{
			string path = GetFilePath(t);
			return File.Exists(path);
		}

		internal static string GetFilePath(Template t)
		{
			return IOHelper.MapPath(ViewPath(t.Alias));
		}

        internal static string GetFileContents(Template t)
        {
            string viewContent = "";
			string path = IOHelper.MapPath(ViewPath(t.Alias));

            if (File.Exists(path))
            {
                TextReader tr = new StreamReader(path);
                viewContent = tr.ReadToEnd();
                tr.Close();
            }

            return viewContent;
        }

        internal static string CreateViewFile(Template t, bool overWrite = false)
        {
            string viewContent;
			string path = IOHelper.MapPath(ViewPath(t.Alias));

            if (File.Exists(path) == false || overWrite)
                viewContent = SaveTemplateToFile(t, t.Alias);
            else
            {
                TextReader tr = new StreamReader(path);
                viewContent = tr.ReadToEnd();
                tr.Close();
            }

            return viewContent;
        }

        internal static string SaveTemplateToFile(Template template, string currentAlias)
        {
            var design = EnsureInheritedLayout(template);
            File.WriteAllText(IOHelper.MapPath(ViewPath(template.Alias)), design, Encoding.UTF8);
            
            return template.Design;
        }
        
        internal static string UpdateViewFile(Template t, string currentAlias = null)
        {
            var path = IOHelper.MapPath(ViewPath(t.Alias));

			if (string.IsNullOrEmpty(currentAlias) == false && currentAlias != t.Alias)
			{
				//NOTE: I don't think this is needed for MVC, this was ported over from the
				// masterpages helper but I think only relates to when templates are stored in the db.
				////Ensure that child templates have the right master masterpage file name
				//if (t.HasChildren)
				//{
				//	var c = t.Children;
				//	foreach (CMSNode cmn in c)
				//		UpdateViewFile(new Template(cmn.Id), null);
				//}

				//then kill the old file.. 
				var oldFile = IOHelper.MapPath(ViewPath(currentAlias));
				if (File.Exists(oldFile))
					File.Delete(oldFile);
			}

            File.WriteAllText(path, t.Design, Encoding.UTF8);
            return t.Design;
        }

		internal static void RemoveViewFile(string alias)
		{
			if (string.IsNullOrWhiteSpace(alias) == false)
			{
				var file = IOHelper.MapPath(ViewPath(alias));
				if (File.Exists(file))
                    File.Delete(file);
			}
		}

        public static string ViewPath(string alias)
        {
			return SystemDirectories.MvcViews + "/" + alias.Replace(" ", "") + ".cshtml";
        }

        private static string EnsureInheritedLayout(Template template)
        {
            string design = template.Design; 
            
            if (string.IsNullOrEmpty(design))
            {
                design = @"@inherits Umbraco.Web.Mvc.UmbracoTemplatePage
                         @{
                             Layout = null;
                         }";

                if (template.MasterTemplate > 0)
                    design = design.Replace("null", string.Format("\"{0}.cshtml\"", new Template(template.MasterTemplate).Alias.Replace(" ", "")));
            }

            return design;
        }
    }
}
