using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Umbraco.Core.Configuration.UmbracoSettings
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
            if (!result.Success)
            {
                //just try a straight up conversion
                try
                {
                    var converted = (T)input;
                    return new Attempt<T>(true, converted);
                }
                catch (Exception e)
                {
                    return new Attempt<T>(e);
                }
            }
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

            if (input is string && destinationType.IsEnum)
            {
                try
                {
                    var output = Enum.Parse(destinationType, (string)input, true);
                    return new Attempt<object>(true, output);
                }
                catch (Exception e)
                {                    
                    return new Attempt<object>(e);
                }
            }

            if (!destinationType.IsGenericType || destinationType.GetGenericTypeDefinition() != typeof(Nullable<>))
            {
                //TODO: Do a check for destination type being IEnumerable<T> and source type implementing IEnumerable<T> with
                // the same 'T', then we'd have to find the extension method for the type AsEnumerable() and execute it.

                if (TypeHelper.IsTypeAssignableFrom(destinationType, input.GetType())
                    && TypeHelper.IsTypeAssignableFrom<IConvertible>(input))
                {
                    try
                    {
                        var casted = Convert.ChangeType(input, destinationType);
                        return new Attempt<object>(true, casted);
                    }
                    catch (Exception e)
                    {
                        return new Attempt<object>(e);
                    }
                }
            }

            var inputConverter = TypeDescriptor.GetConverter(input);
            if (inputConverter.CanConvertTo(destinationType))
            {
                try
                {
                    var converted = inputConverter.ConvertTo(input, destinationType);
                    return new Attempt<object>(true, converted);
                }
                catch (Exception e)
                {
                    return new Attempt<object>(e);
                }
            }

            if (destinationType == typeof(bool))
            {
                var boolConverter = new CustomBooleanTypeConverter();
                if (boolConverter.CanConvertFrom(input.GetType()))
                {
                    try
                    {
                        var converted = boolConverter.ConvertFrom(input);
                        return new Attempt<object>(true, converted);
                    }
                    catch (Exception e)
                    {
                        return new Attempt<object>(e);
                    }
                }
            }

            var outputConverter = TypeDescriptor.GetConverter(destinationType);
            if (outputConverter.CanConvertFrom(input.GetType()))
            {
                try
                {
                    var converted = outputConverter.ConvertFrom(input);
                    return new Attempt<object>(true, converted);
                }
                catch (Exception e)
                {
                    return new Attempt<object>(e);
                }
            }


            if (TypeHelper.IsTypeAssignableFrom<IConvertible>(input))
            {
                try
                {
                    var casted = Convert.ChangeType(input, destinationType);
                    return new Attempt<object>(true, casted);
                }
                catch (Exception e)
                {
                    return new Attempt<object>(e);
                }
            }

            return Attempt<object>.False;
        }

        public static void CheckThrowObjectDisposed(this IDisposable disposable, bool isDisposed, string objectname)
        {
            //TODO: Localise this exception
            if (isDisposed)
                throw new ObjectDisposedException(objectname);
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

       

        

    }
}