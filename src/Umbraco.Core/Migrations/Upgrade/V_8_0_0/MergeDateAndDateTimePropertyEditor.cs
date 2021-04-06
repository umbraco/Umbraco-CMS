using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class MergeDateAndDateTimePropertyEditor : MigrationBase
    {
        public MergeDateAndDateTimePropertyEditor(IMigrationContext context)
            : base(context)
        {
        }

        public override void Migrate()
        {
            var dataTypes = GetDataTypes(Constants.PropertyEditors.Legacy.Aliases.Date);

            foreach (var dataType in dataTypes)
            {
                DateTimeConfiguration config;
                try
                {
                    config = (DateTimeConfiguration) new CustomDateTimeConfigurationEditor().FromDatabase(
                        dataType.Configuration);

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
                    Logger.Error<DropDownPropertyEditorsMigration, string>(
                        ex,
                        "Invalid property editor configuration detected: \"{Configuration}\", cannot convert editor, values will be cleared",
                        dataType.Configuration);

                    continue;
                }

                config.OffsetTime = false;

                dataType.EditorAlias = Constants.PropertyEditors.Aliases.DateTime;
                dataType.Configuration = ConfigurationEditor.ToDatabase(config);

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
        }
    }
}
