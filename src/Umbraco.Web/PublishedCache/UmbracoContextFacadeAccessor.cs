using System;

namespace Umbraco.Web.PublishedCache
{
    public class UmbracoContextFacadeAccessor : IFacadeAccessor
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public UmbracoContextFacadeAccessor(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public IFacade Facade
        {
            get
            {
                var umbracoContext = _umbracoContextAccessor.UmbracoContext;
                if (umbracoContext == null) throw new Exception("The IUmbracoContextAccessor could not provide an UmbracoContext.");
                return umbracoContext.Facade;
            }

            set
            {
                throw new NotSupportedException(); // not ok to set
            }
        }
    }
}