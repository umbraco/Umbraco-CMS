using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_17_0;

public class AddPropertyTypeGroupColumns : MigrationBase
{
    private readonly IShortStringHelper _shortStringHelper;

    public AddPropertyTypeGroupColumns(IMigrationContext context, IShortStringHelper shortStringHelper)
        : base(context) => _shortStringHelper = shortStringHelper;

    internal IEnumerable<PropertyTypeGroupDto> PopulateAliases(IEnumerable<PropertyTypeGroupDto> dtos)
    {
        foreach (IGrouping<string?, PropertyTypeGroupDto> dtosPerAlias in dtos.GroupBy(x =>
                     x.Text?.ToSafeAlias(_shortStringHelper, true)))
        {
            IEnumerable<IGrouping<string?, PropertyTypeGroupDto>> dtosPerAliasAndText =
                dtosPerAlias.GroupBy(x => x.Text);
            var numberSuffix = 1;
            foreach (IGrouping<string?, PropertyTypeGroupDto> dtosPerText in dtosPerAliasAndText)
            {
                foreach (PropertyTypeGroupDto dto in dtosPerText)
                {
                    dto.Alias = dtosPerAlias.Key ?? string.Empty;

                    if (numberSuffix > 1)
                    {
                        // More than 1 name found for the alias, so add a suffix
                        dto.Alias += numberSuffix;
                    }

                    yield return dto;
                }

                numberSuffix++;
            }

            if (numberSuffix > 2)
            {
                Logger.LogError(
                    "Detected the same alias {Alias} for different property group names {Names}, the migration added suffixes, but this might break backwards compatibility.",
                    dtosPerAlias.Key, dtosPerAliasAndText.Select(x => x.Key));
            }
        }
    }

    protected override void Migrate()
    {
        AddColumn<PropertyTypeGroupDto>("type");

        // Add column without constraints
        AddColumn<PropertyTypeGroupDto>("alias", out IEnumerable<string> sqls);

        // Populate non-null alias column
        List<PropertyTypeGroupDto>? dtos = Database.Fetch<PropertyTypeGroupDto>();
        foreach (PropertyTypeGroupDto dto in PopulateAliases(dtos))
        {
            Database.Update(dto, x => new { x.Alias });
        }

        // Finally add the constraints
        foreach (var sql in sqls)
        {
            Database.Execute(sql);
        }
    }
}
