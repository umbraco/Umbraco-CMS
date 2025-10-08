using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class TreeBuilderExtensions
{
    internal static IUmbracoBuilder AddTrees(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IUserStartNodeEntitiesService, UserStartNodeEntitiesService>();

        builder.Services.AddUnique<IPartialViewTreeService, PartialViewTreeService>();
        builder.Services.AddUnique<IScriptTreeService, ScriptTreeService>();
        builder.Services.AddUnique<IStyleSheetTreeService, StyleSheetTreeService>();

        builder.Services.AddUnique<IPhysicalFileSystemTreeService, PhysicalFileSystemTreeService>();

        return builder;
    }
}
