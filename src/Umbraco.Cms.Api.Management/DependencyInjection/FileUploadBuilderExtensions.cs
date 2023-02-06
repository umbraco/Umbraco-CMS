using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class FileUploadBuilderExtensions
{
    internal static IUmbracoBuilder AddFileUpload(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IUploadFileService, UploadFileService>();

        return builder;
    }
}
