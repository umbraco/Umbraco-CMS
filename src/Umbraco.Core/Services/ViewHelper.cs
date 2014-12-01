using System.IO;
using System.Text;
using Umbraco.Core.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    internal class ViewHelper
    {
        internal static bool ViewExists(ITemplate t)
        {
            string path = GetFilePath(t);
            return System.IO.File.Exists(path);
        }

        internal static string GetFilePath(ITemplate t)
        {
            return IOHelper.MapPath(ViewPath(t.Alias));
        }

        internal static string GetFileContents(ITemplate t)
        {
            string viewContent = "";
            string path = IOHelper.MapPath(ViewPath(t.Alias));

            if (System.IO.File.Exists(path))
            {
                TextReader tr = new StreamReader(path);
                viewContent = tr.ReadToEnd();
                tr.Close();
            }

            return viewContent;
        }

        internal static string CreateViewFile(ITemplate t, bool overWrite = false)
        {
            string viewContent;
            string path = IOHelper.MapPath(ViewPath(t.Alias));

            if (System.IO.File.Exists(path) == false || overWrite)
                viewContent = SaveTemplateToFile(t, t.Alias);
            else
            {
                TextReader tr = new StreamReader(path);
                viewContent = tr.ReadToEnd();
                tr.Close();
            }

            return viewContent;
        }

        internal static string SaveTemplateToFile(ITemplate template, string currentAlias)
        {
            var design = EnsureInheritedLayout(template);
            System.IO.File.WriteAllText(IOHelper.MapPath(ViewPath(template.Alias)), design, Encoding.UTF8);

            return template.Content;
        }

        internal static string UpdateViewFile(ITemplate t, string currentAlias = null)
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
                if (System.IO.File.Exists(oldFile))
                    System.IO.File.Delete(oldFile);
            }

            System.IO.File.WriteAllText(path, t.Content, Encoding.UTF8);
            return t.Content;
        }

        internal static void RemoveViewFile(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias) == false)
            {
                var file = IOHelper.MapPath(ViewPath(alias));
                if (System.IO.File.Exists(file))
                    System.IO.File.Delete(file);
            }
        }

        public static string ViewPath(string alias)
        {
            return SystemDirectories.MvcViews + "/" + alias.Replace(" ", "") + ".cshtml";
        }

        private static string EnsureInheritedLayout(ITemplate template)
        {
            string design = template.Content;

            if (string.IsNullOrEmpty(design))
            {
                design = @"@inherits Umbraco.Web.Mvc.UmbracoTemplatePage
@{
    Layout = null;
}";

                if (template.MasterTemplateAlias.IsNullOrWhiteSpace() == false)
                    design = design.Replace("null", string.Format("\"{0}.cshtml\"", template.MasterTemplateAlias));
            }

            return design;
        }
    }
}