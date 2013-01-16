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
	/// Resolver for IPublishedContentFinder objects.
	/// </summary>
	internal sealed class IPublishedContentFinderResolver : ManyObjectsResolverBase<IPublishedContentFinderResolver, IPublishedContentFinder>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IPublishedContentFinderResolver"/> class with an initial list of finder types.
		/// </summary>
		/// <param name="finders">The list of finder types</param>
		/// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
		internal IPublishedContentFinderResolver(IEnumerable<Type> finders)
			: base(finders)
		{ }
		
		/// <summary>
		/// Gets the finders.
		/// </summary>
		public IEnumerable<IPublishedContentFinder> Finders
		{
			get { return Values; }
		}
	}
}
