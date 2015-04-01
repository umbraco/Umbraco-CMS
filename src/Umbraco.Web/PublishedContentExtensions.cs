using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using Examine.LuceneEngine.SearchCriteria;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Core;
using Umbraco.Web.Routing;
using ContentType = umbraco.cms.businesslogic.ContentType;

namespace Umbraco.Web
{
    /// <summary>
    /// Provides extension methods for <c>IPublishedContent</c>.
    /// </summary>
	public static class PublishedContentExtensions
	{    
        #region Urls

        /// <summary>
		/// Gets the url for the content.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <returns>The url for the content.</returns>
		[Obsolete("NiceUrl() is obsolete, use the Url() method instead")]
		public static string NiceUrl(this IPublishedContent content)
		{
			return content.Url();
		}

		/// <summary>
		/// Gets the url for the content.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <returns>The url for the content.</returns>
		/// <remarks>Better use the <c>Url</c> property but that method is here to complement <c>UrlAbsolute()</c>.</remarks>
		public static string Url(this IPublishedContent content)
		{
		    return content.Url;
		}

		/// <summary>
		/// Gets the absolute url for the content.
		/// </summary>
        /// <param name="content">The content.</param>
		/// <returns>The absolute url for the content.</returns>
		[Obsolete("NiceUrlWithDomain() is obsolete, use the UrlAbsolute() method instead.")]
		public static string NiceUrlWithDomain(this IPublishedContent content)
		{
            return content.UrlAbsolute();
		}

		/// <summary>
		/// Gets the absolute url for the content.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <returns>The absolute url for the content.</returns>
        //[Obsolete("UrlWithDomain() is obsolete, use the UrlAbsolute() method instead.")]
        public static string UrlWithDomain(this IPublishedContent content)
		{
		    return content.UrlAbsolute();
		}

