using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using ContentVariation = Umbraco.Web.Models.ContentEditing.ContentVariation;
using Language = Umbraco.Web.Models.ContentEditing.Language;

namespace Umbraco.Web.Models.Mapping
{
    internal class VariationResolver : IValueResolver<IContent, ContentItemDisplay, IEnumerable<ContentVariation>>
    {
        private readonly ILocalizationService _localizationService;

        public VariationResolver(ILocalizationService localizationService)
        {
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        }

        public IEnumerable<ContentVariation> Resolve(IContent source, ContentItemDisplay destination, IEnumerable<ContentVariation> destMember, ResolutionContext context)
        {
            var allLanguages = _localizationService.GetAllLanguages().OrderBy(x => x.Id).ToList();
            if (allLanguages.Count == 0) return Enumerable.Empty<ContentVariation>(); //there's only 1 language defined so we don't have language variants enabled

            var langs = Mapper.Map<IEnumerable<Language>>(allLanguages).ToList();
            var variants = langs.Select(x => new ContentVariation
            {
                Language = x,
                //fixme these all need to the variant values but we need to wait for the db/service changes
                Name = source.Name ,
                ExpireDate = source.ExpireDate,
                PublishDate = source.PublishDate,
                ReleaseDate = source.ReleaseDate,
                Exists = source.HasVariation(x.Id),
                PublishedState = source.PublishedState.ToString()
            }).ToList();

            //if there's only one language, by default it is the default
            if (langs.Count == 1)
            {
                langs[0].IsDefaultVariantLanguage = true;
                langs[0].Mandatory = true;
            }
            else if (allLanguages.All(x => !x.IsDefaultVariantLanguage))
            {
                //if no language has the default flag, then the defaul language is the one with the lowest id
                langs[0].IsDefaultVariantLanguage = true;
                langs[0].Mandatory = true;
            }

            //TODO: Not sure if this is required right now, IsCurrent could purely be a UI thing, we'll see
            //set the 'current'
            variants.First(x => x.Language.IsDefaultVariantLanguage).IsCurrent = true;

            return variants;
        }

    }
}
