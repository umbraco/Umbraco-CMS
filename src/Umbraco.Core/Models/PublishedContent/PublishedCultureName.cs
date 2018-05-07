using System;

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
}
