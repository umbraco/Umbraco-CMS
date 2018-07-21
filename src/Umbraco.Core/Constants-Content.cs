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
            public static class FallbackMethods
            {
                public const int None = 0;
                public const int RecursiveTree = 1;
                public const int FallbackLanguage = 2;
            }
        }
    }
}
