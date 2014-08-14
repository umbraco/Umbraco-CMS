using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI;
using umbraco.BasePages;
using Umbraco.Core;
using umbraco.BusinessLogic;

namespace umbraco
{
    /// <summary>
    /// The base UI 'tasks' for the create dialog and delete processes
    /// </summary>
    [UmbracoWillObsolete("http://issues.umbraco.org/issue/U4-1373", "This will one day be removed when we overhaul the create process")]
    public abstract class PartialViewTasksBase : LegacyDialogTask
    {
        private readonly Regex _headerMatch = new Regex("^@inherits\\s+?.*$", RegexOptions.Multiline | RegexOptions.Compiled);

        protected abstract string CodeHeader { get; }

        protected abstract string ParentFolderName { get; }

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
            var relativeFilePath = ParentFolderName.EnsureEndsWith('/') + partialViewsFileSystem.GetRelativePath(fileName);
            var fullFilePath = partialViewsFileSystem.GetFullPath(fileName);

            //return the link to edit the file if it already exists
            if (partialViewsFileSystem.FileExists(fullFilePath))
            {
                _returnUrl = string.Format(EditViewFile + "?file={0}", HttpUtility.UrlEncode(relativeFilePath));
                return true;
            }

            if (Creating.IsRaisedEventCancelled(new NewEventArgs<string>(fullFilePath, fileName, ParentFolderName), this))
            {
                return false;
            }

            //create the file
            var snippetPathAttempt = TryGetSnippetPath(snippetName);
            if (snippetPathAttempt.Success == false)
            {
                throw new InvalidOperationException("Could not load template with name " + snippetName);
            }

            using (var snippetFile = new StreamReader(partialViewsFileSystem.OpenFile(snippetPathAttempt.Result)))
            {
                var snippetContent = snippetFile.ReadToEnd().Trim();

                //strip the @inherits if it's there
                snippetContent = _headerMatch.Replace(snippetContent, string.Empty);

                var content = string.Format("{0}{1}{2}", CodeHeader, Environment.NewLine, snippetContent);

                var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                partialViewsFileSystem.AddFile(fullFilePath, stream);
            }

            // Create macro?
            if (ParentID == 1)
            {
                var name = fileName.Substring(0, (fileName.LastIndexOf('.') + 1))
                            .Trim('.')
                            .SplitPascalCasing()
                            .ToFirstUpperInvariant();

                var m = cms.businesslogic.macro.Macro.MakeNew(name);

                m.ScriptingFile = BasePath + fileName;
                m.Save();
            }

            _returnUrl = string.Format(EditViewFile + "?file={0}", HttpUtility.UrlEncode(relativeFilePath));

            Created.RaiseEvent(new NewEventArgs<string>(fullFilePath, fileName, ParentFolderName), this);

            return true;
        }

        protected virtual void WriteTemplateHeader(StreamWriter sw)
        {
            //write out the template header
            sw.Write("@inherits ");
            sw.Write(typeof(UmbracoTemplatePage).FullName.TrimEnd("`1"));
        }

        public override bool PerformDelete()
        {
            var partialViewsFileSystem = new PhysicalFileSystem(BasePath);
            var path = Alias.TrimStart('/');
            var fullFilePath = partialViewsFileSystem.GetFullPath(path);

            if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<string>(fullFilePath), this))
            {
                return false;
            }

            if (partialViewsFileSystem.FileExists(path))
                partialViewsFileSystem.DeleteFile(path);
            else if (partialViewsFileSystem.DirectoryExists(path))
                partialViewsFileSystem.DeleteDirectory(path, true);

            LogHelper.Info<PartialViewMacroTasks>(string.Format("{0} Deleted by user {1}", Alias, UmbracoEnsuredPage.CurrentUser.Id));

            Deleted.RaiseEvent(new DeleteEventArgs<string>(fullFilePath, false), this);

            return true;
        }


        private string _returnUrl = "";
        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return string.Empty; }
        }

        private Attempt<string> TryGetSnippetPath(string fileName)
        {
            var partialViewsFileSystem = new PhysicalFileSystem(BasePath);
            var snippetPath = IOHelper.MapPath(string.Format("{0}/PartialViewMacros/Templates/{1}", SystemDirectories.Umbraco, fileName));
            
            return partialViewsFileSystem.FileExists(snippetPath)
                ? Attempt<string>.Succeed(snippetPath)
                : Attempt<string>.Fail();
        }

        /// <summary>
        /// Occurs before Create
        /// </summary>
        internal static event TypedEventHandler<PartialViewTasksBase, NewEventArgs<string>> Creating;

        /// <summary>
        /// Occurs after Create
        /// </summary>
        internal static event TypedEventHandler<PartialViewTasksBase, NewEventArgs<string>> Created;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        internal static event TypedEventHandler<PartialViewTasksBase, DeleteEventArgs<string>> Deleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        internal static event TypedEventHandler<PartialViewTasksBase, DeleteEventArgs<string>> Deleted;
    }
}
