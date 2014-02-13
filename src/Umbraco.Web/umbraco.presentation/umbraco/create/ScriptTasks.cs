using System;
using System.Data;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
	public class ScriptTasks : interfaces.ITaskReturnUrl
    {
        private string _alias;
        private int _parentID;
        private int _typeID;
        private int _userID;

        public int UserId
        {
            set { _userID = value; }
        }
        public int TypeID
        {
            set { _typeID = value; }
            get { return _typeID; }
        }


        public string Alias
        {
            set { _alias = value; }
            get { return _alias; }
        }

        public int ParentID
        {
            set { _parentID = value; }
            get { return _parentID; }
        }

        public bool Save()
        {
            var scriptFileAr = _alias.Split('\u00A4');

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
                m_returnUrl = string.Format("settings/scripts/editScript.aspx?file={0}{1}.{2}", relPath, fileName, fileType);
                return true;
            }

            ApplicationContext.Current.Services.FileService.SaveScript(new Script(relPath + fileName + "." + fileType));
            m_returnUrl = string.Format("settings/scripts/editScript.aspx?file={0}{1}.{2}", relPath, fileName, fileType);
            return true;
        }

        public bool Delete()
        {
            if (_alias.Contains(".") == false)
            {
                //there is no extension so we'll assume it's a folder
                ApplicationContext.Current.Services.FileService.DeleteScriptFolder(_alias.TrimStart('/'));
            }
            else
            {
                ApplicationContext.Current.Services.FileService.DeleteScript(_alias.TrimStart('/'));    
            }

            return true;
        }

        #region ITaskReturnUrl Members
        private string m_returnUrl = "";
        public string ReturnUrl
        {
            get { return m_returnUrl; }
        }

        #endregion
    }
}
