using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Fluent builder for configuring a custom OpenAPI document.
/// </summary>
public sealed class BackOfficeOpenApiDocumentBuilder
{
    private readonly List<Action<OpenApiOptions>> _configurations = [];

    private string? _title;
    private string? _uiTitle;
    private bool _includedInUi = true;
    private Func<IServiceProvider, JsonOptions>? _httpJsonOptionsFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeOpenApiDocumentBuilder"/> class.
    /// </summary>
    /// <param name="documentName">The name of the OpenAPI document being configured.</param>
    internal BackOfficeOpenApiDocumentBuilder(string documentName)
        => DocumentName = documentName;

    /// <summary>
    /// Gets the name of the OpenAPI document being configured.
    /// </summary>
    public string DocumentName { get; }

    /// <summary>
    /// Sets the document's <c>Info.Title</c>. Also used as the UI dropdown label unless overridden via
    /// <see cref="WithUiTitle"/>.
    /// </summary>
    /// <param name="title">The title to display.</param>
    /// <returns>The same builder for chaining.</returns>
    public BackOfficeOpenApiDocumentBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Overrides the UI dropdown label for this document.
    /// </summary>
    /// <param name="uiTitle">The label to display.</param>
    /// <returns>The same builder for chaining.</returns>
    public BackOfficeOpenApiDocumentBuilder WithUiTitle(string uiTitle)
    {
        _uiTitle = uiTitle;
        return this;
    }

    /// <summary>
    /// Excludes this document from the UI dropdown.
    /// </summary>
    /// <returns>The same builder for chaining.</returns>
    public BackOfficeOpenApiDocumentBuilder ExcludeFromUi()
    {
        _includedInUi = false;
        return this;
    }

    /// <summary>
    /// Adds an <see cref="OpenApiOptions"/> configuration callback. Multiple calls compose.
    /// </summary>
    /// <param name="configure">Callback to configure the options.</param>
    /// <returns>The same builder for chaining.</returns>
    public BackOfficeOpenApiDocumentBuilder ConfigureOpenApiOptions(Action<OpenApiOptions> configure)
    {
        _configurations.Add(configure);
        return this;
    }

    /// <summary>
    /// Sets the named <see cref="JsonOptions">Microsoft.AspNetCore.Http.Json.JsonOptions</see> used when
    /// generating this document's schema. Use this to match the serialization conventions of the API
    /// endpoints the document describes.
    /// </summary>
    /// <param name="jsonOptionsName">The name of the registered HTTP <see cref="JsonOptions"/> to apply.</param>
    /// <returns>The same builder for chaining.</returns>
    public BackOfficeOpenApiDocumentBuilder WithJsonOptions(string jsonOptionsName)
        => WithJsonOptions(sp => sp.GetRequiredService<IOptionsMonitor<JsonOptions>>().Get(jsonOptionsName));

    /// <summary>
    /// Sets the <see cref="JsonOptions">Microsoft.AspNetCore.Http.Json.JsonOptions</see> used when
    /// generating this document's schema. Use this to match the serialization conventions of the API
    /// endpoints the document describes.
    /// </summary>
    /// <param name="jsonOptions">The HTTP JSON options to apply.</param>
    /// <returns>The same builder for chaining.</returns>
    public BackOfficeOpenApiDocumentBuilder WithJsonOptions(JsonOptions jsonOptions)
        => WithJsonOptions(_ => jsonOptions);

    /// <summary>
    /// Sets a factory that produces the <see cref="JsonOptions">Microsoft.AspNetCore.Http.Json.JsonOptions</see>
    /// used when generating this document's schema. Use this to match the serialization conventions of the
    /// API endpoints the document describes.
    /// </summary>
    /// <param name="jsonOptionsFactory">Factory invoked when the schema service is first resolved.</param>
    /// <returns>The same builder for chaining.</returns>
    public BackOfficeOpenApiDocumentBuilder WithJsonOptions(Func<IServiceProvider, JsonOptions> jsonOptionsFactory)
    {
        _httpJsonOptionsFactory = jsonOptionsFactory;
        return this;
    }

    /// <summary>
    /// Applies the accumulated configuration to the supplied <see cref="IUmbracoBuilder"/>'s service
    /// collection. Called by <c>AddBackOfficeOpenApiDocument</c> once the user-supplied callback returns.
    /// </summary>
    /// <param name="builder">The Umbraco builder to register services against.</param>
    internal void Build(IUmbracoBuilder builder)
    {
        builder.Services.AddOpenApi(
            DocumentName,
            options =>
            {
                options.ShouldInclude = apiDescription =>
                    apiDescription.ActionDescriptor.HasMapToApiAttribute(DocumentName);

                options.CreateSchemaReferenceId = UmbracoSchemaIdGenerator.CreateSchemaReferenceId;

                if (_title is not null)
                {
                    options.AddDocumentTransformer((document, _, _) =>
                    {
                        document.Info.Title = _title;
                        return Task.CompletedTask;
                    });
                }

                // Generate operation IDs using Umbraco's naming conventions.
                options.AddOperationTransformer<UmbracoOperationIdTransformer>();

                // Tag actions by group name and cleanup unused tags (caused by the tag changes).
                options
                    .AddOperationTransformer<TagActionsByGroupNameTransformer>()
                    .AddDocumentTransformer<TagActionsByGroupNameTransformer>()
                    .AddDocumentTransformer<SortTagsAndPathsTransformer>();

                foreach (Action<OpenApiOptions> configure in _configurations)
                {
                    configure(options);
                }
            });

        if (_includedInUi)
        {
            builder.Services.AddOpenApiDocumentToUi(DocumentName, _uiTitle ?? _title);
        }

        if (_httpJsonOptionsFactory is not null)
        {
            builder.Services.ReplaceOpenApiSchemaService(DocumentName, _httpJsonOptionsFactory);
        }
    }
}
