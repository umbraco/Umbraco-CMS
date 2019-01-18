﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// Provide an abstract base class for <c>IPublishedContent</c> implementations.
    /// </summary>
    /// <remarks>This base class does which (a) consitently resolves and caches the Url, (b) provides an implementation
    /// for this[alias], and (c) provides basic content set management.</remarks>
    [DebuggerDisplay("Content Id: {Id}, Name: {Name}")]
    public abstract class PublishedContentBase : IPublishedContent
    {
        private string _url; // fixme - cannot cache urls, they depends on the current request

        #region ContentType

        public abstract PublishedContentType ContentType { get; }

        #endregion

        #region PublishedElement

        /// <inheritdoc />
        public abstract Guid Key { get; }

        #endregion

        #region PublishedContent

        /// <inheritdoc />
        public abstract int Id { get; }

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract string UrlSegment { get; }

        /// <inheritdoc />
        public abstract int SortOrder { get; }

        /// <inheritdoc />
        public abstract int Level { get; }

        /// <inheritdoc />
        public abstract string Path { get; }

        /// <inheritdoc />
        public abstract int? TemplateId { get; }

        /// <inheritdoc />
        public abstract int CreatorId { get; }

        /// <inheritdoc />
        public abstract string CreatorName { get; }

        /// <inheritdoc />
        public abstract DateTime CreateDate { get; }

        /// <inheritdoc />
        public abstract int WriterId { get; }

        /// <inheritdoc />
        public abstract string WriterName { get; }

        /// <inheritdoc />
        public abstract DateTime UpdateDate { get; }

        /// <inheritdoc />
        public virtual string Url => GetUrl();

        /// <inheritdoc />
        /// <remarks>
        /// The url of documents are computed by the document url providers. The url of medias are, at the moment,
        /// computed here from the 'umbracoFile' property -- but we should move to media url providers at some point.
        /// </remarks>
        public virtual string GetUrl(string culture = null) // fixme - consider .GetCulture("fr-FR").Url
        {
                switch (ItemType)
                {
                    case PublishedItemType.Content:
                        // fixme - consider injecting an umbraco context accessor
                        if (UmbracoContext.Current == null)
                            throw new InvalidOperationException("Cannot compute Url for a content item when UmbracoContext.Current is null.");
                        if (UmbracoContext.Current.UrlProvider == null)
                            throw new InvalidOperationException("Cannot compute Url for a content item when UmbracoContext.Current.UrlProvider is null.");
                        return UmbracoContext.Current.UrlProvider.GetUrl(this, culture);

                    case PublishedItemType.Media:
                        if (_url != null) return _url; // assume it will not depend on current uri/culture

                        var prop = GetProperty(Constants.Conventions.Media.File);
                        if (prop?.GetValue() == null)
                        {
                            _url = string.Empty;
                            return _url;
                        }

                        var propType = ContentType.GetPropertyType(Constants.Conventions.Media.File);

                        // fixme - consider implementing media url providers
                        // note: that one does not support variations
                        //This is a hack - since we now have 2 properties that support a URL: upload and cropper, we need to detect this since we always
                        // want to return the normal URL and the cropper stores data as json
                        switch (propType.EditorAlias)
                        {
                            case Constants.PropertyEditors.Aliases.UploadField:
                                _url = prop.GetValue().ToString();
                                break;
                            case Constants.PropertyEditors.Aliases.ImageCropper:
                                //get the url from the json format

                                var stronglyTyped = prop.GetValue() as ImageCropperValue;
                                if (stronglyTyped != null)
                                {
                                    _url = stronglyTyped.Src;
                                    break;
                                }
                                _url = prop.GetValue()?.ToString();
                                break;
                        }

                        return _url;

                    default:
                        throw new NotSupportedException();
                }
        }

        /// <inheritdoc />
        public abstract PublishedCultureInfo GetCulture(string culture = null);

        /// <inheritdoc />
        public abstract IReadOnlyDictionary<string, PublishedCultureInfo> Cultures { get; }

        /// <inheritdoc />
        public abstract PublishedItemType ItemType { get; }

        /// <inheritdoc />
        public abstract bool IsDraft(string culture = null);

        #endregion

        #region Tree

        /// <inheritdoc />
        public abstract IPublishedContent Parent { get; }

        /// <inheritdoc />
        public abstract IEnumerable<IPublishedContent> Children { get; }

        #endregion

        #region Properties

        /// <inheritdoc cref="IPublishedElement.Properties"/>
        public abstract IEnumerable<IPublishedProperty> Properties { get; }

        /// <inheritdoc cref="IPublishedElement.GetProperty(string)"/>
        public abstract IPublishedProperty GetProperty(string alias);

        #endregion
    }
}
