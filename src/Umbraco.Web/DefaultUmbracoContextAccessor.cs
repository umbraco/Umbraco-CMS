using System;

namespace Umbraco.Web
{
    internal class DefaultUmbracoContextAccessor : IUmbracoContextAccessor
    {
        private readonly Func<UmbracoContext> _umbracoContext;

        public DefaultUmbracoContextAccessor(Func<UmbracoContext> umbracoContext)
        {
            _umbracoContext = umbracoContext;
        }

        public UmbracoContext Value
        {
            get { return _umbracoContext(); }
        }
    }
}