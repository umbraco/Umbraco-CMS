﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.DataTypes;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0
{

    public class DataTypeMigration : MigrationBase
    {
        private readonly PreValueMigratorCollection _preValueMigrators;
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly ILogger<DataTypeMigration> _logger;
        private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

        private static readonly ISet<string> LegacyAliases = new HashSet<string>()
        {
            Cms.Core.Constants.PropertyEditors.Legacy.Aliases.Date,
            Cms.Core.Constants.PropertyEditors.Legacy.Aliases.Textbox,
            Cms.Core.Constants.PropertyEditors.Legacy.Aliases.ContentPicker2,
            Cms.Core.Constants.PropertyEditors.Legacy.Aliases.MediaPicker2,
            Cms.Core.Constants.PropertyEditors.Legacy.Aliases.MemberPicker2,
            Cms.Core.Constants.PropertyEditors.Legacy.Aliases.RelatedLinks2,
            Cms.Core.Constants.PropertyEditors.Legacy.Aliases.TextboxMultiple,
            Cms.Core.Constants.PropertyEditors.Legacy.Aliases.MultiNodeTreePicker2,
        };

        public DataTypeMigration(IMigrationContext context,
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

        public override void Migrate()
        {
            // drop and create columns
            Delete.Column("pk").FromTable("cmsDataType").Do();

            // rename the table
            Rename.Table("cmsDataType").To(Cms.Core.Constants.DatabaseSchema.Tables.DataType).Do();

            // create column
            AddColumn<DataTypeDto>(Cms.Core.Constants.DatabaseSchema.Tables.DataType, "config");
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
                var json = _configurationEditorJsonSerializer.Serialize(config);

                // validate - and kill the migration if it fails
                var newAlias = migrator.GetNewAlias(dataType.EditorAlias);
                if (newAlias == null)
                {
                    if (!LegacyAliases.Contains(dataType.EditorAlias))
                    {
                        _logger.LogWarning(
                            "Skipping validation of configuration for data type {NodeId} : {EditorAlias}."
                            + " Please ensure that the configuration is valid. The site may fail to start and / or load data types and run.",
                            dataType.NodeId, dataType.EditorAlias);
                    }
                }
                else if (!_propertyEditors.TryGet(newAlias, out var propertyEditor))
                {
                    if (!LegacyAliases.Contains(newAlias))
                    {
                        _logger.LogWarning("Skipping validation of configuration for data type {NodeId} : {NewEditorAlias} (was: {EditorAlias})"
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
                        var _ = configEditor.FromDatabase(json, _configurationEditorJsonSerializer);
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "Failed to validate configuration for data type {NodeId} : {NewEditorAlias} (was: {EditorAlias})."
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
}
