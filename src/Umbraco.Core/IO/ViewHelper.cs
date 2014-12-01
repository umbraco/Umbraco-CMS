using System;
using System.IO;
using System.Text;
using Umbraco.Core.Models;

namespace Umbraco.Core.IO
{
    internal class ViewHelper
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

        //internal string GetFileContents(ITemplate t)
        //{
        //    string viewContent = "";
        //    string path = ViewPath(t.Alias);

        //    if (_viewFileSystem.FileExists(path))
        //    {
        //        using (var tr = new StreamReader(_viewFileSystem.OpenFile(path)))
        //        {
        //            viewContent = tr.ReadToEnd();
        //            tr.Close();
        //        }
        //    }

        //    return viewContent;
        //}

        //internal string CreateViewFile(ITemplate t, bool overWrite = false)
        //{
        //    string viewContent;
        //    string path = ViewPath(t.Alias);

        //    if (_viewFileSystem.FileExists(path) == false || overWrite)
        //    {
        //        viewContent = SaveTemplateToFile(t, t.Alias);   
        //    }                
        //    else
        //    {
        //        using (var tr = new StreamReader(_viewFileSystem.OpenFile(path)))
        //        {
        //            viewContent = tr.ReadToEnd();
        //            tr.Close();
        //        }
        //    }

        //    return viewContent;
        //}

        internal static string GetDefaultFileContent(string layoutPageAlias = null)
        {
            var design = @"@inherits Umbraco.Web.Mvc.UmbracoTemplatePage
@{
    Layout = null;
}";

            if (layoutPageAlias.IsNullOrWhiteSpace() == false)
                design = design.Replace("null", string.Format("\"{0}.cshtml\"", layoutPageAlias));

            return design;
        }

        //internal string SaveTemplateToFile(ITemplate template, string currentAlias)
        //{
        //    var design = EnsureInheritedLayout(template);
        //    var path = ViewPath(template.Alias);

        //    using (var ms = new MemoryStream())
        //    using (var writer = new StreamWriter(ms, Encoding.UTF8))
        //    {
        //        writer.Write(design);
        //        _viewFileSystem.AddFile(path, ms, true);
        //    }

        //    return template.Content;
        //}

        //internal string UpdateViewFile(ITemplate t, string currentAlias = null)
        //{
        //    var path = ViewPath(t.Alias);

        //    if (string.IsNullOrEmpty(currentAlias) == false && currentAlias != t.Alias)
        //    {
        //        //NOTE: I don't think this is needed for MVC, this was ported over from the
        //        // masterpages helper but I think only relates to when templates are stored in the db.
        //        ////Ensure that child templates have the right master masterpage file name
        //        //if (t.HasChildren)
        //        //{
        //        //	var c = t.Children;
        //        //	foreach (CMSNode cmn in c)
        //        //		UpdateViewFile(new Template(cmn.Id), null);
        //        //}

        //        //then kill the old file.. 
        //        var oldFile = ViewPath(currentAlias);
        //        if (_viewFileSystem.FileExists(oldFile))
        //            _viewFileSystem.DeleteFile(oldFile);
        //    }

        //    using (var ms = new MemoryStream())
        //    using (var writer = new StreamWriter(ms, Encoding.UTF8))
        //    {
        //        writer.Write(t.Content);
        //        _viewFileSystem.AddFile(path, ms, true);
        //    }
        //    return t.Content;
        //}

        //internal void RemoveViewFile(string alias)
        //{
        //    if (string.IsNullOrWhiteSpace(alias) == false)
        //    {
        //        var file = ViewPath(alias);
        //        if (_viewFileSystem.FileExists(file))
        //            _viewFileSystem.DeleteFile(file);
        //    }
        //}

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