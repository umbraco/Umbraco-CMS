namespace Umbraco.Cms.Core.Models.Membership
{
    /// <summary>
    ///     Represents the model for exporting member data.
    /// </summary>
    public class MemberExportModel
    {
        /// <summary>
        ///     Gets or sets the unique identifier for the member.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Gets or sets the unique key for the member.
        /// </summary>
        public Guid Key { get; set; }

        /// <summary>
        ///     Gets or sets the name of the member.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        ///     Gets or sets the username of the member.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        ///     Gets or sets the email address of the member.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        ///     Gets or sets the list of group names the member belongs to.
        /// </summary>
        public List<string> Groups { get; set; } = new ();

        /// <summary>
        ///     Gets or sets the content type alias of the member.
        /// </summary>
        public string? ContentTypeAlias { get; set; }

        /// <summary>
        ///     Gets or sets the date and time when the member was created.
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        ///     Gets or sets the date and time when the member was last updated.
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        ///     Gets or sets the list of custom properties for the member.
        /// </summary>
        public List<MemberExportProperty> Properties { get; set; } = new ();
    }
}
