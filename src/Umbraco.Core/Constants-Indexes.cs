using System;
using System.ComponentModel;

namespace Umbraco.Core
{
    public static partial class Constants
    {
        public static class UmbracoIndexes
        {
            public const string InternalIndexName = InternalIndexPath + "Index";
            public const string ExternalIndexName = ExternalIndexPath + "Index";
            public const string MembersIndexName = MembersIndexPath + "Index";

            public const string InternalIndexPath = "Internal";
            public const string ExternalIndexPath = "External";
            public const string MembersIndexPath = "Members";
        }
    }
}
