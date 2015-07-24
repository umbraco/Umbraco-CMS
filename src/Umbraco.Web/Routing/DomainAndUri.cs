using System;
using System.ComponentModel;
using Umbraco.Core;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Models;

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
        public DomainAndUri(IDomain domain, string scheme)
        {
            UmbracoDomain = domain;
            try
            {
                Uri = new Uri(UriUtility.TrimPathEndSlash(UriUtility.StartWithScheme(domain.DomainName, scheme)));
            }
            catch (UriFormatException)
            {
                var name = domain.DomainName.ToCSharpString();
                throw new ArgumentException(string.Format("Failed to parse invalid domain: node id={0}, hostname=\"{1}\"."
                    + " Hostname should be a valid uri.", domain.RootContent.Id, name), "domain");
            }
        }

        [Obsolete("This should not be used, use the other contructor specifying the non legacy IDomain instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public DomainAndUri(Domain domain, string scheme)
            : this(domain.DomainEntity, scheme)
        {
            
        }

        
        [Obsolete("This should not be used, use the non-legacy property called UmbracoDomain instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Domain Domain
        {
            get { return new Domain(UmbracoDomain); }
        }

        /// <summary>
        /// Gets the Umbraco domain.
        /// </summary>
        public IDomain UmbracoDomain { get; private set; }

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
            return string.Format("{{ \"{0}\", \"{1}\" }}", UmbracoDomain.DomainName, Uri);
        }
    }
}
