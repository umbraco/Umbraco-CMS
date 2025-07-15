namespace Umbraco.Cms.Core;

public static partial class Constants
{
    public static class ServerEvents
    {
        public static class EventSource
        {
            public const string Document = "Umbraco:CMS:Document";

            public const string DocumentType = "Umbraco:CMS:DocumentType";

            public const string Media = "Umbraco:CMS:Media";

            public const string MediaType = "Umbraco:CMS:MediaType";

            public const string Member = "Umbraco:CMS:Member";

            public const string MemberType = "Umbraco:CMS:MemberType";

            public const string MemberGroup = "Umbraco:CMS:MemberGroup";

            public const string DataType = "Umbraco:CMS:DataType";

            public const string Language = "Umbraco:CMS:Language";

            public const string Script = "Umbraco:CMS:Script";

            public const string Stylesheet = "Umbraco:CMS:Stylesheet";

            public const string Template = "Umbraco:CMS:Template";

            public const string DictionaryItem = "Umbraco:CMS:DictionaryItem";

            public const string Domain = "Umbraco:CMS:Domain";

            public const string PartialView = "Umbraco:CMS:PartialView";

            public const string PublicAccessEntry = "Umbraco:CMS:PublicAccessEntry";

            public const string Relation = "Umbraco:CMS:Relation";

            public const string RelationType = "Umbraco:CMS:RelationType";

            public const string UserGroup = "Umbraco:CMS:UserGroup";

            public const string User = "Umbraco:CMS:User";

            public const string CurrentUser = "Umbraco:CMS:CurrentUser";

            public const string Webhook = "Umbraco:CMS:Webhook";
        }

        public static class EventType
        {
            public static string Created = "Created";

            public static string Updated = "Updated";

            public static string Deleted = "Deleted";

            public static string Trashed = "Trashed";
        }
    }
}
