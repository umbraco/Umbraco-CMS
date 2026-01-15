using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public interface IReferenceResponseModel : IOpenApiDiscriminator
{
    public Guid Id { get; }

    public string? Name { get; }
}
