using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Umbraco.Core;
using Umbraco.Web.Routing;

using umbraco;
using umbraco.IO;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides nice urls for a nodes.
	/// </summary>
    internal class NiceUrlProvider
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="NiceUrlProvider"/> class.
		/// </summary>
		/// <param name="contentStore">The content store.</param>
		/// <param name="umbracoContext">The Umbraco context.</param>
		public NiceUrlProvider(ContentStore contentStore, UmbracoContext umbracoContext)
        {
            _umbracoContext = umbracoContext;
			_contentStore = contentStore;
        }

        private readonly UmbracoContext _umbracoContext;
		private readonly ContentStore _contentStore;

        // note: this could be a parameter...
        const string UrlNameProperty = "@urlName";

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
        	string path;
			Uri domainUri;

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
				domainUri = id > 0 ? DomainUriAtNode(id, current) : null;
			}
			else
			{
				var node = _contentStore.GetNodeById(nodeId);
				if (node == null)
					return "#"; // legacy wrote to the log here...

				var pathParts = new List<string>();
				int id = nodeId;
				domainUri = DomainUriAtNode(id, current);
				while (domainUri == null && id > 0)
				{
					pathParts.Add(_contentStore.GetNodeProperty(node, UrlNameProperty));
					node = _contentStore.GetNodeParent(node);
					id = int.Parse(_contentStore.GetNodeProperty(node, "@id")); // will be -1 or 1234
					domainUri = id > 0 ? DomainUriAtNode(id, current) : null;
	            }

				// no domain, respect HideTopLevelNodeFromPath for legacy purposes
				if (domainUri == null && global::umbraco.GlobalSettings.HideTopLevelNodeFromPath)
					pathParts.RemoveAt(pathParts.Count - 1);

				pathParts.Reverse();
				path = "/" + string.Join("/", pathParts); // will be "/" or "/foo" or "/foo/bar" etc
				route = id.ToString() + path;

				if (!_umbracoContext.InPreviewMode)
					_umbracoContext.RoutesCache.Store(nodeId, route);
			}

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
				var node = _contentStore.GetNodeById(nodeId);
				if (node == null)
					return new string[] { "#" }; // legacy wrote to the log here...

				var pathParts = new List<string>();
				int id = nodeId;
				domainUris = DomainUrisAtNode(id, current);
				while (!domainUris.Any() && id > 0)
				{
					pathParts.Add(_contentStore.GetNodeProperty(node, UrlNameProperty));
					node = _contentStore.GetNodeParent(node);
					id = int.Parse(_contentStore.GetNodeProperty(node, "@id")); // will be -1 or 1234
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
				uri = new Uri(global::umbraco.IO.SystemDirectories.Root + path, UriKind.Relative);
			}
			else
			{
				// a domain was found : return an absolute or relative url
				// cannot handle vdir, has to be in domain uri
				if (!absolute && current != null && domainUri.GetLeftPart(UriPartial.Authority) == current.GetLeftPart(UriPartial.Authority))
					uri = new Uri(domainUri.AbsolutePath.TrimEnd('/') + path, UriKind.Relative); // relative
				else
					uri = new Uri(domainUri.GetLeftPart(UriPartial.Path).TrimEnd('/') + path); // absolute
			}

			return UriFromUmbraco(uri);
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
				return new Uri[] { new Uri(global::umbraco.IO.SystemDirectories.Root + path, UriKind.Relative) };
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

		#region Map public urls to/from umbraco urls

		// fixme - what about vdir?
		// path = path.Substring(UriUtility.AppVirtualPathPrefix.Length); // remove virtual directory

		public static Uri UriFromUmbraco(Uri uri)
		{
			var path = uri.GetSafeAbsolutePath();
			if (path == "/")
				return uri;

			if (!global::umbraco.GlobalSettings.UseDirectoryUrls)
				path += ".aspx";
			else if (global::umbraco.UmbracoSettings.AddTrailingSlash)
				path += "/";

			return uri.Rewrite(path);
		}

		public static Uri UriToUmbraco(Uri uri)
		{
			var path = uri.GetSafeAbsolutePath();

			path = path.ToLower();
			if (path != "/")
				path = path.TrimEnd('/');
			if (path.EndsWith(".aspx"))
				path = path.Substring(0, path.Length - ".aspx".Length);

			return uri.Rewrite(path);
		}

		#endregion
	}
}