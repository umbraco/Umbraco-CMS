﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0
{
    public class MergeDateAndDateTimePropertyEditor : MigrationBase
    {
        private readonly IIOHelper _ioHelper;
        private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

        public MergeDateAndDateTimePropertyEditor(IMigrationContext context, IIOHelper ioHelper, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
            : base(context)
        {
            _ioHelper = ioHelper;
            _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        }

        public override void Migrate()
        {
            var dataTypes = GetDataTypes(Cms.Core.Constants.PropertyEditors.Legacy.Aliases.Date);

            foreach (var dataType in dataTypes)
            {
                DateTimeConfiguration config;
                try
                {
                    config = (DateTimeConfiguration) new CustomDateTimeConfigurationEditor(_ioHelper).FromDatabase(
                        dataType.Configuration, _configurationEditorJsonSerializer);

                    // If the Umbraco.Date type is the default from V7 and it has never been updated, then the
                    // configuration is empty, and the format stuff is handled by in JS by moment.js. - We can't do that
                    // after the migration, so we force the format to the default from V7.
                    if (string.IsNullOrEmpty(dataType.Configuration))
                    {
                        config.Format = "YYYY-MM-DD";
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(
                        ex,
                        "Invalid property editor configuration detected: \"{Configuration}\", cannot convert editor, values will be cleared",
                        dataType.Configuration);

                    continue;
                }

                config.OffsetTime = false;

                dataType.EditorAlias = Cms.Core.Constants.PropertyEditors.Aliases.DateTime;
                dataType.Configuration = ConfigurationEditor.ToDatabase(config, _configurationEditorJsonSerializer);

                Database.Update(dataType);
            }
        }



        private List<DataTypeDto> GetDataTypes(string editorAlias)
        {
            //need to convert the old drop down data types to use the new one
            var dataTypes = Database.Fetch<DataTypeDto>(Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(x => x.EditorAlias == editorAlias));
            return dataTypes;
        }



        private class CustomDateTimeConfigurationEditor : ConfigurationEditor<DateTimeConfiguration>
        {
            public CustomDateTimeConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
            {
            }
        }
    }
}
