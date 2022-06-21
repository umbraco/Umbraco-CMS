using NUnit.Framework;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class ReadOnlyUserGroupExtensions
{
    private static int _swedishLanguageId = 1;
    private static int _danishLanguageId = 2;
    private static int _germanLanguageId = 3 ;

    [Test]
    public void CanCheckIfUserHasAccessToLanguage()
    {
        var userGrp = new UserGroup(new MockShortStringHelper());
        userGrp.AddAllowedLanguage(_swedishLanguageId);
        userGrp.AddAllowedLanguage(_danishLanguageId);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(userGrp.HasAccessToLanguage(_swedishLanguageId));
            Assert.IsTrue(userGrp.HasAccessToLanguage(_danishLanguageId));
            Assert.IsFalse(userGrp.HasAccessToLanguage(_germanLanguageId));
        });
    }

    [Test]
    public void CheckIfAllowedLanugagesIsEmptyMeansAccessToAllLanguages()
    {
        var userGrp = new UserGroup(new MockShortStringHelper());

        Assert.Multiple(() =>
        {
            Assert.IsTrue(userGrp.HasAccessToLanguage(_swedishLanguageId));
            Assert.IsTrue(userGrp.HasAccessToLanguage(_danishLanguageId));
            Assert.IsTrue(userGrp.HasAccessToLanguage(_germanLanguageId));
        });
    }
}
