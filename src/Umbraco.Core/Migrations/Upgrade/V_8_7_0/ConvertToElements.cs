using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_7_0
{
    public class ConvertToElements : MigrationBase
    {
        public ConvertToElements(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            // Get all document type IDs by alias
            var docTypes = Database.Fetch<ContentTypeDto>();
            var docTypeMap = new Dictionary<string, int>(docTypes.Count);
            docTypes.ForEach(d => docTypeMap[d.Alias] = d.NodeId);

            // Find all Nested Content or Block List data types
            var dataTypes = GetDataTypes(Constants.PropertyEditors.Aliases.BlockList);

            // Find all document types listed in each
            var elementTypeIds = dataTypes.SelectMany(d => GetDocTypeIds(d.Configuration, docTypeMap)).ToList();

            // Find all compositions those document types use
            var parentElementTypeIds = Database.Fetch<ContentType2ContentTypeDto>(Sql()
                .Select<ContentType2ContentTypeDto>()
                .From<ContentType2ContentTypeDto>()
                .WhereIn<ContentType2ContentTypeDto>(c => c.ChildId, elementTypeIds)
                ).Select(c => c.ParentId);

            elementTypeIds = elementTypeIds.Union(parentElementTypeIds).ToList();

            // Convert all those document types to element type
            // TODO: We need to wait on an update from @benjaminc to make this 'safe'
            // see https://github.com/umbraco/Umbraco-CMS/pull/7910#discussion_r409927495
            foreach (var docType in docTypes)
            {
                if (!elementTypeIds.Contains(docType.NodeId)) continue;

                docType.IsElement = true;
                Database.Update(docType);
            }
        }

        private List<DataTypeDto> GetDataTypes(params string[] aliases)
        {
            var sql = Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .WhereIn<DataTypeDto>(d => d.EditorAlias, aliases);

            return Database.Fetch<DataTypeDto>(sql);
        }

        private IEnumerable<int> GetDocTypeIds(string configuration, Dictionary<string, int> idMap)
        {
            if (configuration.IsNullOrWhiteSpace() || configuration[0] != '{') return Enumerable.Empty<int>();

            var obj = JObject.Parse(configuration);
            if (obj["blocks"] is JArray blArr)
            {
                var arr = blArr.ToObject<BlockConfiguration[]>();
                return arr.Select(i => idMap.TryGetValue(i.Alias, out var id) ? id : 0).Where(i => i != 0);
            }

            return Enumerable.Empty<int>();
        }

        public class BlockConfiguration
        {
            [JsonProperty("contentTypeAlias")]
            public string Alias { get; set; }
        }
    }
}
