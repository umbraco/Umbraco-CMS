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
            if (allLanguages.Count == 0) return Enumerable.Empty<ContentVariation>(); 

            var langs = context.Mapper.Map<IEnumerable<ILanguage>, IEnumerable<Language>>(allLanguages, null, context);
            var variants = langs.Select(x => new ContentVariation
            {
                Language = x,
                Mandatory = x.Mandatory,
                //fixme these all need to the variant values but we need to wait for the db/service changes
                Name = source.Name ,                
                Exists = source.HasVariation(x.Id), //TODO: This needs to be wired up with new APIs when they are ready
                PublishedState = source.PublishedState.ToString()
            }).ToList();

            var langId = context.GetLanguageId();

            //set the current variant being edited to the one found in the context or the default, whichever matches
            variants.First(x => (langId.HasValue && langId.Value == x.Language.Id) || x.Language.IsDefaultVariantLanguage).IsCurrent = true;

            return variants;
        }

    }
}
