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
		internal DocumentLookupsResolver(IEnumerable<Type> resolvers, IRequestDocumentLastChanceResolver lastChanceResolver)
		{
			//TODO: I've changed this to resolve types but the intances are not created yet!
			// I've created a method on the PluginTypeResolver to create types: PluginTypesResolver.Current.CreateInstances<T>()
			

			_resolverTypes.AddRange(resolvers);
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

		private readonly List<Type> _resolverTypes = new List<Type>(); 
		readonly ManyWeightedResolved<IDocumentLookup> _resolvers = new ManyWeightedResolved<IDocumentLookup>();

		public IEnumerable<IDocumentLookup> GetDocumentLookups
		{
			get { return _resolvers.Values; }
		}

		//why do we have this?
		public ManyWeightedResolved<IDocumentLookup> GetDocumentLookupResolution
		{
			get { return _resolvers; }
		}

		#endregion
	}
}
