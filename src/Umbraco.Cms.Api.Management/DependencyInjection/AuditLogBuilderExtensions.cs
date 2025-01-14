using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class AuditLogBuilderExtensions
{
    internal static IUmbracoBuilder AddAuditLogs(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IAuditLogPresentationFactory, AuditLogPresentationFactory>();

        return builder;
    }
}
