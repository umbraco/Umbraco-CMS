using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class ContentVariationMigration : MigrationBase
{
    public ContentVariationMigration(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        static byte GetNewValue(byte oldValue)
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

        List<PropertyTypeDto80>? propertyTypes =
            Database.Fetch<PropertyTypeDto80>(Sql().Select<PropertyTypeDto80>().From<PropertyTypeDto80>());
        foreach (PropertyTypeDto80? dto in propertyTypes)
        {
            dto.Variations = GetNewValue(dto.Variations);
            Database.Update(dto);
        }

        List<ContentTypeDto80>? contentTypes =
            Database.Fetch<ContentTypeDto80>(Sql().Select<ContentTypeDto80>().From<ContentTypeDto80>());
        foreach (ContentTypeDto80? dto in contentTypes)
        {
            dto.Variations = GetNewValue(dto.Variations);
            Database.Update(dto);
        }
    }

    // we *need* to use these private DTOs here, which does *not* have extra properties, which would kill the migration
}
