using AutoMapper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ClientDependency.Core;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [PrefixlessBodyModelValidator]
    [UmbracoApplicationAuthorizeAttribute(Core.Constants.Applications.Settings)]
    public class CodeFileController : BackOfficeNotificationsController
    {

        /// <summary>
        /// Used to create a brand new file
        /// </summary>
        /// <param name="type">This is a string but will be 'scripts' 'partialViews', 'partialViewMacros'</param>
        /// <param name="display"></param>
        /// <returns>Will return a simple 200 if file creation succeeds</returns>
        [ValidationFilter]
        public HttpResponseMessage PostCreate(string type, CodeFileDisplay display)
        {
            switch (type)
            {
                case Core.Constants.Trees.PartialViews:
                    var view = new PartialView(display.VirtualPath);
                    view.Content = display.Content;
                    var result = Services.FileService.CreatePartialView(view, display.Snippet, Security.CurrentUser.Id);
                    return result.Success == true ? Request.CreateResponse(HttpStatusCode.OK) : Request.CreateNotificationValidationErrorResponse(result.Exception.Message);

                case Core.Constants.Trees.PartialViewMacros:
                    var viewMacro = new PartialView(display.VirtualPath);
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
        /// Used to get a specific file from disk via the FileService
        /// </summary>
        /// <param name="type">This is a string but will be 'scripts' 'partialViews', 'partialViewMacros'</param>
        /// <param name="virtualPath">The filename or urlencoded path of the file to open</param>
        /// <returns>The file and its contents from the virtualPath</returns>
        public CodeFileDisplay GetByPath(string type, string virtualPath)
        {
            if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(virtualPath))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

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
                        return display;
                    }
                    return null;

                case Core.Constants.Trees.PartialViewMacros:
                    var viewMacro = Services.FileService.GetPartialViewMacro(virtualPath);
                    if (viewMacro != null)
                    {
                        var display = Mapper.Map<IPartialView, CodeFileDisplay>(viewMacro);
                        display.FileType = Core.Constants.Trees.PartialViewMacros;
                        display.Path = Url.GetTreePathFromFilePath(viewMacro.Path);
                        return display;
                    }
                    return null;

                case Core.Constants.Trees.Scripts:
                    var script = Services.FileService.GetScriptByName(virtualPath);
                    if (script != null)
                    {
                        var display = Mapper.Map<Script, CodeFileDisplay>(script);
                        display.FileType = Core.Constants.Trees.Scripts;
                        display.Path = Url.GetTreePathFromFilePath(script.Path);
                        return display;
                    }
                    return null;
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Used to get a list of available templates/snippets to base a new Partial View og Partial View Macro from
        /// </summary>
        /// <param name="type">This is a string but will be 'partialViews', 'partialViewMacros'</param>
        /// <returns>Returns a list of <see cref="SnippetDisplay"/> if a correct type is sent</returns>
        public IEnumerable<SnippetDisplay> GetSnippets(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

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
        /// Used to scaffold the json object for the editors for 'scripts', 'partialViews', 'partialViewMacros'
        /// </summary>
        /// <param name="type">This is a string but will be 'scripts' 'partialViews', 'partialViewMacros'</param>
        /// <param name="id"></param>
        /// <param name="snippetName"></param>
        /// <returns></returns>
        public CodeFileDisplay GetScaffold(string type, string id = null, string snippetName = null)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            if (id.IsNullOrWhiteSpace())
                id = string.Empty;

            CodeFileDisplay codeFileDisplay;

            switch (type)
            {
                case Core.Constants.Trees.PartialViews:
                    codeFileDisplay = Mapper.Map<IPartialView, CodeFileDisplay>(new PartialView(string.Empty));
                    codeFileDisplay.VirtualPath = SystemDirectories.PartialViews;
                    if (snippetName.IsNullOrWhiteSpace() == false)
                        codeFileDisplay.Content = Services.FileService.GetPartialViewSnippetContent(snippetName);
                    break;
                case Core.Constants.Trees.PartialViewMacros:
                    codeFileDisplay = Mapper.Map<IPartialView, CodeFileDisplay>(new PartialView(string.Empty));
                    codeFileDisplay.VirtualPath = SystemDirectories.MacroPartials;
                    if (snippetName.IsNullOrWhiteSpace() == false)
                        codeFileDisplay.Content = Services.FileService.GetPartialViewMacroSnippetContent(snippetName);
                    break;
                case Core.Constants.Trees.Scripts:
                    codeFileDisplay = Mapper.Map<Script, CodeFileDisplay>(new Script(string.Empty));
                    codeFileDisplay.VirtualPath = SystemDirectories.Scripts;
                    break;
                default:
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unsupported editortype"));
            }

            // Make sure that the root virtual path ends with '/'
            codeFileDisplay.VirtualPath = codeFileDisplay.VirtualPath.EnsureEndsWith("/");

            if (id.IsNullOrWhiteSpace() == false && id != Core.Constants.System.Root.ToInvariantString())
            {
                codeFileDisplay.VirtualPath += id.TrimStart("/").EnsureEndsWith("/");
            }

            codeFileDisplay.VirtualPath = codeFileDisplay.VirtualPath.TrimStart("~");
            codeFileDisplay.Path = Url.GetTreePathFromFilePath(id);
            codeFileDisplay.FileType = type;

            return codeFileDisplay;
        }

        /// <summary>
        /// Used to delete a specific file from disk via the FileService
        /// </summary>
        /// <param name="type">This is a string but will be 'scripts' 'partialViews', 'partialViewMacros'</param>
        /// <param name="virtualPath">The filename or urlencoded path of the file to delete</param>
        /// <returns>Will return a simple 200 if file deletion succeeds</returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage Delete(string type, string virtualPath)
        {
            if (string.IsNullOrWhiteSpace(type) == false && string.IsNullOrWhiteSpace(virtualPath) == false)
            {
                virtualPath = System.Web.HttpUtility.UrlDecode(virtualPath);

                switch (type)
                {
                    case Core.Constants.Trees.PartialViews:
                        if (IsDirectory(virtualPath, SystemDirectories.PartialViews))
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
                        if (IsDirectory(virtualPath, SystemDirectories.MacroPartials))
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
                        if (IsDirectory(virtualPath, SystemDirectories.Scripts))
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

                    default:
                        return Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Used to create or update a 'partialview', 'partialviewmacro' or 'script' file
        /// </summary>
        /// <param name="display"></param>
        /// <returns>The updated CodeFileDisplay model</returns>
        public CodeFileDisplay PostSave(CodeFileDisplay display)
        {
            if (ModelState.IsValid == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            if (display == null || string.IsNullOrWhiteSpace(display.FileType))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            switch (display.FileType)
            {
                case Core.Constants.Trees.PartialViews:
                    var partialViewResult = CreateOrUpdatePartialView(display);
                    if (partialViewResult.Success)
                    {
                        display = Mapper.Map(partialViewResult.Result, display);
                        display.Path = Url.GetTreePathFromFilePath(partialViewResult.Result.Path);
                        return display;
                    }

                    display.AddErrorNotification(
                        Services.TextService.Localize("speechBubbles/partialViewErrorHeader"),
                        Services.TextService.Localize("speechBubbles/partialViewErrorText"));
                    break;

                case Core.Constants.Trees.PartialViewMacros:
                    var partialViewMacroResult = CreateOrUpdatePartialViewMacro(display);
                    if (partialViewMacroResult.Success)
                    {
                        display = Mapper.Map(partialViewMacroResult.Result, display);
                        display.Path = Url.GetTreePathFromFilePath(partialViewMacroResult.Result.Path);
                        return display;
                    }
                        
                    display.AddErrorNotification(
                        Services.TextService.Localize("speechBubbles/partialViewErrorHeader"),
                        Services.TextService.Localize("speechBubbles/partialViewErrorText"));
                    break;

                case Core.Constants.Trees.Scripts:
                    var virtualPath = display.VirtualPath;
                    var script = Services.FileService.GetScriptByName(display.VirtualPath);
                    if (script != null)
                    {
                        script.Path = display.Name;
                        display = Mapper.Map(script, display);
                        display.Path = Url.GetTreePathFromFilePath(script.Path);
                        return display;
                        
                    }
                    else
                    {
                        var fileName = EnsurePartialViewExtension(display.Name, ".js");
                        script = new Script(virtualPath + fileName);
                    }

                    script.Content = display.Content;

                    Services.FileService.SaveScript(script, Security.CurrentUser.Id);
                    break;

                default:
                    throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return display;
        }

        private Attempt<IPartialView> CreateOrUpdatePartialView(CodeFileDisplay display)
        {
            Attempt<IPartialView> partialViewResult;
            string virtualPath = NormalizeVirtualPath(display.VirtualPath, SystemDirectories.PartialViews);
            var view = Services.FileService.GetPartialView(virtualPath);
            if (view != null)
            {
                // might need to find the path
                var orgPath = view.OriginalPath.Substring(0, view.OriginalPath.IndexOf(view.Name));
                view.Path = orgPath + display.Name;

                view.Content = display.Content;
                partialViewResult = Services.FileService.SavePartialView(view, Security.CurrentUser.Id);
            }
            else
            {
                var fileName = EnsurePartialViewExtension(display.Name, ".cshtml");
                view = new PartialView(virtualPath + fileName);
                view.Content = display.Content;
                partialViewResult = Services.FileService.CreatePartialView(view, display.Snippet, Security.CurrentUser.Id);
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

        private Attempt<IPartialView> CreateOrUpdatePartialViewMacro(CodeFileDisplay display)
        {
            Attempt<IPartialView> partialViewMacroResult;
            var virtualPath = display.VirtualPath ?? string.Empty;
            var viewMacro = Services.FileService.GetPartialViewMacro(virtualPath);
            if (viewMacro != null)
            {
                viewMacro.Content = display.Content;
                viewMacro.Path = display.Name;
                partialViewMacroResult = Services.FileService.SavePartialViewMacro(viewMacro, Security.CurrentUser.Id);
            }
            else
            {
                var fileName = EnsurePartialViewExtension(display.Name, ".cshtml");
                viewMacro = new PartialView(virtualPath + fileName);
                viewMacro.Content = display.Content;
                partialViewMacroResult = Services.FileService.CreatePartialViewMacro(viewMacro, display.Snippet, Security.CurrentUser.Id);
            }

            return partialViewMacroResult;
        }

        private string EnsurePartialViewExtension(string value, string extension)
        {
            if (value.EndsWith(extension) == false)
                value += extension;

            return value;
        }

        private bool IsDirectory(string virtualPath, string systemDirectory)
        {
            var path = IOHelper.MapPath(systemDirectory + "/" + virtualPath);
            var dirInfo = new DirectoryInfo(path);
            return dirInfo.Attributes == FileAttributes.Directory;
        }
    }
}
