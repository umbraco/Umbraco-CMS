using System;
using System.Text;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core;
using Umbraco.Core.Xml;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IContentFinder"/> that handles page aliases.
	/// </summary>
	/// <remarks>
	/// <para>Handles <c>/just/about/anything</c> where <c>/just/about/anything</c> is contained in the <c>umbracoUrlAlias</c> property of a document.</para>
	/// <para>The alias is the full path to the document. There can be more than one alias, separated by commas.</para>
	/// </remarks>
    public class ContentFinderByUrlAlias : IContentFinder
    {
		/// <summary>
		/// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>PublishedContentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public bool TryFindContent(PublishedContentRequest docRequest)
		{
			IPublishedContent node = null;

			if (docRequest.Uri.AbsolutePath != "/") // no alias if "/"
			{
				node = FindContentByAlias(docRequest.RoutingContext.UmbracoContext.ContentCache,
					docRequest.HasDomain ? docRequest.Domain.RootNodeId : 0, 
					docRequest.Uri.GetAbsolutePathDecoded());

				if (node != null)
				{					
					docRequest.PublishedContent = node;
					LogHelper.Debug<ContentFinderByUrlAlias>("Path \"{0}\" is an alias for id={1}", () => docRequest.Uri.AbsolutePath, () => docRequest.PublishedContent.Id);
				}
			}

			return node != null;
		}

        private static IPublishedContent FindContentByAlias(ContextualPublishedContentCache cache, int rootNodeId, string alias)
        {
            if (alias == null) throw new ArgumentNullException("alias");

            // the alias may be "foo/bar" or "/foo/bar"
            // there may be spaces as in "/foo/bar,  /foo/nil"
            // these should probably be taken care of earlier on

            alias = alias.TrimStart('/');
            var xpathBuilder = new StringBuilder();
            xpathBuilder.Append(XPathStringsDefinition.Root);

            if (rootNodeId > 0)
                xpathBuilder.AppendFormat(XPathStrings.DescendantDocumentById, rootNodeId);

            XPathVariable var = null;
            if (alias.Contains('\'') || alias.Contains('"'))
            {
                // use a var, as escaping gets ugly pretty quickly
                var = new XPathVariable("alias", alias);
                alias = "$alias";
            }
            xpathBuilder.AppendFormat(XPathStrings.DescendantDocumentByAlias, alias);

            var xpath = xpathBuilder.ToString();

            // note: it's OK if var is null, will be ignored
            return cache.GetSingleByXPath(xpath, var);
        }

        #region XPath Strings

        class XPathStringsDefinition
		{
			public int Version { get; private set; }

			public static string Root { get { return "/root"; } }
			public string DescendantDocumentById { get; private set; }
			public string DescendantDocumentByAlias { get; private set; }

			public XPathStringsDefinition(int version)
			{
				Version = version;

				switch (version)
				{
					// legacy XML schema
					case 0:
						DescendantDocumentById = "//node [@id={0}]";
						DescendantDocumentByAlias = "//node[("
							+ "contains(concat(',',translate(data [@alias='umbracoUrlAlias'], ' ', ''),','),',{0},')"
							+ " or contains(concat(',',translate(data [@alias='umbracoUrlAlias'], ' ', ''),','),',/{0},')"
							+ ")]";
						break;

					// default XML schema as of 4.10
					case 1:
						DescendantDocumentById = "//* [@isDoc and @id={0}]";
						DescendantDocumentByAlias = "//* [@isDoc and ("
							+ "contains(concat(',',translate(umbracoUrlAlias, ' ', ''),','),',{0},')"
							+ " or contains(concat(',',translate(umbracoUrlAlias, ' ', ''),','),',/{0},')"
							+ ")]";
						break;

					default:
						throw new Exception(string.Format("Unsupported Xml schema version '{0}').", version));
				}
			}
		}

		static XPathStringsDefinition _xPathStringsValue;
		static XPathStringsDefinition XPathStrings
		{
			get
			{
				// in theory XPathStrings should be a static variable that
				// we should initialize in a static ctor - but then test cases
				// that switch schemas fail - so cache and refresh when needed,
				// ie never when running the actual site

				var version = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? 0 : 1;
				if (_xPathStringsValue == null || _xPathStringsValue.Version != version)
					_xPathStringsValue = new XPathStringsDefinition(version);
				return _xPathStringsValue;
			}
		}

		#endregion
    }
}