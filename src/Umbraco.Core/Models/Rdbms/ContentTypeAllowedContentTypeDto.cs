using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsContentTypeAllowedContentType")]
    [PrimaryKey("Id", autoIncrement = false)]
    [ExplicitColumns]
    internal class ContentTypeAllowedContentTypeDto
    {
        [Column("Id")]
        public int Id { get; set; }

        [Column("AllowedId")]
        public int AllowedId { get; set; }
    }
}