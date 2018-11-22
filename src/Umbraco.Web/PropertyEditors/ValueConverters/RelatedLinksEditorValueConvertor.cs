// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelatedLinksPropertyConverter.cs" company="Umbraco">
//   Umbraco
// </copyright>
// <summary>
//   Defines the RelatedLinksPropertyConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Web.Extensions;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The related links property value converter.
    /// </summary>
    [DefaultPropertyValueConverter(typeof(LegacyRelatedLinksEditorValueConvertor), typeof(JsonValueConverter))]
    [PropertyValueType(typeof(RelatedLinks))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.ContentCache)]
    public class RelatedLinksPropertyConverter : PropertyValueConverterBase
    {
        private readonly UrlProvider _urlProvider;

        public RelatedLinksPropertyConverter()
        {
        }

        public RelatedLinksPropertyConverter(UrlProvider urlProvider)
        {
            if (urlProvider == null) throw new ArgumentNullException("urlProvider");
            _urlProvider = urlProvider;
        }

        /// <summary>
        /// Checks if this converter can convert the property editor and registers if it can.
        /// </summary>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            if (propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.RelatedLinks2Alias))
                return true;

            if (UmbracoConfig.For.UmbracoSettings().Content.EnablePropertyValueConverters)
            {
                return propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.RelatedLinksAlias);
            }
            return false;
        }

        /// <summary>
        /// Convert the source nodeId into a RelatedLinks object
        /// </summary>
        /// <param name="propertyType">
        /// The published property type.
        /// </param>
        /// <param name="source">
        /// The value of the property
        /// </param>
        /// <param name="preview">
        /// The preview.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null)
            {
                return null;
            }

            var sourceString = source.ToString();

            var relatedLinksData = JsonConvert.DeserializeObject<IEnumerable<RelatedLink>>(sourceString);
            var relatedLinks = new List<RelatedLink>();

            if (UmbracoContext.Current == null) return source;

            var helper = new UmbracoHelper(UmbracoContext.Current);

            foreach (var linkData in relatedLinksData)
            {
                var relatedLink = new RelatedLink
                {
                    Caption = linkData.Caption,
                    NewWindow = linkData.NewWindow,
                    IsInternal = linkData.IsInternal,
                    Type = linkData.Type,
                    Link = linkData.Link
                };

                int contentId;
                if (int.TryParse(relatedLink.Link, out contentId))
                {
                    relatedLink.Id = contentId;
                    relatedLink = CreateLink(relatedLink);
                    relatedLink.Content = UmbracoContext.Current.ContentCache.GetById(contentId);
                }
                else
                {
                    var strLinkId = linkData.Link;
                    var udiAttempt = strLinkId.TryConvertTo<Udi>();
                    if (udiAttempt.Success && udiAttempt.Result != null)
                    {
                        var content = helper.TypedContent(udiAttempt.Result);
                        if (content != null)
                        {
                            relatedLink.Id = content.Id;
                            relatedLink = CreateLink(relatedLink);
                            relatedLink.Content = content;
                        }
                    }
                }

                if (relatedLink.IsDeleted == false)
                {
                    relatedLinks.Add(relatedLink);
                }
                else
                {
                    LogHelper.Warn<RelatedLinksPropertyConverter>(
                        string.Format("Related Links value converter skipped a link as the node has been unpublished/deleted (Internal Link NodeId: {0}, Link Caption: \"{1}\")", relatedLink.Link, relatedLink.Caption));
                }
            }

            return new RelatedLinks(relatedLinks, sourceString);
        }

        private RelatedLink CreateLink(RelatedLink link)
        {
            if (link.IsInternal && link.Id != null)
            {
                if (_urlProvider == null && UmbracoContext.Current == null)
                {
                    return null;
                }

                var urlProvider = _urlProvider ?? UmbracoContext.Current.UrlProvider;

                link.Link = urlProvider.GetUrl((int)link.Id);
                if (link.Link.Equals("#"))
                {
                    link.IsDeleted = true;
                    link.Link = link.Id.ToString();
                }
                else
                {
                    link.IsDeleted = false;
                }
            }

            return link;
        }
    }
}
