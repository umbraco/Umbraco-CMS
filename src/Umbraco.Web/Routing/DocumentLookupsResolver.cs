using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Umbraco.Core;
using Umbraco.Core.Resolving;

namespace Umbraco.Web.Routing
{
	class DocumentLookupsResolver : ResolverBase<DocumentLookupsResolver>
	{
		internal DocumentLookupsResolver(IEnumerable<IDocumentLookup> resolvers, IRequestDocumentLastChanceResolver lastChanceResolver)
		{
			_resolvers.AddRange(resolvers);
			_lastChanceResolver.Value = lastChanceResolver;
		}

		#region LastChanceResolver

		readonly SingleResolved<IRequestDocumentLastChanceResolver> _lastChanceResolver = new SingleResolved<IRequestDocumentLastChanceResolver>(true);

		public IRequestDocumentLastChanceResolver RequestDocumentLastChanceResolver
		{
			get { return _lastChanceResolver.Value; }
			set { _lastChanceResolver.Value = value; }
		}

		#endregion

		#region Resolvers

		readonly ManyWeightedResolved<IDocumentLookup> _resolvers = new ManyWeightedResolved<IDocumentLookup>();

		public IEnumerable<IDocumentLookup> RequestDocumentResolvers
		{
			get { return _resolvers.Values; }
		}

		public ManyWeightedResolved<IDocumentLookup> RequestDocumentResolversResolution
		{
			get { return _resolvers; }
		}

		#endregion
	}
}
