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
    public void Setup() => ContentRepositoryBase.ThrowOnWarning = true;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.AddNotificationHandler<ContentCopiedNotification, RelateOnCopyNotificationHandler>();

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
            InvariantName = rootName
        };

        var root = (await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result.Content!;

        contentType.AllowedContentTypes = new List<ContentTypeSort>
        {
            new (contentType.Key, 1, contentType.Alias)
        };
        ContentTypeService.Save(contentType);

        createModel.ParentKey = root.Key;
        createModel.InvariantName = childName;

        var child = (await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result.Content!;
        Assert.AreEqual(root.Id, child.ParentId);

        return (root, child);
    }
 }
