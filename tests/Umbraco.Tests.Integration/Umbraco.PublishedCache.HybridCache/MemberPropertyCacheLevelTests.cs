using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

public class MemberPropertyCacheLevelTests : PropertyCacheLevelTestsBase
{
    private static readonly Guid _memberKey = new("1ADC9048-E437-460B-95DC-3B8E19239CBD");

    private IMemberCacheService MemberCacheService => GetRequiredService<IMemberCacheService>();

    private IMemberEditingService MemberEditingService => GetRequiredService<IMemberEditingService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    [SetUp]
    public void SetUpTest()
        => PropertyValueLevelDetectionTestsConverter.Reset();

    [TestCase(PropertyCacheLevel.None, 2, 10)]
    [TestCase(PropertyCacheLevel.Element, 2, 2)]
    [TestCase(PropertyCacheLevel.Elements, 2, 10)]
    public async Task Property_Value_Conversion_Respects_Property_Cache_Level(PropertyCacheLevel cacheLevel, int expectedSourceConverts, int expectedInterConverts)
    {
        PropertyValueLevelDetectionTestsConverter.SetCacheLevel(cacheLevel);

        var member = await CreateMember();

        var publishedMember1 = await MemberCacheService.Get(member);
        Assert.That(publishedMember1, Is.Not.Null);

        var publishedMember2 = await MemberCacheService.Get(member);
        Assert.That(publishedMember2, Is.Not.Null);

        Assert.That(publishedMember2, Is.Not.SameAs(publishedMember1));

        var titleValue1 = publishedMember1.Value<string>("title");
        Assert.That(titleValue1, Is.EqualTo("The title"));

        var titleValue2 = publishedMember2.Value<string>("title");
        Assert.That(titleValue2, Is.Not.Null);

        Assert.That(titleValue2, Is.EqualTo("The title"));

        // fetch title values 10 times in total, 5 times from each published member instance
        titleValue1 = publishedMember1.Value<string>("title");
        titleValue1 = publishedMember1.Value<string>("title");
        titleValue1 = publishedMember1.Value<string>("title");
        titleValue1 = publishedMember1.Value<string>("title");

        titleValue2 = publishedMember2.Value<string>("title");
        titleValue2 = publishedMember2.Value<string>("title");
        titleValue2 = publishedMember2.Value<string>("title");
        titleValue2 = publishedMember2.Value<string>("title");

        Assert.That(PropertyValueLevelDetectionTestsConverter.SourceConverts, Is.EqualTo(expectedSourceConverts));
        Assert.That(PropertyValueLevelDetectionTestsConverter.InterConverts, Is.EqualTo(expectedInterConverts));
    }

    private IUser SuperUser() => GetRequiredService<IUserService>().GetAsync(Constants.Security.SuperUserKey).GetAwaiter().GetResult();

    private async Task<IMember> CreateMember()
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        var memberTypeCreateResult = await MemberTypeService.UpdateAsync(memberType, Constants.Security.SuperUserKey);
        Assert.That(memberTypeCreateResult.Success, Is.True);

        var createModel = new MemberCreateModel
        {
            Key = _memberKey,
            Email = "test@test.com",
            Username = "test",
            Password = "SuperSecret123",
            IsApproved = true,
            ContentTypeKey = memberType.Key,
            Roles = [],
            Variants = [new() { Name = "T. Est" }],
            Properties = [new() { Alias = "title", Value = "The title" }],
        };

        var memberCreateResult = await MemberEditingService.CreateAsync(createModel, SuperUser());
        Assert.That(memberCreateResult.Success, Is.True);
        Assert.That(memberCreateResult.Result.Content, Is.Not.Null);

        return memberCreateResult.Result.Content;
    }
}
