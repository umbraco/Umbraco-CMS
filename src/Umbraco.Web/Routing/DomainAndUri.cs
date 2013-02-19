using System;
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
    internal class DomainAndUri
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainAndUri"/> class with a Domain and a Uri.
        /// </summary>
        /// <param name="domain">The Domain.</param>
        /// <param name="uri">The Uri.</param>
        public DomainAndUri(Domain domain, Uri uri)
        {
            Domain = domain;
            Uri = uri;
        }

        /// <summary>
        /// Gets or sets the Umbraco domain.
        /// </summary>
        public Domain Domain { get; internal set; }

        /// <summary>
        /// Gets or sets the normalized uri of the domain.
        /// </summary>
        public Uri Uri { get; internal set; }

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
