// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Tests.Common
{
    public class TestPublishedSnapshotAccessor : IPublishedSnapshotAccessor
    {
        public bool TryGetPublishedSnapshot(out IPublishedSnapshot publishedSnapshot)
        {
            publishedSnapshot = null;
            return false;
        }
    }
}
