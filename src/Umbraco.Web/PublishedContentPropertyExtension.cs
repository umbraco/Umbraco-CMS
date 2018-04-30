using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web
{
    /// <summary>
    /// Provides extension methods for <c>IPublishedProperty</c>.
    /// </summary>
    public static class PublishedPropertyExtension
    {
        #region Value<T>

        public static T Value<T>(this IPublishedProperty property, string culture = null, string segment = null, T defaultValue = default)
        {
            // for Value<T> when defaultValue is not specified, and HasValue() is false, we still want to convert the result (see below)
            // but we have no way to tell whether default value is specified or not - we could do it with overloads, but then defaultValue
            // comes right after property and conflicts with culture when T is string - so we're just not doing it - if defaultValue is
            // default, whether specified or not, we give a chance to the converter
            //
            //if (!property.HasValue(culture, segment) && 'defaultValue is explicitely specified') return defaultValue;

            // give the converter a chance to handle the default value differently
            // eg for IEnumerable<T> it may return Enumerable<T>.Empty instead of null

            var value = property.GetValue(culture, segment);

            // if value is null (strange but why not) it still is OK to call TryConvertTo
            // because it's an extension method (hence no NullRef) which will return a
            // failed attempt. So, no need to care for value being null here.

            // if already the requested type, return
            if (value is T variable) return variable;

            // if can convert to requested type, return
            var convert = value.TryConvertTo<T>();
            if (convert.Success) return convert.Result;

            // at that point, the code tried with the raw value
            // that makes no sense because it sort of is unpredictable,
            // you wouldn't know when the converters run or don't run.
            // so, it's commented out now.

            // try with the raw value
            //var source = property.ValueSource;
            //if (source is string) source = TextValueConverterHelper.ParseStringValueSource((string)source);
            //if (source is T) return (T)source;
            //convert = source.TryConvertTo<T>();
            //if (convert.Success) return convert.Result;

            return defaultValue;
        }

        #endregion
    }
}
