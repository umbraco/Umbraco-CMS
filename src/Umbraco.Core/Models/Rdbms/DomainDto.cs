using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoDomains")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class DomainDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("domainDefaultLanguage")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? DefaultLanguage { get; set; }

        [Column("domainRootStructureID")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? RootStructureId { get; set; }

        [Column("domainName")]
        public string DomainName { get; set; }
    }
}