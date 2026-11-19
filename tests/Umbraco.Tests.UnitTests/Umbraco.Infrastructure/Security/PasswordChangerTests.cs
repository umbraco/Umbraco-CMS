using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Security;

[TestFixture]
public class PasswordChangerTests
{
    // Several ICU cultures (for example sv-SE, nb-NO, fi-FI) use U+2212 MINUS SIGN as the negative sign,
    // whereas identity ids are always formatted with the invariant culture.
    [Test]
    public async Task ChangePasswordWithIdentityAsync_Resolves_User_By_Invariant_Id_Under_Culture_With_Non_Ascii_Negative_Sign()
    {
        const string invariantUserId = "-1";
        var identityUser = new BackOfficeIdentityUser(new GlobalSettings(), Constants.Security.SuperUserId, Array.Empty<IReadOnlyUserGroup>());

        var userManager = new Mock<IUmbracoUserManager<BackOfficeIdentityUser>>();
        userManager
            .Setup(x => x.FindByIdAsync(invariantUserId))
            .ReturnsAsync(identityUser);
        userManager
            .Setup(x => x.GeneratePasswordResetTokenAsync(identityUser))
            .ReturnsAsync("token");
        userManager
            .Setup(x => x.ChangePasswordWithResetAsync(invariantUserId, "token", "newPassword"))
            .ReturnsAsync(IdentityResult.Success);

        var sut = new PasswordChanger<BackOfficeIdentityUser>(NullLogger<PasswordChanger<BackOfficeIdentityUser>>.Instance);

        CultureInfo savedCulture = CultureInfo.CurrentCulture;
        Attempt<PasswordChangedModel?> result;
        try
        {
            var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.NumberFormat.NegativeSign = "−";
            CultureInfo.CurrentCulture = culture;

            result = await sut.ChangePasswordWithIdentityAsync(
                new ChangingPasswordModel { Id = Constants.Security.SuperUserId, NewPassword = "newPassword" },
                userManager.Object,
                null);
        }
        finally
        {
            CultureInfo.CurrentCulture = savedCulture;
        }

        Assert.IsTrue(result.Success, result.Result?.Error?.ErrorMessage);
        userManager.Verify(x => x.ChangePasswordWithResetAsync(invariantUserId, "token", "newPassword"), Times.Once);
    }
}
