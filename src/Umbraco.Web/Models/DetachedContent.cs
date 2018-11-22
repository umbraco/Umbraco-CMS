using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models
{
    internal class DetachedContent
    {
        private readonly Dictionary<string, IPublishedProperty> _properties;

        /// <summary>
        /// Initialized a new instance of the <see cref="DetachedContent"/> class with properties.
        /// </summary>
        /// <param name="properties">The properties</param>
        /// <remarks>Properties must be detached or nested properties ie their property type must be detached or nested.
        /// Such a detached content can be part of a published property value.</remarks>
        public DetachedContent(IEnumerable<IPublishedProperty> properties)
        {
            _properties = properties.ToDictionary(x => x.PropertyTypeAlias, x => x, StringComparer.InvariantCultureIgnoreCase);
        }

        // don't uncomment until you know what you are doing
        /*
        public DetachedContent(IPublishedContent content)
        {
            _properties = content.Properties.ToDictionary(x => x.PropertyTypeAlias, x => x, StringComparer.InvariantCultureIgnoreCase);
        }
        */

        // don't uncomment until you know what you are doing
        // at the moment, I don't fully
        /*
        public DetachedContent(IContent content, bool isPreviewing)
        {
            var publishedContentType = PublishedContentType.Get(PublishedItemType.Content, content.ContentType.Alias);
            _properties = PublishedProperty.MapProperties(publishedContentType.PropertyTypes, content.Properties,
                (t, v) => PublishedProperty.GetDetached(t.Detached(), v, isPreviewing))
                .ToDictionary(x => x.PropertyTypeAlias, x => x, StringComparer.InvariantCultureIgnoreCase);
        }

        public DetachedContent(IMedia media, bool isPreviewing)
        {
            var publishedContentType = PublishedContentType.Get(PublishedItemType.Media, media.ContentType.Alias);
            _properties = PublishedProperty.MapProperties(publishedContentType.PropertyTypes, media.Properties,
                (t, v) => PublishedProperty.GetDetached(t.Detached(), v, isPreviewing))
                .ToDictionary(x => x.PropertyTypeAlias, x => x, StringComparer.InvariantCultureIgnoreCase);
        }

        public DetachedContent(IMember member, bool isPreviewing)
        {
            var publishedContentType = PublishedContentType.Get(PublishedItemType.Member, member.ContentType.Alias);
            _properties = PublishedProperty.MapProperties(publishedContentType.PropertyTypes, member.Properties,
                (t, v) => PublishedProperty.GetDetached(t.Detached(), v, isPreviewing))
                .ToDictionary(x => x.PropertyTypeAlias, x => x, StringComparer.InvariantCultureIgnoreCase);
        }
        */

        public ICollection<IPublishedProperty> Properties
        {
            get { return _properties.Values; }
        }

        public IPublishedProperty GetProperty(string alias)
        {
            IPublishedProperty property;
            return _properties.TryGetValue(alias, out property) ? property : null;
        }

        public bool HasProperty(string alias)
        {
            var property = GetProperty(alias);
            return property != null;
        }

        public bool HasValue(string alias)
        {
            var property = GetProperty(alias);
            return property != null && property.HasValue;
        }

        public object GetPropertyValue(string alias)
        {
            var property = GetProperty(alias);
            return property == null ? null : property.Value;
        }

        public object GetPropertyValue(string alias, string defaultValue)
        {
            var property = GetProperty(alias);
            return property == null || property.HasValue == false ? defaultValue : property.Value;
        }

        public object GetPropertyValue(string alias, object defaultValue)
        {
            var property = GetProperty(alias);
            return property == null || property.HasValue == false ? defaultValue : property.Value;
        }

        public T GetPropertyValue<T>(string alias)
        {
            var property = GetProperty(alias);
            if (property == null) return default(T);
            return property.GetValue(false, default(T));
        }

        public T GetPropertyValue<T>(string alias, T defaultValue)
        {
            var property = GetProperty(alias);
            if (property == null) return defaultValue;
            return property.GetValue(true, defaultValue);
        }
    }
}
