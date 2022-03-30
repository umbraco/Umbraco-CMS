using System;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Extensions
{
    public static class PublishedSnapshotAccessorExtensions
    {
        public static IPublishedSnapshot GetRequiredPublishedSnapshot(this IPublishedSnapshotAccessor publishedSnapshotAccessor)
        {
            if (publishedSnapshotAccessor.TryGetPublishedSnapshot(out var publishedSnapshot))
            {
                return publishedSnapshot!;
            }

            throw new InvalidOperationException("Wasn't possible to a get a valid Snapshot");
        }
    }
}
