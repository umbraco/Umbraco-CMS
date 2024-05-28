using Umbraco.Cms.Core.Features;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <inheritdoc />
internal sealed class FeatureAuthorizer : IFeatureAuthorizer
{
    private readonly UmbracoFeatures _umbracoFeatures;

    public FeatureAuthorizer(UmbracoFeatures umbracoFeatures) => _umbracoFeatures = umbracoFeatures;

    /// <inheritdoc />
    public Task<bool> IsDeniedAsync(Type type)
        => Task.FromResult(_umbracoFeatures.IsControllerEnabled(type) is false);
}
