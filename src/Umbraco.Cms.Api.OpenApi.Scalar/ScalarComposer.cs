using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.OpenApi.Scalar;

/// <summary>
/// Serves the OpenAPI documents with the Scalar API reference, in place of the default OpenAPI UI.
/// </summary>
public sealed class ScalarComposer : IComposer
{
    /// <inheritdoc />
    public void Compose(IUmbracoBuilder builder) => builder.AddScalarOpenApiUi();
}
