using System;
using System.ComponentModel;

namespace Umbraco.Core
{
    public static partial class Constants
    {
        
        public static class UmbracoIndexes
        {
            public const string InternalIndexName = InternalIndexPath + "Indexer";
            public const string ExternalIndexName = ExternalIndexPath + "Indexer";
            public const string MembersIndexName = MembersIndexPath + "Indexer";

            public const string InternalIndexPath = "Internal";
            public const string ExternalIndexPath = "External";
            public const string MembersIndexPath = "Members";

        }
    }
}
