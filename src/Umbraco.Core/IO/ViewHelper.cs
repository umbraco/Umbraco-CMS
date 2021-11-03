using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.IO
{
    public class ViewHelper : IViewHelper
    {
        private readonly IFileSystem _viewFileSystem;
        private readonly IDefaultViewContentProvider _defaultViewContentProvider;

        [Obsolete("Use ctor with all params")]
        public ViewHelper(IFileSystem viewFileSystem)
        {
            _viewFileSystem = viewFileSystem ?? throw new ArgumentNullException(nameof(viewFileSystem));
            _defaultViewContentProvider = StaticServiceProvider.Instance.GetRequiredService<IDefaultViewContentProvider>();
        }

        public ViewHelper(FileSystems fileSystems, IDefaultViewContentProvider defaultViewContentProvider)
        {
            _viewFileSystem = fileSystems.MvcViewsFileSystem ?? throw new ArgumentNullException(nameof(fileSystems));
            _defaultViewContentProvider = defaultViewContentProvider ?? throw new ArgumentNullException(nameof(defaultViewContentProvider));
        }

        public bool ViewExists(ITemplate t) => _viewFileSystem.FileExists(ViewPath(t.Alias));
    

        public string GetFileContents(ITemplate t)
        {
            var viewContent = "";
            var path = ViewPath(t.Alias);

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
            var path = ViewPath(t.Alias);

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

        [Obsolete("Inject IDefaultViewContentProvider instead")]
        public static string GetDefaultFileContent(string layoutPageAlias = null, string modelClassName = null,
            string modelNamespace = null, string modelNamespaceAlias = null)
        {
            var viewContentProvider = StaticServiceProvider.Instance.GetRequiredService<IDefaultViewContentProvider>();
            return viewContentProvider.GetDefaultFileContent(layoutPageAlias, modelClassName, modelNamespace,
                modelNamespaceAlias);
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

        public string ViewPath(string alias)
        {
            return _viewFileSystem.GetRelativePath(alias.Replace(" ", "") + ".cshtml");
        }

        private string EnsureInheritedLayout(ITemplate template)
        {
            var design = template.Content;

            if (string.IsNullOrEmpty(design))
                design = _defaultViewContentProvider.GetDefaultFileContent(template.MasterTemplateAlias);

            return design;
        }
    }
}
