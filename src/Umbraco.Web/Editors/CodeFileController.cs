using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ClientDependency.Core;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Core.Strings.Css;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web.Trees;
using Stylesheet = Umbraco.Core.Models.Stylesheet;
using StylesheetRule = Umbraco.Web.Models.ContentEditing.StylesheetRule;
using CharArrays = Umbraco.Core.Constants.CharArrays;

namespace Umbraco.Web.Editors
{
    // TODO: Put some exception filters in our webapi to return 404 instead of 500 when we throw ArgumentNullException
    // ref: https://www.exceptionnotfound.net/the-asp-net-web-api-exception-handling-pipeline-a-guided-tour/
    [PluginController("UmbracoApi")]
    [PrefixlessBodyModelValidator]
    [UmbracoApplicationAuthorize(Core.Constants.Applications.Settings)]
    public class CodeFileController : BackOfficeNotificationsController
    {
        public CodeFileController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
        }

        /// <summary>
        /// Used to create a brand new file
        /// </summary>
        /// <param name="type">This is a string but will be 'scripts' 'partialViews', 'partialViewMacros'</param>
        /// <param name="display"></param>
        /// <returns>Will return a simple 200 if file creation succeeds</returns>
        [ValidationFilter]
        public HttpResponseMessage PostCreate(string type, CodeFileDisplay display)
        {
            if (display == null) throw new ArgumentNullException("display");
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Value cannot be null or whitespace.", "type");

            switch (type)
            {
                case Core.Constants.Trees.PartialViews:
                    var view = new PartialView(PartialViewType.PartialView, display.VirtualPath);
                    view.Content = display.Content;
                    var result = Services.FileService.CreatePartialView(view, display.Snippet, Security.CurrentUser.Id);
                    return result.Success == true ? Request.CreateResponse(HttpStatusCode.OK) : Request.CreateNotificationValidationErrorResponse(result.Exception.Message);

                case Core.Constants.Trees.PartialViewMacros:
                    var viewMacro = new PartialView(PartialViewType.PartialViewMacro, display.VirtualPath);
                    viewMacro.Content = display.Content;
                    var resultMacro = Services.FileService.CreatePartialViewMacro(viewMacro, display.Snippet, Security.CurrentUser.Id);
                    return resultMacro.Success == true ? Request.CreateResponse(HttpStatusCode.OK) : Request.CreateNotificationValidationErrorResponse(resultMacro.Exception.Message);

                case Core.Constants.Trees.Scripts:
                    var script = new Script(display.VirtualPath);
                    Services.FileService.SaveScript(script, Security.CurrentUser.Id);
                    return Request.CreateResponse(HttpStatusCode.OK);

                default:
                    return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Used to create a container/folder in 'partialViews', 'partialViewMacros', 'scripts' or 'stylesheets'
        /// </summary>
        /// <param name="type">'partialViews', 'partialViewMacros' or 'scripts'</param>
        /// <param name="parentId">The virtual path of the parent.</param>
        /// <param name="name">The name of the container/folder</param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage PostCreateContainer(string type, string parentId, string name)
        {
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Value cannot be null or whitespace.", "type");
            if (string.IsNullOrWhiteSpace(parentId)) throw new ArgumentException("Value cannot be null or whitespace.", "parentId");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", "name");
            if (name.ContainsAny(Path.GetInvalidPathChars())) {
                return Request.CreateNotificationValidationErrorResponse(Services.TextService.Localize("codefile", "createFolderIllegalChars"));
            }

            // if the parentId is root (-1) then we just need an empty string as we are
            // creating the path below and we don't want -1 in the path
            if (parentId == Core.Constants.System.RootString)
            {
                parentId = string.Empty;
            }

            name = System.Web.HttpUtility.UrlDecode(name);

            if (parentId.IsNullOrWhiteSpace() == false)
            {
                parentId = System.Web.HttpUtility.UrlDecode(parentId);
                name = parentId.EnsureEndsWith("/") + name;
            }

            var virtualPath = string.Empty;
            switch (type)
            {
                case Core.Constants.Trees.PartialViews:
                    virtualPath = NormalizeVirtualPath(name, SystemDirectories.PartialViews);
                    Services.FileService.CreatePartialViewFolder(virtualPath);
                    break;
                case Core.Constants.Trees.PartialViewMacros:
                    virtualPath = NormalizeVirtualPath(name, SystemDirectories.MacroPartials);
                    Services.FileService.CreatePartialViewMacroFolder(virtualPath);
                    break;
                case Core.Constants.Trees.Scripts:
                    virtualPath = NormalizeVirtualPath(name, SystemDirectories.Scripts);
                    Services.FileService.CreateScriptFolder(virtualPath);
                    break;
                case Core.Constants.Trees.Stylesheets:
                    virtualPath = NormalizeVirtualPath(name, SystemDirectories.Css);
                    Services.FileService.CreateStyleSheetFolder(virtualPath);
                    break;

            }

            return Request.CreateResponse(HttpStatusCode.OK, new CodeFileDisplay
            {
                VirtualPath = virtualPath,
                Path = Url.GetTreePathFromFilePath(virtualPath)
            });
        }

        /// <summary>
        /// Used to get a specific file from disk via the FileService
        /// </summary>
        /// <param name="type">This is a string but will be 'scripts' 'partialViews', 'partialViewMacros' or 'stylesheets'</param>
        /// <param name="virtualPath">The filename or URL encoded path of the file to open</param>
        /// <returns>The file and its contents from the virtualPath</returns>
        public CodeFileDisplay GetByPath(string type, string virtualPath)
        {
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Value cannot be null or whitespace.", "type");
            if (string.IsNullOrWhiteSpace(virtualPath)) throw new ArgumentException("Value cannot be null or whitespace.", "virtualPath");

            virtualPath = System.Web.HttpUtility.UrlDecode(virtualPath);

            switch (type)
            {
                case Core.Constants.Trees.PartialViews:
                    var view = Services.FileService.GetPartialView(virtualPath);
                    if (view != null)
                    {
                        var display = Mapper.Map<IPartialView, CodeFileDisplay>(view);
                        display.FileType = Core.Constants.Trees.PartialViews;
                        display.Path = Url.GetTreePathFromFilePath(view.Path);
                        display.Id = System.Web.HttpUtility.UrlEncode(view.Path);
                        return display;
                    }
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                case Core.Constants.Trees.PartialViewMacros:
                    var viewMacro = Services.FileService.GetPartialViewMacro(virtualPath);
                    if (viewMacro != null)
                    {
                        var display = Mapper.Map<IPartialView, CodeFileDisplay>(viewMacro);
                        display.FileType = Core.Constants.Trees.PartialViewMacros;
                        display.Path = Url.GetTreePathFromFilePath(viewMacro.Path);
                        display.Id = System.Web.HttpUtility.UrlEncode(viewMacro.Path);
                        return display;
                    }
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                case Core.Constants.Trees.Scripts:
                    var script = Services.FileService.GetScriptByName(virtualPath);
                    if (script != null)
                    {
                        var display = Mapper.Map<Script, CodeFileDisplay>(script);
                        display.FileType = Core.Constants.Trees.Scripts;
                        display.Path = Url.GetTreePathFromFilePath(script.Path);
                        display.Id = System.Web.HttpUtility.UrlEncode(script.Path);
                        return display;
                    }
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                case Core.Constants.Trees.Stylesheets:
                    var stylesheet = Services.FileService.GetStylesheetByName(virtualPath);
                    if (stylesheet != null)
                    {
                        var display = Mapper.Map<Stylesheet, CodeFileDisplay>(stylesheet);
                        display.FileType = Core.Constants.Trees.Stylesheets;
                        display.Path = Url.GetTreePathFromFilePath(stylesheet.Path);
                        display.Id = System.Web.HttpUtility.UrlEncode(stylesheet.Path);
                        return display;
                    }
                    throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Used to get a list of available templates/snippets to base a new Partial View or Partial View Macro from
        /// </summary>
        /// <param name="type">This is a string but will be 'partialViews', 'partialViewMacros'</param>
        /// <returns>Returns a list of <see cref="SnippetDisplay"/> if a correct type is sent</returns>
        public IEnumerable<SnippetDisplay> GetSnippets(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Value cannot be null or whitespace.", "type");

            IEnumerable<string> snippets;
            switch (type)
            {
                case Core.Constants.Trees.PartialViews:
                    snippets = Services.FileService.GetPartialViewSnippetNames(
                        //ignore these - (this is taken from the logic in "PartialView.ascx.cs")
                        "Gallery",
                        "ListChildPagesFromChangeableSource",
                        "ListChildPagesOrderedByProperty",
                        "ListImagesFromMediaFolder");
                    break;
                case Core.Constants.Trees.PartialViewMacros:
                    snippets = Services.FileService.GetPartialViewSnippetNames();
                    break;
                default:
                    throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return snippets.Select(snippet => new SnippetDisplay() {Name = snippet.SplitPascalCasing().ToFirstUpperInvariant(), FileName = snippet});
        }

        /// <summary>
        /// Used to scaffold the json object for the editors for 'scripts', 'partialViews', 'partialViewMacros' and 'stylesheets'
        /// </summary>
        /// <param name="type">This is a string but will be 'scripts' 'partialViews', 'partialViewMacros' or 'stylesheets'</param>
        /// <param name="id"></param>
        /// <param name="snippetName"></param>
        /// <returns></returns>
        public CodeFileDisplay GetScaffold(string type, string id, string snippetName = null)
        {
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Value cannot be null or whitespace.", "type");
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Value cannot be null or whitespace.", "id");

            CodeFileDisplay codeFileDisplay;

            switch (type)
            {
                case Core.Constants.Trees.PartialViews:
                    codeFileDisplay = Mapper.Map<IPartialView, CodeFileDisplay>(new PartialView(PartialViewType.PartialView, string.Empty));
                    codeFileDisplay.VirtualPath = SystemDirectories.PartialViews;
                    if (snippetName.IsNullOrWhiteSpace() == false)
                        codeFileDisplay.Content = Services.FileService.GetPartialViewSnippetContent(snippetName);
                    break;
                case Core.Constants.Trees.PartialViewMacros:
                    codeFileDisplay = Mapper.Map<IPartialView, CodeFileDisplay>(new PartialView(PartialViewType.PartialViewMacro, string.Empty));
                    codeFileDisplay.VirtualPath = SystemDirectories.MacroPartials;
                    if (snippetName.IsNullOrWhiteSpace() == false)
                        codeFileDisplay.Content = Services.FileService.GetPartialViewMacroSnippetContent(snippetName);
                    break;
                case Core.Constants.Trees.Scripts:
                    codeFileDisplay = Mapper.Map<Script, CodeFileDisplay>(new Script(string.Empty));
                    codeFileDisplay.VirtualPath = SystemDirectories.Scripts;
                    break;
                case Core.Constants.Trees.Stylesheets:
                    codeFileDisplay = Mapper.Map<Stylesheet, CodeFileDisplay>(new Stylesheet(string.Empty));
                    codeFileDisplay.VirtualPath = SystemDirectories.Css;
                    break;
                default:
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unsupported editortype"));
            }

            // Make sure that the root virtual path ends with '/'
            codeFileDisplay.VirtualPath = codeFileDisplay.VirtualPath.EnsureEndsWith("/");

            if (id != Core.Constants.System.RootString)
            {
                codeFileDisplay.VirtualPath += id.TrimStart(CharArrays.ForwardSlash).EnsureEndsWith("/");
                //if it's not new then it will have a path, otherwise it won't
                codeFileDisplay.Path = Url.GetTreePathFromFilePath(id);
            }

            codeFileDisplay.VirtualPath = codeFileDisplay.VirtualPath.TrimStart("~");
            codeFileDisplay.FileType = type;
            return codeFileDisplay;
        }

        /// <summary>
        /// Used to delete a specific file from disk via the FileService
        /// </summary>
        /// <param name="type">This is a string but will be 'scripts' 'partialViews', 'partialViewMacros' or 'stylesheets'</param>
        /// <param name="virtualPath">The filename or URL encoded path of the file to delete</param>
        /// <returns>Will return a simple 200 if file deletion succeeds</returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage Delete(string type, string virtualPath)
        {
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Value cannot be null or whitespace.", "type");
            if (string.IsNullOrWhiteSpace(virtualPath)) throw new ArgumentException("Value cannot be null or whitespace.", "virtualPath");

            virtualPath = System.Web.HttpUtility.UrlDecode(virtualPath);

            switch (type)
            {
                case Core.Constants.Trees.PartialViews:
                    if (IsDirectory(virtualPath, SystemDirectories.PartialViews, Current.FileSystems.PartialViewsFileSystem))
                    {
                        Services.FileService.DeletePartialViewFolder(virtualPath);
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    if (Services.FileService.DeletePartialView(virtualPath, Security.CurrentUser.Id))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No Partial View or folder found with the specified path");

                case Core.Constants.Trees.PartialViewMacros:
                    if (IsDirectory(virtualPath, SystemDirectories.MacroPartials, Current.FileSystems.MacroPartialsFileSystem))
                    {
                        Services.FileService.DeletePartialViewMacroFolder(virtualPath);
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    if (Services.FileService.DeletePartialViewMacro(virtualPath, Security.CurrentUser.Id))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No Partial View Macro or folder found with the specified path");

                case Core.Constants.Trees.Scripts:
                    if (IsDirectory(virtualPath, SystemDirectories.Scripts, Current.FileSystems.ScriptsFileSystem))
                    {
                        Services.FileService.DeleteScriptFolder(virtualPath);
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    if (Services.FileService.GetScriptByName(virtualPath) != null)
                    {
                        Services.FileService.DeleteScript(virtualPath, Security.CurrentUser.Id);
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No Script or folder found with the specified path");

                case Core.Constants.Trees.Stylesheets:
                    if (IsDirectory(virtualPath, SystemDirectories.Css, Current.FileSystems.StylesheetsFileSystem))
                    {
                        Services.FileService.DeleteStyleSheetFolder(virtualPath);
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    if (Services.FileService.GetStylesheetByName(virtualPath) != null)
                    {
                        Services.FileService.DeleteStylesheet(virtualPath, Security.CurrentUser.Id);
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No Stylesheet found with the specified path");

                default:
                    return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Used to create or update a 'partialview', 'partialviewmacro', 'script' or 'stylesheets' file
        /// </summary>
        /// <param name="display"></param>
        /// <returns>The updated CodeFileDisplay model</returns>
        public CodeFileDisplay PostSave(CodeFileDisplay display)
        {
            if (display == null) throw new ArgumentNullException("display");

            if (ModelState.IsValid == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            switch (display.FileType)
            {
                case Core.Constants.Trees.PartialViews:
                    var partialViewResult = CreateOrUpdatePartialView(display);
                    if (partialViewResult.Success)
                    {
                        display = Mapper.Map(partialViewResult.Result, display);
                        display.Path = Url.GetTreePathFromFilePath(partialViewResult.Result.Path);
                        display.Id = System.Web.HttpUtility.UrlEncode(partialViewResult.Result.Path);
                        return display;
                    }

                    display.AddErrorNotification(
                        Services.TextService.Localize("speechBubbles", "partialViewErrorHeader"),
                        Services.TextService.Localize("speechBubbles", "partialViewErrorText"));
                    break;

                case Core.Constants.Trees.PartialViewMacros:
                    var partialViewMacroResult = CreateOrUpdatePartialViewMacro(display);
                    if (partialViewMacroResult.Success)
                    {
                        display = Mapper.Map(partialViewMacroResult.Result, display);
                        display.Path = Url.GetTreePathFromFilePath(partialViewMacroResult.Result.Path);
                        display.Id = System.Web.HttpUtility.UrlEncode(partialViewMacroResult.Result.Path);
                        return display;
                    }

                    display.AddErrorNotification(
                        Services.TextService.Localize("speechBubbles", "partialViewErrorHeader"),
                        Services.TextService.Localize("speechBubbles", "partialViewErrorText"));
                    break;

                case Core.Constants.Trees.Scripts:

                    var scriptResult = CreateOrUpdateScript(display);
                    display = Mapper.Map(scriptResult, display);
                    display.Path = Url.GetTreePathFromFilePath(scriptResult.Path);
                    display.Id = System.Web.HttpUtility.UrlEncode(scriptResult.Path);
                    return display;

                //display.AddErrorNotification(
                //    Services.TextService.Localize("speechBubbles/partialViewErrorHeader"),
                //    Services.TextService.Localize("speechBubbles/partialViewErrorText"));

                case Core.Constants.Trees.Stylesheets:

                    var stylesheetResult = CreateOrUpdateStylesheet(display);
                    display = Mapper.Map(stylesheetResult, display);
                    display.Path = Url.GetTreePathFromFilePath(stylesheetResult.Path);
                    display.Id = System.Web.HttpUtility.UrlEncode(stylesheetResult.Path);
                    return display;

                default:
                    throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return display;
        }

        /// <summary>
        /// Extracts "umbraco style rules" from a style sheet
        /// </summary>
        /// <param name="data">The style sheet data</param>
        /// <returns>The style rules</returns>
        public StylesheetRule[] PostExtractStylesheetRules(StylesheetData data)
        {
            if (data.Content.IsNullOrWhiteSpace())
            {
                return new StylesheetRule[0];
            }

            return StylesheetHelper.ParseRules(data.Content)?.Select(rule => new StylesheetRule
            {
                Name = rule.Name,
                Selector = rule.Selector,
                Styles = rule.Styles
            }).ToArray();
        }

        /// <summary>
        /// Creates a style sheet from CSS and style rules
        /// </summary>
        /// <param name="data">The style sheet data</param>
        /// <returns>The style sheet combined from the CSS and the rules</returns>
        /// <remarks>
        /// Any "umbraco style rules" in the CSS will be removed and replaced with the rules passed in <see cref="data"/>
        /// </remarks>
        public string PostInterpolateStylesheetRules(StylesheetData data)
        {
            // first remove all existing rules
            var existingRules = data.Content.IsNullOrWhiteSpace()
                ? new Core.Strings.Css.StylesheetRule[0]
                : StylesheetHelper.ParseRules(data.Content).ToArray();
            foreach (var rule in existingRules)
            {
                data.Content = StylesheetHelper.ReplaceRule(data.Content, rule.Name, null);
            }

            data.Content = data.Content.TrimEnd(CharArrays.LineFeedCarriageReturn);

            // now add all the posted rules
            if (data.Rules != null && data.Rules.Any())
            {
                foreach (var rule in data.Rules)
                {
                    data.Content = StylesheetHelper.AppendRule(data.Content, new Core.Strings.Css.StylesheetRule
                    {
                        Name = rule.Name,
                        Selector = rule.Selector,
                        Styles = rule.Styles
                    });
                }

                data.Content += Environment.NewLine;
            }

            return data.Content;
        }

        /// <summary>
        /// Create or Update a Script
        /// </summary>
        /// <param name="display"></param>
        /// <returns></returns>
        /// <remarks>
        /// It's important to note that Scripts are DIFFERENT from cshtml files since scripts use IFileSystem and cshtml files
        /// use a normal file system because they must exist on a real file system for ASP.NET to work.
        /// </remarks>
        private Script CreateOrUpdateScript(CodeFileDisplay display)
        {
            return CreateOrUpdateFile(display, ".js", Current.FileSystems.ScriptsFileSystem,
                name => Services.FileService.GetScriptByName(name),
                (script, userId) => Services.FileService.SaveScript(script, userId),
                name => new Script(name));
        }

        private Stylesheet CreateOrUpdateStylesheet(CodeFileDisplay display)
        {
            return CreateOrUpdateFile(display, ".css", Current.FileSystems.StylesheetsFileSystem,
                name => Services.FileService.GetStylesheetByName(name),
                (stylesheet, userId) => Services.FileService.SaveStylesheet(stylesheet, userId),
                name => new Stylesheet(name)
            );
        }

        private T CreateOrUpdateFile<T>(CodeFileDisplay display, string extension, IFileSystem fileSystem,
            Func<string, T> getFileByName, Action<T, int> saveFile, Func<string, T> createFile) where T : Core.Models.File
        {
            //must always end with the correct extension
            display.Name = EnsureCorrectFileExtension(display.Name, extension);

            var virtualPath = display.VirtualPath ?? string.Empty;
            // this is all weird, should be using relative paths everywhere!
            var relPath = fileSystem.GetRelativePath(virtualPath);

            if (relPath.EndsWith(extension) == false)
            {
                //this would typically mean it's new
                relPath = relPath.IsNullOrWhiteSpace()
                    ? relPath + display.Name
                    : relPath.EnsureEndsWith('/') + display.Name;
            }

            var file = getFileByName(relPath);
            if (file != null)
            {
                // might need to find the path
                var orgPath = file.OriginalPath.Substring(0, file.OriginalPath.IndexOf(file.Name));
                file.Path = orgPath + display.Name;

                file.Content = display.Content;
                //try/catch? since this doesn't return an Attempt?
                saveFile(file, Security.CurrentUser.Id);
            }
            else
            {
                file = createFile(relPath);
                file.Content = display.Content;
                saveFile(file, Security.CurrentUser.Id);
            }

            return file;
        }

        private Attempt<IPartialView> CreateOrUpdatePartialView(CodeFileDisplay display)
        {
            return CreateOrUpdatePartialView(display, SystemDirectories.PartialViews,
                Services.FileService.GetPartialView, Services.FileService.SavePartialView, Services.FileService.CreatePartialView);
        }

        private Attempt<IPartialView> CreateOrUpdatePartialViewMacro(CodeFileDisplay display)
        {
            return CreateOrUpdatePartialView(display, SystemDirectories.MacroPartials,
                Services.FileService.GetPartialViewMacro, Services.FileService.SavePartialViewMacro, Services.FileService.CreatePartialViewMacro);
        }

        /// <summary>
        /// Helper method to take care of persisting partial views or partial view macros - so we're not duplicating the same logic
        /// </summary>
        /// <param name="display"></param>
        /// <param name="systemDirectory"></param>
        /// <param name="getView"></param>
        /// <param name="saveView"></param>
        /// <param name="createView"></param>
        /// <returns></returns>
        private Attempt<IPartialView> CreateOrUpdatePartialView(
            CodeFileDisplay display, string systemDirectory,
            Func<string, IPartialView> getView,
            Func<IPartialView, int, Attempt<IPartialView>> saveView,
            Func<IPartialView, string, int, Attempt<IPartialView>> createView)
        {
            //must always end with the correct extension
            display.Name = EnsureCorrectFileExtension(display.Name, ".cshtml");

            Attempt<IPartialView> partialViewResult;
            var virtualPath = NormalizeVirtualPath(display.VirtualPath, systemDirectory);
            var view = getView(virtualPath);
            if (view != null)
            {
                // might need to find the path
                var orgPath = view.OriginalPath.Substring(0, view.OriginalPath.IndexOf(view.Name));
                view.Path = orgPath + display.Name;

                view.Content = display.Content;
                partialViewResult = saveView(view, Security.CurrentUser.Id);
            }
            else
            {
                view = new PartialView(PartialViewType.PartialView, virtualPath + display.Name);
                view.Content = display.Content;
                partialViewResult = createView(view, display.Snippet, Security.CurrentUser.Id);
            }

            return partialViewResult;
        }

        private string NormalizeVirtualPath(string virtualPath, string systemDirectory)
        {
            if (virtualPath.IsNullOrWhiteSpace())
                return string.Empty;

            systemDirectory = systemDirectory.TrimStart("~");
            systemDirectory = systemDirectory.Replace('\\', '/');
            virtualPath = virtualPath.TrimStart("~");
            virtualPath = virtualPath.Replace('\\', '/');
            virtualPath = virtualPath.ReplaceFirst(systemDirectory, string.Empty);

            return virtualPath;
        }

        private string EnsureCorrectFileExtension(string value, string extension)
        {
            if (value.EndsWith(extension) == false)
                value += extension;

            return value;
        }

        private bool IsDirectory(string virtualPath, string systemDirectory, IFileSystem fileSystem)
        {
            var path = IOHelper.MapPath(systemDirectory + "/" + virtualPath);

            // If it's a physical filesystem check with directory info
            if (fileSystem.CanAddPhysical)
            {
                var dirInfo = new DirectoryInfo(path);

                // If you turn off indexing in Windows this will have the attribute:
                // `FileAttributes.Directory | FileAttributes.NotContentIndexed`
                return (dirInfo.Attributes & FileAttributes.Directory) != 0;
            }

            // Otherwise check the filesystem abstraction to see if the folder exists
            // Since this is used for delete, it presumably exists if we're trying to delete it
            return fileSystem.DirectoryExists(path);
        }

        // this is an internal class for passing stylesheet data from the client to the controller while editing
        public class StylesheetData
        {
            public string Content { get; set; }

            public StylesheetRule[] Rules { get; set; }
        }
    }
}
