using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI;
using umbraco.BasePages;
using Umbraco.Core;
using umbraco.BusinessLogic;

namespace umbraco
{
    /// <summary>
    /// The UI 'tasks' for the create dialog and delete processes
    /// </summary>
    [UmbracoWillObsolete("http://issues.umbraco.org/issue/U4-1373", "This will one day be removed when we overhaul the create process")]
    public class PartialViewMacroTasks : LegacyDialogTask
    {
        private const string CodeHeader = "@inherits Umbraco.Web.Macros.PartialViewMacroPage";
        private string _returnUrl = "";
        private readonly Regex _headerMatch = new Regex("^@inherits\\s+?.*$", RegexOptions.Multiline | RegexOptions.Compiled);

        
        protected virtual string EditViewFile
        {
            get { return "Settings/Views/EditView.aspx"; }
        }

        protected string BasePath
        {
            get { return SystemDirectories.MvcViews + "/" + ParentFolderName.EnsureEndsWith('/'); }
        }

        protected virtual string ParentFolderName
        {
            get { return "MacroPartials"; }
        }

        public override bool PerformSave()
        {
            var pipesIndex = Alias.IndexOf("|||", System.StringComparison.Ordinal);
            var template = Alias.Substring(0, pipesIndex).Trim();
            var fileName = Alias.Substring(pipesIndex + 3, Alias.Length - pipesIndex - 3) + ".cshtml";

            var fullFilePath = IOHelper.MapPath(BasePath + fileName);

            //return the link to edit the file if it already exists
            if (File.Exists(fullFilePath))
            {
                _returnUrl = string.Format(EditViewFile + "?file={0}", HttpUtility.UrlEncode(ParentFolderName.EnsureEndsWith('/') + fileName));
                return true;
            }

            //create the file
            using (var sw = File.CreateText(fullFilePath))
            {
                using (var templateFile = File.OpenText(IOHelper.MapPath(SystemDirectories.Umbraco + "/PartialViewMacros/Templates/" + template)))
                {
                    var templateContent = templateFile.ReadToEnd().Trim();

                    //strip the @inherits if it's there
                    templateContent = _headerMatch.Replace(templateContent, string.Empty);

                    sw.Write(
                        "{0}{1}{2}",
                        CodeHeader, 
                        Environment.NewLine, 
                        templateContent);
                }
            }

            // Create macro?
            if (ParentID == 1)
            {
                var name = fileName
                    .Substring(0, (fileName.LastIndexOf('.') + 1)).Trim('.')
                    .SplitPascalCasing().ToFirstUpperInvariant();
                var m = cms.businesslogic.macro.Macro.MakeNew(name);
                m.ScriptingFile = BasePath + fileName;
                m.Save();
            }

            _returnUrl = string.Format(EditViewFile + "?treeType={0}&file={1}", "partialViewMacros", HttpUtility.UrlEncode(ParentFolderName.EnsureEndsWith('/') + fileName));
            return true;
        }

        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return DefaultApps.developer.ToString(); }
        }

        public override bool PerformDelete()
        {
            var path = IOHelper.MapPath(BasePath + Alias.TrimStart('/'));

            if (File.Exists(path))
                File.Delete(path);
            else if (Directory.Exists(path))
                Directory.Delete(path, true);

            LogHelper.Info<PartialViewTasks>(string.Format("{0} Deleted by user {1}", Alias, User.Id));

            return true;
        }

    }
}