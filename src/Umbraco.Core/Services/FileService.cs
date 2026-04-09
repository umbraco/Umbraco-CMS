using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
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
///     Represents the File Service, which provides access to operations involving <see cref="IFile" /> objects like
///     Scripts, Stylesheets, Templates, and Partial Views.
/// </summary>
/// <remarks>
///     Many methods in this service are marked as obsolete. For new implementations, use the specific
///     services: <see cref="IStylesheetService" />, <see cref="IScriptService" />, <see cref="ITemplateService" />,
///     <see cref="IPartialViewService" />, and their corresponding folder services.
/// </remarks>
public class FileService : RepositoryService, IFileService
{
    private const string PartialViewHeader = "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage";
    private readonly IAuditService _auditService;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IPartialViewRepository _partialViewRepository;
    private readonly IScriptRepository _scriptRepository;
    private readonly IStylesheetRepository _stylesheetRepository;
    private readonly ITemplateService _templateService;
    private readonly ITemplateRepository _templateRepository;
    private readonly IUserIdKeyResolver _userIdKeyResolver;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileService" /> class.
    /// </summary>
    /// <param name="uowProvider">The core scope provider for database operations.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="stylesheetRepository">The repository for stylesheet operations.</param>
    /// <param name="scriptRepository">The repository for script operations.</param>
    /// <param name="partialViewRepository">The repository for partial view operations.</param>
    /// <param name="auditService">The audit service for logging operations.</param>
    /// <param name="hostingEnvironment">The hosting environment for path resolution.</param>
    /// <param name="templateService">The template service for template operations.</param>
    /// <param name="templateRepository">The template repository for direct template access.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user IDs to keys.</param>
    public FileService(
        ICoreScopeProvider uowProvider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IStylesheetRepository stylesheetRepository,
        IScriptRepository scriptRepository,
        IPartialViewRepository partialViewRepository,
        IAuditService auditService,
        IHostingEnvironment hostingEnvironment,
        ITemplateService templateService,
        ITemplateRepository templateRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : base(uowProvider, loggerFactory, eventMessagesFactory)
    {
        _stylesheetRepository = stylesheetRepository;
        _scriptRepository = scriptRepository;
        _partialViewRepository = partialViewRepository;
        _auditService = auditService;
        _hostingEnvironment = hostingEnvironment;
        _templateService = templateService;
        _templateRepository = templateRepository;
        _userIdKeyResolver = userIdKeyResolver;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileService" /> class.
    /// </summary>
    /// <param name="uowProvider">The core scope provider for database operations.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="stylesheetRepository">The repository for stylesheet operations.</param>
    /// <param name="scriptRepository">The repository for script operations.</param>
    /// <param name="partialViewRepository">The repository for partial view operations.</param>
    /// <param name="auditRepository">The audit repository (no longer used).</param>
    /// <param name="hostingEnvironment">The hosting environment for path resolution.</param>
    /// <param name="templateService">The template service for template operations.</param>
    /// <param name="templateRepository">The template repository for direct template access.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user IDs to keys.</param>
    /// <param name="shortStringHelper">The short string helper (no longer used).</param>
    /// <param name="globalSettings">The global settings options (no longer used).</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 19.")]
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
        IShortStringHelper shortStringHelper,
        IOptions<GlobalSettings> globalSettings)
        : this(
            uowProvider,
            loggerFactory,
            eventMessagesFactory,
            stylesheetRepository,
            scriptRepository,
            partialViewRepository,
            StaticServiceProvider.Instance.GetRequiredService<IAuditService>(),
            hostingEnvironment,
            templateService,
            templateRepository,
            userIdKeyResolver)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileService" /> class.
    /// </summary>
    /// <param name="uowProvider">The core scope provider for database operations.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="stylesheetRepository">The repository for stylesheet operations.</param>
    /// <param name="scriptRepository">The repository for script operations.</param>
    /// <param name="partialViewRepository">The repository for partial view operations.</param>
    /// <param name="auditService">The audit service for logging operations.</param>
    /// <param name="auditRepository">The audit repository (no longer used).</param>
    /// <param name="hostingEnvironment">The hosting environment for path resolution.</param>
    /// <param name="templateService">The template service for template operations.</param>
    /// <param name="templateRepository">The template repository for direct template access.</param>
    /// <param name="userIdKeyResolver">The resolver for converting user IDs to keys.</param>
    /// <param name="shortStringHelper">The short string helper (no longer used).</param>
    /// <param name="globalSettings">The global settings options (no longer used).</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 19.")]
    public FileService(
        ICoreScopeProvider uowProvider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IStylesheetRepository stylesheetRepository,
        IScriptRepository scriptRepository,
        IPartialViewRepository partialViewRepository,
        IAuditService auditService,
        IAuditRepository auditRepository,
        IHostingEnvironment hostingEnvironment,
        ITemplateService templateService,
        ITemplateRepository templateRepository,
        IUserIdKeyResolver userIdKeyResolver,
        IShortStringHelper shortStringHelper,
        IOptions<GlobalSettings> globalSettings)
        : this(
            uowProvider,
            loggerFactory,
            eventMessagesFactory,
            stylesheetRepository,
            scriptRepository,
            partialViewRepository,
            auditService,
            hostingEnvironment,
            templateService,
            templateRepository,
            userIdKeyResolver)
    {
    }

    #region Stylesheets
    /// <summary>
    ///     Records an audit entry synchronously.
    /// </summary>
    /// <param name="type">The type of audit action.</param>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <param name="objectId">The ID of the object being acted upon.</param>
    /// <param name="entityType">The type of entity being audited.</param>
    private void Audit(AuditType type, int userId, int objectId, string? entityType) =>
        AuditAsync(type, userId, objectId, entityType).GetAwaiter().GetResult();

    /// <summary>
    ///     Records an audit entry asynchronously.
    /// </summary>
    /// <param name="type">The type of audit action.</param>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <param name="objectId">The ID of the object being acted upon.</param>
    /// <param name="entityType">The type of entity being audited.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task AuditAsync(AuditType type, int userId, int objectId, string? entityType)
    {
        Guid userKey = await _userIdKeyResolver.GetAsync(userId);

        await _auditService.AddAsync(
            type,
            userKey,
            objectId,
            entityType);
    }

    /// <inheritdoc />
    [Obsolete("Please use IStylesheetService for stylesheet operations. Scheduled for removal in Umbraco 18.")]
    public IStylesheet? GetStylesheet(string? path)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _stylesheetRepository.Get(path);
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IStylesheetService for stylesheet operations. Scheduled for removal in Umbraco 18.")]
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
    #endregion

    #region Scripts

    /// <inheritdoc />
    [Obsolete("Please use IScriptService for script operations. Scheduled for removal in Umbraco 18.")]
    public IScript? GetScript(string? name)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _scriptRepository.Get(name);
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IScriptService for script operations. Scheduled for removal in Umbraco 18.")]
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
    #endregion

    #region Templates
    /// <summary>
    ///     Create a new template, setting the content if a view exists in the filesystem
    /// </summary>
    /// <param name="name"></param>
    /// <param name="alias"></param>
    /// <param name="content"></param>
    /// <param name="masterTemplate"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    [Obsolete("Please use ITemplateService for template operations. Scheduled for removal in Umbraco 18.")]
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
    [Obsolete("Please use ITemplateService for template operations. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<ITemplate> GetTemplates(params string[] aliases)
        => _templateService.GetAllAsync(aliases).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets a list of all <see cref="ITemplate" /> objects
    /// </summary>
    /// <returns>An enumerable list of <see cref="ITemplate" /> objects</returns>
    [Obsolete("Please use ITemplateService for template operations. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<ITemplate> GetTemplates(int masterTemplateId)
        => _templateService.GetChildrenAsync(masterTemplateId).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its alias.
    /// </summary>
    /// <param name="alias">The alias of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the alias, or null.</returns>
    [Obsolete("Please use ITemplateService for template operations. Scheduled for removal in Umbraco 18.")]
    public ITemplate? GetTemplate(string? alias)
        => _templateService.GetAsync(alias).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the identifier, or null.</returns>
    [Obsolete("Please use ITemplateService for template operations. Scheduled for removal in Umbraco 18.")]
    public ITemplate? GetTemplate(int id)
        => _templateService.GetAsync(id).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets a <see cref="ITemplate" /> object by its guid identifier.
    /// </summary>
    /// <param name="id">The guid identifier of the template.</param>
    /// <returns>The <see cref="ITemplate" /> object matching the identifier, or null.</returns>
    [Obsolete("Please use ITemplateService for template operations. Scheduled for removal in Umbraco 18.")]
    public ITemplate? GetTemplate(Guid id)
        => _templateService.GetAsync(id).GetAwaiter().GetResult();

    /// <summary>
    ///     Gets the template descendants
    /// </summary>
    /// <param name="masterTemplateId"></param>
    /// <returns></returns>
    [Obsolete("Please use ITemplateService for template operations. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<ITemplate> GetTemplateDescendants(int masterTemplateId)
        => _templateService.GetDescendantsAsync(masterTemplateId).GetAwaiter().GetResult();

    /// <summary>
    ///     Saves a <see cref="Template" />
    /// </summary>
    /// <param name="template"><see cref="Template" /> to save</param>
    /// <param name="userId"></param>
    [Obsolete("Please use ITemplateService for template operations. Scheduled for removal in Umbraco 18.")]
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
    ///     Deletes a template by its alias
    /// </summary>
    /// <param name="alias">Alias of the <see cref="ITemplate" /> to delete</param>
    /// <param name="userId"></param>
    [Obsolete("Please use ITemplateService for template operations. Scheduled for removal in Umbraco 18.")]
    public void DeleteTemplate(string alias, int userId = Constants.Security.SuperUserId)
    {
        Guid currentUserKey = _userIdKeyResolver.GetAsync(userId).GetAwaiter().GetResult();
        _templateService.DeleteAsync(alias, currentUserKey).GetAwaiter().GetResult();
    }
    #endregion

    #region Partial Views

    /// <inheritdoc />
    [Obsolete("Please use IPartialViewService for partial view operations. Scheduled for removal in Umbraco 18.")]
    public IPartialView? GetPartialView(string path)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _partialViewRepository.Get(path);
        }
    }

    /// <inheritdoc />
    [Obsolete("Please use IPartialViewService for partial view operations. Scheduled for removal in Umbraco 18.")]
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
    #endregion

    // TODO: Method to change name and/or alias of view template
}
