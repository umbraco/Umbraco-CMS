// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Tests.Common
{
    public class TestLastChanceFinder : IContentLastChanceFinder
    {
        public bool TryFindContent(IPublishedRequestBuilder frequest) => false;
    }
}
