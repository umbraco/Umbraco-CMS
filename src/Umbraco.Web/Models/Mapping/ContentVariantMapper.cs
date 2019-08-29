using System;
using System.Collections.Generic;
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

        public ContentVariantMapper(ILocalizationService localizationService)
        {
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        }

        public IEnumerable<ContentVariantDisplay> Map(IContent source, MapperContext context)
        {
            var variants = new List<ContentVariantDisplay>();

            var variesByCulture = source.ContentType.VariesByCulture();
            var variesBySegment = source.ContentType.VariesBySegment();

            if (!variesByCulture && !variesBySegment)
            {
                //this is invariant so just map the IContent instance to ContentVariationDisplay
                variants.Add(context.Map<ContentVariantDisplay>(source));
                return variants;
            }

            if (variesByCulture && !variesBySegment)
            {
                // Culture only

                var allLanguages = _localizationService.GetAllLanguages().OrderBy(x => x.Id).ToList();
                if (allLanguages.Count == 0) return Enumerable.Empty<ContentVariantDisplay>(); //this should never happen

                var langs = context.MapEnumerable<ILanguage, Language>(allLanguages).ToList();

                //create a variant for each language, then we'll populate the values
                variants.AddRange(langs.Select(x =>
                {
                    //We need to set the culture in the mapping context since this is needed to ensure that the correct property values
                    //are resolved during the mapping
                    context.SetCulture(x.IsoCode);
                    return context.Map<ContentVariantDisplay>(source);
                }));

                for (int i = 0; i < langs.Count; i++)
                {
                    var x = langs[i];
                    var variant = variants[i];

                    variant.Language = x;
                    variant.Name = source.GetCultureName(x.IsoCode);
                }

                //Put the default language first in the list & then sort rest by a-z
                var defaultLang = variants.SingleOrDefault(x => x.Language.IsDefault);

                //Remove the default language from the list for now
                variants.Remove(defaultLang);

                //Sort the remaining languages a-z
                variants = variants.OrderBy(x => x.Language.Name).ToList();

                //Insert the default language as the first item
                variants.Insert(0, defaultLang);
            }
            else if (variesBySegment && !variesByCulture)
            {
                // Segment only
                throw new NotSupportedException("ContentVariantMapper not implemented for segment only!");
            }
            else
            {
                // Culture and segment

                var allLanguages = _localizationService.GetAllLanguages().OrderBy(x => x.Id).ToList();
                if (allLanguages.Count == 0) return Enumerable.Empty<ContentVariantDisplay>(); //this should never happen

                var langs = context.MapEnumerable<ILanguage, Language>(allLanguages).ToList();

                // All segments, including the unsegmented (= NULL) segment.
                // TODO: The NULl segment might have to be changed to be empty string?
                var segments = source.Properties
                    .SelectMany(p => p.Values.Select(v => v.Segment))
                    .Distinct();

                // Add all variants
                foreach (var language in langs)
                {
                    foreach (var segment in segments)
                    {
                        context.SetCulture(language.IsoCode);
                        context.SetSegment(segment);

                        var variantDisplay = context.Map<ContentVariantDisplay>(source);

                        variantDisplay.Language = language;
                        variantDisplay.Segment = segment;
                        variantDisplay.Name = source.GetCultureName(language.IsoCode);

                        variants.Add(variantDisplay);
                    }
                }

                // TODO: Sorting
            }

            return variants;
        }
    }
}
