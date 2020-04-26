using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;
using static Umbraco.Core.Constants.PropertyEditors;

namespace Umbraco.Core.Migrations.Upgrade.V_8_7_0
{
    /// <summary>
    /// Sets all content types used in a Nested Content data type, and any
    /// compositions those content types use, to be Element types.  Also
    /// changes checkboxlist and dropdown data within nested content data from
    /// strings to raw JSON types
    /// </summary>
    public class NestedContentUpgrade : MigrationBase
    {
        public NestedContentUpgrade(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            var changes = MigrateElementTypes();
            changes = MigrateData() || changes;

            // if some data types have been updated directly in the database (editing DataTypeDto and/or PropertyDataDto),
            // bypassing the services, then we need to rebuild the cache entirely, including the umbracoContentNu table
            if (changes)
                Context.AddPostMigration<RebuildPublishedSnapshot>();
        }

        private bool MigrateElementTypes()
        {
            var sql = Sql()
                .Select<ContentTypeDto>()
                .From<ContentTypeDto>()
                .Where<ContentTypeDto>(d => d.IsElement);

            // Don't run this migration in a database where someone has already manually setup element types
            if (Database.Fetch<ContentTypeDto>(sql).Count > 0)
                return false;

            sql = Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(d => d.EditorAlias == Constants.PropertyEditors.Aliases.NestedContent);

            // Find all content types that are used in a nested content data type
            var dataTypes = Database.Fetch<DataTypeDto>(sql);
            var contentTypeAliases = new List<string>();
            dataTypes.ForEach(d => contentTypeAliases.AddRange(GetUsedContentTypes(d)));

            sql = Sql()
                .Select<ContentTypeDto>()
                .From<ContentTypeDto>()
                .WhereIn<ContentTypeDto>(d => d.Alias, contentTypeAliases);
            var dtos = Database.Fetch<ContentTypeDto>(sql);

            // Find all compositions used on content types used in a nested content data type
            sql = Sql()
                .Select<ContentType2ContentTypeDto>()
                .From<ContentType2ContentTypeDto>()
                .WhereIn<ContentType2ContentTypeDto>(c => c.ChildId, dtos.Select(d => d.NodeId));
            var c2cs = Database.Fetch<ContentType2ContentTypeDto>(sql);

            sql = Sql()
                .Select<ContentTypeDto>()
                .From<ContentTypeDto>()
                .WhereIn<ContentTypeDto>(d => d.NodeId, c2cs.Select(c => c.ParentId));
            dtos.AddRange(Database.Fetch<ContentTypeDto>(sql));

            foreach (var dto in dtos)
            {
                dto.IsElement = true;
                Database.Update(dto);
            }

            return dtos.Count > 0;
        }

        private IEnumerable<string> GetUsedContentTypes(DataTypeDto dto)
        {
            if (dto.Configuration.IsNullOrWhiteSpace() || dto.Configuration[0] != '{') return Enumerable.Empty<string>();

            var config = JsonConvert.DeserializeObject<NcConfig>(dto.Configuration);
            return config.ContentTypes?.Select(c => c.Alias) ?? Enumerable.Empty<string>();
        }

        private bool MigrateData()
        {
            // Get all checkboxlist and dropdown data types so that we can identify them in the data
            var propsToUpdate = GetPropertiesToUpdateByContentType();
            var nestedContentPropertyIds = propsToUpdate.Values.SelectMany(v => v.Select(r => r.PropertyTypeId));

            var sql = Sql()
                .Select<PropertyDataDto>()
                .From<PropertyDataDto>()
                .WhereIn<PropertyDataDto>(p => p.PropertyTypeId, nestedContentPropertyIds);
            var dtos = Database.Fetch<PropertyDataDto>(sql);
            var changes = false;

            foreach (var dto in dtos)
            {
                var updated = GetUpdatedNestedContent(dto.TextValue, propsToUpdate);
                if (updated == dto.TextValue) continue;

                dto.TextValue = updated;
                changes = true;
                Database.Update(dto);
            }

            return changes;
        }

