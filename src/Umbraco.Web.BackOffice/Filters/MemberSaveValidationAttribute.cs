﻿using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core.Logging;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// Validates the incoming <see cref="MemberSave"/> model
    /// </summary>
    internal sealed class MemberSaveValidationAttribute : TypeFilterAttribute
    {
        public MemberSaveValidationAttribute() : base(typeof(MemberSaveValidationFilter))
        {

        }

        private sealed class MemberSaveValidationFilter : IActionFilter
        {
            private readonly ILogger _logger;
            private readonly IBackofficeSecurityAccessor _backofficeSecurityAccessor;
            private readonly ILocalizedTextService _textService;
            private readonly IMemberTypeService _memberTypeService;
            private readonly IMemberService _memberService;
            private readonly IShortStringHelper _shortStringHelper;
            private readonly IPropertyValidationService _propertyValidationService;

            public MemberSaveValidationFilter(
                ILogger logger,
                IBackofficeSecurityAccessor backofficeSecurityAccessor,
                ILocalizedTextService textService,
                IMemberTypeService memberTypeService,
                IMemberService memberService,
                IShortStringHelper shortStringHelper,
                IPropertyValidationService propertyValidationService)
            {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
                _textService = textService ?? throw new ArgumentNullException(nameof(textService));
                _memberTypeService = memberTypeService ?? throw new ArgumentNullException(nameof(memberTypeService));
                _memberService = memberService  ?? throw new ArgumentNullException(nameof(memberService));
                _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
                _propertyValidationService = propertyValidationService ?? throw new ArgumentNullException(nameof(propertyValidationService));
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                var model = (MemberSave)context.ActionArguments["contentItem"];
                var contentItemValidator = new MemberSaveModelValidator(_logger, _backofficeSecurityAccessor.BackofficeSecurity, _textService, _memberTypeService, _memberService, _shortStringHelper, _propertyValidationService);
                //now do each validation step
                if (contentItemValidator.ValidateExistingContent(model, context))
                    if (contentItemValidator.ValidateProperties(model, model, context))
                        contentItemValidator.ValidatePropertiesData(model, model, model.PropertyCollectionDto, context.ModelState);
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }

        }
    }
}
