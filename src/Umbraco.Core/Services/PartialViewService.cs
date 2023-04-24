﻿using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Snippets;
using Umbraco.New.Cms.Core.Models;
using PartialViewSnippet = Umbraco.Cms.Core.Snippets.PartialViewSnippet;

namespace Umbraco.Cms.Core.Services;

public class PartialViewService : FileServiceBase, IPartialViewService
{
    private readonly PartialViewSnippetCollection _snippetCollection;
    private readonly IPartialViewRepository _partialViewRepository;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly ILogger<PartialViewService> _logger;
    private readonly IAuditRepository _auditRepository;

    protected override string[] AllowedFileExtensions { get; } = { ".cshtml" };

    public PartialViewService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        PartialViewSnippetCollection snippetCollection,
        IPartialViewRepository partialViewRepository,
        IUserIdKeyResolver userIdKeyResolver,
        ILogger<PartialViewService> logger,
        IAuditRepository auditRepository)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _snippetCollection = snippetCollection;
        _partialViewRepository = partialViewRepository;
        _userIdKeyResolver = userIdKeyResolver;
        _logger = logger;
        _auditRepository = auditRepository;
    }

    public Task<IPartialView?> GetAsync(string path)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IPartialView? partialView = _partialViewRepository.Get(path);

        scope.Complete();
        return Task.FromResult(partialView);
    }

    public async Task<PartialViewOperationStatus> DeleteAsync(string path, Guid performingUserKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IPartialView? partialView = _partialViewRepository.Get(path);
        if (partialView is null)
        {
            return PartialViewOperationStatus.NotFound;
        }

        EventMessages eventMessages = EventMessagesFactory.Get();

        var deletingNotification = new PartialViewDeletingNotification(partialView, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
        {
            return PartialViewOperationStatus.CancelledByNotification;
        }

        _partialViewRepository.Delete(partialView);

        scope.Notifications.Publish(
            new PartialViewDeletedNotification(partialView, eventMessages).WithStateFrom(deletingNotification));

        await AuditAsync(AuditType.Delete, performingUserKey);
        return PartialViewOperationStatus.Success;
    }

    public Task<PagedModel<string>> GetSnippetNamesAsync(int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        string[] names = _snippetCollection.GetNames().ToArray();
        var total = names.Length;

        IEnumerable<string> snippets = names
            .Skip(skip)
            .Take(take);

        return Task.FromResult(new PagedModel<string>(total, snippets));
    }

    public Task<PartialViewSnippet?> GetSnippetByNameAsync(string name)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        // A bit weird but the "Name" of the snippet is the file name and extensions
        // However when getting the content it's just the name without the extension
        var fileName = name + ".cshtml";
        if (_snippetCollection.Any(x => x.Name == fileName) is false)
        {
            return Task.FromResult<PartialViewSnippet?>(null);
        }

        var content = _snippetCollection.GetContentFromName(name);
        var snippet = new PartialViewSnippet(name, content);

        return Task.FromResult<PartialViewSnippet?>(snippet);
    }

    public async Task<Attempt<IPartialView?, PartialViewOperationStatus>> CreateAsync(PartialViewCreateModel createModel, Guid performingUserKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        try
        {
            PartialViewOperationStatus validationResult = ValidateCreate(createModel);
            if (validationResult is not PartialViewOperationStatus.Success)
            {
                return Attempt.FailWithStatus<IPartialView?, PartialViewOperationStatus>(validationResult, null);
            }
        }
        catch (PathTooLongException exception)
        {
            _logger.LogError(exception, "The partial view path was too long");
            return Attempt.FailWithStatus<IPartialView?, PartialViewOperationStatus>(PartialViewOperationStatus.PathTooLong, null);
        }

        var partialView = new PartialView(PartialViewType.PartialView, createModel.FilePath) { Content = createModel.Content };

        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingNotification = new PartialViewSavingNotification(partialView, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            return Attempt.FailWithStatus<IPartialView?, PartialViewOperationStatus>(
                PartialViewOperationStatus.CancelledByNotification, null);
        }

        _partialViewRepository.Save(partialView);
        scope.Notifications.Publish(new PartialViewSavedNotification(partialView, eventMessages).WithStateFrom(savingNotification));
        await AuditAsync(AuditType.Save, performingUserKey);

        scope.Complete();
        return Attempt.SucceedWithStatus<IPartialView?, PartialViewOperationStatus>(PartialViewOperationStatus.Success, partialView);
    }

    private PartialViewOperationStatus ValidateCreate(PartialViewCreateModel createModel)
    {
        if (_partialViewRepository.Exists(createModel.FilePath))
        {
            return PartialViewOperationStatus.AlreadyExists;
        }

        if (string.IsNullOrWhiteSpace(createModel.ParentPath) is false &&
            _partialViewRepository.FolderExists(createModel.ParentPath) is false)
        {
            return PartialViewOperationStatus.ParentNotFound;
        }

        if (HasValidFileName(createModel.Name) is false)
        {
            return PartialViewOperationStatus.InvalidName;
        }

        if (HasValidFileExtension(createModel.FilePath) is false)
        {
            return PartialViewOperationStatus.InvalidFileExtension;
        }

        return PartialViewOperationStatus.Success;
    }

    public async Task<Attempt<IPartialView?, PartialViewOperationStatus>> UpdateAsync(PartialViewUpdateModel updateModel, Guid performingUserKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IPartialView? partialView = _partialViewRepository.Get(updateModel.ExistingPath);

        if (partialView is null)
        {
            return Attempt.FailWithStatus<IPartialView?, PartialViewOperationStatus>(PartialViewOperationStatus.NotFound, null);
        }

        PartialViewOperationStatus validationResult = ValidateUpdate(updateModel);
        if (validationResult is not PartialViewOperationStatus.Success)
        {
            return Attempt.FailWithStatus<IPartialView?, PartialViewOperationStatus>(validationResult, null);
        }

        partialView.Content = updateModel.Content;
        if (partialView.Name != updateModel.Name)
        {
            var newPath = partialView.Path.Replace(partialView.Name!, updateModel.Name);
            partialView.Path = newPath;
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingNotification = new PartialViewSavingNotification(partialView, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            return Attempt.FailWithStatus<IPartialView?, PartialViewOperationStatus>(PartialViewOperationStatus.CancelledByNotification, null);
        }

        _partialViewRepository.Save(partialView);
        scope.Notifications.Publish(new PartialViewSavedNotification(partialView, eventMessages).WithStateFrom(savingNotification));

        await AuditAsync(AuditType.Save, performingUserKey);

        scope.Complete();
        return Attempt.SucceedWithStatus<IPartialView?, PartialViewOperationStatus>(PartialViewOperationStatus.Success, partialView);
    }

    private PartialViewOperationStatus ValidateUpdate(PartialViewUpdateModel updateModel)
    {
        if (HasValidFileExtension(updateModel.Name) is false)
        {
            return PartialViewOperationStatus.InvalidFileExtension;
        }

        if (HasValidFileName(updateModel.Name) is false)
        {
            return PartialViewOperationStatus.InvalidName;
        }

        return PartialViewOperationStatus.Success;
    }

    private async Task AuditAsync(AuditType type, Guid performingUserKey)
    {
        int userId = await _userIdKeyResolver.GetAsync(performingUserKey);
        _auditRepository.Save(new AuditItem(-1, type, userId, "PartialView"));
    }
}
