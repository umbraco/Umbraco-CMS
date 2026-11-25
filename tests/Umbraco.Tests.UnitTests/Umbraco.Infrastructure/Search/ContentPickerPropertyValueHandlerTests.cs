using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Infrastructure.Search.PropertyValueHandlers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Search;

[TestFixture]
internal sealed class ContentPickerPropertyValueHandlerTests
{
    private static readonly Guid _first = new("0f4f9a0e-6b1e-4d0f-9b1c-2a3f6e5d4c3b");
    private static readonly Guid _second = new("8c2d7e61-3a4b-4c5d-8e9f-0a1b2c3d4e5f");

    [Test]
    public void Stored_Udi_Is_Indexed_As_Its_Key()
        => Assert.That(IndexedKeywords(new GuidUdi(Constants.UdiEntityType.Document, _second).ToString()), Is.EqualTo(new[] { _second.AsKeyword() }));

    [Test]
    public void Stored_Udi_List_Is_Indexed_As_Its_First_Key()
        => Assert.That(
            IndexedKeywords($"{new GuidUdi(Constants.UdiEntityType.Document, _first)},{new GuidUdi(Constants.UdiEntityType.Document, _second)}"),
            Is.EqualTo(new[] { _first.AsKeyword() }));

    [Test]
    public void Empty_Value_Yields_No_Field()
        => Assert.That(IndexFields(string.Empty), Is.Empty);

    private static IEnumerable<string>? IndexedKeywords(string storedValue)
        => IndexFields(storedValue).Single().Value.Keywords;

    private static IndexField[] IndexFields(string storedValue)
    {
        var property = new Mock<IProperty>();
        property.SetupGet(p => p.Alias).Returns("picker");
        property
            .Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns(storedValue);

        return new ContentPickerPropertyValueHandler()
            .GetIndexFields(property.Object, null, null, true, Mock.Of<IContentBase>())
            .ToArray();
    }
}
