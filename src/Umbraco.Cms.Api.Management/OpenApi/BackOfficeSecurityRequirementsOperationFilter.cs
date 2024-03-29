using Umbraco.Cms.Api.Management.DependencyInjection;

namespace Umbraco.Cms.Api.Management.OpenApi;

internal class BackOfficeSecurityRequirementsOperationFilter : BackOfficeSecurityRequirementsOperationFilterBase
{
    protected override string ApiName => ManagementApiConfiguration.ApiName;
}
