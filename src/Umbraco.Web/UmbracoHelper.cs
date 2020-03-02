﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.XPath;
using Umbraco.Composing;
using Umbraco.Core;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;
using Umbraco.Web.Mvc;

namespace Umbraco.Web
{
    /// <summary>
    /// A helper class that provides many useful methods and functionality for using Umbraco in templates
    /// </summary>
    /// <remarks>
    /// This object is a request based lifetime
    /// </remarks>
    public class UmbracoHelper
    {
        private readonly IPublishedContentQuery _publishedContentQuery;
        private readonly IUmbracoComponentRenderer _componentRenderer;
        private readonly ICultureDictionaryFactory _cultureDictionaryFactory;

        private IPublishedContent _currentPage;
        private ICultureDictionary _cultureDictionary;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="UmbracoHelper"/>.
        /// </summary>
        /// <param name="currentPage">The <see cref="IPublishedContent"/> item assigned to the helper.</param>
        /// <param name="cultureDictionary"></param>
        /// <param name="componentRenderer"></param>
        /// <param name="publishedContentQuery"></param>
        /// <remarks>Sets the current page to the context's published content request's content item.</remarks>
        public UmbracoHelper(IPublishedContent currentPage,
            ICultureDictionaryFactory cultureDictionary,
            IUmbracoComponentRenderer componentRenderer,
            IPublishedContentQuery publishedContentQuery)
        {
            _cultureDictionaryFactory = cultureDictionary ?? throw new ArgumentNullException(nameof(cultureDictionary));
            _componentRenderer = componentRenderer ?? throw new ArgumentNullException(nameof(componentRenderer));
            _publishedContentQuery = publishedContentQuery ?? throw new ArgumentNullException(nameof(publishedContentQuery));
            _currentPage = currentPage;
        }

        /// <summary>
        /// Initializes a new empty instance of <see cref="UmbracoHelper"/>.
        /// </summary>
        /// <remarks>For tests - nothing is initialized.</remarks>
        internal UmbracoHelper()
        { }

        #endregion


        /// <summary>
        /// Gets (or sets) the current <see cref="IPublishedContent"/> item assigned to the UmbracoHelper.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that this is the assigned IPublishedContent item to the
        /// UmbracoHelper, this is not necessarily the Current IPublishedContent
        /// item being rendered that is assigned to the UmbracoContext.
        /// This IPublishedContent object is contextual to the current UmbracoHelper instance.
        /// </para>
        ///<para>
        /// In some cases accessing this property will throw an exception if
        /// there is not IPublishedContent assigned to the Helper this will
        /// only ever happen if the Helper is constructed via DI during a non front-end request.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the
        /// UmbracoHelper is constructed with an UmbracoContext and it is not a
        /// front-end request.</exception>
        public IPublishedContent AssignedContentItem
        {
            get
            {
                if (_currentPage != null)
                {
                    return _currentPage;
                }

                throw new InvalidOperationException(
                    $"Cannot return the {nameof(IPublishedContent)} because the {nameof(UmbracoHelper)} was not constructed with an {nameof(IPublishedContent)}."
                    );

            }
            set => _currentPage = value;
        }

        /// <summary>
        /// Renders the template for the specified pageId and an optional altTemplateId
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="altTemplateId">If not specified, will use the template assigned to the node</param>
        /// <returns></returns>
        public IHtmlString RenderTemplate(int contentId, int? altTemplateId = null)
        {
            return _componentRenderer.RenderTemplate(contentId, altTemplateId);
        }

        #region RenderMacro

        /// <summary>
        /// Renders the macro with the specified alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        public IHtmlString RenderMacro(string alias)
        {
            return _componentRenderer.RenderMacro(AssignedContentItem?.Id ?? 0, alias, null);
        }

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IHtmlString RenderMacro(string alias, object parameters)
        {
            return _componentRenderer.RenderMacro(AssignedContentItem?.Id ?? 0, alias, parameters?.ToDictionary<object>());
        }

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IHtmlString RenderMacro(string alias, IDictionary<string, object> parameters)
        {
            return _componentRenderer.RenderMacro(AssignedContentItem?.Id ?? 0, alias, parameters);
        }

