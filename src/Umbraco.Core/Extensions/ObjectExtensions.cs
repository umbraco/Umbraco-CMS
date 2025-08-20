// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Collections;

namespace Umbraco.Extensions;

/// <summary>
/// Provides object extension methods.
/// </summary>
public static class ObjectExtensions
{
    private static readonly ConcurrentDictionary<Type, Type?> _nullableGenericCache = new();
    private static readonly ConcurrentDictionary<CompositeTypeTypeKey, TypeConverter?> _inputTypeConverterCache = new();
    private static readonly ConcurrentDictionary<CompositeTypeTypeKey, TypeConverter?> _destinationTypeConverterCache = new();
    private static readonly ConcurrentDictionary<CompositeTypeTypeKey, bool> _assignableTypeCache = new();
    private static readonly ConcurrentDictionary<Type, bool> _boolConvertCache = new();
    private static readonly char[] _numberDecimalSeparatorsToNormalize = ['.', ','];
    private static readonly CustomBooleanTypeConverter _customBooleanTypeConverter = new();

    /// <summary>
    /// Returns an XML serialized safe string representation for the value and type.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>
    /// The XML serialized safe string representation.
    /// </returns>
    public static string ToXmlString<T>(this object value) => value.ToXmlString(typeof(T));

    /// <summary>
    /// Returns an XML serialized safe string representation for the value and type.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="type">The type of the value. This can only be a primitive type or <see cref="Guid" /> and <see cref="T:byte[]" />, otherwise an exception is thrown.</param>
    /// <returns>
    /// The XML serialized safe string representation.
    /// </returns>
    public static string ToXmlString(this object value, Type type)
    {
        if (value == null)
        {
            return string.Empty;

        }

        if (type == typeof(string))
        {
            return value.ToString().OrIfNullOrWhiteSpace(string.Empty);
        }

        if (type == typeof(bool))
        {
            return XmlConvert.ToString((bool)value);
        }

        if (type == typeof(byte))
        {
            return XmlConvert.ToString((byte)value);
        }

        if (type == typeof(char))
        {
            return XmlConvert.ToString((char)value);
        }

        if (type == typeof(DateTime))
        {
            return XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.Unspecified);
        }

        if (type == typeof(DateTimeOffset))
        {
            return XmlConvert.ToString((DateTimeOffset)value);
        }

        if (type == typeof(decimal))
        {
            return XmlConvert.ToString((decimal)value);
        }

        if (type == typeof(double))
        {
            return XmlConvert.ToString((double)value);
        }

        if (type == typeof(float))
        {
            return XmlConvert.ToString((float)value);
        }

        if (type == typeof(Guid))
        {
            return XmlConvert.ToString((Guid)value);
        }

        if (type == typeof(int))
        {
            return XmlConvert.ToString((int)value);
        }

        if (type == typeof(long))
        {
            return XmlConvert.ToString((long)value);
        }

        if (type == typeof(sbyte))
        {
            return XmlConvert.ToString((sbyte)value);
        }

        if (type == typeof(short))
        {
            return XmlConvert.ToString((short)value);
        }

        if (type == typeof(TimeSpan))
        {
            return XmlConvert.ToString((TimeSpan)value);
        }

        if (type == typeof(uint))
        {
            return XmlConvert.ToString((uint)value);
        }

        if (type == typeof(ulong))
        {
            return XmlConvert.ToString((ulong)value);
        }

        if (type == typeof(ushort))
        {
            return XmlConvert.ToString((ushort)value);
        }

