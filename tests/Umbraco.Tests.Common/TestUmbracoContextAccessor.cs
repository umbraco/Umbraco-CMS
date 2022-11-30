// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Tests.Common;

public class TestUmbracoContextAccessor : IUmbracoContextAccessor
{
    private IUmbracoContext _umbracoContext;

    public TestUmbracoContextAccessor()
    {
    }

    public TestUmbracoContextAccessor(IUmbracoContext umbracoContext) => _umbracoContext = umbracoContext;

    public bool TryGetUmbracoContext(out IUmbracoContext umbracoContext)
    {
        umbracoContext = _umbracoContext;
        return umbracoContext is not null;
    }

    public void Clear() => _umbracoContext = null;
    public void Set(IUmbracoContext umbracoContext) => _umbracoContext = umbracoContext;
}
