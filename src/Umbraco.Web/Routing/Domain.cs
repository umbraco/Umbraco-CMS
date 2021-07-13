using System;
using System.Globalization;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Represents a published snapshot domain.
    /// </summary>
    public class Domain
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Domain" /> class.
        /// </summary>
        /// <param name="id">The unique identifier of the domain.</param>
        /// <param name="name">The name of the domain.</param>
        /// <param name="contentId">The identifier of the content which supports the domain.</param>
        /// <param name="culture">The culture of the domain.</param>
        /// <param name="isWildcard">A value indicating whether the domain is a wildcard domain.</param>
        [Obsolete("Use the constructor specifying all properties instead.")]
        public Domain(int id, string name, int contentId, CultureInfo culture, bool isWildcard)
            : this(id, name, contentId, culture, isWildcard, -1)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Domain" /> class.
        /// </summary>
        /// <param name="id">The unique identifier of the domain.</param>
        /// <param name="name">The name of the domain.</param>
        /// <param name="contentId">The identifier of the content which supports the domain.</param>
        /// <param name="culture">The culture of the domain.</param>
        /// <param name="isWildcard">A value indicating whether the domain is a wildcard domain.</param>
        /// <param name="sortOrder">The sort order.</param>
        public Domain(int id, string name, int contentId, CultureInfo culture, bool isWildcard, int sortOrder)
        {
            Id = id;
            Name = name;
            ContentId = contentId;
            Culture = culture;
            IsWildcard = isWildcard;
            SortOrder = sortOrder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Domain"/> class.
        /// </summary>
        /// <param name="domain">An origin domain.</param>
        protected Domain(Domain domain)
        {
            Id = domain.Id;
            Name = domain.Name;
            ContentId = domain.ContentId;
            Culture = domain.Culture;
            IsWildcard = domain.IsWildcard;
            SortOrder = domain.SortOrder;
        }

        /// <summary>
        /// Gets the unique identifier of the domain.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the name of the domain.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the identifier of the content which supports the domain.
        /// </summary>
        public int ContentId { get; }

        /// <summary>
        /// Gets the culture of the domain.
        /// </summary>
        public CultureInfo Culture { get; }

        /// <summary>
        /// Gets a value indicating whether the domain is a wildcard domain.
        /// </summary>
        public bool IsWildcard { get; }

        /// <summary>
        /// Gets the sort order.
        /// </summary>
        /// <value>
        /// The sort order.
        /// </value>
        public int SortOrder { get; }
    }
}
