using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI;
using umbraco.BasePages;
using Umbraco.Core;
using umbraco.BusinessLogic;

namespace umbraco
{
	/// <summary>
	/// The UI 'tasks' for the create dialog and delete processes
	/// </summary>
	[UmbracoWillObsolete("http://issues.umbraco.org/issue/U4-1373", "This will one day be removed when we overhaul the create process")]
    public class PartialViewTasks : LegacyDialogTask
    {
        private const string CodeHeader = "@inherits Umbraco.Web.Mvc.UmbracoTemplatePage";
		
        private readonly Regex _headerMatch = new Regex("^@inherits\\s+?.*$", RegexOptions.Multiline | RegexOptions.Compiled);


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

	    public override bool PerformSave()
	    {
            var pipesIndex = Alias.IndexOf("|||", System.StringComparison.Ordinal);
            var template = Alias.Substring(0, pipesIndex).Trim();
            var fileName = Alias.Substring(pipesIndex + 3, Alias.Length - pipesIndex - 3) + ".cshtml";

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
                var templatePathAttempt = TryGetTemplatePath(template);
                if (templatePathAttempt.Success == false)
                {
                    throw new InvalidOperationException("Could not load template with name " + template);
                }

                using (var templateFile = File.OpenText(templatePathAttempt.Result))
                {
                    var templateContent = templateFile.ReadToEnd().Trim();

                    //strip the @inherits if it's there
                    templateContent = _headerMatch.Replace(templateContent, string.Empty);

                    sw.Write(
                        "{0}{1}{2}",
                        CodeHeader,
                        Environment.NewLine,
                        templateContent);
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
                m.Save();
            }

            _returnUrl = string.Format(EditViewFile + "?file={0}", HttpUtility.UrlEncode(ParentFolderName.EnsureEndsWith('/') + fileName));
            return true;
	    }

        protected virtual void WriteTemplateHeader(StreamWriter sw)
        {
            //write out the template header
            sw.Write("@inherits ");
			sw.Write(typeof(UmbracoTemplatePage).FullName.TrimEnd("`1"));
		}

	    public override bool PerformDelete()
	    {
            var path = IOHelper.MapPath(BasePath + Alias.TrimStart('/'));

            if (File.Exists(path))
                File.Delete(path);
            else if (Directory.Exists(path))
                Directory.Delete(path, true);

            LogHelper.Info<PartialViewTasks>(string.Format("{0} Deleted by user {1}", Alias, UmbracoEnsuredPage.CurrentUser.Id));

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


        /// <summary>
        /// Looks first in the Partial View template folder and then in the macro partials template folder if not found
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private Attempt<string> TryGetTemplatePath(string fileName)
        {
            var templatePath = IOHelper.MapPath(SystemDirectories.Umbraco + "/PartialViews/Templates/" + fileName);

            if (File.Exists(templatePath))
            {
                return Attempt<string>.Succeed(templatePath);
            }

            templatePath = IOHelper.MapPath(SystemDirectories.Umbraco + "/PartialViewMacros/Templates/" + fileName);

            return File.Exists(templatePath) 
                ? Attempt<string>.Succeed(templatePath) 
                : Attempt<string>.Fail();
        }
    }
}
