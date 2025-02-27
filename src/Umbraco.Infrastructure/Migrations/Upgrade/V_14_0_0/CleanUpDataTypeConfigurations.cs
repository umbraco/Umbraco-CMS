using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

public class CleanUpDataTypeConfigurations : MigrationBase
{
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;
    private readonly ILogger<MigrateDataTypeConfigurations> _logger;

    public CleanUpDataTypeConfigurations(
        IMigrationContext context,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer,
        ILogger<MigrateDataTypeConfigurations> logger)
        : base(context)
    {
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        _logger = logger;
    }

    protected override void Migrate()
    {
        Sql<ISqlContext> sql = Sql()
            .Select<DataTypeDto>()
            .AndSelect<NodeDto>()
            .From<DataTypeDto>()
            .InnerJoin<NodeDto>()
            .On<DataTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .Where<DataTypeDto>(x => x.EditorAlias.Contains("Umbraco."));

        List<DataTypeDto> dataTypeDtos = Database.Fetch<DataTypeDto>(sql);

        foreach (DataTypeDto dataTypeDto in dataTypeDtos)
        {
            try
            {
                Dictionary<string, object?> configurationData = dataTypeDto.Configuration.IsNullOrWhiteSpace()
                    ? new Dictionary<string, object?>()
                    : _configurationEditorJsonSerializer
                          .Deserialize<Dictionary<string, object?>>(dataTypeDto.Configuration)
                          ?? new Dictionary<string, object?>();

                var configurationDataWithoutNullValues = configurationData
                    .Where(pair => pair.Value is not null)
                    .ToDictionary(pair => pair.Key, pair => pair.Value!);

                if (configurationData.Count == configurationDataWithoutNullValues.Count)
                {
                    continue;
                }

                dataTypeDto.Configuration = _configurationEditorJsonSerializer.Serialize(configurationDataWithoutNullValues);
                Database.Update(dataTypeDto);
                _logger.LogInformation("Configuration cleaned up for data type: {dataTypeName} (id: {dataTypeId}, editor alias: {dataTypeEditorAlias})", dataTypeDto.NodeDto?.Text, dataTypeDto.NodeId, dataTypeDto.EditorAlias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Configuration clean-up failed for data type: {dataTypeName} (id: {dataTypeId}, editor alias: {dataTypeEditorAlias})", dataTypeDto.NodeDto?.Text, dataTypeDto.NodeId, dataTypeDto.EditorAlias);
            }
        }
    }
}
