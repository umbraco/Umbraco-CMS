using System;
using System.Collections.Generic;
using AutoMapper;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Configure's model mappings for Data types
    /// </summary>
    internal class DataTypeMapperProfile : Profile
    {
        public DataTypeMapperProfile(IDataTypeService dataTypeService)
        {
            var lazyDataTypeService = new Lazy<IDataTypeService>(() => dataTypeService);

            // create, capture, cache
            var availablePropertyEditorsResolver = new AvailablePropertyEditorsResolver(UmbracoConfig.For.UmbracoSettings().Content);
            var preValueDisplayResolver = new PreValueDisplayResolver(lazyDataTypeService);
            var databaseTypeResolver = new DatabaseTypeResolver();

            CreateMap<PropertyEditor, PropertyEditorBasic>();

            //just maps the standard properties, does not map the value!
            CreateMap<PreValueField, PreValueFieldDisplay>()
                .ForMember(dest => dest.Value, opt => opt.Ignore());

            var systemIds = new[]
            {
                Constants.DataTypes.DefaultContentListView,
                Constants.DataTypes.DefaultMediaListView,
                Constants.DataTypes.DefaultMembersListView
            };

            CreateMap<PropertyEditor, DataTypeBasic>()
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.HasPrevalues, opt => opt.Ignore())
                .ForMember(dest => dest.IsSystemDataType, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.Key, opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());

            CreateMap<IDataTypeDefinition, DataTypeBasic>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(Constants.UdiEntityType.DataType, src.Key)))
                .ForMember(dest => dest.HasPrevalues, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.Group, opt => opt.Ignore())
                .ForMember(dest => dest.IsSystemDataType, opt => opt.MapFrom(src => systemIds.Contains(src.Id)))
                .AfterMap((src, dest) =>
                {
                    var editor = Current.PropertyEditors[src.PropertyEditorAlias];
                    if (editor != null)
                    {
                        dest.Alias = editor.Alias;
                        dest.Group = editor.Group;
                        dest.Icon = editor.Icon;
                    }
                });

            CreateMap<IDataTypeDefinition, DataTypeDisplay>()
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(src => Udi.Create(Constants.UdiEntityType.DataType, src.Key)))
                .ForMember(dest => dest.AvailableEditors, opt => opt.ResolveUsing(src => availablePropertyEditorsResolver.Resolve(src)))
                .ForMember(dest => dest.PreValues, opt => opt.ResolveUsing(src => preValueDisplayResolver.Resolve(src)))
                .ForMember(dest => dest.SelectedEditor, opt => opt.MapFrom(src => src.PropertyEditorAlias.IsNullOrWhiteSpace() ? null : src.PropertyEditorAlias))
                .ForMember(dest => dest.HasPrevalues, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.Group, opt => opt.Ignore())
                .ForMember(dest => dest.IsSystemDataType, opt => opt.MapFrom(src => systemIds.Contains(src.Id)))
                .AfterMap((src, dest) =>
                {
                    var editor = Current.PropertyEditors[src.PropertyEditorAlias];
                    if (editor != null)
                    {
                        dest.Group = editor.Group;
                        dest.Icon = editor.Icon;
                    }
                });

            //gets a list of PreValueFieldDisplay objects from the data type definition
            CreateMap<IDataTypeDefinition, IEnumerable<PreValueFieldDisplay>>()
                  .ConvertUsing(src => preValueDisplayResolver.Resolve(src));

            CreateMap<DataTypeSave, IDataTypeDefinition>()
                .ConstructUsing(src => new DataTypeDefinition(src.SelectedEditor) {CreateDate = DateTime.Now})
                .IgnoreDeletableEntityCommonProperties()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Convert.ToInt32(src.Id)))
                //we have to ignore the Key otherwise this will reset the UniqueId field which should never change!
                // http://issues.umbraco.org/issue/U4-3911
                .ForMember(dest => dest.Key, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.PropertyEditorAlias, opt => opt.MapFrom(src => src.SelectedEditor))
                .ForMember(dest => dest.DatabaseType, opt => opt.ResolveUsing(src => databaseTypeResolver.Resolve(src)))
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
                .ForMember(dest => dest.Level, opt => opt.Ignore())
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore());

            //Converts a property editor to a new list of pre-value fields - used when creating a new data type or changing a data type with new pre-vals
            CreateMap<PropertyEditor, IEnumerable<PreValueFieldDisplay>>()
                .ConvertUsing(src =>
                    {
                        //this is a new data type, so just return the field editors, there are no values yet
                        var defaultVals = src.DefaultPreValues;
                        var fields = src.PreValueEditor.Fields.Select(Mapper.Map<PreValueFieldDisplay>).ToArray();
                        if (defaultVals != null)
                        {
                            PreValueDisplayResolver.MapPreValueValuesToPreValueFields(fields, defaultVals);
                        }
                        return fields;
                    });
        }
    }
}
