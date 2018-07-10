using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
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
    }
}
