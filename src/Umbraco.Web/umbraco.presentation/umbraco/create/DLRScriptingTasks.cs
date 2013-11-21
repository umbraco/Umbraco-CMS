using System;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core;
using Umbraco.Web.UI;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;

namespace umbraco
{
    public class DLRScriptingTasks : LegacyDialogTask
    {
     
        public override bool PerformSave()
        {
            var template = Alias.Substring(0, Alias.IndexOf("|||")).Trim();
            var fileName = Alias.Substring(Alias.IndexOf("|||") + 3, Alias.Length - Alias.IndexOf("|||") - 3).Replace(" ", "");

            if (!fileName.Contains("."))
                fileName = Alias + ".py";

            var scriptContent = "";
            if (!string.IsNullOrEmpty(template))
            {
                var templateFile = System.IO.File.OpenText(IOHelper.MapPath(SystemDirectories.Umbraco + "/scripting/templates/" + template));
                scriptContent = templateFile.ReadToEnd();
                templateFile.Close();
            }

			var abFileName = IOHelper.MapPath(SystemDirectories.MacroScripts + "/" + fileName);

			if (!System.IO.File.Exists(abFileName))
			{
				if (fileName.Contains("/")) //if there's a / create the folder structure for it
				{
					var folders = fileName.Split("/".ToCharArray());
					var basePath = IOHelper.MapPath(SystemDirectories.MacroScripts);
					for (var i = 0; i < folders.Length - 1; i++)
					{
						basePath = System.IO.Path.Combine(basePath, folders[i]);
						System.IO.Directory.CreateDirectory(basePath);
					}
				}

				var scriptWriter = System.IO.File.CreateText(abFileName);
				scriptWriter.Write(scriptContent);
				scriptWriter.Flush();
				scriptWriter.Close();

				if (ParentID == 1)
				{
                    var name = fileName
                        .Substring(0, (fileName.LastIndexOf('.') + 1)).Trim('.')
                        .SplitPascalCasing().ToFirstUpperInvariant();
					cms.businesslogic.macro.Macro m = cms.businesslogic.macro.Macro.MakeNew(name);
					m.ScriptingFile = fileName;
				    m.Save();
				}
			}

            _returnUrl = string.Format(SystemDirectories.Umbraco + "/developer/python/editPython.aspx?file={0}", fileName);
            return true;
        }

        public override bool PerformDelete()
        {
            var path = IOHelper.MapPath(SystemDirectories.MacroScripts + "/" + Alias.TrimStart('/'));
            try
            {
                System.IO.File.Delete(path);
            }
            catch (Exception ex)
            {
                LogHelper.Error<DLRScriptingTasks>(string.Format("Could not remove DLR file {0} - User {1}", Alias, User.Id), ex);
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