        /// <summary>
        /// Gets the absolute url for the content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The absolute url for the content.</returns>
        public static string UrlAbsolute(this IPublishedContent content)
        {
            // adapted from PublishedContentBase.Url
            switch (content.ItemType)
            {
                case PublishedItemType.Content:
                    if (UmbracoContext.Current == null)
                        throw new InvalidOperationException("Cannot resolve a Url for a content item when UmbracoContext.Current is null.");
                    if (UmbracoContext.Current.UrlProvider == null)
                        throw new InvalidOperationException("Cannot resolve a Url for a content item when UmbracoContext.Current.UrlProvider is null.");
                    return UmbracoContext.Current.UrlProvider.GetUrl(content.Id, true);
                case PublishedItemType.Media:
                    throw new NotSupportedException("AbsoluteUrl is not supported for media types.");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Template

        /// <summary>
		/// Returns the current template Alias
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		public static string GetTemplateAlias(this IPublishedContent content)
        {
            var template = ApplicationContext.Current.Services.FileService.GetTemplate(content.TemplateId);
			return template == null ? string.Empty : template.Alias;
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
        public static bool HasProperty(this IPublishedContent content, string alias)
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
        public static bool HasValue(this IPublishedContent content, string alias)
        {
            return content.HasValue(alias, false);
        }

        /// <summary>
        /// Gets a value indicating whether the content has a value for a property identified by its alias.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="recurse">A value indicating whether to navigate the tree upwards until a property with a value is found.</param>
        /// <returns>A value indicating whether the content has a value for the property identified by the alias.</returns>
        /// <remarks>Returns true if <c>GetProperty(alias, recurse)</c> is not <c>null</c> and <c>GetProperty(alias, recurse).HasValue</c> is <c>true</c>.</remarks>
        public static bool HasValue(this IPublishedContent content, string alias, bool recurse)
        {
            var prop = content.GetProperty(alias, recurse);
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
        public static IHtmlString HasValue(this IPublishedContent content, string alias,
            string valueIfTrue, string valueIfFalse = null)
        {
            return content.HasValue(alias, false)
                ? new HtmlString(valueIfTrue)
                : new HtmlString(valueIfFalse ?? string.Empty);
        }

        /// <summary>
        /// Returns one of two strings depending on whether the content has a value for a property identified by its alias.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="recurse">A value indicating whether to navigate the tree upwards until a property with a value is found.</param>
        /// <param name="valueIfTrue">The value to return if the content has a value for the property.</param>
        /// <param name="valueIfFalse">The value to return if the content has no value for the property.</param>
        /// <returns>Either <paramref name="valueIfTrue"/> or <paramref name="valueIfFalse"/> depending on whether the content
        /// has a value for the property identified by the alias.</returns>
        public static IHtmlString HasValue(this IPublishedContent content, string alias, bool recurse,
            string valueIfTrue, string valueIfFalse = null)
        {
            return content.HasValue(alias, recurse)
                ? new HtmlString(valueIfTrue)
                : new HtmlString(valueIfFalse ?? string.Empty);
        }

        #endregion

        #region GetPropertyValue

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
        public static object GetPropertyValue(this IPublishedContent content, string alias)
        {
            var property = content.GetProperty(alias);
            return property == null ? null : property.Value;
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
        public static object GetPropertyValue(this IPublishedContent content, string alias, string defaultValue)
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
        public static object GetPropertyValue(this IPublishedContent content, string alias, object defaultValue)
        {
            var property = content.GetProperty(alias);
            return property == null || property.HasValue == false ? defaultValue : property.Value;
        }

        /// <summary>
        /// Recursively gets the value of a content's property identified by its alias.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="recurse">A value indicating whether to recurse.</param>
        /// <returns>The recursive value of the content's property identified by the alias.</returns>
        /// <remarks>
        /// <para>Recursively means: walking up the tree from <paramref name="content"/>, get the first value that can be found.</para>
        /// <para>The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering content.</para>
        /// <para>If no property with the specified alias exists, or if the property has no value, returns <c>null</c>.</para>
        /// <para>If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the converter.</para>
        /// <para>The alias is case-insensitive.</para>
        /// </remarks>
        public static object GetPropertyValue(this IPublishedContent content, string alias, bool recurse)
        {
            var property = content.GetProperty(alias, recurse);
            return property == null ? null : property.Value;
        }

        /// <summary>
        /// Recursively the value of a content's property identified by its alias, if it exists, otherwise a default value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="recurse">A value indicating whether to recurse.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the content's property identified by the alias, if it exists, otherwise a default value.</returns>
        /// <remarks>
        /// <para>Recursively means: walking up the tree from <paramref name="content"/>, get the first value that can be found.</para>
        /// <para>The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering content.</para>
        /// <para>If no property with the specified alias exists, or if the property has no value, returns <paramref name="defaultValue"/>.</para>
        /// <para>If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the converter.</para>
        /// <para>The alias is case-insensitive.</para>
        /// </remarks>
        public static object GetPropertyValue(this IPublishedContent content, string alias, bool recurse, object defaultValue)
        {
            var property = content.GetProperty(alias, recurse);
            return property == null || property.HasValue == false ? defaultValue : property.Value;
        }

        #endregion

        #region GetPropertyValue<T>

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
        public static T GetPropertyValue<T>(this IPublishedContent content, string alias)
		{
			return content.GetPropertyValue(alias, false, false, default(T));
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
        public static T GetPropertyValue<T>(this IPublishedContent content, string alias, T defaultValue)
        {
            return content.GetPropertyValue(alias, false, true, defaultValue);
        }

        /// <summary>
        /// Recursively gets the value of a content's property identified by its alias, converted to a specified type.
        /// </summary>
        /// <typeparam name="T">The target property type.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="recurse">A value indicating whether to recurse.</param>
        /// <returns>The value of the content's property identified by the alias, converted to the specified type.</returns>
        /// <remarks>
        /// <para>Recursively means: walking up the tree from <paramref name="content"/>, get the first value that can be found.</para>
        /// <para>The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering content.</para>
        /// <para>If no property with the specified alias exists, or if the property has no value, or if it could not be converted, returns <c>default(T)</c>.</para>
        /// <para>If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the converter.</para>
        /// <para>The alias is case-insensitive.</para>
        /// </remarks>
        public static T GetPropertyValue<T>(this IPublishedContent content, string alias, bool recurse)
        {
            return content.GetPropertyValue(alias, recurse, false, default(T));
        }

        /// <summary>
        /// Recursively gets the value of a content's property identified by its alias, converted to a specified type, if it exists, otherwise a default value.
        /// </summary>
        /// <typeparam name="T">The target property type.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="recurse">A value indicating whether to recurse.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the content's property identified by the alias, converted to the specified type, if it exists, otherwise a default value.</returns>
        /// <remarks>
        /// <para>Recursively means: walking up the tree from <paramref name="content"/>, get the first value that can be found.</para>
        /// <para>The value comes from <c>IPublishedProperty</c> field <c>Value</c> ie it is suitable for use when rendering content.</para>
        /// <para>If no property with the specified alias exists, or if the property has no value, or if it could not be converted, returns <paramref name="defaultValue"/>.</para>
        /// <para>If eg a numeric property wants to default to 0 when value source is empty, this has to be done in the converter.</para>
        /// <para>The alias is case-insensitive.</para>
        /// </remarks>
        public static T GetPropertyValue<T>(this IPublishedContent content, string alias, bool recurse, T defaultValue)
        {
            return content.GetPropertyValue(alias, recurse, true, defaultValue);
        }

        internal static T GetPropertyValue<T>(this IPublishedContent content, string alias, bool recurse, bool withDefaultValue, T defaultValue)
        {
            var property = content.GetProperty(alias, recurse);
            if (property == null) return defaultValue;

            return property.GetValue(withDefaultValue, defaultValue);
		}

		#endregion

        // copied over from Core.PublishedContentExtensions - should be obsoleted
        [Obsolete("GetRecursiveValue() is obsolete, use GetPropertyValue().")]
        public static string GetRecursiveValue(this IPublishedContent content, string alias)
        {
            var value = content.GetPropertyValue(alias, true);
            return value == null ? string.Empty : value.ToString();
        }

		#region Search

        public static IEnumerable<IPublishedContent> Search(this IPublishedContent content, string term, bool useWildCards = true, string searchProvider = null)
		{
			var searcher = Examine.ExamineManager.Instance.DefaultSearchProvider;
			if (string.IsNullOrEmpty(searchProvider) == false)
				searcher = Examine.ExamineManager.Instance.SearchProviderCollection[searchProvider];

			var t = term.Escape().Value;
			if (useWildCards)
				t = term.MultipleCharacterWildcard().Value;

			var luceneQuery = "+__Path:(" + content.Path.Replace("-", "\\-") + "*) +" + t;
			var crit = searcher.CreateSearchCriteria().RawQuery(luceneQuery);

			return content.Search(crit, searcher);
		}

        public static IEnumerable<IPublishedContent> SearchDescendants(this IPublishedContent content, string term, bool useWildCards = true, string searchProvider = null)
		{
			return content.Search(term, useWildCards, searchProvider);
		}

        public static IEnumerable<IPublishedContent> SearchChildren(this IPublishedContent content, string term, bool useWildCards = true, string searchProvider = null)
		{
			var searcher = Examine.ExamineManager.Instance.DefaultSearchProvider;
			if (string.IsNullOrEmpty(searchProvider) == false)
				searcher = Examine.ExamineManager.Instance.SearchProviderCollection[searchProvider];

			var t = term.Escape().Value;
			if (useWildCards)
				t = term.MultipleCharacterWildcard().Value;

			var luceneQuery = "+parentID:" + content.Id + " +" + t;
			var crit = searcher.CreateSearchCriteria().RawQuery(luceneQuery);

			return content.Search(crit, searcher);
		}

        public static IEnumerable<IPublishedContent> Search(this IPublishedContent content, Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
		{
			var s = Examine.ExamineManager.Instance.DefaultSearchProvider;
			if (searchProvider != null)
				s = searchProvider;

			var results = s.Search(criteria);
			return results.ConvertSearchResultToPublishedContent(UmbracoContext.Current.ContentCache);
		}

		#endregion

        #region ToContentSet

        /// <summary>
        /// Returns the content enumerable as a content set.
        /// </summary>
        /// <param name="source">The content enumerable.</param>
        /// <returns>A content set wrapping the content enumerable.</returns>
        public static PublishedContentSet<T> ToContentSet<T>(this IEnumerable<T> source)
            where T : class, IPublishedContent
        {
            return new PublishedContentSet<T>(source);
        }

        /// <summary>
        /// Returns the ordered content enumerable as an ordered content set.
        /// </summary>
        /// <param name="source">The ordered content enumerable.</param>
        /// <returns>A ordered content set wrapping the ordered content enumerable.</returns>
        public static PublishedContentOrderedSet<T> ToContentSet<T>(this IOrderedEnumerable<T> source)
            where T : class, IPublishedContent
        {
            return new PublishedContentOrderedSet<T>(source);
        }

        #endregion

        #region Dynamic Linq Extensions

        // todo - we should keep this file clean and remove dynamic linq stuff from it

        public static IQueryable<IPublishedContent> OrderBy(this IEnumerable<IPublishedContent> source, string predicate)
		{
			var dList = new DynamicPublishedContentList(source);
			return dList.OrderBy<DynamicPublishedContent>(predicate);
		}

		public static IQueryable<IPublishedContent> Where(this IEnumerable<IPublishedContent> list, string predicate)
		{
            // wrap in DynamicPublishedContentList so that the ContentSet is correct
            // though that code is somewhat ugly.

		    var dlist = new DynamicPublishedContentList(new DynamicPublishedContentList(list)
		                                                    .Where<DynamicPublishedContent>(predicate));

		    return dlist.AsQueryable<IPublishedContent>();
		}

		public static IEnumerable<IGrouping<object, IPublishedContent>> GroupBy(this IEnumerable<IPublishedContent> list, string predicate)
		{
			var dList = new DynamicPublishedContentList(list);
			return dList.GroupBy(predicate);
		}

		public static IQueryable Select(this IEnumerable<IPublishedContent> list, string predicate, params object[] values)
		{
			var dList = new DynamicPublishedContentList(list);
			return dList.Select(predicate);
		}

        public static HtmlString Where(this IPublishedContent content, string predicate, string valueIfTrue)
        {
            if (content == null) throw new ArgumentNullException("content");
            return content.Where(predicate, valueIfTrue, string.Empty);
        }

        public static HtmlString Where(this IPublishedContent content, string predicate, string valueIfTrue, string valueIfFalse)
        {
            if (content == null) throw new ArgumentNullException("content");
            return new HtmlString(content.Where(predicate) ? valueIfTrue : valueIfFalse);
        }

        public static bool Where(this IPublishedContent content, string predicate)
        {
            if (content == null) throw new ArgumentNullException("content");
            var dynamicDocumentList = new DynamicPublishedContentList { content.AsDynamicOrNull() };
            var filtered = dynamicDocumentList.Where<DynamicPublishedContent>(predicate);
            return filtered.Count() == 1;
        }
        
        #endregion

        #region AsDynamic

        // it is ok to have dynamic here

        // content should NOT be null
		public static dynamic AsDynamic(this IPublishedContent content)
		{
			if (content == null) throw new ArgumentNullException("content");
			return new DynamicPublishedContent(content);
		}

        // content CAN be null
        internal static DynamicPublishedContent AsDynamicOrNull(this IPublishedContent content)
		{
		    return content == null ? null : new DynamicPublishedContent(content);
		}

        #endregion

		#region ContentSet

		public static int Position(this IPublishedContent content)
		{
			return content.GetIndex();
		}

        public static int Index(this IPublishedContent content)
        {
            return content.GetIndex();
        }

        private static int GetIndex(this IPublishedContent content, IEnumerable<IPublishedContent> set)
        {
            var index = set.FindIndex(n => n.Id == content.Id);
            if (index < 0)
                throw new IndexOutOfRangeException("Could not find content in the content set.");
            return index;
        }

		#endregion

		#region IsSomething: misc.

        /// <summary>
        /// Gets a value indicating whether the content is visible.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>A value indicating whether the content is visible.</returns>
        /// <remarks>A content is not visible if it has an umbracoNaviHide property with a value of "1". Otherwise,
        /// the content is visible.</remarks>
        public static bool IsVisible(this IPublishedContent content)
        {
            // note: would be better to ensure we have an IPropertyEditorValueConverter for booleans
            // and then treat the umbracoNaviHide property as a boolean - vs. the hard-coded "1".

            // rely on the property converter - will return default bool value, ie false, if property
            // is not defined, or has no value, else will return its value.
            return content.GetPropertyValue<bool>(Constants.Conventions.Content.NaviHide) == false;
        }

        /// <summary>
        /// Determines whether the specified content is a specified content type.
        /// </summary>
        /// <param name="content">The content to determine content type of.</param>
        /// <param name="docTypeAlias">The alias of the content type to test against.</param>
        /// <returns>True if the content is of the specified content type; otherwise false.</returns>
	    public static bool IsDocumentType(this IPublishedContent content, string docTypeAlias)
	    {
	        return content.DocumentTypeAlias.InvariantEquals(docTypeAlias);
	    }

	    /// <summary>
	    /// Determines whether the specified content is a specified content type or it's derived types.
	    /// </summary>
	    /// <param name="content">The content to determine content type of.</param>
	    /// <param name="docTypeAlias">The alias of the content type to test against.</param>
	    /// <param name="recursive">When true, recurses up the content type tree to check inheritance; when false just calls IsDocumentType(this IPublishedContent content, string docTypeAlias).</param>
	    /// <returns>True if the content is of the specified content type or a derived content type; otherwise false.</returns>
	    public static bool IsDocumentType(this IPublishedContent content, string docTypeAlias, bool recursive)
		{
			if (content.IsDocumentType(docTypeAlias))
				return true;

			if (recursive)
				return IsDocumentTypeRecursive(content, docTypeAlias);
			return false;
		}

		private static bool IsDocumentTypeRecursive(IPublishedContent content, string docTypeAlias)
		{
			var contentTypeService = UmbracoContext.Current.Application.Services.ContentTypeService;
			var type = contentTypeService.GetContentType(content.DocumentTypeAlias);
			while (type != null && type.ParentId > 0)
			{
				type = contentTypeService.GetContentType(type.ParentId);
				if (type.Alias.InvariantEquals(docTypeAlias))
					return true;
			}
			return false;
		}

		public static bool IsNull(this IPublishedContent content, string alias, bool recurse)
		{
		    return content.HasValue(alias, recurse) == false;
		}

		public static bool IsNull(this IPublishedContent content, string alias)
		{
		    return content.HasValue(alias) == false;
		}

        #endregion

        #region IsSomething: position in set

        public static bool IsFirst(this IPublishedContent content)
        {
            return content.GetIndex() == 0;
        }

        public static HtmlString IsFirst(this IPublishedContent content, string valueIfTrue)
        {
            return content.IsFirst(valueIfTrue, string.Empty);
        }

        public static HtmlString IsFirst(this IPublishedContent content, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(content.IsFirst() ? valueIfTrue : valueIfFalse);
        }

        public static bool IsNotFirst(this IPublishedContent content)
        {
            return content.IsFirst() == false;
        }

        public static HtmlString IsNotFirst(this IPublishedContent content, string valueIfTrue)
        {
            return content.IsNotFirst(valueIfTrue, string.Empty);
        }

        public static HtmlString IsNotFirst(this IPublishedContent content, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(content.IsNotFirst() ? valueIfTrue : valueIfFalse);
        }

        public static bool IsPosition(this IPublishedContent content, int index)
        {
            return content.GetIndex() == index;
        }

        public static HtmlString IsPosition(this IPublishedContent content, int index, string valueIfTrue)
        {
            return content.IsPosition(index, valueIfTrue, string.Empty);
        }

        public static HtmlString IsPosition(this IPublishedContent content, int index, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(content.IsPosition(index) ? valueIfTrue : valueIfFalse);
        }

        public static bool IsModZero(this IPublishedContent content, int modulus)
        {
            return content.GetIndex() % modulus == 0;
        }

        public static HtmlString IsModZero(this IPublishedContent content, int modulus, string valueIfTrue)
        {
            return content.IsModZero(modulus, valueIfTrue, string.Empty);
        }

        public static HtmlString IsModZero(this IPublishedContent content, int modulus, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(content.IsModZero(modulus) ? valueIfTrue : valueIfFalse);
        }

        public static bool IsNotModZero(this IPublishedContent content, int modulus)
        {
            return content.IsModZero(modulus) == false;
        }

        public static HtmlString IsNotModZero(this IPublishedContent content, int modulus, string valueIfTrue)
        {
            return content.IsNotModZero(modulus, valueIfTrue, string.Empty);
        }

        public static HtmlString IsNotModZero(this IPublishedContent content, int modulus, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(content.IsNotModZero(modulus) ? valueIfTrue : valueIfFalse);
        }

        public static bool IsNotPosition(this IPublishedContent content, int index)
        {
            return content.IsPosition(index) == false;
        }

        public static HtmlString IsNotPosition(this IPublishedContent content, int index, string valueIfTrue)
        {
            return content.IsNotPosition(index, valueIfTrue, string.Empty);
        }

        public static HtmlString IsNotPosition(this IPublishedContent content, int index, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(content.IsNotPosition(index) ? valueIfTrue : valueIfFalse);
        }

        public static bool IsLast(this IPublishedContent content)
        {
            return content.GetIndex() == content.ContentSet.Count() - 1;
        }

        public static HtmlString IsLast(this IPublishedContent content, string valueIfTrue)
        {
            return content.IsLast(valueIfTrue, string.Empty);
        }

        public static HtmlString IsLast(this IPublishedContent content, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(content.IsLast() ? valueIfTrue : valueIfFalse);
        }

        public static bool IsNotLast(this IPublishedContent content)
        {
            return content.IsLast() == false;
        }

        public static HtmlString IsNotLast(this IPublishedContent content, string valueIfTrue)
        {
            return content.IsNotLast(valueIfTrue, string.Empty);
        }

        public static HtmlString IsNotLast(this IPublishedContent content, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(content.IsNotLast() ? valueIfTrue : valueIfFalse);
        }

        public static bool IsEven(this IPublishedContent content)
        {
            return content.GetIndex() % 2 == 0;
        }

        public static HtmlString IsEven(this IPublishedContent content, string valueIfTrue)
        {
            return content.IsEven(valueIfTrue, string.Empty);
        }

        public static HtmlString IsEven(this IPublishedContent content, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(content.IsEven() ? valueIfTrue : valueIfFalse);
        }

        public static bool IsOdd(this IPublishedContent content)
        {
            return content.GetIndex() % 2 == 1;
        }

        public static HtmlString IsOdd(this IPublishedContent content, string valueIfTrue)
        {
            return content.IsOdd(valueIfTrue, string.Empty);
        }

        public static HtmlString IsOdd(this IPublishedContent content, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(content.IsOdd() ? valueIfTrue : valueIfFalse);
        } 

        #endregion

        #region IsSomething: equality

        public static bool IsEqual(this IPublishedContent content, IPublishedContent other)
		{
			return content.Id == other.Id;
		}

        public static HtmlString IsEqual(this IPublishedContent content, IPublishedContent other, string valueIfTrue)
        {
            return content.IsEqual(other, valueIfTrue, string.Empty);
        }

		public static HtmlString IsEqual(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return new HtmlString(content.IsEqual(other) ? valueIfTrue : valueIfFalse);
		}

		public static bool IsNotEqual(this IPublishedContent content, IPublishedContent other)
		{
		    return content.IsEqual(other) == false;
		}

        public static HtmlString IsNotEqual(this IPublishedContent content, IPublishedContent other, string valueIfTrue)
        {
            return content.IsNotEqual(other, valueIfTrue, string.Empty);
        }
        
        public static HtmlString IsNotEqual(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
			return new HtmlString(content.IsNotEqual(other) ? valueIfTrue : valueIfFalse);
		}

        #endregion

        #region IsSomething: ancestors and descendants

        public static bool IsDescendant(this IPublishedContent content, IPublishedContent other)
        {
            return content.Ancestors().Any(x => x.Id == other.Id);
		}

		public static HtmlString IsDescendant(this IPublishedContent content, IPublishedContent other, string valueIfTrue)
		{
		    return content.IsDescendant(other, valueIfTrue, string.Empty);
		}

		public static HtmlString IsDescendant(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
            return new HtmlString(content.IsDescendant(other) ? valueIfTrue : valueIfFalse);
		}

		public static bool IsDescendantOrSelf(this IPublishedContent content, IPublishedContent other)
		{
            return content.AncestorsOrSelf().Any(x => x.Id == other.Id);
        }

		public static HtmlString IsDescendantOrSelf(this IPublishedContent content, IPublishedContent other, string valueIfTrue)
		{
            return content.IsDescendantOrSelf(other, valueIfTrue, string.Empty);
        }

		public static HtmlString IsDescendantOrSelf(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
            return new HtmlString(content.IsDescendantOrSelf(other) ? valueIfTrue : valueIfFalse);
        }

		public static bool IsAncestor(this IPublishedContent content, IPublishedContent other)
		{
            // avoid using Descendants(), that's expensive
		    return other.Ancestors().Any(x => x.Id == content.Id);
		}

		public static HtmlString IsAncestor(this IPublishedContent content, IPublishedContent other, string valueIfTrue)
		{
            return content.IsAncestor(other, valueIfTrue, string.Empty);
        }

		public static HtmlString IsAncestor(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
            return new HtmlString(content.IsAncestor(other) ? valueIfTrue : valueIfFalse);
        }

		public static bool IsAncestorOrSelf(this IPublishedContent content, IPublishedContent other)
		{
            // avoid using DescendantsOrSelf(), that's expensive
            return other.AncestorsOrSelf().Any(x => x.Id == content.Id);
        }

		public static HtmlString IsAncestorOrSelf(this IPublishedContent content, IPublishedContent other, string valueIfTrue)
		{
            return content.IsAncestorOrSelf(other, valueIfTrue, string.Empty);
        }

		public static HtmlString IsAncestorOrSelf(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse)
		{
            return new HtmlString(content.IsAncestorOrSelf(other) ? valueIfTrue : valueIfFalse);
        }

        #endregion

        #region Axes: ancestors, ancestors-or-self

        // as per XPath 1.0 specs §2.2,
        // - the ancestor axis contains the ancestors of the context node; the ancestors of the context node consist
        //   of the parent of context node and the parent's parent and so on; thus, the ancestor axis will always
        //   include the root node, unless the context node is the root node.
        // - the ancestor-or-self axis contains the context node and the ancestors of the context node; thus,
        //   the ancestor axis will always include the root node.
        //
        // as per XPath 2.0 specs §3.2.1.1,
        // - the ancestor axis is defined as the transitive closure of the parent axis; it contains the ancestors
        //   of the context node (the parent, the parent of the parent, and so on) - The ancestor axis includes the
        //   root node of the tree in which the context node is found, unless the context node is the root node.
        // - the ancestor-or-self axis contains the context node and the ancestors of the context node; thus,
        //   the ancestor-or-self axis will always include the root node.
        //
        // the ancestor and ancestor-or-self axis are reverse axes ie they contain the context node or nodes that
        // are before the context node in document order.
        //
        // document order is defined by §2.4.1 as:
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
            return content.AncestorsOrSelf(false, n => n.DocumentTypeAlias == contentTypeAlias);
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
            return content.AncestorsOrSelf(true, n => n.DocumentTypeAlias == contentTypeAlias);
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
            return content.EnumerateAncestors(false).FirstOrDefault(x => x.DocumentTypeAlias == contentTypeAlias);
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
            return content.EnumerateAncestors(true).FirstOrDefault(x => x.DocumentTypeAlias == contentTypeAlias);
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
        
        internal static IEnumerable<IPublishedContent> AncestorsOrSelf(this IPublishedContent content, bool orSelf, Func<IPublishedContent, bool> func)
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
        /// <param name="docTypeAlias"></param>
        /// <returns></returns>
        /// <remarks>
        /// This can be useful in order to return all nodes in an entire site by a type when combined with TypedContentAtRoot
        /// </remarks>
        public static IEnumerable<IPublishedContent> DescendantsOrSelf(this IEnumerable<IPublishedContent> parentNodes, string docTypeAlias)
        {
            return parentNodes.SelectMany(x => x.DescendantsOrSelf(docTypeAlias));
        }

        /// <summary>
        /// Returns all DescendantsOrSelf of all content referenced
        /// </summary>
        /// <param name="parentNodes"></param>
        /// <returns></returns>
        /// <remarks>
        /// This can be useful in order to return all nodes in an entire site by a type when combined with TypedContentAtRoot
        /// </remarks>
        public static IEnumerable<T> DescendantsOrSelf<T>(this IEnumerable<IPublishedContent> parentNodes)
            where T : class, IPublishedContent
        {
            return parentNodes.SelectMany(x => x.DescendantsOrSelf<T>());
        } 


        // as per XPath 1.0 specs §2.2,
        // - the descendant axis contains the descendants of the context node; a descendant is a child or a child of a child and so on; thus
        //   the descendant axis never contains attribute or namespace nodes.
        // - the descendant-or-self axis contains the context node and the descendants of the context node.
        //
        // as per XPath 2.0 specs §3.2.1.1,
        // - the descendant axis is defined as the transitive closure of the child axis; it contains the descendants of the context node (the
        //   children, the children of the children, and so on).
        // - the descendant-or-self axis contains the context node and the descendants of the context node.
        //
        // the descendant and descendant-or-self axis are forward axes ie they contain the context node or nodes that are after the context
        // node in document order.
        //
        // document order is defined by §2.4.1 as:
        // - the root node is the first node.
        // - every node occurs before all of its children and descendants.
        // - the relative order of siblings is the order in which they occur in the children property of their parent node.
        // - children and descendants occur before following siblings.

        public static IEnumerable<IPublishedContent> Descendants(this IPublishedContent content)
        {
            return content.DescendantsOrSelf(false, null);
        }

        public static IEnumerable<IPublishedContent> Descendants(this IPublishedContent content, int level)
        {
            return content.DescendantsOrSelf(false, p => p.Level >= level);
        }

        public static IEnumerable<IPublishedContent> Descendants(this IPublishedContent content, string contentTypeAlias)
		{
			return content.DescendantsOrSelf(false, p => p.DocumentTypeAlias == contentTypeAlias);
		}

        public static IEnumerable<T> Descendants<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            return content.Descendants().OfType<T>();
        }

        public static IEnumerable<T> Descendants<T>(this IPublishedContent content, int level)
            where T : class, IPublishedContent
        {
            return content.Descendants(level).OfType<T>();
        }
        
        public static IEnumerable<IPublishedContent> DescendantsOrSelf(this IPublishedContent content)
        {
            return content.DescendantsOrSelf(true, null);
        }

        public static IEnumerable<IPublishedContent> DescendantsOrSelf(this IPublishedContent content, int level)
		{
			return content.DescendantsOrSelf(true, p => p.Level >= level);
		}

        public static IEnumerable<IPublishedContent> DescendantsOrSelf(this IPublishedContent content, string contentTypeAlias)
		{
			return content.DescendantsOrSelf(true, p => p.DocumentTypeAlias == contentTypeAlias);
		}

        public static IEnumerable<T> DescendantsOrSelf<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            return content.DescendantsOrSelf().OfType<T>();
        }

        public static IEnumerable<T> DescendantsOrSelf<T>(this IPublishedContent content, int level)
            where T : class, IPublishedContent
        {
            return content.DescendantsOrSelf(level).OfType<T>();
        }

        public static IPublishedContent Descendant(this IPublishedContent content)
        {
            return content.Children.FirstOrDefault();
        }

        public static IPublishedContent Descendant(this IPublishedContent content, int level)
        {
            return content.EnumerateDescendants(false).FirstOrDefault(x => x.Level == level);
        }

        public static IPublishedContent Descendant(this IPublishedContent content, string contentTypeAlias)
        {
            return content.EnumerateDescendants(false).FirstOrDefault(x => x.DocumentTypeAlias == contentTypeAlias);
        }

        public static T Descendant<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            return content.EnumerateDescendants(false).FirstOrDefault(x => x is T) as T;
        }

        public static T Descendant<T>(this IPublishedContent content, int level)
            where T : class, IPublishedContent
        {
            return content.Descendant(level) as T;
        }

        public static IPublishedContent DescendantOrSelf(this IPublishedContent content)
        {
            return content;
        }

        public static IPublishedContent DescendantOrSelf(this IPublishedContent content, int level)
        {
            return content.EnumerateDescendants(true).FirstOrDefault(x => x.Level == level);
        }

        public static IPublishedContent DescendantOrSelf(this IPublishedContent content, string contentTypeAlias)
        {
            return content.EnumerateDescendants(true).FirstOrDefault(x => x.DocumentTypeAlias == contentTypeAlias);
        }

        public static T DescendantOrSelf<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            return content.EnumerateDescendants(true).FirstOrDefault(x => x is T) as T;
        }

        public static T DescendantOrSelf<T>(this IPublishedContent content, int level)
            where T : class, IPublishedContent
        {
            return content.DescendantOrSelf(level) as T;
        }
        
        internal static IEnumerable<IPublishedContent> DescendantsOrSelf(this IPublishedContent content, bool orSelf, Func<IPublishedContent, bool> func)
        {
            return content.EnumerateDescendants(orSelf).Where(x => func == null || func(x));
        }

        internal static IEnumerable<IPublishedContent> EnumerateDescendants(this IPublishedContent content, bool orSelf)
        {
            if (orSelf) yield return content;

            foreach (var child in content.Children)
                foreach (var child2 in child.EnumerateDescendants())
                    yield return child2;
        }

        internal static IEnumerable<IPublishedContent> EnumerateDescendants(this IPublishedContent content)
        {
            yield return content;

            foreach (var child in content.Children)
                foreach (var child2 in child.EnumerateDescendants())
                    yield return child2;
        }
        
        #endregion

		#region Axes: following-sibling, preceding-sibling, following, preceding + pseudo-axes up, down, next, previous

        // up pseudo-axe ~ ancestors
        // bogus, kept for backward compatibility but we should get rid of it
        // better use ancestors

		public static IPublishedContent Up(this IPublishedContent content)
		{
		    return content.Parent;
		}

		public static IPublishedContent Up(this IPublishedContent content, int number)
		{
            if (number < 0)
                throw new ArgumentOutOfRangeException("number", "Must be greater than, or equal to, zero.");
		    return number == 0 ? content : content.EnumerateAncestors(false).Skip(number).FirstOrDefault();
		}

		public static IPublishedContent Up(this IPublishedContent content, string contentTypeAlias)
		{
		    return string.IsNullOrEmpty(contentTypeAlias) 
                ? content.Parent 
                : content.Ancestor(contentTypeAlias);
		}

        // down pseudo-axe ~ children (not descendants)
        // bogus, kept for backward compatibility but we should get rid of it
        // better use descendants

		public static IPublishedContent Down(this IPublishedContent content)
		{
		    return content.Children.FirstOrDefault();
		}

		public static IPublishedContent Down(this IPublishedContent content, int number)
		{
            if (number < 0)
                throw new ArgumentOutOfRangeException("number", "Must be greater than, or equal to, zero.");
		    if (number == 0) return content;

            content = content.Children.FirstOrDefault();
            while (content != null && --number > 0)
                content = content.Children.FirstOrDefault();

		    return content;
		}

		public static IPublishedContent Down(this IPublishedContent content, string contentTypeAlias)
		{
		    if (string.IsNullOrEmpty(contentTypeAlias))
		        return content.Children.FirstOrDefault();

            // note: this is what legacy did, but with a broken Descendant
            // so fixing Descendant will change how it works...
			return content.Descendant(contentTypeAlias);
		}

        // next pseudo-axe ~ following within the content set
        // bogus, kept for backward compatibility but we should get rid of it

		public static IPublishedContent Next(this IPublishedContent content)
		{
            return content.ContentSet.ElementAtOrDefault(content.GetIndex() + 1);
        }

		public static IPublishedContent Next(this IPublishedContent current, Func<IPublishedContent, bool> func) {
			IPublishedContent next = current.Next();
			while (next != null) {
				if (func(next)) return next;
				next = next.Next();
			}
			return null;
		}

        public static IPublishedContent Next(this IPublishedContent content, int number)
		{
            if (number < 0)
                throw new ArgumentOutOfRangeException("number", "Must be greater than, or equal to, zero.");
            return number == 0 ? content : content.ContentSet.ElementAtOrDefault(content.GetIndex() + number);
        }

        public static IPublishedContent Next(this IPublishedContent content, string contentTypeAlias)
        {
            return content.Next(contentTypeAlias, false);
        }

        public static IPublishedContent Next(this IPublishedContent content, string contentTypeAlias, bool wrap)
        {
            return content.Next(content.ContentSet, x => x.DocumentTypeAlias.InvariantEquals(contentTypeAlias), wrap);
        }

        public static T Next<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            return content.Next<T>(false);
        }

        public static T Next<T>(this IPublishedContent content, bool wrap)
            where T : class, IPublishedContent
        {
            return content.Next(content.ContentSet, x => x is T, wrap) as T;
        }

        static IPublishedContent Next(this IPublishedContent content, IEnumerable<IPublishedContent> axis, Func<IPublishedContent, bool> predicate, bool wrap)
        {
            var b4 = true;
            IPublishedContent wrapped = null;
            foreach (var c in axis)
            {
                if (b4)
                {
                    if (c.Id == content.Id)
                        b4 = false;
                    else if (wrap && wrapped == null && predicate(c))
                        wrapped = c;
                    continue;
                }
                if (predicate(c))
                    return c;
            }

            return wrapped;
        }

        // previous pseudo-axe ~ preceding within the content set
        // bogus, kept for backward compatibility but we should get rid of it

        public static IPublishedContent Previous(this IPublishedContent content)
		{
            return content.ContentSet.ElementAtOrDefault(content.GetIndex() - 1);
        }

		public static IPublishedContent Previous(this IPublishedContent current, Func<IPublishedContent, bool> func) {
			IPublishedContent prev = current.Previous();
			while (prev != null) {
				if (func(prev)) return prev;
				prev = prev.Previous();
			}
			return null;
		}

		public static IPublishedContent Previous(this IPublishedContent content, int number)
		{
            if (number < 0)
                throw new ArgumentOutOfRangeException("number", "Must be greater than, or equal to, zero.");
            return number == 0 ? content : content.ContentSet.ElementAtOrDefault(content.GetIndex() - number);
        }

		public static IPublishedContent Previous(this IPublishedContent content, string contentTypeAlias)
		{
		    return content.Previous(contentTypeAlias, false);
		}

        public static IPublishedContent Previous(this IPublishedContent content, string contentTypeAlias, bool wrap)
        {
            return content.Next(content.ContentSet.Reverse(), x => x.DocumentTypeAlias.InvariantEquals(contentTypeAlias), wrap);
        }

        public static T Previous<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            return content.Previous<T>(false);
        }

        public static T Previous<T>(this IPublishedContent content, bool wrap)
            where T : class, IPublishedContent
        {
            return content.Next(content.ContentSet.Reverse(), x => x is T, wrap) as T;
        }

        //

        [Obsolete("Obsolete, use FollowingSibling or PrecedingSibling instead.")]
		public static IPublishedContent Sibling(this IPublishedContent content, int number)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException("number", "Must be greater than, or equal to, zero.");
            number += 1; // legacy is zero-based
            return content.FollowingSibling(number);
		}

