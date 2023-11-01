using Moq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

public abstract class BlockPropertyValueConverterTestsBase<TPropertyEditorConfig>
{
    protected abstract string PropertyEditorAlias { get; }

    protected const string ContentAlias1 = "Test1";
    protected const string ContentAlias2 = "Test2";
    protected const string SettingAlias1 = "Setting1";
    protected const string SettingAlias2 = "Setting2";

    protected Guid ContentKey1 { get; } = Guid.NewGuid();

    protected Guid ContentKey2 { get; } = Guid.NewGuid();

    protected Guid SettingKey1 { get; } = Guid.NewGuid();

    protected Guid SettingKey2 { get; } = Guid.NewGuid();

    /// <summary>
    ///     Setup mocks for IPublishedSnapshotAccessor
    /// </summary>
    protected IPublishedSnapshotAccessor GetPublishedSnapshotAccessor()
    {
        var test1ContentType = Mock.Of<IPublishedContentType>(x =>
            x.IsElement == true
            && x.Key == ContentKey1
            && x.Alias == ContentAlias1);
        var test2ContentType = Mock.Of<IPublishedContentType>(x =>
            x.IsElement == true
            && x.Key == ContentKey2
            && x.Alias == ContentAlias2);
        var test3ContentType = Mock.Of<IPublishedContentType>(x =>
            x.IsElement == true
            && x.Key == SettingKey1
            && x.Alias == SettingAlias1);
        var test4ContentType = Mock.Of<IPublishedContentType>(x =>
            x.IsElement == true
            && x.Key == SettingKey2
            && x.Alias == SettingAlias2);
        var contentCache = new Mock<IPublishedContentCache>();
        contentCache.Setup(x => x.GetContentType(ContentKey1)).Returns(test1ContentType);
        contentCache.Setup(x => x.GetContentType(ContentKey2)).Returns(test2ContentType);
        contentCache.Setup(x => x.GetContentType(SettingKey1)).Returns(test3ContentType);
        contentCache.Setup(x => x.GetContentType(SettingKey2)).Returns(test4ContentType);
        var publishedSnapshot = Mock.Of<IPublishedSnapshot>(x => x.Content == contentCache.Object);
        var publishedSnapshotAccessor =
            Mock.Of<IPublishedSnapshotAccessor>(x => x.TryGetPublishedSnapshot(out publishedSnapshot));
        return publishedSnapshotAccessor;
    }

    protected IPublishedPropertyType GetPropertyType(TPropertyEditorConfig config)
    {
        var dataType = new PublishedDataType(1, "test", new Lazy<object>(() => config));
        var propertyType = Mock.Of<IPublishedPropertyType>(x =>
            x.EditorAlias == PropertyEditorAlias
            && x.DataType == dataType);
        return propertyType;
    }
}
