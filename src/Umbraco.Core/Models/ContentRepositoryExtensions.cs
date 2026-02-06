using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods used to manipulate content variations by the document repository
/// </summary>
public static class ContentRepositoryExtensions
{
    /// <summary>
    ///     Sets the culture information for a specific culture on the content item.
    /// </summary>
    /// <param name="content">The content item to update.</param>
    /// <param name="culture">The culture code (e.g., "en-US").</param>
    /// <param name="name">The name of the content in the specified culture.</param>
    /// <param name="date">The date to associate with this culture information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name" /> or <paramref name="culture" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name" /> or <paramref name="culture" /> is empty or whitespace.</exception>
    public static void SetCultureInfo(this IContentBase content, string? culture, string? name, DateTime date)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(name));
        }

        if (culture == null)
        {
            throw new ArgumentNullException(nameof(culture));
        }

        if (string.IsNullOrWhiteSpace(culture))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(culture));
        }

        content.CultureInfos?.AddOrUpdate(culture, name, date);
    }

    /// <summary>
    ///     Updates a culture date, if the culture exists.
    /// </summary>
    public static void TouchCulture(this IContentBase content, string? culture)
    {
        if (culture.IsNullOrWhiteSpace() || content.CultureInfos is null)
        {
            return;
        }

        if (!content.CultureInfos.TryGetValue(culture!, out ContentCultureInfos infos))
        {
            return;
        }

        content.CultureInfos?.AddOrUpdate(culture!, infos.Name, DateTime.UtcNow);
    }

    /// <summary>
    ///     Used to synchronize all culture dates to the same date if they've been modified
    /// </summary>
    /// <param name="content"></param>
    /// <param name="date"></param>
    /// <param name="publishing"></param>
    /// <remarks>
    ///     This is so that in an operation where (for example) 2 languages are updates like french and english, it is possible
    ///     that
    ///     these dates assigned to them differ by a couple of Ticks, but we need to ensure they are persisted at the exact
    ///     same time.
    /// </remarks>
    public static void AdjustDates(this IPublishableContentBase content, DateTime date, bool publishing)
    {
        if (content.EditedCultures is not null)
        {
            foreach (var culture in content.EditedCultures.ToList())
            {
                if (content.CultureInfos is null)
                {
                    continue;
                }

                if (!content.CultureInfos.TryGetValue(culture, out ContentCultureInfos editedInfos))
                {
                    continue;
                }

                // if it's not dirty, it means it hasn't changed so there's nothing to adjust
                if (!editedInfos.IsDirty())
                {
                    continue;
                }

                content.CultureInfos?.AddOrUpdate(culture, editedInfos?.Name, date);
            }
        }

        if (!publishing)
        {
            return;
        }

        foreach (var culture in content.PublishedCultures.ToList())
        {
            if (content.PublishCultureInfos is null)
            {
                continue;
            }

            if (!content.PublishCultureInfos.TryGetValue(culture, out ContentCultureInfos publishInfos))
            {
                continue;
            }

            // if it's not dirty, it means it hasn't changed so there's nothing to adjust
            if (!publishInfos.IsDirty())
            {
                continue;
            }

            content.PublishCultureInfos.AddOrUpdate(culture, publishInfos.Name, date);

            if (content.CultureInfos?.TryGetValue(culture, out ContentCultureInfos infos) ?? false)
            {
                SetCultureInfo(content, culture, infos.Name, date);
            }
        }
    }

    /// <summary>
    ///     Gets the cultures that have been flagged for unpublishing.
    /// </summary>
    /// <remarks>Gets cultures for which content.UnpublishCulture() has been invoked.</remarks>
    public static IReadOnlyList<string>? GetCulturesUnpublishing(this IPublishableContentBase content)
    {
        if (!content.Published || !content.ContentType.VariesByCulture() ||
            !content.IsPropertyDirty("PublishCultureInfos"))
        {
            return Array.Empty<string>();
        }

        IEnumerable<string>? culturesUnpublishing = content.CultureInfos?.Values
            .Where(x => content.IsPropertyDirty(ContentBase.ChangeTrackingPrefix.UnpublishedCulture + x.Culture))
            .Select(x => x.Culture);

        return culturesUnpublishing?.ToList();
    }

    /// <summary>
    ///     Copies values from another document.
    /// </summary>
    public static void CopyFrom(this IPublishableContentBase content, IPublishableContentBase other, string? culture = "*")
    {
        if (other.ContentTypeId != content.ContentTypeId)
        {
            throw new InvalidOperationException("Cannot copy values from a different content type.");
        }

        culture = culture?.ToLowerInvariant().NullOrWhiteSpaceAsNull();

        // the variation should be supported by the content type properties
        //  if the content type is invariant, only '*' and 'null' is ok
        //  if the content type varies, everything is ok because some properties may be invariant
        if (!content.ContentType.SupportsPropertyVariation(culture, "*", true))
        {
            throw new NotSupportedException(
                $"Culture \"{culture}\" is not supported by content type \"{content.ContentType.Alias}\" with variation \"{content.ContentType.Variations}\".");
        }

        // copying from the same Id and VersionPk
        var copyingFromSelf = content.Id == other.Id && content.VersionId == other.VersionId;
        var published = copyingFromSelf;

        // note: use property.SetValue(), don't assign pvalue.EditValue, else change tracking fails

        // clear all existing properties for the specified culture
        foreach (IProperty property in content.Properties)
        {
            // each property type may or may not support the variation
            if ((!property.PropertyType?.SupportsVariation(culture, "*", true) ?? false) &&
                    !(property.PropertyType?.Variations == ContentVariation.Nothing))
            {
                continue;
            }

            foreach (IPropertyValue pvalue in property.Values)
            {
                if (((property.PropertyType?.SupportsVariation(pvalue.Culture, pvalue.Segment, true) ?? false) &&
                    (culture == "*" || (pvalue.Culture?.InvariantEquals(culture) ?? false))) ||
                    property.PropertyType?.Variations == ContentVariation.Nothing)
                {
                    property.SetValue(null, pvalue.Culture, pvalue.Segment);
                }
            }
        }

        // copy properties from 'other'
        IPropertyCollection otherProperties = other.Properties;
        foreach (IProperty otherProperty in otherProperties)
        {
            if ((!otherProperty?.PropertyType?.SupportsVariation(culture, "*", true) ?? true) &&
                    !(otherProperty?.PropertyType?.Variations == ContentVariation.Nothing))
            {
                continue;
            }

            var alias = otherProperty?.PropertyType.Alias;
            if (otherProperty is not null && alias is not null)
            {
                foreach (IPropertyValue pvalue in otherProperty.Values)
                {
                    if (((otherProperty?.PropertyType?.SupportsVariation(pvalue.Culture, pvalue.Segment, true) ?? false) &&
                        (culture == "*" ||(pvalue.Culture?.InvariantEquals(culture) ?? false))) ||
                         otherProperty?.PropertyType?.Variations == ContentVariation.Nothing)
                    {
                        var value = published ? pvalue.PublishedValue : pvalue.EditedValue;
                        content.SetValue(alias, value, pvalue.Culture, pvalue.Segment);
                    }
                }
            }
        }

        // copy names, too
        if (culture == "*")
        {
            content.CultureInfos?.Clear();
            content.CultureInfos = null;
        }

        if (culture == null || culture == "*")
        {
            content.Name = other.Name;
        }

        // ReSharper disable once UseDeconstruction
        if (other.CultureInfos is not null)
        {
            foreach (ContentCultureInfos cultureInfo in other.CultureInfos)
            {
                if (culture == "*" || culture.InvariantEquals(cultureInfo.Culture))
                {
                    content.SetCultureName(cultureInfo.Name, cultureInfo.Culture);
                }
            }
        }
    }

    /// <summary>
    ///     Sets the publish information for a specific culture on the content item.
    /// </summary>
    /// <param name="content">The content item to update.</param>
    /// <param name="culture">The culture code (e.g., "en-US").</param>
    /// <param name="name">The published name of the content in the specified culture.</param>
    /// <param name="date">The publish date to associate with this culture.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name" /> or <paramref name="culture" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name" /> or <paramref name="culture" /> is empty or whitespace.</exception>
    public static void SetPublishInfo(this IPublishableContentBase content, string? culture, string? name, DateTime date)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(name));
        }

        if (culture == null)
        {
            throw new ArgumentNullException(nameof(culture));
        }

        if (string.IsNullOrWhiteSpace(culture))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(culture));
        }

        content.PublishCultureInfos?.AddOrUpdate(culture, name, date);
    }

    /// <summary>
    ///     Sets the cultures that have been edited on the content item.
    /// </summary>
    /// <param name="content">The content item to update.</param>
    /// <param name="cultures">The collection of culture codes that have been edited, or null to clear.</param>
    public static void SetCultureEdited(this IPublishableContentBase content, IEnumerable<string?>? cultures)
    {
        if (cultures == null)
        {
            content.EditedCultures = null;
        }
        else
        {
            var editedCultures = new HashSet<string>(
                cultures.Where(x => !x.IsNullOrWhiteSpace())!,
                StringComparer.OrdinalIgnoreCase);
            content.EditedCultures = editedCultures.Count > 0 ? editedCultures : null;
        }
    }

    /// <summary>
    ///     Sets the publishing values for names and properties.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="impact"></param>
    /// <param name="publishTime"></param>
    /// <param name="propertyEditorCollection"></param>
    /// <returns>
    ///     A value indicating whether it was possible to publish the names and values for the specified
    ///     culture(s). The method may fail if required names are not set, but it does NOT validate property data
    /// </returns>
    public static bool PublishCulture(this IPublishableContentBase content, CultureImpact? impact, DateTime publishTime, PropertyEditorCollection propertyEditorCollection)
    {
        if (impact == null)
        {
            throw new ArgumentNullException(nameof(impact));
        }

        // the variation should be supported by the content type properties
        //  if the content type is invariant, only '*' and 'null' is ok
        //  if the content type varies, everything is ok because some properties may be invariant
        if (!content.ContentType.SupportsPropertyVariation(impact.Culture, "*", true))
        {
            throw new NotSupportedException($"Culture \"{impact.Culture}\" is not supported by content type \"{content.ContentType.Alias}\" with variation \"{content.ContentType.Variations}\".");
        }

        // set names
        if (impact.ImpactsAllCultures)
        {
            // does NOT contain the invariant culture
            foreach (var culture in content.AvailableCultures)
            {
                var name = content.GetCultureName(culture);
                if (string.IsNullOrWhiteSpace(name))
                {
                    return false;
                }

                content.SetPublishInfo(culture, name, publishTime);
            }
        }
        else if (impact.ImpactsOnlyInvariantCulture)
        {
            if (string.IsNullOrWhiteSpace(content.Name))
            {
                return false;
            }
            // PublishName set by repository - nothing to do here
        }
        else if (impact.ImpactsExplicitCulture)
        {
            var name = content.GetCultureName(impact.Culture);
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            content.SetPublishInfo(impact.Culture, name, publishTime);
        }

        // set values
        // property.PublishValues only publishes what is valid, variation-wise,
        // but accepts any culture arg: null, all, specific
        foreach (IProperty property in content.Properties)
        {
            // for the specified culture (null or all or specific)
            PublishPropertyValues(content, property, impact.Culture, propertyEditorCollection);

            // maybe the specified culture did not impact the invariant culture, so PublishValues
            // above would skip it, yet it *also* impacts invariant properties
            if (impact.ImpactsAlsoInvariantProperties && (property.PropertyType.VariesByCulture() is false || impact.ImpactsOnlyDefaultCulture))
            {
                PublishPropertyValues(content, property, null, propertyEditorCollection);
            }
        }

        content.PublishedState = PublishedState.Publishing;
        return true;
    }

    private static void PublishPropertyValues(IPublishableContentBase content, IProperty property, string? culture, PropertyEditorCollection propertyEditorCollection)
    {
        // if the content varies by culture, let data editor opt-in to perform partial property publishing (per culture)
        if (content.ContentType.VariesByCulture()
            && propertyEditorCollection.TryGet(property.PropertyType.PropertyEditorAlias, out IDataEditor? dataEditor)
            && dataEditor.CanMergePartialPropertyValues(property.PropertyType))
        {
            // perform partial publishing for the current culture
            property.PublishPartialValues(dataEditor, culture);
            return;
        }

        // for the specified culture (null or all or specific)
        property.PublishValues(culture);
    }

    /// <summary>
    ///     Returns false if the culture is already unpublished
    /// </summary>
    /// <param name="content"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public static bool UnpublishCulture(this IPublishableContentBase content, string? culture = "*")
    {
        culture = culture?.NullOrWhiteSpaceAsNull();

        // the variation should be supported by the content type properties
        if (!content.ContentType.SupportsPropertyVariation(culture, "*", true))
        {
            throw new NotSupportedException(
                $"Culture \"{culture}\" is not supported by content type \"{content.ContentType.Alias}\" with variation \"{content.ContentType.Variations}\".");
        }

        var keepProcessing = true;

        if (culture == "*" || (content.ContentType.VariesByCulture() is false && culture is null))
        {
            // all cultures
            content.ClearPublishInfos();
        }
        else
        {
            // one single culture
            keepProcessing = content.ClearPublishInfo(culture);
        }

        if (keepProcessing)
        {
            // property.PublishValues only publishes what is valid, variation-wise
            foreach (IProperty property in content.Properties)
            {
                property.UnpublishValues(culture);
            }

            content.PublishedState = PublishedState.Publishing;
        }

        return keepProcessing;
    }

    /// <summary>
    ///     Clears all publish culture information from the content item.
    /// </summary>
    /// <param name="content">The content item to clear publish information from.</param>
    public static void ClearPublishInfos(this IPublishableContentBase content) => content.PublishCultureInfos = null;

    /// <summary>
    ///     Returns false if the culture is already unpublished
    /// </summary>
    /// <param name="content"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public static bool ClearPublishInfo(this IPublishableContentBase content, string? culture)
    {
        if (culture == null)
        {
            throw new ArgumentNullException(nameof(culture));
        }

        if (string.IsNullOrWhiteSpace(culture))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(culture));
        }

        var removed = content.PublishCultureInfos?.Remove(culture);
        if (removed ?? false)
        {
            // set the culture to be dirty - it's been modified
            content.TouchCulture(culture);
        }

        return removed ?? false;
    }
}
