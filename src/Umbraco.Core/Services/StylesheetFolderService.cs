using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public class StylesheetFolderService : PathFolderServiceBase<IStylesheetRepository, StylesheetFolderOperationStatus>, IStylesheetFolderService
{
    private readonly IStylesheetRepository _stylesheetRepository;

    public StylesheetFolderService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IStylesheetRepository stylesheetRepository)
        : base(provider, loggerFactory, eventMessagesFactory) =>
        _stylesheetRepository = stylesheetRepository;

    protected override IStylesheetRepository Repository => _stylesheetRepository;

    protected override StylesheetFolderOperationStatus SuccessStatus => StylesheetFolderOperationStatus.Success;

    protected override Task<Attempt<StylesheetFolderOperationStatus>> ValidateCreateAsync(PathContainer container)
    {
        if (container.Name.ContainsAny(Path.GetInvalidFileNameChars()))
        {
            return Task.FromResult(Attempt.Fail(StylesheetFolderOperationStatus.InvalidName));
        }

        if (_stylesheetRepository.FolderExists(container.Path))
        {
            return Task.FromResult(Attempt.Fail(StylesheetFolderOperationStatus.AlreadyExists));
        }

        if (string.IsNullOrWhiteSpace(container.ParentPath) is false &&
            _stylesheetRepository.FolderExists(container.ParentPath) is false)
        {
            return Task.FromResult(Attempt.Fail(StylesheetFolderOperationStatus.ParentNotFound));
        }

        return Task.FromResult(Attempt.Succeed(StylesheetFolderOperationStatus.Success));
    }

    protected override Task<Attempt<StylesheetFolderOperationStatus>> ValidateDeleteAsync(string path)
    {
        if(_stylesheetRepository.FolderExists(path) is false)
        {
            return Task.FromResult(Attempt.Fail(StylesheetFolderOperationStatus.NotFound));
        }

        if (_stylesheetRepository.FolderHasContent(path))
        {
            return Task.FromResult(Attempt.Fail(StylesheetFolderOperationStatus.NotEmpty));
        }

        return Task.FromResult(Attempt.Succeed(StylesheetFolderOperationStatus.Success));
    }
}
