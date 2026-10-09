using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Member;

/// <summary>
/// Shared setup for the member endpoints that withhold member account state from users without access to
/// sensitive data.
/// </summary>
public abstract class MemberSensitiveDataTestBase<T> : ManagementApiTest<T>
    where T : ManagementApiControllerBase
{
    protected const int ExpectedFailedPasswordAttempts = 5;

    protected static readonly DateTime ExpectedLastLoginDate = new(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc);

    protected static readonly DateTime ExpectedLastLockoutDate = new(2026, 8, 2, 11, 0, 0, DateTimeKind.Utc);

    private IMemberTypeEditingService MemberTypeEditingService => GetRequiredService<IMemberTypeEditingService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    // The endpoints serialize with the back office options, so responses have to be read back with them too.
    private JsonSerializerOptions BackOfficeJsonSerializerOptions => GetRequiredService<IOptionsMonitor<JsonOptions>>()
        .Get(Constants.JsonOptionsNames.BackOffice)
        .JsonSerializerOptions;

    protected Guid MemberKey { get; private set; }

    protected override Expression<Func<T, object>> MethodSelector { get; set; }

    /// <summary>
    /// Builds the selector for the endpoint under test, once <see cref="MemberKey"/> is known.
    /// </summary>
    /// <returns>An expression selecting the controller method the fixture requests.</returns>
    protected abstract Expression<Func<T, object>> BuildMethodSelector();

    [SetUp]
    public async Task SetUpLockedOutMember()
    {
        // The fixture keeps one database for all its tests, so each test seeds its own member type and member.
        var suffix = Guid.NewGuid().ToString("N");

        var memberTypeModel = new MemberTypeCreateModel { Alias = $"memberType{suffix}", Name = "Test Member Type" };
        Attempt<IMemberType?, ContentTypeOperationStatus> memberType =
            await MemberTypeEditingService.CreateAsync(memberTypeModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(memberType.Success, memberType.Status.ToString());

        IMember member = MemberService.CreateMember(
            $"member{suffix}",
            $"member{suffix}@umbraco.com",
            "Test Member",
            memberType.Result!.Alias);
        MemberService.Save(member);

        // Mirror the state a front-end sign-in leaves behind: a successful login, followed by enough failed
        // attempts to lock the member out.
        member.LastLoginDate = ExpectedLastLoginDate;
        member.FailedPasswordAttempts = ExpectedFailedPasswordAttempts;
        member.IsLockedOut = true;
        member.LastLockoutDate = ExpectedLastLockoutDate;
        MemberService.Save(member);

        MemberKey = member.Key;
        MethodSelector = BuildMethodSelector();
    }

    protected Task AuthenticateAdministratorAsync()
        => AuthenticateAsync("administrator", Constants.Security.AdminGroupKey);

    protected Task AuthenticateAdministratorWithSensitiveDataAccessAsync()
        => AuthenticateAsync("sensitive", Constants.Security.AdminGroupKey, Constants.Security.SensitiveDataGroupKey);

    protected async Task<TResponse> GetAsync<TResponse>(string url)
    {
        HttpResponseMessage response = await Client.GetAsync(url);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        TResponse? model = await response.Content.ReadFromJsonAsync<TResponse>(BackOfficeJsonSerializerOptions);
        Assert.IsNotNull(model);
        return model!;
    }

    protected static void AssertSensitiveValuesAreWithheld(MemberResponseModel model) =>
        Assert.Multiple(() =>
        {
            Assert.IsFalse(model.IsApproved);
            Assert.IsFalse(model.IsLockedOut);
            Assert.IsFalse(model.IsTwoFactorEnabled);
            Assert.AreEqual(0, model.FailedPasswordAttempts);
            Assert.IsNull(model.LastLoginDate);
            Assert.IsNull(model.LastLockoutDate);
            Assert.IsNull(model.LastPasswordChangeDate);
        });

    // The shared harness authenticates the built-in super user for the administrators group, and that user is
    // a member of the sensitive data group too. To represent a regular back office user we always create a new
    // user belonging to the requested groups only.
    private Task AuthenticateAsync(string name, params Guid[] userGroupKeys) =>
        AuthenticateClientAsync(
            Client,
            async userService =>
            {
                Attempt<UserCreationResult, UserOperationStatus> result = await userService.CreateAsync(
                    Constants.Security.SuperUserKey,
                    new UserCreateModel
                    {
                        Email = $"{name}@umbraco.com",
                        Name = name,
                        UserName = $"{name}@umbraco.com",
                        UserGroupKeys = new HashSet<Guid>(userGroupKeys),
                    },
                    true);

                return (result.Result.CreatedUser!, UserPassword);
            },
            name);
}