        throw new NotSupportedException($"Cannot convert type {type.FullName} to a string using ToXmlString, as it is not supported by XmlConvert.");
    }

    /// <summary>
    /// Attempts to convert the input object to the output type.
    /// </summary>
    /// <typeparam name="T">The type to convert to.</typeparam>
    /// <param name="input">The input.</param>
    /// <returns>
    /// The <see cref="Attempt{T}" />.
    /// </returns>
    /// <remarks>
    /// This code is an optimized version of the original Umbraco method.
    /// </remarks>
    public static Attempt<T> TryConvertTo<T>(this object? input)
    {
        Attempt<object?> result = TryConvertTo(input, typeof(T));

        if (result.Success)
        {
            return Attempt<T>.Succeed((T?)result.Result);
        }

        if (input == null)
        {
            if (typeof(T).IsValueType)
            {
                // fail, cannot convert null to a value type
                return Attempt<T>.Fail();
            }

            // sure, null can be any object
            return Attempt<T>.Succeed((T)input!);
        }

        // just try to cast
        try
        {
            return Attempt<T>.Succeed((T)input);
        }
        catch (Exception ex)
        {
            return Attempt<T>.Fail(ex);
        }
    }

    /// <summary>
    /// Attempts to convert the input object to the output type.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="target">The type to convert to.</param>
    /// <returns>
    /// The <see cref="T:Attempt{object?}" />.
    /// </returns>
    /// <remarks>
    /// This code is an optimized version of the original Umbraco method.
    /// </remarks>
    public static Attempt<object?> TryConvertTo(this object? input, Type target)
    {
        if (target == null)
        {
            return Attempt<object?>.Fail();
        }

        try
        {
            if (input == null)
            {
                // Nullable is ok
                if (target.IsGenericType && GetCachedGenericNullableType(target) != null)
                {
                    return Attempt<object?>.Succeed(null);
                }

                // Reference types are ok
                return Attempt<object?>.If(target.IsValueType == false, null);
            }

            Type inputType = input.GetType();

            // Easy
            if (target == typeof(object) || inputType == target)
            {
                return Attempt.Succeed(input);
            }

            // Check for string so that overloads of ToString() can take advantage of the conversion.
            if (target == typeof(string))
            {
                return Attempt<object?>.Succeed(input.ToString());
            }

            // If we've got a nullable of something, we try to convert directly to that thing.
            // We cache the destination type and underlying nullable types
            // Any other generic types need to fall through
            if (target.IsGenericType)
            {
                Type? underlying = GetCachedGenericNullableType(target);
                if (underlying != null)
                {
                    // Special case for empty strings for bools/dates which should return null if an empty string.
                    if (input is string inputString)
                    {
                        // TODO: Why the check against only bool/date when a string is null/empty? In what scenario can we convert to another type when the string is null or empty other than just being null?
                        if (string.IsNullOrEmpty(inputString) &&
                            (underlying == typeof(DateTime) || underlying == typeof(bool)))
                        {
                            return Attempt<object?>.Succeed(null);
                        }
                    }

                    // Recursively call into this method with the inner (not-nullable) type and handle the outcome
                    Attempt<object?> inner = input.TryConvertTo(underlying);

                    // And if successful, fall on through to rewrap in a nullable; if failed, pass on the exception
                    if (inner.Success)
                    {
                        input = inner.Result; // Now fall on through...
                    }
                    else
                    {
                        return Attempt<object?>.Fail(inner.Exception);
                    }
                }
            }
            else
            {
                // target is not a generic type
                var inputString = input as string ?? (input is StringValues sv ? sv.ToString() : null);
                if (inputString != null)
                {
                    // Try convert from string or StringValues, returns an Attempt if the string could be
                    // processed (either succeeded or failed), else null if we need to try
                    // other methods
                    Attempt<object?>? result = TryConvertToFromString(inputString, target);
                    if (result.HasValue)
                    {
                        return result.Value;
                    }
                }

                // TODO: Do a check for destination type being IEnumerable<T> and source type implementing IEnumerable<T> with
                // the same 'T', then we'd have to find the extension method for the type AsEnumerable() and execute it.
                if (GetCachedCanAssign(input, inputType, target))
                {
                    return Attempt.Succeed(Convert.ChangeType(input, target));
                }
            }

            if (target == typeof(bool))
            {
                if (GetCachedCanConvertToBoolean(inputType))
                {
                    return Attempt.Succeed(_customBooleanTypeConverter.ConvertFrom(input!));
                }
            }

            if (target == typeof(DateTime) && input is DateTimeOffset dateTimeOffset)
            {
                // IMPORTANT: for compatibility with various editors, we must discard any Offset information and assume UTC time here
                return Attempt.Succeed((object?)new DateTime(
                    new DateOnly(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day),
                    new TimeOnly(dateTimeOffset.Hour, dateTimeOffset.Minute, dateTimeOffset.Second, dateTimeOffset.Millisecond, dateTimeOffset.Microsecond),
                    DateTimeKind.Utc));
            }

            if (target == typeof(DateTimeOffset) && input is DateTime dateTime)
            {
                // IMPORTANT: for compatibility with various editors, we must discard any DateTimeKind information and assume UTC time here
                return Attempt.Succeed((object?)new DateTimeOffset(
                    new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day),
                    new TimeOnly(dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond, dateTime.Microsecond),
                    TimeSpan.Zero));
            }

            TypeConverter? inputConverter = GetCachedSourceTypeConverter(inputType, target);
            if (inputConverter != null)
            {
                return Attempt.Succeed(inputConverter.ConvertTo(input, target));
            }

            TypeConverter? outputConverter = GetCachedTargetTypeConverter(inputType, target);
            if (outputConverter != null)
            {
                return Attempt.Succeed(outputConverter.ConvertFrom(input!));
            }

            if (target.IsGenericType && GetCachedGenericNullableType(target) != null)
            {
                // cannot Convert.ChangeType as that does not work with nullable
                // input has already been converted to the underlying type - just
                // return input, there's an implicit conversion from T to T? anyways
                return Attempt.Succeed(input);
            }

            // Re-check convertibles since we altered the input through recursion
            if (input is IConvertible convertible2)
            {
                return Attempt.Succeed(Convert.ChangeType(convertible2, target));
            }
        }
        catch (Exception e)
        {
            return Attempt<object?>.Fail(e);
        }

        return Attempt<object?>.Fail();
    }

    /// <summary>
    /// Attempts to convert the input string to the output type.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="target">The type to convert to</param>
    /// <returns>
    /// The <see cref="T:Attempt{object?}" />
    /// </returns>
    /// <remarks>
    /// This code is an optimized version of the original Umbraco method.
    /// </remarks>
    private static Attempt<object?>? TryConvertToFromString(this string input, Type target)
    {
        // Easy
        if (target == typeof(string))
        {
            return Attempt<object?>.Succeed(input);
        }

        // Null, empty, whitespaces
        if (string.IsNullOrWhiteSpace(input))
        {
            if (target == typeof(bool))
            {
                // null/empty = bool false
                return Attempt<object?>.Succeed(false);
            }

            if (target == typeof(DateTime))
            {
                // null/empty = min DateTime value
                return Attempt<object?>.Succeed(DateTime.MinValue);
            }

            // Cannot decide here,
            // Any of the types below will fail parsing and will return a failed attempt
            // but anything else will not be processed and will return null
            // so even though the string is null/empty we have to proceed.
        }

        // Look for type conversions in the expected order of frequency of use.
        //
        // By using a mixture of ordered if statements and switches we can optimize both for
        // fast conditional checking for most frequently used types and the branching
        // that does not depend on previous values available to switch statements.
        if (target.IsPrimitive)
        {
            if (target == typeof(int))
            {
                if (int.TryParse(input, out var value))
                {
                    return Attempt<object?>.Succeed(value);
                }

                // Because decimal 100.01m will happily convert to integer 100, it
                // makes sense that string "100.01" *also* converts to integer 100.
                var input2 = NormalizeNumberDecimalSeparator(input);
                return Attempt<object?>.If(decimal.TryParse(input2, out var value2), Convert.ToInt32(value2));
            }

            if (target == typeof(long))
            {
                if (long.TryParse(input, out var value))
                {
                    return Attempt<object?>.Succeed(value);
                }

                // Same as int
                var input2 = NormalizeNumberDecimalSeparator(input);
                return Attempt<object?>.If(decimal.TryParse(input2, out var value2), Convert.ToInt64(value2));
            }

            // TODO: Should we do the decimal trick for short, byte, unsigned?
            if (target == typeof(bool))
            {
                if (bool.TryParse(input, out var value))
                {
                    return Attempt<object?>.Succeed(value);
                }

                // Don't declare failure so the CustomBooleanTypeConverter can try
                return null;
            }

            // Calling this method directly is faster than any attempt to cache it.
            switch (Type.GetTypeCode(target))
            {
                case TypeCode.Int16:
                    return Attempt<object?>.If(short.TryParse(input, out var value), value);

                case TypeCode.Double:
                    var input2 = NormalizeNumberDecimalSeparator(input);
                    return Attempt<object?>.If(double.TryParse(input2, out var valueD), valueD);

                case TypeCode.Single:
                    var input3 = NormalizeNumberDecimalSeparator(input);
                    return Attempt<object?>.If(float.TryParse(input3, out var valueF), valueF);

                case TypeCode.Char:
                    return Attempt<object?>.If(char.TryParse(input, out var valueC), valueC);

                case TypeCode.Byte:
                    return Attempt<object?>.If(byte.TryParse(input, out var valueB), valueB);

                case TypeCode.SByte:
                    return Attempt<object?>.If(sbyte.TryParse(input, out var valueSb), valueSb);

                case TypeCode.UInt32:
                    return Attempt<object?>.If(uint.TryParse(input, out var valueU), valueU);

                case TypeCode.UInt16:
                    return Attempt<object?>.If(ushort.TryParse(input, out var valueUs), valueUs);

                case TypeCode.UInt64:
                    return Attempt<object?>.If(ulong.TryParse(input, out var valueUl), valueUl);
            }
        }
        else if (target == typeof(Guid))
        {
            return Attempt<object?>.If(Guid.TryParse(input, out Guid value), value);
        }
        else if (target == typeof(DateTime))
        {
            if (DateTime.TryParse(input, out DateTime value))
            {
                switch (value.Kind)
                {
                    case DateTimeKind.Unspecified:
                    case DateTimeKind.Utc:
                        return Attempt<object?>.Succeed(value);

                    case DateTimeKind.Local:
                        return Attempt<object?>.Succeed(value.ToUniversalTime());

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return Attempt<object?>.Fail();
        }
        else if (target == typeof(DateTimeOffset))
        {
            return Attempt<object?>.If(DateTimeOffset.TryParse(input, out DateTimeOffset value), value);
        }
        else if (target == typeof(TimeSpan))
        {
            return Attempt<object?>.If(TimeSpan.TryParse(input, out TimeSpan value), value);
        }
        else if (target == typeof(decimal))
        {
            var input2 = NormalizeNumberDecimalSeparator(input);
            return Attempt<object?>.If(decimal.TryParse(input2, out var value), value);
        }
        else if (input != null && target == typeof(Version))
        {
            return Attempt<object?>.If(Version.TryParse(input, out Version? value), value);
        }

        // E_NOTIMPL IPAddress, BigInteger
        return null; // we can't decide...
    }

    private static string? ToDebugString(this object? obj, int levels = 0)
    {
        if (obj == null)
        {
            return "{null}";
        }

        try
        {
            if (obj is string)
            {
                return "\"{0}\"".InvariantFormat(obj);
            }

            if (obj is int || obj is short || obj is long || obj is float || obj is double || obj is bool ||
                obj is int? || obj is short? || obj is long? || obj is float? || obj is double? || obj is bool?)
            {
                return "{0}".InvariantFormat(obj);
            }

            if (obj is Enum)
            {
                return "[{0}]".InvariantFormat(obj);
            }

            if (obj is IEnumerable enumerable)
            {
                var items = (from object enumItem in enumerable
                             let value = GetEnumPropertyDebugString(enumItem, levels)
                             where value != null
                             select value).Take(10).ToList();

                return items.Any()
                    ? "{{ {0} }}".InvariantFormat(string.Join(", ", items))
                    : null;
            }

            PropertyInfo[] props = obj.GetType().GetProperties();
            if (props.Length == 2 && props[0].Name == "Key" && props[1].Name == "Value" && levels > -2)
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
                    (from propertyInfo in props
                     let value = GetPropertyDebugString(propertyInfo, obj, levels)
                     where value != null
                     select "{0}={1}".InvariantFormat(propertyInfo.Name, value)).ToArray();

                return items.Any()
                    ? "[{0}]:{{ {1} }}".InvariantFormat(obj.GetType().Name, string.Join(", ", items))
                    : null;
            }
        }
        catch (Exception ex)
        {
            return "[Exception:{0}]".InvariantFormat(ex.Message);
        }

        return null;
    }

    private static string? GetEnumPropertyDebugString(object enumItem, int levels)
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

    private static string? GetPropertyDebugString(PropertyInfo propertyInfo, object obj, int levels)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string NormalizeNumberDecimalSeparator(string s)
    {
        var normalized = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
        return s.ReplaceMany(_numberDecimalSeparatorsToNormalize, normalized);
    }

    // gets a converter for source, that can convert to target, or null if none exists
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TypeConverter? GetCachedSourceTypeConverter(Type source, Type target)
    {
        var key = new CompositeTypeTypeKey(source, target);

        if (_inputTypeConverterCache.TryGetValue(key, out TypeConverter? typeConverter))
        {
            return typeConverter;
        }

        TypeConverter converter = TypeDescriptor.GetConverter(source);
        if (converter.CanConvertTo(target))
        {
            return _inputTypeConverterCache[key] = converter;
        }

        _inputTypeConverterCache[key] = null;
        return null;
    }

    // gets a converter for target, that can convert from source, or null if none exists
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TypeConverter? GetCachedTargetTypeConverter(Type source, Type target)
    {
        var key = new CompositeTypeTypeKey(source, target);

        if (_destinationTypeConverterCache.TryGetValue(key, out TypeConverter? typeConverter))
        {
            return typeConverter;
        }

        TypeConverter converter = TypeDescriptor.GetConverter(target);
        if (converter.CanConvertFrom(source))
        {
            return _destinationTypeConverterCache[key] = converter;
        }

        _destinationTypeConverterCache[key] = null;
        return null;
    }

    // gets the underlying type of a nullable type, or null if the type is not nullable
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Type? GetCachedGenericNullableType(Type type)
    {
        if (_nullableGenericCache.TryGetValue(type, out Type? underlyingType))
        {
            return underlyingType;
        }

        if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            Type? underlying = Nullable.GetUnderlyingType(type);
            return _nullableGenericCache[type] = underlying;
        }

        _nullableGenericCache[type] = null;
        return null;
    }

    // gets an IConvertible from source to target type, or null if none exists
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetCachedCanAssign(object input, Type source, Type target)
    {
        var key = new CompositeTypeTypeKey(source, target);
        if (_assignableTypeCache.TryGetValue(key, out var canConvert))
        {
            return canConvert;
        }

        // "object is" is faster than "Type.IsAssignableFrom.
        // We can use it to very quickly determine whether true/false
        if (input is IConvertible && target.IsAssignableFrom(source))
        {
            return _assignableTypeCache[key] = true;
        }

        return _assignableTypeCache[key] = false;
    }

    // determines whether a type can be converted to boolean
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetCachedCanConvertToBoolean(Type type)
    {
        if (_boolConvertCache.TryGetValue(type, out var result))
        {
            return result;
        }

        if (_customBooleanTypeConverter.CanConvertFrom(type))
        {
            return _boolConvertCache[type] = true;
        }

        return _boolConvertCache[type] = false;
    }
}