        // contentTypeAlias is case-insensitive
        [Obsolete("Obsolete, use FollowingSibling or PrecedingSibling instead.")]
        public static IPublishedContent Sibling(this IPublishedContent content, string contentTypeAlias)
        {
            // note: the original implementation seems to loop on all siblings
            // ie if it reaches the end of the set, it starts again at the beginning.
            // so here we wrap, although it's not consistent... but anyway those
            // methods should be obsoleted.

            return content.FollowingSibling(contentTypeAlias, true);
        }

        // following-sibling, preceding-sibling axes

        public static IPublishedContent FollowingSibling(this IPublishedContent content)
        {
            return content.Siblings().ElementAtOrDefault(content.GetIndex(content.Siblings()) + 1);
        }

        public static IPublishedContent FollowingSibling(this IPublishedContent content, int number)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException("number", "Must be greater than, or equal to, zero.");
            return number == 0 ? content : content.Siblings().ElementAtOrDefault(content.GetIndex(content.Siblings()) + number);
        }

        // contentTypeAlias is case-insensitive
        public static IPublishedContent FollowingSibling(this IPublishedContent content, string contentTypeAlias)
        {
            return content.FollowingSibling(contentTypeAlias, false);
        }

        // contentTypeAlias is case-insensitive
        // note: not sure that one makes a lot of sense but it is here for backward compatibility
        public static IPublishedContent FollowingSibling(this IPublishedContent content, string contentTypeAlias, bool wrap)
        {
            return content.Next(content.Siblings(), x => x.DocumentTypeAlias.InvariantEquals(contentTypeAlias), wrap);
        }

