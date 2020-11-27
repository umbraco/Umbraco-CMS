using System;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Default media URL provider.
    /// </summary>
    public class DefaultMediaUrlProvider : IMediaUrlProvider
    {
        private readonly PropertyEditorCollection _propertyEditors;

        public DefaultMediaUrlProvider(PropertyEditorCollection propertyEditors)
        {
            _propertyEditors = propertyEditors ?? throw new ArgumentNullException(nameof(propertyEditors));
        }

        [Obsolete("Use the constructor with all parameters instead")]
        public DefaultMediaUrlProvider() : this(Current.PropertyEditors)
        {
        }

        /// <inheritdoc />
        public virtual UrlInfo GetMediaUrl(UmbracoContext umbracoContext, IPublishedContent content,
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

            if (_propertyEditors.TryGet(propType.EditorAlias, out var editor)
                && editor is IDataEditorWithMediaPath dataEditor)
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

            return UriUtility.MediaUriFromUmbraco(uri);
        }
    }
}
