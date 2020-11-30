using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Web.Composing;
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
        // Update to this comment 8/2/2020: issue as been ameliorated by creating extensions methods in Umbraco.Core
        // that accept the dependencies as arguments for many of these extension methods, and can be used within the Umbraco code-base.
        // For site developers, the "friendly" extension methods using service location have been maintained, delegating to the ones that
        // take the dependencies as parameters.

        private static IPublishedValueFallback PublishedValueFallback => Current.PublishedValueFallback;

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
            return content.Value(PublishedValueFallback, alias, culture, segment, fallback, defaultValue);
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
            return content.Value<T>(PublishedValueFallback, alias, culture, segment, fallback, defaultValue);
        }

        #endregion

        #region IsSomething

        /// <summary>
        /// Gets a value indicating whether the content is visible.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>A value indicating whether the content is visible.</returns>
        /// <remarks>A content is not visible if it has an umbracoNaviHide property with a value of "1". Otherwise,
        /// the content is visible.</remarks>
        public static bool IsVisible(this IPublishedElement content)
        {
            return content.IsVisible(PublishedValueFallback);
        }

        #endregion

        #region MediaUrl

        /// <summary>
        /// Gets the URL for a media.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="culture">The culture (use current culture by default).</param>
        /// <param name="mode">The URL mode (use site configuration by default).</param>
        /// <param name="propertyAlias">The alias of the property (use 'umbracoFile' by default).</param>
        /// <returns>The URL for the media.</returns>
        /// <remarks>
        /// <para>The value of this property is contextual. It depends on the 'current' request uri,
        /// if any. In addition, when the content type is multi-lingual, this is the URL for the
        /// specified culture. Otherwise, it is the invariant URL.</para>
        /// </remarks>
        public static string MediaUrl(this IPublishedContent content, string culture = null, UrlMode mode = UrlMode.Default, string propertyAlias = Constants.Conventions.Media.File)
        {
            var publishedUrlProvider = Current.PublishedUrlProvider;

            if (publishedUrlProvider== null)
                throw new InvalidOperationException("Cannot resolve a Url when Current.PublishedUrlProvider is null.");

            return publishedUrlProvider.GetMediaUrl(content, mode, culture, propertyAlias);
        }

        #endregion
    }
}
