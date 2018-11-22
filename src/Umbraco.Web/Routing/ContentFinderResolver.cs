using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Routing
{
	/// <summary>
    /// Resolves IPublishedContentFinder objects.
	/// </summary>
	public sealed class ContentFinderResolver : ManyObjectsResolverBase<ContentFinderResolver, IContentFinder>
	{
	    /// <summary>
	    /// Initializes a new instance of the <see cref="ContentFinderResolver"/> class with an initial list of finder types.
	    /// </summary>
	    /// <param name="serviceProvider"></param>
	    /// <param name="logger"></param>
	    /// <param name="finders">The list of finder types</param>
	    /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
	    internal ContentFinderResolver(IServiceProvider serviceProvider, ILogger logger, IEnumerable<Type> finders)
			: base(serviceProvider, logger, finders)
		{ }

	    /// <summary>
	    /// Initializes a new instance of the <see cref="ContentFinderResolver"/> class with an initial list of finder types.
	    /// </summary>
	    /// <param name="logger"></param>
	    /// <param name="finders">The list of finder types</param>
	    /// <param name="serviceProvider"></param>
	    /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
	    internal ContentFinderResolver(IServiceProvider serviceProvider, ILogger logger, params Type[] finders)
            : base(serviceProvider, logger, finders)
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
