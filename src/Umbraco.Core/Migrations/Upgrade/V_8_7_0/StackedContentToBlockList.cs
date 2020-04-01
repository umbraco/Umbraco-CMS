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

        private Dictionary<Guid, string> GetKnownDocumentTypes()
        {
            var sql = Sql()
                .Select<ContentTypeDto>(r => r.Select(x => x.NodeDto))
                .From<ContentTypeDto>()
                .InnerJoin<NodeDto>()
                .On<ContentTypeDto, NodeDto>(c => c.NodeId, n => n.NodeId);

            var types = Database.Fetch<ContentTypeDto>(sql);
            var map = new Dictionary<Guid, string>(types.Count);
            types.ForEach(t => map[t.NodeDto.UniqueId] = t.Alias);
            return map;
        }

        private bool Migrate(IEnumerable<DataTypeDto> dataTypesToMigrate, Dictionary<Guid, string> knownDocumentTypes)
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

        private BlockListConfiguration UpdateConfiguration(DataTypeDto dataType, Dictionary<Guid, string> knownDocumentTypes)
        {
            var old = JsonConvert.DeserializeObject<StackedContentConfiguration>(dataType.Configuration);
            var config = new BlockListConfiguration
            {
                Blocks = old.ContentTypes?.Select(t => new BlockListConfiguration.BlockConfiguration
                {
                    Alias = knownDocumentTypes[t.IcContentTypeGuid],
                    Label = t.NameTemplate
                }).ToArray(),
                UseInlineEditingAsDefault = old.SingleItemMode == "1" || old.SingleItemMode == bool.TrueString
            };

            if (int.TryParse(old.MaxItems, out var max) && max > 0)
            {
                config.ValidationLimit = new BlockListConfiguration.NumberRange { Max = max };
            }

            dataType.Configuration = ConfigurationEditor.ToDatabase(config);

            return config;
        }

        private void UpdatePropertyData(DataTypeDto dataType, BlockListConfiguration config, Dictionary<Guid, string> knownDocumentTypes)
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


        private bool UpdatePropertyDataDto(PropertyDataDto dto, BlockListConfiguration config, Dictionary<Guid, string> knownDocumentTypes)
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

            [ConfigurationField("blocks", "Available Blocks", "views/propertyeditors/blocklist/prevalue/blocklist.elementtypepicker.html", Description = "Define the available blocks.")]
            public BlockConfiguration[] Blocks { get; set; }


            [ConfigurationField("validationLimit", "Amount", "numberrange", Description = "Set a required range of blocks")]
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
                [JsonProperty("contentTypeAlias")]
                public string Alias { get; set; }

                [JsonProperty("settingsElementTypeAlias")]
                public string SettingsElementTypeAlias { get; set; }

                [JsonProperty("view")]
                public string View { get; set; }

                [JsonProperty("label")]
                public string Label { get; set; }
            }

            [ConfigurationField("useInlineEditingAsDefault", "Inline editing mode", "boolean", Description = "Use the inline editor as the default block view")]
            public bool UseInlineEditingAsDefault { get; set; }

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

            public void AddDataItem(JObject obj, Dictionary<Guid, string> knownDocumentTypes)
            {
                if (!Guid.TryParse(obj["key"].ToString(), out var key)) throw new ArgumentException("Could not find a valid key in the data item");
                if (!Guid.TryParse(obj["icContentTypeGuid"].ToString(), out var ctGuid)) throw new ArgumentException("Could not find a valid content type GUID in the data item");
                if (!knownDocumentTypes.TryGetValue(ctGuid, out var ctAlias)) throw new ArgumentException($"Unknown content type GUID '{ctGuid}'");

                obj.Remove("key");
                obj.Remove("icContentTypeGuid");

                var udi = new GuidUdi(Constants.UdiEntityType.Element, key).ToString();
                obj["udi"] = udi;
                obj["contentTypeAlias"] = ctAlias;

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
    }
}
