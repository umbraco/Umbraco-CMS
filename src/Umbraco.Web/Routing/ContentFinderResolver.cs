using System;
using System.Collections.Generic;
using Umbraco.Core.LightInject;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Routing
{
	/// <summary>
    /// Resolves IPublishedContentFinder objects.
	/// </summary>
	public sealed class ContentFinderResolver : ContainerManyObjectsResolver<ContentFinderResolver, IContentFinder>
	{
	    /// <summary>
	    /// Initializes a new instance of the <see cref="ContentFinderResolver"/> class with an initial list of finder types.
	    /// </summary>
	    /// <param name="container"></param>
	    /// <param name="logger"></param>
	    /// <param name="finders">The list of finder types</param>
	    /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
	    internal ContentFinderResolver(IServiceContainer container, ILogger logger, IEnumerable<Type> finders)
            : base(container, logger, finders)
		{ }

	    /// <summary>
	    /// Initializes a new instance of the <see cref="ContentFinderResolver"/> class with an initial list of finder types.
	    /// </summary>
	    /// <param name="logger"></param>
	    /// <param name="finders">The list of finder types</param>
	    /// <param name="container"></param>
	    /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal ContentFinderResolver(IServiceContainer container, ILogger logger, params Type[] finders)
            : base(container, logger, finders)
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
