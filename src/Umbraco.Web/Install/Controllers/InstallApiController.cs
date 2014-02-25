using System;
using System.Web.Http;

namespace Umbraco.Web.Install.Controllers
{

    [HttpInstallAuthorize]
    public class InstallApiController : ApiController
    {
        protected InstallApiController()
            : this(UmbracoContext.Current)
        {

        }

        protected InstallApiController(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            UmbracoContext = umbracoContext;
        }

        /// <summary>
        /// Returns the current UmbracoContext
        /// </summary>
        public UmbracoContext UmbracoContext { get; private set; }

    }
}
