using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class UpdatePickerIntegerValuesToUdi : MigrationBase
    {
        public UpdatePickerIntegerValuesToUdi(IMigrationContext context) : base(context)
        { }

        public override void Migrate()
        {
            var sqlDataTypes = Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(x => x.EditorAlias == Constants.PropertyEditors.Aliases.ContentPicker
                                         || x.EditorAlias == Constants.PropertyEditors.Aliases.MediaPicker
                                         || x.EditorAlias == Constants.PropertyEditors.Aliases.MultiNodeTreePicker);

            var dataTypes = Database.Fetch<DataTypeDto>(sqlDataTypes).ToList();

            foreach (var datatype in dataTypes.Where(x => !x.Configuration.IsNullOrWhiteSpace()))
            {
                switch (datatype.EditorAlias)
                {
                    case Constants.PropertyEditors.Aliases.ContentPicker:
                    case Constants.PropertyEditors.Aliases.MediaPicker:
                        {
                            var config = JsonConvert.DeserializeObject<JObject>(datatype.Configuration);
                            var startNodeId = config.Value<string>("startNodeId");
                            if (!startNodeId.IsNullOrWhiteSpace() && int.TryParse(startNodeId, out var intStartNode))
                            {
                                var guid = intStartNode <= 0
                                    ? null
                                    : Context.Database.ExecuteScalar<Guid?>(
                                        Sql().Select<NodeDto>(x => x.UniqueId).From<NodeDto>().Where<NodeDto>(x => x.NodeId == intStartNode));
                                if (guid.HasValue)
                                {
                                    var udi = new GuidUdi(datatype.EditorAlias == Constants.PropertyEditors.Aliases.MediaPicker
                                        ? Constants.UdiEntityType.Media
                                        : Constants.UdiEntityType.Document, guid.Value);
                                    config["startNodeId"] = new JValue(udi.ToString());
                                }
                                else
                                    config.Remove("startNodeId");

                                datatype.Configuration = JsonConvert.SerializeObject(config);
                                Database.Update(datatype);
                            }

                            break;
                        }
                    case Constants.PropertyEditors.Aliases.MultiNodeTreePicker:
                        {
                            var config = JsonConvert.DeserializeObject<JObject>(datatype.Configuration);
                            var startNodeConfig = config.Value<JObject>("startNode");
                            if (startNodeConfig != null)
                            {
                                var startNodeId = startNodeConfig.Value<string>("id");
                                var objectType = startNodeConfig.Value<string>("type");
                                if (!objectType.IsNullOrWhiteSpace()
                                    && !startNodeId.IsNullOrWhiteSpace()
                                    && int.TryParse(startNodeId, out var intStartNode))
                                {
                                    var guid = intStartNode <= 0
                                        ? null
                                        : Context.Database.ExecuteScalar<Guid?>(
                                            Sql().Select<NodeDto>(x => x.UniqueId).From<NodeDto>().Where<NodeDto>(x => x.NodeId == intStartNode));

                                    string entityType = null;
                                    switch (objectType.ToLowerInvariant())
                                    {
                                        case "content":
                                            entityType = Constants.UdiEntityType.Document;
                                            break;
                                        case "media":
                                            entityType = Constants.UdiEntityType.Media;
                                            break;
                                        case "member":
                                            entityType = Constants.UdiEntityType.Member;
                                            break;
                                    }

                                    if (entityType != null && guid.HasValue)
                                    {
                                        var udi = new GuidUdi(entityType, guid.Value);
                                        startNodeConfig["id"] = new JValue(udi.ToString());
                                    }
                                    else
                                        startNodeConfig.Remove("id");

                                    datatype.Configuration = JsonConvert.SerializeObject(config);
                                    Database.Update(datatype);
                                }
                            }

                            break;
                        }
                }
            }

        }
    }
}
