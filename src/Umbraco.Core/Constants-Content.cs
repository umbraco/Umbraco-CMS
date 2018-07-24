namespace Umbraco.Core
{
    public static partial class Constants
    {
        /// <summary>
        /// Defines content retrieval related constants
        /// </summary>
        public static class Content
        {
            /// <summary>
            /// Defines core supported content fall-back options when retrieving content property values.
            /// Defined as constants rather than enum to allow solution or package defined fall-back methods.
            /// </summary>
            public static class ValueFallback
            {
                /// <summary>
                /// No fallback at all.
                /// </summary>
                public const int None = -1;

                /// <summary>
                /// Default fallback.
                /// </summary>
                public const int Default = 0;

                /// <summary>
                /// Recurse up the tree.
                /// </summary>
                public const int Recurse = 1;

                /// <summary>
                /// Fallback to other languages.
                /// </summary>
                public const int Language = 2;
            }
        }
    }
}
