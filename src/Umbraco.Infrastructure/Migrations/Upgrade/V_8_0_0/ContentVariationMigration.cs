using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class ContentVariationMigration : MigrationBase
    {
        public ContentVariationMigration(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            byte GetNewValue(byte oldValue)
            {
                switch (oldValue)
                {
                    case 0: // Unknown
                    case 1: // InvariantNeutral
                        return 0; // Unknown
                    case 2: // CultureNeutral
                    case 3: // CultureNeutral | InvariantNeutral
                        return 1; // Culture
                    case 4: // InvariantSegment
                    case 5: // InvariantSegment | InvariantNeutral
                        return 2; // Segment
                    case 6: // InvariantSegment | CultureNeutral
                    case 7: // InvariantSegment | CultureNeutral | InvariantNeutral
                    case 8: // CultureSegment
                    case 9: // CultureSegment | InvariantNeutral
                    case 10: // CultureSegment | CultureNeutral
                    case 11: // CultureSegment | CultureNeutral | InvariantNeutral
                    case 12: // etc
                    case 13:
                    case 14:
                    case 15:
                        return 3; // Culture | Segment
                    default:
                        throw new NotSupportedException($"Invalid value {oldValue}.");
                }
            }

            var propertyTypes = Database.Fetch<PropertyTypeDto>(Sql().Select<PropertyTypeDto>().From<PropertyTypeDto>());
            foreach (var dto in propertyTypes)
            {
                dto.Variations = GetNewValue(dto.Variations);
                Database.Update(dto);
            }

            var contentTypes = Database.Fetch<ContentTypeDto>(Sql().Select<ContentTypeDto>().From<ContentTypeDto>());
            foreach (var dto in contentTypes)
            {
                dto.Variations = GetNewValue(dto.Variations);
                Database.Update(dto);
            }
        }

        // we *need* to use this private DTO here, which does *not* have extra properties, which would kill the migration

        [TableName(TableName)]
        [PrimaryKey("pk")]
        [ExplicitColumns]
        private class ContentTypeDto
        {
            public const string TableName = Constants.DatabaseSchema.Tables.ContentType;

            [Column("pk")]
            [PrimaryKeyColumn(IdentitySeed = 535)]
            public int PrimaryKey { get; set; }

            [Column("nodeId")]
            [ForeignKey(typeof(NodeDto))]
            [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsContentType")]
            public int NodeId { get; set; }

            [Column("alias")]
            [NullSetting(NullSetting = NullSettings.Null)]
            public string Alias { get; set; }

            [Column("icon")]
            [Index(IndexTypes.NonClustered)]
            [NullSetting(NullSetting = NullSettings.Null)]
            public string Icon { get; set; }

            [Column("thumbnail")]
            [Constraint(Default = "folder.png")]
            public string Thumbnail { get; set; }

            [Column("description")]
            [NullSetting(NullSetting = NullSettings.Null)]
            [Length(1500)]
            public string Description { get; set; }

            [Column("isContainer")]
            [Constraint(Default = "0")]
            public bool IsContainer { get; set; }

            [Column("allowAtRoot")]
            [Constraint(Default = "0")]
            public bool AllowAtRoot { get; set; }

            [Column("variations")]
            [Constraint(Default = "1" /*ContentVariation.InvariantNeutral*/)]
            public byte Variations { get; set; }

            [ResultColumn]
            public NodeDto NodeDto { get; set; }
        }

    }
}
