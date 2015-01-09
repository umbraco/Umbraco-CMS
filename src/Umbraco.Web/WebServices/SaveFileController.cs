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
using Umbraco.Core;

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
            //NOTE: This is a bit of a hack because we're sharing the View editor with templates/partial views/partial view macros, so the path starts with
            // 'Partials' which we need to remove because the path that we construct the partial view with is a relative path to the root path of 
            // Views/Partials
            if (filename.InvariantStartsWith("MacroPartials/"))
            {
                filename = filename.TrimStart("MacroPartials/");
            }
            if (oldName.IsNullOrWhiteSpace() == false)
            {
                if (oldName.InvariantStartsWith("MacroPartials/"))
                {
                    oldName = oldName.TrimStart("MacroPartials/");
                }
            }

            var fileService = (FileService)Services.FileService;

            //try to get the file by the old name first if they are different and delete that file
            if (filename.Trim().InvariantEquals(oldName.Trim()) == false)
            {
                var existing = fileService.GetPartialViewMacro(oldName);
                if (existing != null)
                {
                    var success = fileService.DeletePartialViewMacro(existing.Path, Security.GetUserId());
                    if (success == false)
                    {
                        return Failed(
                            ui.Text("speechBubbles", "partialViewErrorText"), ui.Text("speechBubbles", "partialViewErrorHeader"),
                            //pass in a new exception ... this will also append the the message
                            new Exception("Could not delete old file: " + oldName));
                    }
                }
            }

            var partialView = new PartialView(filename)
            {
                Content = contents
            };

            var attempt = fileService.SavePartialViewMacro(partialView);

            if (attempt.Success == false)
            {
                return Failed(
                    ui.Text("speechBubbles", "partialViewErrorText"), ui.Text("speechBubbles", "partialViewErrorHeader"),
                    //pass in a new exception ... this will also append the the message
                    attempt.Exception);
            }

            return Success(ui.Text("speechBubbles", "partialViewSavedText"), ui.Text("speechBubbles", "partialViewSavedHeader"));
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
            //NOTE: This is a bit of a hack because we're sharing the View editor with templates/partial views/partial view macros, so the path starts with
            // 'Partials' which we need to remove because the path that we construct the partial view with is a relative path to the root path of 
            // Views/Partials
            if (filename.InvariantStartsWith("Partials/"))
            {
                filename = filename.TrimStart("Partials/");
            }
            if (oldName.IsNullOrWhiteSpace() == false)
            {
                if (oldName.InvariantStartsWith("Partials/"))
                {
                    oldName = oldName.TrimStart("Partials/");
                }    
            }

            var fileService = (FileService)Services.FileService;
            
            //try to get the file by the old name first if they are different and delete that file
            if (filename.Trim().InvariantEquals(oldName.Trim()) == false)
            {
                var existing = fileService.GetPartialView(oldName);
                if (existing != null)
                {
                    var success = fileService.DeletePartialView(existing.Path, Security.GetUserId());
                    if (success == false)
                    {
                        return Failed(
                            ui.Text("speechBubbles", "partialViewErrorText"), ui.Text("speechBubbles", "partialViewErrorHeader"),
                            //pass in a new exception ... this will also append the the message
                            new Exception("Could not delete old file: " + oldName));
                    }
                }
            }
            
            var partialView = new PartialView(filename)
            {
                Content = contents
            };
            
            var attempt = fileService.SavePartialView(partialView);

            if (attempt.Success == false)
            {
                return Failed(
                    ui.Text("speechBubbles", "partialViewErrorText"), ui.Text("speechBubbles", "partialViewErrorHeader"),
                    //pass in a new exception ... this will also append the the message
                    attempt.Exception);
            }

            return Success(ui.Text("speechBubbles", "partialViewSavedText"), ui.Text("speechBubbles", "partialViewSavedHeader"));
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
                    Text = templateName,
                    Alias = templateAlias,
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
                        contents = t.Design
                    });
            }
            catch (Exception ex)
            {
                return Failed(ui.Text("speechBubbles", "templateErrorText"), ui.Text("speechBubbles", "templateErrorHeader"), ex);
            }
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
