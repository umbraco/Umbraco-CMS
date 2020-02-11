using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;

namespace Umbraco.Core
{
    public static class PublishedContentExtensions
    {
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
        /// <param name="variationContextAccessor"></param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null).</param>
        internal static IEnumerable<T> WhereIsInvariantOrHasCulture<T>(this IEnumerable<T> contents, IVariationContextAccessor variationContextAccessor, string culture = null)
            where T : class, IPublishedContent
        {
            if (contents == null) throw new ArgumentNullException(nameof(contents));

            culture = culture ?? variationContextAccessor.VariationContext?.Culture ?? "";

            // either does not vary by culture, or has the specified culture
            return contents.Where(x => !x.ContentType.VariesByCulture() || HasCulture(x, culture));
        }

        /// <summary>
        /// Gets the name of the content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="variationContextAccessor"></param>
        /// <param name="culture">The specific culture to get the name for. If null is used the current culture is used (Default is null).</param>
        public static string Name(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string culture = null)
        {
            // invariant has invariant value (whatever the requested culture)
            if (!content.ContentType.VariesByCulture())
                return content.Cultures.TryGetValue("", out var invariantInfos) ? invariantInfos.Name : null;

            // handle context culture for variant
            if (culture == null)
                culture = variationContextAccessor?.VariationContext?.Culture ?? "";

            // get
            return culture != "" && content.Cultures.TryGetValue(culture, out var infos) ? infos.Name : null;
        }


        /// <summary>
        /// Gets the url segment of the content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="variationContextAccessor"></param>
        /// <param name="culture">The specific culture to get the url segment for. If null is used the current culture is used (Default is null).</param>
        public static string UrlSegment(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string culture = null)
        {
            // invariant has invariant value (whatever the requested culture)
            if (!content.ContentType.VariesByCulture())
                return content.Cultures.TryGetValue("", out var invariantInfos) ? invariantInfos.UrlSegment : null;

            // handle context culture for variant
            if (culture == null)
                culture = variationContextAccessor?.VariationContext?.Culture ?? "";

            // get
            return culture != "" && content.Cultures.TryGetValue(culture, out var infos) ? infos.UrlSegment : null;
        }

        /// <summary>
        /// Gets the culture date of the content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="variationContextAccessor"></param>
        /// <param name="culture">The specific culture to get the name for. If null is used the current culture is used (Default is null).</param>
        public static DateTime CultureDate(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string culture = null)
        {
            // invariant has invariant value (whatever the requested culture)
            if (!content.ContentType.VariesByCulture())
                return content.UpdateDate;

            // handle context culture for variant
            if (culture == null)
                culture = variationContextAccessor?.VariationContext?.Culture ?? "";

            // get
            return culture != "" && content.Cultures.TryGetValue(culture, out var infos) ? infos.Date : DateTime.MinValue;
        }

        /// <summary>
        /// Gets the children of the content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="variationContextAccessor"></param>
        /// <param name="culture">
        /// The specific culture to get the url children for. Default is null which will use the current culture in <see cref="VariationContext"/>
        /// </param>
        /// <remarks>
        /// <para>Gets children that are available for the specified culture.</para>
        /// <para>Children are sorted by their sortOrder.</para>
        /// <para>
        /// For culture,
        /// if null is used the current culture is used.
        /// If an empty string is used only invariant children are returned.
        /// If "*" is used all children are returned.
        /// </para>
        /// <para>
        /// If a variant culture is specified or there is a current culture in the <see cref="VariationContext"/> then the Children returned
        /// will include both the variant children matching the culture AND the invariant children because the invariant children flow with the current culture.
        /// However, if an empty string is specified only invariant children are returned.
        /// </para>
        /// </remarks>
        public static IEnumerable<IPublishedContent> Children(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string culture = null)
        {
            // handle context culture for variant
            if (culture == null)
                culture = variationContextAccessor?.VariationContext?.Culture ?? "";

            var children = content.ChildrenForAllCultures;
            return culture == "*"
                ? children
                : children.Where(x => x.IsInvariantOrHasCulture(culture));
        }

        #region Writer and creator

        public static string GetCreatorName(this IPublishedContent content, IUserService userService)
        {
            var user = userService.GetProfileById(content.CreatorId);
            return user?.Name;
        }

