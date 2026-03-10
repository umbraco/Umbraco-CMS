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
        Assert.IsTrue(publishResult.Success, "Content should be published successfully");

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
        Assert.IsNotNull(publishedContent);
        Assert.AreEqual(TextpageId, publishedContent.Id);
        Assert.AreEqual(Textpage.Key!.Value, publishedContent.Key);
        Assert.AreEqual("Homepage", publishedContent.Name);
        Assert.AreEqual(PublishedItemType.Content, publishedContent.ItemType);
        Assert.AreEqual("Test Title", publishedContent.Value("title"));
        Assert.IsNotNull(publishedContent.ContentType);
        Assert.AreEqual(ContentType.Id, publishedContent.ContentType.Id);
        Assert.AreEqual(ContentType.Key, publishedContent.ContentType.Key);
        Assert.AreEqual(ContentType.Alias, publishedContent.ContentType.Alias);
    }

    [Test]
    public async Task ToIPublishedMedia_Returns_PublishedMedia_For_Media_Item()
    {
        // Arrange - Create media type and media item
        var imageMediaType = MediaTypeService.Get("Image");
        Assert.IsNotNull(imageMediaType, "Image media type should exist");

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
        Assert.IsTrue(mediaResult.Success, "Media should be created successfully");
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
        Assert.IsNotNull(publishedMedia);
        Assert.AreEqual(mediaId, publishedMedia.Id);
        Assert.AreEqual(mediaKey, publishedMedia.Key);
        Assert.AreEqual("TestImage", publishedMedia.Name);
        Assert.AreEqual(PublishedItemType.Media, publishedMedia.ItemType);
        Assert.IsNotNull(publishedMedia.ContentType);
        Assert.AreEqual(imageMediaType.Id, publishedMedia.ContentType.Id);
        Assert.AreEqual(imageMediaType.Key, publishedMedia.ContentType.Key);
        Assert.AreEqual(imageMediaType.Alias, publishedMedia.ContentType.Alias);
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
        Assert.IsTrue(memberResult.Success, "Member should be created successfully");
        var member = memberResult.Result.Content!;

        // Act
        var publishedMember = PublishedContentFactory.ToPublishedMember(member);

        // Assert
        Assert.IsNotNull(publishedMember);
        Assert.AreEqual(member.Id, publishedMember.Id);
        Assert.AreEqual(member.Key, publishedMember.Key);
        Assert.AreEqual("Test User", publishedMember.Name);
        Assert.AreEqual(PublishedItemType.Member, publishedMember.ItemType);
        Assert.AreEqual("test@example.com", publishedMember.Email);
        Assert.AreEqual("testuser", publishedMember.UserName);
        Assert.IsTrue(publishedMember.IsApproved);
        Assert.AreEqual("Test Title Value", publishedMember.Value("title"));
        Assert.IsNotNull(publishedMember.ContentType);
        Assert.AreEqual(memberType.Id, publishedMember.ContentType.Id);
        Assert.AreEqual(memberType.Key, publishedMember.ContentType.Key);
        Assert.AreEqual(memberType.Alias, publishedMember.ContentType.Alias);
    }

    private IUser SuperUser() => UserService.GetAsync(Constants.Security.SuperUserKey).GetAwaiter().GetResult()!;
}
