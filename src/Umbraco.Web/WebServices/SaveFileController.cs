using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Macros;
using Umbraco.Web.Mvc;
using umbraco;
using umbraco.cms.businesslogic.macro;
using System.Collections.Generic;
using umbraco.cms.helpers;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Template = umbraco.cms.businesslogic.template.Template;

namespace Umbraco.Web.WebServices
{
    /// <summary>
    /// A REST controller used to save files such as templates, partial views, macro files, etc...
    /// </summary>
    /// <remarks>
    /// This isn't fully implemented yet but we should migrate all of the logic in the umbraco.presentation.webservices.codeEditorSave
    /// over to this controller.
    /// </remarks>
    [ValidateMvcAngularAntiForgeryToken]
    public class SaveFileController : UmbracoAuthorizedController
    {
        /// <summary>
        /// Saves a partial view macro
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="oldName"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SavePartialViewMacro(string filename, string oldName, string contents)
        {
            var svce = (FileService) Services.FileService;

            return SavePartialView(svce,
                filename, oldName, contents,
                "MacroPartials/",
                (s, n) => s.GetPartialViewMacro(n),
                (s, v) => s.ValidatePartialViewMacro((PartialView) v),
                (s, v) => s.SavePartialViewMacro(v));
        }

        /// <summary>
        /// Saves a partial view
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="oldName"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SavePartialView(string filename, string oldName, string contents)
        {
            var svce = (FileService) Services.FileService;

            return SavePartialView(svce,
                filename, oldName, contents,
                "Partials/",
                (s, n) => s.GetPartialView(n),
                (s, v) => s.ValidatePartialView((PartialView) v),
                (s, v) => s.SavePartialView(v));
        }

        private JsonResult SavePartialView(IFileService svce,
            string filename, string oldname, string contents,
            string pathPrefix,
            Func<IFileService, string, IPartialView> get,
            Func<IFileService, IPartialView, bool> validate,
            Func<IFileService, IPartialView, Attempt<IPartialView>> save)
        {
            // sanitize input - partial view names have an extension
            filename = filename
                .Replace('\\', '/')
                .TrimStart('/');

            // sharing the editor with partial views & partial view macros,
            // using path prefix to differenciate,
            // but the file service manages different filesystems,
            // and we need to come back to filesystem-relative paths

            // not sure why we still need this but not going to change it now

            if (filename.InvariantStartsWith(pathPrefix))
                filename = filename.TrimStart(pathPrefix);

            if (oldname != null)
            {
                oldname = oldname.TrimStart('/', '\\');
                if (oldname.InvariantStartsWith(pathPrefix))
                    oldname = oldname.TrimStart(pathPrefix);
            }

            var currentView = oldname.IsNullOrWhiteSpace() 
                ? get(svce, filename)
                : get(svce, oldname);

            if (currentView == null)
                currentView = new PartialView(filename);
            else
                currentView.Path = filename;
            currentView.Content = contents;

            


            Attempt<IPartialView> attempt;
            try
            {
                var partialView = currentView as PartialView;
                if (partialView != null && validate != null && validate(svce, partialView) == false)
                    return Failed(ui.Text("speechBubbles", "partialViewErrorText"), ui.Text("speechBubbles", "partialViewErrorHeader"),
                                    new FileSecurityException("File '" + currentView.Path + "' is not a valid partial view file."));

                attempt = save(svce, currentView);
            }
            catch (Exception e)
            {
                return Failed(ui.Text("speechBubbles", "partialViewErrorText"), ui.Text("speechBubbles", "partialViewErrorHeader"), e);
            }

            if (attempt.Success == false)
            {
                return Failed(ui.Text("speechBubbles", "partialViewErrorText"), ui.Text("speechBubbles", "partialViewErrorHeader"),
                                attempt.Exception);
            }


            return Success(ui.Text("speechBubbles", "partialViewSavedText"), ui.Text("speechBubbles", "partialViewSavedHeader"), new { name = currentView.Name, path = currentView.Path });
        }

