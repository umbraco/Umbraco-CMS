using Moq;
using Umbraco.Cms.Core.Models;
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
    protected IPublishedContentTypeCache GetPublishedContentTypeCache()
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

        var publishedContentTypeCacheMock = new Mock<IPublishedContentTypeCache>();
        publishedContentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Element, ContentKey1)).Returns(test1ContentType);
        publishedContentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Element, ContentKey2)).Returns(test2ContentType);
        publishedContentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Element, SettingKey1)).Returns(test3ContentType);
        publishedContentTypeCacheMock.Setup(x => x.Get(PublishedItemType.Element, SettingKey2)).Returns(test4ContentType);

        return publishedContentTypeCacheMock.Object;
    }

    protected IPublishedPropertyType GetPropertyType(TPropertyEditorConfig config)
    {
        var dataType = new PublishedDataType(1, "test", new Lazy<object>(() => config));
        var propertyType = Mock.Of<IPublishedPropertyType>(x =>
            x.EditorAlias == PropertyEditorAlias
            && x.DataType == dataType);
        return propertyType;
    }

    protected IPublishedElement GetPublishedElement()
        => Mock.Of<IPublishedElement>(m => m.ContentType == Mock.Of<IPublishedContentType>());
}
