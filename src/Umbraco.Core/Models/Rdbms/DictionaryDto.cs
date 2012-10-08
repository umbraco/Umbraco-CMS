using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsDictionary")]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    internal class DictionaryDto
    {
        [Column("pk")]
        public int PrimaryKey { get; set; }

        [Column("id")]
        public Guid Id { get; set; }

        [Column("parent")]
        public Guid Parent { get; set; }

        [Column("key")]
        public string Key { get; set; }

        [ResultColumn]
        public List<LanguageTextDto> LanguageTextDtos { get; set; }
    }
}