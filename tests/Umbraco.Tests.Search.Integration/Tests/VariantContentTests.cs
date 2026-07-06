using Umbraco.Cms.Search.Core;
using Umbraco.Cms.Search.Core.Helpers;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Tests.Search.Integration.Services;

namespace Umbraco.Tests.Search.Integration.Tests;

public class VariantContentTests : VariantContentTestBase
{
    [Test]
    public void PublishedStructure_YieldsAllPublishedDocuments()
    {
        ContentService.Save(Root());
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
            VerifyDocumentPropertyValues(documents[0], "The root title", "The root message", 12);
            VerifyDocumentPropertyValues(documents[1], "The child title", "The child message", 34);
            VerifyDocumentPropertyValues(documents[2], "The grandchild title", "The grandchild message", 56);
            VerifyDocumentPropertyValues(documents[3], "The great grandchild title", "The great grandchild message", 78);
        });
    }

    [Test]
    public void PublishedStructure_CanRefreshChild_InSingleCulture()
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        IContent child = Child();
        child.SetValue("title", "The updated child title in English", "en-US");
        child.SetValue("message", "The updated child message in English (default)", "en-US");
        child.SetValue("message", "The updated child message in English (segment-1)", "en-US", "segment-1");
        child.SetValue("message", "The updated child message in English (segment-2)", "en-US", "segment-2");
        ContentService.Save(child);
        ContentService.Publish(Child(), ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(4));

        VerifyDocumentPropertyValues(
            documents[1],
            "The updated child title in English",
            "The child title in Danish",
            "The updated child message in English",
            "The child message in Danish",
            34);
    }

    [Test]
    public void PublishedStructure_CanRefreshChild_InMultipleCultures()
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        IContent child = Child();
        child.SetValue("title", "The updated child title in English", "en-US");
        child.SetValue("title", "The updated child title in Danish", "da-DK");
        ContentService.Save(child);
        ContentService.Publish(Child(), ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(4));

        VerifyDocumentPropertyValues(documents[1], "The updated child title", "The child message", 34);
    }

    [Test]
    public void PublishedStructure_CanRefreshChild_InvariantCulture()
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        IContent child = Child();
        child.SetValue("count", 123456);
        ContentService.Save(child);
        ContentService.Publish(Child(), ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(4));

        VerifyDocumentPropertyValues(documents[1], "The child title", "The child message", 123456);
    }

    [Test]
    public void PublishedStructure_YieldsSystemFields()
    {
        ContentService.Save(Root());
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
            VerifyDocumentSystemValues(documents[0], Root());
            VerifyDocumentSystemValues(documents[1], Child());
            VerifyDocumentSystemValues(documents[2], Grandchild());
            VerifyDocumentSystemValues(documents[3], GreatGrandchild());
        });
    }

    private void VerifyDocumentPropertyValues(TestIndexDocument document, string title, string message, int count)
        => VerifyDocumentPropertyValues(document, $"{title} in English", $"{title} in Danish", $"{message} in English", $"{message} in Danish", count);

    private void VerifyDocumentPropertyValues(TestIndexDocument document, string englishTitle, string danishTitle, string englishMessage, string danishMessage, int count)
        => Assert.Multiple(() =>
        {
            IndexField[] titleFields = document.Fields.Where(f => f.FieldName == "title").ToArray();
            Assert.That(titleFields.Length, Is.EqualTo(2));
            Assert.That(titleFields.SingleOrDefault(f => f.Culture.InvariantEquals("en-US"))?.Value.Texts?.SingleOrDefault(), Is.EqualTo(englishTitle));
            Assert.That(titleFields.SingleOrDefault(f => f.Culture.InvariantEquals("da-DK"))?.Value.Texts?.SingleOrDefault(), Is.EqualTo(danishTitle));

            IndexField[] messageFields = document.Fields.Where(f => f.FieldName == "message").ToArray();
            Assert.That(messageFields.Length, Is.EqualTo(6));
            Assert.That(messageFields.SingleOrDefault(f => f.Culture.InvariantEquals("en-US") && f.Segment is null)?.Value.Texts?.SingleOrDefault(), Is.EqualTo($"{englishMessage} (default)"));
            Assert.That(messageFields.SingleOrDefault(f => f.Culture.InvariantEquals("en-US") && f.Segment == "segment-1")?.Value.Texts?.SingleOrDefault(), Is.EqualTo($"{englishMessage} (segment-1)"));
            Assert.That(messageFields.SingleOrDefault(f => f.Culture.InvariantEquals("en-US") && f.Segment == "segment-2")?.Value.Texts?.SingleOrDefault(), Is.EqualTo($"{englishMessage} (segment-2)"));
            Assert.That(messageFields.SingleOrDefault(f => f.Culture.InvariantEquals("da-DK") && f.Segment is null)?.Value.Texts?.SingleOrDefault(), Is.EqualTo($"{danishMessage} (default)"));
            Assert.That(messageFields.SingleOrDefault(f => f.Culture.InvariantEquals("da-DK") && f.Segment == "segment-1")?.Value.Texts?.SingleOrDefault(), Is.EqualTo($"{danishMessage} (segment-1)"));
            Assert.That(messageFields.SingleOrDefault(f => f.Culture.InvariantEquals("da-DK") && f.Segment == "segment-2")?.Value.Texts?.SingleOrDefault(), Is.EqualTo($"{danishMessage} (segment-2)"));

            var countValue = document.Fields.FirstOrDefault(f => f.FieldName == "count")?.Value.Integers?.SingleOrDefault();
            Assert.That(countValue, Is.EqualTo(count));
        });

    private void VerifyDocumentSystemValues(TestIndexDocument document, IContent content)
    {
        IDateTimeOffsetConverter dateTimeOffsetConverter = GetRequiredService<IDateTimeOffsetConverter>();

        Assert.Multiple(() =>
        {
            var contentTypeValue = document.Fields.FirstOrDefault(f => f.FieldName == Constants.FieldNames.ContentTypeId)?.Value.Keywords?.SingleOrDefault();
            Assert.That(contentTypeValue, Is.EqualTo(content.ContentType.Key.AsKeyword()));

            IndexField[] nameFields = document.Fields.Where(f => f.FieldName == Constants.FieldNames.Name).ToArray();
            Assert.That(nameFields.Length, Is.EqualTo(2));
            Assert.That(nameFields.SingleOrDefault(f => f.Culture.InvariantEquals("en-US"))?.Value.TextsR1?.SingleOrDefault(), Is.EqualTo(content.GetCultureName("en-US")));
            Assert.That(nameFields.SingleOrDefault(f => f.Culture.InvariantEquals("da-DK"))?.Value.TextsR1?.SingleOrDefault(), Is.EqualTo(content.GetCultureName("da-DK")));

            DateTimeOffset? createDateValue = document.Fields.FirstOrDefault(f => f.FieldName == Constants.FieldNames.CreateDate)?.Value.DateTimeOffsets?.SingleOrDefault();
            Assert.That(createDateValue, Is.EqualTo(dateTimeOffsetConverter.ToDateTimeOffset(content.CreateDate)));

            DateTimeOffset? updateDateValue = document.Fields.FirstOrDefault(f => f.FieldName == Constants.FieldNames.UpdateDate)?.Value.DateTimeOffsets?.SingleOrDefault();
            Assert.That(updateDateValue, Is.EqualTo(dateTimeOffsetConverter.ToDateTimeOffset(content.UpdateDate)));

            var levelValue = document.Fields.FirstOrDefault(f => f.FieldName == Constants.FieldNames.Level)?.Value.Integers?.SingleOrDefault();
            Assert.That(levelValue, Is.EqualTo(content.Level));

            var sortOrderValue = document.Fields.FirstOrDefault(f => f.FieldName == Constants.FieldNames.SortOrder)?.Value.Integers?.SingleOrDefault();
            Assert.That(sortOrderValue, Is.EqualTo(content.SortOrder));
        });
    }
}
