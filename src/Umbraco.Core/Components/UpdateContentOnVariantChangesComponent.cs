using System;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Manages data changes for when content/property types have variation changes
    /// </summary>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    //fixme: this one MUST fire before any of the content cache ones
    public sealed class UpdateContentOnVariantChangesComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        private IContentService _contentService;
        private ILocalizationService _langService;

        public void Initialize(IContentService contentService, ILocalizationService langService)
        {
            _contentService = contentService;
            _langService = langService;
            ContentTypeService.ScopedRefreshedEntity += OnContentTypeRefreshedEntity;
        }

        private void OnContentTypeRefreshedEntity(IContentTypeService sender, ContentTypeChange<IContentType>.EventArgs e)
        {
            var defaultLang = _langService.GetDefaultLanguageIsoCode(); //this will be cached

            foreach (var c in e.Changes)
            {
                // existing property alias change?
                var hasAnyPropertyVariationChanged = c.Item.WasPropertyTypeVariationChanged(out var aliases);
                if (hasAnyPropertyVariationChanged)
                {
                    var contentOfType = _contentService.GetByType(c.Item.Id);

                    foreach (var a in aliases)
                    {
                        var propType = c.Item.PropertyTypes.First(x => x.Alias == a);

                        //fixme: this does not take into account segments, but how can we since we don't know what it changed from?

                        switch (propType.Variations)
                        {
                            case ContentVariation.Culture:
                                //if the current variation is culture it means that the previous was nothing
                                foreach (var content in contentOfType)
                                {
                                    object invariantVal;
                                    try
                                    {
                                        content.Properties[a].PropertyType.Variations = ContentVariation.Nothing;
                                        //now get the invariant val
                                        invariantVal = content.GetValue(a);
                                    }
                                    finally
                                    {
                                        content.Properties[a].PropertyType.Variations = ContentVariation.Culture;
                                    }
                                    //set the invariant value
                                    content.SetValue(a, invariantVal, defaultLang);
                                }
                                break;
                            case ContentVariation.Nothing:
                                //if the current variation is nothing it means that the previous was culture
                                foreach(var content in contentOfType)
                                {
                                    object cultureVal;
                                    try
                                    {
                                        content.Properties[a].PropertyType.Variations = ContentVariation.Culture;
                                        //now get the culture val
                                        cultureVal = content.GetValue(a, defaultLang);
                                    }
                                    finally
                                    {
                                        content.Properties[a].PropertyType.Variations = ContentVariation.Nothing;
                                    }
                                    //set the invariant value
                                    content.SetValue(a, cultureVal);
                                }
                                break;
                            default:
                                throw new NotSupportedException("The variation change is not supported");
                        }
                    }

                    _contentService.Save(contentOfType);
                }
            }
        }
    }
}
