using Umbraco.Cms.Infrastructure.HybridCache.Serialization;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

/// <summary>
///     Produces a cheap, approximate byte size for a <see cref="ContentCacheNode" />, used to track the
///     retained size of the L0 converted-content cache.
/// </summary>
/// <remarks>
///     The estimate sums the node's stored string/property content without decompressing
///     <see cref="LazyCompressedString" /> values or walking the converted object graph, so it is safe to
///     call on the cache-insert path. It is an <em>underlying-content</em> figure and a lower bound on the
///     true managed heap cost — it deliberately ignores the (highly property-editor-dependent) blow-up from
///     converting stored values into their typed model. The true heap figure comes from a GC dump.
/// </remarks>
internal static class ContentCacheNodeSizeEstimator
{
    // Rough allowances for object headers and dictionary/array bookkeeping (x64).
    private const int BaseOverheadBytes = 64;
    private const int PerPropertyOverheadBytes = 24;

    public static long EstimateBytes(ContentCacheNode node)
    {
        long bytes = BaseOverheadBytes;

        ContentData? data = node.Data;
        if (data is null)
        {
            return bytes;
        }

        bytes += EstimateStringBytes(data.Name) + EstimateStringBytes(data.UrlSegment);

        foreach (KeyValuePair<string, PropertyData[]> property in data.Properties)
        {
            bytes += EstimateStringBytes(property.Key);
            foreach (PropertyData propertyData in property.Value)
            {
                bytes += PerPropertyOverheadBytes
                    + EstimateStringBytes(propertyData.Culture)
                    + EstimateStringBytes(propertyData.Segment)
                    + EstimateValueBytes(propertyData.Value);
            }
        }

        return bytes;
    }

    private static long EstimateStringBytes(string? value) => value is null ? 0 : value.Length * 2L;

    private static long EstimateValueBytes(object? value) => value switch
    {
        null => 0,
        LazyCompressedString lazyCompressedString => lazyCompressedString.GetApproximateByteCount(),
        string stringValue => stringValue.Length * 2L,
        byte[] bytes => bytes.Length,
        _ => 8,
    };
}
