using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides methods to handle extended content.
    /// </summary>
    internal interface IPublishedContentExtended : IPublishedContent
    {
        /// <summary>
        /// Adds a property to the extended content.
        /// </summary>
        /// <param name="property">The property to add.</param>
        void AddProperty(IPublishedProperty property);

        /// <summary>
        /// Gets a value indicating whether properties were added to the extended content.
        /// </summary>
        bool HasAddedProperties { get; }

        /// <summary>
        /// Sets the content set of the extended content.
        /// </summary>
        /// <param name="contentSet"></param>
        void SetContentSet(IEnumerable<IPublishedContent> contentSet);

        /// <summary>
        /// Resets the content set of the extended content.
        /// </summary>
        void ClearContentSet();

        /// <summary>
        /// Sets the index of the extended content.
        /// </summary>
        /// <param name="value">The index value.</param>
        void SetIndex(int value);

        /// <summary>
        /// Resets the index of the extended content.
        /// </summary>
        void ClearIndex();
    }
}
