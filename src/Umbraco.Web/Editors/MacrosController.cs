using Umbraco.Core.Services;

namespace Umbraco.Web.Editors
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Umbraco.Core;
    using Umbraco.Core.IO;
    using Umbraco.Core.Logging;
    using Umbraco.Core.Models;
    using Umbraco.Web.Composing;
    using Umbraco.Web.Models.ContentEditing;
    using Umbraco.Web.Mvc;
    using Umbraco.Web.UI;
    using Umbraco.Web.WebApi;
    using Umbraco.Web.WebApi.Filters;

    using Constants = Umbraco.Core.Constants;

    /// <summary>
    /// The API controller used for editing dictionary items
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.Macros)]
    public class MacrosController : BackOfficeNotificationsController
    {
        private readonly IMacroService _macroService;

        public MacrosController(IMacroService macroService)
        {
            _macroService = macroService;
        }

        /// <summary>
        /// Creates a new macro
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpPost]
        public HttpResponseMessage Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return this.ReturnErrorResponse("Name can not be empty");
            }

            var alias = name.ToSafeAlias();

            if (_macroService.GetByAlias(alias) != null)
            {
                return this.ReturnErrorResponse("Macro with this alias already exists");
            }

            try
            {
                var macro = new Macro
                {
                    Alias = alias,
                    Name = name,
                    MacroSource = string.Empty,
                    MacroType = MacroTypes.PartialView
                };

                _macroService.Save(macro, this.Security.CurrentUser.Id);

                return this.Request.CreateResponse(HttpStatusCode.OK, macro.Id);
            }
            catch (Exception exception)
            {
                return this.ReturnErrorResponse("Error creating macro", true, exception);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetById(int id)
        {
            var macro = _macroService.GetById(id);

            if (macro == null)
            {
                return this.ReturnErrorResponse($"Macro with id {id} does not exist");
            }

            var macroDisplay = new MacroDisplay
            {
                Alias = macro.Alias,
                Id = macro.Id,
                Key = macro.Key,
                Name = macro.Name,
                CacheByPage = macro.CacheByPage,
                CacheByUser = macro.CacheByMember,
                CachePeriod = macro.CacheDuration,
                View = macro.MacroSource,
                RenderInEditor = !macro.DontRender,
                UseInEditor = macro.UseInEditor,
                Path = $"-1,{macro.Id}"
            };

            var parameters = new List<MacroParameterDisplay>();

            foreach (var param in macro.Properties.Values.OrderBy(x => x.SortOrder))
            {
                parameters.Add(new MacroParameterDisplay
                {
                    Editor = param.EditorAlias,
                    Key = param.Alias,
                    Label = param.Name,
                    Id = param.Id
                });
            }

            macroDisplay.Parameters = parameters;

            return this.Request.CreateResponse(HttpStatusCode.OK, macroDisplay);
        }


        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var macro = _macroService.GetById(id);

            if (macro == null)
            {
                return this.ReturnErrorResponse($"Macro with id {id} does not exist");
            }

            _macroService.Delete(macro);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        public HttpResponseMessage Save(MacroDisplay macroDisplay)
        {
            if (macroDisplay == null)
            {
                return this.ReturnErrorResponse($"No macro data found in request");
            }

            var macro = _macroService.GetById(int.Parse(macroDisplay.Id.ToString()));

            if (macro == null)
            {
                return this.ReturnErrorResponse($"Macro with id {macroDisplay.Id} does not exist");
            }

            if (macroDisplay.Alias != macro.Alias)
            {
                var macroByAlias = _macroService.GetByAlias(macroDisplay.Alias);

                if (macroByAlias != null)
                {
                    return this.ReturnErrorResponse("Macro with this alias already exists");
                    }
            }

            macro.Alias = macroDisplay.Alias;
            macro.Name = macroDisplay.Name;
            macro.CacheByMember = macroDisplay.CacheByUser;
            macro.CacheByPage = macroDisplay.CacheByPage;
            macro.CacheDuration = macroDisplay.CachePeriod;
            macro.DontRender = !macroDisplay.RenderInEditor;
            macro.UseInEditor = macroDisplay.UseInEditor;
            macro.MacroSource = macroDisplay.View;
            macro.MacroType = MacroTypes.PartialView;
            macro.Properties.ReplaceAll(macroDisplay.Parameters.Select((x,i) => new MacroProperty(x.Key, x.Label, i, x.Editor)));

            try
            {
                _macroService.Save(macro, this.Security.CurrentUser.Id);

                macroDisplay.Notifications.Clear();

                macroDisplay.Notifications.Add(new Models.ContentEditing.Notification("Success", "Macro saved", SpeechBubbleIcon.Success));

                return this.Request.CreateResponse(HttpStatusCode.OK, macroDisplay);
            }
            catch (Exception exception)
            {
                return this.ReturnErrorResponse("Error creating macro", true, exception);
            }
        }

        /// <summary>
        /// Gets a list of available macro partials
        /// </summary>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage GetPartialViews()
        {
            var views = new List<string>();

            views.AddRange(this.FindPartialViewsFiles());

            return this.Request.CreateResponse(HttpStatusCode.OK, views);
        }

        /// <summary>
        /// Gets the available parameter editors
        /// </summary>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage GetParameterEditors()
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, Current.ParameterEditors);
        }

        /// <summary>
        /// Returns a error response and optionally logs it
        /// </summary>
        /// <param name="message">
        /// The error message.
        /// </param>
        /// <param name="logError">
        /// Value to indicate if the error needs to be logged
        /// </param>
        /// <param name="exception">
        /// The exception to log
        /// </param>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        private HttpResponseMessage ReturnErrorResponse(string message, bool logError = false, Exception exception = null)
        {
            if (logError && exception != null)
            {
                this.Logger.Error<MacrosController>(exception, message);
            }

            return this.Request.CreateNotificationValidationErrorResponse(message);
        }

        /// <summary>
        /// Finds all the macro partials
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        private IEnumerable<string> FindPartialViewsFiles()
        {
            var files = new List<string>();

            files.AddRange(this.FindPartialViewFilesInViewsFolder());
            files.AddRange(this.FindPartialViewFilesInPluginFolders());

            return files;
        }

        /// <summary>
        /// Finds all macro partials in the views folder
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        private IEnumerable<string> FindPartialViewFilesInViewsFolder()
        {
            var partialsDir = IOHelper.MapPath(SystemDirectories.MacroPartials);

            return this.FindPartialViewFilesInFolder(
                 partialsDir,
                 partialsDir,
                 SystemDirectories.MacroPartials);
        }

        /// <summary>
        /// Finds partial view files in app plugin folders.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        private IEnumerable<string> FindPartialViewFilesInPluginFolders()
        {
            var files = new List<string>();

            var appPluginsFolder = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.AppPlugins));

            if (!appPluginsFolder.Exists)
            {
                return files;
            }

            foreach (var directory in appPluginsFolder.GetDirectories())
            {
                var viewsFolder = directory.GetDirectories("Views");
                if (viewsFolder.Any())
                {
                    var macroPartials = viewsFolder.First().GetDirectories("MacroPartials");
                    if (macroPartials.Any())
                    {
                        files.AddRange(this.FindPartialViewFilesInFolder(macroPartials.First().FullName, macroPartials.First().FullName, SystemDirectories.AppPlugins + "/" + directory.Name + "/Views/MacroPartials"));
                    }
                }
            }

            return files;
        }

        /// <summary>
        /// Finds all partial views in a folder and subfolders
        /// </summary>
        /// <param name="orgPath">
        /// The org path.
        /// </param>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <param name="prefixVirtualPath">
        /// The prefix virtual path.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        private IEnumerable<string> FindPartialViewFilesInFolder(string orgPath, string path, string prefixVirtualPath)
        {
            var files = new List<string>();
            var dirInfo = new DirectoryInfo(path);

            foreach (var dir in dirInfo.GetDirectories())
            {
                files.AddRange(this.FindPartialViewFilesInFolder(orgPath, path + "/" + dir.Name, prefixVirtualPath));
            }

            var fileInfo = dirInfo.GetFiles("*.*");

            files.AddRange(
                fileInfo.Select(file =>
                    prefixVirtualPath.TrimEnd('/') + "/" + (path.Replace(orgPath, string.Empty).Trim('/') + "/" + file.Name).Trim('/')));

            return files;
        }
    }
}
