using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{

    public class DataTypeMigration : MigrationBase
    {
        private readonly PreValueMigratorCollection _preValueMigrators;
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly ILogger _logger;

        private static readonly ISet<string> LegacyAliases = new HashSet<string>()
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

        public DataTypeMigration(IMigrationContext context, PreValueMigratorCollection preValueMigrators, PropertyEditorCollection propertyEditors, ILogger logger)
            : base(context)
        {
            _preValueMigrators = preValueMigrators;
            _propertyEditors = propertyEditors;
            _logger = logger;
        }

        public override void Migrate()
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
            var sql = Sql()
                .Select<DataTypeDto>()
                .AndSelect<PreValueDto>(x => x.Id, x => x.Alias, x => x.SortOrder, x => x.Value)
                .From<DataTypeDto>()
                .InnerJoin<PreValueDto>().On<DataTypeDto, PreValueDto>((left, right) => left.NodeId == right.NodeId)
                .OrderBy<DataTypeDto>(x => x.NodeId)
                .AndBy<PreValueDto>(x => x.SortOrder);

            var dtos = Database.Fetch<PreValueDto>(sql).GroupBy(x => x.NodeId);

            foreach (var group in dtos)
            {
                var dataType = Database.Fetch<DataTypeDto>(Sql()
                    .Select<DataTypeDto>()
                    .From<DataTypeDto>()
                    .Where<DataTypeDto>(x => x.NodeId == group.Key)).First();

                // check for duplicate aliases
                var aliases = group.Select(x => x.Alias).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                if (aliases.Distinct().Count() != aliases.Length)
                    throw new InvalidOperationException($"Cannot migrate prevalues for datatype id={dataType.NodeId}, editor={dataType.EditorAlias}: duplicate alias.");

                // handle null/empty aliases
                int index = 0;
                var dictionary = group.ToDictionary(x => string.IsNullOrWhiteSpace(x.Alias) ? index++.ToString() : x.Alias);

                // migrate the preValues to configuration
                var migrator = _preValueMigrators.GetMigrator(dataType.EditorAlias) ?? new DefaultPreValueMigrator();
                var config = migrator.GetConfiguration(dataType.NodeId, dataType.EditorAlias, dictionary);
                var json = JsonConvert.SerializeObject(config);

                // validate - and kill the migration if it fails
                var newAlias = migrator.GetNewAlias(dataType.EditorAlias);
                if (newAlias == null)
                {
                    if (!LegacyAliases.Contains(dataType.EditorAlias))
                    {
                        _logger.Warn<DataTypeMigration,int,string>(
                            "Skipping validation of configuration for data type {NodeId} : {EditorAlias}."
                            + " Please ensure that the configuration is valid. The site may fail to start and / or load data types and run.",
                            dataType.NodeId, dataType.EditorAlias);
                    }
                }
                else if (!_propertyEditors.TryGet(newAlias, out var propertyEditor))
                {
                    if (!LegacyAliases.Contains(newAlias))
                    {
                        _logger.Warn<DataTypeMigration,int,string,string>("Skipping validation of configuration for data type {NodeId} : {NewEditorAlias} (was: {EditorAlias})"
                                                        + " because no property editor with that alias was found."
                                                        + " Please ensure that the configuration is valid. The site may fail to start and / or load data types and run.",
                            dataType.NodeId, newAlias, dataType.EditorAlias);
                    }
                }
                else
                {
                    var configEditor = propertyEditor.GetConfigurationEditor();
                    try
                    {
                        var _ = configEditor.FromDatabase(json);
                    }
                    catch (Exception e)
                    {
                        _logger.Warn<DataTypeMigration,int,string,string>(e, "Failed to validate configuration for data type {NodeId} : {NewEditorAlias} (was: {EditorAlias})."
                                                        + " Please fix the configuration and ensure it is valid. The site may fail to start and / or load data types and run.",
                                                        dataType.NodeId, newAlias, dataType.EditorAlias);
                    }
                }

                // update
                dataType.Configuration = JsonConvert.SerializeObject(config);
                Database.Update(dataType);
            }
        }
    }
}
