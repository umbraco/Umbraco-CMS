using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal abstract class ContentEditingPresentationFactory<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
    protected TContentEditingModel MapContentEditingModel<TContentEditingModel>(
        ContentModelBase<TValueModel, TVariantModel> contentModel)
        where TContentEditingModel : ContentEditingModelBase, new()
        => new()
        {
            Properties = contentModel
                .Values
                .Select(value => new PropertyValueModel
                {
                    Alias = value.Alias, Value = value.Value, Culture = value.Culture, Segment = value.Segment
                }).ToArray(),
            Variants = contentModel
                .Variants
                .Select(variant => new VariantModel
                {
                    Culture = variant.Culture, Segment = variant.Segment, Name = variant.Name
                })
        };
}
