using Umbraco.Cms.Core.Features;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <inheritdoc />
internal sealed class FeatureAuthorizer : IFeatureAuthorizer
{
    private readonly UmbracoFeatures _umbracoFeatures;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FeatureAuthorizer" /> class.
    /// </summary>
    /// <param name="umbracoFeatures">The Umbraco features configuration.</param>
    public FeatureAuthorizer(UmbracoFeatures umbracoFeatures) => _umbracoFeatures = umbracoFeatures;

    /// <inheritdoc />
    public Task<bool> IsDeniedAsync(Type type)
        => Task.FromResult(_umbracoFeatures.IsControllerEnabled(type) is false);
}
