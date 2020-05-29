using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Umbraco.Web.BackOffice.Validation
{
    public class PrefixlessBodyModelValidator : ObjectModelValidator
    {
        public PrefixlessBodyModelValidator(IModelMetadataProvider modelMetadataProvider, IList<IModelValidatorProvider> validatorProviders) :
            base(modelMetadataProvider, validatorProviders)
        {

        }

        public override ValidationVisitor GetValidationVisitor(ActionContext actionContext, IModelValidatorProvider validatorProvider,
            ValidatorCache validatorCache, IModelMetadataProvider metadataProvider, ValidationStateDictionary validationState)
        {
            var visitor = new PrefixlessValidationVisitor(
                actionContext,
                validatorProvider,
                validatorCache,
                metadataProvider,
                validationState);

            return visitor;
        }

        private class PrefixlessValidationVisitor : ValidationVisitor
        {
            public PrefixlessValidationVisitor(ActionContext actionContext, IModelValidatorProvider validatorProvider, ValidatorCache validatorCache, IModelMetadataProvider metadataProvider, ValidationStateDictionary validationState)
                : base(actionContext, validatorProvider, validatorCache, metadataProvider, validationState) {

            }

            public override bool Validate(ModelMetadata metadata, string key, object model, bool alwaysValidateAtTopLevel)
            {
                return base.Validate(metadata, string.Empty, model, alwaysValidateAtTopLevel);
            }
        }
    }


}
