using CSharpTest.Net.Serialization;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

internal class ContentNodeKitSerializer : ISerializer<ContentNodeKit>
{
    private static readonly ContentDataSerializer S_defaultDataSerializer = new();
    private readonly ContentDataSerializer? _contentDataSerializer;

    public ContentNodeKitSerializer(ContentDataSerializer? contentDataSerializer = null)
    {
        _contentDataSerializer = contentDataSerializer;
        if (_contentDataSerializer == null)
        {
            _contentDataSerializer = S_defaultDataSerializer;
        }
    }

    // static readonly ListOfIntSerializer ChildContentIdsSerializer = new ListOfIntSerializer();
    public ContentNodeKit ReadFrom(Stream stream)
    {
        var contentNode = new ContentNode(
            PrimitiveSerializer.Int32.ReadFrom(stream), // id
            PrimitiveSerializer.Guid.ReadFrom(stream), // uid
            PrimitiveSerializer.Int32.ReadFrom(stream), // level
            PrimitiveSerializer.String.ReadFrom(stream), // path
            PrimitiveSerializer.Int32.ReadFrom(stream), // sort order
            PrimitiveSerializer.Int32.ReadFrom(stream), // parent id
            PrimitiveSerializer.DateTime.ReadFrom(stream), // date created
            PrimitiveSerializer.Int32.ReadFrom(stream)); // creator id

        var contentTypeId = PrimitiveSerializer.Int32.ReadFrom(stream);
        var hasDraft = PrimitiveSerializer.Boolean.ReadFrom(stream);
        ContentData? draftData = null;
        ContentData? publishedData = null;
        if (hasDraft)
        {
            draftData = _contentDataSerializer?.ReadFrom(stream);
        }

        var hasPublished = PrimitiveSerializer.Boolean.ReadFrom(stream);
        if (hasPublished)
        {
            publishedData = _contentDataSerializer?.ReadFrom(stream);
        }

        var kit = new ContentNodeKit(
            contentNode,
            contentTypeId,
            draftData,
            publishedData);

        return kit;
    }

    public void WriteTo(ContentNodeKit value, Stream stream)
    {
        if (value.Node is not null)
        {
            PrimitiveSerializer.Int32.WriteTo(value.Node.Id, stream);
            PrimitiveSerializer.Guid.WriteTo(value.Node.Uid, stream);
            PrimitiveSerializer.Int32.WriteTo(value.Node.Level, stream);
            PrimitiveSerializer.String.WriteTo(value.Node.Path, stream);
            PrimitiveSerializer.Int32.WriteTo(value.Node.SortOrder, stream);
            PrimitiveSerializer.Int32.WriteTo(value.Node.ParentContentId, stream);
            PrimitiveSerializer.DateTime.WriteTo(value.Node.CreateDate, stream);
            PrimitiveSerializer.Int32.WriteTo(value.Node.CreatorId, stream);
            PrimitiveSerializer.Int32.WriteTo(value.ContentTypeId, stream);
        }

        PrimitiveSerializer.Boolean.WriteTo(value.DraftData != null, stream);
        if (value.DraftData != null)
        {
            _contentDataSerializer?.WriteTo(value.DraftData, stream);
        }

        PrimitiveSerializer.Boolean.WriteTo(value.PublishedData != null, stream);
        if (value.PublishedData != null)
        {
            _contentDataSerializer?.WriteTo(value.PublishedData, stream);
        }
    }
}
