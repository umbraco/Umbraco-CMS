using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.DataTypes;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class DataTypeMigration : MigrationBase
{
    private static readonly ISet<string> _legacyAliases = new HashSet<string>
    {
        Constants.PropertyEditors.Legacy.Aliases.Date,
        Constants.PropertyEditors.Legacy.Aliases.Textbox,
        Constants.PropertyEditors.Legacy.Aliases.ContentPicker2,
        Constants.PropertyEditors.Legacy.Aliases.MediaPicker2,
        Constants.PropertyEditors.Legacy.Aliases.MemberPicker2,
        Constants.PropertyEditors.Legacy.Aliases.RelatedLinks2,
        Constants.PropertyEditors.Legacy.Aliases.TextboxMultiple,
        Constants.PropertyEditors.Legacy.Aliases.MultiNodeTreePicker2,
    };

    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;
    private readonly ILogger<DataTypeMigration> _logger;
    private readonly PreValueMigratorCollection _preValueMigrators;
    private readonly PropertyEditorCollection _propertyEditors;

    public DataTypeMigration(
        IMigrationContext context,
        PreValueMigratorCollection preValueMigrators,
        PropertyEditorCollection propertyEditors,
        ILogger<DataTypeMigration> logger,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(context)
    {
        _preValueMigrators = preValueMigrators;
        _propertyEditors = propertyEditors;
        _logger = logger;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
    }

    protected override void Migrate()
    {
        // drop and create columns
        Delete.Column("pk").FromTable("cmsDataType").Do();

        // rename the table
        Rename.Table("cmsDataType").To(Constants.DatabaseSchema.Tables.DataType).Do();

        // create column
        AddColumn<DataTypeDto>(Constants.DatabaseSchema.Tables.DataType, "config");
        Execute.Sql(Sql().Update<DataTypeDto>(u => u.Set(x => x.Configuration, string.Empty))).Do();

        // renames
        Execute.Sql(Sql()
            .Update<DataTypeDto>(u => u.Set(x => x.EditorAlias, "Umbraco.ColorPicker"))
            .Where<DataTypeDto>(x => x.EditorAlias == "Umbraco.ColorPickerAlias")).Do();

        // from preValues to configuration...
        Sql<ISqlContext> sql = Sql()
            .Select<DataTypeDto>()
            .AndSelect<PreValueDto>(x => x.Id, x => x.Alias, x => x.SortOrder, x => x.Value)
            .From<DataTypeDto>()
            .InnerJoin<PreValueDto>().On<DataTypeDto, PreValueDto>((left, right) => left.NodeId == right.NodeId)
            .OrderBy<DataTypeDto>(x => x.NodeId)
            .AndBy<PreValueDto>(x => x.SortOrder);

        IEnumerable<IGrouping<int, PreValueDto>> dtos = Database.Fetch<PreValueDto>(sql).GroupBy(x => x.NodeId);

        foreach (IGrouping<int, PreValueDto> group in dtos)
        {
            DataTypeDto? dataType = Database.Fetch<DataTypeDto>(Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(x => x.NodeId == group.Key)).First();

            // check for duplicate aliases
            var aliases = group.Select(x => x.Alias).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (aliases.Distinct().Count() != aliases.Length)
            {
                throw new InvalidOperationException(
                    $"Cannot migrate prevalues for datatype id={dataType.NodeId}, editor={dataType.EditorAlias}: duplicate alias.");
            }

            // handle null/empty aliases
            var index = 0;
            var dictionary = group.ToDictionary(x => string.IsNullOrWhiteSpace(x.Alias) ? index++.ToString() : x.Alias);

            // migrate the preValues to configuration
            IPreValueMigrator migrator =
                _preValueMigrators.GetMigrator(dataType.EditorAlias) ?? new DefaultPreValueMigrator();
            var config = migrator.GetConfiguration(dataType.NodeId, dataType.EditorAlias, dictionary);
            var json = _configurationEditorJsonSerializer.Serialize(config);

            // validate - and kill the migration if it fails
            var newAlias = migrator.GetNewAlias(dataType.EditorAlias);
            if (newAlias == null)
            {
                if (!_legacyAliases.Contains(dataType.EditorAlias))
                {
                    _logger.LogWarning(
                        "Skipping validation of configuration for data type {NodeId} : {EditorAlias}."
                        + " Please ensure that the configuration is valid. The site may fail to start and / or load data types and run.",
                        dataType.NodeId, dataType.EditorAlias);
                }
            }
            else if (!_propertyEditors.TryGet(newAlias, out IDataEditor? propertyEditor))
            {
                if (!_legacyAliases.Contains(newAlias))
                {
                    _logger.LogWarning(
                        "Skipping validation of configuration for data type {NodeId} : {NewEditorAlias} (was: {EditorAlias})"
                        + " because no property editor with that alias was found."
                        + " Please ensure that the configuration is valid. The site may fail to start and / or load data types and run.",
                        dataType.NodeId, newAlias, dataType.EditorAlias);
                }
            }
            else
            {
                IConfigurationEditor configEditor = propertyEditor.GetConfigurationEditor();
                try
                {
                    var _ = configEditor.FromDatabase(json, _configurationEditorJsonSerializer);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(
                        e,
                        "Failed to validate configuration for data type {NodeId} : {NewEditorAlias} (was: {EditorAlias})."
                        + " Please fix the configuration and ensure it is valid. The site may fail to start and / or load data types and run.",
                        dataType.NodeId, newAlias, dataType.EditorAlias);
                }
            }

            // update
            dataType.Configuration = _configurationEditorJsonSerializer.Serialize(config);
            Database.Update(dataType);
        }
    }
}
