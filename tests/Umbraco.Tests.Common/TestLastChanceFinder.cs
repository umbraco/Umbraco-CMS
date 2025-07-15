// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Threading.Tasks;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Tests.Common;

public class TestLastChanceFinder : IContentLastChanceFinder
{
    public Task<bool> TryFindContent(IPublishedRequestBuilder frequest) => Task.FromResult(false);
}
