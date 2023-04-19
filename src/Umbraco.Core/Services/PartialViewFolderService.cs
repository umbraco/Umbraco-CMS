using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public class PartialViewFolderService : PathFolderServiceBase<IPartialViewRepository, PartialViewFolderOperationStatus>, IPartialViewFolderService
{
    private readonly IPartialViewRepository _partialViewRepository;

    public PartialViewFolderService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IPartialViewRepository partialViewRepository)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _partialViewRepository = partialViewRepository;
    }

    protected override IPartialViewRepository Repository => _partialViewRepository;

    protected override PartialViewFolderOperationStatus SuccessStatus => PartialViewFolderOperationStatus.Success;

    protected override Task<Attempt<PartialViewFolderOperationStatus>> ValidateCreateAsync(PathContainer container)
    {
        if(container.Name.ContainsAny(Path.GetInvalidFileNameChars()))
        {
            return Task.FromResult(Attempt.Fail(PartialViewFolderOperationStatus.InvalidName));
        }

        if(_partialViewRepository.FolderExists(container.Path))
        {
            return Task.FromResult(Attempt.Fail(PartialViewFolderOperationStatus.AlreadyExists));
        }

        if(string.IsNullOrWhiteSpace(container.ParentPath) is false &&
           _partialViewRepository.FolderExists(container.ParentPath) is false)
        {
            return Task.FromResult(Attempt.Fail(PartialViewFolderOperationStatus.ParentNotFound));
        }

        return Task.FromResult(Attempt.Succeed(PartialViewFolderOperationStatus.Success));
    }

    protected override Task<Attempt<PartialViewFolderOperationStatus>> ValidateDeleteAsync(string path)
    {
        if (_partialViewRepository.FolderExists(path) is false)
        {
            return Task.FromResult(Attempt.Fail(PartialViewFolderOperationStatus.NotFound));
        }

        if (_partialViewRepository.FolderHasContent(path))
        {
            return Task.FromResult(Attempt.Fail(PartialViewFolderOperationStatus.NotEmpty));
        }

        return Task.FromResult(Attempt.Succeed(PartialViewFolderOperationStatus.Success));
    }
}