        public static T FollowingSibling<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            return content.FollowingSibling<T>(false);
        }

        public static T FollowingSibling<T>(this IPublishedContent content, bool wrap)
            where T : class, IPublishedContent
        {
            return content.Next(content.Siblings(), x => x is T, wrap) as T;
        }

        public static IPublishedContent PrecedingSibling(this IPublishedContent content)
        {
            return content.Siblings().ElementAtOrDefault(content.GetIndex(content.Siblings()) - 1);
        }

        public static IPublishedContent PrecedingSibling(this IPublishedContent content, int number)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException("number", "Must be greater than, or equal to, zero.");
            return number == 0 ? content : content.Siblings().ElementAtOrDefault(content.GetIndex(content.Siblings()) - number);
        }

        // contentTypeAlias is case-insensitive
        public static IPublishedContent PrecedingSibling(this IPublishedContent content, string contentTypeAlias)
        {
            return content.PrecedingSibling(contentTypeAlias, false);
        }

        // contentTypeAlias is case-insensitive
        // note: not sure that one makes a lot of sense but it is here for backward compatibility
        public static IPublishedContent PrecedingSibling(this IPublishedContent content, string contentTypeAlias, bool wrap)
        {
            return content.Next(content.Siblings().Reverse(), x => x.DocumentTypeAlias.InvariantEquals(contentTypeAlias), wrap);
        }

        public static T PrecedingSibling<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            return content.PrecedingSibling<T>(false);
        }

        public static T PrecedingSibling<T>(this IPublishedContent content, bool wrap)
            where T : class, IPublishedContent
        {
            return content.Next(content.Siblings().Reverse(), x => x is T, wrap) as T;
        }

        // following, preceding axes - NOT IMPLEMENTED

        // utilities

        public static IEnumerable<IPublishedContent> Siblings(this IPublishedContent content)
        {
            // content.Parent, content.Children and cache.GetAtRoot() should be fast enough,
            // or cached by the content cache, so that we don't have to implement cache here.

            // returns the true tree siblings, even if the content is in a set
            // get the root docs if parent is null

            // note: I don't like having to refer to the "current" content cache here, but
            // what else? would need root content to have a special, non-null but hidden,
            // parent...



            var siblings = content.Parent == null
                ? content.ItemType == PublishedItemType.Media ? UmbracoContext.Current.MediaCache.GetAtRoot() : UmbracoContext.Current.ContentCache.GetAtRoot()
                : content.Parent.Children;

            // make sure we run it once
            return siblings.ToArray();
        }

        public static IEnumerable<T> Siblings<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            return content.Siblings().OfType<T>();
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
            return content.Parent as T;
        }

        #endregion

        #region Axes: children

        /// <summary>
		/// Gets the children of the content.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <returns>The children of the content.</returns>
		/// <remarks>
        /// <para>Children are sorted by their sortOrder.</para>
        /// <para>This method exists for consistency, it is the same as calling content.Children as a property.</para>
		/// </remarks>
		public static IEnumerable<IPublishedContent> Children(this IPublishedContent content)
		{
		    return content.Children;
		}

        /// <summary>
        /// Gets the children of the content, filtered by a predicate.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The children of the content, filtered by the predicate.</returns>
        /// <remarks>
        /// <para>Children are sorted by their sortOrder.</para>
        /// </remarks>
        public static IEnumerable<IPublishedContent> Children(this IPublishedContent content, Func<IPublishedContent, bool> predicate)
        {
            return content.Children().Where(predicate);
        }

        /// <summary>
        /// Gets the children of the content, of a given content type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="content">The content.</param>
        /// <returns>The children of content, of the given content type.</returns>
        /// <remarks>
        /// <para>Children are sorted by their sortOrder.</para>
        /// </remarks>
        public static IEnumerable<T> Children<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            return content.Children().OfType<T>();
        }

        public static IPublishedContent FirstChild(this IPublishedContent content)
        {
            return content.Children().First();
        }

        public static IPublishedContent FirstChild(this IPublishedContent content, Func<IPublishedContent, bool> predicate)
        {
            return content.Children(predicate).FirstOrDefault();
        }

        public static IPublishedContent FirstChild<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            return content.Children<T>().FirstOrDefault();
        }

		/// <summary>
		/// Gets the children of the content in a DataTable.
		/// </summary>
        /// <param name="content">The content.</param>
        /// <param name="contentTypeAliasFilter">An optional content type alias.</param>
        /// <returns>The children of the content.</returns>
		public static DataTable ChildrenAsTable(this IPublishedContent content, string contentTypeAliasFilter = "")
		{
            return GenerateDataTable(content, contentTypeAliasFilter);
		}

		/// <summary>
        /// Gets the children of the content in a DataTable.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="contentTypeAliasFilter">An optional content type alias.</param>
        /// <returns>The children of the content.</returns>
        private static DataTable GenerateDataTable(IPublishedContent content, string contentTypeAliasFilter = "")
		{
			var firstNode = contentTypeAliasFilter.IsNullOrWhiteSpace()
								? content.Children.Any()
									? content.Children.ElementAt(0)
									: null
								: content.Children.FirstOrDefault(x => x.DocumentTypeAlias == contentTypeAliasFilter);
			if (firstNode == null)
				return new DataTable(); //no children found 

			//use new utility class to create table so that we don't have to maintain code in many places, just one
			var dt = Core.DataTableExtensions.GenerateDataTable(
				//pass in the alias of the first child node since this is the node type we're rendering headers for
				firstNode.DocumentTypeAlias,
				//pass in the callback to extract the Dictionary<string, string> of all defined aliases to their names
				alias => GetPropertyAliasesAndNames(alias),
				//pass in a callback to populate the datatable, yup its a bit ugly but it's already legacy and we just want to maintain code in one place.
				() =>
				{
					//create all row data
					var tableData = Core.DataTableExtensions.CreateTableData();
					//loop through each child and create row data for it
					foreach (var n in content.Children.OrderBy(x => x.SortOrder))
					{
						if (contentTypeAliasFilter.IsNullOrWhiteSpace() == false)
						{
							if (n.DocumentTypeAlias != contentTypeAliasFilter)
								continue; //skip this one, it doesn't match the filter
						}

						var standardVals = new Dictionary<string, object>
						    {
									{ "Id", n.Id },
									{ "NodeName", n.Name },
									{ "NodeTypeAlias", n.DocumentTypeAlias },
									{ "CreateDate", n.CreateDate },
									{ "UpdateDate", n.UpdateDate },
									{ "CreatorName", n.CreatorName },
									{ "WriterName", n.WriterName },
									{ "Url", n.Url }
								};

						var userVals = new Dictionary<string, object>();
                        foreach (var p in from IPublishedProperty p in n.Properties where p.DataValue != null select p)
                        {
                            // probably want the "object value" of the property here...
							userVals[p.PropertyTypeAlias] = p.Value;
						}
						//add the row data
						Core.DataTableExtensions.AddRowData(tableData, standardVals, userVals);
					}
					return tableData;
				}
				);
			return dt;
		}

        #endregion

        #region OfTypes

        // the .OfType<T>() filter is nice when there's only one type
        // this is to support filtering with multiple types

        public static IEnumerable<IPublishedContent> OfTypes(this IEnumerable<IPublishedContent> contents, params Type[] types)
        {
            return contents.Where(x => types.Contains(x.GetType()));
        }

        public static IEnumerable<IPublishedContent> OfTypes(this IEnumerable<IPublishedContent> contents, params string[] types)
        {
            types = types.Select(x => x.ToLowerInvariant()).ToArray();
            return contents.Where(x => types.Contains(x.DocumentTypeAlias.ToLowerInvariant()));
        }

        public static T OfType<T>(this IPublishedContent content)
            where T : class, IPublishedContent
        {
            return content as T;
        }

        #endregion

        #region PropertyAliasesAndNames

        private static Func<string, Dictionary<string, string>> _getPropertyAliasesAndNames;

		/// <summary>
		/// This is used only for unit tests to set the delegate to look up aliases/names dictionary of a content type
		/// </summary>
		internal static Func<string, Dictionary<string, string>> GetPropertyAliasesAndNames
		{
			get
			{
				return _getPropertyAliasesAndNames ?? (_getPropertyAliasesAndNames = alias =>
					{
						var userFields = ContentType.GetAliasesAndNames(alias);
						//ensure the standard fields are there
						var allFields = new Dictionary<string, string>()
							{
								{"Id", "Id"},
								{"NodeName", "NodeName"},
								{"NodeTypeAlias", "NodeTypeAlias"},
								{"CreateDate", "CreateDate"},
								{"UpdateDate", "UpdateDate"},
								{"CreatorName", "CreatorName"},
								{"WriterName", "WriterName"},
								{"Url", "Url"}
							};
						foreach (var f in userFields.Where(f => allFields.ContainsKey(f.Key) == false))
						{
							allFields.Add(f.Key, f.Value);
						}
						return allFields;
					});
			}
			set { _getPropertyAliasesAndNames = value; }
        }

        #endregion

        #region Culture

        /// <summary>
        /// Gets the culture that would be selected to render a specified content,
        /// within the context of a specified current request.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="current">The request Uri.</param>
        /// <returns>The culture that would be selected to render the content.</returns>
        public static CultureInfo GetCulture(this IPublishedContent content, Uri current = null)
        {
            return Models.ContentExtensions.GetCulture(UmbracoContext.Current,
                ApplicationContext.Current.Services.DomainService, 
                ApplicationContext.Current.Services.LocalizationService,
                ApplicationContext.Current.Services.ContentService,
                content.Id, content.Path,
                current);
        }

        #endregion
    }
}
