using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Language = Umbraco.Web.Models.ContentEditing.Language;

namespace Umbraco.Web.Models.Mapping
{

    internal class ContentVariantResolver : IValueResolver<IContent, ContentItemDisplay, IEnumerable<ContentVariantDisplay>>
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedTextService _textService;

        public ContentVariantResolver(ILocalizationService localizationService, ILocalizedTextService textService)
        {
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _textService = textService ?? throw new ArgumentNullException(nameof(textService));
        }

        public IEnumerable<ContentVariantDisplay> Resolve(IContent source, ContentItemDisplay destination, IEnumerable<ContentVariantDisplay> destMember, ResolutionContext context)
        {
            var result = new List<ContentVariantDisplay>();
            if (!source.ContentType.VariesByCulture())
            {
                //this is invariant so just map the IContent instance to ContentVariationDisplay
                result.Add(context.Mapper.Map<ContentVariantDisplay>(source));
            }
            else
            {
                var allLanguages = _localizationService.GetAllLanguages().OrderBy(x => x.Id).ToList();
                if (allLanguages.Count == 0) return Enumerable.Empty<ContentVariantDisplay>(); //this should never happen

                var langs = context.Mapper.Map<IEnumerable<ILanguage>, IEnumerable<Language>>(allLanguages, null, context).ToList();

                //create a variant for each lang, then we'll populate the values
                var variants = langs.Select(x =>
                {
                    //We need to set the culture in the mapping context since this is needed to ensure that the correct property values
                    //are resolved during the mapping
                    context.Items[ResolutionContextExtensions.CultureKey] = x.IsoCode;
                    return context.Mapper.Map<IContent, ContentVariantDisplay>(source, null, context);
                }).ToList();

                for (int i = 0; i < langs.Count; i++)
                {
                    var x = langs[i];
                    var variant = variants[i];

                    variant.Language = x;
                    variant.Name = source.GetCultureName(x.IsoCode);
                    variant.Exists = source.IsCultureAvailable(x.IsoCode); // segments ??

                    var publishedState = source.PublishedState == PublishedState.Unpublished //if the entire document is unpublished, then flag every variant as unpublished
                        ? PublishedState.Unpublished
                        : source.IsCulturePublished(x.IsoCode)
                            ? PublishedState.Published
                            : PublishedState.Unpublished;
                    var isEdited = source.Id > 0 && source.IsCultureEdited(x.IsoCode);

                    //now we can calculate the content state
                    if (!isEdited && publishedState == PublishedState.Unpublished)
                        variant.State = ContentSavedState.NotCreated;
                    else if (isEdited && publishedState == PublishedState.Unpublished)
                        variant.State = ContentSavedState.Draft;
                    else if (!isEdited && publishedState == PublishedState.Published)
                        variant.State = ContentSavedState.Published;
                    else
                        variant.State = ContentSavedState.PublishedPendingChanges;
                }

                return variants;
            }
            return result;
        }
    }
    
}
