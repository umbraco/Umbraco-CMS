using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0
{
    public class UpdatePickerIntegerValuesToUdi : MigrationBase
    {
        public UpdatePickerIntegerValuesToUdi(IMigrationContext context) : base(context)
        { }

        protected override void Migrate()
        {
            var sqlDataTypes = Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(x => x.EditorAlias == Cms.Core.Constants.PropertyEditors.Aliases.ContentPicker
                                         || x.EditorAlias == Cms.Core.Constants.PropertyEditors.Aliases.MediaPicker
                                         || x.EditorAlias == Cms.Core.Constants.PropertyEditors.Aliases.MultiNodeTreePicker);

            var dataTypes = Database.Fetch<DataTypeDto>(sqlDataTypes).ToList();

            foreach (var datatype in dataTypes.Where(x => !x.Configuration.IsNullOrWhiteSpace()))
            {
                switch (datatype.EditorAlias)
                {
                    case Cms.Core.Constants.PropertyEditors.Aliases.ContentPicker:
                    case Cms.Core.Constants.PropertyEditors.Aliases.MediaPicker:
                        {
                            var config = JsonConvert.DeserializeObject<JObject>(datatype.Configuration!);
                            var startNodeId = config!.Value<string>("startNodeId");
                            if (!startNodeId.IsNullOrWhiteSpace() && int.TryParse(startNodeId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intStartNode))
                            {
                                var guid = intStartNode <= 0
                                    ? null
                                    : Context.Database.ExecuteScalar<Guid?>(
                                        Sql().Select<NodeDto>(x => x.UniqueId).From<NodeDto>().Where<NodeDto>(x => x.NodeId == intStartNode));
                                if (guid.HasValue)
                                {
                                    var udi = new GuidUdi(datatype.EditorAlias == Cms.Core.Constants.PropertyEditors.Aliases.MediaPicker
                                        ? Cms.Core.Constants.UdiEntityType.Media
                                        : Cms.Core.Constants.UdiEntityType.Document, guid.Value);
                                    config!["startNodeId"] = new JValue(udi.ToString());
                                }
                                else
                                    config!.Remove("startNodeId");

                                datatype.Configuration = JsonConvert.SerializeObject(config);
                                Database.Update(datatype);
                            }

                            break;
                        }
                    case Cms.Core.Constants.PropertyEditors.Aliases.MultiNodeTreePicker:
                        {
                            var config = JsonConvert.DeserializeObject<JObject>(datatype.Configuration!);
                            var startNodeConfig = config!.Value<JObject>("startNode");
                            if (startNodeConfig != null)
                            {
                                var startNodeId = startNodeConfig.Value<string>("id");
                                var objectType = startNodeConfig.Value<string>("type");
                                if (!objectType.IsNullOrWhiteSpace()
                                    && !startNodeId.IsNullOrWhiteSpace()
                                    && int.TryParse(startNodeId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intStartNode))
                                {
                                    var guid = intStartNode <= 0
                                        ? null
                                        : Context.Database.ExecuteScalar<Guid?>(
                                            Sql().Select<NodeDto>(x => x.UniqueId).From<NodeDto>().Where<NodeDto>(x => x.NodeId == intStartNode));

                                    string? entityType = null;
                                    switch (objectType?.ToLowerInvariant())
                                    {
                                        case "content":
                                            entityType = Cms.Core.Constants.UdiEntityType.Document;
                                            break;
                                        case "media":
                                            entityType = Cms.Core.Constants.UdiEntityType.Media;
                                            break;
                                        case "member":
                                            entityType = Cms.Core.Constants.UdiEntityType.Member;
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
