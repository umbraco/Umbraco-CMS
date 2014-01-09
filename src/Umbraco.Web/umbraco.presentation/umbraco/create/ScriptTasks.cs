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

            var relPath = scriptFileAr[0];
            var fileName = scriptFileAr[1];
            var fileType = scriptFileAr[2];
            
            var createFolder = ParentID;

            if (createFolder == 1)
            {
                ApplicationContext.Current.Services.FileService.CreateScriptFolder(relPath + fileName);
                return true;
            }

            var found = ApplicationContext.Current.Services.FileService.GetScriptByName(relPath + fileName + "." + fileType);
            if (found != null)
            {
                _returnUrl = string.Format("settings/scripts/editScript.aspx?file={0}{1}.{2}", relPath, fileName, fileType);
                return true;
            }

            ApplicationContext.Current.Services.FileService.SaveScript(new Script(relPath + fileName + "." + fileType));
            _returnUrl = string.Format("settings/scripts/editScript.aspx?file={0}{1}.{2}", relPath, fileName, fileType);
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
                ApplicationContext.Current.Services.FileService.DeleteScript(Alias.TrimStart('/'));    
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
