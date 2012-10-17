using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsContentTypeAllowedContentType")]
    [PrimaryKey("Id", autoIncrement = false)]
    [ExplicitColumns]
    internal class ContentTypeAllowedContentTypeDto
    {
        [Column("Id")]
        [ForeignKey(typeof(ContentTypeDto), Name = "FK_cmsContentTypeAllowedContentType_cmsContentType")]
        [PrimaryKeyColumn(AutoIncrement = false, Clustered = true, Name = "PK_cmsContentTypeAllowedContentType", OnColumns = "[Id], [AllowedId]")]
        public int Id { get; set; }

        [Column("AllowedId")]
        [ForeignKey(typeof(ContentTypeDto), Name = "FK_cmsContentTypeAllowedContentType_cmsContentType1")]
        public int AllowedId { get; set; }

        [Column("SortOrder")]
        [Constraint(Name = "df_cmsContentTypeAllowedContentType_sortOrder", Default = "0")]
        public int SortOrder { get; set; }
    }
}