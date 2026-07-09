using Examine;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Examine;

[TestFixture]
public class UmbracoIndexUniqueKeyFieldNameTests
{
    [Test]
    public void IUmbracoIndex_Default_UniqueKeyFieldName_Returns_NodeKeyFieldName()
    {
        // Stub that implements IUmbracoIndex without overriding UniqueKeyFieldName,
        // so the default interface implementation is exercised.
        IUmbracoIndex index = new StubUmbracoIndex();

        Assert.AreEqual(UmbracoExamineFieldNames.NodeKeyFieldName, index.UniqueKeyFieldName);
    }

    [Test]
    public void DeliveryApiContentIndex_ItemId_Constant_Matches_Expected_Value()
    {
        Assert.AreEqual("itemId", UmbracoExamineFieldNames.DeliveryApiContentIndex.ItemId);
    }

    /// <summary>
    /// Minimal stub that implements <see cref="IUmbracoIndex"/> without overriding
    /// <see cref="IUmbracoIndex.UniqueKeyFieldName"/>, so the default interface
    /// implementation is tested.
    /// </summary>
    private class StubUmbracoIndex : IUmbracoIndex
    {
        public bool EnableDefaultEventHandler => false;

        public bool PublishedValuesOnly => false;

        public bool SupportProtectedContent => false;

        public string Name => "Stub";

        public ISearcher Searcher => Mock.Of<ISearcher>();

        public ReadOnlyFieldDefinitionCollection FieldDefinitions => new(Array.Empty<FieldDefinition>());

        public void IndexItems(IEnumerable<ValueSet> values) { }

        public void DeleteFromIndex(IEnumerable<string> itemIds) { }

        public void CreateIndex() { }

        public bool IndexExists() => false;

        public long GetDocumentCount() => 0;

        public IEnumerable<string> GetFieldNames() => [];

        public event EventHandler<IndexOperationEventArgs>? IndexOperationComplete;

        public event EventHandler<IndexingItemEventArgs>? TransformingIndexValues;

        public event EventHandler<IndexingErrorEventArgs>? IndexingError;
    }
}
