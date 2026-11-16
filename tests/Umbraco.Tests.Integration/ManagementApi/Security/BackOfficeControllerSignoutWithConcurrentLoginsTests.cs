using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

/// <summary>
/// Runs the <see cref="BackOfficeControllerSignoutTests"/> with concurrent logins allowed, where the security stamp of
/// the backoffice cookie is only validated once per validation interval.
/// </summary>
public class BackOfficeControllerSignoutWithConcurrentLoginsTests : BackOfficeControllerSignoutTests
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.Services.Configure<SecuritySettings>(x => x.UserAllowConcurrentLogins = true);
    }

    /// <summary>
    /// Verifies the premise of this fixture. Without a validation interval the replayed cookie is rejected before it
    /// reaches the authorize endpoint, and the inherited tests would not cover the endpoint's own security stamp check.
    /// </summary>
    [Test]
    public void Security_Stamp_Validation_Is_Deferred()
        => Assert.Greater(GetRequiredService<IOptions<BackOfficeSecurityStampValidatorOptions>>().Value.ValidationInterval, TimeSpan.Zero);
}
