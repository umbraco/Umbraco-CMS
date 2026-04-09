namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains the valid selector values.
    /// </summary>
    public static class DeploySelector
    {
        /// <summary>
        ///     Selector value for selecting only the current item.
        /// </summary>
        public const string This = "this";

        /// <summary>
        ///     Selector value for selecting the current item and its direct children.
        /// </summary>
        public const string ThisAndChildren = "this-and-children";

        /// <summary>
        ///     Selector value for selecting the current item and all its descendants.
        /// </summary>
        public const string ThisAndDescendants = "this-and-descendants";

        /// <summary>
        ///     Selector value for selecting only the direct children of the current item.
        /// </summary>
        public const string ChildrenOfThis = "children";

        /// <summary>
        ///     Selector value for selecting only the descendants of the current item.
        /// </summary>
        public const string DescendantsOfThis = "descendants";

        /// <summary>
        ///     Selector value for selecting all entities of a specific type.
        /// </summary>
        public const string EntitiesOfType = "entities-of-type";
    }
}
