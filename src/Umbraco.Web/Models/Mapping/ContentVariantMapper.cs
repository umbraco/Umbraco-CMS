using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Language = Umbraco.Web.Models.ContentEditing.Language;

namespace Umbraco.Web.Models.Mapping
{
    internal class ContentVariantMapper
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedTextService _localizedTextService;

        public ContentVariantMapper(ILocalizationService localizationService, ILocalizedTextService localizedTextService)
        {
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _localizedTextService = localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
        }

        public IEnumerable<ContentVariantDisplay> Map(IContent source, MapperContext context)
        {
            var variesByCulture = source.ContentType.VariesByCulture();
            var variesBySegment = source.ContentType.VariesBySegment();

            IList<ContentVariantDisplay> variants = new List<ContentVariantDisplay>();

            if (!variesByCulture && !variesBySegment)
            {
                // this is invariant so just map the IContent instance to ContentVariationDisplay
                var variantDisplay = context.Map<ContentVariantDisplay>(source);
                variants.Add(variantDisplay);
            }
            else if (variesByCulture && !variesBySegment)
            {
                var languages = GetLanguages(context);
                variants = languages
                    .Select(language => CreateVariantDisplay(context, source, language, null))
                    .ToList();
            }
            else if (variesBySegment && !variesByCulture)
            {
                // Segment only
                var segments = GetSegments(source);
                variants = segments
                    .Select(segment => CreateVariantDisplay(context, source, null, segment))
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

                variants = languages
                    .SelectMany(language => segments
                        .Select(segment => CreateVariantDisplay(context, source, language, segment)))
                    .ToList();
            }

            return SortVariants(variants);
        }

        private IList<ContentVariantDisplay> SortVariants(IList<ContentVariantDisplay> variants)
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

        private static bool IsDefaultSegment(ContentVariantDisplay variant)
        {
            return variant.Segment == null;
        }

        private static bool IsDefaultLanguage(ContentVariantDisplay variant)
        {
            return variant.Language == null || variant.Language.IsDefault;
        }

        private IEnumerable<Language> GetLanguages(MapperContext context)
        {
            var allLanguages = _localizationService.GetAllLanguages().OrderBy(x => x.Id).ToList();
            if (allLanguages.Count == 0)
            {
                // This should never happen
                return Enumerable.Empty<Language>();
            }
            else
            {
                return context.MapEnumerable<ILanguage, Language>(allLanguages).ToList();
            }
        }

        /// <summary>
        /// Returns all segments assigned to the content
        /// </summary>
        /// <param name="content"></param>
        /// <returns>
        /// Returns all segments assigned to the content including the default `null` segment.
        /// </returns>
        private IEnumerable<string> GetSegments(IContent content)
        {
            // The default segment (null) is always there,
            // even when there is no property data at all yet
            var segments = new List<string> { null };

            // Add actual segments based on the property values
            segments.AddRange(content.Properties.SelectMany(p => p.Values.Select(v => v.Segment)));

            // Do not return a segment more than once
            return segments.Distinct();
        }

        private ContentVariantDisplay CreateVariantDisplay(MapperContext context, IContent content, Language language, string segment)
        {
            context.SetCulture(language?.IsoCode);
            context.SetSegment(segment);

            var variantDisplay = context.Map<ContentVariantDisplay>(content);

            variantDisplay.Segment = segment;
            variantDisplay.Language = language;
            variantDisplay.Name = content.GetCultureName(language?.IsoCode);
            variantDisplay.DisplayName = GetDisplayName(language, segment);

            return variantDisplay;
        }

        private string GetDisplayName(Language language, string segment)
        {
            var isCultureVariant = language != null;
            var isSegmentVariant = !segment.IsNullOrWhiteSpace();

            if(!isCultureVariant && !isSegmentVariant)
            {
                return _localizedTextService.Localize("general", "default");
            }

            var parts = new List<string>();

            if (isSegmentVariant)
                parts.Add(segment);

            if (isCultureVariant)
                parts.Add(language.Name);

            return string.Join(" — ", parts);

        }
    }
}
