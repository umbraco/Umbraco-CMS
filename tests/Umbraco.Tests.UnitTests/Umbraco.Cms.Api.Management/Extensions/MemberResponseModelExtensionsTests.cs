using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Extensions;

[TestFixture]
public class MemberResponseModelExtensionsTests
{
    private static readonly DateTimeOffset _lastLoginDate = new(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset _lastLockoutDate = new(2026, 8, 2, 11, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset _lastPasswordChangeDate = new(2026, 8, 3, 12, 0, 0, TimeSpan.Zero);

    [Test]
    public void Cannot_See_Account_State_Without_Sensitive_Data_Access()
    {
        MemberResponseModel responseModel = CreateResponseModel();

        responseModel.ClearSensitiveValuesFor(CreateUser(hasAccessToSensitiveData: false));

        Assert.Multiple(() =>
        {
            Assert.IsFalse(responseModel.IsApproved);
            Assert.IsFalse(responseModel.IsLockedOut);
            Assert.IsFalse(responseModel.IsTwoFactorEnabled);
            Assert.AreEqual(0, responseModel.FailedPasswordAttempts);
            Assert.IsNull(responseModel.LastLoginDate);
            Assert.IsNull(responseModel.LastLockoutDate);
            Assert.IsNull(responseModel.LastPasswordChangeDate);
        });
    }

    [Test]
    public void Can_See_Account_State_With_Sensitive_Data_Access()
    {
        MemberResponseModel responseModel = CreateResponseModel();

        responseModel.ClearSensitiveValuesFor(CreateUser(hasAccessToSensitiveData: true));

        Assert.Multiple(() =>
        {
            Assert.IsTrue(responseModel.IsApproved);
            Assert.IsTrue(responseModel.IsLockedOut);
            Assert.IsTrue(responseModel.IsTwoFactorEnabled);
            Assert.AreEqual(5, responseModel.FailedPasswordAttempts);
            Assert.AreEqual(_lastLoginDate, responseModel.LastLoginDate);
            Assert.AreEqual(_lastLockoutDate, responseModel.LastLockoutDate);
            Assert.AreEqual(_lastPasswordChangeDate, responseModel.LastPasswordChangeDate);
        });
    }

    // Only account state is gated by sensitive data access. Everything else the endpoint chose to return -
    // including property values, which are filtered by their own sensitivity rules - must survive untouched.
    [Test]
    public void Can_See_Everything_Other_Than_Account_State_Without_Sensitive_Data_Access()
    {
        MemberResponseModel responseModel = CreateResponseModel();
        Guid id = responseModel.Id;
        Guid groupKey = responseModel.Groups.Single();

        responseModel.ClearSensitiveValuesFor(CreateUser(hasAccessToSensitiveData: false));

        Assert.Multiple(() =>
        {
            Assert.AreEqual(id, responseModel.Id);
            Assert.AreEqual("member@umbraco.com", responseModel.Email);
            Assert.AreEqual("member", responseModel.Username);
            Assert.AreEqual(MemberKind.Default, responseModel.Kind);
            Assert.AreEqual("{ \"claim\": \"value\" }", responseModel.ProfileData);
            Assert.AreEqual(groupKey, responseModel.Groups.Single());
            Assert.AreEqual("title", responseModel.Values.Single().Alias);
            Assert.AreEqual("Test Member", responseModel.Variants.Single().Name);
        });
    }

    private static MemberResponseModel CreateResponseModel() =>
        new()
        {
            Id = Guid.NewGuid(),
            Email = "member@umbraco.com",
            Username = "member",
            MemberType = new MemberTypeReferenceResponseModel(),
            Kind = MemberKind.Default,
            ProfileData = "{ \"claim\": \"value\" }",
            Groups = [Guid.NewGuid()],
            Values = [new MemberValueResponseModel { Alias = "title", Value = "The title value" }],
            Variants = [new MemberVariantResponseModel { Name = "Test Member" }],
            IsApproved = true,
            IsLockedOut = true,
            IsTwoFactorEnabled = true,
            FailedPasswordAttempts = 5,
            LastLoginDate = _lastLoginDate,
            LastLockoutDate = _lastLockoutDate,
            LastPasswordChangeDate = _lastPasswordChangeDate,
        };

    private static IUser CreateUser(bool hasAccessToSensitiveData)
    {
        var groups = new List<IReadOnlyUserGroup>();
        if (hasAccessToSensitiveData)
        {
            var group = new Mock<IReadOnlyUserGroup>();
            group.Setup(x => x.Key).Returns(Constants.Security.SensitiveDataGroupKey);
            groups.Add(group.Object);
        }

        var user = new Mock<IUser>();
        user.Setup(x => x.Groups).Returns(groups);
        return user.Object;
    }
}
