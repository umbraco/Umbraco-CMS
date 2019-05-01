// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContentPickerPropertyConverter.cs" company="Umbraco">
//   Umbraco
// </copyright>
// <summary>
//   The content picker property editor converter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The content picker property value converter.
    /// </summary>
    [DefaultPropertyValueConverter]
    [PropertyValueType(typeof(IPublishedContent))]
    [PropertyValueCache(PropertyCacheValue.Object, PropertyCacheLevel.ContentCache)]
    [PropertyValueCache(PropertyCacheValue.Source, PropertyCacheLevel.Content)]
    [PropertyValueCache(PropertyCacheValue.XPath, PropertyCacheLevel.Content)]
    public class ContentPickerPropertyConverter : PropertyValueConverterBase
    {
        /// <summary>
        /// The properties to exclude.
        /// </summary>
        private static readonly List<string> PropertiesToExclude = new List<string>()
        {
            Constants.Conventions.Content.InternalRedirectId,
            Constants.Conventions.Content.Redirect
        };

        /// <summary>
        /// Checks if this converter can convert the property editor and registers if it can.
        /// </summary>
        /// <param name="propertyType">
        /// The published property type.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            if (propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.ContentPicker2Alias))
                return true;

            if (UmbracoConfig.For.UmbracoSettings().Content.EnablePropertyValueConverters)
            {
                return propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.ContentPickerAlias);
            }

            return false;
        }

        /// <summary>
        /// Convert the raw string into a nodeId integer
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
        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            var attemptConvertInt = source.TryConvertTo<int>();
            if (attemptConvertInt.Success)
                return attemptConvertInt.Result;

            var attemptConvertUdi = source.TryConvertTo<Udi>();
            if (attemptConvertUdi.Success)
                return attemptConvertUdi.Result;

            return null;
        }

        /// <summary>
        /// Convert the source nodeId into a IPublishedContent (or DynamicPublishedContent)
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
                return null;

            var umbracoContext = UmbracoContext.Current;
            if (umbracoContext == null)
                return source;

            // Do not convert source for excluded properties
            if (propertyType.PropertyTypeAlias == null || PropertiesToExclude.InvariantContains(propertyType.PropertyTypeAlias))
                return source;

            if (source is int sourceInt)
            {
                return umbracoContext.ContentCache.GetById(sourceInt);
            }
            else if (source is Udi sourceUdi)
            {
                return umbracoContext.ContentCache.GetById(sourceUdi);
            }

            return null;
        }

        /// <summary>
        /// The convert source to xPath.
        /// </summary>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="preview">
        /// The preview.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public override object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
        {
            return source.ToString();
        }
    }
}
