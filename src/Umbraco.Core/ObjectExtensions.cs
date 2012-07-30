using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using System.Xml;

namespace Umbraco.Core
{
	internal static class ObjectExtensions
	{
		//private static readonly ConcurrentDictionary<Type, Func<object>> ObjectFactoryCache = new ConcurrentDictionary<Type, Func<object>>();

		public static IEnumerable<T> AsEnumerableOfOne<T>(this T input)
		{
			return Enumerable.Repeat(input, 1);
		}

		public static void DisposeIfDisposable(this object input)
		{
			var disposable = input as IDisposable;
			if (disposable != null) disposable.Dispose();
		}

		/// <summary>
		/// Provides a shortcut way of safely casting an input when you cannot guarantee the <typeparam name="T"></typeparam> is an instance type (i.e., when the C# AS keyword is not applicable)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		public static T SafeCast<T>(this object input)
		{
			if (ReferenceEquals(null, input) || ReferenceEquals(default(T), input)) return default(T);
			if (input is T) return (T)input;
			return default(T);
		}

		/// <summary>
		/// Tries to convert the input object to the output type using TypeConverters
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="input"></param>
		/// <returns></returns>
		public static Attempt<T> TryConvertTo<T>(this object input)
		{
			var result = TryConvertTo(input, typeof(T));
			return !result.Success ? Attempt<T>.False : new Attempt<T>(true, (T)result.Result);
		}

		/// <summary>
		/// Tries to convert the input object to the output type using TypeConverters. If the destination type is a superclass of the input type,
		/// if will use <see cref="Convert.ChangeType(object,System.Type)"/>.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="destinationType">Type of the destination.</param>
		/// <returns></returns>
		public static Attempt<object> TryConvertTo(this object input, Type destinationType)
		{
			if (input == null) return Attempt<object>.False;

			if (destinationType == typeof(object)) return new Attempt<object>(true, input);

			if (input.GetType() == destinationType) return new Attempt<object>(true, input);

			if (!destinationType.IsGenericType || destinationType.GetGenericTypeDefinition() != typeof(Nullable<>))
			{
				if (TypeHelper.IsTypeAssignableFrom(destinationType, input.GetType())
					&& TypeHelper.IsTypeAssignableFrom<IConvertible>(input))
				{
					var casted = Convert.ChangeType(input, destinationType);
					return new Attempt<object>(true, casted);
				}
			}

			var inputConverter = TypeDescriptor.GetConverter(input);
			if (inputConverter != null)
			{
				if (inputConverter.CanConvertTo(destinationType))
				{
					return new Attempt<object>(true, inputConverter.ConvertTo(input, destinationType));
				}
			}

			if (destinationType == typeof(bool))
			{
				var boolConverter = new CustomBooleanTypeConverter();
				if (boolConverter.CanConvertFrom(input.GetType()))
				{
					return new Attempt<object>(true, boolConverter.ConvertFrom(input));
				}
			}

			var outputConverter = TypeDescriptor.GetConverter(destinationType);
			if (outputConverter != null)
			{
				if (outputConverter.CanConvertFrom(input.GetType()))
				{
					return new Attempt<object>(true, outputConverter.ConvertFrom(input));
				}
			}

			try
			{
				if (TypeHelper.IsTypeAssignableFrom<IConvertible>(input))
				{
					var casted = Convert.ChangeType(input, destinationType);
					return new Attempt<object>(true, casted);
				}
			}
			catch (Exception)
			{
				/* Swallow */
			}

			return Attempt<object>.False;
		}

		public static void CheckThrowObjectDisposed(this IDisposable disposable, bool isDisposed, string objectname)
		{
			//TODO: Localise this exception
			if (isDisposed)
				throw new ObjectDisposedException(objectname);
		}

		//public enum PropertyNamesCaseType
		//{
		//    CamelCase,
		//    CaseInsensitive
		//}
		
		///// <summary>
		///// Convert an object to a JSON string with camelCase formatting
		///// </summary>
		///// <param name="obj"></param>
		///// <returns></returns>
		//public static string ToJsonString(this object obj)
		//{
		//    return obj.ToJsonString(PropertyNamesCaseType.CamelCase);
		//}

