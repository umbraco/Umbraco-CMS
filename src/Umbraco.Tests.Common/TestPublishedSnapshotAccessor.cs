// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.Common
{
    public class TestPublishedSnapshotAccessor : IPublishedSnapshotAccessor
    {
        public IPublishedSnapshot PublishedSnapshot { get; set; }
    }
}
