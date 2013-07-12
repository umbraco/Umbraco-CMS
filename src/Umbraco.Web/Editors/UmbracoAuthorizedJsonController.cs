using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// An abstract API controller that only supports JSON
    /// </summary>
    public abstract class UmbracoAuthorizedJsonController : UmbracoAuthorizedApiController
    {
        protected UmbracoAuthorizedJsonController()
        {
        }

        protected UmbracoAuthorizedJsonController(UmbracoContext umbracoContext) : base(umbracoContext)
        {
        }

        /// <summary>
        /// Remove the xml formatter... only support JSON!
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(global::System.Web.Http.Controllers.HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            controllerContext.Configuration.Formatters.Remove(controllerContext.Configuration.Formatters.XmlFormatter);
        }

    }
}