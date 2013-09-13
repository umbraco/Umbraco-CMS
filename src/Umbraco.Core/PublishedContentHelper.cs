using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using log4net.Util.TypeConverters;

namespace Umbraco.Core
{

    /// <summary>
    /// Utility class for dealing with data types and value conversions
    /// </summary>
    /// <remarks>
    /// TODO: The logic for the GetDataType + cache should probably be moved to a service, no ? 
    /// 
    /// We inherit from ApplicationEventHandler so we can bind to the ContentTypeService events to ensure that our local cache
    /// object gets cleared when content types change.
    /// </remarks>
    internal class PublishedContentHelper : ApplicationEventHandler
	{
        /// <summary>
        /// Used to invalidate the cache from the ICacherefresher
        /// </summary>
        internal static void ClearPropertyTypeCache()
        {
            PropertyTypeCache.Clear();
        }

        /// <summary>
        /// This callback is used only for unit tests which enables us to return any data we want and not rely on having the data in a database
        /// </summary>
        internal static Func<string, string, string> GetDataTypeCallback = null;

        private static readonly ConcurrentDictionary<Tuple<string, string, PublishedItemType>, string> PropertyTypeCache = new ConcurrentDictionary<Tuple<string, string, PublishedItemType>, string>();

        /// <summary>
        /// Return the GUID Id for the data type assigned to the document type with the property alias
        /// </summary>
        /// <param name="applicationContext"></param>
        /// <param name="docTypeAlias"></param>
        /// <param name="propertyAlias"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        internal static string GetPropertyEditor(ApplicationContext applicationContext, string docTypeAlias, string propertyAlias, PublishedItemType itemType)
        {
            if (GetDataTypeCallback != null)
                return GetDataTypeCallback(docTypeAlias, propertyAlias);

            var key = new Tuple<string, string, PublishedItemType>(docTypeAlias, propertyAlias, itemType);
            return PropertyTypeCache.GetOrAdd(key, tuple =>
                {
                    IContentTypeComposition result = null;
                    switch (itemType)
                    {
                        case PublishedItemType.Content:
                            result = applicationContext.Services.ContentTypeService.GetContentType(docTypeAlias);                            
                            break;
                        case PublishedItemType.Media:
                            result = applicationContext.Services.ContentTypeService.GetMediaType(docTypeAlias);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("itemType");
                    }
                    
                    if (result == null) return string.Empty;
                    
                    //SD: we need to check for 'any' here because the collection is backed by KeyValuePair which is a struct
                    // and can never be null so FirstOrDefault doesn't actually work. Have told Seb and Morten about thsi 
                    // issue.
                    if (!result.CompositionPropertyTypes.Any(x => x.Alias.InvariantEquals(propertyAlias)))
                    {
                        return string.Empty;
                    }
                    var property = result.CompositionPropertyTypes.FirstOrDefault(x => x.Alias.InvariantEquals(propertyAlias));
                    //as per above, this will never be null but we'll keep the check here anyways.
                    if (property == null) return string.Empty;
                    return property.PropertyEditorAlias;
                });
		}

        /// <summary>
        /// Converts the currentValue to a correctly typed value based on known registered converters, then based on known standards.
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="propertyDefinition"></param>
        /// <returns></returns>
        internal static Attempt<object> ConvertPropertyValue(object currentValue, PublishedPropertyDefinition propertyDefinition)
		{
			if (currentValue == null) return Attempt<object>.False;

            //First, we need to check the v7+ PropertyValueConverters
		    var converters = PropertyValueConvertersResolver.Current.Converters
                                                            .Where(x => x.AssociatedPropertyEditorAlias == propertyDefinition.PropertyEditorAlias)
		                                                    .ToArray();
            if (converters.Any())
            {
                if (converters.Count() > 1)
                {
                    throw new NotSupportedException("Only one " + typeof(PropertyValueConverter) + " can be registered for the property editor: " + propertyDefinition.PropertyEditorAlias);
                }
                var result = converters.Single().ConvertSourceToObject(
                    currentValue,
                    propertyDefinition, 
                    false);

                //if it is good return it, otherwise we'll continue processing the legacy stuff below.
                if (result.Success)
                {
                    return new Attempt<object>(true, result.Result);
                }
            }

            //In order to maintain backwards compatibility here with IPropertyEditorValueConverter we need to attempt to lookup the 
            // legacy GUID for the current property editor. If one doesn't exist then we will abort the conversion.
            var legacyId = LegacyPropertyEditorIdToAliasConverter.GetLegacyIdFromAlias(propertyDefinition.PropertyEditorAlias);
            if (legacyId.HasValue == false)
            {
                return Attempt<object>.False;
            }

			//First lets check all registered converters for this data type.			
			var legacyConverters = PropertyEditorValueConvertersResolver.Current.Converters
                .Where(x => x.IsConverterFor(legacyId.Value, propertyDefinition.DocumentTypeAlias, propertyDefinition.PropertyTypeAlias))
				.ToArray();

			//try to convert the value with any of the converters:
			foreach (var converted in legacyConverters
				.Select(p => p.ConvertPropertyValue(currentValue))
				.Where(converted => converted.Success))
			{
				return new Attempt<object>(true, converted.Result);
			}

			//if none of the converters worked, then we'll process this from what we know

			var sResult = Convert.ToString(currentValue).Trim();

			//this will eat csv strings, so only do it if the decimal also includes a decimal seperator (according to the current culture)
			if (sResult.Contains(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
			{
				decimal dResult;
				if (decimal.TryParse(sResult, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.CurrentCulture, out dResult))
				{
					return new Attempt<object>(true, dResult);
				}
			}
			//process string booleans as booleans
			if (sResult.InvariantEquals("true"))
			{
				return new Attempt<object>(true, true);
			}
			if (sResult.InvariantEquals("false"))
			{
				return new Attempt<object>(true, false);
			}

			//a really rough check to see if this may be valid xml
			//TODO: This is legacy code, I'm sure there's a better and nicer way
			if (sResult.StartsWith("<") && sResult.EndsWith(">") && sResult.Contains("/"))
			{
				try
				{
                    var e = XElement.Parse(sResult, LoadOptions.None);

					//check that the document element is not one of the disallowed elements
					//allows RTE to still return as html if it's valid xhtml
					var documentElement = e.Name.LocalName;

					//TODO: See note against this setting, pretty sure we don't need this
                    if (UmbracoConfiguration.Current.UmbracoSettings.Scripting.Razor.NotDynamicXmlDocumentElements.Any(
                        tag => string.Equals(tag.Element, documentElement, StringComparison.CurrentCultureIgnoreCase)) == false)
					{
						return new Attempt<object>(true, new DynamicXml(e));
					}
					return Attempt<object>.False;
				}
				catch (Exception)
				{
					return Attempt<object>.False;
				}
			}
			return Attempt<object>.False;
		}
	}
}