        #endregion

        #region Dictionary

        /// <summary>
        /// Returns the dictionary value for the key specified
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetDictionaryValue(string key)
        {
            return CultureDictionary[key];
        }

        /// <summary>
        /// Returns the dictionary value for the key specified, and if empty returns the specified default fall back value
        /// </summary>
        /// <param name="key">key of dictionary item</param>
        /// <param name="altText">fall back text if dictionary item is empty - Name altText to match Umbraco.Field</param>
        /// <returns></returns>
        public string GetDictionaryValue(string key, string altText)
        {
            var dictionaryValue = GetDictionaryValue(key);
            if (String.IsNullOrWhiteSpace(dictionaryValue))
            {
                dictionaryValue = altText;
            }
            return dictionaryValue;
        }

        /// <summary>
        /// Returns the ICultureDictionary for access to dictionary items
        /// </summary>
        public ICultureDictionary CultureDictionary => _cultureDictionary ??= _cultureDictionaryFactory.CreateDictionary();

        #endregion



        #region Content

        /// <summary>
        /// Gets a content item from the cache.
        /// </summary>
        /// <param name="id">The unique identifier, or the key, of the content item.</param>
        /// <returns>The content, or null of the content item is not in the cache.</returns>
        public IPublishedContent Content(object id)
        {
            return ContentForObject(id);
        }

        private IPublishedContent ContentForObject(object id)
        {
            if (ConvertIdObjectToInt(id, out var intId))
                return _publishedContentQuery.Content(intId);
            if (ConvertIdObjectToGuid(id, out var guidId))
                return _publishedContentQuery.Content(guidId);
            if (ConvertIdObjectToUdi(id, out var udiId))
                return _publishedContentQuery.Content(udiId);
            return null;
        }

        /// <summary>
        /// Gets a content item from the cache.
        /// </summary>
        /// <param name="id">The unique identifier of the content item.</param>
        /// <returns>The content, or null of the content item is not in the cache.</returns>
        public IPublishedContent Content(int id)
        {
            return _publishedContentQuery.Content(id);
        }

        /// <summary>
        /// Gets a content item from the cache.
        /// </summary>
        /// <param name="id">The key of the content item.</param>
        /// <returns>The content, or null of the content item is not in the cache.</returns>
        public IPublishedContent Content(Guid id)
        {
            return _publishedContentQuery.Content(id);
        }

        /// <summary>
        /// Gets a content item from the cache.
        /// </summary>
        /// <param name="id">The unique identifier, or the key, of the content item.</param>
        /// <returns>The content, or null of the content item is not in the cache.</returns>
        public IPublishedContent Content(string id)
        {
            return ContentForObject(id);
        }

        public IPublishedContent Content(Udi id)
        {
            return _publishedContentQuery.Content(id);
        }

