﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Extensions;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// Validator for <see cref="ContentItemSave"/>
    /// </summary>
    internal class MemberSaveModelValidator : ContentModelValidator<IMember, MemberSave, IContentProperties<ContentPropertyBasic>>
    {
        private readonly IMemberTypeService _memberTypeService;
        private readonly IMemberService _memberService;
        private readonly IShortStringHelper _shortStringHelper;

        public MemberSaveModelValidator(
            ILogger logger,
            IBackofficeSecurity backofficeSecurity,
            ILocalizedTextService textService,
            IMemberTypeService memberTypeService,
            IMemberService memberService,
            IShortStringHelper shortStringHelper,
            IPropertyValidationService propertyValidationService)
            : base(logger, backofficeSecurity, textService, propertyValidationService)
        {
            _memberTypeService = memberTypeService ?? throw new ArgumentNullException(nameof(memberTypeService));
            _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
            _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
        }

        public override bool ValidatePropertiesData(MemberSave model, IContentProperties<ContentPropertyBasic> modelWithProperties, ContentPropertyCollectionDto dto,
            ModelStateDictionary modelState)
        {
            if (model.Username.IsNullOrWhiteSpace())
            {
                modelState.AddPropertyError(
                    new ValidationResult("Invalid user name", new[] { "value" }),
                    $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}login");
            }

            if (model.Email.IsNullOrWhiteSpace() || new EmailAddressAttribute().IsValid(model.Email) == false)
            {
                modelState.AddPropertyError(
                    new ValidationResult("Invalid email", new[] { "value" }),
                    $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}email");
            }

            var validEmail = ValidateUniqueEmail(model);
            if (validEmail == false)
            {
                modelState.AddPropertyError(
                    new ValidationResult("Email address is already in use", new[] { "value" }),
                    $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}email");
            }

            var validLogin = ValidateUniqueLogin(model);
            if (validLogin == false)
            {
                modelState.AddPropertyError(
                    new ValidationResult("Username is already in use", new[] { "value" }),
                    $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}login");
            }

            return base.ValidatePropertiesData(model, modelWithProperties, dto, modelState);
        }

           /// <summary>
        /// This ensures that the internal membership property types are removed from validation before processing the validation
        /// since those properties are actually mapped to real properties of the IMember.
        /// This also validates any posted data for fields that are sensitive.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="modelWithProperties"></param>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        public override bool ValidateProperties(MemberSave model, IContentProperties<ContentPropertyBasic> modelWithProperties, ActionExecutingContext actionContext)
        {
            var propertiesToValidate = model.Properties.ToList();
            var defaultProps = ConventionsHelper.GetStandardPropertyTypeStubs(_shortStringHelper);
            var exclude = defaultProps.Select(x => x.Value.Alias).ToArray();
            foreach (var remove in exclude)
            {
                propertiesToValidate.RemoveAll(property => property.Alias == remove);
            }

            //if the user doesn't have access to sensitive values, then we need to validate the incoming properties to check
            //if a sensitive value is being submitted.
            if (BackofficeSecurity.CurrentUser.HasAccessToSensitiveData() == false)
            {
                var contentType = _memberTypeService.Get(model.PersistedContent.ContentTypeId);
                var sensitiveProperties = contentType
                    .PropertyTypes.Where(x => contentType.IsSensitiveProperty(x.Alias))
                    .ToList();

                foreach (var sensitiveProperty in sensitiveProperties)
                {
                    var prop = propertiesToValidate.FirstOrDefault(x => x.Alias == sensitiveProperty.Alias);

                    if (prop != null)
                    {
                        //this should not happen, this means that there was data posted for a sensitive property that
                        //the user doesn't have access to, which means that someone is trying to hack the values.

                        var message = $"property with alias: {prop.Alias} cannot be posted";
                        actionContext.Result = new NotFoundObjectResult(new InvalidOperationException(message));
                        return false;
                    }
                }
            }

            return ValidateProperties(propertiesToValidate, model.PersistedContent.Properties.ToList(), actionContext);
        }

        internal bool ValidateUniqueLogin(MemberSave model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var existingByName = _memberService.GetByUsername(model.Username.Trim());
            switch (model.Action)
            {
                case ContentSaveAction.Save:

                    //ok, we're updating the member, we need to check if they are changing their login and if so, does it exist already ?
                    if (model.PersistedContent.Username.InvariantEquals(model.Username.Trim()) == false)
                    {
                        //they are changing their login name
                        if (existingByName != null && existingByName.Username == model.Username.Trim())
                        {
                            //the user cannot use this login
                            return false;
                        }
                    }
                    break;
                case ContentSaveAction.SaveNew:
                    //check if the user's login already exists
                    if (existingByName != null && existingByName.Username == model.Username.Trim())
                    {
                        //the user cannot use this login
                        return false;
                    }
                    break;
                default:
                    //we don't support this for members
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        internal bool ValidateUniqueEmail(MemberSave model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var existingByEmail = _memberService.GetByEmail(model.Email.Trim());
            switch (model.Action)
            {
                case ContentSaveAction.Save:
                    //ok, we're updating the member, we need to check if they are changing their email and if so, does it exist already ?
                    if (model.PersistedContent.Email.InvariantEquals(model.Email.Trim()) == false)
                    {
                        //they are changing their email
                        if (existingByEmail != null && existingByEmail.Email.InvariantEquals(model.Email.Trim()))
                        {
                            //the user cannot use this email
                            return false;
                        }
                    }
                    break;
                case ContentSaveAction.SaveNew:
                    //check if the user's email already exists
                    if (existingByEmail != null && existingByEmail.Email.InvariantEquals(model.Email.Trim()))
                    {
                        //the user cannot use this email
                        return false;
                    }
                    break;
                default:
                    //we don't support this for members
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }
    }
}
