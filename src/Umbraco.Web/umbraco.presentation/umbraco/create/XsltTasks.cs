using System;
using System.Data;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
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
        private int _parentId;
        private int _typeId;
        private int _userId;

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
            string template = _alias.Substring(0, _alias.IndexOf("|||"));
            string fileName = _alias.Substring(_alias.IndexOf("|||") + 3, _alias.Length - _alias.IndexOf("|||") - 3).Replace(" ", "");
            string xsltTemplateSource = IOHelper.MapPath(SystemDirectories.Umbraco + "/xslt/templates/" + template);
            string xsltNewFilename = IOHelper.MapPath(SystemDirectories.Xslt + "/" + fileName + ".xslt");


			if (!System.IO.File.Exists(xsltNewFilename))
			{
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
				    var name = _alias.Substring(_alias.IndexOf("|||") + 3, _alias.Length - _alias.IndexOf("|||") - 3)
				                     .SplitPascalCasing().ToFirstUpperInvariant();
					cms.businesslogic.macro.Macro m =
						cms.businesslogic.macro.Macro.MakeNew(name);
					m.Xslt = fileName + ".xslt";
				}
			}

            _returnUrl = string.Format(SystemDirectories.Umbraco + "/developer/xslt/editXslt.aspx?file={0}.xslt", fileName);

            return true;
        }

        public bool Delete()
        {
            string path = IOHelper.MapPath(SystemDirectories.Xslt + "/" + Alias.TrimStart('/'));

            System.Web.HttpContext.Current.Trace.Warn("", "*" + path + "*");

            try
            {
                if(System.IO.Directory.Exists(path))
                    System.IO.Directory.Delete(path);
                else if(System.IO.File.Exists(path))
                    System.IO.File.Delete(path);                
            }
            catch (Exception ex)
            {
                LogHelper.Error<XsltTasks>(string.Format("Could not remove XSLT file {0} - User {1}", Alias, UmbracoEnsuredPage.CurrentUser.Id), ex);
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
        private string _returnUrl = "";
        public string ReturnUrl
        {
            get { return _returnUrl; }
        }

        #endregion

    }
}