        /// <summary>
        /// Saves a template
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="templateAlias"></param>
        /// <param name="templateContents"></param>
        /// <param name="templateId"></param>
        /// <param name="masterTemplateId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveTemplate(string templateName, string templateAlias, string templateContents, int templateId, int masterTemplateId)
        {
            //TODO: Change this over to use the new API - Also this will be migrated to a TemplateEditor or ViewEditor when it's all moved to angular

            Template t;
            bool pathChanged = false;
            try
            {
                t = new Template(templateId)
                {
                    Text = templateName.CleanForXss('[', ']', '(', ')', ':'),
                    Alias = templateAlias.CleanForXss('[', ']', '(', ')', ':'),
                    Design = templateContents
                };

                //check if the master page has changed - we need to normalize both - if it's 0 or -1, then make it 0... this is easy
                // to do with Math.Max
                if (Math.Max(t.MasterTemplate, 0) != Math.Max(masterTemplateId, 0))
                {
                    t.MasterTemplate = Math.Max(masterTemplateId, 0);
                    pathChanged = true;                  
                }
            }
            catch (ArgumentException ex)
            {
                //the template does not exist
                return Failed("Template does not exist", ui.Text("speechBubbles", "templateErrorHeader"), ex);
            }

            try
            {
                t.Save();

                //ensure the correct path is synced as the parent might have been changed
                // http://issues.umbraco.org/issue/U4-2300                
                if (pathChanged)
                {
                    //need to re-look it up
                    t = new Template(templateId);
                }
                var syncPath = "-1,init," + t.Path.Replace("-1,", "");

                return Success(ui.Text("speechBubbles", "templateSavedText"), ui.Text("speechBubbles", "templateSavedHeader"),
                    new
                    {
                        path = syncPath,
                        contents = t.Design,
                        alias = t.Alias // might have been updated!
                    });
            }
            catch (Exception ex)
            {
                return Failed(ui.Text("speechBubbles", "templateErrorText"), ui.Text("speechBubbles", "templateErrorHeader"), ex);
            }
        }

        [HttpPost]
        public JsonResult SaveScript(string filename, string oldName, string contents)
        {
            // sanitize input - script names have an extension
            filename = filename
                .Replace('\\', '/')
                .TrimStart('/');

            var svce = (FileService) Services.FileService;
            var script = svce.GetScriptByName(oldName);
            if (script == null)
                script = new Script(filename);
            else
                script.Path = filename;
            script.Content = contents;

            try
            {
                if (svce.ValidateScript(script) == false)
                    return Failed(ui.Text("speechBubbles", "scriptErrorText"), ui.Text("speechBubbles", "scriptErrorHeader"),
                                    new FileSecurityException("File '" + filename + "' is not a valid script file."));
                
                svce.SaveScript(script);
            }
            catch (Exception e)
            {
                return Failed(ui.Text("speechBubbles", "scriptErrorText"), ui.Text("speechBubbles", "scriptErrorHeader"), e);
            }

            return Success(ui.Text("speechBubbles", "scriptSavedText"), ui.Text("speechBubbles", "scriptSavedHeader"),
                new
                {
                    path = DeepLink.GetTreePathFromFilePath(script.Path),
                    name = script.Path,
                    url = script.VirtualPath,
                    contents = script.Content
                });
        }

        [HttpPost]
        public JsonResult SaveStylesheet(string filename, string oldName, string contents)
        {
            // sanitize input - stylesheet names have no extension
            filename = filename
                .Replace('\\', '/')
                .TrimStart('/')
                .EnsureEndsWith(".css");

            var svce = (FileService) Services.FileService;
            var stylesheet = svce.GetStylesheetByName(oldName);
            if (stylesheet == null)
                stylesheet = new Stylesheet(filename);
            else
                stylesheet.Path = filename;
            stylesheet.Content = contents;

            try
            {
                if (svce.ValidateStylesheet(stylesheet) == false)
                    return Failed(ui.Text("speechBubbles", "cssErrorText"), ui.Text("speechBubbles", "cssErrorHeader"),
                                    new FileSecurityException("File '" + filename + "' is not a valid stylesheet file."));

                svce.SaveStylesheet(stylesheet);
            }
            catch (Exception e)
            {
                return Failed(ui.Text("speechBubbles", "cssErrorText"), ui.Text("speechBubbles", "cssErrorHeader"), e);
            }

            return Success(ui.Text("speechBubbles", "cssSavedText"), ui.Text("speechBubbles", "cssSavedHeader"),
                new
                {
                    path = DeepLink.GetTreePathFromFilePath(stylesheet.Path),
                    name = stylesheet.Path,
                    url = stylesheet.VirtualPath,
                    contents = stylesheet.Content
                });
        }

        /// <summary>
        /// Returns a successful message
        /// </summary>
        /// <param name="message">The message to display in the speach bubble</param>
        /// <param name="header">The header to display in the speach bubble</param>
        /// <param name="additionalVals"></param>
        /// <returns></returns>
        private JsonResult Success(string message, string header, object additionalVals = null)
        {
            var d = additionalVals == null ? new Dictionary<string, object>() : additionalVals.ToDictionary<object>();
            d["success"] = true;
            d["message"] = message;
            d["header"] = header;

            return Json(d);
        }

        /// <summary>
        /// Returns a failed message
        /// </summary>
        /// <param name="message">The message to display in the speach bubble</param>
        /// <param name="header">The header to display in the speach bubble</param>
        /// <param name="exception">The exception if there was one</param>
        /// <returns></returns>
        private JsonResult Failed(string message, string header, Exception exception = null)
        {
            if (exception != null)
                LogHelper.Error<SaveFileController>("An error occurred saving a file. " + message, exception);
            return Json(new
            {
                success = false,
                header = header,
                message = message + (exception == null ? "" : (exception.Message + ". Check log for details."))
            });
        }
    }
}
