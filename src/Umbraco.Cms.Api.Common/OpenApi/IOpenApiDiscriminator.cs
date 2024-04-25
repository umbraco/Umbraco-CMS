namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Marker interface that ensure the type have a "$type" discriminator in the open api schema.
/// </summary>
/// <remarks>
/// This is required when an endpoint can receive different types, to ensure the correct type is deserialized.
/// </remarks>
public interface IOpenApiDiscriminator
{
}
