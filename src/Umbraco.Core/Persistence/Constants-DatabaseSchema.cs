// ReSharper disable once CheckNamespace

namespace Umbraco.Cms.Core;

public static partial class Constants
{
    public static class DatabaseSchema
    {
        // TODO: Why aren't all table names with the same prefix?
        public const string TableNamePrefix = "umbraco";

        public static class Tables
        {
            public const string Lock = TableNamePrefix + "Lock";
            public const string Log = TableNamePrefix + "Log";

            public const string Node = TableNamePrefix + "Node";
            public const string NodeData = /*TableNamePrefix*/ "cms" + "ContentNu";

            public const string ContentType = /*TableNamePrefix*/ "cms" + "ContentType";
            public const string ContentChildType = /*TableNamePrefix*/ "cms" + "ContentTypeAllowedContentType";
            public const string DocumentType = /*TableNamePrefix*/ "cms" + "DocumentType";
            public const string ElementTypeTree = /*TableNamePrefix*/ "cms" + "ContentType2ContentType";
            public const string DataType = TableNamePrefix + "DataType";
            public const string Template = /*TableNamePrefix*/ "cms" + "Template";

            public const string Content = TableNamePrefix + "Content";
            public const string ContentVersion = TableNamePrefix + "ContentVersion";
            public const string ContentVersionCultureVariation = TableNamePrefix + "ContentVersionCultureVariation";
            public const string ContentVersionCleanupPolicy = TableNamePrefix + "ContentVersionCleanupPolicy";

            public const string Document = TableNamePrefix + "Document";
            public const string DocumentCultureVariation = TableNamePrefix + "DocumentCultureVariation";
            public const string DocumentVersion = TableNamePrefix + "DocumentVersion";
            public const string MediaVersion = TableNamePrefix + "MediaVersion";
            public const string ContentSchedule = TableNamePrefix + "ContentSchedule";

            public const string PropertyType = /*TableNamePrefix*/ "cms" + "PropertyType";
            public const string PropertyTypeGroup = /*TableNamePrefix*/ "cms" + "PropertyTypeGroup";
            public const string PropertyData = TableNamePrefix + "PropertyData";

            public const string RelationType = TableNamePrefix + "RelationType";
            public const string Relation = TableNamePrefix + "Relation";

            public const string Domain = TableNamePrefix + "Domain";
            public const string Language = TableNamePrefix + "Language";
            public const string DictionaryEntry = /*TableNamePrefix*/ "cms" + "Dictionary";
            public const string DictionaryValue = /*TableNamePrefix*/ "cms" + "LanguageText";

            public const string User = TableNamePrefix + "User";
            public const string UserGroup = TableNamePrefix + "UserGroup";
            public const string UserStartNode = TableNamePrefix + "UserStartNode";
            public const string User2UserGroup = TableNamePrefix + "User2UserGroup";
            public const string User2NodeNotify = TableNamePrefix + "User2NodeNotify";
            public const string UserGroup2App = TableNamePrefix + "UserGroup2App";
            public const string UserGroup2Node = TableNamePrefix + "UserGroup2Node";
            public const string UserGroup2NodePermission = TableNamePrefix + "UserGroup2NodePermission";
            public const string UserGroup2Language = TableNamePrefix + "UserGroup2Language";
            public const string ExternalLogin = TableNamePrefix + "ExternalLogin";
            public const string TwoFactorLogin = TableNamePrefix + "TwoFactorLogin";
            public const string ExternalLoginToken = TableNamePrefix + "ExternalLoginToken";

            public const string Macro = /*TableNamePrefix*/ "cms" + "Macro";
            public const string MacroProperty = /*TableNamePrefix*/ "cms" + "MacroProperty";

            public const string Member = /*TableNamePrefix*/ "cms" + "Member";
            public const string MemberPropertyType = /*TableNamePrefix*/ "cms" + "MemberType";
            public const string Member2MemberGroup = /*TableNamePrefix*/ "cms" + "Member2MemberGroup";

            public const string Access = TableNamePrefix + "Access";
            public const string AccessRule = TableNamePrefix + "AccessRule";
            public const string RedirectUrl = TableNamePrefix + "RedirectUrl";

            public const string CacheInstruction = TableNamePrefix + "CacheInstruction";
            public const string Server = TableNamePrefix + "Server";

            public const string Tag = /*TableNamePrefix*/ "cms" + "Tags";
            public const string TagRelationship = /*TableNamePrefix*/ "cms" + "TagRelationship";

            public const string KeyValue = TableNamePrefix + "KeyValue";

            public const string AuditEntry = TableNamePrefix + "Audit";
            public const string Consent = TableNamePrefix + "Consent";
            public const string UserLogin = TableNamePrefix + "UserLogin";

            public const string LogViewerQuery = TableNamePrefix + "LogViewerQuery";

            public const string CreatedPackageSchema = TableNamePrefix + "CreatedPackageSchema";
        }
    }
}
