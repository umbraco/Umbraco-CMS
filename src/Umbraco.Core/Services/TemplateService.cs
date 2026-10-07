using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using File = Umbraco.Cms.Core.Models.File;
using IScopeProvider = Umbraco.Cms.Core.Scoping.EFCore.IScopeProvider;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides functionality for managing templates (Razor views) including CRUD operations and layout template relationships.
/// </summary>
/// <remarks>
///     Template data is persisted through <see cref="ITemplateRepository" />; the template view files are read and
///     written by this service. View files are not written in <see cref="RuntimeMode.Production" />.
/// </remarks>
public class TemplateService : AsyncRepositoryService, ITemplateService
{
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ITemplateRepository _templateRepository;
    private readonly IAuditService _auditService;
    private readonly ITemplateContentParserService _templateContentParserService;
    private readonly IViewHelper _viewHelper;
    private readonly FileSystems _fileSystems;
    private readonly IOptionsMonitor<RuntimeSettings> _runtimeSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateService" /> class.
    /// </summary>
    /// <param name="provider">The scope provider for unit of work operations.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    /// <param name="shortStringHelper">The helper for short string operations.</param>
    /// <param name="templateRepository">The repository for template data access.</param>
    /// <param name="auditService">The audit service for recording audit entries.</param>
    /// <param name="templateContentParserService">The service for parsing template content.</param>
    /// <param name="viewHelper">The helper for creating and updating template view files.</param>
    /// <param name="fileSystems">The file systems, providing access to the template view files.</param>
    /// <param name="runtimeSettings">The runtime settings, determining whether view files are written.</param>
    public TemplateService(
        IScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IShortStringHelper shortStringHelper,
        ITemplateRepository templateRepository,
        IAuditService auditService,
        ITemplateContentParserService templateContentParserService,
        IViewHelper viewHelper,
        FileSystems fileSystems,
        IOptionsMonitor<RuntimeSettings> runtimeSettings)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _shortStringHelper = shortStringHelper;
        _templateRepository = templateRepository;
        _auditService = auditService;
        _templateContentParserService = templateContentParserService;
        _viewHelper = viewHelper;
        _fileSystems = fileSystems;
        _runtimeSettings = runtimeSettings;
    }

    private IFileSystem? ViewsFileSystem => _fileSystems.MvcViewsFileSystem;

    private bool CanWriteViewFiles => _runtimeSettings.CurrentValue.Mode != RuntimeMode.Production;

    /// <inheritdoc />
    public async Task<ITemplate?> GetAsync(Guid key, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        ITemplate? template = await _templateRepository.GetAsync(key, cancellationToken);
        scope.Complete();
        return WithContentLoader(template);
    }

    /// <inheritdoc />
    public async Task<ITemplate?> GetAsync(string alias, CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        ITemplate? template = await _templateRepository.GetByAliasAsync(alias, cancellationToken);
        scope.Complete();
        return WithContentLoader(template);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ITemplate>> GetAllAsync(CancellationToken cancellationToken)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IEnumerable<ITemplate> templates = await _templateRepository.GetAllAsync(cancellationToken);
        scope.Complete();
        return WithContentLoader(templates.OrderBy(t => t.Name));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ITemplate>> GetManyAsync(IEnumerable<Guid> keys, CancellationToken cancellationToken)
    {
        Guid[] keysAsArray = keys.ToArray();
        if (keysAsArray.Length == 0)
        {
            return [];
        }

        using ICoreScope scope = ScopeProvider.CreateScope();
        IEnumerable<ITemplate> templates = await _templateRepository.GetManyAsync(keysAsArray, cancellationToken);
        scope.Complete();
        return WithContentLoader(templates);
    }

    /// <inheritdoc />
    public async Task<Attempt<ITemplate, TemplateOperationStatus>> CreateAsync(
        string name,
        string alias,
        string? content,
        Guid? templateKey,
        Guid userKey,
        CancellationToken cancellationToken)
        => await CreateAsync(
            new Template(_shortStringHelper, name, alias) { Content = content, Key = templateKey ?? Guid.NewGuid() },
            userKey,
            cancellationToken);

    /// <inheritdoc />
    public async Task<Attempt<ITemplate, TemplateOperationStatus>> CreateAsync(ITemplate template, Guid userKey, CancellationToken cancellationToken)
        => await CreateAsync(template, userKey, null, cancellationToken);

    /// <inheritdoc />
    public async Task<Attempt<ITemplate?, TemplateOperationStatus>> CreateForContentTypeAsync(
        string name,
        string alias,
        string contentTypeAlias,
        Guid userKey,
        CancellationToken cancellationToken)
    {
        ITemplate template =
            new Template(_shortStringHelper, name, alias) { Key = Guid.CreateVersion7() };

        Attempt<ITemplate, TemplateOperationStatus> result = await CreateAsync(template, userKey, contentTypeAlias, cancellationToken);
        return result.Success
            ? Attempt.SucceedWithStatus<ITemplate?, TemplateOperationStatus>(result.Status, result.Result)
            : Attempt<ITemplate?, TemplateOperationStatus>.Fail(result.Status);
    }

    /// <inheritdoc />
    public async Task<Attempt<ITemplate, TemplateOperationStatus>> UpdateAsync(ITemplate template, Guid userKey, CancellationToken cancellationToken)
        => await SaveAsync(
            template,
            AuditType.Save,
            userKey,
            () => ValidateUpdateAsync(template, cancellationToken),
            null,
            cancellationToken);

    /// <inheritdoc />
    public async Task<Attempt<ITemplate?, TemplateOperationStatus>> DeleteAsync(Guid key, Guid userKey, CancellationToken cancellationToken)
    {
        using (ICoreScope scope = ScopeProvider.CreateScope())
        {
            ITemplate? template = await _templateRepository.GetAsync(key, cancellationToken);
            if (template == null)
            {
                scope.Complete();
                return Attempt.FailWithStatus<ITemplate?, TemplateOperationStatus>(TemplateOperationStatus.TemplateNotFound, null);
            }

            if (template.IsLayoutTemplate)
            {
                scope.Complete();
                return Attempt.FailWithStatus<ITemplate?, TemplateOperationStatus>(TemplateOperationStatus.LayoutTemplateCannotBeDeleted, null);
            }

            EventMessages eventMessages = EventMessagesFactory.Get();
            var deletingNotification = new TemplateDeletingNotification(template, eventMessages);
            if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
            {
                scope.Complete();
                return Attempt.FailWithStatus<ITemplate?, TemplateOperationStatus>(TemplateOperationStatus.CancelledByNotification, template);
            }

            await _templateRepository.DeleteAsync(template, cancellationToken);

            if (CanWriteViewFiles)
            {
                ViewsFileSystem?.DeleteFile(ViewFileName(template.Alias));
            }

            scope.Notifications.Publish(
                new TemplateDeletedNotification(template, eventMessages).WithStateFrom(deletingNotification));

            await Audit(AuditType.Delete, userKey, template.Id, UmbracoObjectTypes.Template.GetName());
            scope.Complete();
            return Attempt.SucceedWithStatus<ITemplate?, TemplateOperationStatus>(TemplateOperationStatus.Success, template);
        }
    }

    /// <summary>
    ///     Creates a template with optional content type association.
    /// </summary>
    private async Task<Attempt<ITemplate, TemplateOperationStatus>> CreateAsync(
        ITemplate template,
        Guid userKey,
        string? contentTypeAlias,
        CancellationToken cancellationToken)
    {
        if (IsValidAlias(template.Alias) is false)
        {
            return Attempt.FailWithStatus(TemplateOperationStatus.InvalidAlias, template);
        }

        try
        {
            // File might already be on disk, if so grab the content to avoid overwriting.
            template.Content = GetViewContent(template.Alias) ?? template.Content;
            return await SaveAsync(
                template,
                AuditType.New,
                userKey,
                () => ValidateCreateAsync(template, cancellationToken),
                contentTypeAlias,
                cancellationToken);
        }
        catch (PathTooLongException ex)
        {
            LoggerFactory.CreateLogger<TemplateService>().LogError(ex, "The template path was too long. Consider making the template alias shorter.");
            return Attempt.FailWithStatus(TemplateOperationStatus.InvalidAlias, template);
        }
    }

    private async Task<TemplateOperationStatus> ValidateCreateAsync(ITemplate templateToCreate, CancellationToken cancellationToken)
    {
        ITemplate? existingTemplate = await _templateRepository.GetByAliasAsync(templateToCreate.Alias, cancellationToken);
        return existingTemplate is not null
            ? TemplateOperationStatus.DuplicateAlias
            : TemplateOperationStatus.Success;
    }

    private async Task<TemplateOperationStatus> ValidateUpdateAsync(ITemplate templateToUpdate, CancellationToken cancellationToken)
    {
        ITemplate? existingTemplate = await _templateRepository.GetByAliasAsync(templateToUpdate.Alias, cancellationToken);
        if (existingTemplate is not null && existingTemplate.Key != templateToUpdate.Key)
        {
            return TemplateOperationStatus.DuplicateAlias;
        }

        if (await _templateRepository.ExistsAsync(templateToUpdate.Key, cancellationToken) is false)
        {
            return TemplateOperationStatus.TemplateNotFound;
        }

        return TemplateOperationStatus.Success;
    }

    /// <summary>
    ///     Saves a template with validation and auditing.
    /// </summary>
    private async Task<Attempt<ITemplate, TemplateOperationStatus>> SaveAsync(
        ITemplate template,
        AuditType auditType,
        Guid userKey,
        Func<Task<TemplateOperationStatus>> scopeValidatorAsync,
        string? contentTypeAlias,
        CancellationToken cancellationToken)
    {
        if (IsValidAlias(template.Alias) == false)
        {
            return Attempt.FailWithStatus(TemplateOperationStatus.InvalidAlias, template);
        }

        using (ICoreScope scope = ScopeProvider.CreateScope())
        {
            TemplateOperationStatus scopeValidatorStatus = await scopeValidatorAsync();
            if (scopeValidatorStatus != TemplateOperationStatus.Success)
            {
                return Attempt.FailWithStatus(scopeValidatorStatus, template);
            }

            // The persisted template holds the alias the view file is currently saved under, which differs from the
            // template's own alias when it is being renamed.
            ITemplate? persistedTemplate = template.HasIdentity
                ? await _templateRepository.GetAsync(template.Key, cancellationToken)
                : null;

            // A template without loaded content (e.g. one reached through a content type) keeps its existing view.
            if (template.HasIdentity && template.Content is null)
            {
                SetContentLoader(template, persistedTemplate?.Alias ?? template.Alias);
            }

            var layoutTemplateAlias = _templateContentParserService.LayoutTemplateAlias(template.Content);
            ITemplate? layoutTemplate = layoutTemplateAlias.IsNullOrWhiteSpace()
                ? null
                : await _templateRepository.GetByAliasAsync(layoutTemplateAlias, cancellationToken);

            // Fail if the template content specifies a layout template but said template does not exist
            if (layoutTemplateAlias.IsNullOrWhiteSpace() == false && layoutTemplate == null)
            {
                return Attempt.FailWithStatus(TemplateOperationStatus.LayoutTemplateNotFound, template);
            }

            // Detect circular references.
            if (layoutTemplateAlias is not null
                && layoutTemplate is not null
                && await HasCircularReferenceAsync(layoutTemplateAlias, template, layoutTemplate, cancellationToken))
            {
                return Attempt.FailWithStatus(TemplateOperationStatus.CircularLayoutTemplateReference, template);
            }

            await SetLayoutTemplateAsync(template, layoutTemplate, userKey, cancellationToken);

            EventMessages eventMessages = EventMessagesFactory.Get();
            var savingNotification = new TemplateSavingNotification(
                template,
                eventMessages,
                !contentTypeAlias.IsNullOrWhiteSpace(),
                contentTypeAlias ?? string.Empty);
            if (await scope.Notifications.PublishCancelableAsync(savingNotification))
            {
                scope.Complete();
                return Attempt.FailWithStatus(TemplateOperationStatus.CancelledByNotification, template);
            }

            // The view file is named after the alias, so a renamed template must move the file from its previous alias.
            var previousAlias = template.HasIdentity && template.IsPropertyDirty(nameof(ITemplate.Alias))
                ? persistedTemplate?.Alias
                : null;

            var isNew = template.HasIdentity is false;
            await _templateRepository.SaveAsync(template, cancellationToken);

            if (CanWriteViewFiles)
            {
                SaveViewFile(template, isNew, previousAlias);
            }

            WithContentLoader(template);

            scope.Notifications.Publish(
                new TemplateSavedNotification(template, eventMessages).WithStateFrom(savingNotification));

            await Audit(auditType, userKey, template.Id, UmbracoObjectTypes.Template.GetName());
            scope.Complete();
            return Attempt.SucceedWithStatus(TemplateOperationStatus.Success, template);
        }
    }

    /// <summary>
    ///     Sets or removes the layout template for the specified template.
    /// </summary>
    private async Task SetLayoutTemplateAsync(ITemplate template, ITemplate? layoutTemplate, Guid userKey, CancellationToken cancellationToken)
    {
        if (template.LayoutTemplateAlias == layoutTemplate?.Alias)
        {
            return;
        }

        var concreteTemplate = (Template)template;

        if (layoutTemplate != null)
        {
            if (layoutTemplate.Id == template.Id)
            {
                concreteTemplate.LayoutTemplateId = new Lazy<int>(() => -1);
                concreteTemplate.LayoutTemplateAlias = null;
            }
            else
            {
                concreteTemplate.LayoutTemplateId = new Lazy<int>(() => layoutTemplate.Id);
                concreteTemplate.LayoutTemplateAlias = layoutTemplate.Alias;

                // After updating the layout - ensure we update the path property if it has any children already assigned.
                if (template.HasIdentity)
                {
                    await UpdateDescendantPathsAsync(template, layoutTemplate, userKey, cancellationToken);
                }
            }
        }
        else
        {
            //remove the layout
            concreteTemplate.LayoutTemplateId = new Lazy<int>(() => -1);
            concreteTemplate.LayoutTemplateAlias = null;
        }
    }

    /// <summary>
    ///     Re-roots the paths of a template's descendants under its new layout template, and saves each descendant.
    /// </summary>
    private async Task UpdateDescendantPathsAsync(ITemplate template, ITemplate layoutTemplate, Guid userKey, CancellationToken cancellationToken)
    {
        IEnumerable<ITemplate> templateHasChildren = await _templateRepository.GetDescendantsAsync(template.Key, cancellationToken);

        foreach (ITemplate childTemplate in templateHasChildren)
        {
            // Template ID to find.
            var templateIdInPath = "," + template.Id + ",";

            if (string.IsNullOrEmpty(childTemplate.Path))
            {
                continue;
            }

            // Find position in current comma separate string path (so we get the correct children path).
            var positionInPath = childTemplate.Path.IndexOf(templateIdInPath) + templateIdInPath.Length;

            // Get the substring of the child & any children (descendants it may have too).
            var childTemplatePath = childTemplate.Path.Substring(positionInPath);

            // As we are updating the template to be a child of a layout set the path to the layout's path and
            // its current template id + the current child path substring.
            childTemplate.Path = layoutTemplate.Path + "," + template.Id + "," + childTemplatePath;

            // Save the children with the updated path.
            await UpdateAsync(WithContentLoader(childTemplate)!, userKey, cancellationToken);
        }
    }

    /// <summary>
    ///     Writes the view file for a saved template, and updates the template content with what was written.
    /// </summary>
    private void SaveViewFile(ITemplate template, bool isNew, string? previousAlias)
    {
        string? content;
        if (template is TemplateOnDisk { IsOnDisk: true })
        {
            content = _viewHelper.GetFileContents(template);
        }
        else
        {
            content = isNew
                ? _viewHelper.CreateView(template, true)
                : _viewHelper.UpdateViewFile(template, previousAlias ?? template.Alias);
        }

        // The content is what is now on disk, so it isn't a change made to the template.
        if (template is Template concreteTemplate)
        {
            concreteTemplate.DisableChangeTracking();
            try
            {
                template.Content = content;
            }
            finally
            {
                concreteTemplate.EnableChangeTracking();
            }
        }
        else
        {
            template.Content = content;
        }
    }

    private ITemplate[] WithContentLoader(IEnumerable<ITemplate> templates)
        => templates.Select(template => WithContentLoader(template)!).ToArray();

    /// <summary>
    ///     Sets a loader on the template that reads its content from the view file when the content is first accessed.
    /// </summary>
    /// <remarks>
    ///     The view file is the one for the alias the template has when the loader is set, so content read after the
    ///     alias is changed (and before it is saved) is still that of the existing view.
    /// </remarks>
    private ITemplate? WithContentLoader(ITemplate? template)
    {
        if (template is not null)
        {
            SetContentLoader(template, template.Alias);
        }

        return template;
    }

    private void SetContentLoader(ITemplate template, string alias)
    {
        if (template is File file)
        {
            file.GetFileContent = _ => ReadViewFile(alias);
        }
    }

    private string ReadViewFile(string alias) => ReadViewFileOrNull(alias) ?? string.Empty;

    /// <summary>
    ///     Gets the content of a view file from disk.
    /// </summary>
    /// <param name="alias">The alias of the template.</param>
    /// <returns>The content of the view file, or null if it doesn't exist or is empty.</returns>
    private string? GetViewContent(string alias) => ReadViewFileOrNull(alias)?.Trim().NullOrWhiteSpaceAsNull();

    /// <summary>
    ///     Reads the view file of the template with the specified alias.
    /// </summary>
    /// <returns>The content of the view file, or null if it doesn't exist.</returns>
    private string? ReadViewFileOrNull(string alias)
    {
        IFileSystem? viewsFileSystem = ViewsFileSystem;
        var fileName = ViewFileName(alias);
        if (viewsFileSystem is null || viewsFileSystem.FileExists(fileName) is false)
        {
            return null;
        }

        try
        {
            using Stream stream = viewsFileSystem.OpenFile(fileName);
            using var reader = new StreamReader(stream, Encoding.UTF8, true);
            return reader.ReadToEnd();
        }
        catch (IOException)
        {
            // The file may have been removed between the existence check and opening it.
            return null;
        }
    }

    private static string ViewFileName(string alias) => string.Concat(alias, ".cshtml");

    private Task Audit(AuditType type, Guid userKey, int objectId, string? entityType) =>
        _auditService.AddAsync(type, userKey, objectId, entityType);

    private static bool IsValidAlias(string alias)
        => alias.IsNullOrWhiteSpace() == false && alias.Length <= 255;

    /// <summary>
    ///     Checks if setting the layout template would create a circular reference.
    /// </summary>
    private async Task<bool> HasCircularReferenceAsync(
        string parsedLayoutTemplateAlias,
        ITemplate template,
        ITemplate layoutTemplate,
        CancellationToken cancellationToken)
    {
        // quick check without extra DB calls as we already have both templates
        if (parsedLayoutTemplateAlias.IsNullOrWhiteSpace() is false
            && layoutTemplate.LayoutTemplateAlias is not null
            && layoutTemplate.LayoutTemplateAlias.Equals(template.Alias))
        {
            return true;
        }

        var processedTemplates = new List<ITemplate> { template, layoutTemplate };
        return await HasRecursiveCircularReferenceAsync(processedTemplates, layoutTemplate.LayoutTemplateAlias, cancellationToken);
    }

    /// <summary>
    ///     Recursively checks for circular references in the layout template chain.
    /// </summary>
    private async Task<bool> HasRecursiveCircularReferenceAsync(
        List<ITemplate> referencedTemplates,
        string? layoutTemplateAlias,
        CancellationToken cancellationToken)
    {
        if (layoutTemplateAlias is null)
        {
            return false;
        }

        if (referencedTemplates.Any(template => template.Alias.Equals(layoutTemplateAlias)))
        {
            return true;
        }

        ITemplate? layoutTemplate = await _templateRepository.GetByAliasAsync(layoutTemplateAlias, cancellationToken);
        if (layoutTemplate is null)
        {
            // This should not happen unless somebody manipulated the data by hand as this function is only called between persisted items.
            return false;
        }

        referencedTemplates.Add(layoutTemplate);

        return await HasRecursiveCircularReferenceAsync(referencedTemplates, layoutTemplate.LayoutTemplateAlias, cancellationToken);
    }
}
