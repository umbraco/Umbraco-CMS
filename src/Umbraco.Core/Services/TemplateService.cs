using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public class TemplateService : RepositoryService, ITemplateService
{
    private readonly IShortStringHelper _shortStringHelper;
    private readonly ITemplateRepository _templateRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ITemplateContentParserService _templateContentParserService;

    public TemplateService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IShortStringHelper shortStringHelper,
        ITemplateRepository templateRepository,
        IAuditRepository auditRepository,
        ITemplateContentParserService templateContentParserService)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _shortStringHelper = shortStringHelper;
        _templateRepository = templateRepository;
        _auditRepository = auditRepository;
        _templateContentParserService = templateContentParserService;
    }

    /// <inheritdoc />
    public async Task<Attempt<OperationResult<OperationResultType, ITemplate>?>> CreateTemplateForContentTypeAsync(
        string contentTypeAlias, string? contentTypeName, int userId = Constants.Security.SuperUserId)
    {
        var template = new Template(_shortStringHelper, contentTypeName,

            // NOTE: We are NOT passing in the content type alias here, we want to use it's name since we don't
            // want to save template file names as camelCase, the Template ctor will clean the alias as
            // `alias.ToCleanString(CleanStringType.UnderscoreAlias)` which has been the default.
            // This fixes: http://issues.umbraco.org/issue/U4-7953
            contentTypeName);

        EventMessages eventMessages = EventMessagesFactory.Get();

        if (contentTypeAlias != null && contentTypeAlias.Length > 255)
        {
            throw new InvalidOperationException("Name cannot be more than 255 characters in length.");
        }

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
                return OperationResult.Attempt.Fail<OperationResultType, ITemplate>(
                    OperationResultType.FailedCancelledByEvent, eventMessages, template);
            }

            _templateRepository.Save(template);
            scope.Notifications.Publish(
                new TemplateSavedNotification(template, eventMessages).WithStateFrom(savingEvent));

            Audit(AuditType.Save, userId, template.Id, UmbracoObjectTypes.Template.GetName());
            scope.Complete();
        }

        return await Task.FromResult(OperationResult.Attempt.Succeed<OperationResultType, ITemplate>(
            OperationResultType.Success,
            eventMessages,
            template));
    }

    /// <inheritdoc />
    public async Task<ITemplate?> CreateTemplateWithIdentityAsync(
        string? name,
        string? alias,
        string? content,
        int userId = Constants.Security.SuperUserId)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty or contain only white-space characters", nameof(name));
        }

        if (name.Length > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "Name cannot be more than 255 characters in length.");
        }

        // file might already be on disk, if so grab the content to avoid overwriting
        var template = new Template(_shortStringHelper, name, alias) { Content = GetViewContent(alias) ?? content };

        return await SaveTemplateAsync(template, userId)
            ? template
            : null;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ITemplate>> GetTemplatesAsync(params string[] aliases)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_templateRepository.GetAll(aliases).OrderBy(x => x.Name));
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ITemplate>> GetTemplatesAsync(int masterTemplateId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_templateRepository.GetChildren(masterTemplateId).OrderBy(x => x.Name));
        }
    }

    /// <inheritdoc />
    public async Task<ITemplate?> GetTemplateAsync(string? alias)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_templateRepository.Get(alias));
        }
    }

    /// <inheritdoc />
    public async Task<ITemplate?> GetTemplateAsync(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_templateRepository.Get(id));
        }
    }

    /// <inheritdoc />
    public async Task<ITemplate?> GetTemplateAsync(Guid id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<ITemplate>? query = Query<ITemplate>().Where(x => x.Key == id);
            return await Task.FromResult(_templateRepository.Get(query)?.SingleOrDefault());
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ITemplate>> GetTemplateDescendantsAsync(int masterTemplateId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_templateRepository.GetDescendants(masterTemplateId));
        }
    }

    /// <inheritdoc />
    public async Task<bool> SaveTemplateAsync(ITemplate template, int userId = Constants.Security.SuperUserId)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        if (string.IsNullOrWhiteSpace(template.Name) || template.Name.Length > 255)
        {
            throw new InvalidOperationException(
                "Name cannot be null, empty, contain only white-space characters or be more than 255 characters in length.");
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            ITemplate? masterTemplate = await GetMasterTemplate(template.Content);
            await SetMasterTemplateAsync(template, masterTemplate);

            EventMessages eventMessages = EventMessagesFactory.Get();
            var savingNotification = new TemplateSavingNotification(template, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return false;
            }

            _templateRepository.Save(template);

            scope.Notifications.Publish(
                new TemplateSavedNotification(template, eventMessages).WithStateFrom(savingNotification));

            Audit(AuditType.Save, userId, template.Id, UmbracoObjectTypes.Template.GetName());
            scope.Complete();
            return true;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SaveTemplateAsync(IEnumerable<ITemplate> templates, int userId = Constants.Security.SuperUserId)
    {
        ITemplate[] templatesA = templates.ToArray();
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            EventMessages eventMessages = EventMessagesFactory.Get();
            var savingNotification = new TemplateSavingNotification(templatesA, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return false;
            }

            foreach (ITemplate template in templatesA)
            {
                _templateRepository.Save(template);
            }

            scope.Notifications.Publish(
                new TemplateSavedNotification(templatesA, eventMessages).WithStateFrom(savingNotification));

            Audit(AuditType.Save, userId, -1, UmbracoObjectTypes.Template.GetName());
            scope.Complete();
            return await Task.FromResult(true);
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteTemplateAsync(string alias, int userId = Constants.Security.SuperUserId)
        => await DeleteTemplateAsync(() => Task.FromResult(_templateRepository.Get(alias)), userId);

    /// <inheritdoc />
    public async Task<bool> DeleteTemplateAsync(Guid key, int userId = Constants.Security.SuperUserId)
        => await DeleteTemplateAsync(async () => await GetTemplateAsync(key), userId);

    /// <inheritdoc />
    public async Task<Stream> GetTemplateFileContentStreamAsync(string filepath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_templateRepository.GetFileContentStream(filepath));
        }
    }

    /// <inheritdoc />
    public async Task SetTemplateFileContentAsync(string filepath, Stream content)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _templateRepository.SetFileContent(filepath, content);
            scope.Complete();
            await Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public async Task<long> GetTemplateFileSizeAsync(string filepath)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await Task.FromResult(_templateRepository.GetFileSize(filepath));
        }
    }

    private async Task<ITemplate?> GetMasterTemplate(string? content)
    {
        var masterTemplateAlias = _templateContentParserService.MasterTemplateAlias(content);
        ITemplate? masterTemplate = masterTemplateAlias.IsNullOrWhiteSpace()
            ? null
            : await GetTemplateAsync(masterTemplateAlias);

        if (masterTemplateAlias.IsNullOrWhiteSpace() == false && masterTemplate == null)
        {
            throw new ArgumentException($"Could not find master template with alias: {masterTemplateAlias}", content);
        }

        return masterTemplate;
    }

    /// <inheritdoc />
    private async Task SetMasterTemplateAsync(ITemplate template, ITemplate? masterTemplate)
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
                    IEnumerable<ITemplate> templateHasChildren = await GetTemplateDescendantsAsync(template.Id);

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
                        await SaveTemplateAsync(childTemplate);
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

    private async Task<bool> DeleteTemplateAsync(Func<Task<ITemplate?>> getTemplate, int userId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            ITemplate? template = await getTemplate();
            if (template == null)
            {
                scope.Complete();
                return true;
            }

            EventMessages eventMessages = EventMessagesFactory.Get();
            var deletingNotification = new TemplateDeletingNotification(template, eventMessages);
            if (scope.Notifications.PublishCancelable(deletingNotification))
            {
                scope.Complete();
                return false;
            }

            _templateRepository.Delete(template);

            scope.Notifications.Publish(
                new TemplateDeletedNotification(template, eventMessages).WithStateFrom(deletingNotification));

            Audit(AuditType.Delete, userId, template.Id, UmbracoObjectTypes.Template.GetName());
            scope.Complete();
            return await Task.FromResult(true);
        }
    }
}
