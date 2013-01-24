using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Routing
{

	/// <summary>
    /// Resolves the last chance IPublishedContentFinder object.
	/// </summary>
	public sealed class ContentLastChanceFinderResolver : SingleObjectResolverBase<ContentLastChanceFinderResolver, IContentFinder>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentLastChanceFinderResolver"/> class with no finder.
		/// </summary>
		/// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
		internal ContentLastChanceFinderResolver()
			: base(true)
		{ }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentLastChanceFinderResolver"/> class with an instance of a finder.
        /// </summary>
        /// <param name="finder">An instance of a finder.</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal ContentLastChanceFinderResolver(IContentFinder finder)
            : base(finder, true)
        { }
        
        /// <summary>
		/// Sets the last chance finder.
		/// </summary>
		/// <param name="finder">The finder.</param>
		/// <remarks>For developers, at application startup.</remarks>
		public void SetFinder(IContentFinder finder)
		{
			Value = finder;
		}

		/// <summary>
		/// Gets the last chance finder.
		/// </summary>
		public IContentFinder Finder
		{
			get { return Value; }
		}

	}
}