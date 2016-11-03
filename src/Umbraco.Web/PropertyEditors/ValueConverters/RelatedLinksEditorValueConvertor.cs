// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelatedLinksPropertyConverter.cs" company="Umbraco">
//   Umbraco
// </copyright>
// <summary>
//   Defines the RelatedLinksPropertyConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The related links property value converter.
    /// </summary>
    [PropertyValueType(typeof(RelatedLinks))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.ContentCache)]
    public class RelatedLinksPropertyConverter : PropertyValueConverterBase
    {
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

            var relatedLinksData = JsonConvert.DeserializeObject<IEnumerable<RelatedLinkData>>(sourceString);
            var relatedLinks = new List<RelatedLink>();

            foreach (var linkData in relatedLinksData)
            {
                var relatedLink = new RelatedLink()
                {
                    Caption = linkData.Caption,
                    NewWindow = linkData.NewWindow,
                    IsInternal = linkData.IsInternal,
                    Type = linkData.Type,
                    Id = linkData.Internal,
                    Link = linkData.Link
                };
                relatedLink = CreateLink(relatedLink);

                if (relatedLink.IsDeleted == false)
                {
                    relatedLinks.Add(relatedLink);
                }
                else
                {
                    LogHelper.Warn<RelatedLinks>(
                        string.Format("Related Links value converter skipped a link as the node has been unpublished/deleted (Internal Link NodeId: {0}, Link Caption: \"{1}\")", relatedLink.Link, relatedLink.Caption));
                }
            }

            return new RelatedLinks(relatedLinks, sourceString);
        }

        private RelatedLink CreateLink(RelatedLink link)
        {
            if (link.IsInternal && link.Id != null)
            {
                if (UmbracoContext.Current == null)
                {
                    return null;
                }

                link.Link = UmbracoContext.Current.UrlProvider.GetUrl((int)link.Id);
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