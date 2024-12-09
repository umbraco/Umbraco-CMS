using System.Buffers;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Infrastructure.HybridCache.Serialization;

internal class HybridCacheSerializer : IHybridCacheSerializer<ContentCacheNode>
{
    private readonly ILogger<HybridCacheSerializer> _logger;
    private readonly MessagePackSerializerOptions _options;

    public HybridCacheSerializer(ILogger<HybridCacheSerializer> logger)
    {
        _logger = logger;
        MessagePackSerializerOptions defaultOptions = ContractlessStandardResolver.Options;
        IFormatterResolver resolver = CompositeResolver.Create(defaultOptions.Resolver);

        _options = defaultOptions
            .WithResolver(resolver)
            .WithCompression(MessagePackCompression.Lz4BlockArray)
            .WithSecurity(MessagePackSecurity.UntrustedData);
    }

    public ContentCacheNode Deserialize(ReadOnlySequence<byte> source)
    {
        try
        {
            return MessagePackSerializer.Deserialize<ContentCacheNode>(source, _options);
        }
        catch (MessagePackSerializationException ex)
        {
            _logger.LogError(ex, "Error deserializing ContentCacheNode");
            return null!;
        }
    }

    public void Serialize(ContentCacheNode value, IBufferWriter<byte> target) => target.Write(MessagePackSerializer.Serialize(value, _options));
}
