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

		    public const int DefaultContentListViewDataTypeId = -95;
            public const int DefaultMediaListViewDataTypeId = -96;
            public const int DefaultMembersListViewDataTypeId = -97;

            // identifiers for lock objects
		    public const int ServersLock = -331;
		}
	}
}