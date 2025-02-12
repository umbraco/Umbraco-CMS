using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_2_0;

public class AddMissingDateTimeConfiguration : MigrationBase
{
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    public AddMissingDateTimeConfiguration(IMigrationContext context, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(context)
        => _configurationEditorJsonSerializer = configurationEditorJsonSerializer;

    protected override void Migrate()
    {
        Sql<ISqlContext> sql = Sql()
            .Select<DataTypeDto>()
            .From<DataTypeDto>()
            .Where<DataTypeDto>(dto =>
                dto.NodeId == Constants.DataTypes.DateTime
                && dto.EditorAlias.Equals(Constants.PropertyEditors.Aliases.DateTime));

        DataTypeDto? dataTypeDto = Database.FirstOrDefault<DataTypeDto>(sql);
        if (dataTypeDto is null)
        {
            return;
        }

        Dictionary<string, object> configurationData = dataTypeDto.Configuration.IsNullOrWhiteSpace()
            ? new Dictionary<string, object>()
            : _configurationEditorJsonSerializer
                  .Deserialize<Dictionary<string, object?>>(dataTypeDto.Configuration)?
                  .Where(item => item.Value is not null)
                  .ToDictionary(item => item.Key, item => item.Value!)
              ?? new Dictionary<string, object>();

        // only proceed with the migration if the data-type has no format assigned
        if (configurationData.TryAdd("format", "YYYY-MM-DD HH:mm:ss") is false)
        {
            return;
        }

        dataTypeDto.Configuration = _configurationEditorJsonSerializer.Serialize(configurationData);
        Database.Update(dataTypeDto);
    }
}
