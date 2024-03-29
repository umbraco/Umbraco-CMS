using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DomainPresentationFactory : IDomainPresentationFactory
{
    private readonly IEntityService _entityService;

    public DomainPresentationFactory(IEntityService entityService)
        => _entityService = entityService;

    public IEnumerable<DomainAssignmentModel> CreateDomainAssignmentModels(IEnumerable<IDomain> domains)
        => domains
            .Where(domain => domain.RootContentId.HasValue)
            .Select(domain =>
            {
                Attempt<Guid> keyResult = _entityService.GetKey(domain.RootContentId!.Value, UmbracoObjectTypes.Document);
                return keyResult.Success
                    ? new DomainAssignmentModel
                    {
                        DomainName = domain.DomainName, Content = new ReferenceByIdModel(keyResult.Result)
                    }
                    : null;
            })
            .WhereNotNull()
            .ToArray();
}
