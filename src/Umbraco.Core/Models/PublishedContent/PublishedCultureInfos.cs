using System;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Contains culture specific values for <see cref="IPublishedContent"/>.
    /// </summary>
    public class PublishedCultureInfos
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedCultureInfos"/> class.
        /// </summary>
        public PublishedCultureInfos(string culture, string name, bool published, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(culture)) throw new ArgumentNullOrEmptyException(nameof(culture));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullOrEmptyException(nameof(name));

            Culture = culture;
            Name = name;
            UrlSegment = name.ToUrlSegment(culture);
            Published = published;
            Date = date;
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
        /// Gets the date associated with the culture.
        /// </summary>
        /// <remarks>
        /// <para>For published culture, this is the date the culture was published. For draft
        /// cultures, this is the date the culture was made available, ie the last time its
        /// name changed.</para>
        /// </remarks>
        public DateTime Date { get; }
    }
}
