using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.OpenApi.SwaggerUI;

/// <summary>
/// Serves the OpenAPI documents with Swagger UI, unless another OpenAPI UI has been chosen.
/// </summary>
/// <remarks>
/// Swagger UI is the UI installed by default with Umbraco, so it gives way to any other OpenAPI UI that is installed
/// or registered with <c>AddOpenApiUi&lt;TProvider&gt;()</c>.
/// </remarks>
public sealed class SwaggerUiComposer : IComposer
{
    /// <inheritdoc />
    public void Compose(IUmbracoBuilder builder) => builder.TryAddSwaggerUiOpenApiUi();
}
