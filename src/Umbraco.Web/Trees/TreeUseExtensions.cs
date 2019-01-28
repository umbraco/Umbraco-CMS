namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Provides extension methods to the <see cref="TreeUse"/> method.
    /// </summary>
    public static class TreeUseExtensions
    {
        /// <summary>
        /// Determines whether a TreeUse has all the specified values.
        /// </summary>
        public static bool Has(this TreeUse use, TreeUse uses)
        {
            return (use & uses) == uses;
        }

        /// <summary>
        /// Determines whether a TreeUse has any of the specified values.
        /// </summary>
        public static bool HasAny(this TreeUse use, TreeUse uses)
        {
            return (use & uses) > 0;
        }
    }
}