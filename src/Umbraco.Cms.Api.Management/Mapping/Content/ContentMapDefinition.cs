using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Content;

public abstract class ContentMapDefinition<TContent, TPropertyViewModel, TVariantViewModel>
    where TContent : IContentBase
    where TPropertyViewModel : PropertyViewModelBase, new()
    where TVariantViewModel : VariantViewModelBase, new()
{
    private readonly PropertyEditorCollection _propertyEditorCollection;

    protected ContentMapDefinition(PropertyEditorCollection propertyEditorCollection) => _propertyEditorCollection = propertyEditorCollection;

    protected delegate void PropertyViewModelMapping(IDataEditor propertyEditor, TPropertyViewModel variantViewModel);

    protected delegate void VariantViewModelMapping(string? culture, string? segment, TVariantViewModel variantViewModel);

    protected IEnumerable<TPropertyViewModel> MapPropertyViewModels(TContent source, PropertyViewModelMapping? additionalPropertyMapping = null) =>
        source
            .Properties
            .SelectMany(property => property
                .Values
                .Select(propertyValue =>
                {
                    IDataEditor? propertyEditor = _propertyEditorCollection[property.PropertyType.PropertyEditorAlias];
                    if (propertyEditor == null)
                    {
                        return null;
                    }

                    var propertyViewModel = new TPropertyViewModel
                    {
                        Culture = propertyValue.Culture,
                        Segment = propertyValue.Segment,
                        Alias = property.Alias,
                        Value = propertyEditor.GetValueEditor().ToEditor(property, propertyValue.Culture, propertyValue.Segment)
                    };
                    additionalPropertyMapping?.Invoke(propertyEditor, propertyViewModel);
                    return propertyViewModel;
                }))
            .WhereNotNull()
            .ToArray();

    protected IEnumerable<TVariantViewModel> MapVariantViewModels(TContent source, VariantViewModelMapping? additionalVariantMapping = null)
    {
        IPropertyValue[] propertyValues = source.Properties.SelectMany(propertyCollection => propertyCollection.Values).ToArray();
        var cultures = source.AvailableCultures.DefaultIfEmpty(null).ToArray();
        var segments = propertyValues.Select(property => property.Segment).Distinct().ToArray();

        return cultures
            .SelectMany(culture => segments.Select(segment =>
            {
                var variantViewModel = new TVariantViewModel
                {
                    Culture = culture,
                    Segment = segment,
                    Name = source.GetCultureName(culture) ?? string.Empty,
                    CreateDate = source.CreateDate, // apparently there is no culture specific creation date
                    UpdateDate = culture == null
                        ? source.UpdateDate
                        : source.GetUpdateDate(culture) ?? source.UpdateDate,
                };
                additionalVariantMapping?.Invoke(culture, segment, variantViewModel);
                return variantViewModel;
            }))
            .ToArray();
    }
}
