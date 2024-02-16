﻿using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Content;

public abstract class ContentMapDefinition<TContent, TValueViewModel, TVariantViewModel>
    where TContent : IContentBase
    where TValueViewModel : ValueModelBase, new()
    where TVariantViewModel : VariantResponseModelBase, new()
{
    private readonly PropertyEditorCollection _propertyEditorCollection;

    protected ContentMapDefinition(PropertyEditorCollection propertyEditorCollection) => _propertyEditorCollection = propertyEditorCollection;

    protected delegate void ValueViewModelMapping(IDataEditor propertyEditor, TValueViewModel variantViewModel);

    protected delegate void VariantViewModelMapping(string? culture, string? segment, TVariantViewModel variantViewModel);

    protected IEnumerable<TValueViewModel> MapValueViewModels(IEnumerable<IProperty> properties, ValueViewModelMapping? additionalPropertyMapping = null) =>
        properties
            .SelectMany(property => property
                .Values
                .Select(propertyValue =>
                {
                    IDataEditor? propertyEditor = _propertyEditorCollection[property.PropertyType.PropertyEditorAlias];
                    if (propertyEditor == null)
                    {
                        return null;
                    }

                    var variantViewModel = new TValueViewModel
                    {
                        Culture = propertyValue.Culture,
                        Segment = propertyValue.Segment,
                        Alias = property.Alias,
                        Value = propertyEditor.GetValueEditor().ToEditor(property, propertyValue.Culture, propertyValue.Segment)
                    };
                    additionalPropertyMapping?.Invoke(propertyEditor, variantViewModel);
                    return variantViewModel;
                }))
            .WhereNotNull()
            .ToArray();

    protected IEnumerable<TVariantViewModel> MapVariantViewModels(TContent source, VariantViewModelMapping? additionalVariantMapping = null)
    {
        IPropertyValue[] propertyValues = source.Properties.SelectMany(propertyCollection => propertyCollection.Values).ToArray();
        var cultures = source.AvailableCultures.DefaultIfEmpty(null).ToArray();
        var segments = propertyValues.Select(property => property.Segment).Distinct().DefaultIfEmpty(null).ToArray();

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
