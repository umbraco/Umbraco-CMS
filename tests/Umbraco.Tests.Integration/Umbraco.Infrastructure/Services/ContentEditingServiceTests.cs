using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests : ContentEditingServiceTestsBase
{
    [SetUp]
    public new void Setup()
    {
        ContentRepositoryBase.ThrowOnWarning = true;
        ContentEditingNotificationHandler.Reset();
    }

    [TearDown]
    public void TearDownNotificationHandler() => ContentEditingNotificationHandler.Reset();

    public void Relate(IContent parent, IContent child, string relationTypeAlias = Constants.Conventions.RelationTypes.RelatedDocumentAlias)
    {
        var relatedContentRelType = RelationService.GetRelationTypeByAlias(relationTypeAlias);

        var relation = RelationService.Relate(parent.Id, child.Id, relatedContentRelType);
        RelationService.Save(relation);
    }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder
            .AddNotificationAsyncHandler<ContentCopiedNotification, RelateOnCopyNotificationHandler>()
            .AddNotificationHandler<ContentSavingNotification, ContentEditingNotificationHandler>()
            .AddNotificationHandler<ContentPublishingNotification, ContentEditingNotificationHandler>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private async Task<IContentType> CreateTextPageContentTypeAsync()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        return contentType;
    }

    private async Task<(IContent root, IContent child)> CreateRootAndChildAsync(IContentType contentType, string rootName = "The Root", string childName = "The Child")
    {
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new () { Name = rootName }]
        };

        var root = (await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result.Content!;

        contentType.AllowedContentTypes = new List<ContentTypeSort>
        {
            new (contentType.Key, 1, contentType.Alias)
        };
        ContentTypeService.Save(contentType);

        createModel.ParentKey = root.Key;
        createModel.Variants = [new() { Name = childName }];

        var child = (await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result.Content!;
        Assert.AreEqual(root.Id, child.ParentId);

        return (root, child);
    }

    /// <summary>
    ///     Lets a test intercept the cancelable content notifications, following the pattern used by
    ///     <c>ContentServiceNotificationTests</c>.
    /// </summary>
    internal sealed class ContentEditingNotificationHandler :
        INotificationHandler<ContentSavingNotification>,
        INotificationHandler<ContentPublishingNotification>
    {
        public static Action<ContentSavingNotification>? SavingContent { get; set; }

        public static Action<ContentPublishingNotification>? PublishingContent { get; set; }

        public static void Reset()
        {
            SavingContent = null;
            PublishingContent = null;
        }

        public void Handle(ContentSavingNotification notification) => SavingContent?.Invoke(notification);

        public void Handle(ContentPublishingNotification notification) => PublishingContent?.Invoke(notification);
    }
}