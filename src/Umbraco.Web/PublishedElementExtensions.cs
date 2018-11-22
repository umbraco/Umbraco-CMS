using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web
{
    /// <summary>
    /// Provides extension methods for <c>IPublishedElement</c>.
    /// </summary>
    public static class PublishedElementExtensions
    {
        // lots of debates about accessing dependencies (IPublishedValueFallback) from extension methods, ranging
        // from "don't do it" i.e. if an extension method is relying on dependencies, a proper service should be
        // created instead, to discussing method injection vs service locator vs other subtleties, see for example
        // this post http://marisks.net/2016/12/19/dependency-injection-with-extension-methods/
        //
        // point is, we do NOT want a service, we DO want to write model.Value<int>("alias", "fr-FR") and hit
        // fallback somehow - which pretty much rules out method injection, and basically anything but service
        // locator - bah, let's face it, it works
        //
        // besides, for tests, Current support setting a fallback without even a container
        //
        private static IPublishedValueFallback PublishedValueFallback => Current.PublishedValueFallback;

        #region IsComposedOf

        /// <summary>
        /// Gets a value indicating whether the content is of a content type composed of the given alias
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="alias">The content type alias.</param>
        /// <returns>A value indicating whether the content is of a content type composed of a content type identified by the alias.</returns>
        public static bool IsComposedOf(this IPublishedElement content, string alias)
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
        public static bool HasProperty(this IPublishedElement content, string alias)
        {
            return content.ContentType.GetPropertyType(alias) != null;
        }

        #endregion

        #region HasValue

        /// <summary>
        /// Gets a value indicating whether the content has a value for a property identified by its alias.
        /// </summary>
        /// <remarks>Returns true if <c>GetProperty(alias)</c> is not <c>null</c> and <c>GetProperty(alias).HasValue</c> is <c>true</c>.</remarks>
        public static bool HasValue(this IPublishedElement content, string alias, string culture = null, string segment = null)
        {
            var prop = content.GetProperty(alias);
            return prop != null && prop.HasValue(culture, segment);
        }

        // fixme - .Value() refactoring - in progress
        // missing variations...

        /// <summary>
        /// Returns one of two strings depending on whether the content has a value for a property identified by its alias.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="valueIfTrue">The value to return if the content has a value for the property.</param>
        /// <param name="valueIfFalse">The value to return if the content has no value for the property.</param>
        /// <returns>Either <paramref name="valueIfTrue"/> or <paramref name="valueIfFalse"/> depending on whether the content
        /// has a value for the property identified by the alias.</returns>
        public static IHtmlString IfValue(this IPublishedElement content, string alias, string valueIfTrue, string valueIfFalse = null)
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
        /// <param name="culture">The variation language.</param>
        /// <param name="segment">The variation segment.</param>
        /// <param name="fallback">Optional fallback strategy.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the content's property identified by the alias, if it exists, otherwise a default value.</returns>
        /// <remarks>
        /// <para>The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering content.</para>
        /// <para>If no property with the specified alias exists, or if the property has no value, returns <paramref name="defaultValue"/>.</para>
        /// <para>If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the converter.</para>
        /// <para>The alias is case-insensitive.</para>
        /// </remarks>
        public static object Value(this IPublishedElement content, string alias, string culture = null, string segment = null, Fallback fallback = default, object defaultValue = default)
        {
            var property = content.GetProperty(alias);

            // if we have a property, and it has a value, return that value
            if (property != null && property.HasValue(culture, segment))
                return property.GetValue(culture, segment);

            // else let fallback try to get a value
            if (PublishedValueFallback.TryGetValue(content, alias, culture, segment, fallback, defaultValue, out var value))
                return value;

            // else... if we have a property, at least let the converter return its own
            // vision of 'no value' (could be an empty enumerable) - otherwise, default
            return property?.GetValue(culture, segment);
        }

        #endregion

        #region Value<T>

        /// <summary>
        /// Gets the value of a content's property identified by its alias, converted to a specified type.
        /// </summary>
        /// <typeparam name="T">The target property type.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="culture">The variation language.</param>
        /// <param name="segment">The variation segment.</param>
        /// <param name="fallback">Optional fallback strategy.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the content's property identified by the alias, converted to the specified type.</returns>
        /// <remarks>
        /// <para>The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering content.</para>
        /// <para>If no property with the specified alias exists, or if the property has no value, or if it could not be converted, returns <c>default(T)</c>.</para>
        /// <para>If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the converter.</para>
        /// <para>The alias is case-insensitive.</para>
        /// </remarks>
        public static T Value<T>(this IPublishedElement content, string alias, string culture = null, string segment = null, Fallback fallback = default, T defaultValue = default)
        {
            var property = content.GetProperty(alias);

            // if we have a property, and it has a value, return that value
            if (property != null && property.HasValue(culture, segment))
                return property.Value<T>(culture, segment);

            // else let fallback try to get a value
            if (PublishedValueFallback.TryGetValue(content, alias, culture, segment, fallback, defaultValue, out var value))
                return value;

            // else... if we have a property, at least let the converter return its own
            // vision of 'no value' (could be an empty enumerable) - otherwise, default
            return property == null ? default : property.Value<T>(culture, segment);
        }

        #endregion

        #region Value or Umbraco.Field - WORK IN PROGRESS

        // fixme - .Value() refactoring - in progress
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
        //
        // there is no strongly typed recurse, etc => needs to be in ModelsBuilder?

        // that one can only happen in ModelsBuilder as that's where the attributes are defined
        // the attribute that carries the alias is in ModelsBuilder!
        //public static TValue Value<TModel, TValue>(this TModel content, Expression<Func<TModel, TValue>> propertySelector, ...)
        //    where TModel : IPublishedElement
        //{
        //    PropertyInfo pi = GetPropertyFromExpression(propertySelector);
        //    var attr = pi.GetCustomAttribute<ImplementPropertyAttribute>();
        //    var alias = attr.Alias;
        //    return content.Value<TValue>(alias, ...)
        //}

        // recurse should be implemented via fallback

        // todo - that one should be refactored, missing culture and so many things
        public static IHtmlString Value<T>(this IPublishedElement content, string aliases, Func<T, string> format, string alt = "")
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

        #endregion

        #region ToIndexedArray

        public static IndexedArrayItem<TContent>[] ToIndexedArray<TContent>(this IEnumerable<TContent> source)
            where TContent : class, IPublishedElement
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
            where T : IPublishedElement
        {
            types = types.Select(x => x.ToLowerInvariant()).ToArray();
            return contents.Where(x => types.Contains(x.ContentType.Alias.ToLowerInvariant()));
        }

        #endregion
    }
}
