using System;
using Umbraco.Core;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Represents a published snapshot domain with its normalized uri.
    /// </summary>
    /// <remarks>
    /// <para>In Umbraco it is valid to create domains with name such as <c>example.com</c>, <c>https://www.example.com</c>, <c>example.com/foo/</c>.</para>
    /// <para>The normalized uri of a domain begins with a scheme and ends with no slash, eg <c>http://example.com/</c>, <c>https://www.example.com/</c>, <c>http://example.com/foo/</c>.</para>
    /// </remarks>
    public class DomainAndUri : Domain
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainAndUri"/> class.
        /// </summary>
        /// <param name="domain">The original domain.</param>
        /// <param name="currentUri">The context current Uri.</param>
        public DomainAndUri(Domain domain, Uri currentUri)
            : base(domain)
        {
            try
            {
                // turn "/en" into "http://whatever.com/en" so it becomes a parseable uri
                var name = Name.StartsWith("/") && currentUri != null
                    ? currentUri.GetLeftPart(UriPartial.Authority) + Name
                    : Name;
                var scheme = currentUri?.Scheme ?? Uri.UriSchemeHttp;
                Uri = new Uri(UriUtility.TrimPathEndSlash(UriUtility.StartWithScheme(name, scheme)));
            }
            catch (UriFormatException)
            {
                throw new ArgumentException($"Failed to parse invalid domain: node id={domain.ContentId}, hostname=\"{Name.ToCSharpString()}\"."
                    + " Hostname should be a valid uri.", nameof(domain));
            }
        }

        /// <summary>
        /// Gets the normalized uri of the domain, within the current context.
        /// </summary>
        public Uri Uri { get; }

        public override string ToString()
        {
            return $"{{ \"{Name}\", \"{Uri}\" }}";
        }
    }
}
