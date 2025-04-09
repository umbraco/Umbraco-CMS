namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Defines reference types.
    /// </summary>
    /// <remarks>
    ///     Reference types are used to identify the type of entity that is being referenced when exposing references
    ///     between Umbraco entities.
    ///     These are used in the management API and backoffice to indicate and warn editors when working with an entity,
    ///     as to what other entities depend on it.
    ///     These consist of references managed by Umbraco relations (e.g. document, media and member).
    ///     But also references that come from schema (e.g. data type usage on content types).
    /// </remarks>
    public static class ReferenceType
    {
        public const string Document = UdiEntityType.Document;
        public const string Media = UdiEntityType.Media;
        public const string Member = UdiEntityType.Member;
        public const string DocumentTypePropertyType = "document-type-property-type";
        public const string MediaTypePropertyType = "media-type-property-type";
        public const string MemberTypePropertyType = "member-type-property-type";
    }
}
