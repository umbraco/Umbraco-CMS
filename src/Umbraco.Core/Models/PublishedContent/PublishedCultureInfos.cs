﻿using System;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Contains culture specific values for <see cref="IPublishedContent"/>.
    /// </summary>
    public class PublishedCultureInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedCultureInfo"/> class.
        /// </summary>
        public PublishedCultureInfo(string culture, string name, string urlSegment, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullOrEmptyException(nameof(name));

            Culture = culture ?? throw new ArgumentNullException(nameof(culture));
            Name = name;
            UrlSegment = urlSegment;
            Date = date;
        }

        /// <summary>
        /// Gets the culture.
        /// </summary>
        public string Culture { get; }

        /// <summary>
        /// Gets the name of the item.
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// Gets the url segment of the item.
        /// </summary>
        internal string UrlSegment { get; }

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
