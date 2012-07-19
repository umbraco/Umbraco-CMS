using System;
using System.Data;
using System.Web.Security;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using umbraco.IO;
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
            string[] scriptFileAr = _alias.Split('¤');



            string relPath = scriptFileAr[0];
            string fileName = scriptFileAr[1];
            string fileType = scriptFileAr[2];

            int createFolder = ParentID;

            string basePath = IOHelper.MapPath(SystemDirectories.Scripts + "/" + relPath + fileName);

            if (createFolder == 1)
            {
                System.IO.Directory.CreateDirectory(basePath);
            }
            else
            {
                System.IO.File.Create(basePath + "." + fileType).Close();
                m_returnUrl = string.Format("settings/scripts/editScript.aspx?file={0}{1}.{2}", relPath, fileName, fileType);
            }
            return true;
        }

        public bool Delete()
        {
            string path = IOHelper.MapPath(SystemDirectories.Scripts + "/" + _alias.TrimStart('/'));

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            else if (System.IO.Directory.Exists(path))
                System.IO.Directory.Delete(path, true);

            BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Delete, umbraco.BasePages.UmbracoEnsuredPage.CurrentUser, -1, _alias + " Deleted");
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
