using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Umbraco.Core;
using Umbraco.Core.Resolving;

namespace Umbraco.Web.Routing
{
	class LastChanceLookupResolver : SingleObjectResolverBase<LastChanceLookupResolver, IDocumentLastChanceLookup>
	{
		public LastChanceLookupResolver(IDocumentLastChanceLookup lastChanceLookup)
		{
			Value = lastChanceLookup;
		}

		/// <summary>
		/// Returns the Last Chance Lookup
		/// </summary>
		public IDocumentLastChanceLookup LastChanceLookup
		{
			get { return Value; }
		}
	}

	class DocumentLookupsResolver2 : ManyObjectResolverBase<DocumentLookupsResolver2, IDocumentLookup>
	{
		private readonly LastChanceLookupResolver _lastChanceLookupResolver;

		internal DocumentLookupsResolver2(IEnumerable<IDocumentLookup> lookups, LastChanceLookupResolver lastChanceLookupResolver)
		{
			_lastChanceLookupResolver = lastChanceLookupResolver;
			foreach (var l in lookups)
			{
				this.Add(l);
			}
		}

		/// <summary>
		/// Gets the <see cref="IDocumentLookup"/> implementations.
		/// </summary>
		public IEnumerable<IDocumentLookup> DocumentLookups
		{
			get { return Values; }
		}

		/// <summary>
		/// Gets or sets the <see cref="IDocumentLastChanceLookup"/> implementation.
		/// </summary>
		public IDocumentLastChanceLookup DocumentLastChanceLookup
		{
			get { return _lastChanceLookupResolver.LastChanceLookup; }
		}
	}

	///// <summary>
	///// Resolves the <see cref="IDocumentLookup"/> implementations and the <see cref="IDocumentLastChanceLookup"/> implementation.
	///// </summary>
	//class DocumentLookupsResolver : ResolverBase<DocumentLookupsResolver>
	//{
	//    /// <summary>
	//    /// Initializes a new instance of the <see cref="DocumentLookupsResolver"/> class with an enumeration of <see cref="IDocumentLookup"/> an an <see cref="IDocumentLastChanceLookup"/>.
	//    /// </summary>
	//    /// <param name="lookupTypes">The document lookups.</param>
	//    /// <param name="lastChanceLookup">The document last chance lookup.</param>
	//    internal DocumentLookupsResolver(IEnumerable<Type> lookupTypes, IDocumentLastChanceLookup lastChanceLookup)
	//    {
	//        //TODO: I've changed this to resolve types but the intances are not created yet!
	//        // I've created a method on the PluginTypeResolver to create types: PluginTypesResolver.Current.FindAndCreateInstances<T>()


	//        _lookupTypes.AddRange(lookupTypes);
	//        _lastChanceLookup.Value = lastChanceLookup;
	//    }

	//    #region LastChanceLookup

	//    readonly SingleResolved<IDocumentLastChanceLookup> _lastChanceLookup = new SingleResolved<IDocumentLastChanceLookup>(true);

	//    /// <summary>
	//    /// Gets or sets the <see cref="IDocumentLastChanceLookup"/> implementation.
	//    /// </summary>
	//    public IDocumentLastChanceLookup DocumentLastChanceLookup
	//    {
	//        get { return _lastChanceLookup.Value; }
	//        set { _lastChanceLookup.Value = value; }
	//    }

	//    #endregion

	//    #region Lookups

	//    private readonly List<Type> _lookupTypes = new List<Type>(); 
	//    readonly ManyWeightedResolved<IDocumentLookup> _lookups = new ManyWeightedResolved<IDocumentLookup>();

	//    /// <summary>
	//    /// Gets the <see cref="IDocumentLookup"/> implementations.
	//    /// </summary>
	//    public IEnumerable<IDocumentLookup> DocumentLookups
	//    {
	//        get { return _lookups.Values; }
	//    }

	//    //why do we have this?
	//    /// <summary>
	//    /// Gets the inner <see cref="IDocumentLookup"/> resolution.
	//    /// </summary>
	//    public ManyWeightedResolved<IDocumentLookup> DocumentLookupsResolution
	//    {
	//        get { return _lookups; }
	//    }

	//    #endregion
	//}
}