        public IPublishedContent ContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        {
            return _publishedContentQuery.ContentSingleAtXPath(xpath, vars);
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The unique identifiers, or the keys, of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        /// <remarks>Does not support mixing identifiers and keys.</remarks>
        public IEnumerable<IPublishedContent> Content(params object[] ids)
        {
            return ContentForObjects(ids);
        }

        /// <summary>
        /// Gets the contents corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The content identifiers.</param>
        /// <returns>The existing contents corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Content(params Udi[] ids)
        {
            return ids.Select(id => _publishedContentQuery.Content(id)).WhereNotNull();
        }

        /// <summary>
        /// Gets the contents corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The content identifiers.</param>
        /// <returns>The existing contents corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Content(params GuidUdi[] ids)
        {
            return ids.Select(id => _publishedContentQuery.Content(id));
        }

        private IEnumerable<IPublishedContent> ContentForObjects(IEnumerable<object> ids)
        {
            var idsA = ids.ToArray();
            if (ConvertIdsObjectToInts(idsA, out var intIds))
                return _publishedContentQuery.Content(intIds);
            if (ConvertIdsObjectToGuids(idsA, out var guidIds))
                return _publishedContentQuery.Content(guidIds);
            return Enumerable.Empty<IPublishedContent>();
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The unique identifiers of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        public IEnumerable<IPublishedContent> Content(params int[] ids)
        {
            return _publishedContentQuery.Content(ids);
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The keys of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        public IEnumerable<IPublishedContent> Content(params Guid[] ids)
        {
            return _publishedContentQuery.Content(ids);
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The unique identifiers, or the keys, of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        /// <remarks>Does not support mixing identifiers and keys.</remarks>
        public IEnumerable<IPublishedContent> Content(params string[] ids)
        {
            return ContentForObjects(ids);
        }

        /// <summary>
        /// Gets the contents corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The content identifiers.</param>
        /// <returns>The existing contents corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Content(IEnumerable<object> ids)
        {
            return ContentForObjects(ids);
        }
        /// <summary>
        /// Gets the contents corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The content identifiers.</param>
        /// <returns>The existing contents corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Content(IEnumerable<Udi> ids)
        {
            return ids.Select(id => _publishedContentQuery.Content(id)).WhereNotNull();
        }

        /// <summary>
        /// Gets the contents corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The content identifiers.</param>
        /// <returns>The existing contents corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Content(IEnumerable<GuidUdi> ids)
        {
            return ids.Select(id => _publishedContentQuery.Content(id));
        }

        /// <summary>
        /// Gets the contents corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The content identifiers.</param>
        /// <returns>The existing contents corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Content(IEnumerable<string> ids)
        {
            return ContentForObjects(ids);
        }

        /// <summary>
        /// Gets the contents corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The content identifiers.</param>
        /// <returns>The existing contents corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Content(IEnumerable<int> ids)
        {
            return _publishedContentQuery.Content(ids);
        }

        public IEnumerable<IPublishedContent> ContentAtXPath(string xpath, params XPathVariable[] vars)
        {
            return _publishedContentQuery.ContentAtXPath(xpath, vars);
        }

        public IEnumerable<IPublishedContent> ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return _publishedContentQuery.ContentAtXPath(xpath, vars);
        }

        public IEnumerable<IPublishedContent> ContentAtRoot()
        {
            return _publishedContentQuery.ContentAtRoot();
        }

        internal static bool ConvertIdObjectToInt(object id, out int intId)
        {
            switch (id)
            {
                case string s:
                    return int.TryParse(s, out intId);

                case int i:
                    intId = i;
                    return true;

                default:
                    intId = default;
                    return false;
            }
        }

        internal static bool ConvertIdObjectToGuid(object id, out Guid guidId)
        {
            switch (id)
            {
                case string s:
                    return Guid.TryParse(s, out guidId);

                case Guid g:
                    guidId = g;
                    return true;

                default:
                    guidId = default;
                    return false;
            }
        }

        private static bool ConvertIdsObjectToInts(IEnumerable<object> ids, out IEnumerable<int> intIds)
        {
            var list = new List<int>();
            intIds = null;
            foreach (var id in ids)
            {
                if (ConvertIdObjectToInt(id, out var intId))
                    list.Add(intId);
                else
                    return false; // if one of them is not an int, fail
            }
            intIds = list;
            return true;
        }

        private static bool ConvertIdsObjectToGuids(IEnumerable<object> ids, out IEnumerable<Guid> guidIds)
        {
            var list = new List<Guid>();
            guidIds = null;
            foreach (var id in ids)
            {
                Guid guidId;
                if (ConvertIdObjectToGuid(id, out guidId))
                    list.Add(guidId);
                else
                    return false; // if one of them is not a guid, fail
            }
            guidIds = list;
            return true;
        }

        /// <remarks>Had to change to internal for testing.</remarks>
        internal static bool ConvertIdObjectToUdi(object id, out Udi guidId)
        {
            switch (id)
            {
                case string s:
                    return UdiParser.TryParse(s, out guidId);

                case Udi u:
                    guidId = u;
                    return true;

                default:
                    guidId = default;
                    return false;
            }
        }


        #endregion
        #region Media

        public IPublishedContent Media(Udi id)
        {
            var guidUdi = id as GuidUdi;
            return guidUdi == null ? null : Media(guidUdi.Guid);
        }

        public IPublishedContent Media(Guid id)
        {
            return _publishedContentQuery.Media(id);
        }

        /// <summary>
        /// Overloaded method accepting an 'object' type
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        /// We accept an object type because GetPropertyValue now returns an 'object', we still want to allow people to pass
        /// this result in to this method.
        /// This method will throw an exception if the value is not of type int or string.
        /// </remarks>
        public IPublishedContent Media(object id)
        {
            return MediaForObject(id);
        }

        private IPublishedContent MediaForObject(object id)
        {
            if (ConvertIdObjectToInt(id, out var intId))
                return _publishedContentQuery.Media(intId);
            if (ConvertIdObjectToGuid(id, out var guidId))
                return _publishedContentQuery.Media(guidId);
            if (ConvertIdObjectToUdi(id, out var udiId))
                return _publishedContentQuery.Media(udiId);
            return null;
        }

        public IPublishedContent Media(int id)
        {
            return _publishedContentQuery.Media(id);
        }

        public IPublishedContent Media(string id)
        {
            return MediaForObject(id);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Media(params object[] ids)
        {
            return MediaForObjects(ids);
        }

        private IEnumerable<IPublishedContent> MediaForObjects(IEnumerable<object> ids)
        {
            var idsA = ids.ToArray();
            if (ConvertIdsObjectToInts(idsA, out var intIds))
                return _publishedContentQuery.Media(intIds);
            if (ConvertIdsObjectToGuids(idsA, out var guidIds))
                return _publishedContentQuery.Media(guidIds);
            return Enumerable.Empty<IPublishedContent>();
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Media(params int[] ids)
        {
            return _publishedContentQuery.Media(ids);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Media(params string[] ids)
        {
            return MediaForObjects(ids);
        }


        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Media(params Udi[] ids)
        {
            return ids.Select(id => _publishedContentQuery.Media(id)).WhereNotNull();
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Media(params GuidUdi[] ids)
        {
            return ids.Select(id => _publishedContentQuery.Media(id));
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Media(IEnumerable<object> ids)
        {
            return MediaForObjects(ids);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Media(IEnumerable<int> ids)
        {
            return _publishedContentQuery.Media(ids);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Media(IEnumerable<Udi> ids)
        {
            return ids.Select(id => _publishedContentQuery.Media(id)).WhereNotNull();
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Media(IEnumerable<GuidUdi> ids)
        {
            return ids.Select(id => _publishedContentQuery.Media(id));
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Media(IEnumerable<string> ids)
        {
            return MediaForObjects(ids);
        }

        public IEnumerable<IPublishedContent> MediaAtRoot()
        {
            return _publishedContentQuery.MediaAtRoot();
        }

        #endregion

        internal static bool DecryptAndValidateEncryptedRouteString(string ufprt, out IDictionary<string, string> parts)
        {
            string decryptedString;
            try
            {
                decryptedString = ufprt.DecryptWithMachineKey();
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentException)
            {
                Current.Logger.Warn(typeof(UmbracoHelper), "A value was detected in the ufprt parameter but Umbraco could not decrypt the string");
                parts = null;
                return false;
            }
            var parsedQueryString = HttpUtility.ParseQueryString(decryptedString);
            parts = new Dictionary<string, string>();
            foreach (var key in parsedQueryString.AllKeys)
            {
                parts[key] = parsedQueryString[key];
            }
            //validate all required keys exist
            //the controller
            if (parts.All(x => x.Key != RenderRouteHandler.ReservedAdditionalKeys.Controller))
                return false;
            //the action
            if (parts.All(x => x.Key != RenderRouteHandler.ReservedAdditionalKeys.Action))
                return false;
            //the area
            if (parts.All(x => x.Key != RenderRouteHandler.ReservedAdditionalKeys.Area))
                return false;

            return true;
        }
    }
}