        private Dictionary<string, IEnumerable<PropertyInfo>> GetPropertiesToUpdateByContentType()
        {
            var sql = Sql()
                .Select<ContentTypeDto>(r => r.Select(x => x.NodeDto))
                .From<ContentTypeDto>()
                .InnerJoin<NodeDto>()
                .On<ContentTypeDto, NodeDto>(c => c.NodeId, n => n.NodeId);

            var types = Database.Fetch<ContentTypeDto>(sql);
            var typeMap = new Dictionary<int, ContentTypeDto>(types.Count);
            types.ForEach(t => typeMap[t.NodeId] = t);

            sql = Sql()
                .Select<ContentType2ContentTypeDto>()
                .From<ContentType2ContentTypeDto>();
            var joins = Database.Fetch<ContentType2ContentTypeDto>(sql);
            // Find all relationships between types, either inherited or composited
            var joinLk = joins
                .Union(types
                    .Where(t => typeMap.ContainsKey(t.NodeDto.ParentId))
                    .Select(t => new ContentType2ContentTypeDto { ChildId = t.NodeId, ParentId = t.NodeDto.ParentId }))
                .ToLookup(j => j.ChildId, j => j.ParentId);

            sql = Sql()
                .Select<PropertyTypeDto>(r => r.Select(x => x.DataTypeDto))
                .From<PropertyTypeDto>()
                .InnerJoin<DataTypeDto>()
                .On<PropertyTypeDto, DataTypeDto>(c => c.DataTypeId, n => n.NodeId)
                .WhereIn<DataTypeDto>(d => d.EditorAlias, new[]
                {
                    Aliases.NestedContent,
                    Aliases.CheckBoxList,
                    Aliases.DropDownListFlexible,
                    Aliases.RadioButtonList
                });
            var props = Database.Fetch<PropertyTypeDto>(sql);
            var propLk = props.ToLookup(p => p.ContentTypeId, p => new PropertyInfo(p));

            // Get all properties that used to be stored as JSON data within string data, but which now need to be stored as raw JSON data
            var propsToUpdate = new Dictionary<string, IEnumerable<PropertyInfo>>(types.Count);
            types.ForEach(t => propsToUpdate[t.Alias] = propLk[t.NodeId].Union(joinLk[t.NodeId].SelectMany(r => propLk[r])));

            return propsToUpdate;
        }

        private string GetUpdatedNestedContent(string contentString, Dictionary<string, IEnumerable<PropertyInfo>> propsToUpdate)
        {
            if (contentString.IsNullOrWhiteSpace() || contentString[0] != '[') return contentString;

            var contents = JArray.Parse(contentString);
            foreach (var item in contents)
            {
                if (!(item is JObject content)
                    || !(content["ncContentTypeAlias"] is JValue jval)
                    || jval.Type != JTokenType.String
                    || !(jval?.ToString() is string contentType)
                    || !propsToUpdate.TryGetValue(contentType, out var properties)
                    || !properties.Any())
                    continue;

                foreach (var property in properties)
                {
                    var value = content[property.Alias]?.ToString();

                    if (property.Recurse) content[property.Alias] = GetUpdatedNestedContent(value, propsToUpdate);
                    else if (!property.Array && !value.IsNullOrWhiteSpace() && value[0] != '"')
                    {
                        value = new JValue(property.Values.TryGetValue(value, out var val) ? val : value).ToString();
                        content[property.Alias] = value;
                    }
                    else if (property.Array && (value.IsNullOrWhiteSpace() || value[0] != '['))
                    {
                        value = new JArray((value ?? "")
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(v => property.Values.TryGetValue(v, out var val) ? val : v)
                            .ToArray())
                            .ToString();
                        content[property.Alias] = value;
                    }
                }
            }

            return contents.ToString(Formatting.None);
        }

        private class NcConfig
        {
            [JsonProperty("contentTypes")]
            public NcContentType[] ContentTypes { get; set; }
        }

        private class NcContentType
        {
            [JsonProperty("ncAlias")]
            public string Alias { get; set; }
        }

        private class KnownContentType
        {
            public string Alias { get; set; }
            public string[] StringToRawProperties { get; set; }
        }

        private class PropertyInfo
        {
            public PropertyInfo(PropertyTypeDto dto)
            {
                Alias = dto.Alias;
                PropertyTypeId = dto.Id;

                if (dto.DataTypeDto.EditorAlias == Aliases.NestedContent)
                {
                    Recurse = true;
                }
                else if (!dto.DataTypeDto.Configuration.IsNullOrWhiteSpace() && dto.DataTypeDto.Configuration[0] == '{')
                {
                    Array = dto.DataTypeDto.EditorAlias != Aliases.RadioButtonList;
                    var config = JsonConvert.DeserializeObject<ValueListConfiguration>(dto.DataTypeDto.Configuration);
                    foreach (var item in config.Items)
                    {
                        Values[item.Id.ToString()] = item.Value;
                    }
                }
            }

            public string Alias { get; set; }
            public bool Recurse { get; set; }
            public bool Array { get; set; }
            public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();
            public int PropertyTypeId { get; set; }
        }
    }
}
