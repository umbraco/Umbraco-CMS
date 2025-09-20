using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class BlockGridPropertyValueConverterTests : BlockPropertyValueConverterTestsBase<BlockGridConfiguration>
{
    protected override string PropertyEditorAlias => Constants.PropertyEditors.Aliases.BlockGrid;

    [Test]
    public void Get_Value_Type()
    {
        var editor = CreateConverter();
        var config = ConfigForSingle();
        var propertyType = GetPropertyType(config);

        var valueType = editor.GetPropertyValueType(propertyType);

        // the result is always block grid model
        Assert.AreEqual(typeof(BlockGridModel), valueType);
    }

    private BlockGridPropertyValueConverter CreateConverter()
    {
        var publishedSnapshotAccessor = GetPublishedSnapshotAccessor();
        var publishedModelFactory = new NoopPublishedModelFactory();
        var editor = new BlockGridPropertyValueConverter(
            Mock.Of<IProfilingLogger>(),
            new BlockEditorConverter(publishedSnapshotAccessor, publishedModelFactory),
            new JsonNetSerializer(),
            new ApiElementBuilder(Mock.Of<IOutputExpansionStrategyAccessor>()),
            new BlockGridPropertyValueConstructorCache());
        return editor;
    }

    private BlockGridConfiguration ConfigForSingle() => new()
    {
        Blocks = new[] { new BlockGridConfiguration.BlockGridBlockConfiguration { ContentElementTypeKey = ContentKey1 } },
    };
}
