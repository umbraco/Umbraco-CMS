using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class RadioAndCheckboxPropertyEditorsMigration : PropertyEditorsMigrationBase
    {
        public RadioAndCheckboxPropertyEditorsMigration(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var refreshCache = false;

            refreshCache |= Migrate(GetDataTypes(Constants.PropertyEditors.Aliases.RadioButtonList), false);
            refreshCache |= Migrate(GetDataTypes(Constants.PropertyEditors.Aliases.CheckBoxList), true);

            // if some data types have been updated directly in the database (editing DataTypeDto and/or PropertyDataDto),
            // bypassing the services, then we need to rebuild the cache entirely, including the umbracoContentNu table
            if (refreshCache)
                Context.AddPostMigration<RebuildPublishedSnapshot>();
        }

        private bool Migrate(IEnumerable<DataTypeDto> dataTypes, bool isMultiple)
        {
            var refreshCache = false;
            ConfigurationEditor configurationEditor = null;

            foreach (var dataType in dataTypes)
            {
                ValueListConfiguration config;

                if (dataType.Configuration.IsNullOrWhiteSpace())
                    continue;

                // parse configuration, and update everything accordingly
                if (configurationEditor == null)
                    configurationEditor = new ValueListConfigurationEditor();
                try
                {
                    config = (ValueListConfiguration) configurationEditor.FromDatabase(dataType.Configuration);
                }
                catch (Exception ex)
                {
                    Logger.Error<DropDownPropertyEditorsMigration, string>(
                        ex, "Invalid configuration: \"{Configuration}\", cannot convert editor.",
                        dataType.Configuration);

                    continue;
                }

                // get property data dtos
                var propertyDataDtos = Database.Fetch<PropertyDataDto>(Sql()
                    .Select<PropertyDataDto>()
                    .From<PropertyDataDto>()
                    .InnerJoin<PropertyTypeDto>().On<PropertyTypeDto, PropertyDataDto>((pt, pd) => pt.Id == pd.PropertyTypeId)
                    .InnerJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>((dt, pt) => dt.NodeId == pt.DataTypeId)
                    .Where<PropertyTypeDto>(x => x.DataTypeId == dataType.NodeId));

                // update dtos
                var updatedDtos = propertyDataDtos.Where(x => UpdatePropertyDataDto(x, config, isMultiple));

                // persist changes
                foreach (var propertyDataDto in updatedDtos)
                    Database.Update(propertyDataDto);

                UpdateDataType(dataType);
                refreshCache = true;
            }

            return refreshCache;
        }

        private void UpdateDataType(DataTypeDto dataType)
        {
            dataType.DbType = ValueStorageType.Nvarchar.ToString();
            Database.Update(dataType);
        }
    }
}
