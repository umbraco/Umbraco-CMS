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

        // guid entity types
        public const string AnyGuid = "any-guid"; // that one is for tests

        public const string Element = "element";
        public const string Document = "document";

        public const string DocumentBlueprint = "document-blueprint";

        public const string Media = "media";
        public const string Member = "member";

        public const string DictionaryItem = "dictionary-item";
        public const string Macro = "macro";
        public const string Template = "template";

        public const string DocumentType = "document-type";
        public const string DocumentTypeContainer = "document-type-container";

        // TODO: What is this? This alias is only used for the blue print tree to render the blueprint's document type, it's not a real udi type
        public const string DocumentTypeBluePrints = "document-type-blueprints";
        public const string MediaType = "media-type";
        public const string MediaTypeContainer = "media-type-container";
        public const string DataType = "data-type";
        public const string DataTypeContainer = "data-type-container";
        public const string MemberType = "member-type";
        public const string MemberGroup = "member-group";

        public const string RelationType = "relation-type";

        // forms
        public const string FormsForm = "forms-form";
        public const string FormsPreValue = "forms-prevalue";
        public const string FormsDataSource = "forms-datasource";

        // string entity types
        public const string AnyString = "any-string"; // that one is for tests

        public const string Language = "language";
        public const string MacroScript = "macroscript";
        public const string MediaFile = "media-file";
        public const string TemplateFile = "template-file";
        public const string Script = "script";
        public const string Stylesheet = "stylesheet";
        public const string PartialView = "partial-view";
        public const string PartialViewMacro = "partial-view-macro";
    }
}
