using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Validators;

/// <summary>
/// Ensures we bypass object graph validation for rendering models.
/// </summary>
internal class BypassRenderingModelValidationMetadataProvider : IValidationMetadataProvider
{
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        if (context.Key.ModelType.IsRenderingModel())
        {
            context.ValidationMetadata.ValidateChildren = false;
        }
    }
}
