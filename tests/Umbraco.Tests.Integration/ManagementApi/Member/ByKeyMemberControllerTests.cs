using System.Linq.Expressions;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Member;
using Umbraco.Cms.Api.Management.ViewModels.Member;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Member;

public class ByKeyMemberControllerTests : MemberSensitiveDataTestBase<ByKeyMemberController>
{
    protected override Expression<Func<ByKeyMemberController, object>> BuildMethodSelector() =>
        x => x.ByKey(CancellationToken.None, MemberKey);

    [Test]
    public async Task Can_See_Login_And_Lockout_State_With_Sensitive_Data_Access()
    {
        await AuthenticateAdministratorWithSensitiveDataAccessAsync();

        MemberResponseModel model = await GetAsync<MemberResponseModel>(Url);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(ExpectedFailedPasswordAttempts, model.FailedPasswordAttempts);
            Assert.IsTrue(model.IsLockedOut);
            Assert.AreEqual(ExpectedLastLockoutDate, model.LastLockoutDate?.UtcDateTime);
            Assert.AreEqual(ExpectedLastLoginDate, model.LastLoginDate?.UtcDateTime);
        });
    }

    [Test]
    public async Task Cannot_See_Login_And_Lockout_State_Without_Sensitive_Data_Access()
    {
        await AuthenticateAdministratorAsync();

        MemberResponseModel model = await GetAsync<MemberResponseModel>(Url);

        AssertSensitiveValuesAreWithheld(model);
    }
}
