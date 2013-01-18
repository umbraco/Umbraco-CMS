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
	internal sealed class IContentFinderResolver : ManyObjectsResolverBase<IContentFinderResolver, IContentFinder>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IContentFinderResolver"/> class with an initial list of finder types.
		/// </summary>
		/// <param name="finders">The list of finder types</param>
		/// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
		internal IContentFinderResolver(IEnumerable<Type> finders)
			: base(finders)
		{ }
		
		/// <summary>
		/// Gets the finders.
		/// </summary>
		public IEnumerable<IContentFinder> Finders
		{
			get { return Values; }
		}
	}
}
