using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Umbraco.Core;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Routing
{

	/// <summary>
	/// A multiply registered resolver to manage all IPublishedContentLookup objects
	/// </summary>
	internal sealed class DocumentLookupsResolver : ManyObjectsResolverBase<DocumentLookupsResolver, IPublishedContentLookup>
	{
		
		internal DocumentLookupsResolver(IEnumerable<Type> lookups)
		{
			foreach (var l in lookups)
			{
				this.AddType(l);
			}
		} 
		
		/// <summary>
		/// Gets the <see cref="IPublishedContentLookup"/> implementations.
		/// </summary>
		public IEnumerable<IPublishedContentLookup> DocumentLookups
		{
			get { return Values; }
		}

	}

}
