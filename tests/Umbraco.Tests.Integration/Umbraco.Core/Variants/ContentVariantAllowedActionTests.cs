using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Language = Umbraco.Cms.Core.Models.Language;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Extensions;

[TestFixture]
public class ContentVariantAllowedActionTests : UmbracoTestServerTestBase
{
    private const string UsIso = "en-US";
    private const string DkIso = "da-DK";
    private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private IUmbracoMapper UmbracoMapper => GetRequiredService<IUmbracoMapper>();

    [SetUp]
    public void SetUpTestDate()
    {
        var dk = new Language(DkIso, "Danish");
        LocalizationService.Save(dk);
    }

    [Test]
    public void CanCheckIfUserHasAccessToLanguage()
    {
        // setup user groups
        var user = UserBuilder.CreateUser();
        UserService.Save(user);

        var userGroup = UserGroupBuilder.CreateUserGroup();
        var languageId = LocalizationService.GetLanguageIdByIsoCode(DkIso);
        userGroup.AddAllowedLanguage(languageId!.Value);
        UserService.Save(userGroup, new []{ user.Id});
        var currentUser = UserService.GetUserById(user.Id);

        var result = CreateContent(currentUser);

        var danishVariant = result.Variants.FirstOrDefault(x => x.Language!.IsoCode is DkIso);
        var usVariant = result.Variants.FirstOrDefault(x => x.Language!.IsoCode is UsIso);

        // Right now we duplicate allowedActions if you have access, this should be changed
        // when we implement granular permissions for languages
        Assert.AreEqual(danishVariant!.AllowedActions, result.AllowedActions);
        Assert.AreEqual(usVariant!.AllowedActions, new [] { ActionBrowse.ActionLetter.ToString() });
    }

    private ContentItemDisplay CreateContent(IUser user)
    {
        var contentTypeService = GetRequiredService<IContentTypeService>();
        var contentType = new ContentTypeBuilder().WithContentVariation(ContentVariation.Culture).Build();
        contentTypeService.Save(contentType);

        var rootNode = new ContentBuilder()
            .WithoutIdentity()
            .WithContentType(contentType)
            .WithCultureName(UsIso, "Root")
            .WithCultureName(DkIso, "Rod")
            .Build();

        var contentService = GetRequiredService<IContentService>();
        contentService.SaveAndPublish(rootNode);

        ContentItemDisplay? display = UmbracoMapper.Map<ContentItemDisplay>(rootNode, context =>
        {
            context.Items["CurrentUser"] = user;
        });

        if (display is not null)
        {
            display.AllowPreview = display.AllowPreview && rootNode?.Trashed == false &&
                                   rootNode.ContentType.IsElement == false;
        }

        return display;
    }
}
