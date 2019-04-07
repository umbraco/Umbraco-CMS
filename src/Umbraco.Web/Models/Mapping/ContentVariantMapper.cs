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
            var result = new List<ContentVariantDisplay>();
            if (!source.ContentType.VariesByCulture())
            {
                //this is invariant so just map the IContent instance to ContentVariationDisplay
                result.Add(context.Map<ContentVariantDisplay>(source));
            }
            else
            {
                var allLanguages = _localizationService.GetAllLanguages().OrderBy(x => x.Id).ToList();
                if (allLanguages.Count == 0) return Enumerable.Empty<ContentVariantDisplay>(); //this should never happen

                var langs = context.Map<IEnumerable<Language>>(allLanguages).ToList();

                //create a variant for each language, then we'll populate the values
                var variants = langs.Select(x =>
                {
                    //We need to set the culture in the mapping context since this is needed to ensure that the correct property values
                    //are resolved during the mapping
                    context.SetCulture(x.IsoCode);
                    return context.Map<ContentVariantDisplay>(source);
                }).ToList();

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
                variants = variants.OrderBy(x => x.Name).ToList();

                //Insert the default language as the first item
                variants.Insert(0, defaultLang);

                return variants;
            }
            return result;
        }
    }
}
