using System;
using System.Web;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Umbraco.Web.WebApi
{
    public abstract class UmbracoApiController : ApiController
    {
        protected UmbracoApiController()
            : this(UmbracoContext.Current)
        {

        }

        protected UmbracoApiController(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            UmbracoContext = umbracoContext;
            InstanceId = Guid.NewGuid();
            Umbraco = new UmbracoHelper(umbracoContext);
        }

        /// <summary>
        /// Tries to retreive the current HttpContext if one exists.
        /// </summary>
        /// <returns></returns>
        protected Attempt<HttpContextBase> TryGetHttpContext()
        {
            object context;
            if (Request.Properties.TryGetValue("MS_HttpContext", out context))
            {
                var httpContext = context as HttpContextBase;
                if (httpContext != null)
                {
                    return new Attempt<HttpContextBase>(true, httpContext);
                }
            }
            return Attempt<HttpContextBase>.False;
        }

        /// <summary>
        /// Returns the current ApplicationContext
        /// </summary>
        public ApplicationContext ApplicationContext
        {
            get { return UmbracoContext.Application; }
        }

        /// <summary>
        /// Returns a ServiceContext
        /// </summary>
        public ServiceContext Services
        {
            get { return ApplicationContext.Services; }
        }

        /// <summary>
        /// Returns a DatabaseContext
        /// </summary>
        public DatabaseContext DatabaseContext
        {
            get { return ApplicationContext.DatabaseContext; }
        }

        /// <summary>
        /// Returns an UmbracoHelper object
        /// </summary>
        public UmbracoHelper Umbraco { get; private set; }

        /// <summary>
        /// Returns the current UmbracoContext
        /// </summary>
        public UmbracoContext UmbracoContext { get; private set; }

        /// <summary>
        /// Useful for debugging
        /// </summary>
        internal Guid InstanceId { get; private set; }
    }
}