using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoLock")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class LockDto
    {
        public LockDto()
        {
            Value = 1;
        }

        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_umbracoLock")]
        public int Id { get; set; }

        [Column("value")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int Value { get; set; }

        [Column("name")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Length(64)]
        public string Name { get; set; }
    }
}