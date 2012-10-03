using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoDomains")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class DomainDto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("omainDefaultLanguage")]
        public int? DefaultLanguage { get; set; }

        [Column("domainRootStructureID")]
        public int? RootStructureId { get; set; }

        [Column("domainName")]
        public string DomainName { get; set; }
    }
}