using System;
using System.Collections.Generic;
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
                System.IO.TextReader tr = new StreamReader(path);
                viewContent = tr.ReadToEnd();
                tr.Close();
            }

            return viewContent;
        }

        internal static string CreateViewFile(Template t, bool overWrite = false)
        {
            string viewContent = "";
			string path = IOHelper.MapPath(ViewPath(t.Alias));

            if (!File.Exists(path) || overWrite)
                viewContent = SaveTemplateToFile(t, t.Alias);
            else
            {
                System.IO.TextReader tr = new StreamReader(path);
                viewContent = tr.ReadToEnd();
                tr.Close();
            }

            return viewContent;
        }

        internal static string SaveTemplateToFile(Template template, string currentAlias)
        {
            var design = EnsureInheritedLayout(template);
            System.IO.File.WriteAllText(IOHelper.MapPath(ViewPath(template.Alias)), design, Encoding.UTF8);
            
            return template.Design;
        }
        
        internal static string UpdateViewFile(Template t, string currentAlias = null)
        {
            var path = IOHelper.MapPath(ViewPath(t.Alias));

			if (!string.IsNullOrEmpty(currentAlias) && currentAlias != t.Alias)
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
				if (System.IO.File.Exists(oldFile))
					System.IO.File.Delete(oldFile);
			}

            System.IO.File.WriteAllText(path, t.Design, Encoding.UTF8);
            return t.Design;
        }

        public static string ViewPath(string alias)
        {
			return Umbraco.Core.IO.SystemDirectories.MvcViews + "/" + alias.Replace(" ", "") + ".cshtml";
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
                    design = design.Replace("null", "\"" + new Template(template.MasterTemplate).Alias.Replace(" ", "") + "\"");

            }

            return design;
        }
    }
}
