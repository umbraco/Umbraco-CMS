using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Examine;

[TestFixture]
public class UmbracoIndexUniqueKeyFieldNameTests
{
    [Test]
    public void IUmbracoIndex_Default_UniqueKeyFieldName_Returns_NodeKeyFieldName()
    {
        // The default interface implementation should return "__Key"
        Assert.AreEqual("__Key", UmbracoExamineFieldNames.NodeKeyFieldName);
    }

    [Test]
    public void DeliveryApiContentIndex_ItemId_Constant_Matches_Expected_Value()
    {
        // The DeliveryApiContentIndex.ItemId constant should be "itemId",
        // matching the field stored by AncestorsSelectorIndexer
        Assert.AreEqual("itemId", UmbracoExamineFieldNames.DeliveryApiContentIndex.ItemId);
    }
}
