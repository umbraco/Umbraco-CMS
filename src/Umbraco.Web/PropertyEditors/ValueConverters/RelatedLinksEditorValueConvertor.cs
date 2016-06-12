// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelatedLinksPropertyConverter.cs" company="Umbraco">
//   Umbraco
// </copyright>
// <summary>
//   Defines the RelatedLinksPropertyConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Umbraco.Core;
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
            return propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.RelatedLinksAlias);
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

            return UmbracoContext.Current != null ? new RelatedLinks(sourceString) : null;
        }
    }
}