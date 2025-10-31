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
        Assert.IsNotNull(publishedMember1);

        var publishedMember2 = await MemberCacheService.Get(member);
        Assert.IsNotNull(publishedMember2);

        Assert.AreNotSame(publishedMember1,  publishedMember2);

        var titleValue1 = publishedMember1.Value<string>("title");
        Assert.AreEqual("The title", titleValue1);

        var titleValue2 = publishedMember2.Value<string>("title");
        Assert.IsNotNull(titleValue2);

        Assert.AreEqual("The title",  titleValue2);

        // fetch title values 10 times in total, 5 times from each published member instance
        titleValue1 = publishedMember1.Value<string>("title");
        titleValue1 = publishedMember1.Value<string>("title");
        titleValue1 = publishedMember1.Value<string>("title");
        titleValue1 = publishedMember1.Value<string>("title");

        titleValue2 = publishedMember2.Value<string>("title");
        titleValue2 = publishedMember2.Value<string>("title");
        titleValue2 = publishedMember2.Value<string>("title");
        titleValue2 = publishedMember2.Value<string>("title");

        Assert.AreEqual(expectedSourceConverts, PropertyValueLevelDetectionTestsConverter.SourceConverts);
        Assert.AreEqual(expectedInterConverts, PropertyValueLevelDetectionTestsConverter.InterConverts);
    }

    private IUser SuperUser() => GetRequiredService<IUserService>().GetAsync(Constants.Security.SuperUserKey).GetAwaiter().GetResult();

    private async Task<IMember> CreateMember()
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        var memberTypeCreateResult = await MemberTypeService.UpdateAsync(memberType, Constants.Security.SuperUserKey);
        Assert.IsTrue(memberTypeCreateResult.Success);

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
        Assert.IsTrue(memberCreateResult.Success);
        Assert.IsNotNull(memberCreateResult.Result.Content);

        return memberCreateResult.Result.Content;
    }
}
