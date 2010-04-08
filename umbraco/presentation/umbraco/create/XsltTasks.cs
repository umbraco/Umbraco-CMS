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
    /// <summary>
    /// Summary description for standardTasks.
    /// </summary>
    ///

    public class XsltTasks : interfaces.ITaskReturnUrl
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
            string template = _alias.Substring(0, _alias.IndexOf("|||"));
            string fileName = _alias.Substring(_alias.IndexOf("|||") + 3, _alias.Length - _alias.IndexOf("|||") - 3).Replace(" ", "");
            string xsltTemplateSource = IOHelper.MapPath(SystemDirectories.Umbraco + "/xslt/templates/" + template);
            string xsltNewFilename = IOHelper.MapPath(SystemDirectories.Xslt + "/" + fileName + ".xslt");

            if (fileName.Contains("/")) //if there's a / create the folder structure for it
            {
                string[] folders = fileName.Split("/".ToCharArray());
                string xsltBasePath = IOHelper.MapPath(SystemDirectories.Xslt);
                for (int i = 0; i < folders.Length - 1; i++)
                {
                    xsltBasePath = System.IO.Path.Combine(xsltBasePath, folders[i]);
                    System.IO.Directory.CreateDirectory(xsltBasePath);
                }
            }

            //            System.IO.File.Copy(xsltTemplateSource, xsltNewFilename, false);

            // update with xslt references
            string xslt = "";
            System.IO.StreamReader xsltFile = System.IO.File.OpenText(xsltTemplateSource);
            xslt = xsltFile.ReadToEnd();
            xsltFile.Close();

            // prepare support for XSLT extensions
            xslt = macro.AddXsltExtensionsToHeader(xslt);
            System.IO.StreamWriter xsltWriter = System.IO.File.CreateText(xsltNewFilename);
            xsltWriter.Write(xslt);
            xsltWriter.Flush();
            xsltWriter.Close();

            // Create macro?
            if (ParentID == 1)
            {
                cms.businesslogic.macro.Macro m =
                    cms.businesslogic.macro.Macro.MakeNew(
                    helper.SpaceCamelCasing(_alias.Substring(_alias.IndexOf("|||") + 3, _alias.Length - _alias.IndexOf("|||") - 3)));
                m.Xslt = fileName + ".xslt";
            }

            m_returnUrl = string.Format(SystemDirectories.Umbraco + "/developer/xslt/editXslt.aspx?file={0}.xslt", fileName);

            return true;
        }

        public bool Delete()
        {
            string path = IOHelper.MapPath(SystemDirectories.Xslt + "/" + Alias.TrimStart('/'));

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

        public XsltTasks()
        {
            //
            // TODO: Add constructor logic here
            //
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
