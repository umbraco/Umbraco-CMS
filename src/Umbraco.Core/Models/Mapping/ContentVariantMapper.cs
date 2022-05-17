using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping;

public class ContentVariantMapper
{
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedTextService _localizedTextService;

    public ContentVariantMapper(ILocalizationService localizationService, ILocalizedTextService localizedTextService)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _localizedTextService = localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
    }

    public IEnumerable<TVariant>? Map<TVariant>(IContent source, MapperContext context)
        where TVariant : ContentVariantDisplay
    {
        var variesByCulture = source.ContentType.VariesByCulture();
        var variesBySegment = source.ContentType.VariesBySegment();

        IList<TVariant>? variants = new List<TVariant>();

        if (!variesByCulture && !variesBySegment)
        {
            // this is invariant so just map the IContent instance to ContentVariationDisplay
            TVariant? variantDisplay = context.Map<TVariant>(source);
            if (variantDisplay is not null)
            {
                variants.Add(variantDisplay);
            }
        }
        else if (variesByCulture && !variesBySegment)
        {
            IEnumerable<ContentEditing.Language> languages = GetLanguages(context);
            variants = languages?
                .Select(language => CreateVariantDisplay<TVariant>(context, source, language, null))
                .WhereNotNull()
                .ToList();
        }
        else if (variesBySegment && !variesByCulture)
        {
            // Segment only
            IEnumerable<string?> segments = GetSegments(source);
            variants = segments?
                .Select(segment => CreateVariantDisplay<TVariant>(context, source, null, segment))
                .WhereNotNull()
                .ToList();
        }
        else
        {
            // Culture and segment
            var languages = GetLanguages(context).ToList();
            var segments = GetSegments(source).ToList();

            if (languages.Count == 0 || segments.Count == 0)
            {
                // This should not happen
                throw new InvalidOperationException("No languages or segments available");
            }

            variants = languages?
                .SelectMany(language => segments
                    .Select(segment => CreateVariantDisplay<TVariant>(context, source, language, segment)))
                .WhereNotNull()
                .ToList();
        }

        return SortVariants(variants);
    }

    private static bool IsDefaultSegment(ContentVariantDisplay variant) => variant.Segment == null;

    private IList<TVariant>? SortVariants<TVariant>(IList<TVariant>? variants)
        where TVariant : ContentVariantDisplay
    {
        if (variants == null || variants.Count <= 1)
        {
            return variants;
        }

        // Default variant first, then order by language, segment.
        return variants
            .OrderBy(v => IsDefaultLanguage(v) ? 0 : 1)
            .ThenBy(v => IsDefaultSegment(v) ? 0 : 1)
            .ThenBy(v => v?.Language?.Name)
            .ThenBy(v => v.Segment)
            .ToList();
    }

    private static bool IsDefaultLanguage(ContentVariantDisplay variant) =>
        variant.Language == null || variant.Language.IsDefault;

    private IEnumerable<ContentEditing.Language> GetLanguages(MapperContext context)
    {
        var allLanguages = _localizationService.GetAllLanguages().OrderBy(x => x.Id).ToList();
        if (allLanguages.Count == 0)
        {
            // This should never happen
            return Enumerable.Empty<ContentEditing.Language>();
        }

        return context.MapEnumerable<ILanguage, ContentEditing.Language>(allLanguages).WhereNotNull().ToList();
    }

    /// <summary>
    ///     Returns all segments assigned to the content
    /// </summary>
    /// <param name="content"></param>
    /// <returns>
    ///     Returns all segments assigned to the content including the default `null` segment.
    /// </returns>
    private IEnumerable<string?> GetSegments(IContent content)
    {
        // The default segment (null) is always there,
        // even when there is no property data at all yet
        var segments = new List<string?> { null };

        // Add actual segments based on the property values
        segments.AddRange(content.Properties.SelectMany(p => p.Values.Select(v => v.Segment)));

        // Do not return a segment more than once
        return segments.Distinct();
    }

    private TVariant? CreateVariantDisplay<TVariant>(MapperContext context, IContent content, ContentEditing.Language? language, string? segment)
        where TVariant : ContentVariantDisplay
    {
        context.SetCulture(language?.IsoCode);
        context.SetSegment(segment);

        TVariant? variantDisplay = context.Map<TVariant>(content);

        if (variantDisplay is null)
        {
            return null;
        }

        variantDisplay.Segment = segment;
        variantDisplay.Language = language;
        variantDisplay.Name = content.GetCultureName(language?.IsoCode);
        variantDisplay.DisplayName = GetDisplayName(language, segment);

        return variantDisplay;
    }

    private string GetDisplayName(ContentEditing.Language? language, string? segment)
    {
        var isCultureVariant = language is not null;
        var isSegmentVariant = !segment.IsNullOrWhiteSpace();

        if (!isCultureVariant && !isSegmentVariant)
        {
            return _localizedTextService.Localize("general", "default");
        }

        var parts = new List<string>();

        if (isSegmentVariant)
        {
            parts.Add(segment!);
        }

        if (isCultureVariant)
        {
            parts.Add(language?.Name!);
        }

        return string.Join(" — ", parts);
    }
}
