using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core
{
    public static class PublishedContentExtensions
    {
        private static IVariationContextAccessor VariationContextAccessor => Current.VariationContextAccessor;

        /// <summary>
        /// Determines whether the content has a culture.
        /// </summary>
        /// <remarks>Culture is case-insensitive.</remarks>
        public static bool HasCulture(this IPublishedContent content, string culture)
            => content.Cultures.ContainsKey(culture ?? string.Empty);

        /// <summary>
        /// Determines whether the content is invariant, or has a culture.
        /// </summary>
        /// <remarks>Culture is case-insensitive.</remarks>
        public static bool IsInvariantOrHasCulture(this IPublishedContent content, string culture)
            => !content.ContentType.VariesByCulture() || content.Cultures.ContainsKey(culture ?? "");

        /// <summary>
        /// Filters a sequence of <see cref="IPublishedContent"/> to return invariant items, and items that are published for the specified culture.
        /// </summary>
        /// <param name="contents">The content items.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null).</param>
        internal static IEnumerable<T> WhereIsInvariantOrHasCulture<T>(this IEnumerable<T> contents, string culture = null)
            where T : class, IPublishedContent
        {
            if (contents == null) throw new ArgumentNullException(nameof(contents));

            culture = culture ?? Current.VariationContextAccessor.VariationContext?.Culture ?? "";

            // either does not vary by culture, or has the specified culture
            return contents.Where(x => !ContentVariationExtensions.VariesByCulture((IPublishedContentType) x.ContentType) || HasCulture(x, culture));
        }

        /// <summary>
        /// Gets the name of the content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="culture">The specific culture to get the name for. If null is used the current culture is used (Default is null).</param>
        public static string Name(this IPublishedContent content, string culture = null)
        {
            // invariant has invariant value (whatever the requested culture)
            if (!content.ContentType.VariesByCulture())
                return "NAME??"; // fixme where should the invariant one come from? should Cultures contain it?

            // handle context culture for variant
            if (culture == null)
                culture = VariationContextAccessor?.VariationContext?.Culture ?? "";

            // get
            return culture != "" && content.Cultures.TryGetValue(culture, out var infos) ? infos.Name : null;
        }


        /// <summary>
        /// Gets the url segment of the content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="culture">The specific culture to get the url segment for. If null is used the current culture is used (Default is null).</param>
        public static string UrlSegment(this IPublishedContent content, string culture = null)
        {
            // invariant has invariant value (whatever the requested culture)
            if (!content.ContentType.VariesByCulture())
                return "URLSEGMENT??"; // fixme where should the invariant one come from? should Cultures contain it?

            // handle context culture for variant
            if (culture == null)
                culture = VariationContextAccessor?.VariationContext?.Culture ?? "";

            // get
            return culture != "" && content.Cultures.TryGetValue(culture, out var infos) ? infos.UrlSegment : null;
        }

        /// <summary>
        /// Gets the culture date of the content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="culture">The specific culture to get the name for. If null is used the current culture is used (Default is null).</param>
        public static DateTime CultureDate(this IPublishedContent content, string culture = null)
        {
            // invariant has invariant value (whatever the requested culture)
            if (!content.ContentType.VariesByCulture())
                return content.UpdateDate;

            // handle context culture for variant
            if (culture == null)
                culture = VariationContextAccessor?.VariationContext?.Culture ?? "";

            // get
            return culture != "" && content.Cultures.TryGetValue(culture, out var infos) ? infos.Date : DateTime.MinValue;
        }


        /// <summary>
        /// Gets the children of the content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="culture">The specific culture to get the url children for. If null is used the current culture is used (Default is null).</param>
        /// <remarks>
        /// <para>Gets children that are available for the specified culture.</para>
        /// <para>Children are sorted by their sortOrder.</para>
        /// <para>The '*' culture and supported and returns everything.</para>
        /// </remarks>
        public static IEnumerable<IPublishedContent> Children(this IPublishedContent content, string culture = null)
        {
            // invariant has invariant value (whatever the requested culture)
            if (!content.ContentType.VariesByCulture() && culture != "*")
                culture = "";

            // handle context culture for variant
            if (culture == null)
                culture = VariationContextAccessor?.VariationContext?.Culture ?? "";

            var children = content.ChildrenForAllCultures;
            return culture == "*"
                ? children
                : children.Where(x => x.IsInvariantOrHasCulture(culture));
        }
    }
}