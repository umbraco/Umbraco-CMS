using System;
using System.IO;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.UI;
using umbraco.BasePages;
using Umbraco.Core;

namespace umbraco
{
    /// <summary>
    /// The base UI 'tasks' for the create dialog and delete processes
    /// </summary>
    [UmbracoWillObsolete("http://issues.umbraco.org/issue/U4-1373", "This will one day be removed when we overhaul the create process")]
    public abstract class PartialViewTasksBase : LegacyDialogTask
    {
        private string _returnUrl = string.Empty;

        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        protected virtual string EditViewFile
        {
            get { return "Settings/Views/EditView.aspx"; }
        }

        protected virtual bool IsPartialViewMacro
        {
            get { return false; }
        }

        public override bool PerformSave()
        {
            var pipesIndex = Alias.IndexOf("|||", StringComparison.Ordinal);
            var snippetName = Alias.Substring(0, pipesIndex).Trim();
            var fileName = Alias.Substring(pipesIndex + 3, Alias.Length - pipesIndex - 3);
            if (fileName.ToLowerInvariant().EndsWith(".cshtml") == false)
            {
                fileName += ".cshtml";
            }

            var model = new PartialView(fileName);
            var fileService = (FileService)ApplicationContext.Current.Services.FileService;
            var macroService = ApplicationContext.Current.Services.MacroService;

            if (IsPartialViewMacro == false)
            {
                var attempt = fileService.CreatePartialView(model, snippetName, User.Id);

                //TODO: We currently need to hack this because we are using the same editor for views, partial views, partial view macros and 
                // the editor is using normal UI whereas the partial view repo and these classes are using IFileSystem with relative references
                // so the model.Path is a relative reference to the ~/Views/Partials folder, we need to ensure it's prefixed with "Partials/"

                _returnUrl = string.Format("settings/views/EditView.aspx?treeType=partialViews&file={0}", model.Path.TrimStart('/').EnsureStartsWith("Partials/"));
                return attempt.Success;
            }
            else
            {

                var attempt = fileService.CreatePartialViewMacro(model, /*ParentID == 1,*/ snippetName, User.Id);
                // if ParentId = 0 then that means that the "Create macro" checkbox was OFF, so don't try to create an actual macro
                // See PartialViewMacro.ascx.cs and PartialView.ascx.cs: SubmitButton_Click
                if (attempt && ParentID != 0)
                {
                    //The partial view path to be saved with the macro must be a fully qualified virtual path
                    var virtualPath = string.Format("{0}/{1}/{2}", SystemDirectories.MvcViews, "MacroPartials", attempt.Result.Path);
                    macroService.Save(new Macro(attempt.Result.Alias, attempt.Result.Alias) { ScriptPath = virtualPath });
                }

                //TODO: We currently need to hack this because we are using the same editor for views, partial views, partial view macros and 
                // the editor is using normal UI whereas the partial view repo and these classes are using IFileSystem with relative references
                // so the model.Path is a relative reference to the ~/Views/Partials folder, we need to ensure it's prefixed with "MacroPartials/"

                _returnUrl = string.Format("settings/views/EditView.aspx?treeType=partialViewMacros&file={0}", model.Path.TrimStart('/').EnsureStartsWith("MacroPartials/"));
                return attempt.Success;
            }

        }

        public override bool PerformDelete()
        {
            var fileService = (FileService)ApplicationContext.Current.Services.FileService;

            if (IsPartialViewMacro == false)
            {
                if (Alias.Contains(".") == false)
                {
                    //there is no extension so we'll assume it's a folder
                    fileService.DeletePartialViewFolder(Alias.TrimStart('/'));
                    return true;
                }
                return fileService.DeletePartialView(Alias.TrimStart('/'), User.Id);
            }

            if (Alias.Contains(".") == false)
            {
                //there is no extension so we'll assume it's a folder
                fileService.DeletePartialViewMacroFolder(Alias.TrimStart('/'));
                return true;
            }
            return fileService.DeletePartialViewMacro(Alias.TrimStart('/'), User.Id);
        }

    }
}
