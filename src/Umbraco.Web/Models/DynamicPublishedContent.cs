using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using System.Reflection;
using System.Xml.Linq;
using umbraco.cms.businesslogic;

namespace Umbraco.Web.Models
{

	/// <summary>
	/// The base dynamic model for views
	/// </summary>
	public class DynamicPublishedContent : DynamicPublishedContentBase
	{
		/// <summary>
		/// This callback is used only so we can set it dynamically for use in unit tests
		/// </summary>
		internal static Func<string, string, Guid> GetDataTypeCallback = (docTypeAlias, propertyAlias) =>
			ContentType.GetDataType(docTypeAlias, propertyAlias);

		public DynamicPublishedContent(IPublishedContent node)
			: base(node)
		{
		}

		/// <summary>
		/// overriden method which uses PropertyEditorValueConverters to convert the resulting value
		/// </summary>
		/// <param name="binder"></param>
		/// <returns></returns>
		protected override Attempt<object> TryGetUserProperty(GetMemberBinder binder)
		{
			var name = binder.Name;
			var recursive = false;
			if (name.StartsWith("_"))
			{
				name = name.Substring(1, name.Length - 1);
				recursive = true;
			}

			var userProperty = GetUserProperty(name, recursive);

			if (userProperty == null)
			{
				return Attempt<object>.False;
			}

			var result = userProperty.Value;

			if (PublishedContent.DocumentTypeAlias == null && userProperty.Alias == null)
			{
				throw new InvalidOperationException("No node alias or property alias available. Unable to look up the datatype of the property you are trying to fetch.");
			}

			//get the data type id for the current property
			var dataType = GetDataType(userProperty.DocumentTypeAlias, userProperty.Alias);

			//convert the string value to a known type
			var converted = ConvertPropertyValue(result, dataType, userProperty.DocumentTypeAlias, userProperty.Alias);
			if (converted.Success)
			{
				result = converted.Result;
			}

			return new Attempt<object>(true, result);
		}

		private static Guid GetDataType(string docTypeAlias, string propertyAlias)
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
		private Attempt<object> ConvertPropertyValue(object currentValue, Guid dataType, string docTypeAlias, string propertyTypeAlias)
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
					var e = XElement.Parse(DynamicXml.StripDashesInElementOrAttributeNames(sResult), LoadOptions.None);

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

		#region Index/Position
		public int Position()
		{
			return Umbraco.Web.PublishedContentExtensions.Position(this);
		}
		public int Index()
		{
			return Umbraco.Web.PublishedContentExtensions.Index(this);
		} 
		#endregion

		#region Is Helpers
		public bool IsNull(string alias, bool recursive)
		{
			return this.PublishedContent.IsNull(alias, recursive);
		}
		public bool IsNull(string alias)
		{
			return this.PublishedContent.IsNull(alias, false);
		}
		public bool IsFirst()
		{
			return this.PublishedContent.IsFirst();
		}
		public HtmlString IsFirst(string valueIfTrue)
		{
			return this.PublishedContent.IsFirst(valueIfTrue);
		}
		public HtmlString IsFirst(string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsFirst(valueIfTrue, valueIfFalse);
		}
		public bool IsNotFirst()
		{
			return this.PublishedContent.IsNotFirst();
		}
		public HtmlString IsNotFirst(string valueIfTrue)
		{
			return this.PublishedContent.IsNotFirst(valueIfTrue);
		}
		public HtmlString IsNotFirst(string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsNotFirst(valueIfTrue, valueIfFalse);
		}
		public bool IsPosition(int index)
		{
			return this.PublishedContent.IsPosition(index);
		}
		public HtmlString IsPosition(int index, string valueIfTrue)
		{
			return this.PublishedContent.IsPosition(index, valueIfTrue);
		}
		public HtmlString IsPosition(int index, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsPosition(index, valueIfTrue, valueIfFalse);
		}
		public bool IsModZero(int modulus)
		{
			return this.PublishedContent.IsModZero(modulus);
		}
		public HtmlString IsModZero(int modulus, string valueIfTrue)
		{
			return this.PublishedContent.IsModZero(modulus, valueIfTrue);
		}
		public HtmlString IsModZero(int modulus, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsModZero(modulus, valueIfTrue, valueIfFalse);
		}

