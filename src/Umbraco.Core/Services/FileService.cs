using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using File = System.IO.File;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Represents the File Service, which is an easy access to operations involving <see cref="IFile" /> objects like
///     Scripts, Stylesheets and Templates
/// </summary>
public class FileService : RepositoryService, IFileService
{
    private const string PartialViewHeader = "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage";
    private readonly IAuditRepository _auditRepository;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IPartialViewRepository _partialViewRepository;
    private readonly IScriptRepository _scriptRepository;
    private readonly IStylesheetRepository _stylesheetRepository;
    private readonly ITemplateService _templateService;
    private readonly ITemplateRepository _templateRepository;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    [Obsolete("Use other ctor - will be removed in Umbraco 15")]
    public FileService(
        ICoreScopeProvider uowProvider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IStylesheetRepository stylesheetRepository,
        IScriptRepository scriptRepository,
        ITemplateRepository templateRepository,
        IPartialViewRepository partialViewRepository,
        IAuditRepository auditRepository,
        IShortStringHelper shortStringHelper,
        IOptions<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment)
        : this(
            uowProvider,
            loggerFactory,
            eventMessagesFactory,
            stylesheetRepository,
            scriptRepository,
            partialViewRepository,
            auditRepository,
            hostingEnvironment,
            StaticServiceProvider.Instance.GetRequiredService<ITemplateService>(),
            templateRepository,
            StaticServiceProvider.Instance.GetRequiredService<IUserIdKeyResolver>(),
            shortStringHelper,
            globalSettings)
    {
    }

    [ActivatorUtilitiesConstructor]
    public FileService(
        ICoreScopeProvider uowProvider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IStylesheetRepository stylesheetRepository,
        IScriptRepository scriptRepository,
        IPartialViewRepository partialViewRepository,
        IAuditRepository auditRepository,
        IHostingEnvironment hostingEnvironment,
        ITemplateService templateService,
        ITemplateRepository templateRepository,
        IUserIdKeyResolver userIdKeyResolver,
        // We need these else it will be ambigious ctors
        IShortStringHelper shortStringHelper,
        IOptions<GlobalSettings> globalSettings)
        : base(uowProvider, loggerFactory, eventMessagesFactory)
    {
        _stylesheetRepository = stylesheetRepository;
        _scriptRepository = scriptRepository;
        _partialViewRepository = partialViewRepository;
        _auditRepository = auditRepository;
        _hostingEnvironment = hostingEnvironment;
        _templateService = templateService;
        _templateRepository = templateRepository;
        _userIdKeyResolver = userIdKeyResolver;
    }

    #region Stylesheets

