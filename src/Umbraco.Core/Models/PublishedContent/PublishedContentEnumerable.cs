using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models.PublishedContent
{
    // fixme imported from 7.6 needs better explanation of what it is

    /// <summary>
    /// The published content enumerable, this model is to allow ToString to be overridden for value converters to support legacy requests for string values
    /// </summary>
    public class PublishedContentEnumerable : IEnumerable<IPublishedContent>
    {
        /// <summary>
        /// The items in the collection
        /// </summary>
        private readonly IEnumerable<IPublishedContent> _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedContentEnumerable"/> class.
        /// </summary>
        /// <param name="publishedContent">
        /// The published content items
        /// </param>
        public PublishedContentEnumerable(IEnumerable<IPublishedContent> publishedContent)
        {
            _items = publishedContent ?? throw new ArgumentNullException(nameof(publishedContent));
        }

        /// <summary>
        /// The ToString method to convert the objects back to CSV
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Join(",", _items.Select(x => x.Id));
        }

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerator"/>.
        /// </returns>
        public IEnumerator<IPublishedContent> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerator"/>.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
