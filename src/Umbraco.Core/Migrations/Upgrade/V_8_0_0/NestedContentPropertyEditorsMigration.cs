using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class NestedContentPropertyEditorsMigration : PropertyEditorsMigrationBase
    {
        private Dictionary<string, int> _elementTypeIds;
        private Dictionary<int, List<PropertyTypeDto>> _propertyTypes;
        private HashSet<int> _elementTypesInUse;

        private ConfigurationEditor _valueListConfigEditor;

        public NestedContentPropertyEditorsMigration(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            Prepare();

            bool refreshCache = UpdatePropertyData();
            refreshCache |= UpdateElementTypes();

            // if some data types have been updated directly in the database (editing DataTypeDto and/or PropertyDataDto),
            // bypassing the services, then we need to rebuild the cache entirely, including the umbracoContentNu table
            if (refreshCache)
                Context.AddPostMigration<RebuildPublishedSnapshot>();
        }

        private void Prepare()
        {
            _elementTypeIds = Database.Fetch<ContentTypeDto>(Sql()
                .Select<ContentTypeDto>(x => x.NodeId, x => x.Alias)
                .From<ContentTypeDto>()
                .InnerJoin<NodeDto>().On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(node => node.NodeObjectType == Constants.ObjectTypes.DocumentType))
                .ToDictionary(ct => ct.Alias, ct => ct.NodeId);

            _valueListConfigEditor = new ValueListConfigurationEditor();

            _elementTypesInUse = new HashSet<int>();
            _propertyTypes = new Dictionary<int, List<PropertyTypeDto>>();
        }

        private bool UpdatePropertyData()
        {
            var refreshCache = false;

            var dataTypes = GetDataTypes(Constants.PropertyEditors.Aliases.NestedContent);
            foreach (var dataType in dataTypes)
            {
                // get property data dtos
                var propertyDataDtos = Database.Fetch<PropertyDataDto>(Sql()
                    .Select<PropertyDataDto>()
                    .From<PropertyDataDto>()
                    .InnerJoin<PropertyTypeDto>().On<PropertyTypeDto, PropertyDataDto>((pt, pd) => pt.Id == pd.PropertyTypeId)
                    .InnerJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>((dt, pt) => dt.NodeId == pt.DataTypeId)
                    .Where<PropertyTypeDto>(x => x.DataTypeId == dataType.NodeId));

                // update dtos
                var updatedDtos = propertyDataDtos.Where(x => UpdateNestedPropertyDataDto(x)).ToArray();

                // persist changes
                foreach (var propertyDataDto in updatedDtos)
                {
                    Database.Update(propertyDataDto);
                    refreshCache = true;
                }
            }

            return refreshCache;
        }

        private bool UpdateNestedPropertyDataDto(PropertyDataDto pd)
        {
            if ( UpdateNestedContent(pd.TextValue, out string newValue))
            {
                pd.TextValue = newValue;
                return true;
            }

            return false;
        }

        private bool UpdateNestedContent(string inputValue, out string newValue)
        {
            bool changed = false;
            newValue = inputValue;

            if (String.IsNullOrWhiteSpace(inputValue))
                return false;

            var elements = JsonConvert.DeserializeObject<List<JObject>>(inputValue);
            foreach(var element in elements)
            {
                var elementTypeAlias = element["ncContentTypeAlias"]?.ToObject<string>();
                if (string.IsNullOrEmpty(elementTypeAlias))
                    continue;
                changed |= UpdateElement(element, elementTypeAlias);
            }

            if (changed)
                newValue = JsonConvert.SerializeObject(elements);

            return changed;
        }

        private bool UpdateElement(JObject element, string elementTypeAlias)
        {
            bool changed = false;

            var elementTypeId = _elementTypeIds[elementTypeAlias];
            _elementTypesInUse.Add(elementTypeId);

            var propertyValues = element.Properties().ToDictionary(p => p.Name, p => p.Value.ToString());
            if (!propertyValues.TryGetValue("key", out var keyo)
                || !Guid.TryParse(keyo.ToString(), out var key))
            {
                changed = true;
                element["key"] = Guid.NewGuid();
            }

            var propertyTypes = GetPropertyTypes(elementTypeId);

            foreach (var pt in propertyTypes)
            {
                if (!propertyValues.ContainsKey(pt.Alias) || String.IsNullOrWhiteSpace(propertyValues[pt.Alias]))
                    continue;                

                var propertyValue = propertyValues[pt.Alias];

                switch (pt.DataTypeDto.EditorAlias)
                {
                    case Constants.PropertyEditors.Aliases.RadioButtonList:
                    case Constants.PropertyEditors.Aliases.CheckBoxList:
                    case Constants.PropertyEditors.Aliases.DropDownListFlexible:
                        var config = (ValueListConfiguration)_valueListConfigEditor.FromDatabase(pt.DataTypeDto.Configuration);
                        bool isMultiple = true;
                        if (pt.DataTypeDto.EditorAlias == Constants.PropertyEditors.Aliases.RadioButtonList)
                            isMultiple = false;
                        element[pt.Alias] = UpdateValueList(propertyValue, config, isMultiple);
                        changed = true;
                        break;

                    case Constants.PropertyEditors.Aliases.NestedContent:
                        if ( UpdateNestedContent(propertyValue, out string newNestedContentValue))
                        {
                            element[pt.Alias] = newNestedContentValue;
                            changed = true;
                        }
                        break;

                    case Constants.PropertyEditors.Legacy.Aliases.RelatedLinks:
                    case Constants.PropertyEditors.Legacy.Aliases.RelatedLinks2:
                        if (string.IsNullOrWhiteSpace(propertyValue))
                            continue;
                        element[pt.Alias] = ConvertRelatedLinksToMultiUrlPicker(propertyValue);
                        changed = true;
                        break;
                }
            }

            return changed;
        }

        private List<PropertyTypeDto> GetPropertyTypes(int elementTypeId)
        {
            if (_propertyTypes.TryGetValue(elementTypeId, out var result))
            {
                return result;
            }
            else
            {
                result = Database.Fetch<PropertyTypeDto>(Sql()
                        .Select<PropertyTypeDto>(r => r.Select(x => x.DataTypeDto))
                        .From<PropertyTypeDto>()
                        .InnerJoin<DataTypeDto>().On<PropertyTypeDto, DataTypeDto>((pt, dt) => pt.DataTypeId == dt.NodeId)
                        .Where<PropertyTypeDto>(pt => pt.ContentTypeId == elementTypeId)
                        );
                _propertyTypes[elementTypeId] = result;

                return result;
            }
        }

        private string UpdateValueList(string propertyValue, ValueListConfiguration config, bool isMultiple)
        {
            var propData = new PropertyDataDto { VarcharValue = propertyValue };
            
            if (UpdatePropertyDataDto(propData, config, isMultiple: isMultiple))
            {
                return propData.VarcharValue;
            }

            return propertyValue;
        }

        private string ConvertRelatedLinksToMultiUrlPicker(string value)
        {
            var relatedLinks = JsonConvert.DeserializeObject<List<RelatedLink>>(value);
            var links = new List<LinkDto>();
            foreach (var relatedLink in relatedLinks)
            {
                GuidUdi udi = null;
                if (relatedLink.IsInternal)
                {
                    var linkIsUdi = GuidUdi.TryParse(relatedLink.Link, out udi);
                    if (linkIsUdi == false)
                    {
                        // oh no.. probably an integer, yikes!
                        if (int.TryParse(relatedLink.Link, out var intId))
                        {
                            var sqlNodeData = Sql()
                                .Select<NodeDto>()
                                .From<NodeDto>()
                                .Where<NodeDto>(x => x.NodeId == intId);

                            var node = Database.Fetch<NodeDto>(sqlNodeData).FirstOrDefault();
                            if (node != null)
                                // Note: RelatedLinks did not allow for picking media items,
                                // so if there's a value this will be a content item - hence
                                // the hardcoded "document" here
                                udi = new GuidUdi("document", node.UniqueId);
                        }
                    }
                }

                var link = new LinkDto
                {
                    Name = relatedLink.Caption,
                    Target = relatedLink.NewWindow ? "_blank" : null,
                    Udi = udi,
                    // Should only have a URL if it's an external link otherwise it wil be a UDI
                    Url = relatedLink.IsInternal == false ? relatedLink.Link : null
                };

                links.Add(link);
            }

            return JsonConvert.SerializeObject(links);
        }

        private bool UpdateElementTypes()
        {
            bool refreshCache = false;

            var documentContentTypesInUse = Database.Fetch<ContentDto>(Sql()
                .SelectDistinct<ContentDto>(x => x.ContentTypeId)
                .From<ContentDto>())
                .Select(x => x.ContentTypeId)
                .ToHashSet();

            var childContentTypes = Database.Fetch<ContentType2ContentTypeDto>(Sql()
                .SelectDistinct<ContentType2ContentTypeDto>(x => x.ChildId)
                .From<ContentType2ContentTypeDto>())
                .Select(x => x.ChildId)
                .ToHashSet();                

            foreach( var elementTypeId in _elementTypesInUse)
            {
                string elementTypeAlias = _elementTypeIds.First(t => t.Value == elementTypeId).Key;

                if (documentContentTypesInUse.Contains(elementTypeId))
                {
                    Logger.Warn<NestedContentPropertyEditorsMigration>("Content type {ContentTypeAlias} is used in nested content but could not be converted to an element type (documents exist)", elementTypeAlias);
                }
                else if (childContentTypes.Contains(elementTypeId))
                {
                    Logger.Warn<NestedContentPropertyEditorsMigration>("Content type {ContentTypeAlias} is used in nested content but could not be converted to an element type (has compositions)", elementTypeAlias);
                }
                else
                {
                    Database.Execute(Sql().Update<ContentTypeDto>(u => u.Set(x => x.IsElement, true)).Where<ContentTypeDto>(x => x.NodeId == elementTypeId));
                    Logger.Info<NestedContentPropertyEditorsMigration>("Marked content type {ContentTypeAlias} as an element type", elementTypeAlias);
                    refreshCache = true;
                }
            }

            return refreshCache;
        }

    }
}
