using System.Net;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Snippets;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Strings.Css;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;
using Stylesheet = Umbraco.Cms.Core.Models.Stylesheet;
using StylesheetRule = Umbraco.Cms.Core.Models.ContentEditing.StylesheetRule;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

// TODO: Put some exception filters in our webapi to return 404 instead of 500 when we throw ArgumentNullException
// ref: https://www.exceptionnotfound.net/the-asp-net-web-api-exception-handling-pipeline-a-guided-tour/
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
//[PrefixlessBodyModelValidator]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public class CodeFileController : BackOfficeNotificationsController
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IFileService _fileService;
    private readonly FileSystems _fileSystems;
    private readonly GlobalSettings _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;

    private readonly ILocalizedTextService _localizedTextService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly PartialViewSnippetCollection _partialViewSnippetCollection;
    private readonly PartialViewMacroSnippetCollection _partialViewMacroSnippetCollection;

    [ActivatorUtilitiesConstructor]
    public CodeFileController(
        IHostingEnvironment hostingEnvironment,
        FileSystems fileSystems,
        IFileService fileService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        ILocalizedTextService localizedTextService,
        IUmbracoMapper umbracoMapper,
        IShortStringHelper shortStringHelper,
        IOptionsSnapshot<GlobalSettings> globalSettings,
        PartialViewSnippetCollection partialViewSnippetCollection,
        PartialViewMacroSnippetCollection partialViewMacroSnippetCollection)
    {
        _hostingEnvironment = hostingEnvironment;
        _fileSystems = fileSystems;
        _fileService = fileService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _localizedTextService = localizedTextService;
        _umbracoMapper = umbracoMapper;
        _shortStringHelper = shortStringHelper;
        _globalSettings = globalSettings.Value;
        _partialViewSnippetCollection = partialViewSnippetCollection;
        _partialViewMacroSnippetCollection = partialViewMacroSnippetCollection;
    }

    [Obsolete("Use ctor will all params. Scheduled for removal in V12.")]
    public CodeFileController(
        IHostingEnvironment hostingEnvironment,
        FileSystems fileSystems,
        IFileService fileService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        ILocalizedTextService localizedTextService,
        IUmbracoMapper umbracoMapper,
        IShortStringHelper shortStringHelper,
        IOptionsSnapshot<GlobalSettings> globalSettings) : this(
        hostingEnvironment,
        fileSystems,
        fileService,
        backOfficeSecurityAccessor,
        localizedTextService,
        umbracoMapper,
        shortStringHelper,
        globalSettings,
        StaticServiceProvider.Instance.GetRequiredService<PartialViewSnippetCollection>(),
        StaticServiceProvider.Instance.GetRequiredService<PartialViewMacroSnippetCollection>())
    {
    }

    /// <summary>
    ///     Used to create a brand new file
    /// </summary>
    /// <param name="type">This is a string but will be 'scripts' 'partialViews', 'partialViewMacros'</param>
    /// <param name="display"></param>
    /// <returns>Will return a simple 200 if file creation succeeds</returns>
    [ValidationFilter]
    public ActionResult<CodeFileDisplay> PostCreate(string type, CodeFileDisplay display)
    {
        if (display == null)
        {
            throw new ArgumentNullException("display");
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", "type");
        }

        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        switch (type)
        {
            case Constants.Trees.PartialViews:
                var view = new PartialView(PartialViewType.PartialView, display.VirtualPath ?? string.Empty)
                {
                    Content = display.Content,
                };
                Attempt<IPartialView?> result = _fileService.CreatePartialView(view, display.Snippet, currentUser?.Id);
                if (result.Success)
                {
                    return Ok();
                }

                return ValidationProblem(result.Exception?.Message);

            case Constants.Trees.PartialViewMacros:
                var viewMacro = new PartialView(PartialViewType.PartialViewMacro, display.VirtualPath ?? string.Empty)
                {
                    Content = display.Content,
                };
                Attempt<IPartialView?> resultMacro =
                    _fileService.CreatePartialViewMacro(viewMacro, display.Snippet, currentUser?.Id);
                if (resultMacro.Success)
                {
                    return Ok();
                }

                return ValidationProblem(resultMacro.Exception?.Message);

            case Constants.Trees.Scripts:
                var script = new Script(display.VirtualPath ?? string.Empty);
                _fileService.SaveScript(script, currentUser?.Id);
                return Ok();

            default:
                return NotFound();
        }
    }

    /// <summary>
    ///     Used to create a container/folder in 'partialViews', 'partialViewMacros', 'scripts' or 'stylesheets'
    /// </summary>
    /// <param name="type">'partialViews', 'partialViewMacros' or 'scripts'</param>
    /// <param name="parentId">The virtual path of the parent.</param>
    /// <param name="name">The name of the container/folder</param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult<CodeFileDisplay> PostCreateContainer(string type, string parentId, string name)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", "type");
        }

        if (string.IsNullOrWhiteSpace(parentId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", "parentId");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", "name");
        }

        if (name.ContainsAny(Path.GetInvalidPathChars()))
        {
            return ValidationProblem(_localizedTextService.Localize("codefile", "createFolderIllegalChars"));
        }

        // if the parentId is root (-1) then we just need an empty string as we are
        // creating the path below and we don't want -1 in the path
        if (parentId == Constants.System.RootString)
        {
            parentId = string.Empty;
        }

        name = HttpUtility.UrlDecode(name);

        if (parentId.IsNullOrWhiteSpace() == false)
        {
            parentId = HttpUtility.UrlDecode(parentId);
            name = parentId.EnsureEndsWith("/") + name;
        }

        var virtualPath = string.Empty;
        switch (type)
        {
            case Constants.Trees.PartialViews:
                virtualPath = NormalizeVirtualPath(name, Constants.SystemDirectories.PartialViews);
                _fileService.CreatePartialViewFolder(virtualPath);
                break;
            case Constants.Trees.PartialViewMacros:
                virtualPath = NormalizeVirtualPath(name, Constants.SystemDirectories.MacroPartials);
                _fileService.CreatePartialViewMacroFolder(virtualPath);
                break;
            case Constants.Trees.Scripts:
                virtualPath = NormalizeVirtualPath(name, _globalSettings.UmbracoScriptsPath);
                _fileService.CreateScriptFolder(virtualPath);
                break;
            case Constants.Trees.Stylesheets:
                virtualPath = NormalizeVirtualPath(name, _globalSettings.UmbracoCssPath);
                _fileService.CreateStyleSheetFolder(virtualPath);
                break;
        }

        return new CodeFileDisplay { VirtualPath = virtualPath, Path = Url.GetTreePathFromFilePath(virtualPath) };
    }

    /// <summary>
    ///     Used to get a specific file from disk via the FileService
    /// </summary>
    /// <param name="type">This is a string but will be 'scripts' 'partialViews', 'partialViewMacros' or 'stylesheets'</param>
    /// <param name="virtualPath">The filename or URL encoded path of the file to open</param>
    /// <returns>The file and its contents from the virtualPath</returns>
    public ActionResult<CodeFileDisplay?> GetByPath(string type, string virtualPath)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", "type");
        }

        if (string.IsNullOrWhiteSpace(virtualPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", "virtualPath");
        }

        virtualPath = HttpUtility.UrlDecode(virtualPath);

        switch (type)
        {
            case Constants.Trees.PartialViews:
                IPartialView? view = _fileService.GetPartialView(virtualPath);
                if (view != null)
                {
                    CodeFileDisplay? display = _umbracoMapper.Map<IPartialView, CodeFileDisplay>(view);

                    if (display is not null)
                    {
                        display.FileType = Constants.Trees.PartialViews;
                        display.Path = Url.GetTreePathFromFilePath(view.Path);
                        display.Id = HttpUtility.UrlEncode(view.Path);
                    }

                    return display;
                }

                break;
            case Constants.Trees.PartialViewMacros:
                IPartialView? viewMacro = _fileService.GetPartialViewMacro(virtualPath);
                if (viewMacro != null)
                {
                    CodeFileDisplay? display = _umbracoMapper.Map<IPartialView, CodeFileDisplay>(viewMacro);

                    if (display is not null)
                    {
                        display.FileType = Constants.Trees.PartialViewMacros;
                        display.Path = Url.GetTreePathFromFilePath(viewMacro.Path);
                        display.Id = HttpUtility.UrlEncode(viewMacro.Path);
                    }

                    return display;
                }

                break;
            case Constants.Trees.Scripts:
                IScript? script = _fileService.GetScript(virtualPath);
                if (script != null)
                {
                    CodeFileDisplay? display = _umbracoMapper.Map<IScript, CodeFileDisplay>(script);

                    if (display is not null)
                    {
                        display.FileType = Constants.Trees.Scripts;
                        display.Path = Url.GetTreePathFromFilePath(script.Path);
                        display.Id = HttpUtility.UrlEncode(script.Path);
                    }

                    return display;
                }

                break;
            case Constants.Trees.Stylesheets:
                IStylesheet? stylesheet = _fileService.GetStylesheet(virtualPath);
                if (stylesheet != null)
                {
                    CodeFileDisplay? display = _umbracoMapper.Map<IStylesheet, CodeFileDisplay>(stylesheet);

                    if (display is not null)
                    {
                        display.FileType = Constants.Trees.Stylesheets;
                        display.Path = Url.GetTreePathFromFilePath(stylesheet.Path);
                        display.Id = HttpUtility.UrlEncode(stylesheet.Path);
                    }

                    return display;
                }

                break;
        }

        return NotFound();
    }

    /// <summary>
    ///     Used to get a list of available templates/snippets to base a new Partial View or Partial View Macro from
    /// </summary>
    /// <param name="type">This is a string but will be 'partialViews', 'partialViewMacros'</param>
    /// <returns>Returns a list of <see cref="SnippetDisplay" /> if a correct type is sent</returns>
    public ActionResult<IEnumerable<SnippetDisplay>> GetSnippets(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", "type");
        }

        IEnumerable<string> snippets;
        switch (type)
        {
            case Constants.Trees.PartialViews:
                snippets = _partialViewSnippetCollection.GetNames();
                break;
            case Constants.Trees.PartialViewMacros:
                snippets = _partialViewMacroSnippetCollection.GetNames();
                break;
            default:
                return NotFound();
        }

        return snippets.Select(snippet => new SnippetDisplay
        {
            Name = snippet.SplitPascalCasing(_shortStringHelper).ToFirstUpperInvariant(),
            FileName = snippet,
        }).ToList();
    }

    /// <summary>
    ///     Used to scaffold the json object for the editors for 'scripts', 'partialViews', 'partialViewMacros' and
    ///     'stylesheets'
    /// </summary>
    /// <param name="type">This is a string but will be 'scripts' 'partialViews', 'partialViewMacros' or 'stylesheets'</param>
    /// <param name="id"></param>
    /// <param name="snippetName"></param>
    /// <returns></returns>
    public ActionResult<CodeFileDisplay?> GetScaffold(string type, string id, string? snippetName = null)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", "type");
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", "id");
        }

        CodeFileDisplay? codeFileDisplay;

        switch (type)
        {
            case Constants.Trees.PartialViews:
                codeFileDisplay =
                    _umbracoMapper.Map<IPartialView, CodeFileDisplay>(new PartialView(PartialViewType.PartialView, string.Empty));
                if (codeFileDisplay is not null)
                {
                    codeFileDisplay.VirtualPath = Constants.SystemDirectories.PartialViews;
                    if (snippetName.IsNullOrWhiteSpace() == false)
                    {
                            codeFileDisplay.Content = _partialViewSnippetCollection.GetContentFromName(snippetName!);
                    }
                }

                break;
            case Constants.Trees.PartialViewMacros:
                codeFileDisplay =
                    _umbracoMapper.Map<IPartialView, CodeFileDisplay>(new PartialView(PartialViewType.PartialViewMacro, string.Empty));
                if (codeFileDisplay is not null)
                {
                    codeFileDisplay.VirtualPath = Constants.SystemDirectories.MacroPartials;
                    if (snippetName.IsNullOrWhiteSpace() == false)
                    {
                        codeFileDisplay.Content = _partialViewMacroSnippetCollection.GetContentFromName(snippetName!);
                    }
                }

                break;
            case Constants.Trees.Scripts:
                codeFileDisplay = _umbracoMapper.Map<Script, CodeFileDisplay>(new Script(string.Empty));
                if (codeFileDisplay is not null)
                {
                    codeFileDisplay.VirtualPath = _globalSettings.UmbracoScriptsPath;
                }

                break;
            case Constants.Trees.Stylesheets:
                codeFileDisplay = _umbracoMapper.Map<Stylesheet, CodeFileDisplay>(new Stylesheet(string.Empty));
                if (codeFileDisplay is not null)
                {
                    codeFileDisplay.VirtualPath = _globalSettings.UmbracoCssPath;
                }

                break;
            default:
                return new UmbracoProblemResult("Unsupported editortype", HttpStatusCode.BadRequest);
        }

        if (codeFileDisplay is null)
        {
            return codeFileDisplay;
        }

        // Make sure that the root virtual path ends with '/'
        codeFileDisplay.VirtualPath = codeFileDisplay.VirtualPath?.EnsureEndsWith("/");

        if (id != Constants.System.RootString)
        {
            codeFileDisplay.VirtualPath += id.TrimStart(Constants.CharArrays.ForwardSlash).EnsureEndsWith("/");
            //if it's not new then it will have a path, otherwise it won't
            codeFileDisplay.Path = Url.GetTreePathFromFilePath(id);
        }

        codeFileDisplay.VirtualPath = codeFileDisplay.VirtualPath?.TrimStart("~");
        codeFileDisplay.FileType = type;
        return codeFileDisplay;
    }

    /// <summary>
    ///     Used to delete a specific file from disk via the FileService
    /// </summary>
    /// <param name="type">This is a string but will be 'scripts' 'partialViews', 'partialViewMacros' or 'stylesheets'</param>
    /// <param name="virtualPath">The filename or URL encoded path of the file to delete</param>
    /// <returns>Will return a simple 200 if file deletion succeeds</returns>
    [HttpDelete]
    [HttpPost]
    public IActionResult Delete(string type, string virtualPath)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", "type");
        }

        if (string.IsNullOrWhiteSpace(virtualPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", "virtualPath");
        }

        virtualPath = HttpUtility.UrlDecode(virtualPath);
        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        switch (type)
        {
            case Constants.Trees.PartialViews:
                if (IsDirectory(
                        _hostingEnvironment.MapPathContentRoot(Path.Combine(Constants.SystemDirectories.PartialViews, virtualPath))))
                {
                    _fileService.DeletePartialViewFolder(virtualPath);
                    return Ok();
                }

                if (_fileService.DeletePartialView(virtualPath, currentUser?.Id))
                {
                    return Ok();
                }

                return new UmbracoProblemResult("No Partial View or folder found with the specified path", HttpStatusCode.NotFound);

            case Constants.Trees.PartialViewMacros:
                if (IsDirectory(
                        _hostingEnvironment.MapPathContentRoot(Path.Combine(Constants.SystemDirectories.MacroPartials, virtualPath))))
                {
                    _fileService.DeletePartialViewMacroFolder(virtualPath);
                    return Ok();
                }

                if (_fileService.DeletePartialViewMacro(virtualPath, currentUser?.Id))
                {
                    return Ok();
                }

                return new UmbracoProblemResult("No Partial View Macro or folder found with the specified path", HttpStatusCode.NotFound);

            case Constants.Trees.Scripts:
                if (IsDirectory(
                        _hostingEnvironment.MapPathWebRoot(Path.Combine(_globalSettings.UmbracoScriptsPath, virtualPath))))
                {
                    _fileService.DeleteScriptFolder(virtualPath);
                    return Ok();
                }

                if (_fileService.GetScript(virtualPath) != null)
                {
                    _fileService.DeleteScript(virtualPath, currentUser?.Id);
                    return Ok();
                }

                return new UmbracoProblemResult("No Script or folder found with the specified path", HttpStatusCode.NotFound);
            case Constants.Trees.Stylesheets:
                if (IsDirectory(
                        _hostingEnvironment.MapPathWebRoot(Path.Combine(_globalSettings.UmbracoCssPath, virtualPath))))
                {
                    _fileService.DeleteStyleSheetFolder(virtualPath);
                    return Ok();
                }

                if (_fileService.GetStylesheet(virtualPath) != null)
                {
                    _fileService.DeleteStylesheet(virtualPath, currentUser?.Id);
                    return Ok();
                }

                return new UmbracoProblemResult("No Stylesheet found with the specified path", HttpStatusCode.NotFound);
            default:
                return NotFound();
        }
    }

    /// <summary>
    ///     Used to create or update a 'partialview', 'partialviewmacro', 'script' or 'stylesheets' file
    /// </summary>
    /// <param name="display"></param>
    /// <returns>The updated CodeFileDisplay model</returns>
    public ActionResult<CodeFileDisplay> PostSave(CodeFileDisplay display)
    {
        if (display == null)
        {
            throw new ArgumentNullException("display");
        }

        TryValidateModel(display);
        if (ModelState.IsValid == false)
        {
            return ValidationProblem(ModelState);
        }

        switch (display.FileType)
        {
            case Constants.Trees.PartialViews:
                Attempt<IPartialView?> partialViewResult = CreateOrUpdatePartialView(display);
                if (partialViewResult.Success)
                {
                    display = _umbracoMapper.Map(partialViewResult.Result, display);
                    display.Path = Url.GetTreePathFromFilePath(partialViewResult.Result?.Path);
                    display.Id = HttpUtility.UrlEncode(partialViewResult.Result?.Path);
                    return display;
                }

                display.AddErrorNotification(
                    _localizedTextService.Localize("speechBubbles", "partialViewErrorHeader"),
                    _localizedTextService.Localize("speechBubbles", "partialViewErrorText"));
                break;

            case Constants.Trees.PartialViewMacros:
                Attempt<IPartialView?> partialViewMacroResult = CreateOrUpdatePartialViewMacro(display);
                if (partialViewMacroResult.Success)
                {
                    display = _umbracoMapper.Map(partialViewMacroResult.Result, display);
                    display.Path = Url.GetTreePathFromFilePath(partialViewMacroResult.Result?.Path);
                    display.Id = HttpUtility.UrlEncode(partialViewMacroResult.Result?.Path);
                    return display;
                }

                display.AddErrorNotification(
                    _localizedTextService.Localize("speechBubbles", "partialViewErrorHeader"),
                    _localizedTextService.Localize("speechBubbles", "partialViewErrorText"));
                break;

            case Constants.Trees.Scripts:

                IScript? scriptResult = CreateOrUpdateScript(display);
                display = _umbracoMapper.Map(scriptResult, display);
                display.Path = Url.GetTreePathFromFilePath(scriptResult?.Path);
                display.Id = HttpUtility.UrlEncode(scriptResult?.Path);
                return display;

            //display.AddErrorNotification(
            //    _localizedTextService.Localize("speechBubbles/partialViewErrorHeader"),
            //    _localizedTextService.Localize("speechBubbles/partialViewErrorText"));

            case Constants.Trees.Stylesheets:

                IStylesheet? stylesheetResult = CreateOrUpdateStylesheet(display);
                display = _umbracoMapper.Map(stylesheetResult, display);
                display.Path = Url.GetTreePathFromFilePath(stylesheetResult?.Path);
                display.Id = HttpUtility.UrlEncode(stylesheetResult?.Path);
                return display;

            default:
                return NotFound();
        }

        return display;
    }

    /// <summary>
    ///     Extracts "umbraco style rules" from a style sheet
    /// </summary>
    /// <param name="data">The style sheet data</param>
    /// <returns>The style rules</returns>
    public StylesheetRule[]? PostExtractStylesheetRules(StylesheetData data)
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
    ///     Creates a style sheet from CSS and style rules
    /// </summary>
    /// <param name="data">The style sheet data</param>
    /// <returns>The style sheet combined from the CSS and the rules</returns>
    /// <remarks>
    ///     Any "umbraco style rules" in the CSS will be removed and replaced with the rules passed in <see cref="data" />
    /// </remarks>
    public string? PostInterpolateStylesheetRules(StylesheetData data)
    {
        // first remove all existing rules
        Core.Strings.Css.StylesheetRule[] existingRules = data.Content.IsNullOrWhiteSpace()
            ? new Core.Strings.Css.StylesheetRule[0]
            : StylesheetHelper.ParseRules(data.Content).ToArray();
        foreach (Core.Strings.Css.StylesheetRule rule in existingRules)
        {
            data.Content = StylesheetHelper.ReplaceRule(data.Content, rule.Name, null);
        }

        data.Content = data.Content?.TrimEnd(Constants.CharArrays.LineFeedCarriageReturn);

        // now add all the posted rules
        if (data.Rules != null && data.Rules.Any())
        {
            foreach (StylesheetRule rule in data.Rules)
            {
                data.Content = StylesheetHelper.AppendRule(
                    data.Content,
                    new Core.Strings.Css.StylesheetRule
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
    ///     Create or Update a Script
    /// </summary>
    /// <param name="display"></param>
    /// <returns></returns>
    /// <remarks>
    ///     It's important to note that Scripts are DIFFERENT from cshtml files since scripts use IFileSystem and cshtml files
    ///     use a normal file system because they must exist on a real file system for ASP.NET to work.
    /// </remarks>
    private IScript? CreateOrUpdateScript(CodeFileDisplay display) =>
        CreateOrUpdateFile(
            display,
            ".js",
            _fileSystems.ScriptsFileSystem,
            name => _fileService.GetScript(name),
            (script, userId) => _fileService.SaveScript(script, userId),
            name => new Script(name ?? string.Empty));

    private IStylesheet? CreateOrUpdateStylesheet(CodeFileDisplay display) =>
        CreateOrUpdateFile(
            display,
            ".css",
            _fileSystems.StylesheetsFileSystem,
            name => _fileService.GetStylesheet(name),
            (stylesheet, userId) => _fileService.SaveStylesheet(stylesheet, userId),
            name => new Stylesheet(name ?? string.Empty));

    private T CreateOrUpdateFile<T>(CodeFileDisplay display, string extension, IFileSystem? fileSystem, Func<string?, T> getFileByName, Action<T, int?> saveFile, Func<string?, T> createFile)
        where T : IFile?
    {
        //must always end with the correct extension
        display.Name = EnsureCorrectFileExtension(display.Name, extension);

        var virtualPath = display.VirtualPath ?? string.Empty;
        // this is all weird, should be using relative paths everywhere!
        var relPath = fileSystem?.GetRelativePath(virtualPath);

        if (relPath?.EndsWith(extension) == false)
        {
            //this would typically mean it's new
            relPath = relPath.IsNullOrWhiteSpace()
                ? relPath + display.Name
                : relPath.EnsureEndsWith('/') + display.Name;
        }

        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        T file = getFileByName(relPath);
        if (file != null)
        {
            // might need to find the path
            var orgPath = file.Name is null
                ? string.Empty
                : file.OriginalPath.Substring(0, file.OriginalPath.IndexOf(file.Name));
            file.Path = orgPath + display.Name;

            file.Content = display.Content;
            //try/catch? since this doesn't return an Attempt?
            saveFile(file, currentUser?.Id);
        }
        else
        {
            file = createFile(relPath);
            if (file is not null)
            {
                file.Content = display.Content;
            }

            saveFile(file, currentUser?.Id);
        }

        return file;
    }

    private Attempt<IPartialView?> CreateOrUpdatePartialView(CodeFileDisplay display) =>
        CreateOrUpdatePartialView(
            display,
            Constants.SystemDirectories.PartialViews,
            _fileService.GetPartialView,
            _fileService.SavePartialView,
            _fileService.CreatePartialView);

    private Attempt<IPartialView?> CreateOrUpdatePartialViewMacro(CodeFileDisplay display) =>
        CreateOrUpdatePartialView(display, Constants.SystemDirectories.MacroPartials, _fileService.GetPartialViewMacro, _fileService.SavePartialViewMacro, _fileService.CreatePartialViewMacro);

    /// <summary>
    ///     Helper method to take care of persisting partial views or partial view macros - so we're not duplicating the same
    ///     logic
    /// </summary>
    /// <param name="display"></param>
    /// <param name="systemDirectory"></param>
    /// <param name="getView"></param>
    /// <param name="saveView"></param>
    /// <param name="createView"></param>
    /// <returns></returns>
    private Attempt<IPartialView?> CreateOrUpdatePartialView(
        CodeFileDisplay display,
        string systemDirectory,
        Func<string, IPartialView?> getView,
        Func<IPartialView, int?,
            Attempt<IPartialView?>> saveView,
        Func<IPartialView, string?,
            int?, Attempt<IPartialView?>> createView)
    {
        //must always end with the correct extension
        display.Name = EnsureCorrectFileExtension(display.Name, ".cshtml");

        Attempt<IPartialView?> partialViewResult;
        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;

        var virtualPath = NormalizeVirtualPath(display.VirtualPath, systemDirectory);
        IPartialView? view = getView(virtualPath);
        if (view != null)
        {
            // might need to find the path
            var orgPath = view.OriginalPath.Substring(0, view.OriginalPath.IndexOf(view.Name ?? string.Empty));
            view.Path = orgPath + display.Name;

            view.Content = display.Content;
            partialViewResult = saveView(view, currentUser?.Id);
        }
        else
        {
            view = new PartialView(PartialViewType.PartialView, virtualPath + display.Name)
            {
                Content = display.Content
            };
            partialViewResult = createView(view, display.Snippet, currentUser?.Id);
        }

        return partialViewResult;
    }

    private string NormalizeVirtualPath(string? virtualPath, string systemDirectory)
    {
        if (virtualPath.IsNullOrWhiteSpace())
        {
            return string.Empty;
        }

        systemDirectory = systemDirectory.TrimStart("~");
        systemDirectory = systemDirectory.Replace('\\', '/');
        virtualPath = virtualPath!.TrimStart("~");
        virtualPath = virtualPath.Replace('\\', '/');
        virtualPath = virtualPath.ReplaceFirst(systemDirectory, string.Empty);

        return virtualPath;
    }

    private string? EnsureCorrectFileExtension(string? value, string extension)
    {
        if (value?.EndsWith(extension) == false)
        {
            value += extension;
        }

        return value;
    }

    private bool IsDirectory(string path)
    {
        var dirInfo = new DirectoryInfo(path);

        // If you turn off indexing in Windows this will have the attribute:
        // `FileAttributes.Directory | FileAttributes.NotContentIndexed`
        return (dirInfo.Attributes & FileAttributes.Directory) != 0;
    }

    // this is an internal class for passing stylesheet data from the client to the controller while editing
    public class StylesheetData
    {
        public string? Content { get; set; }

        public StylesheetRule[]? Rules { get; set; }
    }
}
