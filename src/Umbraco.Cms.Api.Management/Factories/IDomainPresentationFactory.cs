using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating domain presentation models used in the management API.
/// </summary>
public interface IDomainPresentationFactory
{
    /// <summary>
    /// Creates domain assignment models from the given collection of domains.
    /// </summary>
    /// <param name="domains">The collection of domains to create assignment models for.</param>
    /// <returns>An enumerable of <see cref="DomainAssignmentModel"/> representing the domain assignments.</returns>
    IEnumerable<DomainAssignmentModel> CreateDomainAssignmentModels(IEnumerable<IDomain> domains);
}
