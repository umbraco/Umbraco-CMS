// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Tests.Common
{
    public class TestUmbracoContextAccessor : IUmbracoContextAccessor
    {
        public IUmbracoContext UmbracoContext { get; set; }

        public TestUmbracoContextAccessor()
        {
        }

        public TestUmbracoContextAccessor(IUmbracoContext umbracoContext)
        {
            UmbracoContext = umbracoContext;
        }
    }
}
