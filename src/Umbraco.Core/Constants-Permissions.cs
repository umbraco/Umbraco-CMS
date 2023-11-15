namespace Umbraco.Cms.Core;

// todo permissions: xml comments
public static partial class Constants
{
    /// <summary>
    ///     Defines the permission (verbs) used in Umbraco
    /// </summary>
    public static class Permissions
    {
        public const string Assign = "assign";
        public const string Browse = "browse";
        public const string Copy = "copy";
    }

    public static class PermissionContexts
    {
        public const string Domain = "domain";
        public const string Content = "content";
    }
}
