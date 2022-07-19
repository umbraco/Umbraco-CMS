using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     The API controller used for editing dictionary items
/// </summary>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMacros)]
[ParameterSwapControllerActionSelector(nameof(GetById), "id", typeof(int), typeof(Guid), typeof(Udi))]
public class MacrosController : BackOfficeNotificationsController
{
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILogger<MacrosController> _logger;
    private readonly IMacroService _macroService;
    private readonly ParameterEditorCollection _parameterEditorCollection;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IUmbracoMapper _umbracoMapper;

    public MacrosController(
        ParameterEditorCollection parameterEditorCollection,
        IMacroService macroService,
        IShortStringHelper shortStringHelper,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        ILogger<MacrosController> logger,
        IHostingEnvironment hostingEnvironment,
        IUmbracoMapper umbracoMapper)
    {
        _parameterEditorCollection = parameterEditorCollection ??
                                     throw new ArgumentNullException(nameof(parameterEditorCollection));
        _macroService = macroService ?? throw new ArgumentNullException(nameof(macroService));
        _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
        _backofficeSecurityAccessor = backofficeSecurityAccessor ??
                                      throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
    }


    /// <summary>
    ///     Creates a new macro
    /// </summary>
    /// <param name="name">
    ///     The name.
    /// </param>
    /// <returns>
    ///     The id of the created macro
    /// </returns>
    [HttpPost]
    public ActionResult<int> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ValidationProblem("Name can not be empty");
        }

        var alias = name.ToSafeAlias(_shortStringHelper);

        if (_macroService.GetByAlias(alias) != null)
        {
            return ValidationProblem("Macro with this alias already exists");
        }

        if (name == null || name.Length > 255)
        {
            return ValidationProblem("Name cannnot be more than 255 characters in length.");
        }

        try
        {
            var macro = new Macro(_shortStringHelper) { Alias = alias, Name = name, MacroSource = string.Empty };

            _macroService.Save(macro, _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ?? -1);

            return macro.Id;
        }
        catch (Exception exception)
        {
            const string errorMessage = "Error creating macro";
            _logger.LogError(exception, errorMessage);
            return ValidationProblem(errorMessage);
        }
    }

    [HttpGet]
    public ActionResult<MacroDisplay?> GetById(int id)
    {
        IMacro? macro = _macroService.GetById(id);

        if (macro == null)
        {
            return ValidationProblem($"Macro with id {id} does not exist");
        }

        MacroDisplay? macroDisplay = MapToDisplay(macro);

        return macroDisplay;
    }

    [HttpGet]
    public ActionResult<MacroDisplay?> GetById(Guid id)
    {
        IMacro? macro = _macroService.GetById(id);

        if (macro == null)
        {
            return ValidationProblem($"Macro with id {id} does not exist");
        }

        MacroDisplay? macroDisplay = MapToDisplay(macro);

        return macroDisplay;
    }

    [HttpGet]
    public ActionResult<MacroDisplay?> GetById(Udi id)
    {
        var guidUdi = id as GuidUdi;
        if (guidUdi == null)
        {
            return ValidationProblem($"Macro with id {id} does not exist");
        }

        IMacro? macro = _macroService.GetById(guidUdi.Guid);
        if (macro == null)
        {
            return ValidationProblem($"Macro with id {id} does not exist");
        }

        MacroDisplay? macroDisplay = MapToDisplay(macro);

        return macroDisplay;
    }

    [HttpPost]
    public IActionResult DeleteById(int id)
    {
        IMacro? macro = _macroService.GetById(id);

        if (macro == null)
        {
            return ValidationProblem($"Macro with id {id} does not exist");
        }

        _macroService.Delete(macro);

        return Ok();
    }

    [HttpPost]
    public ActionResult<MacroDisplay> Save(MacroDisplay macroDisplay)
    {
        if (macroDisplay == null)
        {
            return ValidationProblem("No macro data found in request");
        }

        if (macroDisplay.Name == null || macroDisplay.Name.Length > 255)
        {
            return ValidationProblem("Name cannnot be more than 255 characters in length.");
        }

        IMacro? macro = macroDisplay.Id is null
            ? null
            : _macroService.GetById(int.Parse(macroDisplay.Id.ToString()!, CultureInfo.InvariantCulture));

        if (macro == null)
        {
            return ValidationProblem($"Macro with id {macroDisplay.Id} does not exist");
        }

        if (macroDisplay.Alias != macro.Alias)
        {
            IMacro? macroByAlias = _macroService.GetByAlias(macroDisplay.Alias);

            if (macroByAlias != null)
            {
                return ValidationProblem("Macro with this alias already exists");
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
        macro.Properties.ReplaceAll(
            macroDisplay.Parameters.Select((x, i) => new MacroProperty(x.Key, x.Label, i, x.Editor)));

        try
        {
            _macroService.Save(macro, _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id ?? -1);

            macroDisplay.Notifications.Clear();

            macroDisplay.Notifications.Add(new BackOfficeNotification("Success", "Macro saved", NotificationStyle.Success));

            return macroDisplay;
        }
        catch (Exception exception)
        {
            const string errorMessage = "Error creating macro";
            _logger.LogError(exception, errorMessage);
            return ValidationProblem(errorMessage);
        }
    }

    /// <summary>
    ///     Gets a list of available macro partials
    /// </summary>
    /// <returns>
    ///     The <see cref="HttpResponseMessage" />.
    /// </returns>
    public IEnumerable<string> GetPartialViews()
    {
        var views = new List<string>();

        views.AddRange(FindPartialViewsFiles());

        return views;
    }

    /// <summary>
    ///     Gets the available parameter editors
    /// </summary>
    /// <returns>
    ///     The <see cref="HttpResponseMessage" />.
    /// </returns>
    public ParameterEditorCollection GetParameterEditors() => _parameterEditorCollection;

    /// <summary>
    ///     Gets the available parameter editors grouped by their group.
    /// </summary>
    /// <returns>
    ///     The <see cref="HttpResponseMessage" />.
    /// </returns>
    public IDictionary<string, IEnumerable<IDataEditor>> GetGroupedParameterEditors()
    {
        IDataEditor[] parameterEditors = _parameterEditorCollection.ToArray();

        var grouped = parameterEditors
            .GroupBy(x => x.Group.IsNullOrWhiteSpace() ? string.Empty : x.Group.ToLower())
            .OrderBy(x => x.Key)
            .ToDictionary(group => group.Key, group => group.OrderBy(d => d.Name).AsEnumerable());

        return grouped;
    }

    /// <summary>
    ///     Get parameter editor by alias.
    /// </summary>
    /// <returns>
    ///     The <see cref="HttpResponseMessage" />.
    /// </returns>
    public IDataEditor? GetParameterEditorByAlias(string alias)
    {
        IDataEditor[] parameterEditors = _parameterEditorCollection.ToArray();

        IDataEditor? parameterEditor = parameterEditors.FirstOrDefault(x => x.Alias.InvariantEquals(alias));

        return parameterEditor;
    }


    /// <summary>
    ///     Finds all the macro partials
    /// </summary>
    /// <returns>
    ///     The <see cref="IEnumerable" />.
    /// </returns>
    private IEnumerable<string> FindPartialViewsFiles()
    {
        var files = new List<string>();

        files.AddRange(FindPartialViewFilesInViewsFolder());
        files.AddRange(FindPartialViewFilesInPluginFolders());

        return files;
    }

    /// <summary>
    ///     Finds all macro partials in the views folder
    /// </summary>
    /// <returns>
    ///     The <see cref="IEnumerable" />.
    /// </returns>
    private IEnumerable<string> FindPartialViewFilesInViewsFolder()
    {
        // TODO: This is inconsistent. We have FileSystems.MacroPartialsFileSystem but we basically don't use
        // that at all except to render the tree. In the future we may want to use it. This also means that
        // we are storing the virtual path of the macro like ~/Views/MacroPartials/Login.cshtml instead of the
        // relative path which would work with the FileSystems.MacroPartialsFileSystem, but these are incompatible.
        // At some point this should all be made consistent and probably just use FileSystems.MacroPartialsFileSystem.

        var partialsDir = _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.MacroPartials);

        return FindPartialViewFilesInFolder(
            partialsDir,
            partialsDir,
            Constants.SystemDirectories.MacroPartials);
    }

    /// <summary>
    ///     Finds partial view files in app plugin folders.
    /// </summary>
    /// <returns>
    /// </returns>
    private IEnumerable<string> FindPartialViewFilesInPluginFolders()
    {
        var files = new List<string>();

        var appPluginsFolder =
            new DirectoryInfo(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.AppPlugins));

        if (!appPluginsFolder.Exists)
        {
            return files;
        }

        foreach (DirectoryInfo directory in appPluginsFolder.GetDirectories())
        {
            DirectoryInfo[] viewsFolder = directory.GetDirectories("Views");
            if (viewsFolder.Any())
            {
                DirectoryInfo[] macroPartials = viewsFolder.First().GetDirectories("MacroPartials");
                if (macroPartials.Any())
                {
                    files.AddRange(FindPartialViewFilesInFolder(
                        macroPartials.First().FullName,
                        macroPartials.First().FullName,
                        Constants.SystemDirectories.AppPlugins + "/" + directory.Name + "/Views/MacroPartials"));
                }
            }
        }

        return files;
    }

    /// <summary>
    ///     Finds all partial views in a folder and subfolders
    /// </summary>
    /// <param name="orgPath">
    ///     The org path.
    /// </param>
    /// <param name="path">
    ///     The path.
    /// </param>
    /// <param name="prefixVirtualPath">
    ///     The prefix virtual path.
    /// </param>
    /// <returns>
    ///     The <see cref="IEnumerable" />.
    /// </returns>
    private IEnumerable<string> FindPartialViewFilesInFolder(string orgPath, string path, string prefixVirtualPath)
    {
        var files = new List<string>();
        var dirInfo = new DirectoryInfo(path);

        if (dirInfo.Exists)
        {
            foreach (DirectoryInfo dir in dirInfo.GetDirectories())
            {
                files.AddRange(FindPartialViewFilesInFolder(orgPath, path + "/" + dir.Name, prefixVirtualPath));
            }

            FileInfo[] fileInfo = dirInfo.GetFiles("*.*");

            files.AddRange(
                fileInfo.Select(file =>
                    prefixVirtualPath.TrimEnd(Constants.CharArrays.ForwardSlash) + "/" +
                    (path.Replace(orgPath, string.Empty).Trim(Constants.CharArrays.ForwardSlash) + "/" + file.Name)
                    .Trim(Constants.CharArrays.ForwardSlash)));
        }

        return files;
    }

    /// <summary>
    ///     Used to map an <see cref="IMacro" /> instance to a <see cref="MacroDisplay" />
    /// </summary>
    /// <param name="macro"></param>
    /// <returns></returns>
    private MacroDisplay? MapToDisplay(IMacro macro)
    {
        MacroDisplay? display = _umbracoMapper.Map<MacroDisplay>(macro);

        IEnumerable<MacroParameterDisplay> parameters = macro.Properties.Values
            .OrderBy(x => x.SortOrder)
            .Select(x => new MacroParameterDisplay { Editor = x.EditorAlias, Key = x.Alias, Label = x.Name, Id = x.Id });

        if (display is not null)
        {
            display.Parameters = parameters;
        }

        return display;
    }
}
