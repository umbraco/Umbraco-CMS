// ReSharper disable once CheckNamespace
namespace Umbraco.Core
{
    public static partial class Constants
    {
        public static class DatabaseSchema
        {
            public const string TableNamePrefix = "umbraco";

            public static class Tables
            {
                public const string Lock = /*TableNamePrefix*/ "umbraco" + "Lock";
                public const string Log = /*TableNamePrefix*/ "umbraco" + "Log";

                public const string Node = /*TableNamePrefix*/ "umbraco" + "Node";
                public const string NodeData = /*TableNamePrefix*/ "cms" + "ContentNu";
                public const string NodeXml = /*TableNamePrefix*/ "cms" + "ContentXml";
                public const string NodePreviewXml = /*TableNamePrefix*/ "cms" + "PreviewXml"; // fixme dbfix kill merge with ContentXml

                public const string ContentType = /*TableNamePrefix*/ "cms" + "ContentType"; // fixme dbfix rename and split uElementType, uDocumentType
                public const string ContentChildType = /*TableNamePrefix*/ "cms" + "ContentTypeAllowedContentType";
                public const string DocumentType = /*TableNamePrefix*/ "cms" + "DocumentType"; // fixme dbfix must rename corresponding DTO
                public const string ElementTypeTree = /*TableNamePrefix*/ "cms" + "ContentType2ContentType"; // fixme dbfix why can't we just use uNode for this?
                public const string DataType = TableNamePrefix + "DataType";
                public const string Template = /*TableNamePrefix*/ "cms" + "Template";

                public const string Content = TableNamePrefix + "Content";
                public const string ContentVersion = TableNamePrefix + "ContentVersion";
                public const string ContentVersionCultureVariation = TableNamePrefix + "ContentVersionCultureVariation";
                public const string Document = TableNamePrefix + "Document";
                public const string DocumentCultureVariation = TableNamePrefix + "DocumentCultureVariation";
                public const string DocumentVersion = TableNamePrefix + "DocumentVersion";
                public const string MediaVersion = TableNamePrefix + "MediaVersion";
                public const string ContentSchedule = TableNamePrefix + "ContentSchedule";

                public const string PropertyType = /*TableNamePrefix*/ "cms" + "PropertyType";
                public const string PropertyTypeGroup = /*TableNamePrefix*/ "cms" + "PropertyTypeGroup";
                public const string PropertyData = TableNamePrefix + "PropertyData";

                public const string RelationType = /*TableNamePrefix*/ "umbraco" + "RelationType";
                public const string Relation = /*TableNamePrefix*/ "umbraco" + "Relation";

                public const string Domain = /*TableNamePrefix*/ "umbraco" + "Domains";
                public const string Language = /*TableNamePrefix*/ "umbraco" + "Language";
                public const string DictionaryEntry = /*TableNamePrefix*/ "cms" + "Dictionary";
                public const string DictionaryValue = /*TableNamePrefix*/ "cms" + "LanguageText";

                public const string User = /*TableNamePrefix*/ "umbraco" + "User";
                public const string UserGroup = /*TableNamePrefix*/ "umbraco" + "UserGroup";
                public const string UserStartNode = /*TableNamePrefix*/ "umbraco" + "UserStartNode";
                public const string User2UserGroup = /*TableNamePrefix*/ "umbraco" + "User2UserGroup";
                public const string User2NodeNotify = /*TableNamePrefix*/ "umbraco" + "User2NodeNotify";
                public const string UserGroup2App = /*TableNamePrefix*/ "umbraco" + "UserGroup2App";
                public const string UserGroup2NodePermission = /*TableNamePrefix*/ "umbraco" + "UserGroup2NodePermission";
                public const string ExternalLogin = /*TableNamePrefix*/ "umbraco" + "ExternalLogin";

                public const string Macro = /*TableNamePrefix*/ "cms" + "Macro";
                public const string MacroProperty = /*TableNamePrefix*/ "cms" + "MacroProperty";

                public const string Member = /*TableNamePrefix*/ "cms" + "Member";
                public const string MemberType = /*TableNamePrefix*/ "cms" + "MemberType";
                public const string Member2MemberGroup = /*TableNamePrefix*/ "cms" + "Member2MemberGroup";

                public const string Access = /*TableNamePrefix*/ "umbraco" + "Access";
                public const string AccessRule = /*TableNamePrefix*/ "umbraco" + "AccessRule";
                public const string RedirectUrl = /*TableNamePrefix*/ "umbraco" + "RedirectUrl";

                public const string CacheInstruction = /*TableNamePrefix*/ "umbraco" + "CacheInstruction";
                public const string Server = /*TableNamePrefix*/ "umbraco" + "Server";

                public const string Tag = /*TableNamePrefix*/ "cms" + "Tags";
                public const string TagRelationship = /*TableNamePrefix*/ "cms" + "TagRelationship";
                
                public const string KeyValue = TableNamePrefix + "KeyValue";

                public const string AuditEntry = /*TableNamePrefix*/ "umbraco" + "Audit";
                public const string Consent = /*TableNamePrefix*/ "umbraco" + "Consent";
                public const string UserLogin = /*TableNamePrefix*/ "umbraco" + "UserLogin";
            }
        }
    }
}
