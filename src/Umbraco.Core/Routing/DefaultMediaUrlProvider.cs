﻿using System;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Default media url provider.
    /// </summary>
    public class DefaultMediaUrlProvider : IMediaUrlProvider
    {
        private readonly UriUtility _uriUtility;
        private readonly MediaUrlGeneratorCollection _mediaPathGenerators;

        public DefaultMediaUrlProvider(MediaUrlGeneratorCollection mediaPathGenerators, UriUtility uriUtility)
        {
            _mediaPathGenerators = mediaPathGenerators ?? throw new ArgumentNullException(nameof(mediaPathGenerators));
            _uriUtility = uriUtility;
        }

        /// <inheritdoc />
        public virtual UrlInfo GetMediaUrl(IPublishedContent content,
            string propertyAlias, UrlMode mode, string culture, Uri current)
        {
            var prop = content.GetProperty(propertyAlias);

            // get the raw source value since this is what is used by IDataEditorWithMediaPath for processing
            var value = prop?.GetSourceValue(culture);
            if (value == null)
            {
                return null;
            }

            var propType = prop.PropertyType;

            if (_mediaPathGenerators.TryGetMediaPath(propType.EditorAlias, value, out var path))
            {
                var url = AssembleUrl(path, current, mode);
                return UrlInfo.Url(url.ToString(), culture);
            }

            return null;
        }

        private Uri AssembleUrl(string path, Uri current, UrlMode mode)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException($"{nameof(path)} cannot be null or whitespace", nameof(path));

            // the stored path is absolute so we just return it as is
            if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
                return new Uri(path);

            Uri uri;

            if (current == null)
                mode = UrlMode.Relative; // best we can do

            switch (mode)
            {
                case UrlMode.Absolute:
                    uri = new Uri(current?.GetLeftPart(UriPartial.Authority) + path);
                    break;
                case UrlMode.Relative:
                case UrlMode.Auto:
                    uri = new Uri(path, UriKind.Relative);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }

            return _uriUtility.MediaUriFromUmbraco(uri);
        }
    }
}
