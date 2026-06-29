using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Content;

/// <summary>
/// Serves as the base class for content update request models, providing generic parameters for value and variant models.
/// </summary>
/// <typeparam name="TValueModel">Specifies the type used for the content value model.</typeparam>
/// <typeparam name="TVariantModel">Specifies the type used for the content variant model.</typeparam>
public abstract class UpdateContentRequestModelBase<TValueModel, TVariantModel>
    : ContentModelBase<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
}
