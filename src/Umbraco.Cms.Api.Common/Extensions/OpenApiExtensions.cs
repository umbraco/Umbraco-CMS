using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods for configuring OpenAPI options for Umbraco APIs.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Configures the default settings and transformers for all Umbraco APIs.
    /// </summary>
    /// <param name="options">The <see cref="OpenApiOptions"/> instance to configure.</param>
    /// <param name="name">The name of the API being configured.</param>
    /// <returns>The configured <see cref="OpenApiOptions"/> instance.</returns>
    public static OpenApiOptions ConfigureUmbracoDefaultApiOptions(this OpenApiOptions options, string name)
    {
        options.ShouldInclude = apiDescription => ShouldInclude(apiDescription, name);

        options.AddDocumentTransformer((document, _, _) =>
        {
            document.SortTagsByName();
            document.SortPaths();
            document.CleanupUnusedTags();
            return Task.CompletedTask;
        });

        options.AddOperationTransformer((operation, context, _) =>
        {
            IOperationIdSelector operationIdSelector =
                context.ApplicationServices.GetRequiredService<IOperationIdSelector>();
            operation.OperationId = operationIdSelector.OperationId(context.Description);
            operation.TagActionsByGroupName(context);
            return Task.CompletedTask;
        });

        options.AddSchemaTransformer((schema, context, _) =>
        {
            ISchemaIdSelector schemaIdSelector =
                context.ApplicationServices.GetRequiredService<ISchemaIdSelector>();
            schema.Id = schemaIdSelector.SchemaId(context.JsonTypeInfo.Type);
            return Task.CompletedTask;
        });

        return options;
    }

    private static bool ShouldInclude(this ApiDescription apiDescription, string apiName)
    {
        if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor && controllerActionDescriptor.HasMapToApiAttribute(apiName))
        {
            return true;
        }

        ApiVersionMetadata apiVersionMetadata = apiDescription.ActionDescriptor.GetApiVersionMetadata();
        return apiVersionMetadata.Name == apiName
               || (string.IsNullOrEmpty(apiVersionMetadata.Name) && apiName == DefaultApiConfiguration.ApiName);
    }

    private static void TagActionsByGroupName(this OpenApiOperation operation, OpenApiOperationTransformerContext context)
    {
        if (context.Document is null || context.Description.GroupName is not { } groupName)
        {
            return;
        }

        operation.Tags = new HashSet<OpenApiTagReference> { new(groupName) };
        if (context.Document.Tags?.Any(t => t.Name == groupName) == true)
        {
            return;
        }

        context.Document.Tags ??= new HashSet<OpenApiTag>();
        context.Document.Tags.Add(new OpenApiTag { Name = groupName });
    }

    private static void SortTagsByName(this OpenApiDocument document)
        => document.Tags = new SortedSet<OpenApiTag>(
            document.Tags ?? Enumerable.Empty<OpenApiTag>(),
            Comparer<OpenApiTag>.Create((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal)));

    private static void SortPaths(this OpenApiDocument document)
    {
        var sortedPaths = new OpenApiPaths();
        foreach (KeyValuePair<string, IOpenApiPathItem> keyValuePair in document.Paths
                     .OrderBy(x => x.Value.Operations?.FirstOrDefault().Value?.Tags?.FirstOrDefault()?.Name)
                     .ThenBy(x => x.Key))
        {
            sortedPaths.Add(keyValuePair.Key, keyValuePair.Value);
        }

        document.Paths = sortedPaths;
    }

    private static void CleanupUnusedTags(this OpenApiDocument document)
    {
        var usedTags = new HashSet<string?>(document.Paths
            .SelectMany(p => (p.Value.Operations ?? []).Values)
            .SelectMany(o => o.Tags ?? new HashSet<OpenApiTagReference>())
            .Select(t => t.Name));

        foreach (OpenApiTag tag in document.Tags ?? Enumerable.Empty<OpenApiTag>())
        {
            if (usedTags.Contains(tag.Name))
            {
                continue;
            }

            document.Tags?.Remove(tag);
        }
    }
}
