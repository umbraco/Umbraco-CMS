namespace Umbraco.Core
{
	public static partial class Constants
	{
		/// <summary>
		/// Defines the identifiers for Umbraco system nodes.
		/// </summary>
		public static class System
		{
			/// <summary>
			/// The integer identifier for global system root node.
			/// </summary>
			public const int Root = -1;

			/// <summary>
			/// The integer identifier for content's recycle bin.
			/// </summary>
			public const int RecycleBinContent = -20;

			/// <summary>
			/// The integer identifier for media's recycle bin.
			/// </summary>
			public const int RecycleBinMedia = -21;

		}

        /// <summary>
        /// Defines the identifiers for Umbraco system nodes.
        /// </summary>
        public static class Web
        {
            /// <summary>
            /// The preview cookie name
            /// </summary>
            public const string PreviewCookieName = "UMB_PREVIEW";

            /// <summary>
            /// The auth cookie name
            /// </summary>
            public const string AuthCookieName = "UMB_UCONTEXT";

        }
	}
}