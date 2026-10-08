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
        /// <summary>
        ///     The reference type for document entities.
        /// </summary>
        public const string Document = UdiEntityType.Document;

        /// <summary>
        ///     The reference type for element entities.
        /// </summary>
        public const string Element = UdiEntityType.Element;

        /// <summary>
        ///     The reference type for element container entities.
        /// </summary>
        public const string ElementContainer = UdiEntityType.ElementContainer;

        /// <summary>
        ///     The reference type for media entities.
        /// </summary>
        public const string Media = UdiEntityType.Media;

        /// <summary>
        ///     The reference type for member entities.
        /// </summary>
        public const string Member = UdiEntityType.Member;

        /// <summary>
        ///     The reference type for document type property type references.
        /// </summary>
        public const string DocumentTypePropertyType = "document-type-property-type";

        /// <summary>
        ///     The reference type for media type property type references.
        /// </summary>
        public const string MediaTypePropertyType = "media-type-property-type";

        /// <summary>
        ///     The reference type for member type property type references.
        /// </summary>
        public const string MemberTypePropertyType = "member-type-property-type";
    }
}
