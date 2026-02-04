namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains constants for validation.
    /// </summary>
    public static class Validation
    {
        /// <summary>
        ///     Contains validation error message constants.
        /// </summary>
        public static class ErrorMessages
        {
            /// <summary>
            ///     Contains property validation error message constants.
            /// </summary>
            public static class Properties
            {
                /// <summary>
                ///     The localization key for missing/null value validation error.
                /// </summary>
                public const string Missing = "#validation_invalidNull";

                /// <summary>
                ///     The localization key for empty value validation error.
                /// </summary>
                public const string Empty = "#validation_invalidEmpty";

                /// <summary>
                ///     The localization key for pattern mismatch validation error.
                /// </summary>
                public const string PatternMismatch = "#validation_invalidPattern";
            }
        }
    }
}
