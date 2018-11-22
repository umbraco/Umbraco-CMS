using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
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
    internal static class ContentTypeProfileExtensions
    {
        public static IMappingExpression<TSource, PropertyGroup> MapPropertyGroupBasicToPropertyGroupPersistence<TSource, TPropertyTypeBasic>(
            this IMappingExpression<TSource, PropertyGroup> mapping)
            where TSource : PropertyGroupBasic<TPropertyTypeBasic>
            where TPropertyTypeBasic : PropertyTypeBasic
        {
            return mapping
                .ConstructUsing(x => new PropertyGroup(false)) // fixme - we have NO idea of isPublishing here = wtf?
                .IgnoreEntityCommonProperties()
                .ForMember(dest => dest.Id, map => map.Condition(src => src.Id > 0))
                .ForMember(dest => dest.Key, map => map.Ignore())
                .ForMember(dest => dest.HasIdentity, map => map.Ignore())
                .ForMember(dest => dest.DeleteDate, map => map.Ignore())
                .ForMember(dest => dest.PropertyTypes, map => map.Ignore());
        }

        public static IMappingExpression<TSource, PropertyGroupDisplay<TPropertyTypeDisplay>> MapPropertyGroupBasicToPropertyGroupDisplay<TSource, TPropertyTypeBasic, TPropertyTypeDisplay>(
            this IMappingExpression<TSource, PropertyGroupDisplay<TPropertyTypeDisplay>> mapping)
            where TSource : PropertyGroupBasic<TPropertyTypeBasic>
            where TPropertyTypeBasic : PropertyTypeBasic
            where TPropertyTypeDisplay : PropertyTypeDisplay
        {
            return mapping
                .ForMember(dest => dest.Id, opt => opt.Condition(src => src.Id > 0))
                .ForMember(dest => dest.ContentTypeId, opt => opt.Ignore())
                .ForMember(dest => dest.ParentTabContentTypes, opt => opt.Ignore())
                .ForMember(dest => dest.ParentTabContentTypeNames, opt => opt.Ignore())
                .ForMember(dest => dest.Properties, opt => opt.MapFrom(src => src.Properties.Select(Mapper.Map<TPropertyTypeDisplay>)));
        }

        public static void AfterMapContentTypeSaveToEntity<TSource, TDestination>(TSource source, TDestination dest, IContentTypeService contentTypeService)
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
                var addCt = contentTypeService.Get(a);
                if (addCt != null)
                    dest.AddContentType(addCt);
            }
        }

        public static void AfterMapMediaTypeSaveToEntity<TSource, TDestination>(TSource source, TDestination dest, IMediaTypeService mediaTypeService)
            where TSource : MediaTypeSave
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
                var addCt = mediaTypeService.Get(a);
                if (addCt != null)
                    dest.AddContentType(addCt);
            }
        }

        public static IMappingExpression<TSource, TDestination> MapBaseContentTypeSaveToDisplay<TSource, TPropertyTypeSource, TDestination, TPropertyTypeDestination>(
            this IMappingExpression<TSource, TDestination> mapping)
            where TSource : ContentTypeSave<TPropertyTypeSource>
            where TDestination : ContentTypeCompositionDisplay<TPropertyTypeDestination>
            where TPropertyTypeDestination : PropertyTypeDisplay
            where TPropertyTypeSource : PropertyTypeBasic
        {
            var propertyGroupDisplayResolver = new PropertyGroupDisplayResolver<TSource, TPropertyTypeSource, TPropertyTypeDestination>();

            return mapping
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.ListViewEditorName, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Errors, opt => opt.Ignore())
                .ForMember(dest => dest.LockedCompositeContentTypes, opt => opt.Ignore())
                .ForMember(dest => dest.Groups, opt => opt.ResolveUsing(src => propertyGroupDisplayResolver.Resolve(src)));
        }

        public static IMappingExpression<TSource, TDestination> MapBaseContentTypeEntityToDisplay<TSource, TDestination, TPropertyTypeDisplay>(
            this IMappingExpression<TSource, TDestination> mapping, PropertyEditorCollection propertyEditors,
            IDataTypeService dataTypeService, IContentTypeService contentTypeService)
            where TSource : IContentTypeComposition
            where TDestination : ContentTypeCompositionDisplay<TPropertyTypeDisplay>
            where TPropertyTypeDisplay : PropertyTypeDisplay, new()
        {
            var contentTypeUdiResolver = new ContentTypeUdiResolver();
            var lockedCompositionsResolver = new LockedCompositionsResolver(contentTypeService);
            var propertyTypeGroupResolver = new PropertyTypeGroupResolver<TPropertyTypeDisplay>(propertyEditors, dataTypeService);

            return mapping
                .ForMember(dest => dest.Udi, opt => opt.ResolveUsing(src => contentTypeUdiResolver.Resolve(src)))
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Blueprints, opt => opt.Ignore())
                .ForMember(dest => dest.Errors, opt => opt.Ignore())
                .ForMember(dest => dest.AllowAsRoot, opt => opt.MapFrom(src => src.AllowedAsRoot))
                .ForMember(dest => dest.ListViewEditorName, opt => opt.Ignore())
                //Ignore because this is not actually used for content types
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())

                .ForMember(dest => dest.AllowedContentTypes, opt => opt.MapFrom(src => src.AllowedContentTypes.Select(x => x.Id.Value)))
                .ForMember(dest => dest.CompositeContentTypes, opt => opt.MapFrom(src => src.ContentTypeComposition))
                .ForMember(dest => dest.LockedCompositeContentTypes, opt => opt.ResolveUsing(src => lockedCompositionsResolver.Resolve(src)))
                .ForMember(dest => dest.Groups, opt => opt.ResolveUsing(src => propertyTypeGroupResolver.Resolve(src)))
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore());
        }

        /// <summary>
        /// Display -> Entity class base mapping logic
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <typeparam name="TSourcePropertyType"></typeparam>
        /// <param name="mapping"></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDestination> MapBaseContentTypeSaveToEntity<TSource, TSourcePropertyType, TDestination>(
            this IMappingExpression<TSource, TDestination> mapping)
            //where TSource : ContentTypeCompositionDisplay
            where TSource : ContentTypeSave<TSourcePropertyType>
            where TDestination : IContentTypeComposition
            where TSourcePropertyType : PropertyTypeBasic
        {
            // fixme not so clean really
            var isPublishing = typeof(IContentType).IsAssignableFrom(typeof(TDestination));

            mapping = mapping
                //only map id if set to something higher then zero
                .ForMember(dest => dest.Id, opt => opt.Condition(src => (Convert.ToInt32(src.Id) > 0)))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Convert.ToInt32(src.Id)))

                //These get persisted as part of the saving procedure, nothing to do with the display model
                .IgnoreEntityCommonProperties()

                .ForMember(dest => dest.AllowedAsRoot, opt => opt.MapFrom(src => src.AllowAsRoot))
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
                .ForMember(dest => dest.Level, opt => opt.Ignore())
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
                //ignore, we'll do this in after map
                .ForMember(dest => dest.PropertyGroups, opt => opt.Ignore())
                .ForMember(dest => dest.NoGroupPropertyTypes, opt => opt.Ignore())
                // ignore, composition is managed in AfterMapContentTypeSaveToEntity
                .ForMember(dest => dest.ContentTypeComposition, opt => opt.Ignore());

            // ignore for members
            mapping = typeof(TDestination) == typeof(IMemberType)
                ? mapping.ForMember(dto => dto.Variations, opt => opt.Ignore())
                : mapping.ForMember(dto => dto.Variations, opt => opt.ResolveUsing<ContentTypeVariationsResolver<TSource, TSourcePropertyType, TDestination>>());

            mapping = mapping
                .ForMember(
                    dest => dest.AllowedContentTypes,
                    opt => opt.MapFrom(src => src.AllowedContentTypes.Select((t, i) => new ContentTypeSort(t, i))))

                .AfterMap((src, dest) =>
                {
                    // handle property groups and property types
                    // note that ContentTypeSave has
                    // - all groups, inherited and local; only *one* occurence per group *name*
                    // - potentially including the generic properties group
                    // - all properties, inherited and local
                    //
                    // also, see PropertyTypeGroupResolver.ResolveCore:
                    // - if a group is local *and* inherited, then Inherited is true
                    //   and the identifier is the identifier of the *local* group
                    //
                    // IContentTypeComposition AddPropertyGroup, AddPropertyType methods do some
                    // unique-alias-checking, etc that is *not* compatible with re-mapping everything
                    // the way we do it here, so we should exclusively do it by
                    // - managing a property group's PropertyTypes collection
                    // - managing the content type's PropertyTypes collection (for generic properties)

                    // handle actual groups (non-generic-properties)
                    var destOrigGroups = dest.PropertyGroups.ToArray(); // local groups
                    var destOrigProperties = dest.PropertyTypes.ToArray(); // all properties, in groups or not
                    var destGroups = new List<PropertyGroup>();
                    var sourceGroups = src.Groups.Where(x => x.IsGenericProperties == false).ToArray();
                    foreach (var sourceGroup in sourceGroups)
                    {
                        // get the dest group
                        var destGroup = MapSaveGroup(sourceGroup, destOrigGroups);

                        // handle local properties
                        var destProperties = sourceGroup.Properties
                            .Where(x => x.Inherited == false)
                            .Select(x => MapSaveProperty(x, destOrigProperties))
                            .ToArray();

                        // if the group has no local properties, skip it, ie sort-of garbage-collect
                        // local groups which would not have local properties anymore
                        if (destProperties.Length == 0)
                            continue;

                        // ensure no duplicate alias, then assign the group properties collection
                        EnsureUniqueAliases(destProperties);
                        destGroup.PropertyTypes = new PropertyTypeCollection(isPublishing, destProperties);
                        destGroups.Add(destGroup);
                    }

                    // ensure no duplicate name, then assign the groups collection
                    EnsureUniqueNames(destGroups);
                    dest.PropertyGroups = new PropertyGroupCollection(destGroups);

                    // because the property groups collection was rebuilt, there is no need to remove
                    // the old groups - they are just gone and will be cleared by the repository

                    // handle non-grouped (ie generic) properties
                    var genericPropertiesGroup = src.Groups.FirstOrDefault(x => x.IsGenericProperties);
                    if (genericPropertiesGroup != null)
                    {
                        // handle local properties
                        var destProperties = genericPropertiesGroup.Properties
                            .Where(x => x.Inherited == false)
                            .Select(x => MapSaveProperty(x, destOrigProperties))
                            .ToArray();

                        // ensure no duplicate alias, then assign the generic properties collection
                        EnsureUniqueAliases(destProperties);
                        dest.NoGroupPropertyTypes = new PropertyTypeCollection(isPublishing, destProperties);
                    }

                    // because all property collections were rebuilt, there is no need to remove
                    // some old properties, they are just gone and will be cleared by the repository
                });

            return mapping;
        }

        private static PropertyGroup MapSaveGroup<TPropertyType>(PropertyGroupBasic<TPropertyType> sourceGroup, IEnumerable<PropertyGroup> destOrigGroups)
            where TPropertyType : PropertyTypeBasic
        {
            PropertyGroup destGroup;
            if (sourceGroup.Id > 0)
            {
                // update an existing group
                // ensure it is still there, then map/update
                destGroup = destOrigGroups.FirstOrDefault(x => x.Id == sourceGroup.Id);
                if (destGroup != null)
                {
                    Mapper.Map(sourceGroup, destGroup);
                    return destGroup;
                }

                // force-clear the ID as it does not match anything
                sourceGroup.Id = 0;
            }

            // insert a new group, or update an existing group that has
            // been deleted in the meantime and we need to re-create
            // map/create
            destGroup = Mapper.Map<PropertyGroup>(sourceGroup);
            return destGroup;
        }

        private static PropertyType MapSaveProperty(PropertyTypeBasic sourceProperty, IEnumerable<PropertyType> destOrigProperties)
        {
            PropertyType destProperty;
            if (sourceProperty.Id > 0)
            {
                // updateg an existing property
                // ensure it is still there, then map/update
                destProperty = destOrigProperties.FirstOrDefault(x => x.Id == sourceProperty.Id);
                if (destProperty != null)
                {
                    Mapper.Map(sourceProperty, destProperty);
                    return destProperty;
                }

                // force-clear the ID as it does not match anything
                sourceProperty.Id = 0;
            }

            // insert a new property, or update an existing property that has
            // been deletedin the meantime and we need to re-create
            // map/create
            destProperty = Mapper.Map<PropertyType>(sourceProperty);
            return destProperty;
        }

        private static void EnsureUniqueAliases(IEnumerable<PropertyType> properties)
        {
            var propertiesA = properties.ToArray();
            var distinctProperties = propertiesA
                .Select(x => x.Alias.ToUpperInvariant())
                .Distinct()
                .Count();
            if (distinctProperties != propertiesA.Length)
                throw new InvalidOperationException("Cannot map properties due to alias conflict.");
        }

        private static void EnsureUniqueNames(IEnumerable<PropertyGroup> groups)
        {
            var groupsA = groups.ToArray();
            var distinctProperties = groupsA
                .Select(x => x.Name.ToUpperInvariant())
                .Distinct()
                .Count();
            if (distinctProperties != groupsA.Length)
                throw new InvalidOperationException("Cannot map groups due to name conflict.");
        }
    }
}
