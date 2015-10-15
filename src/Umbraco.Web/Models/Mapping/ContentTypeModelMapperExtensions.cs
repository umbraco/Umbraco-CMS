using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Used as a shared way to do the underlying mapping for content types base classes
    /// </summary>
    /// <remarks>
    /// We used to use 'Include' Automapper inheritance functionality and although this works, the unit test
    ///     to assert mappings fails which is an Automapper bug. So instead we will use an extension method for the mappings
    ///     to re-use mappings.
    /// </remarks>
    internal static class ContentTypeModelMapperExtensions
    {

        public static void AfterMapContentTypeSaveToEntity<TSource, TDestination>(
            TSource source, TDestination dest,
            ApplicationContext applicationContext)
            where TSource : ContentTypeSave
            where TDestination : IContentTypeComposition
        {
            //sync compositions
            var current = dest.CompositionAliases().ToArray();
            var proposed = source.CompositeContentTypes;

            var remove = current.Where(x => proposed.Contains(x) == false);
            var add = proposed.Where(x => current.Contains(x) == false);

            foreach (var rem in remove)
            {
                dest.RemoveContentType(rem);
            }

            foreach (var a in add)
            {
                //TODO: Remove N+1 lookup
                var addCt = applicationContext.Services.ContentTypeService.GetContentType(a);
                if (addCt != null)
                    dest.AddContentType(addCt);
            }
        }

        public static IMappingExpression<TSource, TDestination> MapBaseContentTypeSaveToDisplay<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> mapping)
            where TSource : ContentTypeSave
            where TDestination : ContentTypeCompositionDisplay
        {
            return mapping
                .ForMember(dto => dto.CreateDate, expression => expression.Ignore())
                .ForMember(dto => dto.UpdateDate, expression => expression.Ignore())                
                .ForMember(dto => dto.ListViewEditorName, expression => expression.Ignore())
                .ForMember(dto => dto.AvailableCompositeContentTypes, expression => expression.Ignore())
                .ForMember(dto => dto.Notifications, expression => expression.Ignore())
                .ForMember(dto => dto.Errors, expression => expression.Ignore());
        }

        public static IMappingExpression<TSource, TDestination> MapBaseContentTypeEntityToDisplay<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> mapping, ApplicationContext applicationContext, Lazy<PropertyEditorResolver> propertyEditorResolver)
            where TSource : IContentTypeComposition
            where TDestination : ContentTypeCompositionDisplay
        {
            return mapping
                .ForMember(display => display.Notifications, expression => expression.Ignore())
                .ForMember(display => display.Errors, expression => expression.Ignore())
                .ForMember(display => display.AllowAsRoot, expression => expression.MapFrom(type => type.AllowedAsRoot))
                .ForMember(display => display.ListViewEditorName, expression => expression.Ignore())
                //Ignore because this is not actually used for content types
                .ForMember(display => display.Trashed, expression => expression.Ignore())

                .ForMember(
                    dto => dto.AllowedContentTypes,
                    expression => expression.MapFrom(dto => dto.AllowedContentTypes.Select(x => x.Id.Value)))

                .ForMember(
                    dto => dto.AvailableCompositeContentTypes,
                    expression => expression.ResolveUsing(new AvailableCompositeContentTypesResolver(applicationContext)))

                .ForMember(
                    dto => dto.CompositeContentTypes,
                    expression => expression.MapFrom(dto => dto.ContentTypeComposition))

                .ForMember(
                    dto => dto.Groups,
                    expression => expression.ResolveUsing(new PropertyTypeGroupResolver(applicationContext, propertyEditorResolver)));
        }

        /// <summary>
        /// Display -> Entity class base mapping logic
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="mapping"></param>
        /// <param name="applicationContext"></param>
        /// <returns></returns>        
        public static IMappingExpression<TSource, TDestination> MapBaseContentTypeSaveToEntity<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> mapping, ApplicationContext applicationContext)
            //where TSource : ContentTypeCompositionDisplay
            where TSource : ContentTypeSave
            where TDestination : IContentTypeComposition
        {
            return mapping
                //only map id if set to something higher then zero
                .ForMember(dto => dto.Id, expression => expression.Condition(display => (Convert.ToInt32(display.Id) > 0)))
                .ForMember(dto => dto.Id, expression => expression.MapFrom(display => Convert.ToInt32(display.Id)))

                //These get persisted as part of the saving procedure, nothing to do with the display model
                .ForMember(dto => dto.CreateDate, expression => expression.Ignore())
                .ForMember(dto => dto.UpdateDate, expression => expression.Ignore())

                .ForMember(dto => dto.AllowedAsRoot, expression => expression.MapFrom(display => display.AllowAsRoot))
                .ForMember(dto => dto.CreatorId, expression => expression.Ignore())
                .ForMember(dto => dto.Level, expression => expression.Ignore())
                .ForMember(dto => dto.SortOrder, expression => expression.Ignore())
                //ignore, we'll do this in after map
                .ForMember(dto => dto.PropertyGroups, expression => expression.Ignore())
                
                .ForMember(
                    dto => dto.AllowedContentTypes,
                    expression => expression.MapFrom(dto => dto.AllowedContentTypes.Select((t, i) => new ContentTypeSort(t, i))))

                .AfterMap((source, dest) =>
                {

                    var addedProperties = new List<string>();

                    //get all properties from groups that are not generic properties or inhertied (-666 id)
                    var selfNonGenericGroups = source.Groups.Where(x => x.Inherited == false && x.Id != -666).ToArray();

                    foreach (var group in selfNonGenericGroups)
                    {
                        //use underlying logic to add the property group which should wire most things up for us
                        dest.AddPropertyGroup(group.Name);

                        //now update that group with the values from the display object
                        Mapper.Map(group, dest.PropertyGroups[group.Name]);

                        foreach (var propType in group.Properties.Where(x => x.Inherited == false))
                        {
                            //update existing
                            if (propType.Id > 0)
                            {
                                var currentPropertyType = dest.PropertyTypes.FirstOrDefault(x => x.Id == propType.Id);
                                Mapper.Map(propType, currentPropertyType);
                            }
                            else
                            {
                                //add new
                                var mapped = Mapper.Map<PropertyType>(propType);
                                dest.AddPropertyType(mapped, group.Name);
                            }

                            addedProperties.Add(propType.Alias);
                        }
                    }

                    //Groups to remove
                    var groupsToRemove = dest.PropertyGroups.Select(x => x.Name).Except(selfNonGenericGroups.Select(x => x.Name)).ToArray();
                    foreach (var toRemove in groupsToRemove)
                    {
                        dest.RemovePropertyGroup(toRemove);
                    }

                    //add generic properties
                    var genericProperties = source.Groups.FirstOrDefault(x => x.Id == -666);
                    if (genericProperties != null)
                    {
                        foreach (var propertyTypeBasic in genericProperties.Properties.Where(x => x.Inherited == false))
                        {
                            dest.AddPropertyType(Mapper.Map<PropertyType>(propertyTypeBasic));
                            addedProperties.Add(propertyTypeBasic.Alias);
                        }
                    }

                    //remove deleted types
                    foreach (var removedType in dest.PropertyTypes
                        .Where(x => addedProperties.Contains(x.Alias) == false).ToList())
                    {
                        dest.RemovePropertyType(removedType.Alias);
                    }


                });
        }
    }
}