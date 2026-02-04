namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Defines well-known entity types.
    /// </summary>
    /// <remarks>
    ///     Well-known entity types are those that Deploy already knows about,
    ///     but entity types are strings and so can be extended beyond what is defined here.
    /// </remarks>
    public static class UdiEntityType
    {
        // note: const fields in this class MUST be consistent with what GetTypes returns
        //  this is validated by UdiTests.ValidateUdiEntityType
        // also, this is used exclusively in Udi static ctor, only once, so there is no
        //  need to keep it around in a field nor to make it readonly

        /// <summary>
        ///     The entity type for unknown entities.
        /// </summary>
        public const string Unknown = "unknown";

        // GUID entity types

        /// <summary>
        ///     The entity type for any GUID-based entity (used for tests).
        /// </summary>
        public const string AnyGuid = "any-guid"; // that one is for tests

        /// <summary>
        ///     The entity type for dictionary items.
        /// </summary>
        public const string DictionaryItem = "dictionary-item";

        /// <summary>
        ///     The entity type for documents (content items).
        /// </summary>
        public const string Document = "document";

        /// <summary>
        ///     The entity type for document blueprints (content templates).
        /// </summary>
        public const string DocumentBlueprint = "document-blueprint";

        /// <summary>
        ///     The entity type for document blueprint containers (folders).
        /// </summary>
        public const string DocumentBlueprintContainer = "document-blueprint-container";

        /// <summary>
        ///     The entity type for document types (content types).
        /// </summary>
        public const string DocumentType = "document-type";

        /// <summary>
        ///     The entity type for document type containers (folders).
        /// </summary>
        public const string DocumentTypeContainer = "document-type-container";

        /// <summary>
        ///     The entity type for member types.
        /// </summary>
        public const string MemberType = "member-type";

        /// <summary>
        ///     The entity type for member type containers (folders).
        /// </summary>
        public const string MemberTypeContainer = "member-type-container";

        /// <summary>
        ///     The entity type for member groups.
        /// </summary>
        public const string MemberGroup = "member-group";

        /// <summary>
        ///     The entity type for members.
        /// </summary>
        public const string Member = "member";

        /// <summary>
        ///     The entity type for data types.
        /// </summary>
        public const string DataType = "data-type";

        /// <summary>
        ///     The entity type for data type containers (folders).
        /// </summary>
        public const string DataTypeContainer = "data-type-container";

        /// <summary>
        ///     The entity type for elements (element type instances).
        /// </summary>
        public const string Element = "element";

        /// <summary>
        ///     The entity type for element type containers (folders).
        /// </summary
        public const string ElementContainer = "element-container";

        /// <summary>
        ///     The entity type for media items.
        /// </summary>
        public const string Media = "media";

        /// <summary>
        ///     The entity type for media types.
        /// </summary>
        public const string MediaType = "media-type";

        /// <summary>
        ///     The entity type for media type containers (folders).
        /// </summary>
        public const string MediaTypeContainer = "media-type-container";

        /// <summary>
        ///     The entity type for relations.
        /// </summary>
        public const string Relation = "relation";

        /// <summary>
        ///     The entity type for relation types.
        /// </summary>
        public const string RelationType = "relation-type";

        /// <summary>
        ///     The entity type for templates.
        /// </summary>
        public const string Template = "template";

        /// <summary>
        ///     The entity type for users.
        /// </summary>
        public const string User = "user";

        /// <summary>
        ///     The entity type for user groups.
        /// </summary>
        public const string UserGroup = "user-group";

        /// <summary>
        ///     The entity type for webhooks.
        /// </summary>
        public const string Webhook = "webhook";

        // String entity types

        /// <summary>
        ///     The entity type for any string-based entity (used for tests).
        /// </summary>
        public const string AnyString = "any-string"; // that one is for tests

        /// <summary>
        ///     The entity type for languages.
        /// </summary>
        public const string Language = "language";

        /// <summary>
        ///     The entity type for media files.
        /// </summary>
        public const string MediaFile = "media-file";

        /// <summary>
        ///     The entity type for partial views.
        /// </summary>
        public const string PartialView = "partial-view";

        /// <summary>
        ///     The entity type for scripts.
        /// </summary>
        public const string Script = "script";

        /// <summary>
        ///     The entity type for stylesheets.
        /// </summary>
        public const string Stylesheet = "stylesheet";

        /// <summary>
        ///     The entity type for template files.
        /// </summary>
        public const string TemplateFile = "template-file";

        // Forms entity types

        /// <summary>
        ///     The entity type for Umbraco Forms data sources.
        /// </summary>
        public const string FormsDataSource = "forms-datasource";

        /// <summary>
        ///     The entity type for Umbraco Forms forms.
        /// </summary>
        public const string FormsForm = "forms-form";

        /// <summary>
        ///     The entity type for Umbraco Forms pre-values.
        /// </summary>
        public const string FormsPreValue = "forms-prevalue";
    }
}
