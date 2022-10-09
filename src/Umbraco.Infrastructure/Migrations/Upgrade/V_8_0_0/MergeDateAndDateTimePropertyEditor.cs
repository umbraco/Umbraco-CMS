using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class MergeDateAndDateTimePropertyEditor : MigrationBase
{
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;

    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public MergeDateAndDateTimePropertyEditor(IMigrationContext context, IIOHelper ioHelper,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : this(context, ioHelper, configurationEditorJsonSerializer,
            StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public MergeDateAndDateTimePropertyEditor(IMigrationContext context, IIOHelper ioHelper,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer,
        IEditorConfigurationParser editorConfigurationParser)
        : base(context)
    {
        _ioHelper = ioHelper;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        _editorConfigurationParser = editorConfigurationParser;
    }

    protected override void Migrate()
    {
        List<DataTypeDto> dataTypes = GetDataTypes(Constants.PropertyEditors.Legacy.Aliases.Date);

        foreach (DataTypeDto dataType in dataTypes)
        {
            DateTimeConfiguration config;
            try
            {
                config = (DateTimeConfiguration)new CustomDateTimeConfigurationEditor(
                    _ioHelper,
                    _editorConfigurationParser).FromDatabase(
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

            dataType.EditorAlias = Constants.PropertyEditors.Aliases.DateTime;
            dataType.Configuration = ConfigurationEditor.ToDatabase(config, _configurationEditorJsonSerializer);

            Database.Update(dataType);
        }
    }

    private List<DataTypeDto> GetDataTypes(string editorAlias)
    {
        // need to convert the old drop down data types to use the new one
        List<DataTypeDto>? dataTypes = Database.Fetch<DataTypeDto>(Sql()
            .Select<DataTypeDto>()
            .From<DataTypeDto>()
            .Where<DataTypeDto>(x => x.EditorAlias == editorAlias));
        return dataTypes;
    }

    private class CustomDateTimeConfigurationEditor : ConfigurationEditor<DateTimeConfiguration>
    {
        public CustomDateTimeConfigurationEditor(
            IIOHelper ioHelper,
            IEditorConfigurationParser editorConfigurationParser)
            : base(ioHelper, editorConfigurationParser)
        {
        }
    }
}
