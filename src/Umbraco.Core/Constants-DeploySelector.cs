namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains the valid selector values.
    /// </summary>
    public static class DeploySelector
    {
        public const string This = "this";
        public const string ThisAndChildren = "this-and-children";
        public const string ThisAndDescendants = "this-and-descendants";
        public const string ChildrenOfThis = "children";
        public const string DescendantsOfThis = "descendants";
    }
}
