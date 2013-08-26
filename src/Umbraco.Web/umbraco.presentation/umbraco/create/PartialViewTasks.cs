using System.IO;
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
            var pipesIndex = _alias.IndexOf("|||", System.StringComparison.Ordinal);
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
                using (var templateFile = File.OpenText(IOHelper.MapPath(SystemDirectories.Umbraco + "/partialviews/templates/" + template)))
                {
                    var templateContent = templateFile.ReadToEnd();
                    sw.Write(templateContent);
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

        protected virtual void WriteTemplateHeader(StreamWriter sw)
        {
            //write out the template header
            sw.Write("@inherits ");
            sw.Write(typeof(UmbracoViewPage<>).FullName.TrimEnd("`1"));
            sw.Write("<dynamic>");
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
    }
}