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
        public const string Unknown = "unknown";

        // GUID entity types
        public const string AnyGuid = "any-guid"; // that one is for tests
        public const string DataType = "data-type";
        public const string DataTypeContainer = "data-type-container";
        public const string DictionaryItem = "dictionary-item";
        public const string Document = "document";
        public const string DocumentBlueprint = "document-blueprint";
        public const string DocumentBlueprintContainer = "document-blueprint-container";
        public const string DocumentType = "document-type";
        public const string DocumentTypeContainer = "document-type-container";
        public const string Element = "element";
        public const string Media = "media";
        public const string MediaType = "media-type";
        public const string MediaTypeContainer = "media-type-container";
        public const string Member = "member";
        public const string MemberGroup = "member-group";
        public const string MemberType = "member-type";
        public const string Relation = "relation";
        public const string RelationType = "relation-type";
        public const string Template = "template";
        public const string User = "user";
        public const string UserGroup = "user-group";
        public const string Webhook = "webhook";

        // String entity types
        public const string AnyString = "any-string"; // that one is for tests
        public const string Language = "language";
        public const string MediaFile = "media-file";
        public const string PartialView = "partial-view";
        public const string Script = "script";
        public const string Stylesheet = "stylesheet";
        public const string TemplateFile = "template-file";

        // Forms entity types
        public const string FormsDataSource = "forms-datasource";
        public const string FormsForm = "forms-form";
        public const string FormsPreValue = "forms-prevalue";
    }
}
