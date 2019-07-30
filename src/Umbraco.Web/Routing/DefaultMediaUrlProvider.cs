﻿using System;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Default media url provider.
    /// </summary>
    public class DefaultMediaUrlProvider : IMediaUrlProvider
    {
        /// <inheritdoc />
        public virtual UrlInfo GetMediaUrl(UmbracoContext umbracoContext, IPublishedContent content,
            string propertyAlias,
            UrlMode mode, string culture, Uri current)
        {
            var prop = content.GetProperty(propertyAlias);
            var value = prop?.GetValue(culture);
            if (value == null)
            {
                return null;
            }

            var propType = prop.PropertyType;
            string path = null;

            switch (propType.EditorAlias)
            {
                case Constants.PropertyEditors.Aliases.UploadField:
                    path = value.ToString();
                    break;
                case Constants.PropertyEditors.Aliases.ImageCropper:
                    //get the url from the json format
                    path = value is ImageCropperValue stronglyTyped ? stronglyTyped.Src : value.ToString();
                    break;
            }

            var url = AssembleUrl(path, current, mode);
            return url == null ? null : UrlInfo.Url(url.ToString(), culture);
        }

        private Uri AssembleUrl(string path, Uri current, UrlMode mode)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            // the stored path is absolute so we just return it as is
            if(Uri.IsWellFormedUriString(path, UriKind.Absolute))
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

            return UriUtility.MediaUriFromUmbraco(uri);
        }
    }
}
