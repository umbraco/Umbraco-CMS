using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     Validates the incoming <see cref="MemberSave" /> model
/// </summary>
internal sealed class MemberSaveValidationAttribute : TypeFilterAttribute
{
    public MemberSaveValidationAttribute() : base(typeof(MemberSaveValidationFilter))
    {
    }

    private sealed class MemberSaveValidationFilter : IActionFilter
    {
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMemberService _memberService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IPropertyValidationService _propertyValidationService;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly SecuritySettings _securitySettings;

        public MemberSaveValidationFilter(
            ILoggerFactory loggerFactory,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IMemberTypeService memberTypeService,
            IMemberService memberService,
            IShortStringHelper shortStringHelper,
            IPropertyValidationService propertyValidationService,
            IOptions<SecuritySettings> securitySettings)
        {
            _loggerFactory = loggerFactory;
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _memberTypeService = memberTypeService;
            _memberService = memberService;
            _shortStringHelper = shortStringHelper;
            _propertyValidationService = propertyValidationService;
            _securitySettings = securitySettings.Value;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var model = (MemberSave?)context.ActionArguments["contentItem"];
            var contentItemValidator = new MemberSaveModelValidator(
                _loggerFactory.CreateLogger<MemberSaveModelValidator>(),
                _backofficeSecurityAccessor.BackOfficeSecurity,
                _memberTypeService,
                _memberService,
                _shortStringHelper,
                _propertyValidationService,
                _securitySettings);
            //now do each validation step
            if (contentItemValidator.ValidateExistingContent(model, context))
            {
                if (contentItemValidator.ValidateProperties(model, model, context))
                {
                    contentItemValidator.ValidatePropertiesData(model, model, model?.PropertyCollectionDto, context.ModelState);
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
