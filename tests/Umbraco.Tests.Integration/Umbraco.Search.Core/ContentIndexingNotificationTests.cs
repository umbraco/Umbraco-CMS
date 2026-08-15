using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Notifications;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public class ContentIndexingNotificationTests : InvariantContentTestBase
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.AddNotificationHandler<ContentIndexingNotification, AddOrUpdateIndexingNotificationHandler>();
    }

    public override Task SetupTest()
    {
        AddOrUpdateIndexingNotificationHandler.ManipulateFields = false;
        AddOrUpdateIndexingNotificationHandler.CancelIndexingFor = [];
        return base.SetupTest();
    }

    [Test]
    public void PublishedContent_CanManipulateIndexedFields()
    {
        AddOrUpdateIndexingNotificationHandler.ManipulateFields = true;
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(4));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
            Assert.That(documents[2].Id, Is.EqualTo(GrandchildKey));
            Assert.That(documents[3].Id, Is.EqualTo(GreatGrandchildKey));
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentPropertyValues(documents[0], "The root title - changed by notification handler", true);
            VerifyDocumentPropertyValues(documents[1], "The child title - changed by notification handler", true);
            VerifyDocumentPropertyValues(documents[2], "The grandchild title - changed by notification handler", true);
            VerifyDocumentPropertyValues(documents[3], "The great grandchild title - changed by notification handler", true);
        });
    }

    [Test]
    public void PublishedContent_CanCancelIndexingForSpecificDocument()
    {
        AddOrUpdateIndexingNotificationHandler.CancelIndexingFor = [ChildKey];
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(3));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(GrandchildKey));
            Assert.That(documents[2].Id, Is.EqualTo(GreatGrandchildKey));
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentPropertyValues(documents[0], "The root title", false);
            VerifyDocumentPropertyValues(documents[1], "The grandchild title", false);
            VerifyDocumentPropertyValues(documents[2], "The great grandchild title", false);
        });
    }

    [Test]
    public void DraftContent_CanManipulateIndexedFields()
    {
        AddOrUpdateIndexingNotificationHandler.ManipulateFields = true;
        ContentService.Save([Root(), Child(), Grandchild(), GreatGrandchild()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(4));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
            Assert.That(documents[2].Id, Is.EqualTo(GrandchildKey));
            Assert.That(documents[3].Id, Is.EqualTo(GreatGrandchildKey));
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentPropertyValues(documents[0], "The root title - changed by notification handler", true);
            VerifyDocumentPropertyValues(documents[1], "The child title - changed by notification handler", true);
            VerifyDocumentPropertyValues(documents[2], "The grandchild title - changed by notification handler", true);
            VerifyDocumentPropertyValues(documents[3], "The great grandchild title - changed by notification handler", true);
        });
    }

    [Test]
    public void DraftContent_CanCancelIndexingForSpecificDocument()
    {
        AddOrUpdateIndexingNotificationHandler.CancelIndexingFor = [ChildKey];
        ContentService.Save([Root(), Child(), Grandchild(), GreatGrandchild()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(3));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(GrandchildKey));
            Assert.That(documents[2].Id, Is.EqualTo(GreatGrandchildKey));
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentPropertyValues(documents[0], "The root title", false);
            VerifyDocumentPropertyValues(documents[1], "The grandchild title", false);
            VerifyDocumentPropertyValues(documents[2], "The great grandchild title", false);
        });
    }

    private void VerifyDocumentPropertyValues(TestIndexDocument document, string title, bool manipulatedByNotificationHandler)
    {
        Assert.That(document.Fields.Any(f => f.FieldName == "count"), manipulatedByNotificationHandler ? Is.False : Is.True);

        IndexField? titleField = document.Fields.FirstOrDefault(f => f.FieldName == "title");
        Assert.That(titleField, Is.Not.Null);

        Assert.That(titleField.Value.Texts?.SingleOrDefault(), Is.EqualTo(title));

        if (manipulatedByNotificationHandler)
        {
            CollectionAssert.AreEqual(titleField.Value.Keywords, new[] { "NotificationHandlerKeyword", document.Id.ToString("D") });

            IndexField? nameField = document.Fields.SingleOrDefault(field => field.FieldName == Constants.FieldNames.Name);
            Assert.That(nameField, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(nameField.Value.TextsR1, Is.Null);
                Assert.That(nameField.Value.TextsR2, Is.Not.Empty);
            });
        }
    }

    private class AddOrUpdateIndexingNotificationHandler : INotificationHandler<ContentIndexingNotification>
    {
        public static bool ManipulateFields { get; set; }

        public static Guid[] CancelIndexingFor { get; set; } = [];

        public void Handle(ContentIndexingNotification notification)
        {
            if (ManipulateFields)
            {
                IndexField? titleField = notification.Fields.SingleOrDefault(field => field.FieldName == "title");
                IndexField? countField = notification.Fields.SingleOrDefault(field => field.FieldName == "count");
                IndexField? nameField = notification.Fields.SingleOrDefault(field => field.FieldName == Constants.FieldNames.Name);
                Assert.Multiple(() =>
                {
                    Assert.That(titleField, Is.Not.Null);
                    Assert.That(countField, Is.Not.Null);
                    Assert.That(nameField, Is.Not.Null);
                });

                IndexField newTitleField = titleField with
                {
                    Value = new IndexValue
                    {
                        Texts = [$"{titleField.Value.Texts!.First()} - changed by notification handler"],
                        Keywords = ["NotificationHandlerKeyword", notification.Id.ToString("D")]
                    }
                };

                IndexField newNameField = nameField with
                {
                    Value = new IndexValue
                    {
                        TextsR2 = nameField.Value.TextsR1
                    }
                };

                notification.Fields = notification
                    .Fields
                    .Except([titleField, nameField, countField!])
                    .Union([newTitleField, newNameField])
                    .ToArray();
            }

            if (CancelIndexingFor.Contains(notification.Id))
            {
                notification.Cancel = true;
            }
        }
    }
}
