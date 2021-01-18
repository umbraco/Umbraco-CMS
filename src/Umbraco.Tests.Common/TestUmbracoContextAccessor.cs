// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Web;

namespace Umbraco.Tests.Common
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
