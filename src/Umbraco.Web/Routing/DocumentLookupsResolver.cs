using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Umbraco.Core;
using Umbraco.Core.Resolving;

namespace Umbraco.Web.Routing
{

	/// <summary>
	/// A multiply registered resolver to manage all IDocumentLookup objects
	/// </summary>
	internal sealed class DocumentLookupsResolver : ManyObjectsResolverBase<IDocumentLookup>
	{
		#region Singleton
		private static readonly DocumentLookupsResolver Instance = new DocumentLookupsResolver(
			//add all known resolvers in the correct order, devs can then modify this list on application startup either by binding to events
			//or in their own global.asax. We could also in the future allow these to be plugin types to be searched by searching for 
			//any IDocumentLookup with a supplied attribute and find them here and add them to the list
				new[]
					{
						typeof (LookupByNiceUrl),
						typeof (LookupById),
						typeof (LookupByNiceUrlAndTemplate),
						typeof (LookupByProfile),
						typeof (LookupByAlias)
					});

		public static DocumentLookupsResolver Current
		{
			get { return Instance; }
		} 
		#endregion

		#region Constructors
		static DocumentLookupsResolver() { }
		internal DocumentLookupsResolver(IEnumerable<Type> lookups)
		{
			foreach (var l in lookups)
			{
				this.AddType(l);
			}
		} 
		#endregion

		/// <summary>
		/// Gets the <see cref="IDocumentLookup"/> implementations.
		/// </summary>
		public IEnumerable<IDocumentLookup> DocumentLookups
		{
			get { return Values; }
		}

	}

}
