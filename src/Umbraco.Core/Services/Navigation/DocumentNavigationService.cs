using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Navigation;

internal sealed class DocumentNavigationService : ContentNavigationServiceBase, IDocumentNavigationQueryService, IDocumentNavigationManagementService
{
    public DocumentNavigationService(ICoreScopeProvider coreScopeProvider, INavigationRepository navigationRepository)
        : base(coreScopeProvider, navigationRepository)
    {
    }

    public override async Task RebuildAsync()
        => await HandleRebuildAsync(Constants.Locks.ContentTree, Constants.ObjectTypes.Document, false);

    public override async Task RebuildBinAsync()
        => await HandleRebuildAsync(Constants.Locks.ContentTree, Constants.ObjectTypes.Document, true);
}
