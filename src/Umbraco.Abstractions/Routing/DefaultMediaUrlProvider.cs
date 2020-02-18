using System;
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
        private readonly DataEditorWithMediaPathCollection _dataEditors;

        public DefaultMediaUrlProvider(DataEditorWithMediaPathCollection dataEditors, UriUtility uriUtility)
        {
            _dataEditors = dataEditors ?? throw new ArgumentNullException(nameof(dataEditors));
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
            string path = null;

            if (_dataEditors.TryGet(propType.EditorAlias, out var dataEditor))
            {
                path = dataEditor.GetMediaPath(value);
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

            return _uriUtility.MediaUriFromUmbraco(uri);
        }
    }
}
