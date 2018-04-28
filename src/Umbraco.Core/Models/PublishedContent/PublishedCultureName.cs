using System;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Contains the culture specific data for a <see cref="IPublishedContent"/> item
    /// </summary>
    public struct PublishedCultureName
    {
        public PublishedCultureName(string name, string urlName) : this()
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            UrlName = urlName ?? throw new ArgumentNullException(nameof(urlName));
        }

        public string Name { get; }
        public string UrlName { get; }
    }

    /// <summary>
    /// Contains culture specific values for <see cref="IPublishedContent"/>.
    /// </summary>
    public class PublishedCultureInfos
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedCultureInfos"/> class.
        /// </summary>
        public PublishedCultureInfos(string culture, string name, bool published, DateTime publishedDate)
        {
            if (string.IsNullOrWhiteSpace(culture)) throw new ArgumentNullOrEmptyException(nameof(culture));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullOrEmptyException(nameof(name));

            Culture = culture;
            Name = name;
            UrlSegment = name.ToUrlSegment(culture);
            Published = published;
            PublishedDate = publishedDate;
        }

        /// <summary>
        /// Gets the culture.
        /// </summary>
        public string Culture { get; }

        /// <summary>
        /// Gets the name of the item.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the url segment of the item.
        /// </summary>
        public string UrlSegment { get; }

        /// <summary>
        /// Gets a value indicating whether the culture is published.
        /// </summary>
        /// <remarks>
        /// A published content item will only have published cultures, and therefore this
        /// value will always be true. On the other hand, fixme drafts?
        /// </remarks>
        public bool Published { get; }

        /// <summary>
        /// Gets the date when fixme?
        /// </summary>
        public DateTime PublishedDate { get; } // fixme - model? model.UpdateDate - here?
    }
}