		public bool IsNotModZero(int modulus)
		{
			return this.PublishedContent.IsNotModZero(modulus);
		}
		public HtmlString IsNotModZero(int modulus, string valueIfTrue)
		{
			return this.PublishedContent.IsNotModZero(modulus, valueIfTrue);
		}
		public HtmlString IsNotModZero(int modulus, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsNotModZero(modulus, valueIfTrue, valueIfFalse);
		}
		public bool IsNotPosition(int index)
		{
			return this.PublishedContent.IsNotPosition(index);
		}
		public HtmlString IsNotPosition(int index, string valueIfTrue)
		{
			return this.PublishedContent.IsNotPosition(index, valueIfTrue);
		}
		public HtmlString IsNotPosition(int index, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsNotPosition(index, valueIfTrue, valueIfFalse);
		}
		public bool IsLast()
		{
			return this.PublishedContent.IsLast();
		}
		public HtmlString IsLast(string valueIfTrue)
		{
			return this.PublishedContent.IsLast(valueIfTrue);
		}
		public HtmlString IsLast(string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsLast(valueIfTrue, valueIfFalse);
		}
		public bool IsNotLast()
		{
			return this.PublishedContent.IsNotLast();
		}
		public HtmlString IsNotLast(string valueIfTrue)
		{
			return this.PublishedContent.IsNotLast(valueIfTrue);
		}
		public HtmlString IsNotLast(string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsNotLast(valueIfTrue, valueIfFalse);
		}
		public bool IsEven()
		{
			return this.PublishedContent.IsEven();
		}
		public HtmlString IsEven(string valueIfTrue)
		{
			return this.PublishedContent.IsEven(valueIfTrue);
		}
		public HtmlString IsEven(string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsEven(valueIfTrue, valueIfFalse);
		}
		public bool IsOdd()
		{
			return this.PublishedContent.IsOdd();
		}
		public HtmlString IsOdd(string valueIfTrue)
		{
			return this.PublishedContent.IsOdd(valueIfTrue);
		}
		public HtmlString IsOdd(string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsOdd(valueIfTrue, valueIfFalse);
		}
		public bool IsEqual(DynamicPublishedContentBase other)
		{
			return this.PublishedContent.IsEqual(other);
		}
		public HtmlString IsEqual(DynamicPublishedContentBase other, string valueIfTrue)
		{
			return this.PublishedContent.IsEqual(other, valueIfTrue);
		}
		public HtmlString IsEqual(DynamicPublishedContentBase other, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsEqual(other, valueIfTrue, valueIfFalse);
		}
		public bool IsNotEqual(DynamicPublishedContentBase other)
		{
			return this.PublishedContent.IsNotEqual(other);
		}
		public HtmlString IsNotEqual(DynamicPublishedContentBase other, string valueIfTrue)
		{
			return this.PublishedContent.IsNotEqual(other, valueIfTrue);
		}
		public HtmlString IsNotEqual(DynamicPublishedContentBase other, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsNotEqual(other, valueIfTrue, valueIfFalse);
		}
		public bool IsDescendant(DynamicPublishedContentBase other)
		{
			return this.PublishedContent.IsDescendant(other);
		}
		public HtmlString IsDescendant(DynamicPublishedContentBase other, string valueIfTrue)
		{
			return this.PublishedContent.IsDescendant(other, valueIfTrue);
		}
		public HtmlString IsDescendant(DynamicPublishedContentBase other, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsDescendant(other, valueIfTrue, valueIfFalse);
		}
		public bool IsDescendantOrSelf(DynamicPublishedContentBase other)
		{
			return this.PublishedContent.IsDescendantOrSelf(other);
		}
		public HtmlString IsDescendantOrSelf(DynamicPublishedContentBase other, string valueIfTrue)
		{
			return this.PublishedContent.IsDescendantOrSelf(other, valueIfTrue);
		}
		public HtmlString IsDescendantOrSelf(DynamicPublishedContentBase other, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsDescendantOrSelf(other, valueIfTrue, valueIfFalse);
		}
		public bool IsAncestor(DynamicPublishedContentBase other)
		{
			return this.PublishedContent.IsAncestor(other);
		}
		public HtmlString IsAncestor(DynamicPublishedContentBase other, string valueIfTrue)
		{
			return this.PublishedContent.IsAncestor(other, valueIfTrue);
		}
		public HtmlString IsAncestor(DynamicPublishedContentBase other, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsAncestor(other, valueIfTrue, valueIfFalse);
		}
		public bool IsAncestorOrSelf(DynamicPublishedContentBase other)
		{
			return this.PublishedContent.IsAncestorOrSelf(other);
		}
		public HtmlString IsAncestorOrSelf(DynamicPublishedContentBase other, string valueIfTrue)
		{
			return this.PublishedContent.IsAncestorOrSelf(other, valueIfTrue);
		}
		public HtmlString IsAncestorOrSelf(DynamicPublishedContentBase other, string valueIfTrue, string valueIfFalse)
		{
			return this.PublishedContent.IsAncestorOrSelf(other, valueIfTrue, valueIfFalse);
		}		
		#endregion

