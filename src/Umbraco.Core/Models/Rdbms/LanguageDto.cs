using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoLanguage")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class LanguageDto
    {
        [Column("id")]
        public short Id { get; set; }

        [Column("languageISOCode")]
        public string IsoCode { get; set; }

        [Column("languageCultureName")]
        public string CultureName { get; set; }
    }
}