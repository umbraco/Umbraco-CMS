// using Microsoft.AspNetCore.Mvc.ApiExplorer;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.OpenApi.Models;
// using Umbraco.Cms.Api.Common.Serialization;
// using Umbraco.Cms.Api.Management.Controllers.Dictionary;
// using Umbraco.Cms.Api.Management.Controllers.Security;
// using Umbraco.Cms.Api.Management.OpenApi;
// using Umbraco.Cms.Core.DependencyInjection;
// using Umbraco.Cms.Infrastructure.Serialization;
// using Umbraco.Extensions;
//
// namespace Umbraco.Cms.Api.Management.DependencyInjection;
//
// internal static class ManagementApiBuilderExtensions
// {
//     internal static IUmbracoBuilder AddSwaggerGen(this IUmbracoBuilder builder)
//     {
//         builder.Services.AddSwaggerGen();
//         builder.Services.ConfigureOptions<ConfigureUmbracoSwaggerGenOptions>();
//         builder.Services.AddSingleton<IUmbracoJsonTypeInfoResolver, UmbracoJsonTypeInfoResolver>();
//
//
//         return builder;
//     }
// }
