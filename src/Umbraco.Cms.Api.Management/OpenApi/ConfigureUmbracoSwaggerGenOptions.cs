// using Microsoft.AspNetCore.Mvc.ApiExplorer;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Options;
// using Microsoft.OpenApi.Models;
// using Swashbuckle.AspNetCore.SwaggerGen;
// using Umbraco.Cms.Api.Common.OpenApi;
// using Umbraco.Cms.Api.Common.Serialization;
// using Umbraco.Cms.Api.Management.Controllers.Security;
// using Umbraco.Cms.Api.Management.DependencyInjection;
// using Umbraco.Cms.Infrastructure.Serialization;
// using Umbraco.Extensions;
//
// namespace Umbraco.Cms.Api.Management.OpenApi;
//
// internal sealed class ConfigureUmbracoSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
// {
//     private readonly IUmbracoJsonTypeInfoResolver _umbracoJsonTypeInfoResolver;
//
//     public ConfigureUmbracoSwaggerGenOptions(IUmbracoJsonTypeInfoResolver umbracoJsonTypeInfoResolver)
//     {
//         _umbracoJsonTypeInfoResolver = umbracoJsonTypeInfoResolver;
//     }
//
//     public void Configure(SwaggerGenOptions swaggerGenOptions)
//     {
//         swaggerGenOptions.SwaggerDoc(
//             ManagementApiConfiguration.DefaultApiDocumentName,
//             new OpenApiInfo
//             {
//                 Title = ManagementApiConfiguration.ApiTitle,
//                 Version = ManagementApiConfiguration.DefaultApiVersion.ToString(),
//                 Description =
//                     "This shows all APIs available in this version of Umbraco - including all the legacy apis that are available for backward compatibility"
//             });
//
//
//     }
//
//
// }
