using System;
using System.Collections.Generic;
using AutoMapper;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Configures model mappings for datatypes.
    /// </summary>
    internal class DataTypeMapperProfile : Profile
    {
        public DataTypeMapperProfile(PropertyEditorCollection propertyEditors, ILogger logger)
        {
            // create, capture, cache
            var availablePropertyEditorsResolver = new AvailablePropertyEditorsResolver(Current.Configs.Settings().Content);
            var configurationDisplayResolver = new DataTypeConfigurationFieldDisplayResolver(logger);
            var databaseTypeResolver = new DatabaseTypeResolver();

            CreateMap<IDataEditor, PropertyEditorBasic>();

            // map the standard properties, not the values
            CreateMap<ConfigurationField, DataTypeConfigurationFieldDisplay>()
                .ForMember(dest => dest.Value, opt => opt.Ignore());

            var systemIds = new[]
            {
                Constants.DataTypes.DefaultContentListView,
                Constants.DataTypes.DefaultMediaListView,
                Constants.DataTypes.DefaultMembersListView
            };

            CreateMap<IDataEditor, DataTypeBasic>()
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.HasPrevalues, opt => opt.Ignore())
                .ForMember(dest => dest.IsSystemDataType, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.Key, opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            CreateMap<IDataType, DataTypeBasic>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(Constants.UdiEntityType.DataType, src.Key)))
                .ForMember(dest => dest.HasPrevalues, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.Group, opt => opt.Ignore())
                .ForMember(dest => dest.IsSystemDataType, opt => opt.MapFrom(src => systemIds.Contains(src.Id)))
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    if (Current.PropertyEditors.TryGet(src.EditorAlias, out var editor))
                    {
                        dest.Alias = editor.Alias;
                        dest.Group = editor.Group;
                        dest.Icon = editor.Icon;
                    }
                });

            CreateMap<IDataType, DataTypeDisplay>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(Constants.UdiEntityType.DataType, src.Key)))
                .ForMember(dest => dest.AvailableEditors, opt => opt.MapFrom(src => availablePropertyEditorsResolver.Resolve(src)))
                .ForMember(dest => dest.PreValues, opt => opt.MapFrom(src => configurationDisplayResolver.Resolve(src)))
                .ForMember(dest => dest.SelectedEditor, opt => opt.MapFrom(src => src.EditorAlias.IsNullOrWhiteSpace() ? null : src.EditorAlias))
                .ForMember(dest => dest.HasPrevalues, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.Group, opt => opt.Ignore())
                .ForMember(dest => dest.IsSystemDataType, opt => opt.MapFrom(src => systemIds.Contains(src.Id)))
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    if (Current.PropertyEditors.TryGet(src.EditorAlias, out var editor))
                    {
                        dest.Group = editor.Group;
                        dest.Icon = editor.Icon;
                    }
                });

            //gets a list of PreValueFieldDisplay objects from the data type definition
            CreateMap<IDataType, IEnumerable<DataTypeConfigurationFieldDisplay>>()
                  .ConvertUsing(src => configurationDisplayResolver.Resolve(src));

            CreateMap<DataTypeSave, IDataType>()
                .ConstructUsing(src => new DataType(propertyEditors[src.EditorAlias], -1) {CreateDate = DateTime.Now})
                .IgnoreEntityCommonProperties()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Convert.ToInt32(src.Id)))
                .ForMember(dest => dest.Key, opt => opt.Ignore()) // ignore key, else resets UniqueId - U4-3911
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.EditorAlias, opt => opt.MapFrom(src => src.EditorAlias))
                .ForMember(dest => dest.DatabaseType, opt => opt.MapFrom(src => databaseTypeResolver.Resolve(src)))
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
                .ForMember(dest => dest.Level, opt => opt.Ignore())
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
                .ForMember(dest => dest.Configuration, opt => opt.Ignore())
                .ForMember(dest => dest.Editor, opt => opt.MapFrom(src => propertyEditors[src.EditorAlias]));

            //Converts a property editor to a new list of pre-value fields - used when creating a new data type or changing a data type with new pre-vals
            CreateMap<IDataEditor, IEnumerable<DataTypeConfigurationFieldDisplay>>()
                .ConvertUsing((dataEditor, configurationFieldDisplays) =>
                    {
                        // this is a new data type, initialize default configuration
                        // get the configuration editor,
                        // get the configuration fields and map to UI,
                        // get the configuration default values and map to UI

                        var configurationEditor = dataEditor.GetConfigurationEditor();

                        var fields = configurationEditor.Fields.Select(Mapper.Map<DataTypeConfigurationFieldDisplay>).ToArray();

                        var defaultConfiguration = configurationEditor.DefaultConfiguration;
                        if (defaultConfiguration != null)
                            DataTypeConfigurationFieldDisplayResolver.MapConfigurationFields(logger, fields, defaultConfiguration);

                        return fields;
                    });
        }
    }
}
