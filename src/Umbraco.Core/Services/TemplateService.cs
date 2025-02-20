using Microsoft.Extensions.Logging;
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
    private readonly IAuditRepository _auditRepository;
    private readonly ITemplateContentParserService _templateContentParserService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly IDefaultViewContentProvider _defaultViewContentProvider;

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
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _shortStringHelper = shortStringHelper;
        _templateRepository = templateRepository;
        _auditRepository = auditRepository;
        _templateContentParserService = templateContentParserService;
        _userIdKeyResolver = userIdKeyResolver;
        _defaultViewContentProvider = defaultViewContentProvider;
    }

    /// <inheritdoc />
    public async Task<Attempt<ITemplate, TemplateOperationStatus>> CreateForContentTypeAsync(
        string contentTypeAlias, string? contentTypeName, Guid userKey)
    {
        ITemplate template = new Template(_shortStringHelper, contentTypeName,

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
        var content = GetViewContent(contentTypeAlias);
        if (content != null)
        {
            template.Content = content;
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var savingEvent = new TemplateSavingNotification(template, eventMessages, true, contentTypeAlias!);
            if (scope.Notifications.PublishCancelable(savingEvent))
            {
                scope.Complete();
                return Attempt.FailWithStatus(TemplateOperationStatus.CancelledByNotification, template);
            }

            _templateRepository.Save(template);
            scope.Notifications.Publish(
                new TemplateSavedNotification(template, eventMessages).WithStateFrom(savingEvent));

            var currentUserId = await _userIdKeyResolver.GetAsync(userKey);
            Audit(AuditType.New, currentUserId, template.Id, UmbracoObjectTypes.Template.GetName());
            scope.Complete();
        }

        return await Task.FromResult(Attempt.SucceedWithStatus(TemplateOperationStatus.Success, template));
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
    {
        try
        {
            // file might already be on disk, if so grab the content to avoid overwriting
            template.Content = GetViewContent(template.Alias) ?? template.Content;
            return await SaveAsync(template, AuditType.New, userKey, () => ValidateCreate(template));
        }
        catch (PathTooLongException ex)
        {
            LoggerFactory.CreateLogger<TemplateService>().LogError(ex, "The template path was too long. Consider making the template alias shorter.");
            return Attempt.FailWithStatus(TemplateOperationStatus.InvalidAlias, template);
        }
    }

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
    public async Task<IEnumerable<ITemplate>> GetAllAsync(params string[] aliases)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_templateRepository.GetAll(aliases).OrderBy(x => x.Name));
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<ITemplate>> GetAllAsync(params Guid[] keys)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        IQuery<ITemplate> query = Query<ITemplate>();
        if (keys.Any())
        {
            query = query.Where(x => keys.Contains(x.Key));
        }

        IEnumerable<ITemplate> templates = _templateRepository.Get(query).OrderBy(x => x.Name);

        return Task.FromResult(templates);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ITemplate>> GetChildrenAsync(int masterTemplateId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_templateRepository.GetChildren(masterTemplateId).OrderBy(x => x.Name));
        }
    }

    /// <inheritdoc />
    public async Task<ITemplate?> GetAsync(string? alias)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_templateRepository.Get(alias));
        }
    }

    /// <inheritdoc />
    public async Task<ITemplate?> GetAsync(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_templateRepository.Get(id));
        }
    }

    /// <inheritdoc />
    public async Task<ITemplate?> GetAsync(Guid id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<ITemplate>? query = Query<ITemplate>().Where(x => x.Key == id);
            return await Task.FromResult(_templateRepository.Get(query)?.SingleOrDefault());
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ITemplate>> GetDescendantsAsync(int masterTemplateId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_templateRepository.GetDescendants(masterTemplateId));
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

    private async Task<Attempt<ITemplate, TemplateOperationStatus>> SaveAsync(ITemplate template, AuditType auditType, Guid userKey, Func<TemplateOperationStatus>? scopeValidator = null)
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
            var savingNotification = new TemplateSavingNotification(template, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return Attempt.FailWithStatus(TemplateOperationStatus.CancelledByNotification, template);
            }

            _templateRepository.Save(template);

            scope.Notifications.Publish(
                new TemplateSavedNotification(template, eventMessages).WithStateFrom(savingNotification));

            var currentUserId = await _userIdKeyResolver.GetAsync(userKey);
            Audit(auditType, currentUserId, template.Id, UmbracoObjectTypes.Template.GetName());
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
    public async Task<Stream> GetFileContentStreamAsync(string filepath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_templateRepository.GetFileContentStream(filepath));
        }
    }

    /// <inheritdoc />
    public async Task SetFileContentAsync(string filepath, Stream content)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _templateRepository.SetFileContent(filepath, content);
            scope.Complete();
            await Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public async Task<long> GetFileSizeAsync(string filepath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_templateRepository.GetFileSize(filepath));
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

    private void Audit(AuditType type, int userId, int objectId, string? entityType) =>
        _auditRepository.Save(new AuditItem(objectId, type, userId, entityType));

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

            var currentUserId = await _userIdKeyResolver.GetAsync(userKey);
            Audit(AuditType.Delete, currentUserId, template.Id, UmbracoObjectTypes.Template.GetName());
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