    /// <inheritdoc />
    [Obsolete("Please use IStylesheetService for stylesheet operations - will be removed in Umbraco 15")]
    public IEnumerable<IStylesheet> GetStylesheets(params string[] paths)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _stylesheetRepository.GetMany(paths);
        }
    }

    private void Audit(AuditType type, int userId, int objectId, string? entityType) =>
        _auditRepository.Save(new AuditItem(objectId, type, userId, entityType));

    /// <inheritdoc />
    [Obsolete("Please use IStylesheetService for stylesheet operations - will be removed in Umbraco 15")]
    public IStylesheet? GetStylesheet(string? path)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _stylesheetRepository.Get(path);
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IStylesheetService for stylesheet operations - will be removed in Umbraco 15")]
    public void SaveStylesheet(IStylesheet? stylesheet, int? userId = null)
    {
        if (stylesheet is null)
        {
            return;
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            var savingNotification = new StylesheetSavingNotification(stylesheet, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return;
            }

            userId ??= Constants.Security.SuperUserId;
            _stylesheetRepository.Save(stylesheet);
            scope.Notifications.Publish(
                new StylesheetSavedNotification(stylesheet, eventMessages).WithStateFrom(savingNotification));
            Audit(AuditType.Save, userId.Value, -1, "Stylesheet");

            scope.Complete();
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IStylesheetService for stylesheet operations - will be removed in Umbraco 15")]
    public void DeleteStylesheet(string path, int? userId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IStylesheet? stylesheet = _stylesheetRepository.Get(path);
            if (stylesheet == null)
            {
                scope.Complete();
                return;
            }

            EventMessages eventMessages = EventMessagesFactory.Get();
            var deletingNotification = new StylesheetDeletingNotification(stylesheet, eventMessages);
            if (scope.Notifications.PublishCancelable(deletingNotification))
            {
                scope.Complete();
                return; // causes rollback
            }

            userId ??= Constants.Security.SuperUserId;
            _stylesheetRepository.Delete(stylesheet);

            scope.Notifications.Publish(
                new StylesheetDeletedNotification(stylesheet, eventMessages).WithStateFrom(deletingNotification));
            Audit(AuditType.Delete, userId.Value, -1, "Stylesheet");

            scope.Complete();
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IStylesheetService for stylesheet operations - will be removed in Umbraco 15")]
    public void CreateStyleSheetFolder(string folderPath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _stylesheetRepository.AddFolder(folderPath);
            scope.Complete();
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IStylesheetFolderService for stylesheet folder operations - will be removed in Umbraco 15")]
    public void DeleteStyleSheetFolder(string folderPath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _stylesheetRepository.DeleteFolder(folderPath);
            scope.Complete();
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IStylesheetService for stylesheet operations - will be removed in Umbraco 15")]
    public Stream GetStylesheetFileContentStream(string filepath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _stylesheetRepository.GetFileContentStream(filepath);
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IStylesheetService for stylesheet operations - will be removed in Umbraco 15")]
    public void SetStylesheetFileContent(string filepath, Stream content)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _stylesheetRepository.SetFileContent(filepath, content);
            scope.Complete();
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IStylesheetService for stylesheet operations - will be removed in Umbraco 15")]
    public long GetStylesheetFileSize(string filepath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _stylesheetRepository.GetFileSize(filepath);
        }
    }

    #endregion

    #region Scripts

    /// <inheritdoc />
    [Obsolete("Please use IScriptService for script operations - will be removed in Umbraco 15")]
    public IEnumerable<IScript> GetScripts(params string[] names)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _scriptRepository.GetMany(names);
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IScriptService for script operations - will be removed in Umbraco 15")]
    public IScript? GetScript(string? name)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _scriptRepository.Get(name);
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IScriptService for script operations - will be removed in Umbraco 15")]
    public void SaveScript(IScript? script, int? userId)
    {
        if (userId is null)
        {
            userId = Constants.Security.SuperUserId;
        }

        if (script is null)
        {
            return;
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            var savingNotification = new ScriptSavingNotification(script, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return;
            }

            _scriptRepository.Save(script);
            scope.Notifications.Publish(
                new ScriptSavedNotification(script, eventMessages).WithStateFrom(savingNotification));

            Audit(AuditType.Save, userId.Value, -1, "Script");
            scope.Complete();
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IScriptService for script operations - will be removed in Umbraco 15")]
    public void DeleteScript(string path, int? userId = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IScript? script = _scriptRepository.Get(path);
            if (script == null)
            {
                scope.Complete();
                return;
            }

            EventMessages eventMessages = EventMessagesFactory.Get();
            var deletingNotification = new ScriptDeletingNotification(script, eventMessages);
            if (scope.Notifications.PublishCancelable(deletingNotification))
            {
                scope.Complete();
                return;
            }

            userId ??= Constants.Security.SuperUserId;
            _scriptRepository.Delete(script);
            scope.Notifications.Publish(
                new ScriptDeletedNotification(script, eventMessages).WithStateFrom(deletingNotification));

            Audit(AuditType.Delete, userId.Value, -1, "Script");
            scope.Complete();
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IScriptFolderService for script folder operations - will be removed in Umbraco 15")]
    public void CreateScriptFolder(string folderPath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _scriptRepository.AddFolder(folderPath);
            scope.Complete();
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IScriptFolderService for script folder operations - will be removed in Umbraco 15")]
    public void DeleteScriptFolder(string folderPath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _scriptRepository.DeleteFolder(folderPath);
            scope.Complete();
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IScriptService for script operations - will be removed in Umbraco 15")]
    public Stream GetScriptFileContentStream(string filepath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _scriptRepository.GetFileContentStream(filepath);
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IScriptService for script operations - will be removed in Umbraco 15")]
    public void SetScriptFileContent(string filepath, Stream content)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _scriptRepository.SetFileContent(filepath, content);
            scope.Complete();
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IScriptService for script operations - will be removed in Umbraco 15")]
    public long GetScriptFileSize(string filepath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _scriptRepository.GetFileSize(filepath);
        }
    }

    #endregion

    #region Templates

    /// <summary>
    ///     Creates a template for a content type
    /// </summary>
    /// <param name="contentTypeAlias"></param>
    /// <param name="contentTypeName"></param>
    /// <param name="userId"></param>
    /// <returns>
    ///     The template created
    /// </returns>
    [Obsolete("Please use ITemplateService for template operations - will be removed in Umbraco 15")]
    public Attempt<OperationResult<OperationResultType, ITemplate>?> CreateTemplateForContentType(
        string contentTypeAlias, string? contentTypeName, int userId = Constants.Security.SuperUserId)
    {
        // mimic old service behavior
        if (contentTypeAlias.Length > 255)
        {
            throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
        }

        Guid currentUserKey = _userIdKeyResolver.GetAsync(userId).GetAwaiter().GetResult();
        Attempt<ITemplate, TemplateOperationStatus> result = _templateService.CreateForContentTypeAsync(contentTypeAlias, contentTypeName, currentUserKey).GetAwaiter().GetResult();

        // mimic old service behavior
        EventMessages eventMessages = EventMessagesFactory.Get();
        return result.Success
            ? OperationResult.Attempt.Succeed(OperationResultType.Success, eventMessages, result.Result)
            : OperationResult.Attempt.Succeed(OperationResultType.Failed, eventMessages, result.Result);
    }

    /// <summary>
    ///     Create a new template, setting the content if a view exists in the filesystem
    /// </summary>
    /// <param name="name"></param>
    /// <param name="alias"></param>
    /// <param name="content"></param>
    /// <param name="masterTemplate"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    [Obsolete("Please use ITemplateService for template operations - will be removed in Umbraco 15")]
    public ITemplate CreateTemplateWithIdentity(
        string? name,
        string? alias,
        string? content,
        ITemplate? masterTemplate = null,
        int userId = Constants.Security.SuperUserId)
    {
        // mimic old service behavior
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(alias);
        if (name.Length > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "Name cannot be more than 255 characters in length.");
        }

        Guid currentUserKey = _userIdKeyResolver.GetAsync(userId).GetAwaiter().GetResult();
        Attempt<ITemplate, TemplateOperationStatus> result = _templateService.CreateAsync(name, alias, content, currentUserKey).GetAwaiter().GetResult();
        return result.Result;
    }

    /// <summary>
    ///     Gets a list of all <see cref="ITemplate" /> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="ITemplate" /> objects</returns>
    [Obsolete("Please use ITemplateService for template operations - will be removed in Umbraco 15")]
    public IEnumerable<ITemplate> GetTemplates(params string[] aliases)
        => _templateService.GetAllAsync(aliases).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets a list of all <see cref="ITemplate" /> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="ITemplate" /> objects</returns>
    [Obsolete("Please use ITemplateService for template operations - will be removed in Umbraco 15")]
    public IEnumerable<ITemplate> GetTemplates(int masterTemplateId)
        => _templateService.GetChildrenAsync(masterTemplateId).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its alias.
    /// </summary>
    /// <param name="alias">The alias of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the alias, or null.</returns>
    [Obsolete("Please use ITemplateService for template operations - will be removed in Umbraco 15")]
    public ITemplate? GetTemplate(string? alias)
        => _templateService.GetAsync(alias).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the identifier, or null.</returns>
    [Obsolete("Please use ITemplateService for template operations - will be removed in Umbraco 15")]
    public ITemplate? GetTemplate(int id)
        => _templateService.GetAsync(id).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its guid identifier.
    /// </summary>
    /// <param name="id">The guid identifier of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the identifier, or null.</returns>
    [Obsolete("Please use ITemplateService for template operations - will be removed in Umbraco 15")]
    public ITemplate? GetTemplate(Guid id)
        => _templateService.GetAsync(id).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets the template descendants
    /// </summary>
    /// <param name="masterTemplateId"></param>
    /// <returns></returns>
    [Obsolete("Please use ITemplateService for template operations - will be removed in Umbraco 15")]
    public IEnumerable<ITemplate> GetTemplateDescendants(int masterTemplateId)
        => _templateService.GetDescendantsAsync(masterTemplateId).GetAwaiter().GetResult();

    /// <summary>
    ///     Saves a <see cref="Template" />
    /// </summary>
    /// <param name="template"><see cref="Template" /> to save</param>
    /// <param name="userId"></param>
    [Obsolete("Please use ITemplateService for template operations - will be removed in Umbraco 15")]
    public void SaveTemplate(ITemplate template, int userId = Constants.Security.SuperUserId)
    {
        // mimic old service behavior
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        if (string.IsNullOrWhiteSpace(template.Name) || template.Name.Length > 255)
        {
            throw new InvalidOperationException(
                "Name cannot be null, empty, contain only white-space characters or be more than 255 characters in length.");
        }

        Guid currentUserKey = _userIdKeyResolver.GetAsync(userId).GetAwaiter().GetResult();
        if (template.Id > 0)
        {
            _templateService.UpdateAsync(template, currentUserKey).GetAwaiter().GetResult();
        }
        else
        {
            _templateService.CreateAsync(template, currentUserKey).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    ///     Saves a collection of <see cref="Template" /> objects
    /// </summary>
    /// <param name="templates">List of <see cref="Template" /> to save</param>
    /// <param name="userId">Optional id of the user</param>
    [Obsolete("Please use ITemplateService for template operations - will be removed in Umbraco 15")]
    public void SaveTemplate(IEnumerable<ITemplate> templates, int userId = Constants.Security.SuperUserId)
    {
        ITemplate[] templatesA = templates.ToArray();
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            var savingNotification = new TemplateSavingNotification(templatesA, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return;
            }

            foreach (ITemplate template in templatesA)
            {
                _templateRepository.Save(template);
            }

            scope.Notifications.Publish(
                new TemplateSavedNotification(templatesA, eventMessages).WithStateFrom(savingNotification));

            Audit(AuditType.Save, userId, -1, UmbracoObjectTypes.Template.GetName());
            scope.Complete();
        }
    }

    /// <summary>
    ///     Deletes a template by its alias
    /// </summary>
    /// <param name="alias">Alias of the <see cref="ITemplate" /> to delete</param>
    /// <param name="userId"></param>
    [Obsolete("Please use ITemplateService for template operations - will be removed in Umbraco 15")]
    public void DeleteTemplate(string alias, int userId = Constants.Security.SuperUserId)
    {
        Guid currentUserKey = _userIdKeyResolver.GetAsync(userId).GetAwaiter().GetResult();
        _templateService.DeleteAsync(alias, currentUserKey).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    [Obsolete("Please use ITemplateService for template operations - will be removed in Umbraco 15")]
    public Stream GetTemplateFileContentStream(string filepath)
        => _templateService.GetFileContentStreamAsync(filepath).GetAwaiter().GetResult();

    /// <inheritdoc />
    [Obsolete("Please use ITemplateService for template operations - will be removed in Umbraco 15")]
    public void SetTemplateFileContent(string filepath, Stream content)
        => _templateService.SetFileContentAsync(filepath, content).GetAwaiter().GetResult();

    /// <inheritdoc />
    [Obsolete("Please use ITemplateService for template operations - will be removed in Umbraco 15")]
    public long GetTemplateFileSize(string filepath)
        => _templateService.GetFileSizeAsync(filepath).GetAwaiter().GetResult();

    #endregion

    #region Partial Views

    [Obsolete("Please use IPartialViewFolderService for partial view folder operations - will be removed in Umbraco 15")]
    public void DeletePartialViewFolder(string folderPath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _partialViewRepository.DeleteFolder(folderPath);
            scope.Complete();
        }
    }

    [Obsolete("Please use IPartialViewService for partial view operations - will be removed in Umbraco 15")]
    public IEnumerable<IPartialView> GetPartialViews(params string[] names)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _partialViewRepository.GetMany(names);
        }
    }

    [Obsolete("Please use IPartialViewService for partial view operations - will be removed in Umbraco 15")]
    public IPartialView? GetPartialView(string path)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _partialViewRepository.Get(path);
        }
    }

    [Obsolete("Please use IPartialViewService for partial view operations - will be removed in Umbraco 15")]
    public Attempt<IPartialView?> CreatePartialView(IPartialView partialView, string? snippetName = null, int? userId = Constants.Security.SuperUserId)
    {
        string? partialViewContent = null;
        if (snippetName.IsNullOrWhiteSpace() == false)
        {
            // create the file
            Attempt<string> snippetPathAttempt = TryGetSnippetPath(snippetName);
            if (snippetPathAttempt.Success == false)
            {
                throw new InvalidOperationException("Could not load snippet with name " + snippetName);
            }

            using (var snippetFile = new StreamReader(File.OpenRead(snippetPathAttempt.Result!)))
            {
                var snippetContent = snippetFile.ReadToEnd().Trim();

                // strip the @inherits if it's there
                snippetContent = StripPartialViewHeader(snippetContent);

                // Update Model.Content. to be Model. when used as PartialView
                snippetContent = snippetContent.Replace("Model.Content.", "Model.");

                partialViewContent = $"{PartialViewHeader}{Environment.NewLine}{snippetContent}";
            }
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            var creatingNotification = new PartialViewCreatingNotification(partialView, eventMessages);
            if (scope.Notifications.PublishCancelable(creatingNotification))
            {
                scope.Complete();
                return Attempt<IPartialView?>.Fail();
            }

            if (partialViewContent != null)
            {
                partialView.Content = partialViewContent;
            }

            _partialViewRepository.Save(partialView);

            scope.Notifications.Publish(
                new PartialViewCreatedNotification(partialView, eventMessages).WithStateFrom(creatingNotification));

            Audit(AuditType.Save, userId!.Value, -1, Constants.UdiEntityType.PartialView);

            scope.Complete();
        }

        return Attempt<IPartialView?>.Succeed(partialView);
    }

    [Obsolete("Please use IPartialViewService for partial view operations - will be removed in Umbraco 15")]
    public Attempt<IPartialView?> SavePartialView(IPartialView partialView, int? userId = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            var savingNotification = new PartialViewSavingNotification(partialView, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return Attempt<IPartialView?>.Fail();
            }

            userId ??= Constants.Security.SuperUserId;
            _partialViewRepository.Save(partialView);

            Audit(AuditType.Save, userId.Value, -1, Constants.UdiEntityType.PartialView);
            scope.Notifications.Publish(
                new PartialViewSavedNotification(partialView, eventMessages).WithStateFrom(savingNotification));

            scope.Complete();
        }

        return Attempt.Succeed(partialView);
    }

    [Obsolete("Please use IPartialViewService for partial view operations - will be removed in Umbraco 15")]
    public bool DeletePartialView(string path, int? userId = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IPartialView? partialView = _partialViewRepository.Get(path);
            if (partialView == null)
            {
                scope.Complete();
                return true;
            }

            EventMessages eventMessages = EventMessagesFactory.Get();
            var deletingNotification = new PartialViewDeletingNotification(partialView, eventMessages);
            if (scope.Notifications.PublishCancelable(deletingNotification))
            {
                scope.Complete();
                return false;
            }

            userId ??= Constants.Security.SuperUserId;
            _partialViewRepository.Delete(partialView);
            scope.Notifications.Publish(
                new PartialViewDeletedNotification(partialView, eventMessages).WithStateFrom(deletingNotification));
            Audit(AuditType.Delete, userId.Value, -1, Constants.UdiEntityType.PartialView);

            scope.Complete();
        }

        return true;
    }

    [Obsolete("Please use IPartialViewFolderService for partial view folder operations - will be removed in Umbraco 15")]
    public void CreatePartialViewFolder(string folderPath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _partialViewRepository.AddFolder(folderPath);
            scope.Complete();
        }
    }

    internal string StripPartialViewHeader(string contents)
    {
        var headerMatch = new Regex("^@inherits\\s+?.*$", RegexOptions.Multiline);
        return headerMatch.Replace(contents, string.Empty);
    }

    internal Attempt<string> TryGetSnippetPath(string? fileName)
    {
        if (fileName?.EndsWith(".cshtml") == false)
        {
            fileName += ".cshtml";
        }

        var snippetPath =
            _hostingEnvironment.MapPathContentRoot(
                $"{Constants.SystemDirectories.Umbraco}/PartialViewMacros/Templates/{fileName}");
        return File.Exists(snippetPath)
            ? Attempt<string>.Succeed(snippetPath)
            : Attempt<string>.Fail();
    }

    /// <inheritdoc />
    [Obsolete("Please use IPartialViewService for partial view operations - will be removed in Umbraco 15")]
    public Stream GetPartialViewFileContentStream(string filepath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _partialViewRepository.GetFileContentStream(filepath);
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IPartialViewService for partial view operations - will be removed in Umbraco 15")]
    public void SetPartialViewFileContent(string filepath, Stream content)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _partialViewRepository.SetFileContent(filepath, content);
            scope.Complete();
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IPartialViewService for partial view operations - will be removed in Umbraco 15")]
    public long GetPartialViewFileSize(string filepath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _partialViewRepository.GetFileSize(filepath);
        }
    }

    #endregion

    // TODO: Method to change name and/or alias of view template
}
