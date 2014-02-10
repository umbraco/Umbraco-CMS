using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using umbraco.BasePages;
using Umbraco.Core;

namespace umbraco
{
    /// <summary>
    /// The UI 'tasks' for the create dialog and delete processes
    /// </summary>
    [UmbracoWillObsolete("http://issues.umbraco.org/issue/U4-1373", "This will one day be removed when we overhaul the create process")]
    public class PartialViewTasks : interfaces.ITaskReturnUrl
    {
        private const string CodeHeader = "@inherits Umbraco.Web.Mvc.UmbracoTemplatePage";
        private readonly Regex _headerMatch = new Regex("^@inherits\\s+?.*$", RegexOptions.Multiline | RegexOptions.Compiled);

        private string _alias;
        private int _parentId;
        private int _typeId;
        private int _userId;

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
            get { return "Partials"; }
        }

        public int UserId
        {
            set { _userId = value; }
        }
        public int TypeID
        {
            set { _typeId = value; }
            get { return _typeId; }
        }


        public string Alias
        {
            set { _alias = value; }
            get { return _alias; }
        }

        public int ParentID
        {
            set { _parentId = value; }
            get { return _parentId; }
        }

        public bool Save()
        {
            var pipesIndex = _alias.IndexOf("|||", StringComparison.Ordinal);
            var template = _alias.Substring(0, pipesIndex).Trim();
            var fileName = _alias.Substring(pipesIndex + 3, _alias.Length - pipesIndex - 3) + ".cshtml";

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
                var templatePathAttempt = TryGetTemplatePath(template);
                if (templatePathAttempt.Success == false)
                {
                    throw new InvalidOperationException("Could not load template with name " + template);
                }

                using (var templateFile = File.OpenText(templatePathAttempt.Result))
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
            }

            _returnUrl = string.Format(EditViewFile + "?file={0}", HttpUtility.UrlEncode(ParentFolderName.EnsureEndsWith('/') + fileName));
            return true;
        }

        public bool Delete()
        {
            var path = IOHelper.MapPath(BasePath + _alias.TrimStart('/'));

            if (File.Exists(path))
                File.Delete(path);
            else if (Directory.Exists(path))
                Directory.Delete(path, true);

            LogHelper.Info<PartialViewTasks>(string.Format("{0} Deleted by user {1}", _alias, UmbracoEnsuredPage.CurrentUser.Id));

            return true;
        }

        #region ITaskReturnUrl Members
        private string _returnUrl = "";
        public string ReturnUrl
        {
            get { return _returnUrl; }
        }

        #endregion

        /// <summary>
        /// Looks first in the Partial View template folder and then in the macro partials template folder if not found
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private Attempt<string> TryGetTemplatePath(string fileName)
        {
            var templatePath = IOHelper.MapPath(SystemDirectories.Umbraco + "/PartialViews/Templates/" + fileName);

            if (File.Exists(templatePath))
            {
                return Attempt<string>.Succeed(templatePath);
            }

            templatePath = IOHelper.MapPath(SystemDirectories.Umbraco + "/PartialViewMacros/Templates/" + fileName);

            return File.Exists(templatePath) 
                ? Attempt<string>.Succeed(templatePath) 
                : Attempt<string>.Fail();
        }
    }
}