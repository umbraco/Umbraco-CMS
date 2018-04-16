namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// The type of published element.
    /// </summary>
    /// <remarks>Can be a simple element, or a document, a media, a member.</remarks>
    public enum PublishedItemType // fixme - need to rename to PublishedElementType but then conflicts?
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A document.
        /// </summary>
        Content,

        /// <summary>
        /// A media.
        /// </summary>
        Media,

        /// <summary>
        /// A member.
        /// </summary>
        Member
    }
}
