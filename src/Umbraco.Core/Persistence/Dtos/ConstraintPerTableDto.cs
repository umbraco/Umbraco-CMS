﻿using NPoco;

namespace Umbraco.Core.Persistence.Dtos
{
    public class ConstraintPerTableDto
    {
        [Column("TABLE_NAME")]
        public string TableName { get; set; }

        [Column("CONSTRAINT_NAME")]
        public string ConstraintName { get; set; }
    }
}
