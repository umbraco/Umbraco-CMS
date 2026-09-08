using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Controllers.Member.Filter;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Member.Filter;

public class FilterMemberFilterControllerTests : MemberSensitiveDataTestBase<FilterMemberFilterController>
{
    protected override Expression<Func<FilterMemberFilterController, object>> BuildMethodSelector() =>
        x => x.Filter(CancellationToken.None, null, null, null, null, "username", Direction.Ascending, null, 0, 100);

    private string LockedOutFilterUrl => GetManagementApiUrl<FilterMemberFilterController>(
        x => x.Filter(CancellationToken.None, null, null, null, true, "username", Direction.Ascending, null, 0, 100));

    private string ApprovedFilterUrl => GetManagementApiUrl<FilterMemberFilterController>(
        x => x.Filter(CancellationToken.None, null, null, true, null, "username", Direction.Ascending, null, 0, 100));

    [Test]
    public async Task Can_See_Login_And_Lockout_State_With_Sensitive_Data_Access()
    {
        await AuthenticateAdministratorWithSensitiveDataAccessAsync();

        MemberResponseModel model = await GetMemberFromCollectionAsync(Url);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(model.IsLockedOut);
            Assert.AreEqual(ExpectedLastLockoutDate, model.LastLockoutDate?.UtcDateTime);
            Assert.AreEqual(ExpectedLastLoginDate, model.LastLoginDate?.UtcDateTime);
        });
    }

    [Test]
    public async Task Cannot_See_Login_And_Lockout_State_Without_Sensitive_Data_Access()
    {
        await AuthenticateAdministratorAsync();

        MemberResponseModel model = await GetMemberFromCollectionAsync(Url);

        AssertSensitiveValuesAreWithheld(model);
    }

    [Test]
    public async Task Can_Filter_By_Lockout_State_With_Sensitive_Data_Access()
    {
        await AuthenticateAdministratorWithSensitiveDataAccessAsync();

        PagedViewModel<MemberResponseModel> collection =
            await GetAsync<PagedViewModel<MemberResponseModel>>(LockedOutFilterUrl);

        Assert.IsNotNull(collection.Items.SingleOrDefault(x => x.Id == MemberKey));
    }

    [Test]
    public async Task Cannot_Filter_By_Lockout_State_Without_Sensitive_Data_Access()
    {
        await AuthenticateAdministratorAsync();

        HttpResponseMessage response = await Client.GetAsync(LockedOutFilterUrl);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Cannot_Filter_By_Approval_State_Without_Sensitive_Data_Access()
    {
        await AuthenticateAdministratorAsync();

        HttpResponseMessage response = await Client.GetAsync(ApprovedFilterUrl);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private async Task<MemberResponseModel> GetMemberFromCollectionAsync(string url)
    {
        PagedViewModel<MemberResponseModel> collection = await GetAsync<PagedViewModel<MemberResponseModel>>(url);
        MemberResponseModel? model = collection.Items.SingleOrDefault(x => x.Id == MemberKey);
        Assert.IsNotNull(model);
        return model!;
    }
}
