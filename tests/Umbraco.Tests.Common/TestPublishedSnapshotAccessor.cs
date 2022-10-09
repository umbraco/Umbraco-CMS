// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Tests.Common;

public class TestPublishedSnapshotAccessor : IPublishedSnapshotAccessor
{
    private IPublishedSnapshot _snapshot;

    public bool TryGetPublishedSnapshot(out IPublishedSnapshot publishedSnapshot)
    {
        publishedSnapshot = _snapshot;
        return _snapshot != null;
    }

    public void SetCurrent(IPublishedSnapshot snapshot) => _snapshot = snapshot;
}