        public static string GetWriterName(this IPublishedContent content, IUserService userService)
        {
            var user = userService.GetProfileById(content.WriterId);
            return user?.Name;
        }

        #endregion

        #region Axes: ancestors, ancestors-or-self

        // as per XPath 1.0 specs �2.2,
        // - the ancestor axis contains the ancestors of the context node; the ancestors of the context node consist
        //   of the parent of context node and the parent's parent and so on; thus, the ancestor axis will always
        //   include the root node, unless the context node is the root node.
        // - the ancestor-or-self axis contains the context node and the ancestors of the context node; thus,
        //   the ancestor axis will always include the root node.
        //
        // as per XPath 2.0 specs �3.2.1.1,
        // - the ancestor axis is defined as the transitive closure of the parent axis; it contains the ancestors
        //   of the context node (the parent, the parent of the parent, and so on) - The ancestor axis includes the
        //   root node of the tree in which the context node is found, unless the context node is the root node.
        // - the ancestor-or-self axis contains the context node and the ancestors of the context node; thus,
        //   the ancestor-or-self axis will always include the root node.
        //
        // the ancestor and ancestor-or-self axis are reverse axes ie they contain the context node or nodes that
        // are before the context node in document order.
        //
        // document order is defined by �2.4.1 as:
        // - the root node is the first node.
        // - every node occurs before all of its children and descendants.
        // - the relative order of siblings is the order in which they occur in the children property of their parent node.
        // - children and descendants occur before following siblings.

        /// <summary>
        /// Gets the ancestors of the content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The ancestors of the content, in down-top order.</returns>
        /// <remarks>Does not consider the content itself.</remarks>
        public static IEnumerable<IPublishedContent> Ancestors(this IPublishedContent content)
        {
            return content.AncestorsOrSelf(false, null);
        }

        /// <summary>
        /// Gets the ancestors of the content, at a level lesser or equal to a specified level.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="maxLevel">The level.</param>
        /// <returns>The ancestors of the content, at a level lesser or equal to the specified level, in down-top order.</returns>
        /// <remarks>Does not consider the content itself. Only content that are "high enough" in the tree are returned.</remarks>
        public static IEnumerable<IPublishedContent> Ancestors(this IPublishedContent content, int maxLevel)
        {
            return content.AncestorsOrSelf(false, n => n.Level <= maxLevel);
        }

        /// <summary>
        /// Gets the ancestors of the content, of a specified content type.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="contentTypeAlias">The content type.</param>
        /// <returns>The ancestors of the content, of the specified content type, in down-top order.</returns>
        /// <remarks>Does not consider the content itself. Returns all ancestors, of the specified content type.</remarks>
        public static IEnumerable<IPublishedContent> Ancestors(this IPublishedContent content, string contentTypeAlias)
        {
            return content.AncestorsOrSelf(false, n => n.ContentType.Alias.InvariantEquals(contentTypeAlias));
        }

        /// <summary>
        /// Gets the ancestors of the content, of a specified content type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="content">The content.</param>
        /// <returns>The ancestors of the content, of the specified content type, in down-top order.</returns>
        /// <remarks>Does not consider the content itself. Returns all ancestors, of the specified content type.</remarks>
        public static IEnumerable<T> Ancestors<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            return content.Ancestors().OfType<T>();
        }

        /// <summary>
        /// Gets the ancestors of the content, at a level lesser or equal to a specified level, and of a specified content type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="maxLevel">The level.</param>
        /// <returns>The ancestors of the content, at a level lesser or equal to the specified level, and of the specified
        /// content type, in down-top order.</returns>
        /// <remarks>Does not consider the content itself. Only content that are "high enough" in the trees, and of the
        /// specified content type, are returned.</remarks>
        public static IEnumerable<T> Ancestors<T>(this IPublishedContent content, int maxLevel)
            where T : class, IPublishedContent
        {
            return content.Ancestors(maxLevel).OfType<T>();
        }

        /// <summary>
        /// Gets the content and its ancestors.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The content and its ancestors, in down-top order.</returns>
        public static IEnumerable<IPublishedContent> AncestorsOrSelf(this IPublishedContent content)
        {
            return content.AncestorsOrSelf(true, null);
        }