		///// <summary>
		///// Convert an object to a JSON string with the specified formatting
		///// </summary>
		///// <param name="obj">The obj.</param>
		///// <param name="propertyNamesCaseType">Type of the property names case.</param>
		///// <returns></returns>
		//public static string ToJsonString(this object obj, PropertyNamesCaseType propertyNamesCaseType)
		//{
		//    var type = obj.GetType();
		//    var dateTimeStyle = "yyyy-MM-dd HH:mm:ss";

		//    if (type.IsPrimitive || typeof(string).IsAssignableFrom(type))
		//    {
		//        return obj.ToString();
		//    }

		//    if (typeof(DateTime).IsAssignableFrom(type) || typeof(DateTimeOffset).IsAssignableFrom(type))
		//    {
		//        return Convert.ToDateTime(obj).ToString(dateTimeStyle);
		//    }

		//    var serializer = new JsonSerializer();

		//    switch (propertyNamesCaseType)
		//    {
		//        case PropertyNamesCaseType.CamelCase:
		//            serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
		//            break;
		//    }

		//    var dateTimeConverter = new IsoDateTimeConverter
		//        {
		//            DateTimeStyles = System.Globalization.DateTimeStyles.None,
		//            DateTimeFormat = dateTimeStyle
		//        };

		//    if (typeof(IDictionary).IsAssignableFrom(type))
		//    {
		//        return JObject.FromObject(obj, serializer).ToString(Formatting.None, dateTimeConverter);
		//    }

		//    if (type.IsArray || (typeof(IEnumerable).IsAssignableFrom(type)))
		//    {
		//        return JArray.FromObject(obj, serializer).ToString(Formatting.None, dateTimeConverter);
		//    }

		//    return JObject.FromObject(obj, serializer).ToString(Formatting.None, dateTimeConverter);
		//}
		
		/// <summary>
		/// Converts an object into a dictionary
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TProperty"></typeparam>
		/// <typeparam name="TVal"> </typeparam>
		/// <param name="o"></param>
		/// <param name="ignoreProperties"></param>
		/// <returns></returns>
		public static IDictionary<string, TVal> ToDictionary<T, TProperty, TVal>(this T o,
		                                                                         params Expression<Func<T, TProperty>>[] ignoreProperties)
		{
			return o.ToDictionary<TVal>(ignoreProperties.Select(e => o.GetPropertyInfo(e)).Select(propInfo => propInfo.Name).ToArray());
		}

		/// <summary>
		/// Turns object into dictionary
		/// </summary>
		/// <param name="o"></param>
		/// <param name="ignoreProperties">Properties to ignore</param>
		/// <returns></returns>
		public static IDictionary<string, TVal> ToDictionary<TVal>(this object o, params string[] ignoreProperties)
		{
			if (o != null)
			{
				var props = TypeDescriptor.GetProperties(o);
				var d = new Dictionary<string, TVal>();
				foreach (var prop in props.Cast<PropertyDescriptor>().Where(x => !ignoreProperties.Contains(x.Name)))
				{
					var val = prop.GetValue(o);
					if (val != null)
					{
						d.Add(prop.Name, (TVal)val);
					}
				}
				return d;
			}
			return new Dictionary<string, TVal>();
		}

