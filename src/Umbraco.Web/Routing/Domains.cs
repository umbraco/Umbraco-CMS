using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.Routing
{
	public class Domains
	{
		public class DomainAndUri
		{
			public Domain Domain;
			public Uri Uri;

			public override string ToString()
			{
				return string.Format("{{ \"{0}\", \"{1}\" }}", Domain.Name, Uri);
			}
		}

		public static DomainAndUri ApplicableDomains(IEnumerable<Domain> domains, Uri current, bool defaultToFirst)
		{
			if (!domains.Any())
				return null;

			// sanitize the list to have proper uris for comparison (scheme, path end with /)
			// we need to end with / because example.com/foo cannot match example.com/foobar
			// we need to order so example.com/foo matches before example.com/
			var scheme = current == null ? Uri.UriSchemeHttp : current.Scheme;
			var domainsAndUris = domains
				.Select(d => new { Domain = d, UriString = UriUtility.EndPathWithSlash(UriUtility.StartWithScheme(d.Name, scheme)) })
				.OrderByDescending(t => t.UriString)
				.Select(t => new DomainAndUri { Domain = t.Domain, Uri = new Uri(t.UriString) });

			DomainAndUri domainAndUri;
			if (current == null)
			{
				// take the first one by default
				domainAndUri = domainsAndUris.First();
			}
			else
			{
				// look for a domain that would be the base of the hint
				// else take the first one by default
				var hintWithSlash = current.EndPathWithSlash();
				domainAndUri = domainsAndUris
					.FirstOrDefault(t => t.Uri.IsBaseOf(hintWithSlash));
				if (domainAndUri == null && defaultToFirst)
					domainAndUri = domainsAndUris.First();
			}

			if (domainAndUri != null)
				domainAndUri.Uri = domainAndUri.Uri.TrimPathEndSlash();
			return domainAndUri;
		}

		public static string PathRelativeToDomain(Uri domainUri, string path)
		{
			return path.Substring(domainUri.AbsolutePath.Length).AtStart('/');
		}
	}
}
