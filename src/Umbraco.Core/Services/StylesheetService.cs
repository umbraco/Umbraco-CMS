using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public class StylesheetService : FileServiceBase, IStylesheetService
{
    private readonly IStylesheetRepository _stylesheetRepository;
    private readonly ILogger<StylesheetService> _logger;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly IAuditRepository _auditRepository;

    protected override string[] AllowedFileExtensions { get; } = { ".css" };

    public StylesheetService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IStylesheetRepository stylesheetRepository,
        ILogger<StylesheetService> logger,
        IUserIdKeyResolver userIdKeyResolver,
        IAuditRepository auditRepository)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _stylesheetRepository = stylesheetRepository;
        _logger = logger;
        _userIdKeyResolver = userIdKeyResolver;
        _auditRepository = auditRepository;
    }

    /// <inheritdoc />
    public Task<IStylesheet?> GetAsync(string path)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IStylesheet? stylesheet = _stylesheetRepository.Get(path);

        scope.Complete();
        return Task.FromResult(stylesheet);
    }

    public async Task<StylesheetOperationStatus> DeleteAsync(string path, Guid performingUserKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IStylesheet? stylesheet = _stylesheetRepository.Get(path);
        if (stylesheet is null)
        {
            return StylesheetOperationStatus.NotFound;
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var deletingNotification = new StylesheetDeletingNotification(stylesheet, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
        {
            return StylesheetOperationStatus.CancelledByNotification;
        }

        _stylesheetRepository.Delete(stylesheet);

        scope.Notifications.Publish(new StylesheetDeletedNotification(stylesheet, eventMessages).WithStateFrom(deletingNotification));
        await AuditAsync(AuditType.Delete, performingUserKey);

        scope.Complete();
        return StylesheetOperationStatus.Success;
    }

    public async Task<Attempt<IStylesheet?, StylesheetOperationStatus>> CreateAsync(StylesheetCreateModel createModel, Guid performingUserKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        try
        {
            StylesheetOperationStatus validationResult = await ValidateCreateAsync(createModel);
            if (validationResult is not StylesheetOperationStatus.Success)
            {
                return Attempt.FailWithStatus<IStylesheet?, StylesheetOperationStatus>(validationResult, null);
            }
        }
        catch (PathTooLongException exception)
        {
            _logger.LogError(exception, "The stylesheet path was too long");
            return Attempt.FailWithStatus<IStylesheet?, StylesheetOperationStatus>(StylesheetOperationStatus.PathTooLong, null);
        }

        var stylesheet = new Stylesheet(createModel.FilePath) { Content = createModel.Content };

        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingNotification = new StylesheetSavingNotification(stylesheet, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            return Attempt.FailWithStatus<IStylesheet?, StylesheetOperationStatus>(StylesheetOperationStatus.CancelledByNotification, null);
        }

        _stylesheetRepository.Save(stylesheet);

        scope.Notifications.Publish(new StylesheetSavedNotification(stylesheet, eventMessages).WithStateFrom(savingNotification));
        await AuditAsync(AuditType.Save, performingUserKey);

        scope.Complete();
        return Attempt.SucceedWithStatus<IStylesheet?, StylesheetOperationStatus>(StylesheetOperationStatus.Success, stylesheet);
    }

    private Task<StylesheetOperationStatus> ValidateCreateAsync(StylesheetCreateModel createModel)
    {
        if (_stylesheetRepository.Exists(createModel.FilePath))
        {
            return Task.FromResult(StylesheetOperationStatus.AlreadyExists);
        }

        if (string.IsNullOrWhiteSpace(createModel.ParentPath) is false
           && _stylesheetRepository.Exists(createModel.ParentPath) is false)
        {
            return Task.FromResult(StylesheetOperationStatus.ParentNotFound);
        }

        if (HasValidFileName(createModel.Name) is false)
        {
            return Task.FromResult(StylesheetOperationStatus.InvalidName);
        }

        if (HasValidFileExtension(createModel.FilePath) is false)
        {
            return Task.FromResult(StylesheetOperationStatus.InvalidFileExtension);
        }

        return Task.FromResult(StylesheetOperationStatus.Success);
    }

    public async Task<Attempt<IStylesheet?, StylesheetOperationStatus>> UpdateAsync(StylesheetUpdateModel updateModel, Guid performingUserKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IStylesheet? stylesheet = _stylesheetRepository.Get(updateModel.ExistingPath);

        if(stylesheet is null)
        {
            return Attempt.FailWithStatus<IStylesheet?, StylesheetOperationStatus>(StylesheetOperationStatus.NotFound, null);
        }

        StylesheetOperationStatus validationResult = ValidateUpdate(updateModel);
        if (validationResult is not StylesheetOperationStatus.Success)
        {
            return Attempt.FailWithStatus<IStylesheet?, StylesheetOperationStatus>(validationResult, null);
        }

        stylesheet.Content = updateModel.Content;
        if (stylesheet.Name != updateModel.Name)
        {
            // Name has been updated, so we need to update the path as well
            var newPath = stylesheet.Path.Replace(stylesheet.Name!, updateModel.Name);
            stylesheet.Path = newPath;
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingNotification = new StylesheetSavingNotification(stylesheet, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            return Attempt.FailWithStatus<IStylesheet?, StylesheetOperationStatus>(StylesheetOperationStatus.CancelledByNotification, null);
        }

        _stylesheetRepository.Save(stylesheet);

        scope.Notifications.Publish(new StylesheetSavedNotification(stylesheet, eventMessages).WithStateFrom(savingNotification));
        await AuditAsync(AuditType.Save, performingUserKey);

        scope.Complete();
        return Attempt.SucceedWithStatus<IStylesheet?, StylesheetOperationStatus>(StylesheetOperationStatus.Success, stylesheet);
    }

    private StylesheetOperationStatus ValidateUpdate(StylesheetUpdateModel updateModel)
    {
        if (HasValidFileExtension(updateModel.Name) is false)
        {
            return StylesheetOperationStatus.InvalidFileExtension;
        }

        if (HasValidFileName(updateModel.Name) is false)
        {
            return StylesheetOperationStatus.InvalidName;
        }

        return StylesheetOperationStatus.Success;
    }

    private async Task AuditAsync(AuditType type, Guid performingUserKey)
    {
        var userId = await _userIdKeyResolver.GetAsync(performingUserKey);
        _auditRepository.Save(new AuditItem(-1, type, userId, "Stylesheet"));
    }
}
