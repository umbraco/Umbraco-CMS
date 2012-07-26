using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Umbraco.Core;
using Umbraco.Core.Resolving;

namespace Umbraco.Web.Routing
{
	class RequestDocumentResolversResolver : ResolverBase<RequestDocumentResolversResolver>
	{
		internal RequestDocumentResolversResolver(IEnumerable<IRequestDocumentResolver> resolvers, IRequestDocumentLastChanceResolver lastChanceResolver)
		{
			_resolvers.AddRange(resolvers);
			_lastChanceResolver.Value = lastChanceResolver;
		}

		#region LastChanceResolver

		SingleResolved<IRequestDocumentLastChanceResolver> _lastChanceResolver = new SingleResolved<IRequestDocumentLastChanceResolver>(true);

		public IRequestDocumentLastChanceResolver RequestDocumentLastChanceResolver
		{
			get { return _lastChanceResolver.Value; }
			set { _lastChanceResolver.Value = value; }
		}

		#endregion

		#region Resolvers

		ManyWeightedResolved<IRequestDocumentResolver> _resolvers = new ManyWeightedResolved<IRequestDocumentResolver>();

		public IEnumerable<IRequestDocumentResolver> RequestDocumentResolvers
		{
			get { return _resolvers.Values; }
		}

		public ManyWeightedResolved<IRequestDocumentResolver> RequestDocumentResolversResolution
		{
			get { return _resolvers; }
		}

		#endregion
	}
}
