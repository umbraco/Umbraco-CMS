using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.HybridCache.Factories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class PublishedContentFactoryTests : UmbracoIntegrationTestWithContentEditing
{
    private IPublishedContentFactory PublishedContentFactory => GetRequiredService<IPublishedContentFactory>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMemberEditingService MemberEditingService => GetRequiredService<IMemberEditingService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberGroupService MemberGroupService => GetRequiredService<IMemberGroupService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    [Test]
    public async Task ToIPublishedContent_Returns_PublishedContent_For_Published_Document()
    {
        // Arrange - Publish the content
        var publishResult = await ContentPublishingService.PublishAsync(
            Textpage.Key!.Value,
            [new CulturePublishScheduleModel()],
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True, "Content should be published successfully");

        // Create ContentCacheNode from the content
#pragma warning disable CS0618 // Type or member is obsolete
        var contentCacheNode = new ContentCacheNode
        {
            Id = TextpageId,
            Key = Textpage.Key!.Value,
            SortOrder = 0,
            CreateDate = DateTime.UtcNow,
            CreatorId = Constants.Security.SuperUserId,
            ContentTypeId = ContentType.Id,
            IsDraft = false,
            Data = new ContentData(
                name: "Homepage",
                urlSegment: "homepage",
                versionId: 1,
                versionDate: DateTime.UtcNow,
                writerId: Constants.Security.SuperUserId,
                templateId: TemplateId,
                published: true,
                properties: new Dictionary<string, PropertyData[]>
                {
                    ["title"] = [new PropertyData { Culture = string.Empty, Segment = string.Empty, Value = "Test Title" }]
                },
                cultureInfos: null)
        };
#pragma warning restore CS0618 // Type or member is obsolete

        // Act
        var publishedContent = PublishedContentFactory.ToIPublishedContent(contentCacheNode, preview: false);

        // Assert
        Assert.That(publishedContent, Is.Not.Null);
        Assert.That(publishedContent.Id, Is.EqualTo(TextpageId));
        Assert.That(publishedContent.Key, Is.EqualTo(Textpage.Key!.Value));
        Assert.That(publishedContent.Name, Is.EqualTo("Homepage"));
        Assert.That(publishedContent.ItemType, Is.EqualTo(PublishedItemType.Content));
        Assert.That(publishedContent.Value("title"), Is.EqualTo("Test Title"));
        Assert.That(publishedContent.ContentType, Is.Not.Null);
        Assert.That(publishedContent.ContentType.Id, Is.EqualTo(ContentType.Id));
        Assert.That(publishedContent.ContentType.Key, Is.EqualTo(ContentType.Key));
        Assert.That(publishedContent.ContentType.Alias, Is.EqualTo(ContentType.Alias));
    }

    [Test]
    public async Task ToIPublishedMedia_Returns_PublishedMedia_For_Media_Item()
    {
        // Arrange - Create media type and media item
        var imageMediaType = MediaTypeService.Get("Image");
        Assert.That(imageMediaType, Is.Not.Null, "Image media type should exist");

        // Make umbracoFile not mandatory for testing
        var umbracoFileProperty = imageMediaType.PropertyTypes.FirstOrDefault(p => p.Alias == "umbracoFile");
        if (umbracoFileProperty != null)
        {
            umbracoFileProperty.Mandatory = false;
            await MediaTypeService.UpdateAsync(imageMediaType, Constants.Security.SuperUserKey);
        }

        // Create media item
        var mediaCreateModel = MediaEditingBuilder.CreateSimpleMedia(imageMediaType.Key, "TestImage", null);
        var mediaResult = await MediaEditingService.CreateAsync(mediaCreateModel, Constants.Security.SuperUserKey);
        Assert.That(mediaResult.Success, Is.True, "Media should be created successfully");
        var mediaId = mediaResult.Result.Content!.Id;
        var mediaKey = mediaCreateModel.Key!.Value;

        // Create ContentCacheNode for media
#pragma warning disable CS0618 // Type or member is obsolete
        var contentCacheNode = new ContentCacheNode
        {
            Id = mediaId,
            Key = mediaKey,
            SortOrder = 0,
            CreateDate = DateTime.UtcNow,
            CreatorId = Constants.Security.SuperUserId,
            ContentTypeId = imageMediaType.Id,
            IsDraft = false,
            Data = new ContentData(
                name: "TestImage",
                urlSegment: null,
                versionId: 1,
                versionDate: DateTime.UtcNow,
                writerId: Constants.Security.SuperUserId,
                templateId: null,
                published: true,
                properties: new Dictionary<string, PropertyData[]>(),
                cultureInfos: null)
        };
#pragma warning restore CS0618 // Type or member is obsolete

        // Act
        var publishedMedia = PublishedContentFactory.ToIPublishedMedia(contentCacheNode);

        // Assert
        Assert.That(publishedMedia, Is.Not.Null);
        Assert.That(publishedMedia.Id, Is.EqualTo(mediaId));
        Assert.That(publishedMedia.Key, Is.EqualTo(mediaKey));
        Assert.That(publishedMedia.Name, Is.EqualTo("TestImage"));
        Assert.That(publishedMedia.ItemType, Is.EqualTo(PublishedItemType.Media));
        Assert.That(publishedMedia.ContentType, Is.Not.Null);
        Assert.That(publishedMedia.ContentType.Id, Is.EqualTo(imageMediaType.Id));
        Assert.That(publishedMedia.ContentType.Key, Is.EqualTo(imageMediaType.Key));
        Assert.That(publishedMedia.ContentType.Alias, Is.EqualTo(imageMediaType.Alias));
    }

    [Test]
    public async Task ToPublishedMember_Returns_PublishedMember_For_Member()
    {
        // Arrange - Create member type and member
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType("testMemberType", "Test Member Type");
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        MemberService.AddRole("TestRole");
        var group = MemberGroupService.GetByName("TestRole");

        var memberCreateModel = new MemberCreateModel
        {
            Key = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            Password = "SuperSecret123",
            IsApproved = true,
            ContentTypeKey = memberType.Key,
            Roles = [group.Key],
            Variants = [new() { Name = "Test User" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Test Title Value" },
                new PropertyValueModel { Alias = "author", Value = "Test Author" }
            ],
        };

        var memberResult = await MemberEditingService.CreateAsync(memberCreateModel, SuperUser());
        Assert.That(memberResult.Success, Is.True, "Member should be created successfully");
        var member = memberResult.Result.Content!;

        // Act
        var publishedMember = PublishedContentFactory.ToPublishedMember(member);

        // Assert
        Assert.That(publishedMember, Is.Not.Null);
        Assert.That(publishedMember.Id, Is.EqualTo(member.Id));
        Assert.That(publishedMember.Key, Is.EqualTo(member.Key));
        Assert.That(publishedMember.Name, Is.EqualTo("Test User"));
        Assert.That(publishedMember.ItemType, Is.EqualTo(PublishedItemType.Member));
        Assert.That(publishedMember.Email, Is.EqualTo("test@example.com"));
        Assert.That(publishedMember.UserName, Is.EqualTo("testuser"));
        Assert.That(publishedMember.IsApproved, Is.True);
        Assert.That(publishedMember.Value("title"), Is.EqualTo("Test Title Value"));
        Assert.That(publishedMember.ContentType, Is.Not.Null);
        Assert.That(publishedMember.ContentType.Id, Is.EqualTo(memberType.Id));
        Assert.That(publishedMember.ContentType.Key, Is.EqualTo(memberType.Key));
        Assert.That(publishedMember.ContentType.Alias, Is.EqualTo(memberType.Alias));
    }

    private IUser SuperUser() => UserService.GetAsync(Constants.Security.SuperUserKey).GetAwaiter().GetResult()!;
}
