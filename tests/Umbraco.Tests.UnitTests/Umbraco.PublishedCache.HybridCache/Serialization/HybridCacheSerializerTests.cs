using System.Buffers;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache.Serialization;

/// <summary>
/// Provides tests to cover the <see cref="HybridCacheSerializer"/> class.
/// </summary>
/// <remarks>
/// A distributed (L2) cache can hold nodes written by an earlier version, and during a rolling upgrade servers on both
/// versions read each other's entries, so a member added to <see cref="ContentCacheNode"/> must read in both directions.
/// </remarks>
[TestFixture]
public class HybridCacheSerializerTests
{
    // The serializer's own options, used here to write and read the earlier node shape.
    private static readonly MessagePackSerializerOptions _options = ContractlessStandardResolver.Options
        .WithCompression(MessagePackCompression.Lz4BlockArray)
        .WithSecurity(MessagePackSecurity.UntrustedData);

    private readonly HybridCacheSerializer _serializer = new(NullLogger<HybridCacheSerializer>.Instance);

    [TestCase(ContentCacheDataSerializerEntityType.Document)]
    [TestCase(ContentCacheDataSerializerEntityType.Media)]
    [TestCase(ContentCacheDataSerializerEntityType.Element)]
    public void Round_Trips_The_Entity_Type(ContentCacheDataSerializerEntityType entityType)
    {
        var node = new ContentCacheNode { Id = 1234, Key = Guid.NewGuid(), EntityType = entityType };

        ContentCacheNode result = RoundTrip(node);

        Assert.Multiple(() =>
        {
            Assert.That(result.Key, Is.EqualTo(node.Key));
            Assert.That(result.EntityType, Is.EqualTo(entityType));
        });
    }

    [Test]
    public void Reads_A_Node_Written_Without_The_Entity_Type_As_Belonging_To_Any_Entity_Type()
    {
        var earlierNode = new NodeWithoutEntityType { Id = 1234, Key = Guid.NewGuid(), ContentTypeId = 5678, IsDraft = true };
        byte[] bytes = MessagePackSerializer.Serialize(earlierNode, _options);

        ContentCacheNode result = _serializer.Deserialize(new ReadOnlySequence<byte>(bytes));

        Assert.Multiple(() =>
        {
            Assert.That(result.Key, Is.EqualTo(earlierNode.Key));
            Assert.That(result.ContentTypeId, Is.EqualTo(earlierNode.ContentTypeId));
            Assert.That(result.IsDraft, Is.True);
            Assert.That(result.EntityType, Is.EqualTo(default(ContentCacheDataSerializerEntityType)));
            Assert.That(result.IsOtherEntityType(ContentCacheDataSerializerEntityType.Document), Is.False);
            Assert.That(result.IsOtherEntityType(ContentCacheDataSerializerEntityType.Media), Is.False);
            Assert.That(result.IsOtherEntityType(ContentCacheDataSerializerEntityType.Element), Is.False);
        });
    }

    [Test]
    public void Writes_A_Node_That_A_Reader_Without_The_Entity_Type_Can_Read()
    {
        var node = new ContentCacheNode { Id = 1234, Key = Guid.NewGuid(), ContentTypeId = 5678, EntityType = ContentCacheDataSerializerEntityType.Element };
        var writer = new ArrayBufferWriter<byte>();
        _serializer.Serialize(node, writer);

        NodeWithoutEntityType result = MessagePackSerializer.Deserialize<NodeWithoutEntityType>(writer.WrittenMemory, _options);

        Assert.Multiple(() =>
        {
            Assert.That(result.Key, Is.EqualTo(node.Key));
            Assert.That(result.ContentTypeId, Is.EqualTo(node.ContentTypeId));
        });
    }

    private ContentCacheNode RoundTrip(ContentCacheNode node)
    {
        var writer = new ArrayBufferWriter<byte>();
        _serializer.Serialize(node, writer);
        return _serializer.Deserialize(new ReadOnlySequence<byte>(writer.WrittenMemory));
    }

    /// <summary>
    /// The shape of <see cref="ContentCacheNode"/> before <see cref="ContentCacheNode.EntityType"/> was added.
    /// </summary>
    public sealed class NodeWithoutEntityType
    {
        public int Id { get; set; }

        public Guid Key { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreateDate { get; set; }

        public int CreatorId { get; set; }

        public int ContentTypeId { get; set; }

        public bool IsDraft { get; set; }

        public ContentData? Data { get; set; }
    }
}
