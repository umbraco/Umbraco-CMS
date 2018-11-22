using System;
using System.Data;
using System.IO;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.UI;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using umbraco.cms.businesslogic.member;
using Umbraco.Core.FileResources;

namespace umbraco
{
    /// <summary>
    /// Summary description for standardTasks.
    /// </summary>
    ///

    public class XsltTasks : LegacyDialogTask
    {

        public override bool PerformSave()
        {
            IOHelper.EnsurePathExists(SystemDirectories.Xslt);
            IOHelper.EnsureFileExists(Path.Combine(IOHelper.MapPath(SystemDirectories.Xslt), "web.config"), Files.BlockingWebConfig);

            var template = Alias.Substring(0, Alias.IndexOf("|||"));
            var fileName = Alias.Substring(Alias.IndexOf("|||") + 3, Alias.Length - Alias.IndexOf("|||") - 3).Replace(" ", "");
            if (fileName.ToLowerInvariant().EndsWith(".xslt") == false)
                fileName += ".xslt";
            var xsltTemplateSource = IOHelper.MapPath(SystemDirectories.Umbraco + "/xslt/templates/" + template);
            var xsltNewFilename = IOHelper.MapPath(SystemDirectories.Xslt + "/" + fileName);
            
			if (File.Exists(xsltNewFilename) == false)
			{
				if (fileName.Contains("/")) //if there's a / create the folder structure for it
				{
					var folders = fileName.Split("/".ToCharArray());
					var xsltBasePath = IOHelper.MapPath(SystemDirectories.Xslt);
					for (var i = 0; i < folders.Length - 1; i++)
					{
						xsltBasePath = System.IO.Path.Combine(xsltBasePath, folders[i]);
						System.IO.Directory.CreateDirectory(xsltBasePath);
					}
				}

				// update with xslt references
				var xslt = "";
				using (var xsltFile = System.IO.File.OpenText(xsltTemplateSource))
				{
                    xslt = xsltFile.ReadToEnd();
                    xsltFile.Close();    
				}

				// prepare support for XSLT extensions
				xslt = macro.AddXsltExtensionsToHeader(xslt);
				var xsltWriter = System.IO.File.CreateText(xsltNewFilename);
				xsltWriter.Write(xslt);
				xsltWriter.Flush();
				xsltWriter.Close();

				// Create macro?
				if (ParentID == 1)
				{
                    var name = Alias.Substring(Alias.IndexOf("|||") + 3, Alias.Length - Alias.IndexOf("|||") - 3);
                    if (name.ToLowerInvariant().EndsWith(".xslt"))
                        name = name.Substring(0, name.Length - 5);

				    name = name.SplitPascalCasing().ToFirstUpperInvariant();
					cms.businesslogic.macro.Macro m =
						cms.businesslogic.macro.Macro.MakeNew(name);
					m.Xslt = fileName;
                    m.Save();
				}
			}

            _returnUrl = string.Format(SystemDirectories.Umbraco + "/developer/xslt/editXslt.aspx?file={0}", fileName);

            return true;
        }

        public override bool PerformDelete()
        {
            var path = IOHelper.MapPath(SystemDirectories.Xslt + "/" + Alias.TrimStart('/'));

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

        private string _returnUrl = "";
        
        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return DefaultApps.developer.ToString(); }
        }
    }
}
