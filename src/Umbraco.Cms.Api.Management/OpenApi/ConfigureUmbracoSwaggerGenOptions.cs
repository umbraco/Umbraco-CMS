using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.OpenApi;

internal sealed class ConfigureUmbracoSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IUmbracoJsonTypeInfoResolver _umbracoJsonTypeInfoResolver;

    public ConfigureUmbracoSwaggerGenOptions(IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver)
    {
        _umbracoJsonTypeInfoResolver = umbracoJsonTypeInfoResolver;
    }

    public void Configure(SwaggerGenOptions swaggerGenOptions)
    {
        swaggerGenOptions.SwaggerDoc(
            ManagementApiConfiguration.DefaultApiDocumentName,
            new OpenApiInfo
            {
                Title = ManagementApiConfiguration.ApiTitle,
                Version = ManagementApiConfiguration.DefaultApiVersion.ToString(),
                Description =
                    "This shows all APIs available in this version of Umbraco - including all the legacy apis that are available for backward compatibility"
            });

        swaggerGenOptions.AddSecurityDefinition(
            "OAuth",
            new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Umbraco",
                Type = SecuritySchemeType.OAuth2,
                Description = "Umbraco Authentication",
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl =
                            new Uri(Paths.BackOfficeApiAuthorizationEndpoint, UriKind.Relative),
                        TokenUrl = new Uri(Paths.BackOfficeApiTokenEndpoint, UriKind.Relative)
                    }
                }
            });

        swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            // this weird looking construct works because OpenApiSecurityRequirement
            // is a specialization of Dictionary<,>
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Id = "OAuth", Type = ReferenceType.SecurityScheme }
                },
                new List<string>()
            }
        });

        swaggerGenOptions.CustomOperationIds(CustomOperationId);
        swaggerGenOptions.DocInclusionPredicate((_, api) => !string.IsNullOrWhiteSpace(api.GroupName));
        swaggerGenOptions.TagActionsBy(api => new[] { api.GroupName });
        swaggerGenOptions.OrderActionsBy(ActionOrderBy);
        swaggerGenOptions.DocumentFilter<MimeTypeDocumentFilter>();
        swaggerGenOptions.SchemaFilter<EnumSchemaFilter>();
        swaggerGenOptions.CustomSchemaIds(SchemaIdGenerator.Generate);
        swaggerGenOptions.SupportNonNullableReferenceTypes();

        swaggerGenOptions.OperationFilter<ReponseHeaderOperationFilter>();
        swaggerGenOptions.UseOneOfForPolymorphism();
        swaggerGenOptions.UseAllOfForInheritance();
        swaggerGenOptions.SelectSubTypesUsing(_umbracoJsonTypeInfoResolver.FindSubTypes);

        swaggerGenOptions.SelectDiscriminatorNameUsing(type =>
        {
            if (type.GetInterfaces().Any())
            {
                return "$type";
            }

            return null;
        });
        swaggerGenOptions.SelectDiscriminatorValueUsing(x => x.Name);
    }

    private static string CustomOperationId(ApiDescription api)
    {
        var httpMethod = api.HttpMethod?.ToLower().ToFirstUpper() ?? "Get";

        // if the route info "Name" is supplied we'll use this explicitly as the operation ID
        // - usage example: [HttpGet("my-api/route}", Name = "MyCustomRoute")]
        if (string.IsNullOrWhiteSpace(api.ActionDescriptor.AttributeRouteInfo?.Name) == false)
        {
            var explicitOperationId = api.ActionDescriptor.AttributeRouteInfo!.Name;
            return explicitOperationId.InvariantStartsWith(httpMethod)
                ? explicitOperationId
                : $"{httpMethod}{explicitOperationId}";
        }

        var relativePath = api.RelativePath;

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new Exception(
                $"There is no relative path for controller action {api.ActionDescriptor.RouteValues["controller"]}");
        }

        // Remove the prefixed base path with version, e.g. /umbraco/management/api/v1/tracked-reference/{id} => tracked-reference/{id}
        var unprefixedRelativePath = OperationIdRegexes
            .VersionPrefixRegex()
            .Replace(relativePath, string.Empty);

        // Remove template placeholders, e.g. tracked-reference/{id} => tracked-reference/Id
        var formattedOperationId = OperationIdRegexes
            .TemplatePlaceholdersRegex()
            .Replace(unprefixedRelativePath, m => $"By{m.Groups[1].Value.ToFirstUpper()}");

        // Remove dashes (-) and slashes (/) and convert the following letter to uppercase with
        // the word "By" in front, e.g. tracked-reference/Id => TrackedReferenceById
        formattedOperationId = OperationIdRegexes
            .ToCamelCaseRegex()
            .Replace(formattedOperationId, m => m.Groups[1].Value.ToUpper());

        // Return the operation ID with the formatted http method verb in front, e.g. GetTrackedReferenceById
        return $"{httpMethod}{formattedOperationId.ToFirstUpper()}";
    }

    // see https://github.com/domaindrivendev/Swashbuckle.AspNetCore#change-operation-sort-order-eg-for-ui-sorting
    private static string ActionOrderBy(ApiDescription apiDesc)
        =>
            $"{apiDesc.GroupName}_{apiDesc.ActionDescriptor.AttributeRouteInfo?.Template ?? apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.ActionDescriptor.RouteValues["action"]}_{apiDesc.HttpMethod}";
}
