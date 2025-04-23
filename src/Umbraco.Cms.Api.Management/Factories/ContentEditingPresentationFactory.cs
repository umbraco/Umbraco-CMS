using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal abstract class ContentEditingPresentationFactory<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
    protected TContentEditingModel MapContentEditingModel<TContentEditingModel>(ContentModelBase<TValueModel, TVariantModel> contentModel)
        where TContentEditingModel : ContentEditingModelBase, new()
    {
        TVariantModel? invariantVariant = contentModel.Variants.FirstOrDefault(variant => variant.DoesNotVaryByCulture() && variant.DoesNotVaryBySegment());
        TValueModel[] invariantProperties = contentModel.Values.Where(value => value.DoesNotVaryByCulture() && value.DoesNotVaryBySegment()).ToArray();

        PropertyValueModel ToPropertyValueModel(TValueModel valueModel)
            => new() { Alias = valueModel.Alias, Value = valueModel.Value };

        return new TContentEditingModel
        {
            InvariantName = invariantVariant?.Name,
            InvariantProperties = invariantProperties.Select(ToPropertyValueModel).ToArray(),
            Variants = contentModel
                .Variants
                .Select(variant => new VariantModel
                {
                    Culture = variant.Culture,
                    Segment = variant.Segment,
                    Name = variant.Name,
                    Properties = contentModel
                        .Values
                        .Where(value => value.Culture == variant.Culture && value.Segment == variant.Segment)
                        .Select(ToPropertyValueModel).ToArray()
                })
        };
    }
}
