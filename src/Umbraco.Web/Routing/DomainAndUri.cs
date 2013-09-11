using System;
using Umbraco.Core;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Represents an Umbraco domain and its normalized uri.
    /// </summary>
    /// <remarks>
    /// <para>In Umbraco it is valid to create domains with name such as <c>example.com</c>, <c>https://www.example.com</c>, <c>example.com/foo/</c>.</para>
    /// <para>The normalized uri of a domain begins with a scheme and ends with no slash, eg <c>http://example.com/</c>, <c>https://www.example.com/</c>, <c>http://example.com/foo/</c>.</para>
    /// </remarks>
    public class DomainAndUri
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainAndUri"/> class with a Domain and a uri scheme.
        /// </summary>
        /// <param name="domain">The domain.</param>
        /// <param name="scheme">The uri scheme.</param>
        public DomainAndUri(Domain domain, string scheme)
        {
            Domain = domain;
            try
            {
                Uri = new Uri(UriUtility.TrimPathEndSlash(UriUtility.StartWithScheme(domain.Name, scheme)));
            }
            catch (UriFormatException)
            {
                var name = domain.Name.ToCSharpString();
                throw new ArgumentException(string.Format("Failed to parse invalid domain: node id={0}, hostname=\"{1}\"."
                    + " Hostname should be a valid uri.", domain.RootNodeId, name), "domain");
            }
        }

        /// <summary>
        /// Gets or sets the Umbraco domain.
        /// </summary>
        public Domain Domain { get; private set; }

        /// <summary>
        /// Gets or sets the normalized uri of the domain.
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// Gets a string that represents the <see cref="DomainAndUri"/> instance.
        /// </summary>
        /// <returns>A string that represents the current <see cref="DomainAndUri"/> instance.</returns>
        public override string ToString()
        {
            return string.Format("{{ \"{0}\", \"{1}\" }}", Domain.Name, Uri);
        }
    }
}
