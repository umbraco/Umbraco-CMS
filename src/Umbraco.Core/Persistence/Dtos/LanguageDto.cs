using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.Language)]
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
        [Length(10)]
        public string IsoCode { get; set; }

        [Column("languageCultureName")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(100)]
        public string CultureName { get; set; }

        /// <summary>
        /// Defines if this language is the default variant language when language variants are in use
        /// </summary>
        [Column("isDefaultVariantLang")]
        [Constraint(Default = "0")]
        public bool IsDefaultVariantLanguage { get; set; }

        /// <summary>
        /// If true, a variant node cannot be published unless this language variant is created
        /// </summary>
        [Column("mandatory")]
        [Constraint(Default = "0")]
        public bool Mandatory { get; set; }
    }
}
