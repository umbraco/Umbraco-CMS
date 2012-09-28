using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Routing;

using umbraco;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides nice urls for a nodes.
	/// </summary>
    internal class NiceUrlProvider
	{

		internal const string NullUrl = "#";

		/// <summary>
		/// Initializes a new instance of the <see cref="NiceUrlProvider"/> class.
		/// </summary>
		/// <param name="publishedContentStore">The content store.</param>
		/// <param name="umbracoContext">The Umbraco context.</param>
		public NiceUrlProvider(IPublishedContentStore publishedContentStore, UmbracoContext umbracoContext)
        {
            _umbracoContext = umbracoContext;
			_publishedContentStore = publishedContentStore;
        }

        private readonly UmbracoContext _umbracoContext;
		private readonly IPublishedContentStore _publishedContentStore;
        
		#region GetNiceUrl

		/// <summary>
		/// Gets the nice url of a node.
		/// </summary>
		/// <param name="nodeId">The node id.</param>
		/// <returns>The nice url for the node.</returns>
		/// <remarks>The url is absolute or relative depending on the current url.</remarks>
		public string GetNiceUrl(int nodeId)
		{
			return GetNiceUrl(nodeId, _umbracoContext.UmbracoUrl, false);
		}

		/// <summary>
		/// Gets the nice url of a node.
		/// </summary>
		/// <param name="nodeId">The node id.</param>
		/// <param name="current">The current url.</param>
		/// <param name="absolute">A value indicating whether the url should be absolute in any case.</param>
		/// <returns>The nice url for the node.</returns>
		/// <remarks>The url is absolute or relative depending on the current url, unless absolute is true, and then it is always absolute.</remarks>
		public string GetNiceUrl(int nodeId, Uri current, bool absolute)
        {
			Uri domainUri;
			string path;

			// do not read cache if previewing
        	var route = _umbracoContext.InPreviewMode
				? null
				: _umbracoContext.RoutesCache.GetRoute(nodeId);

            if (!string.IsNullOrEmpty(route))
            {
				// there was a route in the cache - extract domainUri and path
				// route is /<path> or <domainRootId>/<path>
				int pos = route.IndexOf('/');
				path = pos == 0 ? route : route.Substring(pos);
				domainUri = pos == 0 ? null : DomainUriAtNode(int.Parse(route.Substring(0, pos)), current);
			}
			else
			{
				// there was no route in the cache - create a route
				var node = _publishedContentStore.GetDocumentById(_umbracoContext, nodeId);
				if (node == null)
				{
					LogHelper.Warn<NiceUrlProvider>(
						"Couldn't find any page with nodeId={0}. This is most likely caused by the page not being published.",
						nodeId);

					return NullUrl; 
				}
				
				// walk up from that node until we hit a node with a domain,
				// or we reach the content root, collecting urls in the way
				var pathParts = new List<string>();
				var n = node;
				domainUri = DomainUriAtNode(n.Id, current);
				while (domainUri == null && n != null) // n is null at root
				{
					// get the url
					var urlName = n.UrlName;
					pathParts.Add(urlName);

					// move to parent node
					n = n.Parent;
					domainUri = n == null ? null : DomainUriAtNode(n.Id, current);
	            }

				// no domain, respect HideTopLevelNodeFromPath for legacy purposes
				if (domainUri == null && global::umbraco.GlobalSettings.HideTopLevelNodeFromPath)
				{
					// in theory if hideTopLevelNodeFromPath is true, then there should be only once
					// top-level node, or else domains should be assigned. but for backward compatibility
					// we add this check - we look for the document matching "/" and if it's not us, then
					// we do not hide the top level path
					// it has to be taken care of in IPublishedContentStore.GetDocumentByRoute too so if
					// "/foo" fails (looking for "/*/foo") we try also "/foo". 
					// this does not make much sense anyway esp. if both "/foo/" and "/bar/foo" exist, but
					// that's the way it works pre-4.10 and we try to be backward compat for the time being
					if (node.Parent == null)
					{
						var rootNode = _publishedContentStore.GetDocumentByRoute(_umbracoContext, "/", true);
						if (rootNode.Id == node.Id) // remove only if we're the default node
							pathParts.RemoveAt(pathParts.Count - 1);
					}
					else
					{
						pathParts.RemoveAt(pathParts.Count - 1);
					}
				}
					
				// assemble the route
				pathParts.Reverse();
				path = "/" + string.Join("/", pathParts); // will be "/" or "/foo" or "/foo/bar" etc
				route = (n == null ? "" : n.Id.ToString()) + path;

				// do not store if previewing
				if (!_umbracoContext.InPreviewMode)
					_umbracoContext.RoutesCache.Store(nodeId, route);
			}

			// assemble the url from domainUri (maybe null) and path
			return AssembleUrl(domainUri, path, current, absolute).ToString();
		}

		/// <summary>
		/// Gets the nice urls of a node.
		/// </summary>
		/// <param name="nodeId">The node id.</param>
		/// <param name="current">The current url.</param>
		/// <returns>An enumeration of all valid urls for the node.</returns>
		/// <remarks>The urls are absolute. A node can have more than one url if more than one domain is defined.</remarks>
		public IEnumerable<string> GetNiceUrls(int nodeId, Uri current)
		{
			// this is for editContent.aspx which had its own, highly buggy, implementation of NiceUrl...
			//TODO: finalize & test implementation then replace in editContent.aspx

			string path;
			IEnumerable<Uri> domainUris;

			// will not read cache if previewing!
			var route = _umbracoContext.InPreviewMode
				? null
				: _umbracoContext.RoutesCache.GetRoute(nodeId);

			if (route != null)
			{
				// route is <id>/<path> eg "-1/", "-1/foo", "123/", "123/foo/bar"...
				int pos = route.IndexOf('/');
				path = route.Substring(pos);
				int id = int.Parse(route.Substring(0, pos)); // will be -1 or 1234
				domainUris = id > 0 ? DomainUrisAtNode(id, current) : new Uri[] { };
			}
			else
			{
				var node = _publishedContentStore.GetDocumentById(_umbracoContext, nodeId);
				if (node == null)
					return new string[] { NullUrl }; // legacy wrote to the log here...

				var pathParts = new List<string>();
				int id = nodeId;
				domainUris = DomainUrisAtNode(id, current);
				while (!domainUris.Any() && id > 0)
				{
					var urlName = node.UrlName;
					pathParts.Add(urlName);
					node = node.Parent; //set to parent node
					id = node.Id;
					domainUris = id > 0 ? DomainUrisAtNode(id, current) : new Uri[] { };
				}

				// no domain, respect HideTopLevelNodeFromPath for legacy purposes
				if (!domainUris.Any() && global::umbraco.GlobalSettings.HideTopLevelNodeFromPath)
					pathParts.RemoveAt(pathParts.Count - 1);

				pathParts.Reverse();
				path = "/" + string.Join("/", pathParts); // will be "/" or "/foo" or "/foo/bar" etc
				route = id.ToString() + path;

				if (!_umbracoContext.InPreviewMode)
					_umbracoContext.RoutesCache.Store(nodeId, route);
			}

			return AssembleUrls(domainUris, path, current).Select(uri => uri.ToString());
		}

		Uri AssembleUrl(Uri domainUri, string path, Uri current, bool absolute)
		{
			Uri uri;

			if (domainUri == null)
			{
				// no domain was found : return a relative url,  add vdir if any
				uri = new Uri(SystemDirectories.Root + path, UriKind.Relative);
			}
			else
			{
				// a domain was found : return an absolute or relative url
				// cannot handle vdir, has to be in domain uri
				if (!absolute && current != null && domainUri.GetLeftPart(UriPartial.Authority) == current.GetLeftPart(UriPartial.Authority))
					uri = new Uri(CombinePaths(domainUri.AbsolutePath, path), UriKind.Relative); // relative
				else
					uri = new Uri(CombinePaths(domainUri.GetLeftPart(UriPartial.Path), path)); // absolute
			}

			return UriUtility.UriFromUmbraco(uri);
		}

		string CombinePaths(string path1, string path2)
		{
			string path = path1.TrimEnd('/') + path2;
			return path == "/" ? path : path.TrimEnd('/');
		}

		IEnumerable<Uri> AssembleUrls(IEnumerable<Uri> domainUris, string path, Uri current)
		{
			if (domainUris.Any())
			{
				return domainUris.Select(domainUri => new Uri(domainUri.GetLeftPart(UriPartial.Path).TrimEnd('/') + path));
			}
			else
			{
				// no domain was found : return a relative url,  add vdir if any
				return new Uri[] { new Uri(SystemDirectories.Root + path, UriKind.Relative) };
			}
		}

		Uri DomainUriAtNode(int nodeId, Uri current)
		{
			// be safe
			if (nodeId <= 0)
				return null;

			// apply filter on domains defined on that node
			var domainAndUri = DomainHelper.DomainMatch(Domain.GetDomainsById(nodeId), current, true);
			return domainAndUri == null ? null : domainAndUri.Uri;
		}

		IEnumerable<Uri> DomainUrisAtNode(int nodeId, Uri current)
		{
			// be safe
			if (nodeId <= 0)
				return new Uri[] { };

			var domainAndUris = DomainHelper.DomainMatches(Domain.GetDomainsById(nodeId), current);
			return domainAndUris.Select(d => d.Uri);
		}

		#endregion
    }
}