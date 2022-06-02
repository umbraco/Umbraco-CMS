using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class UpdatePickerIntegerValuesToUdi : MigrationBase
{
    public UpdatePickerIntegerValuesToUdi(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        Sql<ISqlContext> sqlDataTypes = Sql()
            .Select<DataTypeDto>()
            .From<DataTypeDto>()
            .Where<DataTypeDto>(x => x.EditorAlias == Constants.PropertyEditors.Aliases.ContentPicker
                                     || x.EditorAlias == Constants.PropertyEditors.Aliases.MediaPicker
                                     || x.EditorAlias == Constants.PropertyEditors.Aliases.MultiNodeTreePicker);

        var dataTypes = Database.Fetch<DataTypeDto>(sqlDataTypes).ToList();

        foreach (DataTypeDto? datatype in dataTypes.Where(x => !x.Configuration.IsNullOrWhiteSpace()))
        {
            switch (datatype.EditorAlias)
            {
                case Constants.PropertyEditors.Aliases.ContentPicker:
                case Constants.PropertyEditors.Aliases.MediaPicker:
                {
                    JObject? config = JsonConvert.DeserializeObject<JObject>(datatype.Configuration!);
                    var startNodeId = config!.Value<string>("startNodeId");
                    if (!startNodeId.IsNullOrWhiteSpace() && int.TryParse(startNodeId, NumberStyles.Integer,
                            CultureInfo.InvariantCulture, out var intStartNode))
                    {
                        Guid? guid = intStartNode <= 0
                            ? null
                            : Context.Database.ExecuteScalar<Guid?>(
                                Sql().Select<NodeDto>(x => x.UniqueId).From<NodeDto>()
                                    .Where<NodeDto>(x => x.NodeId == intStartNode));
                        if (guid.HasValue)
                        {
                            var udi = new GuidUdi(
                                datatype.EditorAlias == Constants.PropertyEditors.Aliases.MediaPicker
                                ? Constants.UdiEntityType.Media
                                : Constants.UdiEntityType.Document, guid.Value);
                            config!["startNodeId"] = new JValue(udi.ToString());
                        }
                        else
                        {
                            config!.Remove("startNodeId");
                        }

                        datatype.Configuration = JsonConvert.SerializeObject(config);
                        Database.Update(datatype);
                    }

                    break;
                }

                case Constants.PropertyEditors.Aliases.MultiNodeTreePicker:
                {
                    JObject? config = JsonConvert.DeserializeObject<JObject>(datatype.Configuration!);
                    JObject? startNodeConfig = config!.Value<JObject>("startNode");
                    if (startNodeConfig != null)
                    {
                        var startNodeId = startNodeConfig.Value<string>("id");
                        var objectType = startNodeConfig.Value<string>("type");
                        if (!objectType.IsNullOrWhiteSpace()
                            && !startNodeId.IsNullOrWhiteSpace()
                            && int.TryParse(startNodeId, NumberStyles.Integer, CultureInfo.InvariantCulture,
                                out var intStartNode))
                        {
                            Guid? guid = intStartNode <= 0
                                ? null
                                : Context.Database.ExecuteScalar<Guid?>(
                                    Sql().Select<NodeDto>(x => x.UniqueId).From<NodeDto>()
                                        .Where<NodeDto>(x => x.NodeId == intStartNode));

                            string? entityType = null;
                            switch (objectType?.ToLowerInvariant())
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
                            {
                                startNodeConfig.Remove("id");
                            }

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
