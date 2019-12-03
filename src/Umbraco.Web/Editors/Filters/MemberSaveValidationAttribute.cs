using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
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
        private readonly ILocalizedTextService _textService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IMemberService _memberService;

        public MemberSaveValidationAttribute()
            : this(Current.Logger, Current.UmbracoContextAccessor, Current.Services.TextService, Current.Services.MemberTypeService, Current.Services.MemberService)
        { }

        public MemberSaveValidationAttribute(ILogger logger, IUmbracoContextAccessor umbracoContextAccessor, ILocalizedTextService textService, IMemberTypeService memberTypeService, IMemberService memberService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _textService = textService ?? throw new ArgumentNullException(nameof(textService));
            _memberTypeService = memberTypeService ?? throw new ArgumentNullException(nameof(memberTypeService));
            _memberService = memberService  ?? throw new ArgumentNullException(nameof(memberService));;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var model = (MemberSave)actionContext.ActionArguments["contentItem"];
            var contentItemValidator = new MemberSaveModelValidator(_logger, _umbracoContextAccessor,_textService, _memberTypeService, _memberService);
            //now do each validation step
            if (contentItemValidator.ValidateExistingContent(model, actionContext))
                if (contentItemValidator.ValidateProperties(model, model, actionContext))
                    contentItemValidator.ValidatePropertiesData(model, model, model.PropertyCollectionDto, actionContext.ModelState);
        }
    }
}
