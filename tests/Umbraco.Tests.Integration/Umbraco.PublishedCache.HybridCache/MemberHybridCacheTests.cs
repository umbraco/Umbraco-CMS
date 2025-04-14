using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MemberHybridCacheTests : UmbracoIntegrationTest
{
    private IPublishedMemberCache PublishedMemberHybridCache => GetRequiredService<IPublishedMemberCache>();

    private IMemberEditingService MemberEditingService => GetRequiredService<IMemberEditingService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberGroupService MemberGroupService => GetRequiredService<IMemberGroupService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    [Test]
    public async Task Can_Get_Member_By_Key()
    {
        Guid key = Guid.NewGuid();
        var createdMember = await CreateMemberAsync(key);

        // Act
        var member = await PublishedMemberHybridCache.GetAsync(createdMember);

        // Assert
        Assert.IsNotNull(member);
        Assert.AreEqual("The title value", member.Value("title"));
        Assert.AreEqual("test@test.com", member.Email);
        Assert.AreEqual("test", member.UserName);
        Assert.IsTrue(member.IsApproved);
        Assert.AreEqual("T. Est", member.Name);
    }

    private async Task<IMember> CreateMemberAsync(Guid? key = null, bool titleIsSensitive = false)
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        memberType.SetIsSensitiveProperty("title", titleIsSensitive);
        MemberTypeService.Save(memberType);
        MemberService.AddRole("RoleOne");
        var group = MemberGroupService.GetByName("RoleOne");

        var createModel = new MemberCreateModel
        {
            Key = key,
            Email = "test@test.com",
            Username = "test",
            Password = "SuperSecret123",
            IsApproved = true,
            ContentTypeKey = memberType.Key,
            Roles = new [] { group.Key },
            InvariantName = "T. Est",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "author", Value = "The author value" }
            }
        };

        var result = await MemberEditingService.CreateAsync(createModel, SuperUser());
        Assert.IsTrue(result.Success);
        return result.Result.Content;
    }

    private IUser SuperUser() => GetRequiredService<IUserService>().GetAsync(Constants.Security.SuperUserKey).GetAwaiter().GetResult();

}
