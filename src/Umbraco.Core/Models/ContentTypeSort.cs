using System;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a POCO for setting sort order on a ContentType reference
    /// </summary>
    public class ContentTypeSort : IValueObject
    {
        public ContentTypeSort()
        {
        }

        public ContentTypeSort(Lazy<int> id, int sortOrder, string @alias)
        {
            Id = id;
            SortOrder = sortOrder;
            Alias = alias;
        }

        /// <summary>
        /// Gets or sets the Id of the ContentType
        /// </summary>
        public Lazy<int> Id { get; set; }

        /// <summary>
        /// Gets or sets the Sort Order of the ContentType
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the Alias of the ContentType
        /// </summary>
        public string Alias { get; set; }
    }
}