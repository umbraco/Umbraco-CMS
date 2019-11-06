namespace Umbraco.Core.Models.Entities
{
    public class MemberEntitySlim : EntitySlim, IMemberEntitySlim
    {
        public string ContentTypeAlias { get; set; }

        /// <inheritdoc />
        public string ContentTypeIcon { get; set; }

        /// <inheritdoc />
        public string ContentTypeThumbnail { get; set; }
    }
}