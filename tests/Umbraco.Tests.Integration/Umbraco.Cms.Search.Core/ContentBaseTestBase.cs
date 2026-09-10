using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Cms.Search.Core;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public abstract class ContentBaseTestBase : TestBase
{
    protected void VerifyDocumentStructureValues(TestIndexDocument document, Guid key, Guid parentKey, Guid[] pathKeys)
        => Assert.Multiple(() =>
        {
            var idValue = document.Fields.FirstOrDefault(f => f.FieldName == global::Umbraco.Cms.Core.Constants.IndexFieldNames.Id)?.Value.Keywords?.SingleOrDefault();
            Assert.That(idValue, Is.EqualTo(key.AsKeyword()));

            var parentIdValue = document.Fields.FirstOrDefault(f => f.FieldName == global::Umbraco.Cms.Core.Constants.IndexFieldNames.ParentId)?.Value.Keywords?.SingleOrDefault();
            Assert.That(parentIdValue, Is.EqualTo(parentKey.AsKeyword()));

            var pathIdsValue = document.Fields.FirstOrDefault(f => f.FieldName == global::Umbraco.Cms.Core.Constants.IndexFieldNames.PathIds)?.Value.Keywords?.ToArray();
            Assert.That(pathIdsValue, Is.Not.Null);
            Assert.That(pathIdsValue!.Length, Is.EqualTo(pathKeys.Length));
            Assert.That(pathIdsValue, Is.EquivalentTo(pathKeys.Select(ancestorId => ancestorId.AsKeyword())));
        });

    protected void VerifyDocumentSystemValues(TestIndexDocument document, IContentBase content, string[] tags)
    {
        IDateTimeOffsetConverter dateTimeOffsetConverter = GetRequiredService<IDateTimeOffsetConverter>();
        var expectedObjectTypeValue = content.ObjectType().ToString();

        Assert.Multiple(() =>
        {
            var contentTypeIdValue = document.Fields.FirstOrDefault(f => f.FieldName == global::Umbraco.Cms.Core.Constants.IndexFieldNames.ContentTypeId)?.Value.Keywords?.SingleOrDefault();
            Assert.That(contentTypeIdValue, Is.EqualTo(content.ContentType.Key.AsKeyword()));

            var nameValue = document.Fields.FirstOrDefault(f => f.FieldName == global::Umbraco.Cms.Core.Constants.IndexFieldNames.Name)?.Value.TextsR1?.SingleOrDefault();
            Assert.That(nameValue, Is.EqualTo(content.Name));

            DateTimeOffset? createDateValue = document.Fields.FirstOrDefault(f => f.FieldName == global::Umbraco.Cms.Core.Constants.IndexFieldNames.CreateDate)?.Value.DateTimeOffsets?.SingleOrDefault();
            Assert.That(createDateValue, Is.EqualTo(dateTimeOffsetConverter.ToDateTimeOffset(content.CreateDate)));

            DateTimeOffset? updateDateValue = document.Fields.FirstOrDefault(f => f.FieldName == global::Umbraco.Cms.Core.Constants.IndexFieldNames.UpdateDate)?.Value.DateTimeOffsets?.SingleOrDefault();
            Assert.That(updateDateValue, Is.EqualTo(dateTimeOffsetConverter.ToDateTimeOffset(content.UpdateDate)));

            var levelValue = document.Fields.FirstOrDefault(f => f.FieldName == global::Umbraco.Cms.Core.Constants.IndexFieldNames.Level)?.Value.Integers?.SingleOrDefault();
            Assert.That(levelValue, Is.EqualTo(content.Level));

            var sortOrderValue = document.Fields.FirstOrDefault(f => f.FieldName == global::Umbraco.Cms.Core.Constants.IndexFieldNames.SortOrder)?.Value.Integers?.SingleOrDefault();
            Assert.That(sortOrderValue, Is.EqualTo(content.SortOrder));

            var objectTypeValue = document.Fields.FirstOrDefault(f => f.FieldName == global::Umbraco.Cms.Core.Constants.IndexFieldNames.ObjectType)?.Value.Keywords?.SingleOrDefault();
            Assert.That(objectTypeValue, Is.EqualTo(expectedObjectTypeValue));

            IEnumerable<string>? tagsValue = document.Fields.FirstOrDefault(f => f.FieldName == global::Umbraco.Cms.Core.Constants.IndexFieldNames.Tags)?.Value.Keywords;
            Assert.That(tagsValue ?? [], Is.EquivalentTo(tags));

            Assert.That(document.Protection, Is.Null);
        });
    }
}
