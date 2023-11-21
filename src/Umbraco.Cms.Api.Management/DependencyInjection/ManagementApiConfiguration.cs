using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class ManagementApiConfiguration
{
    internal const string ApiSecurityName = "Backoffice User";
    internal const string ApiTitle = "Umbraco Management API";

    public const string ApiName = "management";
}