        /// <summary>
        /// Gets the content and its ancestors, at a level lesser or equal to a specified level.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="maxLevel">The level.</param>
        /// <returns>The content and its ancestors, at a level lesser or equal to the specified level,
        /// in down-top order.</returns>
        /// <remarks>Only content that are "high enough" in the tree are returned. So it may or may not begin
        /// with the content itself, depending on its level.</remarks>
        public static IEnumerable<IPublishedContent> AncestorsOrSelf(this IPublishedContent content, int maxLevel)
        {
            return content.AncestorsOrSelf(true, n => n.Level <= maxLevel);
        }

        /// <summary>
        /// Gets the content and its ancestors, of a specified content type.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="contentTypeAlias">The content type.</param>
        /// <returns>The content and its ancestors, of the specified content type, in down-top order.</returns>
        /// <remarks>May or may not begin with the content itself, depending on its content type.</remarks>
        public static IEnumerable<IPublishedContent> AncestorsOrSelf(this IPublishedContent content, string contentTypeAlias)
        {
            return content.AncestorsOrSelf(true, n => n.ContentType.Alias.InvariantEquals(contentTypeAlias));
        }

        /// <summary>
        /// Gets the content and its ancestors, of a specified content type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="content">The content.</param>
        /// <returns>The content and its ancestors, of the specified content type, in down-top order.</returns>
        /// <remarks>May or may not begin with the content itself, depending on its content type.</remarks>
        public static IEnumerable<T> AncestorsOrSelf<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            return content.AncestorsOrSelf().OfType<T>();
        }

        /// <summary>
        /// Gets the content and its ancestor, at a lever lesser or equal to a specified level, and of a specified content type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="maxLevel">The level.</param>
        /// <returns>The content and its ancestors, at a level lesser or equal to the specified level, and of the specified
        /// content type, in down-top order.</returns>
        /// <remarks>May or may not begin with the content itself, depending on its level and content type.</remarks>
        public static IEnumerable<T> AncestorsOrSelf<T>(this IPublishedContent content, int maxLevel)
            where T : class, IPublishedContent
        {
            return content.AncestorsOrSelf(maxLevel).OfType<T>();
        }

        /// <summary>
        /// Gets the ancestor of the content, ie its parent.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The ancestor of the content.</returns>
        /// <remarks>This method is here for consistency purposes but does not make much sense.</remarks>
        public static IPublishedContent Ancestor(this IPublishedContent content)
        {
            return content.Parent;
        }

        /// <summary>
        /// Gets the nearest ancestor of the content, at a lever lesser or equal to a specified level.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="maxLevel">The level.</param>
        /// <returns>The nearest (in down-top order) ancestor of the content, at a level lesser or equal to the specified level.</returns>
        /// <remarks>Does not consider the content itself. May return <c>null</c>.</remarks>
        public static IPublishedContent Ancestor(this IPublishedContent content, int maxLevel)
        {
            return content.EnumerateAncestors(false).FirstOrDefault(x => x.Level <= maxLevel);
        }

        /// <summary>
        /// Gets the nearest ancestor of the content, of a specified content type.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="contentTypeAlias">The content type alias.</param>
        /// <returns>The nearest (in down-top order) ancestor of the content, of the specified content type.</returns>
        /// <remarks>Does not consider the content itself. May return <c>null</c>.</remarks>
        public static IPublishedContent Ancestor(this IPublishedContent content, string contentTypeAlias)
        {
            return content.EnumerateAncestors(false).FirstOrDefault(x => x.ContentType.Alias.InvariantEquals(contentTypeAlias));
        }

        /// <summary>
        /// Gets the nearest ancestor of the content, of a specified content type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="content">The content.</param>
        /// <returns>The nearest (in down-top order) ancestor of the content, of the specified content type.</returns>
        /// <remarks>Does not consider the content itself. May return <c>null</c>.</remarks>
        public static T Ancestor<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            return content.Ancestors<T>().FirstOrDefault();
        }

        /// <summary>
        /// Gets the nearest ancestor of the content, at the specified level and of the specified content type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="maxLevel">The level.</param>
        /// <returns>The ancestor of the content, at the specified level and of the specified content type.</returns>
        /// <remarks>Does not consider the content itself. If the ancestor at the specified level is
        /// not of the specified type, returns <c>null</c>.</remarks>
        public static T Ancestor<T>(this IPublishedContent content, int maxLevel)
            where T : class, IPublishedContent
        {
            return content.Ancestors<T>(maxLevel).FirstOrDefault();
        }

        /// <summary>
        /// Gets the content or its nearest ancestor.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The content.</returns>
        /// <remarks>This method is here for consistency purposes but does not make much sense.</remarks>
        public static IPublishedContent AncestorOrSelf(this IPublishedContent content)
        {
            return content;
        }

        /// <summary>
        /// Gets the content or its nearest ancestor, at a lever lesser or equal to a specified level.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="maxLevel">The level.</param>
        /// <returns>The content or its nearest (in down-top order) ancestor, at a level lesser or equal to the specified level.</returns>
        /// <remarks>May or may not return the content itself depending on its level. May return <c>null</c>.</remarks>
        public static IPublishedContent AncestorOrSelf(this IPublishedContent content, int maxLevel)
        {
            return content.EnumerateAncestors(true).FirstOrDefault(x => x.Level <= maxLevel);
        }

        /// <summary>
        /// Gets the content or its nearest ancestor, of a specified content type.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="contentTypeAlias">The content type.</param>
        /// <returns>The content or its nearest (in down-top order) ancestor, of the specified content type.</returns>
        /// <remarks>May or may not return the content itself depending on its content type. May return <c>null</c>.</remarks>
        public static IPublishedContent AncestorOrSelf(this IPublishedContent content, string contentTypeAlias)
        {
            return content.EnumerateAncestors(true).FirstOrDefault(x => x.ContentType.Alias.InvariantEquals(contentTypeAlias));
        }

        /// <summary>
        /// Gets the content or its nearest ancestor, of a specified content type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="content">The content.</param>
        /// <returns>The content or its nearest (in down-top order) ancestor, of the specified content type.</returns>
        /// <remarks>May or may not return the content itself depending on its content type. May return <c>null</c>.</remarks>
        public static T AncestorOrSelf<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            return content.AncestorsOrSelf<T>().FirstOrDefault();
        }

        /// <summary>
        /// Gets the content or its nearest ancestor, at a lever lesser or equal to a specified level, and of a specified content type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="maxLevel">The level.</param>
        /// <returns></returns>
        public static T AncestorOrSelf<T>(this IPublishedContent content, int maxLevel)
            where T : class, IPublishedContent
        {
            return content.AncestorsOrSelf<T>(maxLevel).FirstOrDefault();
        }

        public static IEnumerable<IPublishedContent> AncestorsOrSelf(this IPublishedContent content, bool orSelf, Func<IPublishedContent, bool> func)
        {
            var ancestorsOrSelf = content.EnumerateAncestors(orSelf);
            return func == null ? ancestorsOrSelf : ancestorsOrSelf.Where(func);
        }

        /// <summary>
        /// Enumerates ancestors of the content, bottom-up.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="orSelf">Indicates whether the content should be included.</param>
        /// <returns>Enumerates bottom-up ie walking up the tree (parent, grand-parent, etc).</returns>
        internal static IEnumerable<IPublishedContent> EnumerateAncestors(this IPublishedContent content, bool orSelf)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (orSelf) yield return content;
            while ((content = content.Parent) != null)
                yield return content;
        }

        #endregion

        #region Axes: descendants, descendants-or-self

        /// <summary>
        /// Returns all DescendantsOrSelf of all content referenced
        /// </summary>
        /// <param name="parentNodes"></param>
        /// <param name="variationContextAccessor">Variation context accessor.</param>
        /// <param name="docTypeAlias"></param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns></returns>
        /// <remarks>
        /// This can be useful in order to return all nodes in an entire site by a type when combined with TypedContentAtRoot
        /// </remarks>
        public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(this IEnumerable<IPublishedContent> parentNodes, IVariationContextAccessor variationContextAccessor, string docTypeAlias, string culture = null)
        {
            return parentNodes.SelectMany(x => x.DescendantsOrSelfOfType(variationContextAccessor, docTypeAlias, culture));
        }

        /// <summary>
        /// Returns all DescendantsOrSelf of all content referenced
        /// </summary>
        /// <param name="parentNodes"></param>
        /// <param name="variationContextAccessor">Variation context accessor.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns></returns>
        /// <remarks>
        /// This can be useful in order to return all nodes in an entire site by a type when combined with TypedContentAtRoot
        /// </remarks>
        public static IEnumerable<T> DescendantsOrSelf<T>(this IEnumerable<IPublishedContent> parentNodes, IVariationContextAccessor variationContextAccessor, string culture = null)
            where T : class, IPublishedContent
        {
            return parentNodes.SelectMany(x => x.DescendantsOrSelf<T>(variationContextAccessor, culture));
        }


        // as per XPath 1.0 specs �2.2,
        // - the descendant axis contains the descendants of the context node; a descendant is a child or a child of a child and so on; thus
        //   the descendant axis never contains attribute or namespace nodes.
        // - the descendant-or-self axis contains the context node and the descendants of the context node.
        //
        // as per XPath 2.0 specs �3.2.1.1,
        // - the descendant axis is defined as the transitive closure of the child axis; it contains the descendants of the context node (the
        //   children, the children of the children, and so on).
        // - the descendant-or-self axis contains the context node and the descendants of the context node.
        //
        // the descendant and descendant-or-self axis are forward axes ie they contain the context node or nodes that are after the context
        // node in document order.
        //
        // document order is defined by �2.4.1 as:
        // - the root node is the first node.
        // - every node occurs before all of its children and descendants.
        // - the relative order of siblings is the order in which they occur in the children property of their parent node.
        // - children and descendants occur before following siblings.

        public static IEnumerable<IPublishedContent> Descendants(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string culture = null)
        {
            return content.DescendantsOrSelf(variationContextAccessor, false, null, culture);
        }

        public static IEnumerable<IPublishedContent> Descendants(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, int level, string culture = null)
        {
            return content.DescendantsOrSelf(variationContextAccessor, false, p => p.Level >= level, culture);
        }

        public static IEnumerable<IPublishedContent> DescendantsOfType(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string contentTypeAlias, string culture = null)
        {
            return content.DescendantsOrSelf(variationContextAccessor, false, p => p.ContentType.Alias.InvariantEquals(contentTypeAlias), culture);
        }

        public static IEnumerable<T> Descendants<T>(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string culture = null)
            where T : class, IPublishedContent
        {
            return content.Descendants(variationContextAccessor, culture).OfType<T>();
        }

        public static IEnumerable<T> Descendants<T>(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, int level, string culture = null)
            where T : class, IPublishedContent
        {
            return content.Descendants(variationContextAccessor, level, culture).OfType<T>();
        }

        public static IEnumerable<IPublishedContent> DescendantsOrSelf(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string culture = null)
        {
            return content.DescendantsOrSelf(variationContextAccessor, true, null, culture);
        }

        public static IEnumerable<IPublishedContent> DescendantsOrSelf(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, int level, string culture = null)
        {
            return content.DescendantsOrSelf(variationContextAccessor, true, p => p.Level >= level, culture);
        }

        public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string contentTypeAlias, string culture = null)
        {
            return content.DescendantsOrSelf(variationContextAccessor, true, p => p.ContentType.Alias.InvariantEquals(contentTypeAlias), culture);
        }

        public static IEnumerable<T> DescendantsOrSelf<T>(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string culture = null)
            where T : class, IPublishedContent
        {
            return content.DescendantsOrSelf(variationContextAccessor, culture).OfType<T>();
        }

        public static IEnumerable<T> DescendantsOrSelf<T>(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, int level, string culture = null)
            where T : class, IPublishedContent
        {
            return content.DescendantsOrSelf(variationContextAccessor, level, culture).OfType<T>();
        }

        public static IPublishedContent Descendant(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string culture = null)
        {
            return content.Children(variationContextAccessor, culture).FirstOrDefault();
        }

        public static IPublishedContent Descendant(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, int level, string culture = null)
        {
            return content.EnumerateDescendants(variationContextAccessor, false, culture).FirstOrDefault(x => x.Level == level);
        }

        public static IPublishedContent DescendantOfType(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string contentTypeAlias, string culture = null)
        {
            return content.EnumerateDescendants(variationContextAccessor, false, culture).FirstOrDefault(x => x.ContentType.Alias.InvariantEquals(contentTypeAlias));
        }

        public static T Descendant<T>(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string culture = null)
            where T : class, IPublishedContent
        {
            return content.EnumerateDescendants(variationContextAccessor, false, culture).FirstOrDefault(x => x is T) as T;
        }

        public static T Descendant<T>(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, int level, string culture = null)
            where T : class, IPublishedContent
        {
            return content.Descendant(variationContextAccessor, level, culture) as T;
        }

        public static IPublishedContent DescendantOrSelf(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string culture = null)
        {
            return content;
        }

        public static IPublishedContent DescendantOrSelf(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, int level, string culture = null)
        {
            return content.EnumerateDescendants(variationContextAccessor, true, culture).FirstOrDefault(x => x.Level == level);
        }

        public static IPublishedContent DescendantOrSelfOfType(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string contentTypeAlias, string culture = null)
        {
            return content.EnumerateDescendants(variationContextAccessor, true, culture).FirstOrDefault(x => x.ContentType.Alias.InvariantEquals(contentTypeAlias));
        }

        public static T DescendantOrSelf<T>(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string culture = null)
            where T : class, IPublishedContent
        {
            return content.EnumerateDescendants(variationContextAccessor, true, culture).FirstOrDefault(x => x is T) as T;
        }

        public static T DescendantOrSelf<T>(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, int level, string culture = null)
            where T : class, IPublishedContent
        {
            return content.DescendantOrSelf(variationContextAccessor, level, culture) as T;
        }

        internal static IEnumerable<IPublishedContent> DescendantsOrSelf(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, bool orSelf, Func<IPublishedContent, bool> func, string culture = null)
        {
            return content.EnumerateDescendants(variationContextAccessor, orSelf, culture).Where(x => func == null || func(x));
        }

        internal static IEnumerable<IPublishedContent> EnumerateDescendants(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, bool orSelf, string culture = null)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (orSelf) yield return content;

            foreach (var desc in content.Children(variationContextAccessor, culture).SelectMany(x => x.EnumerateDescendants(variationContextAccessor, culture)))
                yield return desc;
        }

        internal static IEnumerable<IPublishedContent> EnumerateDescendants(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string culture = null)
        {
            yield return content;

            foreach (var desc in content.Children(variationContextAccessor, culture).SelectMany(x => x.EnumerateDescendants(variationContextAccessor, culture)))
                yield return desc;
        }

        #endregion

        #region Axes: children

        /// <summary>
        /// Gets the children of the content, filtered by a predicate.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="publishedSnapshot">Published snapshot instance</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns>The children of the content, filtered by the predicate.</returns>
        /// <remarks>
        /// <para>Children are sorted by their sortOrder.</para>
        /// </remarks>
        public static IEnumerable<IPublishedContent> Children(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, Func<IPublishedContent, bool> predicate, string culture = null)
        {
            return content.Children(variationContextAccessor, culture).Where(predicate);
        }

        /// <summary>
        /// Gets the children of the content, of any of the specified types.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="publishedSnapshot">Published snapshot instance</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <param name="contentTypeAlias">The content type alias.</param>
        /// <returns>The children of the content, of any of the specified types.</returns>
        public static IEnumerable<IPublishedContent> ChildrenOfType(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string contentTypeAlias, string culture = null)
        {
            return content.Children(variationContextAccessor, x => x.ContentType.Alias.InvariantEquals(contentTypeAlias), culture);
        }

        /// <summary>
        /// Gets the children of the content, of a given content type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="publishedSnapshot">Published snapshot instance</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns>The children of content, of the given content type.</returns>
        /// <remarks>
        /// <para>Children are sorted by their sortOrder.</para>
        /// </remarks>
        public static IEnumerable<T> Children<T>(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string culture = null)
            where T : class, IPublishedContent
        {
            return content.Children(variationContextAccessor, culture).OfType<T>();
        }

        public static IPublishedContent FirstChild(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string culture = null)
        {
            return content.Children(variationContextAccessor, culture).FirstOrDefault();
        }

        /// <summary>
        /// Gets the first child of the content, of a given content type.
        /// </summary>
        public static IPublishedContent FirstChildOfType(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string contentTypeAlias, string culture = null)
        {
            return content.ChildrenOfType(variationContextAccessor, contentTypeAlias, culture).FirstOrDefault();
        }

        public static IPublishedContent FirstChild(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, Func<IPublishedContent, bool> predicate, string culture = null)
        {
            return content.Children(variationContextAccessor, predicate, culture).FirstOrDefault();
        }

        public static IPublishedContent FirstChild(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, Guid uniqueId, string culture = null)
        {
            return content.Children(variationContextAccessor, x => x.Key == uniqueId, culture).FirstOrDefault();
        }

        public static T FirstChild<T>(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, string culture = null)
            where T : class, IPublishedContent
        {
            return content.Children<T>(variationContextAccessor, culture).FirstOrDefault();
        }

        public static T FirstChild<T>(this IPublishedContent content, IVariationContextAccessor variationContextAccessor, Func<T, bool> predicate, string culture = null)
            where T : class, IPublishedContent
        {
            return content.Children<T>(variationContextAccessor, culture).FirstOrDefault(predicate);
        }

        #endregion

        #region Axes: parent

        // Parent is native

        /// <summary>
        /// Gets the parent of the content, of a given content type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="content">The content.</param>
        /// <returns>The parent of content, of the given content type, else null.</returns>
        public static T Parent<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            return content.Parent as T;
        }

        #endregion

        #region Axes: Siblings

        /// <summary>
        /// Gets the siblings of the content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="publishedSnapshot">Published snapshot instance</param>
        /// <param name="variationContextAccessor">Variation context accessor.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns>The siblings of the content.</returns>
        /// <remarks>
        ///   <para>Note that in V7 this method also return the content node self.</para>
        /// </remarks>
        public static IEnumerable<IPublishedContent> Siblings(this IPublishedContent content, IPublishedSnapshot publishedSnapshot, IVariationContextAccessor variationContextAccessor, string culture = null)
        {
            return SiblingsAndSelf(content, publishedSnapshot, variationContextAccessor, culture).Where(x => x.Id != content.Id);
        }

        /// <summary>
        /// Gets the siblings of the content, of a given content type.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="publishedSnapshot">Published snapshot instance</param>
        /// <param name="variationContextAccessor">Variation context accessor.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <param name="contentTypeAlias">The content type alias.</param>
        /// <returns>The siblings of the content, of the given content type.</returns>
        /// <remarks>
        ///   <para>Note that in V7 this method also return the content node self.</para>
        /// </remarks>
        public static IEnumerable<IPublishedContent> SiblingsOfType(this IPublishedContent content, IPublishedSnapshot publishedSnapshot, IVariationContextAccessor variationContextAccessor, string contentTypeAlias, string culture = null)
        {
            return SiblingsAndSelfOfType(content, publishedSnapshot, variationContextAccessor, contentTypeAlias, culture).Where(x => x.Id != content.Id);
        }

        /// <summary>
        /// Gets the siblings of the content, of a given content type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="publishedSnapshot">Published snapshot instance</param>
        /// <param name="variationContextAccessor">Variation context accessor.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns>The siblings of the content, of the given content type.</returns>
        /// <remarks>
        ///   <para>Note that in V7 this method also return the content node self.</para>
        /// </remarks>
        public static IEnumerable<T> Siblings<T>(this IPublishedContent content, IPublishedSnapshot publishedSnapshot, IVariationContextAccessor variationContextAccessor, string culture = null)
            where T : class, IPublishedContent
        {
            return SiblingsAndSelf<T>(content, publishedSnapshot, variationContextAccessor, culture).Where(x => x.Id != content.Id);
        }

        /// <summary>
        /// Gets the siblings of the content including the node itself to indicate the position.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="publishedSnapshot">Published snapshot instance</param>
        /// <param name="variationContextAccessor">Variation context accessor.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns>The siblings of the content including the node itself.</returns>
        public static IEnumerable<IPublishedContent> SiblingsAndSelf(this IPublishedContent content, IPublishedSnapshot publishedSnapshot, IVariationContextAccessor variationContextAccessor, string culture = null)
        {
            return content.Parent != null
                ? content.Parent.Children(variationContextAccessor, culture)
                : publishedSnapshot.Content.GetAtRoot().WhereIsInvariantOrHasCulture(variationContextAccessor, culture);
        }

        /// <summary>
        /// Gets the siblings of the content including the node itself to indicate the position, of a given content type.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="publishedSnapshot">Published snapshot instance</param>
        /// <param name="variationContextAccessor">Variation context accessor.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <param name="contentTypeAlias">The content type alias.</param>
        /// <returns>The siblings of the content including the node itself, of the given content type.</returns>
        public static IEnumerable<IPublishedContent> SiblingsAndSelfOfType(this IPublishedContent content, IPublishedSnapshot publishedSnapshot, IVariationContextAccessor variationContextAccessor, string contentTypeAlias, string culture = null)
        {
            return content.Parent != null
                ? content.Parent.ChildrenOfType(variationContextAccessor, contentTypeAlias, culture)
                : publishedSnapshot.Content.GetAtRoot().OfTypes(contentTypeAlias).WhereIsInvariantOrHasCulture(variationContextAccessor, culture);
        }

        /// <summary>
        /// Gets the siblings of the content including the node itself to indicate the position, of a given content type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="publishedSnapshot">Published snapshot instance</param>
        /// <param name="variationContextAccessor">Variation context accessor.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns>The siblings of the content including the node itself, of the given content type.</returns>
        public static IEnumerable<T> SiblingsAndSelf<T>(this IPublishedContent content, IPublishedSnapshot publishedSnapshot, IVariationContextAccessor variationContextAccessor, string culture = null)
            where T : class, IPublishedContent
        {
            return content.Parent != null
                ? content.Parent.Children<T>(variationContextAccessor, culture)
                : publishedSnapshot.Content.GetAtRoot().OfType<T>().WhereIsInvariantOrHasCulture(variationContextAccessor, culture);
        }

        #endregion

        #region Axes: custom

        /// <summary>
        /// Gets the root content for this content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The 'site' content ie AncestorOrSelf(1).</returns>
        public static IPublishedContent Root(this IPublishedContent content)
        {
            return content.AncestorOrSelf(1);
        }

        #endregion


        /// <summary>
        /// Gets the url of the content item.
        /// </summary>
        /// <remarks>
        /// <para>If the content item is a document, then this method returns the url of the
        /// document. If it is a media, then this methods return the media url for the
        /// 'umbracoFile' property. Use the MediaUrl() method to get the media url for other
        /// properties.</para>
        /// <para>The value of this property is contextual. It depends on the 'current' request uri,
        /// if any. In addition, when the content type is multi-lingual, this is the url for the
        /// specified culture. Otherwise, it is the invariant url.</para>
        /// </remarks>
        public static string Url(this IPublishedContent content, IPublishedUrlProvider publishedUrlProvider, string culture = null, UrlMode mode = UrlMode.Default)
        {
            if (publishedUrlProvider == null)
                throw new InvalidOperationException("Cannot resolve a Url when Current.UmbracoContext.UrlProvider is null.");

            switch (content.ContentType.ItemType)
            {
                case PublishedItemType.Content:
                    return publishedUrlProvider.GetUrl(content, mode, culture);

                case PublishedItemType.Media:
                    return publishedUrlProvider.GetMediaUrl(content, mode, culture, Constants.Conventions.Media.File);

                default:
                    throw new NotSupportedException();
            }
        }

         #region Template

        /// <summary>
        /// Returns the current template Alias
        /// </summary>
        /// <param name="content"></param>
        /// <returns>Empty string if none is set.</returns>
        public static string GetTemplateAlias(this IPublishedContent content, IFileService fileService)
        {
            if(content.TemplateId.HasValue == false)
            {
                return string.Empty;
            }

            var template = fileService.GetTemplate(content.TemplateId.Value);
            return template == null ? string.Empty : template.Alias;
        }
        public static bool IsAllowedTemplate(this IPublishedContent content, int templateId, IUmbracoSettingsSection umbracoSettingsSection, IContentTypeService contentTypeService)
        {
            if (umbracoSettingsSection.WebRouting.DisableAlternativeTemplates)
                return content.TemplateId == templateId;

            if (content.TemplateId == templateId || !umbracoSettingsSection.WebRouting.ValidateAlternativeTemplates)
                return true;

            var publishedContentContentType = contentTypeService.Get(content.ContentType.Id);
            if (publishedContentContentType == null)
                throw new NullReferenceException("No content type returned for published content (contentType='" + content.ContentType.Id + "')");

            return publishedContentContentType.IsAllowedTemplate(templateId);
        }
        public static bool IsAllowedTemplate(this IPublishedContent content, string templateAlias,  IUmbracoSettingsSection umbracoSettingsSection, IContentTypeService contentTypeService, IFileService fileService)
        {
            var template = fileService.GetTemplate(templateAlias);
            return template != null && content.IsAllowedTemplate(template.Id, umbracoSettingsSection, contentTypeService);
        }

        #endregion
    }
}
