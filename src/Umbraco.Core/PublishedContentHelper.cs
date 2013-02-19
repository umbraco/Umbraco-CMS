using System;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dynamics;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core
{
	internal class PublishedContentHelper
	{
		/// <summary>
		/// This callback is used only so we can set it dynamically because in the "Core" project currently we don't have
		/// access to the business logic layer. 
		/// TODO: Once 6.0 is released we need to change this to use the new business logic layer that we can access from 
		///  this proejct. Until then this will return a Guid.Empty but the callback will need to be set in the WebBootManager
		///  to work in the website. if people use this in a non-web aspect without the WebBootManager, the the IPropertyEditorValueConverters
		///  will not be executed.
		/// </summary>
		internal static Func<string, string, Guid> GetDataTypeCallback = (docTypeAlias, propertyAlias) => Guid.Empty;

		internal static Guid GetDataType(string docTypeAlias, string propertyAlias)
		{
			return GetDataTypeCallback(docTypeAlias, propertyAlias);
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
			if (currentValue == null) return Attempt<object>.False;

			//First lets check all registered converters for this data type.			
			var converters = PropertyEditorValueConvertersResolver.Current.Converters
				.Where(x => x.IsConverterFor(dataType, docTypeAlias, propertyTypeAlias))
				.ToArray();

			//try to convert the value with any of the converters:
			foreach (var converted in converters
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
					if (!UmbracoSettings.NotDynamicXmlDocumentElements.Any(
						tag => string.Equals(tag, documentElement, StringComparison.CurrentCultureIgnoreCase)))
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