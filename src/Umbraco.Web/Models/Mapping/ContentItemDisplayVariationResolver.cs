using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using ContentVariation = Umbraco.Web.Models.ContentEditing.ContentVariation;
using Language = Umbraco.Web.Models.ContentEditing.Language;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Used to map the <see cref="ContentItemDisplay"/> variations collection from an <see cref="IContent"/> instance
    /// </summary>
    internal class ContentItemDisplayVariationResolver : IValueResolver<IContent, ContentItemDisplay, IEnumerable<ContentVariation>>
    {
        private readonly ILocalizationService _localizationService;

        public ContentItemDisplayVariationResolver(ILocalizationService localizationService)
        {
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        }

        public IEnumerable<ContentVariation> Resolve(IContent source, ContentItemDisplay destination, IEnumerable<ContentVariation> destMember, ResolutionContext context)
        {
            if (!source.ContentType.VariesByCulture())
                return Enumerable.Empty<ContentVariation>();

            var allLanguages = _localizationService.GetAllLanguages().OrderBy(x => x.Id).ToList();
            if (allLanguages.Count == 0) return Enumerable.Empty<ContentVariation>();

            var langs = context.Mapper.Map<IEnumerable<ILanguage>, IEnumerable<Language>>(allLanguages, null, context);
            var variants = langs.Select(x => new ContentVariation
            {
                Language = x,
                Mandatory = x.Mandatory,
                Name = source.GetCultureName(x.IsoCode),
                Exists = source.IsCultureAvailable(x.IsoCode), // segments ??
                PublishedState = (source.PublishedState == PublishedState.Unpublished //if the entire document is unpublished, then flag every variant as unpublished
                    ? PublishedState.Unpublished
                    : source.IsCulturePublished(x.IsoCode)
                        ? PublishedState.Published
                        : PublishedState.Unpublished).ToString(),
                IsEdited = source.IsCultureEdited(x.IsoCode)
                //Segment = ?? We'll need to populate this one day when we support segments
            }).ToList();

            var culture = context.GetCulture();

            //set the current variant being edited to the one found in the context or the default if nothing matches
            var foundCurrent = false;
            foreach (var variant in variants)
            {
                if (culture.InvariantEquals(variant.Language.IsoCode))
                {
                    variant.IsCurrent = true;
                    foundCurrent = true;
                    break;
                }
            }
            if (!foundCurrent)
                variants.First(x => x.Language.IsDefaultVariantLanguage).IsCurrent = true;

            return variants;
        }

    }
}
