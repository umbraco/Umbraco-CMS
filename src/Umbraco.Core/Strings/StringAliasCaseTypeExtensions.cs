namespace Umbraco.Core.Strings
{
    /// <summary>
    /// Provides extension methods to StringAliasCaseType to facilitate migration to CleanStringType.
    /// </summary>
    public static class StringAliasCaseTypeExtensions
    {
        /// <summary>
        /// Gets the CleanStringType value corresponding to the StringAliasCaseType value.
        /// </summary>
        /// <param name="aliasCaseType">The value.</param>
        /// <returns>A CleanStringType value corresponding to the StringAliasCaseType value.</returns>
        public static CleanStringType ToCleanStringType(this StringAliasCaseType aliasCaseType)
        {
            switch (aliasCaseType)
            {
                case StringAliasCaseType.PascalCase:
                    return CleanStringType.PascalCase;
                case StringAliasCaseType.CamelCase:
                    return CleanStringType.CamelCase;
                //case StringAliasCaseType.Unchanged:
                default:
                    return CleanStringType.Unchanged;
            }
        }
    }
}
