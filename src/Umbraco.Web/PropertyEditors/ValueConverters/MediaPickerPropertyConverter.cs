// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaPickerPropertyConverter.cs" company="Umbraco">
//   Umbraco
// </copyright>
// <summary>
//  The legacy media picker value converter
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The media picker property value converter.
    /// </summary>
    public class MediaPickerPropertyConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
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
            // ** Value converter disabled as not sure if we want to convert the legacy media picker or not **
            return false;

            //return propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MediaPickerAlias);
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
            {
                return attemptConvertInt.Result;
            }

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
            {
                return null;
            }

            if (UmbracoContext.Current != null)
            {
                return UmbracoContext.Current.MediaCache.GetById((int)source);
            }

            return null;
        }

        /// <summary>
        /// The get property cache level.
        /// </summary>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <param name="cacheValue">
        /// The cache value.
        /// </param>
        /// <returns>
        /// The <see cref="PropertyCacheLevel"/>.
        /// </returns>
        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType, PropertyCacheValue cacheValue)
        {
            PropertyCacheLevel returnLevel;
            switch (cacheValue)
            {
                case PropertyCacheValue.Object:
                    returnLevel = PropertyCacheLevel.ContentCache;
                    break;
                case PropertyCacheValue.Source:
                    returnLevel = PropertyCacheLevel.Content;
                    break;
                case PropertyCacheValue.XPath:
                    returnLevel = PropertyCacheLevel.Content;
                    break;
                default:
                    returnLevel = PropertyCacheLevel.None;
                    break;
            }

            return returnLevel;
        }

        /// <summary>
        /// The CLR type that the value converter returns.
        /// </summary>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <returns>
        /// The <see cref="Type"/>.
        /// </returns>
        public virtual Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof(IPublishedContent);
        }
    }
}