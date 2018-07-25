using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoLanguage")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class LanguageDto
    {
        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 2)]
        public short Id { get; set; }

        [Column("languageISOCode")]
        [Index(IndexTypes.UniqueNonClustered)]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(14)]
        public string IsoCode { get; set; }

        [Column("languageCultureName")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(100)]
        public string CultureName { get; set; }
    }
}
