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
        internal static string GetViewFile(Template t)
        {
            string viewContent = "";
            string path = IOHelper.MapPath(ViewPath(t));

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
            string path = IOHelper.MapPath(ViewPath(t));

            if (!File.Exists(path) || overWrite)
                viewContent = saveTemplateToFile(t, t.Alias);
            else
            {
                System.IO.TextReader tr = new StreamReader(path);
                viewContent = tr.ReadToEnd();
                tr.Close();
            }

            return viewContent;
        }

        internal static string saveTemplateToFile(Template template, string currentAlias)
        {
            var design = EnsureInheritedLayout(template);
            System.IO.File.WriteAllText(IOHelper.MapPath(ViewPath(template)), design, Encoding.UTF8);
            
            return template.Design;
        }
        
        internal static string UpdateViewFile(Template t)
        {
            var path = IOHelper.MapPath(ViewPath(t));
            System.IO.File.WriteAllText(path, t.Design, Encoding.UTF8);
            return t.Design;
        }

        public static string ViewPath(Template t)
        {
            return Umbraco.Core.IO.SystemDirectories.MvcViews + "/" + t.Alias.Replace(" ", "") + ".cshtml";
        }



        private static string EnsureInheritedLayout(Template template)
        {
            string design = template.Design; 
            
            if (string.IsNullOrEmpty(design))
            {
                design = @"@inherits Umbraco.Web.Mvc.RenderViewPage
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
