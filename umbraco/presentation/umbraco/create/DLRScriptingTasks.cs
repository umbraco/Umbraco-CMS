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
    public class DLRScriptingTasks : interfaces.ITaskReturnUrl
    {
        private string _alias;
        private int _parentID;
        private int _typeID;
        private int _userID;

        public int UserId
        {
            set
            {
                _userID = value;
            }
        }

        public int TypeID
        {
            get
            {
                return _typeID;
            }
            set
            {
                _typeID = value;
            }
        }

        public string Alias
        {
            get
            {
                return _alias;
            }
            set
            {
                _alias = value;
            }
        }

        public int ParentID
        {
            get
            {
                return _parentID;
            }
            set
            {
                _parentID = value;
            }
        }

        public bool Save()
        {

            string template = _alias.Substring(0, _alias.IndexOf("|||")).Trim();
            string fileName = _alias.Substring(_alias.IndexOf("|||") + 3, _alias.Length - _alias.IndexOf("|||") - 3).Replace(" ", "");

            if (!fileName.Contains("."))
                fileName = _alias + ".py";

            string scriptContent = "";
            if (!string.IsNullOrEmpty(template))
            {
                System.IO.StreamReader templateFile = System.IO.File.OpenText(IOHelper.MapPath(IO.SystemDirectories.Umbraco + "/scripting/templates/" + template));
                scriptContent = templateFile.ReadToEnd();
                templateFile.Close();
            }


            if (fileName.Contains("/")) //if there's a / create the folder structure for it
            {
                string[] folders = fileName.Split("/".ToCharArray());
                string basePath = IOHelper.MapPath(SystemDirectories.Python);
                for (int i = 0; i < folders.Length - 1; i++)
                {
                    basePath = System.IO.Path.Combine(basePath, folders[i]);
                    System.IO.Directory.CreateDirectory(basePath);
                }
            }

            string abFileName = IOHelper.MapPath(SystemDirectories.Python + "/" + fileName);

            System.IO.StreamWriter scriptWriter = System.IO.File.CreateText(abFileName);
            scriptWriter.Write(scriptContent);
            scriptWriter.Flush();
            scriptWriter.Close();


            if (ParentID == 1)
            {
                cms.businesslogic.macro.Macro m = cms.businesslogic.macro.Macro.MakeNew(
                    helper.SpaceCamelCasing(fileName.Substring(0, (fileName.LastIndexOf('.') + 1)).Trim('.')));
                m.ScriptingFile = fileName;
            }

            m_returnUrl = string.Format(SystemDirectories.Umbraco + "/developer/python/editPython.aspx?file={0}", fileName);
            return true;
        }

        public bool Delete()
        {

            string path = IOHelper.MapPath(SystemDirectories.Python + "/" + Alias.TrimStart('/'));

            System.Web.HttpContext.Current.Trace.Warn("", "*" + path + "*");
            try
            {
                System.IO.File.Delete(path);
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, UmbracoEnsuredPage.CurrentUser, -1, "Could not remove XSLT file " + Alias + ". ERROR: " + ex.Message);
            }
            return true;
        }

        public DLRScriptingTasks()
        {
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
