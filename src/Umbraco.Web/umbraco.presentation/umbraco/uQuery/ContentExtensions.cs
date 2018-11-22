using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.datatype;
using System.Xml;
using Umbraco.Core;

namespace umbraco
{
	/// <summary>
	/// uQuery extensions for the Content object (the Document / Media and Memeber objects derive from Content, hence these extension methods are available to Documents / Media and Members)
	/// </summary>
	public static class ContentExtensions
	{
		/// <summary>
		/// Determines whether the specified content item has property.
		/// </summary>
		/// <param name="item">The content item.</param>
		/// <param name="propertyAlias">The property alias.</param>
		/// <returns>
		/// 	<c>true</c> if the specified content item has property; otherwise, <c>false</c>.
		/// </returns>
		[Obsolete("Use the new Services APIs instead")]
		public static bool HasProperty(this Content item, string propertyAlias)
		{
			var property = item.getProperty(propertyAlias);
			return (property != null);
		}

		/// <summary>
		/// Get a value (of specified type) from a content item's property.
		/// </summary>
		/// <typeparam name="T">The type.</typeparam>
		/// <param name="item">The content item.</param>
		/// <param name="propertyAlias">alias of property to get</param>
		/// <returns>default(T) or property value cast to (T)</returns>
        [Obsolete("Use the new Services APIs instead")]
		public static T GetProperty<T>(this Content item, string propertyAlias)
        {
            // check to see if return object handles it's own object hydration
            if (typeof(uQuery.IGetProperty).IsAssignableFrom(typeof(T)))
            {
                // create new instance of T with empty constructor
                uQuery.IGetProperty t = (uQuery.IGetProperty)Activator.CreateInstance<T>();

                // call method to hydrate the object from a string value
                t.LoadPropertyValue(item.GetProperty<string>(propertyAlias));

                return (T)t;
            }

            var typeConverter = TypeDescriptor.GetConverter(typeof(T));
            if (typeConverter != null)
            {
                // Boolean
                if (typeof(T) == typeof(bool))
                {
                    return (T)typeConverter.ConvertFrom(item.GetPropertyAsBoolean(propertyAlias).ToString());
                }

                // XmlDocument
                else if (typeof(T) == typeof(XmlDocument))
                {
                    var xmlDocument = new XmlDocument();

                    try
                    {
                        xmlDocument.LoadXml(item.GetPropertyAsString(propertyAlias));
                    }
                    catch
                    {
                    }

                    return (T)((object)xmlDocument);
                }

//                // umbraco.MacroEngines.DynamicXml
//                else if (typeof(T) == typeof(DynamicXml))
//                {
//                    try
//                    {
//                        return (T)((object)new DynamicXml(item.GetPropertyAsString(propertyAlias)));
//                    }
//                    catch
//                    {
//                    }
//                }

                try
                {
                    return (T)typeConverter.ConvertFromString(item.GetPropertyAsString(propertyAlias));
                }
                catch
                {
                }
            }

            return default(T);
        }

		/// <summary>
		/// Get a string value from a content item's property.
		/// </summary>
		/// <param name="item">The content item.</param>
		/// <param name="propertyAlias">alias of propety to get</param>
		/// <returns>
		/// empty string, or property value as string
		/// </returns>
        [Obsolete("Use the new Services APIs instead")]
		private static string GetPropertyAsString(this Content item, string propertyAlias)
		{
			var propertyValue = string.Empty;
			var property = item.getProperty(propertyAlias);

			if (property != null && property.Value != null)
			{
				propertyValue = Convert.ToString(property.Value);
			}

			return propertyValue;
		}

		/// <summary>
		/// Get a boolean value from a content item's property, (works with built in Yes/No dataype).
		/// </summary>
		/// <param name="item">The content item.</param>
		/// <param name="propertyAlias">alias of propety to get</param>
		/// <returns>
		/// true if can cast value, else false for all other circumstances
		/// </returns>
        [Obsolete("Use the new Services APIs instead")]
        private static bool GetPropertyAsBoolean(this Content item, string propertyAlias)
		{
			var propertyValue = false;
			var property = item.getProperty(propertyAlias);

			if (property != null && property.Value != null)
			{
				// Umbraco yes / no datatype stores a string value of '1' or '0'
				if (Convert.ToString(property.Value) == "1")
				{
					propertyValue = true;
				}
				else
				{
					bool.TryParse(Convert.ToString(property.Value), out propertyValue);
				}
			}

			return propertyValue;
		}

		/// <summary>
		/// Gets the random content item.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <param name="items">The content items.</param>
		/// <returns>
		/// Returns a random content item from a collection of content items.
		/// </returns>
		public static TSource GetRandom<TSource>(this IList<TSource> items)
		{
			return items.RandomOrder().First();
		}

