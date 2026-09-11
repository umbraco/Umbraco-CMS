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
            Assert.That(responseModel.IsApproved, Is.False);
            Assert.That(responseModel.IsLockedOut, Is.False);
            Assert.That(responseModel.IsTwoFactorEnabled, Is.False);
            Assert.That(responseModel.FailedPasswordAttempts, Is.EqualTo(0));
            Assert.That(responseModel.LastLoginDate, Is.Null);
            Assert.That(responseModel.LastLockoutDate, Is.Null);
            Assert.That(responseModel.LastPasswordChangeDate, Is.Null);
        });
    }

    [Test]
    public void Can_See_Account_State_With_Sensitive_Data_Access()
    {
        MemberResponseModel responseModel = CreateResponseModel();

        responseModel.ClearSensitiveValuesFor(CreateUser(hasAccessToSensitiveData: true));

        Assert.Multiple(() =>
        {
            Assert.That(responseModel.IsApproved, Is.True);
            Assert.That(responseModel.IsLockedOut, Is.True);
            Assert.That(responseModel.IsTwoFactorEnabled, Is.True);
            Assert.That(responseModel.FailedPasswordAttempts, Is.EqualTo(5));
            Assert.That(responseModel.LastLoginDate, Is.EqualTo(_lastLoginDate));
            Assert.That(responseModel.LastLockoutDate, Is.EqualTo(_lastLockoutDate));
            Assert.That(responseModel.LastPasswordChangeDate, Is.EqualTo(_lastPasswordChangeDate));
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
            Assert.That(responseModel.Id, Is.EqualTo(id));
            Assert.That(responseModel.Email, Is.EqualTo("member@umbraco.com"));
            Assert.That(responseModel.Username, Is.EqualTo("member"));
            Assert.That(responseModel.Kind, Is.EqualTo(MemberKind.Default));
            Assert.That(responseModel.ProfileData, Is.EqualTo("{ \"claim\": \"value\" }"));
            Assert.That(responseModel.Groups.Single(), Is.EqualTo(groupKey));
            Assert.That(responseModel.Values.Single().Alias, Is.EqualTo("title"));
            Assert.That(responseModel.Variants.Single().Name, Is.EqualTo("Test Member"));
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
