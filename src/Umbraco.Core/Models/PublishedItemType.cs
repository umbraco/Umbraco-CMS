namespace Umbraco.Core.Models
{
    /// <summary>
	/// The type of published content, ie whether it is a content or a media.
	/// </summary>
	public enum PublishedItemType
	{
        /// <summary>
        /// A content, ie what was formerly known as a document.
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