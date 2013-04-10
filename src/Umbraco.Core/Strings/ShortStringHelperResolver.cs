using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Strings
{
	/// <summary>
    /// Resolves the IShortStringHelper object
	/// </summary>
    public sealed class ShortStringHelperResolver : SingleObjectResolverBase<ShortStringHelperResolver, IShortStringHelper>
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="ShortStringHelperResolver"/> class with an instance of a helper.
        /// </summary>
        /// <param name="helper">A instance of a helper.</param>
        /// <remarks>The resolver is created by the <c>CoreBootManager</c> and thus the constructor remains internal.</remarks>
        internal ShortStringHelperResolver(IShortStringHelper helper)
            : base(helper)
        {
            Resolution.Frozen += (sender, args) => Value.Freeze();
        }
        
        /// <summary>
		/// Sets the helper.
		/// </summary>
        /// <param name="helper">The helper.</param>
		/// <remarks>For developers, at application startup.</remarks>
        public void SetHelper(IShortStringHelper helper)
		{
            Value = helper;
		}

		/// <summary>
		/// Gets the helper.
		/// </summary>
        public IShortStringHelper Helper
		{
			get { return Value; }
		}

	}
}