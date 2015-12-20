namespace Umbraco.Web
{
    internal class DefaultUmbracoContextAccessor : IUmbracoContextAccessor
    {
        private readonly UmbracoContext _umbracoContext;

        public DefaultUmbracoContextAccessor(UmbracoContext umbracoContext)
        {
            _umbracoContext = umbracoContext;
        }

        public UmbracoContext Value
        {
            get { return _umbracoContext; }
        }
    }
}