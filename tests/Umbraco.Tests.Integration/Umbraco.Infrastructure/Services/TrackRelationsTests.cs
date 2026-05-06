using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Relations;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true,
    Boot = true)]
internal sealed class TrackRelationsTests : UmbracoIntegrationTestWithContent
{
    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IRelationService RelationService => GetRequiredService<IRelationService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder
            .AddNotificationHandler<ContentSavedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<RelationSavedNotification, RelationSavedTracker>()
            .AddNotificationHandler<RelationDeletedNotification, RelationDeletedTracker>();
    }

    [SetUp]
    public override void Setup()
    {
        RelationSavedTracker.Reset();
        RelationDeletedTracker.Reset();
        base.Setup();
    }

    [Test]
    [LongRunning]
    public async Task Automatically_Track_Relations()
    {
        var mt = MediaTypeBuilder.CreateSimpleMediaType("testMediaType", "Test Media Type");
        await MediaTypeService.CreateAsync(mt, Constants.Security.SuperUserKey);
        var m1 = MediaBuilder.CreateSimpleMedia(mt, "hello 1", -1);
        var m2 = MediaBuilder.CreateSimpleMedia(mt, "hello 1", -1);
        MediaService.Save(m1);
        MediaService.Save(m2);

        var memberType = MemberTypeBuilder.CreateSimpleMemberType("testMemberType", "Test Member Type");
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
        var member = MemberBuilder.CreateSimpleMember(memberType, "Test Member", "test@test.com", "xxxxxxxx", "testMember");
        MemberService.Save(member);

        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var ct = ContentTypeBuilder.CreateTextPageContentType("richTextTest", defaultTemplateId: template.Id);
        ct.AllowedTemplates = Enumerable.Empty<ITemplate>();
        await ContentTypeService.CreateAsync(ct, Constants.Security.SuperUserKey);

        var c1 = ContentBuilder.CreateTextpageContent(ct, "my content 1", -1);
        ContentService.Save(c1);

        var c2 = ContentBuilder.CreateTextpageContent(ct, "my content 2", -1);

        // 'bodyText' is a property with a RTE property editor which we knows tracks relations
        c2.Properties["bodyText"].SetValue(@"<p>
        <img src='/media/12312.jpg' data-udi='umb://media/" + m1.Key.ToString("N") + @"' />
</p><p><img src='/media/234234.jpg' data-udi=""umb://media/" + m2.Key.ToString("N") + @""" />
</p>
<p>
    <a href=""{locallink:umb://document/" + c1.Key.ToString("N") + @"}"">hello</a>
</p>
<p>
    <!-- A test reference to a member that will be picked up in the RTE reference extraction -->
    <div data-udi='umb://member/" + member.Key.ToString("N") + @"'></div>
</p>");

        ContentService.Save(c2);

        var relations = RelationService.GetByParentId(c2.Id).ToList();
        Assert.AreEqual(4, relations.Count);
        Assert.AreEqual(Constants.Conventions.RelationTypes.RelatedMediaAlias, relations[0].RelationType.Alias);
        Assert.AreEqual(m1.Id, relations[0].ChildId);
        Assert.AreEqual(Constants.Conventions.RelationTypes.RelatedMediaAlias, relations[1].RelationType.Alias);
        Assert.AreEqual(m2.Id, relations[1].ChildId);
        Assert.AreEqual(Constants.Conventions.RelationTypes.RelatedDocumentAlias, relations[2].RelationType.Alias);
        Assert.AreEqual(c1.Id, relations[2].ChildId);
        Assert.AreEqual(Constants.Conventions.RelationTypes.RelatedMemberAlias, relations[3].RelationType.Alias);
        Assert.AreEqual(member.Id, relations[3].ChildId);
    }

    [Test]
    [LongRunning]
    public async Task Automatic_Relations_Publish_Saved_Notification()
    {
        var mt = MediaTypeBuilder.CreateSimpleMediaType("testMediaType", "Test Media Type");
        await MediaTypeService.CreateAsync(mt, Constants.Security.SuperUserKey);
        var m1 = MediaBuilder.CreateSimpleMedia(mt, "media 1", -1);
        var m2 = MediaBuilder.CreateSimpleMedia(mt, "media 2", -1);
        MediaService.Save(m1);
        MediaService.Save(m2);

        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);
        var ct = ContentTypeBuilder.CreateTextPageContentType("richTextTest", defaultTemplateId: template.Id);
        ct.AllowedTemplates = Enumerable.Empty<ITemplate>();
        await ContentTypeService.CreateAsync(ct, Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateTextpageContent(ct, "my content", -1);
        content.Properties["bodyText"]!.SetValue(
            "<p><img src='/media/1.jpg' data-udi='umb://media/" + m1.Key.ToString("N") + "' /></p>" +
            "<p><img src='/media/2.jpg' data-udi='umb://media/" + m2.Key.ToString("N") + "' /></p>");

        RelationSavedTracker.Reset();
        ContentService.Save(content);

        // Verify the saved notification was published with the correct relations.
        Assert.AreEqual(2, RelationSavedTracker.SavedRelations.Count);
        Assert.IsTrue(RelationSavedTracker.SavedRelations.Any(r => r.ChildId == m1.Id && r.RelationType.Alias == Constants.Conventions.RelationTypes.RelatedMediaAlias));
        Assert.IsTrue(RelationSavedTracker.SavedRelations.Any(r => r.ChildId == m2.Id && r.RelationType.Alias == Constants.Conventions.RelationTypes.RelatedMediaAlias));
        Assert.IsTrue(RelationSavedTracker.SavedRelations.All(r => r.ParentId == content.Id));
        Assert.IsTrue(RelationSavedTracker.LastIsAutomatic);
    }

    [Test]
    [LongRunning]
    public async Task Automatic_Relations_Publish_Deleted_Notification_When_References_Removed()
    {
        var mt = MediaTypeBuilder.CreateSimpleMediaType("testMediaType", "Test Media Type");
        await MediaTypeService.CreateAsync(mt, Constants.Security.SuperUserKey);
        var m1 = MediaBuilder.CreateSimpleMedia(mt, "media 1", -1);
        var m2 = MediaBuilder.CreateSimpleMedia(mt, "media 2", -1);
        MediaService.Save(m1);
        MediaService.Save(m2);

        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);
        var ct = ContentTypeBuilder.CreateTextPageContentType("richTextTest", defaultTemplateId: template.Id);
        ct.AllowedTemplates = Enumerable.Empty<ITemplate>();
        await ContentTypeService.CreateAsync(ct, Constants.Security.SuperUserKey);

        // Save content with two media references.
        var content = ContentBuilder.CreateTextpageContent(ct, "my content", -1);
        content.Properties["bodyText"]!.SetValue(
            "<p><img src='/media/1.jpg' data-udi='umb://media/" + m1.Key.ToString("N") + "' /></p>" +
            "<p><img src='/media/2.jpg' data-udi='umb://media/" + m2.Key.ToString("N") + "' /></p>");
        ContentService.Save(content);

        // Remove one media reference, keeping only m1.
        RelationDeletedTracker.Reset();
        content.Properties["bodyText"]!.SetValue(
            "<p><img src='/media/1.jpg' data-udi='umb://media/" + m1.Key.ToString("N") + "' /></p>");
        ContentService.Save(content);

        // Verify the deleted notification was published for the removed relation.
        Assert.AreEqual(1, RelationDeletedTracker.DeletedRelations.Count);
        Assert.AreEqual(m2.Id, RelationDeletedTracker.DeletedRelations[0].ChildId);
        Assert.AreEqual(content.Id, RelationDeletedTracker.DeletedRelations[0].ParentId);
        Assert.IsTrue(RelationDeletedTracker.LastIsAutomatic);
    }

    [Test]
    [LongRunning]
    public async Task Automatic_Relations_Publish_Deleted_Notification_When_All_References_Removed()
    {
        var mt = MediaTypeBuilder.CreateSimpleMediaType("testMediaType", "Test Media Type");
        await MediaTypeService.CreateAsync(mt, Constants.Security.SuperUserKey);
        var m1 = MediaBuilder.CreateSimpleMedia(mt, "media 1", -1);
        MediaService.Save(m1);

        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);
        var ct = ContentTypeBuilder.CreateTextPageContentType("richTextTest", defaultTemplateId: template.Id);
        ct.AllowedTemplates = Enumerable.Empty<ITemplate>();
        await ContentTypeService.CreateAsync(ct, Constants.Security.SuperUserKey);

        // Save content with a media reference.
        var content = ContentBuilder.CreateTextpageContent(ct, "my content", -1);
        content.Properties["bodyText"]!.SetValue(
            "<p><img src='/media/1.jpg' data-udi='umb://media/" + m1.Key.ToString("N") + "' /></p>");
        ContentService.Save(content);

        // Remove all references (hits the early-exit path in ContentRelationsUpdate).
        RelationDeletedTracker.Reset();
        content.Properties["bodyText"]!.SetValue("<p>no references</p>");
        ContentService.Save(content);

        // Verify the deleted notification was published for the removed relation.
        Assert.AreEqual(1, RelationDeletedTracker.DeletedRelations.Count);
        Assert.AreEqual(m1.Id, RelationDeletedTracker.DeletedRelations[0].ChildId);
        Assert.AreEqual(content.Id, RelationDeletedTracker.DeletedRelations[0].ParentId);
        Assert.IsTrue(RelationDeletedTracker.LastIsAutomatic);
    }

    [Test]
    [LongRunning]
    public async Task Automatic_Relations_No_Notification_When_Unchanged()
    {
        var mt = MediaTypeBuilder.CreateSimpleMediaType("testMediaType", "Test Media Type");
        await MediaTypeService.CreateAsync(mt, Constants.Security.SuperUserKey);
        var m1 = MediaBuilder.CreateSimpleMedia(mt, "media 1", -1);
        MediaService.Save(m1);

        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);
        var ct = ContentTypeBuilder.CreateTextPageContentType("richTextTest", defaultTemplateId: template.Id);
        ct.AllowedTemplates = Enumerable.Empty<ITemplate>();
        await ContentTypeService.CreateAsync(ct, Constants.Security.SuperUserKey);

        // Save content with a media reference.
        var content = ContentBuilder.CreateTextpageContent(ct, "my content", -1);
        content.Properties["bodyText"]!.SetValue(
            "<p><img src='/media/1.jpg' data-udi='umb://media/" + m1.Key.ToString("N") + "' /></p>");
        ContentService.Save(content);

        // Save again with the same references - no notifications should fire.
        RelationSavedTracker.Reset();
        RelationDeletedTracker.Reset();
        ContentService.Save(content);

        Assert.AreEqual(0, RelationSavedTracker.SavedRelations.Count);
        Assert.AreEqual(0, RelationDeletedTracker.DeletedRelations.Count);
        Assert.IsNull(RelationSavedTracker.LastIsAutomatic);
        Assert.IsNull(RelationDeletedTracker.LastIsAutomatic);
    }

    private sealed class RelationSavedTracker : INotificationHandler<RelationSavedNotification>
    {
        public static List<IRelation> SavedRelations { get; } = new();

        public static bool? LastIsAutomatic { get; private set; }

        public static void Reset()
        {
            SavedRelations.Clear();
            LastIsAutomatic = null;
        }

        public void Handle(RelationSavedNotification notification)
        {
            SavedRelations.AddRange(notification.SavedEntities);
            LastIsAutomatic = notification.IsAutomatic;
        }
    }

    private sealed class RelationDeletedTracker : INotificationHandler<RelationDeletedNotification>
    {
        public static List<IRelation> DeletedRelations { get; } = new();

        public static bool? LastIsAutomatic { get; private set; }

        public static void Reset()
        {
            DeletedRelations.Clear();
            LastIsAutomatic = null;
        }

        public void Handle(RelationDeletedNotification notification)
        {
            DeletedRelations.AddRange(notification.DeletedEntities);
            LastIsAutomatic = notification.IsAutomatic;
        }
    }
}
