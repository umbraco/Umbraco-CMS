// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text;
using K4os.Compression.LZ4;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HybridCache;

[TestFixture]
public class ContentCacheNodeSizeEstimatorTests
{
    [Test]
    public void Can_Estimate_Base_Overhead_For_Node_Without_Data()
    {
        var node = new ContentCacheNode { Id = 1, Key = Guid.Empty, Data = null };

        Assert.That(ContentCacheNodeSizeEstimator.EstimateBytes(node), Is.GreaterThan(0));
    }

    [Test]
    public void Can_Estimate_Larger_Size_For_More_Content()
    {
        ContentCacheNode small = Node(("title", "a"));
        ContentCacheNode large = Node(("title", "a very much longer title value"), ("body", "additional content"));

        Assert.That(
            ContentCacheNodeSizeEstimator.EstimateBytes(large),
            Is.GreaterThan(ContentCacheNodeSizeEstimator.EstimateBytes(small)));
    }

    [Test]
    public void Can_Estimate_LazyCompressedString_Property_Without_Throwing()
    {
        var compressed = new LazyCompressedString(LZ4Pickler.Pickle(Encoding.UTF8.GetBytes("a compressed value")));
        ContentCacheNode node = Node(("body", compressed));

        long bytes = 0;
        Assert.DoesNotThrow(() => bytes = ContentCacheNodeSizeEstimator.EstimateBytes(node));
        Assert.That(bytes, Is.GreaterThan(0));
    }

    private static ContentCacheNode Node(params (string Alias, object? Value)[] properties)
    {
        var propertyData = new Dictionary<string, PropertyData[]>();
        foreach ((string alias, object? value) in properties)
        {
            propertyData[alias] = [new PropertyData { Culture = string.Empty, Segment = string.Empty, Value = value }];
        }

        var data = new ContentData("Test", "test", 1, new DateTime(2024, 1, 1), 0, null, true, propertyData, null);
        return new ContentCacheNode { Id = 1, Key = Guid.Empty, Data = data };
    }
}
