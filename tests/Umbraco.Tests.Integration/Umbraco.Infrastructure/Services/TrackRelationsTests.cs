using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence.Relations;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
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
            .AddNotificationHandler<ContentPublishedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<ContentUnpublishedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<RelationSavedNotification, RelationSavedTracker>()
            .AddNotificationHandler<RelationDeletedNotification, RelationDeletedTracker>();
    }

    [SetUp]
    public override async Task Setup()
    {
        RelationSavedTracker.Reset();
        RelationDeletedTracker.Reset();
        await base.Setup();
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
        Assert.That(relations, Has.Count.EqualTo(4));
        Assert.That(relations[0].RelationType.Alias, Is.EqualTo(Constants.Conventions.RelationTypes.RelatedMediaAlias));
        Assert.That(relations[0].ChildId, Is.EqualTo(m1.Id));
        Assert.That(relations[1].RelationType.Alias, Is.EqualTo(Constants.Conventions.RelationTypes.RelatedMediaAlias));
        Assert.That(relations[1].ChildId, Is.EqualTo(m2.Id));
        Assert.That(relations[2].RelationType.Alias, Is.EqualTo(Constants.Conventions.RelationTypes.RelatedDocumentAlias));
        Assert.That(relations[2].ChildId, Is.EqualTo(c1.Id));
        Assert.That(relations[3].RelationType.Alias, Is.EqualTo(Constants.Conventions.RelationTypes.RelatedMemberAlias));
        Assert.That(relations[3].ChildId, Is.EqualTo(member.Id));
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
        Assert.That(RelationSavedTracker.SavedRelations, Has.Count.EqualTo(2));
        Assert.That(RelationSavedTracker.SavedRelations.Any(r => r.ChildId == m1.Id && r.RelationType.Alias == Constants.Conventions.RelationTypes.RelatedMediaAlias), Is.True);
        Assert.That(RelationSavedTracker.SavedRelations.Any(r => r.ChildId == m2.Id && r.RelationType.Alias == Constants.Conventions.RelationTypes.RelatedMediaAlias), Is.True);
        Assert.That(RelationSavedTracker.SavedRelations.All(r => r.ParentId == content.Id), Is.True);
        Assert.That(RelationSavedTracker.LastIsAutomatic, Is.True);
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
        Assert.That(RelationDeletedTracker.DeletedRelations, Has.Count.EqualTo(1));
        Assert.That(RelationDeletedTracker.DeletedRelations[0].ChildId, Is.EqualTo(m2.Id));
        Assert.That(RelationDeletedTracker.DeletedRelations[0].ParentId, Is.EqualTo(content.Id));
        Assert.That(RelationDeletedTracker.LastIsAutomatic, Is.True);
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
        Assert.That(RelationDeletedTracker.DeletedRelations, Has.Count.EqualTo(1));
        Assert.That(RelationDeletedTracker.DeletedRelations[0].ChildId, Is.EqualTo(m1.Id));
        Assert.That(RelationDeletedTracker.DeletedRelations[0].ParentId, Is.EqualTo(content.Id));
        Assert.That(RelationDeletedTracker.LastIsAutomatic, Is.True);
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

        Assert.That(RelationSavedTracker.SavedRelations.Count, Is.EqualTo(0));
        Assert.That(RelationDeletedTracker.DeletedRelations.Count, Is.EqualTo(0));
        Assert.That(RelationSavedTracker.LastIsAutomatic, Is.Null);
        Assert.That(RelationDeletedTracker.LastIsAutomatic, Is.Null);
    }

    [Test]
    [LongRunning]
    public async Task Can_Remove_Stale_Published_Relation_When_Saving_Draft_After_Unpublish()
    {
        (IContent source, IContent targetA, IContent targetB) = await CreatePublishedContentPickerScenario();

        // The published document references target A via the content picker.
        Assert.That(RelationService.GetByParentId(source.Id).Select(x => x.ChildId), Is.EquivalentTo(new[] { targetA.Id }));

        // Unpublish. The document no longer has a live published version, but the property's PublishedValue still holds target A.
        Assert.That(ContentService.Unpublish(source).Success, Is.True);

        // Save a draft that re-points the picker to target B.
        IContent draft = ContentService.GetById(source.Id)!;
        draft.Properties["contentPicker"]!.SetValue(Udi.Create(Constants.UdiEntityType.Document, targetB.Key).ToString());
        ContentService.Save(draft);

        // The stale relation to target A (from the previously-published snapshot) must be gone; only the draft reference to B remains.
        Assert.That(RelationService.GetByParentId(source.Id).Select(x => x.ChildId), Is.EquivalentTo(new[] { targetB.Id }));
    }

    [Test]
    [LongRunning]
    public async Task Can_Remove_Stale_Published_Relation_When_Unpublishing()
    {
        (IContent source, IContent targetA, IContent targetB) = await CreatePublishedContentPickerScenario();

        // Save a draft that re-points the picker to target B, without publishing. The document remains published, so both the
        // live (published A) and draft (edited B) references are legitimately tracked.
        IContent draft = ContentService.GetById(source.Id)!;
        draft.Properties["contentPicker"]!.SetValue(Udi.Create(Constants.UdiEntityType.Document, targetB.Key).ToString());
        ContentService.Save(draft);
        Assert.That(RelationService.GetByParentId(source.Id).Select(x => x.ChildId), Is.EquivalentTo(new[] { targetA.Id, targetB.Id }));

        // Unpublish. There is no longer a live published version, so the stale published reference to A must be removed,
        // leaving only the draft reference to B.
        Assert.That(ContentService.Unpublish(draft).Success, Is.True);

        Assert.That(RelationService.GetByParentId(source.Id).Select(x => x.ChildId), Is.EquivalentTo(new[] { targetB.Id }));
    }

    [Test]
    [LongRunning]
    public async Task Can_Delete_Content_Type_Of_Published_Content_With_Relation()
    {
        // The relation target uses a different, unrelated content type so that it survives the deletion below -
        // this is what allows ContentRelationsUpdate to still resolve the (now-deleted) target's node ID when
        // it re-runs for the source's ContentUnpublishedNotification, and therefore attempt the FK-violating INSERT.
        var targetType = new ContentTypeBuilder().WithAlias("target").WithName("Target Type").Build();
        await ContentTypeService.CreateAsync(targetType, Constants.Security.SuperUserKey);

        var sourceType = new ContentTypeBuilder()
            .WithAlias("source")
            .WithName("Source Type")
            .AddPropertyType()
                .WithAlias("contentPicker")
                .WithName("Content Picker")
                .WithDataTypeId(1046)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ContentPicker)
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(sourceType, Constants.Security.SuperUserKey);

        var target = new ContentBuilder().WithContentType(targetType).WithName("Target").Build();
        ContentService.Save(target);

        var source = new ContentBuilder().WithContentType(sourceType).WithName("Source").Build();
        source.Properties["contentPicker"]!.SetValue(Udi.Create(Constants.UdiEntityType.Document, target.Key).ToString());
        ContentService.Save(source);
        PublishResult publishResult = ContentService.Publish(source, ["*"]);
        Assert.That(publishResult.Success, Is.True, publishResult.Result.ToString());

        // The automatic relation exists for the published content before its content type (and the content
        // itself) is deleted.
        Assert.That(RelationService.GetByParentId(source.Id).Select(x => x.ChildId), Is.EquivalentTo(new[] { target.Id }));

        // Deleting the content type cascades to permanently deleting the published content within the same scope
        // that later raises ContentUnpublishedNotification for it. Before the fix, ContentRelationsUpdate would
        // try to re-persist the relation for the now-deleted node and throw a foreign key violation.
        ContentTypeOperationStatus status = await ContentTypeService.DeleteAsync(sourceType.Key, Constants.Security.SuperUserKey);
        Assert.That(status, Is.EqualTo(ContentTypeOperationStatus.Success));

        Assert.That(ContentService.GetById(source.Id), Is.Null);
        Assert.That(RelationService.GetByChildId(target.Id), Is.Empty);
    }

    private async Task<(IContent Source, IContent TargetA, IContent TargetB)> CreatePublishedContentPickerScenario()
    {
        var contentType = new ContentTypeBuilder()
            .WithName("Page")
            .AddPropertyType()
                .WithAlias("contentPicker")
                .WithName("Content Picker")
                .WithDataTypeId(1046)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ContentPicker)
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 0, contentType.Alias)];
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        var targetA = new ContentBuilder().WithContentType(contentType).WithName("Target A").Build();
        ContentService.Save(targetA);
        var targetB = new ContentBuilder().WithContentType(contentType).WithName("Target B").Build();
        ContentService.Save(targetB);

        // Publish the source document with the picker pointing at target A. Both the edited and published property
        // snapshots now reference target A.
        var source = new ContentBuilder().WithContentType(contentType).WithName("Source").Build();
        source.Properties["contentPicker"]!.SetValue(Udi.Create(Constants.UdiEntityType.Document, targetA.Key).ToString());
        ContentService.Save(source);
        PublishResult publishResult = ContentService.Publish(source, ["*"]);
        Assert.That(publishResult.Success, Is.True, publishResult.Result.ToString());

        return (source, targetA, targetB);
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
