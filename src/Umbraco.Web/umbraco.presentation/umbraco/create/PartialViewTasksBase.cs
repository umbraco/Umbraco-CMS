using System;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
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

        protected abstract string CodeHeader { get; }

        protected abstract string ParentFolderName { get; }

        public override string AssignedApp
        {
            get { return string.Empty; }
        }

        protected virtual string EditViewFile
        {
            get { return "Settings/Views/EditView.aspx"; }
        }

        protected string BasePath
        {
            get { return SystemDirectories.MvcViews + "/" + ParentFolderName.EnsureEndsWith('/'); }
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

            var partialViewsFileSystem = new PhysicalFileSystem(BasePath);
            var fullFilePath = partialViewsFileSystem.GetFullPath(fileName);

            var model = new PartialView(fullFilePath)
                        {
                            FileName = fileName,
                            SnippetName = snippetName,
                            CreateMacro = ParentID == 1,
                            CodeHeader = CodeHeader,
                            ParentFolderName = ParentFolderName,
                            EditViewFile = EditViewFile,
                            BasePath = BasePath
                        };

            var fileService = (FileService)ApplicationContext.Current.Services.FileService;
            var attempt = fileService.CreatePartialView(model);

            _returnUrl = attempt.Result.ReturnUrl;

            return attempt.Success;
        }

        public override bool PerformDelete()
        {
            var partialViewsFileSystem = new PhysicalFileSystem(BasePath);
            var path = Alias.TrimStart('/');
            var fullFilePath = partialViewsFileSystem.GetFullPath(path);
            
            var model = new PartialView(fullFilePath) { BasePath = BasePath, FileName = path };

            var fileService = (FileService)ApplicationContext.Current.Services.FileService;
            return fileService.DeletePartialView(model, UmbracoEnsuredPage.CurrentUser.Id);
        }
    }
}
