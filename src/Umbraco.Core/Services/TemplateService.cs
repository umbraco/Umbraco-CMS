using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public class TemplateService : RepositoryService, ITemplateService
{
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ITemplateRepository _templateRepository;
    private readonly IAuditService _auditService;
    private readonly ITemplateContentParserService _templateContentParserService;

    public TemplateService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IShortStringHelper shortStringHelper,
        ITemplateRepository templateRepository,
        IAuditService auditService,
        ITemplateContentParserService templateContentParserService)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _shortStringHelper = shortStringHelper;
        _templateRepository = templateRepository;
        _auditService = auditService;
        _templateContentParserService = templateContentParserService;
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    public TemplateService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IShortStringHelper shortStringHelper,
        ITemplateRepository templateRepository,
        IAuditRepository auditRepository,
        ITemplateContentParserService templateContentParserService,
        IUserIdKeyResolver userIdKeyResolver,
        IDefaultViewContentProvider defaultViewContentProvider)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            shortStringHelper,
            templateRepository,
            StaticServiceProvider.Instance.GetRequiredService<IAuditService>(),
            templateContentParserService)
    {
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled removal in v19.")]
    public TemplateService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IShortStringHelper shortStringHelper,
        ITemplateRepository templateRepository,
        IAuditService auditService,
        IAuditRepository auditRepository,
        ITemplateContentParserService templateContentParserService,
        IUserIdKeyResolver userIdKeyResolver,
        IDefaultViewContentProvider defaultViewContentProvider)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            shortStringHelper,
            templateRepository,
            auditService,
            templateContentParserService)
    {
    }

    /// <inheritdoc />
    [Obsolete("Use the overload that includes name and alias parameters instead. Scheduled for removal in v19.")]
    public async Task<Attempt<ITemplate, TemplateOperationStatus>> CreateForContentTypeAsync(
        string contentTypeAlias,
        string? contentTypeName,
        Guid userKey)
    {
        ITemplate template = new Template(
            _shortStringHelper,
            contentTypeName,
            // NOTE: We are NOT passing in the content type alias here, we want to use it's name since we don't
            // want to save template file names as camelCase, the Template ctor will clean the alias as
            // `alias.ToCleanString(CleanStringType.UnderscoreAlias)` which has been the default.
            // This fixes: http://issues.umbraco.org/issue/U4-7953
            contentTypeName);

        if (IsValidAlias(template.Alias) == false)
        {
            return Attempt.FailWithStatus(TemplateOperationStatus.InvalidAlias, template);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();

        // check that the template hasn't been created on disk before creating the content type
        // if it exists, set the new template content to the existing file content
        var content = GetViewContent(template.Alias);
        if (content != null)
        {
            template.Content = content;
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var savingEvent = new TemplateSavingNotification(template, eventMessages, true, contentTypeAlias!);
            if (await scope.Notifications.PublishCancelableAsync(savingEvent))
            {
                scope.Complete();
                return Attempt.FailWithStatus(TemplateOperationStatus.CancelledByNotification, template);
            }

            _templateRepository.Save(template);
            scope.Notifications.Publish(
                new TemplateSavedNotification(template, eventMessages).WithStateFrom(savingEvent));

            await Audit(AuditType.New, userKey, template.Id, UmbracoObjectTypes.Template.GetName());
            scope.Complete();
        }

        return Attempt.SucceedWithStatus(TemplateOperationStatus.Success, template);
    }

    /// <inheritdoc />
    public async Task<Attempt<ITemplate?, TemplateOperationStatus>> CreateForContentTypeAsync(
        string name,
        string alias,
        string contentTypeAlias,
        Guid userKey)
    {
        ITemplate template =
            new Template(_shortStringHelper, name, alias) { Key = Guid.CreateVersion7() };

        Attempt<ITemplate, TemplateOperationStatus> result = await CreateAsync(template, userKey, contentTypeAlias);
        return result.Success
            ? Attempt.SucceedWithStatus<ITemplate?, TemplateOperationStatus>(result.Status, result.Result)
            : Attempt<ITemplate?, TemplateOperationStatus>.Fail(result.Status);
    }

    /// <inheritdoc />
    public async Task<Attempt<ITemplate, TemplateOperationStatus>> CreateAsync(
        string name,
        string alias,
        string? content,
        Guid userKey,
        Guid? templateKey = null)
        => await CreateAsync(new Template(_shortStringHelper, name, alias) { Content = content, Key = templateKey ?? Guid.NewGuid() }, userKey);

    /// <inheritdoc />
    public async Task<Attempt<ITemplate, TemplateOperationStatus>> CreateAsync(ITemplate template, Guid userKey)
        => await CreateAsync(template, userKey, null);

    private TemplateOperationStatus ValidateCreate(ITemplate templateToCreate)
    {
        ITemplate? existingTemplate = GetAsync(templateToCreate.Alias).GetAwaiter().GetResult();
        if (existingTemplate is not null)
        {
            return TemplateOperationStatus.DuplicateAlias;
        }

        return TemplateOperationStatus.Success;
    }

    /// <inheritdoc />
    public Task<IEnumerable<ITemplate>> GetAllAsync(params string[] aliases)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return Task.FromResult<IEnumerable<ITemplate>>(_templateRepository.GetAll(aliases).OrderBy(x => x.Name));
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<ITemplate>> GetAllAsync(params Guid[] keys)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<ITemplate> templates = _templateRepository.GetMany(keys);
        return Task.FromResult(templates);
    }

    /// <inheritdoc />
    public Task<IEnumerable<ITemplate>> GetChildrenAsync(int masterTemplateId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return Task.FromResult<IEnumerable<ITemplate>>(_templateRepository.GetChildren(masterTemplateId).OrderBy(x => x.Name));
        }
    }

    /// <inheritdoc />
    public Task<ITemplate?> GetAsync(string? alias)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return Task.FromResult(_templateRepository.Get(alias));
        }
    }

    /// <inheritdoc />
    public Task<ITemplate?> GetAsync(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return Task.FromResult(_templateRepository.Get(id));
        }
    }

    /// <inheritdoc />
    public Task<ITemplate?> GetAsync(Guid id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        ITemplate? template = _templateRepository.Get(id);
        return Task.FromResult(template);
    }

    /// <inheritdoc />
    public Task<IEnumerable<ITemplate>> GetDescendantsAsync(int masterTemplateId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return Task.FromResult(_templateRepository.GetDescendants(masterTemplateId));
        }
    }

    /// <inheritdoc />
    public async Task<Attempt<ITemplate, TemplateOperationStatus>> UpdateAsync(ITemplate template, Guid userKey)
        => await SaveAsync(
            template,
            AuditType.Save,
            userKey,
            // fail the attempt if the template does not exist within the scope
            () => ValidateUpdate(template));

    private TemplateOperationStatus ValidateUpdate(ITemplate templateToUpdate)
    {
        ITemplate? existingTemplate = GetAsync(templateToUpdate.Alias).GetAwaiter().GetResult();
        if (existingTemplate is not null && existingTemplate.Key != templateToUpdate.Key)
        {
            return TemplateOperationStatus.DuplicateAlias;
        }

        if (_templateRepository.Exists(templateToUpdate.Id) is false)
        {
            return TemplateOperationStatus.TemplateNotFound;
        }

        return TemplateOperationStatus.Success;
    }

    private async Task<Attempt<ITemplate, TemplateOperationStatus>> SaveAsync(
            ITemplate template,
            AuditType auditType,
            Guid userKey,
            Func<TemplateOperationStatus>? scopeValidator = null,
            string? contentTypeAlias = null)
    {
        if (IsValidAlias(template.Alias) == false)
        {
            return Attempt.FailWithStatus(TemplateOperationStatus.InvalidAlias, template);
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            TemplateOperationStatus scopeValidatorStatus = scopeValidator?.Invoke() ?? TemplateOperationStatus.Success;
            if (scopeValidatorStatus != TemplateOperationStatus.Success)
            {
                return Attempt.FailWithStatus(scopeValidatorStatus, template);
            }

            var masterTemplateAlias = _templateContentParserService.MasterTemplateAlias(template.Content);
            ITemplate? masterTemplate = masterTemplateAlias.IsNullOrWhiteSpace()
                ? null
                : await GetAsync(masterTemplateAlias);

            // fail if the template content specifies a master template but said template does not exist
            if (masterTemplateAlias.IsNullOrWhiteSpace() == false && masterTemplate == null)
            {
                return Attempt.FailWithStatus(TemplateOperationStatus.MasterTemplateNotFound, template);
            }

            // detect circular references
            if (masterTemplateAlias is not null
                && masterTemplate is not null
                && await HasCircularReference(masterTemplateAlias, template, masterTemplate))
            {
                return Attempt.FailWithStatus(TemplateOperationStatus.CircularMasterTemplateReference, template);
            }

            await SetMasterTemplateAsync(template, masterTemplate, userKey);

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

            _templateRepository.Save(template);

            scope.Notifications.Publish(
                new TemplateSavedNotification(template, eventMessages).WithStateFrom(savingNotification));

            await Audit(auditType, userKey, template.Id, UmbracoObjectTypes.Template.GetName());
            scope.Complete();
            return Attempt.SucceedWithStatus(TemplateOperationStatus.Success, template);
        }
    }

    /// <inheritdoc />
    public async Task<Attempt<ITemplate?, TemplateOperationStatus>> DeleteAsync(string alias, Guid userKey)
        => await DeleteAsync(() => Task.FromResult(_templateRepository.Get(alias)), userKey);

    /// <inheritdoc />
    public async Task<Attempt<ITemplate?, TemplateOperationStatus>> DeleteAsync(Guid key, Guid userKey)
        => await DeleteAsync(async () => await GetAsync(key), userKey);

    /// <inheritdoc />
    public Task<Stream> GetFileContentStreamAsync(string filepath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return Task.FromResult(_templateRepository.GetFileContentStream(filepath));
        }
    }

    /// <inheritdoc />
    public Task SetFileContentAsync(string filepath, Stream content)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _templateRepository.SetFileContent(filepath, content);
            scope.Complete();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<long> GetFileSizeAsync(string filepath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return Task.FromResult(_templateRepository.GetFileSize(filepath));
        }
    }

    /// <inheritdoc />
    private async Task SetMasterTemplateAsync(ITemplate template, ITemplate? masterTemplate, Guid userKey)
    {
        if (template.MasterTemplateAlias == masterTemplate?.Alias)
        {
            return;
        }

        if (masterTemplate != null)
        {
            if (masterTemplate.Id == template.Id)
            {
                template.SetMasterTemplate(null);
            }
            else
            {
                template.SetMasterTemplate(masterTemplate);

                //After updating the master - ensure we update the path property if it has any children already assigned
                if (template.Id > 0)
                {
                    IEnumerable<ITemplate> templateHasChildren = await GetDescendantsAsync(template.Id);

                    foreach (ITemplate childTemplate in templateHasChildren)
                    {
                        //template ID to find
                        var templateIdInPath = "," + template.Id + ",";

                        if (string.IsNullOrEmpty(childTemplate.Path))
                        {
                            continue;
                        }

                        //Find position in current comma separate string path (so we get the correct children path)
                        var positionInPath = childTemplate.Path.IndexOf(templateIdInPath) + templateIdInPath.Length;

                        //Get the substring of the child & any children (descendants it may have too)
                        var childTemplatePath = childTemplate.Path.Substring(positionInPath);

                        //As we are updating the template to be a child of a master
                        //Set the path to the master's path + its current template id + the current child path substring
                        childTemplate.Path = masterTemplate.Path + "," + template.Id + "," + childTemplatePath;

                        //Save the children with the updated path
                        await UpdateAsync(childTemplate, userKey);
                    }
                }
            }
        }
        else
        {
            //remove the master
            template.SetMasterTemplate(null);
        }
    }

    private string? GetViewContent(string? fileName)
    {
        if (fileName.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        if (!fileName!.EndsWith(".cshtml"))
        {
            fileName = $"{fileName}.cshtml";
        }

        Stream fs = _templateRepository.GetFileContentStream(fileName);

        using (var view = new StreamReader(fs))
        {
            return view.ReadToEnd().Trim().NullOrWhiteSpaceAsNull();
        }
    }

    private Task Audit(AuditType type, Guid userKey, int objectId, string? entityType) =>
        _auditService.AddAsync(type, userKey, objectId, entityType);

    private async Task<Attempt<ITemplate, TemplateOperationStatus>> CreateAsync(ITemplate template, Guid userKey, string? contentTypeAlias)
    {
        if (IsValidAlias(template.Alias) is false)
        {
            return Attempt.FailWithStatus(TemplateOperationStatus.InvalidAlias, template);
        }

        try
        {
            // file might already be on disk, if so grab the content to avoid overwriting
            template.Content = GetViewContent(template.Alias) ?? template.Content;
            return await SaveAsync(template, AuditType.New, userKey, () => ValidateCreate(template), contentTypeAlias);
        }
        catch (PathTooLongException ex)
        {
            LoggerFactory.CreateLogger<TemplateService>().LogError(ex, "The template path was too long. Consider making the template alias shorter.");
            return Attempt.FailWithStatus(TemplateOperationStatus.InvalidAlias, template);
        }
    }

    private async Task<Attempt<ITemplate?, TemplateOperationStatus>> DeleteAsync(Func<Task<ITemplate?>> getTemplate, Guid userKey)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            ITemplate? template = await getTemplate();
            if (template == null)
            {
                scope.Complete();
                return Attempt.FailWithStatus<ITemplate?, TemplateOperationStatus>(TemplateOperationStatus.TemplateNotFound, null);
            }

            if (template.IsMasterTemplate)
            {
                scope.Complete();
                return Attempt.FailWithStatus<ITemplate?, TemplateOperationStatus>(TemplateOperationStatus.MasterTemplateCannotBeDeleted, null);
            }

            EventMessages eventMessages = EventMessagesFactory.Get();
            var deletingNotification = new TemplateDeletingNotification(template, eventMessages);
            if (scope.Notifications.PublishCancelable(deletingNotification))
            {
                scope.Complete();
                return Attempt.FailWithStatus<ITemplate?, TemplateOperationStatus>(TemplateOperationStatus.CancelledByNotification, template);
            }

            _templateRepository.Delete(template);

            scope.Notifications.Publish(
                new TemplateDeletedNotification(template, eventMessages).WithStateFrom(deletingNotification));

            await Audit(AuditType.Delete, userKey, template.Id, UmbracoObjectTypes.Template.GetName());
            scope.Complete();
            return Attempt.SucceedWithStatus<ITemplate?, TemplateOperationStatus>(TemplateOperationStatus.Success, template);
        }
    }

    private static bool IsValidAlias(string alias)
        => alias.IsNullOrWhiteSpace() == false && alias.Length <= 255;

    private async Task<bool> HasCircularReference(string parsedMasterTemplateAlias, ITemplate template, ITemplate masterTemplate)
    {
        // quick check without extra DB calls as we already have both templates
        if (parsedMasterTemplateAlias.IsNullOrWhiteSpace() is false
            && masterTemplate.MasterTemplateAlias is not null
            && masterTemplate.MasterTemplateAlias.Equals(template.Alias))
        {
            return true;
        }

        var processedTemplates = new List<ITemplate> { template, masterTemplate };
        return await HasRecursiveCircularReference(processedTemplates, masterTemplate.MasterTemplateAlias);
    }

    private async Task<bool> HasRecursiveCircularReference(List<ITemplate> referencedTemplates, string? masterTemplateAlias)
    {
        if (masterTemplateAlias is null)
        {
            return false;
        }

        if (referencedTemplates.Any(template => template.Alias.Equals(masterTemplateAlias)))
        {
            return true;
        }

        ITemplate? masterTemplate = await GetAsync(masterTemplateAlias);
        if (masterTemplate is null)
        {
            // this should not happen unless somebody manipulated the data by hand as this function is only called between persisted items
            return false;
        }

        referencedTemplates.Add(masterTemplate);

        return await HasRecursiveCircularReference(referencedTemplates, masterTemplate.MasterTemplateAlias);
    }
}
