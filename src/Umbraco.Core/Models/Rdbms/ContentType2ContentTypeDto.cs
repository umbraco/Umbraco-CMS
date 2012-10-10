using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsContentType2ContentType")]
    [ExplicitColumns]
    internal class ContentType2ContentTypeDto
    {
        [Column("parentContentTypeId")]
        public int ParentId { get; set; }

        [Column("childContentTypeId")]
        public int ChildId { get; set; }
    }
}