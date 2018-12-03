using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors.Filters
{
    /// <summary>
    /// Validates the incoming <see cref="MemberSave"/> model
    /// </summary>
    internal class MemberSaveValidationAttribute : ActionFilterAttribute
    {
        private readonly ILogger _logger;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public MemberSaveValidationAttribute() : this(Current.Logger, Current.UmbracoContextAccessor)
        {
        }

        public MemberSaveValidationAttribute(ILogger logger, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _logger = logger;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var model = (MemberSave)actionContext.ActionArguments["contentItem"];
            var contentItemValidator = new MemberValidationHelper(_logger, _umbracoContextAccessor);
            //now do each validation step
            if (contentItemValidator.ValidateExistingContent(model, actionContext))
                if (contentItemValidator.ValidateProperties(model, model, actionContext))
                    contentItemValidator.ValidatePropertyData(model, model, model.PropertyCollectionDto, actionContext.ModelState);
        }
    }
}
