using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web
{
    /// <summary>
    /// Provides extension methods for <c>IPublishedItem</c>.
    /// </summary>
    public static class PublishedFragmentExtensions
    {
        #region IsComposedOf

        /// <summary>
        /// Gets a value indicating whether the content is of a content type composed of the given alias
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="alias">The content type alias.</param>
        /// <returns>A value indicating whether the content is of a content type composed of a content type identified by the alias.</returns>
        public static bool IsComposedOf(this IPublishedFragment content, string alias)
        {
            return content.ContentType.CompositionAliases.Contains(alias);
        }

        #endregion

        #region HasProperty

        /// <summary>
        /// Gets a value indicating whether the content has a property identified by its alias.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <returns>A value indicating whether the content has the property identified by the alias.</returns>
        /// <remarks>The content may have a property, and that property may not have a value.</remarks>
        public static bool HasProperty(this IPublishedFragment content, string alias)
        {
            return content.ContentType.GetPropertyType(alias) != null;
        }

        #endregion

        #region HasValue

        /// <summary>
        /// Gets a value indicating whether the content has a value for a property identified by its alias.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <returns>A value indicating whether the content has a value for the property identified by the alias.</returns>
        /// <remarks>Returns true if <c>GetProperty(alias)</c> is not <c>null</c> and <c>GetProperty(alias).HasValue</c> is <c>true</c>.</remarks>
        public static bool HasValue(this IPublishedFragment content, string alias)
        {
            var prop = content.GetProperty(alias);
            return prop != null && prop.HasValue;
        }

        /// <summary>
        /// Returns one of two strings depending on whether the content has a value for a property identified by its alias.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="valueIfTrue">The value to return if the content has a value for the property.</param>
        /// <param name="valueIfFalse">The value to return if the content has no value for the property.</param>
        /// <returns>Either <paramref name="valueIfTrue"/> or <paramref name="valueIfFalse"/> depending on whether the content
        /// has a value for the property identified by the alias.</returns>
        public static IHtmlString HasValue(this IPublishedFragment content, string alias,
            string valueIfTrue, string valueIfFalse = null)
        {
            return content.HasValue(alias)
                ? new HtmlString(valueIfTrue)
                : new HtmlString(valueIfFalse ?? string.Empty);
        }

        #endregion

        #region Value

        /// <summary>
        /// Gets the value of a content's property identified by its alias.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <returns>The value of the content's property identified by the alias.</returns>
        /// <remarks>
        /// <para>The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering content.</para>
        /// <para>If no property with the specified alias exists, or if the property has no value, returns <c>null</c>.</para>
        /// <para>If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the converter.</para>
        /// <para>The alias is case-insensitive.</para>
        /// </remarks>
        public static object Value(this IPublishedFragment content, string alias)
        {
            var property = content.GetProperty(alias);
            return property?.Value;
        }

        /// <summary>
        /// Gets the value of a content's property identified by its alias, if it exists, otherwise a default value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the content's property identified by the alias, if it exists, otherwise a default value.</returns>
        /// <remarks>
        /// <para>The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering content.</para>
        /// <para>If no property with the specified alias exists, or if the property has no value, returns <paramref name="defaultValue"/>.</para>
        /// <para>If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the converter.</para>
        /// <para>The alias is case-insensitive.</para>
        /// </remarks>
        public static object Value(this IPublishedFragment content, string alias, string defaultValue)
        {
            var property = content.GetProperty(alias);
            return property == null || property.HasValue == false ? defaultValue : property.Value;
        }

        /// <summary>
        /// Gets the value of a content's property identified by its alias, if it exists, otherwise a default value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the content's property identified by the alias, if it exists, otherwise a default value.</returns>
        /// <remarks>
        /// <para>The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering content.</para>
        /// <para>If no property with the specified alias exists, or if the property has no value, returns <paramref name="defaultValue"/>.</para>
        /// <para>If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the converter.</para>
        /// <para>The alias is case-insensitive.</para>
        /// </remarks>
        public static object Value(this IPublishedFragment content, string alias, object defaultValue)
        {
            var property = content.GetProperty(alias);
            return property == null || property.HasValue == false ? defaultValue : property.Value;
        }

        #endregion

        #region Value<T>

        /// <summary>
        /// Gets the value of a content's property identified by its alias, converted to a specified type.
        /// </summary>
        /// <typeparam name="T">The target property type.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <returns>The value of the content's property identified by the alias, converted to the specified type.</returns>
        /// <remarks>
        /// <para>The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering content.</para>
        /// <para>If no property with the specified alias exists, or if the property has no value, or if it could not be converted, returns <c>default(T)</c>.</para>
        /// <para>If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the converter.</para>
        /// <para>The alias is case-insensitive.</para>
        /// </remarks>
        public static T Value<T>(this IPublishedFragment content, string alias)
        {
            return content.Value(alias, false, default(T));
        }

        /// <summary>
        /// Gets the value of a content's property identified by its alias, converted to a specified type, if it exists, otherwise a default value.
        /// </summary>
        /// <typeparam name="T">The target property type.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the content's property identified by the alias, converted to the specified type, if it exists, otherwise a default value.</returns>
        /// <remarks>
        /// <para>The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering content.</para>
        /// <para>If no property with the specified alias exists, or if the property has no value, or if it could not be converted, returns <paramref name="defaultValue"/>.</para>
        /// <para>If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the converter.</para>
        /// <para>The alias is case-insensitive.</para>
        /// </remarks>
        public static T Value<T>(this IPublishedFragment content, string alias, T defaultValue)
        {
            return content.Value(alias, true, defaultValue);
        }

        internal static T Value<T>(this IPublishedFragment content, string alias, bool withDefaultValue, T defaultValue)
        {
            var property = content.GetProperty(alias);
            if (property == null) return defaultValue;

            return property.Value(withDefaultValue, defaultValue);
        }

        #endregion

        #region Value

        // trying to reproduce Umbraco.Field so we can get rid of it
        //
        // what we want:
        // - alt aliases
        // - recursion
        // - default value
        // - before & after (if value)
        //
        // convertLineBreaks: should be an extension string.ConvertLineBreaks()
        // stripParagraphs: should be an extension string.StripParagraphs()
        // format: should use the standard .ToString(format)
        //
        // see UmbracoComponentRenderer.Field - which is ugly ;-(

        // recurse first, on each alias (that's how it's done in Field)
        // TODO: strongly typed properties howto?
        // there is no strongly typed recurse, etc => needs to be in ModelsBuilder?

        public static IHtmlString Value<T>(this IPublishedFragment content, string aliases, Func<T, string> format, string alt = "")
        {
            if (format == null) format = x => x.ToString();

            var property = aliases.Split(',')
                .Where(x => string.IsNullOrWhiteSpace(x) == false)
                .Select(x => content.GetProperty(x.Trim()))
                .FirstOrDefault(x => x != null);

            return property != null
                ? new HtmlString(format(property.Value<T>()))
                : new HtmlString(alt);
        }

        // fixme - move that one!
        public static IHtmlString Value<T>(this IPublishedContent content, string aliases, Func<T, string> format, string alt = "", bool recurse = false)
        {
            if (format == null) format = x => x.ToString();

            var property = aliases.Split(',')
                .Where(x => string.IsNullOrWhiteSpace(x) == false)
                .Select(x => content.GetProperty(x.Trim(), recurse))
                .FirstOrDefault(x => x != null);

            return property != null
                ? new HtmlString(format(property.Value<T>()))
                : new HtmlString(alt);
        }

        #endregion

        #region ToIndexedArray

        public static IndexedArrayItem<TContent>[] ToIndexedArray<TContent>(this IEnumerable<TContent> source)
            where TContent : class, IPublishedFragment
        {
            var set = source.Select((content, index) => new IndexedArrayItem<TContent>(content, index)).ToArray();
            foreach (var setItem in set) setItem.TotalCount = set.Length;
            return set;
        }

        #endregion

        #region OfTypes

        // the .OfType<T>() filter is nice when there's only one type
        // this is to support filtering with multiple types
        public static IEnumerable<T> OfTypes<T>(this IEnumerable<T> contents, params string[] types)
            where T : IPublishedFragment
        {
            types = types.Select(x => x.ToLowerInvariant()).ToArray();
            return contents.Where(x => types.Contains(x.ContentType.Alias.ToLowerInvariant()));
        }

        #endregion
    }
}