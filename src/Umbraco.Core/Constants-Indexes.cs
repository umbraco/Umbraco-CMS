namespace Umbraco.Cms.Core
{
    public static partial class Constants
    {
        public static class UmbracoIndexes
        {
            public const string InternalIndexName = InternalIndexPath + "Index";
            public const string ExternalIndexName = ExternalIndexPath + "Index";
            public const string MembersIndexName = MembersIndexPath + "Index";

            private const string InternalIndexPath = "Internal";
            private const string ExternalIndexPath = "External";
            private const string MembersIndexPath = "Members";
        }
    }
}
