using System;
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
            UrlProviderMode mode, string culture, Uri current)
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

        private Uri AssembleUrl(string path, Uri current, UrlProviderMode mode)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            Uri uri;

            if (current == null)
                mode = UrlProviderMode.Relative; // best we can do

            switch (mode)
            {
                case UrlProviderMode.Absolute:
                    uri = new Uri(current?.GetLeftPart(UriPartial.Authority) + path);
                    break;
                case UrlProviderMode.Relative:
                case UrlProviderMode.Auto:
                    uri = new Uri(path, UriKind.Relative);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }

            return UriUtility.MediaUriFromUmbraco(uri);
        }
    }
}
