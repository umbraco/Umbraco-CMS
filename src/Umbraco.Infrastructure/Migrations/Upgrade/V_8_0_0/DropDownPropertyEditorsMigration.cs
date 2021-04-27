﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Migrations.PostMigrations;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0
{
    public class DropDownPropertyEditorsMigration : PropertyEditorsMigrationBase
    {
        private readonly IIOHelper _ioHelper;
        private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

        public DropDownPropertyEditorsMigration(IMigrationContext context, IIOHelper ioHelper, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
            : base(context)
        {
            _ioHelper = ioHelper;
            _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        }

        public override void Migrate()
        {
            var refreshCache = Migrate(GetDataTypes(".DropDown", false));

            // if some data types have been updated directly in the database (editing DataTypeDto and/or PropertyDataDto),
            // bypassing the services, then we need to rebuild the cache entirely, including the umbracoContentNu table
            if (refreshCache)
                Context.AddPostMigration<RebuildPublishedSnapshot>();
        }

        private bool Migrate(IEnumerable<DataTypeDto> dataTypes)
        {
            var refreshCache = false;
            ConfigurationEditor configurationEditor = null;

            foreach (var dataType in dataTypes)
            {
                ValueListConfiguration config;

                if (!dataType.Configuration.IsNullOrWhiteSpace())
                {
                    // parse configuration, and update everything accordingly
                    if (configurationEditor == null)
                        configurationEditor = new ValueListConfigurationEditor(_ioHelper);
                    try
                    {
                        config = (ValueListConfiguration) configurationEditor.FromDatabase(dataType.Configuration, _configurationEditorJsonSerializer);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(
                            ex, "Invalid configuration: \"{Configuration}\", cannot convert editor.",
                            dataType.Configuration);

                        // reset
                        config = new ValueListConfiguration();
                    }

                    // get property data dtos
                    var propertyDataDtos = Database.Fetch<PropertyDataDto>(Sql()
                        .Select<PropertyDataDto>()
                        .From<PropertyDataDto>()
                        .InnerJoin<PropertyTypeDto>().On<PropertyTypeDto, PropertyDataDto>((pt, pd) => pt.Id == pd.PropertyTypeId)
                        .InnerJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>((dt, pt) => dt.NodeId == pt.DataTypeId)
                        .Where<PropertyTypeDto>(x => x.DataTypeId == dataType.NodeId));

                    // update dtos
                    var updatedDtos = propertyDataDtos.Where(x => UpdatePropertyDataDto(x, config, true));

                    // persist changes
                    foreach (var propertyDataDto in updatedDtos)
                        Database.Update(propertyDataDto);
                }
                else
                {
                    // default configuration
                    config = new ValueListConfiguration();
                }

                switch (dataType.EditorAlias)
                {
                    case string ea when ea.InvariantEquals("Umbraco.DropDown"):
                        UpdateDataType(dataType, config, false);
                        break;
                    case string ea when ea.InvariantEquals("Umbraco.DropdownlistPublishingKeys"):
                        UpdateDataType(dataType, config, false);
                        break;
                    case string ea when ea.InvariantEquals("Umbraco.DropDownMultiple"):
                        UpdateDataType(dataType, config, true);
                        break;
                    case string ea when ea.InvariantEquals("Umbraco.DropdownlistMultiplePublishKeys"):
                        UpdateDataType(dataType, config, true);
                        break;
                }

                refreshCache = true;
            }

            return refreshCache;
        }

        private void UpdateDataType(DataTypeDto dataType, ValueListConfiguration config, bool isMultiple)
        {
            dataType.DbType = ValueStorageType.Nvarchar.ToString();
            dataType.EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.DropDownListFlexible;

            var flexConfig = new DropDownFlexibleConfiguration
            {
                Items = config.Items,
                Multiple = isMultiple
            };
            dataType.Configuration = ConfigurationEditor.ToDatabase(flexConfig, _configurationEditorJsonSerializer);

            Database.Update(dataType);
        }
    }
}
