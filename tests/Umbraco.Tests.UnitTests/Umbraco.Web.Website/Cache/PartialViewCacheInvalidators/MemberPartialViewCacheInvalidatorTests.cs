using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Website.Cache.PartialViewCacheInvalidators;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Routing;

[TestFixture]
public class MemberPartialViewCacheInvalidatorTests
{
    [Test]
    public void ClearPartialViewCacheItems_Clears_ExpectedItems()
    {
        var runTimeCacheMock = new Mock<IAppPolicyCache>();
        runTimeCacheMock
            .Setup(x => x.ClearByRegex(It.IsAny<string>()))
            .Verifiable();
        var appCaches = new AppCaches(
            runTimeCacheMock.Object,
            NoAppCache.Instance,
            new IsolatedCaches(type => new ObjectCacheAppCache()));
        var memberPartialViewCacheInvalidator = new MemberPartialViewCacheInvalidator(appCaches);

        var memberIds = new[] { 1, 2, 3 };

        memberPartialViewCacheInvalidator.ClearPartialViewCacheItems(memberIds);

        foreach (var memberId in memberIds)
        {
            var regex = $"Umbraco.Web.PartialViewCacheKey.*-m{memberId}-*";
            runTimeCacheMock
                .Verify(x => x.ClearByRegex(It.Is<string>(x => x == regex)), Times.Once);
        }
    }

    [Test]
    public async Task ClearPartialViewCacheItems_Regex_Matches_CachedKeys()
    {
        const int MemberId = 1234;

        var memberManagerMock = new Mock<IMemberManager>();
        memberManagerMock
            .Setup(x => x.GetCurrentMemberAsync())
            .ReturnsAsync(new MemberIdentityUser { Id = MemberId.ToString() });

        var cacheKey = await HtmlHelperRenderExtensions.GenerateCacheKeyForCachedPartialViewAsync(
            "TestPartial.cshtml",
            true,
            Mock.Of<IUmbracoContext>(),
            true,
            memberManagerMock.Object,
            new TestViewModel(),
            new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
            null);
        cacheKey = CoreCacheHelperExtensions.PartialViewCacheKey + cacheKey;
        Assert.AreEqual("Umbraco.Web.PartialViewCacheKeyTestPartial.cshtml-en-US-0-m1234-", cacheKey);

        var regexForMember = $"Umbraco.Web.PartialViewCacheKey.*-m{MemberId}-*";
        var regexMatch = Regex.IsMatch(cacheKey, regexForMember);
        Assert.IsTrue(regexMatch);

        var regexForAnotherMember = $"Umbraco.Web.PartialViewCacheKey.*-m{4321}-*";
        regexMatch = Regex.IsMatch(cacheKey, regexForAnotherMember);
        Assert.IsFalse(regexMatch);
    }

    private class TestViewModel
    {
    }
}
