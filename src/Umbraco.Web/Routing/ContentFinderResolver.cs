using System;
using System.Collections.Generic;
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
		/// <param name="finders">The list of finder types</param>
		/// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
		internal ContentFinderResolver(IEnumerable<Type> finders)
			: base(finders)
		{ }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentFinderResolver"/> class with an initial list of finder types.
        /// </summary>
        /// <param name="finders">The list of finder types</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal ContentFinderResolver(params Type[] finders)
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