		public static string ToDebugString(this object obj, int levels = 0)
		{
			if (obj == null) return "{null}";
			try
			{
				if (obj is string)
				{
					return "\"{0}\"".InvariantFormat(obj);
				}
				if (obj is int || obj is Int16 || obj is Int64 || obj is double || obj is bool || obj is int? || obj is Int16? || obj is Int64? || obj is double? || obj is bool?)
				{
					return "{0}".InvariantFormat(obj);
				}
				if (obj is Enum)
				{
					return "[{0}]".InvariantFormat(obj);
				}
				if (obj is IEnumerable)
				{
					var enumerable = (obj as IEnumerable);

					var items = (from object enumItem in enumerable let value = GetEnumPropertyDebugString(enumItem, levels) where value != null select value).Take(10).ToList();

					return items.Count() > 0
					       	? "{{ {0} }}".InvariantFormat(String.Join(", ", items))
					       	: null;
				}

				var props = obj.GetType().GetProperties();
				if ((props.Count() == 2) && props[0].Name == "Key" && props[1].Name == "Value" && levels > -2)
				{
					try
					{
						var key = props[0].GetValue(obj, null) as string;
						var value = props[1].GetValue(obj, null).ToDebugString(levels - 1);
						return "{0}={1}".InvariantFormat(key, value);
					}
					catch (Exception)
					{
						return "[KeyValuePropertyException]";
					}
				}
				if (levels > -1)
				{
					var items =
						from propertyInfo in props
						let value = GetPropertyDebugString(propertyInfo, obj, levels)
						where value != null
						select "{0}={1}".InvariantFormat(propertyInfo.Name, value);

					return items.Count() > 0
					       	? "[{0}]:{{ {1} }}".InvariantFormat(obj.GetType().Name, String.Join(", ", items))
					       	: null;
				}
			}
			catch (Exception ex)
			{
				return "[Exception:{0}]".InvariantFormat(ex.Message);
			}
			return null;
		}


		/// <summary>
		/// Attempts to serialize the value to an XmlString using ToXmlString
		/// </summary>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Attempt<string> TryConvertToXmlString(this object value, Type type)
		{
			try
			{
				var output = value.ToXmlString(type);
				return new Attempt<string>(true, output);
			}
			catch (NotSupportedException ex)
			{
				return new Attempt<string>(ex);
			}
		}

		/// <summary>
		/// Returns an XmlSerialized safe string representation for the value
		/// </summary>
		/// <param name="value"></param>
		/// <param name="type">The Type can only be a primitive type or Guid and byte[] otherwise an exception is thrown</param>
		/// <returns></returns>
		public static string ToXmlString(this object value, Type type)
		{
			if (type == typeof(string)) return ((string)value).IsNullOrWhiteSpace() ? "" : (string)value;
			if (type == typeof(bool)) return XmlConvert.ToString((bool)value);
			if (type == typeof(byte)) return XmlConvert.ToString((byte)value);
			if (type == typeof(char)) return XmlConvert.ToString((char)value);
			if (type == typeof(DateTime)) return XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.RoundtripKind);
			if (type == typeof(DateTimeOffset)) return XmlConvert.ToString((DateTimeOffset)value);
			if (type == typeof(decimal)) return XmlConvert.ToString((decimal)value);
			if (type == typeof(double)) return XmlConvert.ToString((double)value);
			if (type == typeof(float)) return XmlConvert.ToString((float)value);
			if (type == typeof(Guid)) return XmlConvert.ToString((Guid)value);
			if (type == typeof(int)) return XmlConvert.ToString((int)value);
			if (type == typeof(long)) return XmlConvert.ToString((long)value);
			if (type == typeof(sbyte)) return XmlConvert.ToString((sbyte)value);
			if (type == typeof(short)) return XmlConvert.ToString((short)value);
			if (type == typeof(TimeSpan)) return XmlConvert.ToString((TimeSpan)value);
			if (type == typeof(bool)) return XmlConvert.ToString((bool)value);
			if (type == typeof(uint)) return XmlConvert.ToString((uint)value);
			if (type == typeof(ulong)) return XmlConvert.ToString((ulong)value);
			if (type == typeof(ushort)) return XmlConvert.ToString((ushort)value);

			throw new NotSupportedException("Cannot convert type " + type.FullName + " to a string using ToXmlString as it is not supported by XmlConvert");
		}

		private static string GetEnumPropertyDebugString(object enumItem, int levels)
		{
			try
			{
				return enumItem.ToDebugString(levels - 1);
			}
			catch (Exception)
			{
				return "[GetEnumPartException]";
			}
		}

		private static string GetPropertyDebugString(PropertyInfo propertyInfo, object obj, int levels)
		{
			try
			{
				return propertyInfo.GetValue(obj, null).ToDebugString(levels - 1);
			}
			catch (Exception)
			{
				return "[GetPropertyValueException]";
			}
		}

	}
}