		#region Traversal
		public DynamicPublishedContent Up()
		{
			return Umbraco.Web.PublishedContentExtensions.Up(this).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Up(int number)
		{
			return Umbraco.Web.PublishedContentExtensions.Up(this, number).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Up(string nodeTypeAlias)
		{
			return Umbraco.Web.PublishedContentExtensions.Up(this, nodeTypeAlias).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Down()
		{
			return Umbraco.Web.PublishedContentExtensions.Down(this).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Down(int number)
		{
			return Umbraco.Web.PublishedContentExtensions.Down(this, number).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Down(string nodeTypeAlias)
		{
			return Umbraco.Web.PublishedContentExtensions.Down(this, nodeTypeAlias).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Next()
		{
			return Umbraco.Web.PublishedContentExtensions.Next(this).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Next(int number)
		{
			return Umbraco.Web.PublishedContentExtensions.Next(this, number).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Next(string nodeTypeAlias)
		{
			return Umbraco.Web.PublishedContentExtensions.Next(this, nodeTypeAlias).AsDynamicPublishedContent();
		}

		public DynamicPublishedContent Previous()
		{
			return Umbraco.Web.PublishedContentExtensions.Previous(this).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Previous(int number)
		{
			return Umbraco.Web.PublishedContentExtensions.Previous(this, number).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Previous(string nodeTypeAlias)
		{
			return Umbraco.Web.PublishedContentExtensions.Previous(this, nodeTypeAlias).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Sibling(int number)
		{
			return Umbraco.Web.PublishedContentExtensions.Previous(this, number).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent Sibling(string nodeTypeAlias)
		{
			return Umbraco.Web.PublishedContentExtensions.Previous(this, nodeTypeAlias).AsDynamicPublishedContent();
		} 
		#endregion

		#region Ancestors, Descendants and Parent
		#region Ancestors
		public DynamicPublishedContentList Ancestors(int level)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.Ancestors(this, level));
		}
		public DynamicPublishedContentList Ancestors(string nodeTypeAlias)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.Ancestors(this, nodeTypeAlias));
		}
		public DynamicPublishedContentList Ancestors()
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.Ancestors(this));
		}
		public DynamicPublishedContentList Ancestors(Func<IPublishedContent, bool> func)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.Ancestors(this, func));
		}
		public DynamicPublishedContent AncestorOrSelf()
		{
			return Umbraco.Web.PublishedContentExtensions.AncestorOrSelf(this).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent AncestorOrSelf(int level)
		{
			return Umbraco.Web.PublishedContentExtensions.AncestorOrSelf(this, level).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent AncestorOrSelf(string nodeTypeAlias)
		{
			return Umbraco.Web.PublishedContentExtensions.AncestorOrSelf(this, nodeTypeAlias).AsDynamicPublishedContent();
		}
		public DynamicPublishedContent AncestorOrSelf(Func<IPublishedContent, bool> func)
		{
			return Umbraco.Web.PublishedContentExtensions.AncestorOrSelf(this, func).AsDynamicPublishedContent();
		}
		public DynamicPublishedContentList AncestorsOrSelf(Func<IPublishedContent, bool> func)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.AncestorsOrSelf(this, func));
		}
		public DynamicPublishedContentList AncestorsOrSelf()
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.AncestorsOrSelf(this));
		}
		public DynamicPublishedContentList AncestorsOrSelf(string nodeTypeAlias)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.AncestorsOrSelf(this, nodeTypeAlias));
		}
		public DynamicPublishedContentList AncestorsOrSelf(int level)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.AncestorsOrSelf(this, level));
		} 
		#endregion
		#region Descendants
		public DynamicPublishedContentList Descendants(string nodeTypeAlias)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.Descendants(this, nodeTypeAlias));
		}
		public DynamicPublishedContentList Descendants(int level)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.Descendants(this, level));
		}
		public DynamicPublishedContentList Descendants()
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.Descendants(this));
		}
		public DynamicPublishedContentList DescendantsOrSelf(int level)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.DescendantsOrSelf(this, level));
		}
		public DynamicPublishedContentList DescendantsOrSelf(string nodeTypeAlias)
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.DescendantsOrSelf(this, nodeTypeAlias));
		}
		public DynamicPublishedContentList DescendantsOrSelf()
		{
			return new DynamicPublishedContentList(
				Umbraco.Web.PublishedContentExtensions.DescendantsOrSelf(this));
		} 
		#endregion

		public DynamicPublishedContent Parent
		{
			get
			{
				if (PublishedContent.Parent != null)
				{
					return PublishedContent.Parent.AsDynamicPublishedContent();
				}
				if (PublishedContent != null && PublishedContent.Id == 0)
				{
					return this;
				}
				return null;
			}
		} 
		
		#endregion
		
		
	}
}
