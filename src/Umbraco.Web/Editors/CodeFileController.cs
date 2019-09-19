using System;
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
    //TODO: Put some exception filters in our webapi to return 404 instead of 500 when we throw ArgumentNullException
    // ref: https://www.exceptionnotfound.net/the-asp-net-web-api-exception-handling-pipeline-a-guided-tour/
    [PluginController("UmbracoApi")]
    [PrefixlessBodyModelValidator]
    [UmbracoApplicationAuthorize(Core.Constants.Applications.Settings)]
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
        /// Used to create a container/folder in 'partialViews', 'partialViewMacros' or 'scripts'
        /// </summary>
        /// <param name="type">'partialViews', 'partialViewMacros' or 'scripts'</param>
        /// <param name="parentId">The virtual path of the parent.</param>
        /// <param name="name">The name of the container/folder</param>
        /// <returns></returns>
        [HttpPost]
        public CodeFileDisplay PostCreateContainer(string type, string parentId, string name)
        {
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Value cannot be null or whitespace.", "type");
            if (string.IsNullOrWhiteSpace(parentId)) throw new ArgumentException("Value cannot be null or whitespace.", "parentId");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", "name");

            // if the parentId is root (-1) then we just need an empty string as we are
            // creating the path below and we don't wan't -1 in the path
            if (parentId == Core.Constants.System.Root.ToInvariantString())
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

            }

            return new CodeFileDisplay
            {
                VirtualPath = virtualPath,
                Path = Url.GetTreePathFromFilePath(virtualPath)
            };
        }

        /// <summary>
        /// Used to get a specific file from disk via the FileService
        /// </summary>
        /// <param name="type">This is a string but will be 'scripts' 'partialViews', 'partialViewMacros'</param>
        /// <param name="virtualPath">The filename or urlencoded path of the file to open</param>
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
        /// Used to scaffold the json object for the editors for 'scripts', 'partialViews', 'partialViewMacros'
        /// </summary>
        /// <param name="type">This is a string but will be 'scripts' 'partialViews', 'partialViewMacros'</param>
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
                default:
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unsupported editortype"));
            }

            // Make sure that the root virtual path ends with '/'
            codeFileDisplay.VirtualPath = codeFileDisplay.VirtualPath.EnsureEndsWith("/");

            if (id != Core.Constants.System.Root.ToInvariantString())
            {
                codeFileDisplay.VirtualPath += id.TrimStart("/").EnsureEndsWith("/");
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
        /// <param name="type">This is a string but will be 'scripts' 'partialViews', 'partialViewMacros'</param>
        /// <param name="virtualPath">The filename or urlencoded path of the file to delete</param>
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

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Used to create or update a 'partialview', 'partialviewmacro' or 'script' file
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
                        Services.TextService.Localize("speechBubbles/partialViewErrorHeader"),
                        Services.TextService.Localize("speechBubbles/partialViewErrorText"));
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
                        Services.TextService.Localize("speechBubbles/partialViewErrorHeader"),
                        Services.TextService.Localize("speechBubbles/partialViewErrorText"));
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

                    break;


                    

                default:
                    throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return display;
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
            //must always end with the correct extension
            display.Name = EnsureCorrectFileExtension(display.Name, ".js");

            var virtualPath = display.VirtualPath ?? string.Empty;
            // this is all weird, should be using relative paths everywhere!
            var relPath = FileSystemProviderManager.Current.ScriptsFileSystem.GetRelativePath(virtualPath);

            if (relPath.EndsWith(".js") == false)
            {
                //this would typically mean it's new
                relPath = relPath.IsNullOrWhiteSpace()
                    ? relPath + display.Name
                    : relPath.EnsureEndsWith('/') + display.Name;
            }            

            var script = Services.FileService.GetScriptByName(relPath);
            if (script != null)
            {
                // might need to find the path
                var orgPath = script.OriginalPath.Substring(0, script.OriginalPath.IndexOf(script.Name));
                script.Path = orgPath + display.Name;

                script.Content = display.Content;
                //try/catch? since this doesn't return an Attempt?
                Services.FileService.SaveScript(script, Security.CurrentUser.Id);
            }
            else
            {
                script = new Script(relPath);
                script.Content = display.Content;
                Services.FileService.SaveScript(script, Security.CurrentUser.Id);
            }

            return script;
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

        private bool IsDirectory(string virtualPath, string systemDirectory)
        {
            var path = IOHelper.MapPath(systemDirectory + "/" + virtualPath);
            var dirInfo = new DirectoryInfo(path);
            return dirInfo.Attributes == FileAttributes.Directory;
        }
    }
}
