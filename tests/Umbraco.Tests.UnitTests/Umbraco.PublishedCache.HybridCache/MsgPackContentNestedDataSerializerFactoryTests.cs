// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

[TestFixture]
public class MsgPackContentNestedDataSerializerFactoryTests
{
    private const string EditorAlias = "Umbraco.TextBox";
    private const int ContentTypeId = 1;

    private readonly IShortStringHelper _shortStringHelper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

    private List<IContentType> _contentTypes = null!;
    private List<IMediaType> _mediaTypes = null!;
    private Mock<IContentTypeService> _contentTypeService = null!;
    private Mock<IMediaTypeService> _mediaTypeService = null!;
    private Mock<IPropertyCacheCompressionOptions> _compressionOptions = null!;
    private MsgPackContentNestedDataSerializerFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _contentTypes = [CreateContentType(ContentTypeId, "page")];
        _mediaTypes = [];

        _contentTypeService = new Mock<IContentTypeService>();
        _contentTypeService.Setup(x => x.GetAll()).Returns(() => _contentTypes.ToArray());
        _mediaTypeService = new Mock<IMediaTypeService>();
        _mediaTypeService.Setup(x => x.GetAll()).Returns(() => _mediaTypes.ToArray());

        _compressionOptions = new Mock<IPropertyCacheCompressionOptions>();

        var editor = new Mock<IDataEditor>();
        editor.SetupGet(x => x.Alias).Returns(EditorAlias);

        _factory = new MsgPackContentNestedDataSerializerFactory(
            _contentTypeService.Object,
            _mediaTypeService.Object,
            new PropertyEditorCollection(new DataEditorCollection(() => [editor.Object])),
            _compressionOptions.Object);
    }

    [Test]
    public void Create_Does_Not_Reload_Types_For_Repeated_Calls()
    {
        for (var i = 0; i < 3; i++)
        {
            _factory.Create(ContentCacheDataSerializerEntityType.Document);
        }

        _contentTypeService.Verify(x => x.GetAll(), Times.Once);
    }

    [Test]
    public void Create_Only_Loads_Types_Of_The_Requested_Entity_Types()
    {
        _factory.Create(ContentCacheDataSerializerEntityType.Document);

        _mediaTypeService.Verify(x => x.GetAll(), Times.Never);

        _factory.Create(ContentCacheDataSerializerEntityType.Media);
        _factory.Create(ContentCacheDataSerializerEntityType.Document | ContentCacheDataSerializerEntityType.Media);

        _contentTypeService.Verify(x => x.GetAll(), Times.Once);
        _mediaTypeService.Verify(x => x.GetAll(), Times.Once);
    }

    [Test]
    public void Create_Reloads_Types_After_The_Type_Cache_Is_Cleared()
    {
        _factory.Create(ContentCacheDataSerializerEntityType.Document);

        _factory.ClearTypeCache();
        _factory.Create(ContentCacheDataSerializerEntityType.Document);

        _contentTypeService.Verify(x => x.GetAll(), Times.Exactly(2));
    }

    [Test]
    public void Create_Reflects_A_Type_Change_Made_After_The_Type_Cache_Was_Cleared()
    {
        const string AddedAlias = "added";
        SerializeWithProperty(AddedAlias);
        VerifyCompressionAsked(AddedAlias, Times.Never());

        _contentTypes[0] = CreateContentType(ContentTypeId, "page", AddedAlias);
        _factory.ClearTypeCache();

        SerializeWithProperty(AddedAlias);

        VerifyCompressionAsked(AddedAlias, Times.Once());
    }

    [Test]
    public void Create_Does_Not_Reflect_A_Type_Change_Until_The_Type_Cache_Is_Cleared()
    {
        const string AddedAlias = "added";
        SerializeWithProperty(AddedAlias);

        _contentTypes[0] = CreateContentType(ContentTypeId, "page", AddedAlias);
        SerializeWithProperty(AddedAlias);

        VerifyCompressionAsked(AddedAlias, Times.Never());
    }

    private void SerializeWithProperty(string propertyAlias)
    {
        var content = new Mock<IReadOnlyContentBase>();
        content.SetupGet(x => x.ContentTypeId).Returns(ContentTypeId);

        var model = new ContentCacheDataModel
        {
            PropertyData = new Dictionary<string, PropertyData[]>
            {
                [propertyAlias] = [new PropertyData { Value = "value" }],
            },
        };

        _factory
            .Create(ContentCacheDataSerializerEntityType.Document)
            .Serialize(content.Object, model, published: true);
    }

    private void VerifyCompressionAsked(string propertyAlias, Times times)
        => _compressionOptions.Verify(
            x => x.IsCompressed(
                It.IsAny<IReadOnlyContentBase>(),
                It.Is<IPropertyType>(propertyType => propertyType.Alias == propertyAlias),
                It.IsAny<IDataEditor>(),
                It.IsAny<bool>()),
            times);

    private IContentType CreateContentType(int id, string alias, params string[] propertyAliases)
    {
        var contentType = new ContentType(_shortStringHelper, -1) { Id = id, Key = Guid.NewGuid(), Alias = alias, Name = alias };
        foreach (var propertyAlias in propertyAliases)
        {
            contentType.AddPropertyType(
                new PropertyType(_shortStringHelper, EditorAlias, ValueStorageType.Nvarchar, propertyAlias) { Name = propertyAlias },
                "content",
                "Content");
        }

        return contentType;
    }
}
