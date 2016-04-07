using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoDeployChecksum")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class UmbracoDeployChecksumDto
    {
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_umbracoDeployChecksum")]
        public int Id { get; set; }

        [Column("entityType")]
        [Length(32)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoDeployChecksum", ForColumns = "entityType,entityGuid,entityPath")]
        public string EntityType { get; set; }

        [Column("entityGuid")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public Guid EntityGuid { get; set; }

        [Column("entityPath")]
        [Length(256)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string EntityPath { get; set; }

        [Column("localChecksum")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Length(32)]
        public string LocalChecksum { get; set; }

        [Column("compositeChecksum")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(32)]
        public string CompositeChecksum { get; set; }
    }
}