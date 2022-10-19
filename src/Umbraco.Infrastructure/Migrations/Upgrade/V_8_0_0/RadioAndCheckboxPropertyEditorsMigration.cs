using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.PostMigrations;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class RadioAndCheckboxPropertyEditorsMigration : PropertyEditorsMigrationBase
{
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;

    public RadioAndCheckboxPropertyEditorsMigration(
        IMigrationContext context,
        IIOHelper ioHelper,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : this(context, ioHelper, configurationEditorJsonSerializer,
            StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public RadioAndCheckboxPropertyEditorsMigration(
        IMigrationContext context,
        IIOHelper ioHelper,
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
        var refreshCache = false;

        refreshCache |= Migrate(GetDataTypes(Constants.PropertyEditors.Aliases.RadioButtonList), false);
        refreshCache |= Migrate(GetDataTypes(Constants.PropertyEditors.Aliases.CheckBoxList), true);

        // if some data types have been updated directly in the database (editing DataTypeDto and/or PropertyDataDto),
        // bypassing the services, then we need to rebuild the cache entirely, including the umbracoContentNu table
        if (refreshCache)
        {
            Context.AddPostMigration<RebuildPublishedSnapshot>();
        }
    }

    private bool Migrate(IEnumerable<DataTypeDto> dataTypes, bool isMultiple)
    {
        var refreshCache = false;
        ConfigurationEditor? configurationEditor = null;

        foreach (DataTypeDto dataType in dataTypes)
        {
            ValueListConfiguration config;

            if (dataType.Configuration.IsNullOrWhiteSpace())
            {
                continue;
            }

            // parse configuration, and update everything accordingly
            if (configurationEditor == null)
            {
                configurationEditor = new ValueListConfigurationEditor(_ioHelper, _editorConfigurationParser);
            }

            try
            {
                config = (ValueListConfiguration)configurationEditor.FromDatabase(
                    dataType.Configuration,
                    _configurationEditorJsonSerializer);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    ex, "Invalid configuration: \"{Configuration}\", cannot convert editor.",
                    dataType.Configuration);

                continue;
            }

            // get property data dtos
            List<PropertyDataDto>? propertyDataDtos = Database.Fetch<PropertyDataDto>(Sql()
                .Select<PropertyDataDto>()
                .From<PropertyDataDto>()
                .InnerJoin<PropertyTypeDto>()
                .On<PropertyTypeDto, PropertyDataDto>((pt, pd) => pt.Id == pd.PropertyTypeId)
                .InnerJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>((dt, pt) => dt.NodeId == pt.DataTypeId)
                .Where<PropertyTypeDto>(x => x.DataTypeId == dataType.NodeId));

            // update dtos
            IEnumerable<PropertyDataDto> updatedDtos =
                propertyDataDtos.Where(x => UpdatePropertyDataDto(x, config, isMultiple));

            // persist changes
            foreach (PropertyDataDto? propertyDataDto in updatedDtos)
            {
                Database.Update(propertyDataDto);
            }

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
