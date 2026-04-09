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

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Factories.DomainPresentationFactory"/> class.
    /// </summary>
    /// <param name="entityService">An <see cref="IEntityService"/> instance used to perform domain-related operations.</param>
    public DomainPresentationFactory(IEntityService entityService)
        => _entityService = entityService;

    /// <summary>
    /// Creates a collection of <see cref="DomainAssignmentModel"/> instances for the provided domains.
    /// Only domains with a valid <c>RootContentId</c> and a successfully resolved content key are included.
    /// </summary>
    /// <param name="domains">The collection of domains to create assignment models for.</param>
    /// <returns>
    /// An enumerable of <see cref="DomainAssignmentModel"/> representing domain assignments for domains with valid root content.
    /// </returns>
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
