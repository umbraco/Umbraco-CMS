using NUnit.Framework;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Extensions;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
public class ReadOnlyUserGroupExtensionsTests : UmbracoIntegrationTest
{
    private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();

    private IShortStringHelper ShortStringHelper => GetRequiredService<IShortStringHelper>();

    [Test]
    public void CanIfUserHasAccessToLanguage()
    {
        var swe = new Language("sv-SE", "Swedish");
        LocalizationService.Save(swe);
        var userGrp = new UserGroup(ShortStringHelper);
        userGrp.AddAllowedLanguage(swe.Id);

        Assert.IsTrue(userGrp.HasAccessToLanguage(swe.Id));
    }
}
