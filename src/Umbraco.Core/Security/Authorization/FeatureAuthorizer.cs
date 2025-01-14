using Umbraco.Cms.Core.Features;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <inheritdoc />
internal sealed class FeatureAuthorizer : IFeatureAuthorizer
{
    private readonly UmbracoFeatures _umbracoFeatures;

    public FeatureAuthorizer(UmbracoFeatures umbracoFeatures) => _umbracoFeatures = umbracoFeatures;

    /// <inheritdoc />
    public async Task<bool> IsDeniedAsync(Type type) =>
        await Task.FromResult(_umbracoFeatures.IsControllerEnabled(type) is false);
}
