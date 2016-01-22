using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Core.Models;

namespace Umbraco.Core.IO
{
    public class ViewHelper
    {
        private readonly IFileSystem _viewFileSystem;

        public ViewHelper(IFileSystem viewFileSystem)
        {
            if (viewFileSystem == null) throw new ArgumentNullException("viewFileSystem");
            _viewFileSystem = viewFileSystem;
        }

        internal bool ViewExists(ITemplate t)
        {
            return _viewFileSystem.FileExists(ViewPath(t.Alias));
        }

        [Obsolete("This is only used for legacy purposes and will be removed in future versions")]
        internal string GetPhysicalFilePath(ITemplate t)
        {
            return _viewFileSystem.GetFullPath(ViewPath(t.Alias));
        }

        internal string GetFileContents(ITemplate t)
        {
            string viewContent = "";
            string path = ViewPath(t.Alias);

            if (_viewFileSystem.FileExists(path))
            {
                using (var tr = new StreamReader(_viewFileSystem.OpenFile(path)))
                {
                    viewContent = tr.ReadToEnd();
                    tr.Close();
                }
            }

            return viewContent;
        }

        public string CreateView(ITemplate t, bool overWrite = false)
        {
            string viewContent;
            string path = ViewPath(t.Alias);

            if (_viewFileSystem.FileExists(path) == false || overWrite)
            {
                viewContent = SaveTemplateToFile(t);   
            }                
            else
            {
                using (var tr = new StreamReader(_viewFileSystem.OpenFile(path)))
                {
                    viewContent = tr.ReadToEnd();
                    tr.Close();
                }
            }

            return viewContent;
        }

        public static string GetDefaultFileContent(string layoutPageAlias = null, string modelClassName = null, string modelNamespace = null, string modelNamespaceAlias = null)
        {
            var content = new StringBuilder();

            if (string.IsNullOrWhiteSpace(modelNamespaceAlias))
                modelNamespaceAlias = "ContentModels";

            // either
            // @inherits Umbraco.Web.Mvc.UmbracoTemplatePage
            // @inherits Umbraco.Web.Mvc.UmbracoTemplatePage<ModelClass>
            // @inherits Umbraco.Web.Mvc.UmbracoTemplatePage<ContentModels.ModelClass>
            content.Append("@inherits Umbraco.Web.Mvc.UmbracoTemplatePage");
            if (modelClassName.IsNullOrWhiteSpace() == false)
            {
                content.Append("<");
                if (modelNamespace.IsNullOrWhiteSpace() == false)
                {
                    content.Append(modelNamespaceAlias);
                    content.Append(".");
                }
                content.Append(modelClassName);
                content.Append(">");
            }
            content.Append("\r\n");

            // if required, add
            // @using ContentModels = ModelNamespace;
            if (modelClassName.IsNullOrWhiteSpace() == false && modelNamespace.IsNullOrWhiteSpace() == false)
            {
                content.Append("@using ");
                content.Append(modelNamespaceAlias);
                content.Append(" = ");
                content.Append(modelNamespace);
                content.Append(";\r\n");
            }

            // either
            // Layout = null;
            // Layout = "layoutPage.cshtml";
            content.Append("@{\r\n\tLayout = ");
            if (layoutPageAlias.IsNullOrWhiteSpace())
            {
                content.Append("null");
            }
            else
            {
                content.Append("\"");
                content.Append(layoutPageAlias);
                content.Append(".cshtml\"");
            }
            content.Append(";\r\n}");
            return content.ToString();
        }

        private string SaveTemplateToFile(ITemplate template)
        {
            var design = template.Content.IsNullOrWhiteSpace() ? EnsureInheritedLayout(template) : template.Content;
            var path = ViewPath(template.Alias);

            var data = Encoding.UTF8.GetBytes(design);
            var withBom = Encoding.UTF8.GetPreamble().Concat(data).ToArray();

            using (var ms = new MemoryStream(withBom))
            {
                _viewFileSystem.AddFile(path, ms, true);
            }

            return design;
        }

        public string UpdateViewFile(ITemplate t, string currentAlias = null)
        {
            var path = ViewPath(t.Alias);

            if (string.IsNullOrEmpty(currentAlias) == false && currentAlias != t.Alias)
            {
                //then kill the old file.. 
                var oldFile = ViewPath(currentAlias);
                if (_viewFileSystem.FileExists(oldFile))
                    _viewFileSystem.DeleteFile(oldFile);
            }

            var data = Encoding.UTF8.GetBytes(t.Content);
            var withBom = Encoding.UTF8.GetPreamble().Concat(data).ToArray();

            using (var ms = new MemoryStream(withBom))
            {
                _viewFileSystem.AddFile(path, ms, true);
            }
            return t.Content;
        }

        internal void RemoveViewFile(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias) == false)
            {
                var file = ViewPath(alias);
                if (_viewFileSystem.FileExists(file))
                    _viewFileSystem.DeleteFile(file);
            }
        }

        public string ViewPath(string alias)
        {
            return _viewFileSystem.GetRelativePath(alias.Replace(" ", "") + ".cshtml");

            //return SystemDirectories.MvcViews + "/" + alias.Replace(" ", "") + ".cshtml";
        }

        private string EnsureInheritedLayout(ITemplate template)
        {
            string design = template.Content;

            if (string.IsNullOrEmpty(design))
            {
                design = GetDefaultFileContent(template.MasterTemplateAlias);
            }

            return design;
        }
    }
}