		/// <summary>
		/// Gets a collection of random content items.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <param name="items">The content items.</param>
		/// <param name="numberOfItems">The number of items.</param>
		/// <returns>
		/// Returns the specified number of random content items from a collection of content items.
		/// </returns>
		public static IEnumerable<TSource> GetRandom<TSource>(this ICollection<TSource> items, int numberOfItems)
		{
			if (numberOfItems > items.Count)
			{
				numberOfItems = items.Count;
			}

			return items.RandomOrder().Take(numberOfItems);
		}

		/// <summary>
		/// Sorts the by property.
		/// </summary>
		/// <typeparam name="T">The type of the property.</typeparam>
		/// <param name="items">The items.</param>
		/// <param name="propertyAlias">The property alias.</param>
		/// <returns></returns>
        [Obsolete("Use the new Services APIs instead")]
		public static IEnumerable<Content> OrderByProperty<T>(this IEnumerable<Content> items, string propertyAlias)
		{
			return items.OrderBy(x => x.GetProperty<T>(propertyAlias));

			////// [LK] Long-winded way! :-)
			////var tmp = new Dictionary<Content, T>();

			////foreach (var item in items)
			////{
			////    var property = item.GetProperty<T>(propertyAlias);
			////    if (property != null)
			////    {
			////        tmp.Add(item, property);
			////    }
			////}

			////return tmp.OrderBy(x => x.Value).Select(x => x.Key);
		}

		/// <summary>
		/// Orders the by property descending.
		/// </summary>
		/// <typeparam name="T">The type of the property.</typeparam>
		/// <param name="items">The items.</param>
		/// <param name="propertyAlias">The property alias.</param>
		/// <returns></returns>
        [Obsolete("Use the new Services APIs instead")]
		public static IEnumerable<Content> OrderByPropertyDescending<T>(this IEnumerable<Content> items, string propertyAlias)
		{
			return items.OrderByDescending(x => x.GetProperty<T>(propertyAlias));
		}

		/// <summary>
		/// Randomizes the order of the content items.
		/// </summary>
		/// <typeparam name="TSource">The type of the source.</typeparam>
		/// <param name="items">The content items.</param>
		/// <returns>Returns a list of content items in a random order.</returns>
		public static IEnumerable<TSource> RandomOrder<TSource>(this IEnumerable<TSource> items)
		{
			var random = umbraco.library.GetRandom();
			return items.OrderBy(x => (random.Next()));
		}

		/// <summary>
		/// Sets a property value, and returns self.
		/// </summary>
		/// <param name="item">The content item.</param>
		/// <param name="propertyAlias">The alias of property to set.</param>
		/// <param name="value">The value to set.</param>
		/// <returns>
		/// The same content item on which this is an extension method.
		/// </returns>
        [Obsolete("Use the new Services APIs instead")]
		public static Content SetProperty(this Content item, string propertyAlias, object value)
		{
			var property = item.getProperty(propertyAlias);

			if (property != null)
			{
				if (value != null)
				{
					var dataTypeGuid = property.PropertyType.DataTypeDefinition.DataType.Id.ToString();

					// switch based on datatype of property being set - if setting a built in ddl or radion button list, then string supplied is checked against prevalues
					switch (dataTypeGuid.ToUpper())
					{
						case Constants.PropertyEditors.DropDownList: // DropDownList
						case Constants.PropertyEditors.RadioButtonList: // RadioButtonList

							var preValues = PreValues.GetPreValues(property.PropertyType.DataTypeDefinition.Id);
							PreValue preValue = null;

							// switch based on the supplied value type
							switch (Type.GetTypeCode(value.GetType()))
							{
								case TypeCode.String:
									// attempt to get prevalue from the label
									preValue = preValues.Values.Cast<PreValue>().Where(x => x.Value == (string)value).FirstOrDefault();
									break;

								case TypeCode.Int16:
								case TypeCode.Int32:
									// attempt to get prevalue from the id
									preValue = preValues.Values.Cast<PreValue>().Where(x => x.Id == (int)value).FirstOrDefault();
									break;
							}

							if (preValue != null)
							{
								// check db field type being saved to and store prevalue id as an int or a string - note can never save a prevalue id to a date field ! 
								switch (((DefaultData)property.PropertyType.DataTypeDefinition.DataType.Data).DatabaseType)
								{
									case DBTypes.Ntext:
									case DBTypes.Nvarchar:
										property.Value = preValue.Id.ToString();
										break;

									case DBTypes.Integer:
										property.Value = preValue.Id;
										break;
								}
							}

							break;

						case Constants.PropertyEditors.Date: // Date (NOTE: currently assumes database type is set to Date)

							switch (Type.GetTypeCode(value.GetType()))
							{
								case TypeCode.DateTime:
									property.Value = ((DateTime)value).Date;
									break;
								case TypeCode.String:
									DateTime valueDateTime;
									if (DateTime.TryParse((string)value, out valueDateTime))
									{
										property.Value = valueDateTime.Date;
									};
									break;
							}

							break;

						default:
							// This saves the property value
							property.Value = value;
							break;
					}
				}
				else
				{
					// the value is NULL
					property.Value = value;
				}
			}

			item.Save();

			return item;
		}
	}
}