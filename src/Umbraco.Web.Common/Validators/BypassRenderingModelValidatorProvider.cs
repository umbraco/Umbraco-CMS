using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Validators;

/// <summary>
/// Ensures we bypass property validation for rendering models.
/// </summary>
internal class BypassRenderingModelValidatorProvider : IModelValidatorProvider
{
    public void CreateValidators(ModelValidatorProviderContext context)
    {
        if (context.ModelMetadata.ModelType.IsRenderingModel())
        {
            context.Results.Clear();
            context.Results.Add(new ValidatorItem
            {
                Validator = null,
                IsReusable = true
            });
        }
    }
}


