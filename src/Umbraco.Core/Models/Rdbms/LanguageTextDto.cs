using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsLanguageText")]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    internal class LanguageTextDto
    {
        [Column("pk")]
        public int PrimaryKey { get; set; }

        [Column("languageId")]
        public int LanguageId { get; set; }

        [Column("UniqueId")]
        public Guid UniqueId { get; set; }

        [Column("value")]
        public string Value { get; set; }
    }
}