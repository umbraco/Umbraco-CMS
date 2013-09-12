using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

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
        internal static Func<string, string, Guid> GetDataTypeCallback = null;

        private static readonly ConcurrentDictionary<Tuple<string, string, PublishedItemType>, Guid> PropertyTypeCache = new ConcurrentDictionary<Tuple<string, string, PublishedItemType>, Guid>();

        /// <summary>
        /// Return the GUID Id for the data type assigned to the document type with the property alias
        /// </summary>
        /// <param name="applicationContext"></param>
        /// <param name="docTypeAlias"></param>
        /// <param name="propertyAlias"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        internal static Guid GetDataType(ApplicationContext applicationContext, string docTypeAlias, string propertyAlias, PublishedItemType itemType)
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
                    
                    if (result == null) return Guid.Empty;
                    
                    //SD: we need to check for 'any' here because the collection is backed by KeyValuePair which is a struct
                    // and can never be null so FirstOrDefault doesn't actually work. Have told Seb and Morten about thsi 
                    // issue.
                    if (!result.CompositionPropertyTypes.Any(x => x.Alias.InvariantEquals(propertyAlias)))
                    {
                        return Guid.Empty;
                    }
                    var property = result.CompositionPropertyTypes.FirstOrDefault(x => x.Alias.InvariantEquals(propertyAlias));
                    //as per above, this will never be null but we'll keep the check here anyways.
                    if (property == null) return Guid.Empty;
                    return property.DataTypeId;
                });
		}

		/// <summary>
		/// Converts the currentValue to a correctly typed value based on known registered converters, then based on known standards.
		/// </summary>
		/// <param name="currentValue"></param>
		/// <param name="dataType"></param>
		/// <param name="docTypeAlias"></param>
		/// <param name="propertyTypeAlias"></param>
		/// <returns></returns>
		internal static Attempt<object> ConvertPropertyValue(object currentValue, Guid dataType, string docTypeAlias, string propertyTypeAlias)
		{
			if (currentValue == null) return Attempt<object>.Fail();

			//First lets check all registered converters for this data type.			
			var converters = PropertyEditorValueConvertersResolver.Current.Converters
				.Where(x => x.IsConverterFor(dataType, docTypeAlias, propertyTypeAlias))
				.ToArray();

			//try to convert the value with any of the converters:
			foreach (var converted in converters
				.Select(p => p.ConvertPropertyValue(currentValue))
				.Where(converted => converted.Success))
			{
				return Attempt.Succeed(converted.Result);
			}

			//if none of the converters worked, then we'll process this from what we know

			var sResult = Convert.ToString(currentValue).Trim();

			//this will eat csv strings, so only do it if the decimal also includes a decimal seperator (according to the current culture)
			if (sResult.Contains(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
			{
				decimal dResult;
				if (decimal.TryParse(sResult, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.CurrentCulture, out dResult))
				{
					return Attempt<object>.Succeed(dResult);
				}
			}
			//process string booleans as booleans
			if (sResult.InvariantEquals("true"))
			{
				return Attempt<object>.Succeed(true);
			}
			if (sResult.InvariantEquals("false"))
			{
				return Attempt<object>.Succeed(false);
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
					if (!UmbracoSettings.NotDynamicXmlDocumentElements.Any(
						tag => string.Equals(tag, documentElement, StringComparison.CurrentCultureIgnoreCase)))
					{
						return Attempt<object>.Succeed(new DynamicXml(e));
					}
					return Attempt<object>.Fail();
				}
				catch (Exception)
				{
					return Attempt<object>.Fail();
				}
			}
			return Attempt<object>.Fail();
		}
	}
}