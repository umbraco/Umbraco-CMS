using System.Buffers;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Hybrid;

namespace Umbraco.Cms.Infrastructure.HybridCache.Serialization;

internal class HybridCacheSerializer : IHybridCacheSerializer<ContentCacheNode>
{
    private readonly MessagePackSerializerOptions _options;

    public HybridCacheSerializer()
    {
        MessagePackSerializerOptions defaultOptions = ContractlessStandardResolver.Options;
        IFormatterResolver resolver = CompositeResolver.Create(defaultOptions.Resolver);

        _options = defaultOptions
            .WithResolver(resolver)
            .WithCompression(MessagePackCompression.Lz4BlockArray)
            .WithSecurity(MessagePackSecurity.UntrustedData);
    }

    public ContentCacheNode Deserialize(ReadOnlySequence<byte> source) => MessagePackSerializer.Deserialize<ContentCacheNode>(source, _options);

    public void Serialize(ContentCacheNode value, IBufferWriter<byte> target) => target.Write(MessagePackSerializer.Serialize(value, _options));
}
