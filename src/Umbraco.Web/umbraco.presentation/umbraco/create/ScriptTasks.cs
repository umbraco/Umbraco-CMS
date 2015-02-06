using Umbraco.Core.IO;
using Umbraco.Web.UI;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using umbraco.BusinessLogic;
using umbraco.BasePages;

namespace umbraco
{
    public class ScriptTasks : LegacyDialogTask
    {
     
        public override bool PerformSave()
        {
            var scriptFileAr = Alias.Split('\u00A4');

            var fileName = scriptFileAr[0];
            var fileType = scriptFileAr[1];
            
            var createFolder = ParentID;

            if (createFolder == 1)
            {
                ApplicationContext.Current.Services.FileService.CreateScriptFolder(fileName);
                return true;
            }

            // remove file extension
            if (fileName.ToLowerInvariant().EndsWith(fileType.ToLowerInvariant()))
            {
                fileName = fileName.Substring(0,
                                              fileName.ToLowerInvariant().LastIndexOf(fileType.ToLowerInvariant(), System.StringComparison.Ordinal) - 1);
            }

            var scriptPath = fileName + "." + fileType;
            var found = ApplicationContext.Current.Services.FileService.GetScriptByName(scriptPath);
            if (found != null)
            {
                _returnUrl = string.Format("settings/scripts/editScript.aspx?file={0}", scriptPath.TrimStart('/'));
                return true;
            }
            var script = new Script(fileName + "." + fileType);
            ApplicationContext.Current.Services.FileService.SaveScript(script);
            _returnUrl = string.Format("settings/scripts/editScript.aspx?file={0}", scriptPath.TrimStart('/'));
            return true;
        }

        public override bool PerformDelete()
        {
            if (Alias.Contains(".") == false)
            {
                //there is no extension so we'll assume it's a folder
                ApplicationContext.Current.Services.FileService.DeleteScriptFolder(Alias.TrimStart('/'));
            }
            else
            {
                ApplicationContext.Current.Services.FileService.DeleteScript(Alias.TrimStart('/'), User.Id);    
            }

            return true;
        }

        private string _returnUrl = "";

        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return DefaultApps.settings.ToString(); }
        }
    }
}
