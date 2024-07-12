using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.Filters;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.WIP;

public class PackageConstants
{
    public const string MyPackageContextIdentifier = "MyPackageContextIdentifier";
    public const string MyPermissionVerb = "MyPermissionVerb";
}

[ApiController]
[ApiVersion("1.0")]
[MapToApi("my-api-v1")]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
[JsonOptionsName(Constants.JsonOptionsNames.BackOffice)]
[Route("api/v{version:apiVersion}/my")]
public class TestController : ManagementApiControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public TestController(IUserGroupService userGroupService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userGroupService = userGroupService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [ContextualAuthorize(PackageConstants.MyPermissionVerb, PackageConstants.MyPackageContextIdentifier)]
    [MapToApiVersion("1.0")]
    [HttpGet]
    public async Task<IActionResult> ProtectedAction()
    {
        return Ok("success value");
    }

    [ContextualAuthorize(PackageConstants.MyPermissionVerb+"nonsense", PackageConstants.MyPackageContextIdentifier)]
    [MapToApiVersion("1.0")]
    [HttpGet("/other")]
    public async Task<IActionResult> InvalidProtectedAction()
    {
        return Ok("success value");
    }

    [MapToApiVersion("1.0")]
    [HttpPost]
    public async Task<IActionResult> SetValidPermissionOnAdminUserGroup()
    {
        var admins = await _userGroupService.GetAsync("admin");
        admins!.GranularPermissions.Add(new UnknownTypeGranularPermission
        {
            Context = PackageConstants.MyPackageContextIdentifier, Permission = PackageConstants.MyPermissionVerb
        });
        await _userGroupService.UpdateAsync(admins, _backOfficeSecurityAccessor.BackOfficeSecurity!.CurrentUser!.Key);
        return Ok();
    }
}


// Included as setup, but is already covered in the docs
public class MyBackOfficeSecurityRequirementsOperationFilter : BackOfficeSecurityRequirementsOperationFilterBase
{
    protected override string ApiName => "my-api-v1";
}

public class MyConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc("my-api-v1", new OpenApiInfo { Title = "My API v1", Version = "1.0" });
        options.OperationFilter<MyBackOfficeSecurityRequirementsOperationFilter>();
    }
}

public class MySwaggerComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder.Services.ConfigureOptions<MyConfigureSwaggerGenOptions>();
}
