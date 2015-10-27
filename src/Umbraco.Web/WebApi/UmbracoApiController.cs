using System.ComponentModel;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Validation;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// The base class for auto-routed API controllers for Umbraco
    /// </summary>
    public abstract class UmbracoApiController : UmbracoApiControllerBase
    {
        protected UmbracoApiController()
        {
        }

        protected UmbracoApiController(UmbracoContext umbracoContext) : base(umbracoContext)
        {
        }
    }
}
