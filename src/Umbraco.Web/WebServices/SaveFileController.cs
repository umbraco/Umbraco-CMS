using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
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
		/// Saves a partial view for a partial view macr
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="oldName"></param>
		/// <param name="contents"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult SavePartialView(string filename, string oldName, string contents)
		{
			var folderPath = SystemDirectories.MvcViews.EnsureEndsWith('/');// +"/Partials/";

			// validate file
			IOHelper.ValidateEditPath(IOHelper.MapPath(folderPath + filename), folderPath);
			// validate extension
			IOHelper.ValidateFileExtension(IOHelper.MapPath(folderPath + filename), new[] { "cshtml" }.ToList());
			
			//TODO: Validate using the macro engine
			var engine = MacroEngineFactory.GetEngine(PartialViewMacroEngine.EngineName);
			//engine.Validate(...)

			var val = contents;
			var saveOldPath = oldName.StartsWith("~/") ? IOHelper.MapPath(oldName) : IOHelper.MapPath(folderPath + oldName);
			var savePath = filename.StartsWith("~/") ? IOHelper.MapPath(filename) : IOHelper.MapPath(folderPath + filename);

			//Directory check.. only allow files in script dir and below to be edited
			if (!savePath.StartsWith(IOHelper.MapPath(folderPath)))
			{
				return Failed(
					ui.Text("speechBubbles", "partialViewErrorText"), ui.Text("speechBubbles", "partialViewErrorHeader"),
					//pass in a new exception ... this will also append the the message
					new ArgumentException("Illegal path: " + savePath));
			}

			//deletes the old file
			if (savePath != saveOldPath)
			{
				if (System.IO.File.Exists(saveOldPath))
					System.IO.File.Delete(saveOldPath);
			}

            //NOTE: I've left the below here just for informational purposes. If we save a file this way, then the UTF8
            // BOM mucks everything up, strangely, if we use WriteAllText everything is ok! 
            // http://issues.umbraco.org/issue/U4-2118
            //using (var sw = System.IO.File.CreateText(savePath))
            //{
            //    sw.Write(val);
            //}

            System.IO.File.WriteAllText(savePath, val, Encoding.UTF8);

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

                //check if the master page has changed
                if (t.MasterTemplate != masterTemplateId)
                {
                    pathChanged = true;
                    t.MasterTemplate = masterTemplateId;
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
                    new {path = syncPath});
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
