using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Migrations.Upgrade.V_8_7_0
{
    public class StackedContentToBlockList : MigrationBase
    {
        public StackedContentToBlockList(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            // Convert all Stacked Content properties to Block List properties, both in the data types and in the property data
            var refreshCache = Migrate(GetDataTypes("Our.Umbraco.StackedContent"), GetKnownDocumentTypes());

            // if some data types have been updated directly in the database (editing DataTypeDto and/or PropertyDataDto),
            // bypassing the services, then we need to rebuild the cache entirely, including the umbracoContentNu table
            if (refreshCache)
                Context.AddPostMigration<RebuildPublishedSnapshot>();
        }

        private List<DataTypeDto> GetDataTypes(string alias)
        {
            var sql = Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(d => d.EditorAlias == alias);

            return Database.Fetch<DataTypeDto>(sql);
        }

        private Dictionary<Guid, KnownContentType> GetKnownDocumentTypes()
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
                .WhereIn<DataTypeDto>(d => d.EditorAlias, new[] { Constants.PropertyEditors.Aliases.NestedContent, Constants.PropertyEditors.Aliases.ColorPicker });
            var props = Database.Fetch<PropertyTypeDto>(sql);
            // Get all nested content and color picker property aliases by content type ID
            var propLk = props.ToLookup(p => p.ContentTypeId, p => p.Alias);

            var knownMap = new Dictionary<Guid, KnownContentType>(types.Count);
            types.ForEach(t => knownMap[t.NodeDto.UniqueId] = new KnownContentType(t.Alias, t.NodeDto.UniqueId, propLk[t.NodeId].Union(joinLk[t.NodeId].SelectMany(r => propLk[r])).ToArray()));
            return knownMap;
        }

        private bool Migrate(IEnumerable<DataTypeDto> dataTypesToMigrate, Dictionary<Guid, KnownContentType> knownDocumentTypes)
        {
            var refreshCache = false;

            foreach (var dataType in dataTypesToMigrate)
            {
                if (!dataType.Configuration.IsNullOrWhiteSpace())
                {
                    var config = UpdateConfiguration(dataType, knownDocumentTypes);

                    if (config.Blocks.Length > 0) UpdatePropertyData(dataType, config, knownDocumentTypes);
                }

                UpdateDataType(dataType);

                refreshCache = true;
            }

            return refreshCache;
        }

        private BlockListConfiguration UpdateConfiguration(DataTypeDto dataType, Dictionary<Guid, KnownContentType> knownDocumentTypes)
        {
            var old = JsonConvert.DeserializeObject<StackedContentConfiguration>(dataType.Configuration);
            var config = new BlockListConfiguration
            {
                Blocks = old.ContentTypes?.Select(t => new BlockListConfiguration.BlockConfiguration
                {
                    Key = knownDocumentTypes.TryGetValue(t.IcContentTypeGuid, out var ct) ? ct.Key : Guid.Empty,
                    Label = t.NameTemplate
                }).Where(c => c.Key != null).ToArray(),
                UseInlineEditingAsDefault = old.SingleItemMode == "1" || old.SingleItemMode == bool.TrueString
            };

            if (int.TryParse(old.MaxItems, out var max) && max > 0)
            {
                config.ValidationLimit = new BlockListConfiguration.NumberRange { Max = max };
            }

            dataType.Configuration = ConfigurationEditor.ToDatabase(config);

            return config;
        }

        private void UpdatePropertyData(DataTypeDto dataType, BlockListConfiguration config, Dictionary<Guid, KnownContentType> knownDocumentTypes)
        {
            // get property data dtos
            var propertyDataDtos = Database.Fetch<PropertyDataDto>(Sql()
                .Select<PropertyDataDto>()
                .From<PropertyDataDto>()
                .InnerJoin<PropertyTypeDto>().On<PropertyTypeDto, PropertyDataDto>((pt, pd) => pt.Id == pd.PropertyTypeId)
                .InnerJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>((dt, pt) => dt.NodeId == pt.DataTypeId)
                .Where<PropertyTypeDto>(x => x.DataTypeId == dataType.NodeId));

            // update dtos
            var updatedDtos = propertyDataDtos.Where(x => UpdatePropertyDataDto(x, config, knownDocumentTypes));

            // persist changes
            foreach (var propertyDataDto in updatedDtos)
                Database.Update(propertyDataDto);
        }


        private bool UpdatePropertyDataDto(PropertyDataDto dto, BlockListConfiguration config, Dictionary<Guid, KnownContentType> knownDocumentTypes)
        {
            var model = new SimpleModel();

            if (dto != null && !dto.TextValue.IsNullOrWhiteSpace() && dto.TextValue[0] == '[')
            {
                var scObjs = JsonConvert.DeserializeObject<JObject[]>(dto.TextValue);
                foreach (var obj in scObjs) model.AddDataItem(obj, knownDocumentTypes);
            }

            dto.TextValue = JsonConvert.SerializeObject(model);

            return true;
        }

        private void UpdateDataType(DataTypeDto dataType)
        {
            dataType.DbType = ValueStorageType.Ntext.ToString();
            dataType.EditorAlias = Constants.PropertyEditors.Aliases.BlockList;

            Database.Update(dataType);
        }

        private class BlockListConfiguration
        {

            [JsonProperty("blocks")]
            public BlockConfiguration[] Blocks { get; set; }


            [JsonProperty("validationLimit")]
            public NumberRange ValidationLimit { get; set; } = new NumberRange();

            public class NumberRange
            {
                [JsonProperty("min")]
                public int? Min { get; set; }

                [JsonProperty("max")]
                public int? Max { get; set; }
            }

            public class BlockConfiguration
            {
                [JsonProperty("backgroundColor")]
                public string BackgroundColor { get; set; }

                [JsonProperty("iconColor")]
                public string IconColor { get; set; }

                [JsonProperty("thumbnail")]
                public string Thumbnail { get; set; }

                [JsonProperty("contentTypeKey")]
                public Guid Key { get; set; }

                [JsonProperty("settingsElementTypeKey")]
                public string settingsElementTypeKey { get; set; }

                [JsonProperty("view")]
                public string View { get; set; }

                [JsonProperty("label")]
                public string Label { get; set; }

                [JsonProperty("editorSize")]
                public string EditorSize { get; set; }
            }

            [JsonProperty("useInlineEditingAsDefault")]
            public bool UseInlineEditingAsDefault { get; set; }

            [JsonProperty("maxPropertyWidth")]
            public string MaxPropertyWidth { get; set; }
        }

        private class StackedContentConfiguration
        {

            public class StackedContentType
            {
                public Guid IcContentTypeGuid { get; set; }
                public string NameTemplate { get; set; }
            }

            public StackedContentType[] ContentTypes { get; set; }
            public string EnableCopy { get; set; }
            public string EnableFilter { get; set; }
            public string EnablePreview { get; set; }
            public string HideLabel { get; set; }
            public string MaxItems { get; set; }
            public string SingleItemMode { get; set; }
        }

        private class SimpleModel
        {
            [JsonProperty("layout")]
            public SimpleLayout Layout { get; } = new SimpleLayout();
            [JsonProperty("data")]
            public List<JObject> Data { get; } = new List<JObject>();

            public void AddDataItem(JObject obj, Dictionary<Guid, KnownContentType> knownDocumentTypes)
            {
                if (!Guid.TryParse(obj["key"].ToString(), out var key)) key = Guid.NewGuid();
                if (!Guid.TryParse(obj["icContentTypeGuid"].ToString(), out var ctGuid)) ctGuid = Guid.Empty;
                if (!knownDocumentTypes.TryGetValue(ctGuid, out var ct)) ct = new KnownContentType(null, ctGuid, null);

                obj.Remove("key");
                obj.Remove("icContentTypeGuid");

                var udi = new GuidUdi(Constants.UdiEntityType.Element, key).ToString();
                obj["udi"] = udi;                
                obj["contentTypeKey"] = ct.Key;

                if (ct.StringToRawProperties != null && ct.StringToRawProperties.Length > 0)
                {
                    // Nested content inside a stacked content item used to be stored as a deserialized string of the JSON array
                    // Now we store the content as the raw JSON array, so we need to convert from the string form to the array
                    foreach (var prop in ct.StringToRawProperties)
                    {
                        var val = obj[prop];
                        var value = val?.ToString();
                        if (val != null && val.Type == JTokenType.String && !value.IsNullOrWhiteSpace())
                            obj[prop] = JsonConvert.DeserializeObject<JToken>(value);
                    }
                }

                Data.Add(obj);
                Layout.Refs.Add(new SimpleLayout.SimpleLayoutRef { Udi = udi });
            }

            public class SimpleLayout
            {
                [JsonProperty(Constants.PropertyEditors.Aliases.BlockList)]
                public List<SimpleLayoutRef> Refs { get; } = new List<SimpleLayoutRef>();

                public class SimpleLayoutRef
                {
                    [JsonProperty("udi")]
                    public string Udi { get; set; }
                }
            }
        }

        private class KnownContentType
        {
            public KnownContentType(string alias, Guid key, string[] stringToRawProperties)
            {
                Alias = alias ?? throw new ArgumentNullException(nameof(alias));
                Key = key;
                StringToRawProperties = stringToRawProperties ?? throw new ArgumentNullException(nameof(stringToRawProperties));
            }

            public string Alias { get; }
            public Guid Key { get;  }
            public string[] StringToRawProperties { get;  }
        }
    }
}
