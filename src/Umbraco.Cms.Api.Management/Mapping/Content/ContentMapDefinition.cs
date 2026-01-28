using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Content;

public abstract class ContentMapDefinition<TContent, TValueViewModel, TVariantViewModel>
    where TContent : IContentBase
    where TValueViewModel : ValueResponseModelBase, new()
    where TVariantViewModel : VariantResponseModelBase, new()
{
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly IDataValueEditorFactory _dataValueEditorFactory;

    protected ContentMapDefinition(
        PropertyEditorCollection propertyEditorCollection,
        IDataValueEditorFactory dataValueEditorFactory)
    {
        _propertyEditorCollection = propertyEditorCollection;
        _dataValueEditorFactory = dataValueEditorFactory;
    }

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 18.")]
    protected ContentMapDefinition(PropertyEditorCollection propertyEditorCollection)
        : this(
            propertyEditorCollection,
            StaticServiceProvider.Instance.GetRequiredService<IDataValueEditorFactory>())
    {
    }

    protected delegate void ValueViewModelMapping(IDataEditor propertyEditor, TValueViewModel variantViewModel);

    protected delegate void VariantViewModelMapping(string? culture, string? segment, TVariantViewModel variantViewModel);

    protected IEnumerable<TValueViewModel> MapValueViewModels(
        IEnumerable<IProperty> properties,
        ValueViewModelMapping? additionalPropertyMapping = null,
        bool published = false)
    {
        Dictionary<string, IDataEditor> missingPropertyEditors = [];
        return properties
            .SelectMany(property => property
                .Values
                .Select(propertyValue =>
                {
                    IDataEditor? propertyEditor = _propertyEditorCollection[property.PropertyType.PropertyEditorAlias];
                    if (propertyEditor is null && !missingPropertyEditors.TryGetValue(property.PropertyType.PropertyEditorAlias, out propertyEditor))
                    {
                        // We cache the missing property editors to avoid creating multiple instances of them
                        propertyEditor = new MissingPropertyEditor(property.PropertyType.PropertyEditorAlias, _dataValueEditorFactory);
                        missingPropertyEditors[property.PropertyType.PropertyEditorAlias] = propertyEditor;
                    }

                    IProperty? publishedProperty = null;
                    if (published)
                    {
                        publishedProperty = new Property(property.PropertyType);
                        publishedProperty.SetValue(
                            propertyValue.PublishedValue,
                            propertyValue.Culture,
                            propertyValue.Segment);
                    }

                    var variantViewModel = new TValueViewModel
                    {
                        Culture = propertyValue.Culture,
                        Segment = propertyValue.Segment,
                        Alias = property.Alias,
                        Value = propertyEditor.GetValueEditor().ToEditor(
                            publishedProperty ?? property,
                            propertyValue.Culture,
                            propertyValue.Segment),
                        EditorAlias = propertyEditor.Alias,
                    };
                    additionalPropertyMapping?.Invoke(propertyEditor, variantViewModel);
                    return variantViewModel;
                }))
            .WhereNotNull()
            .ToArray();
    }

    protected IEnumerable<TVariantViewModel> MapVariantViewModels(TContent source, VariantViewModelMapping? additionalVariantMapping = null)
    {
        IPropertyValue[] propertyValues = source.Properties.SelectMany(propertyCollection => propertyCollection.Values).ToArray();
        var cultures = source.AvailableCultures.DefaultIfEmpty(null).ToArray();
        // the default segment (null) must always be included in the view model - both for variant and invariant documents
        var segments = propertyValues.Select(property => property.Segment).Union([null]).Distinct().ToArray();

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

    protected void MapContentScheduleCollection<TContentResponseModel, TPublishableVariantResponseModelBase>(ContentScheduleCollection source, TContentResponseModel target, MapperContext context)
        where TContentResponseModel : ContentResponseModelBase<TValueViewModel, TPublishableVariantResponseModelBase>
        where TPublishableVariantResponseModelBase : PublishableVariantResponseModelBase, TVariantViewModel
    {
        foreach (ContentSchedule schedule in source.FullSchedule)
        {
            TPublishableVariantResponseModelBase? variant = target.Variants
                .FirstOrDefault(v =>
                    v.Culture == schedule.Culture ||
                    (IsInvariant(v.Culture) && IsInvariant(schedule.Culture)));
            if (variant is null)
            {
                continue;
            }

            switch (schedule.Action)
            {
                case ContentScheduleAction.Release:
                    variant.ScheduledPublishDate = new DateTimeOffset(schedule.Date, TimeSpan.Zero);
                    break;
                case ContentScheduleAction.Expire:
                    variant.ScheduledUnpublishDate = new DateTimeOffset(schedule.Date, TimeSpan.Zero);
                    break;
            }
        }
    }

    private static bool IsInvariant(string? culture) => culture.IsNullOrWhiteSpace() || culture == Core.Constants.System.InvariantCulture;
}
