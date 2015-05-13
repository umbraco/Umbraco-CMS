using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class PropertyTypeGroupResolver : ValueResolver<IContentType, IEnumerable<PropertyTypeGroupDisplay>>
    {
        protected override IEnumerable<PropertyTypeGroupDisplay> ResolveCore(IContentType source)
        {
            var groups = new List<PropertyTypeGroupDisplay>();

            var propGroups = source.CompositionPropertyGroups;
            var genericGroup = new PropertyTypeGroupDisplay() { Name = "properties", Id = 0, ParentGroupId = 0 };
            genericGroup.Properties = MapProperties(source.PropertyTypes);
            genericGroup.Groups = new List<PropertyTypeGroupDisplay>();

            foreach (var group in propGroups.Where(pg => pg.ParentId.HasValue == false))
            {
                var mapped = new PropertyTypeGroupDisplay() {  Id = group.Id, ParentGroupId = 0, Name = group.Name, SortOrder = group.SortOrder };
                mapped.Properties = MapProperties(group.PropertyTypes);
                mapped.Groups = MapChildGroups(mapped, propGroups);
                groups.Add(mapped);
            }

            groups.Add(genericGroup);
            return groups;
        }

        private IEnumerable<PropertyTypeGroupDisplay> MapChildGroups(PropertyTypeGroupDisplay parent, IEnumerable<PropertyGroup> groups)
        {
            var mappedGroups = new List<PropertyTypeGroupDisplay>();
            
            foreach (var child in groups.Where(x => x.ParentId == parent.Id))
            {
                var mapped = new PropertyTypeGroupDisplay() { Id = child.Id, ParentGroupId = child.ParentId.Value, Name = child.Name, SortOrder = child.SortOrder };
                mapped.Name += child.PropertyTypes.Count.ToString();
                mapped.Properties = MapProperties(child.PropertyTypes);               
                mapped.Groups = MapChildGroups(mapped, groups);
                mappedGroups.Add(mapped);
            }

            return mappedGroups;
        }

        private IEnumerable<PropertyTypeDisplay> MapProperties(IEnumerable<PropertyType> properties)
        {
            var mappedProperties = new List<PropertyTypeDisplay>();
            foreach (var p in properties)
            {
                var editor = PropertyEditorResolver.Current.GetByAlias(p.PropertyEditorAlias);
                var preVals = UmbracoContext.Current.Application.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(p.DataTypeDefinitionId);

                mappedProperties.Add(
                    new PropertyTypeDisplay()
                    {
                        Alias = p.Alias,
                        Description = p.Description,
                        Editor = p.PropertyEditorAlias,
                        Validation = new PropertyTypeValidation() { Mandatory = p.Mandatory, Pattern = p.ValidationRegExp },
                        Label = p.Name,
                        View = editor.ValueEditor.View,
                        Config = editor.PreValueEditor.ConvertDbToEditor(editor.DefaultPreValues, preVals) ,
                        Value = ""
                    });
            }

            return mappedProperties;
        }
    }
}
