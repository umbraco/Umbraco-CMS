using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

/// <summary>
/// Defines the contract for a response model containing information about a tracked reference.
/// </summary>
public interface IReferenceResponseModel : IOpenApiDiscriminator
{
    /// <summary>
    /// Gets the unique identifier of the reference.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the name of the reference.
    /// </summary>
    public string? Name { get; }
}
