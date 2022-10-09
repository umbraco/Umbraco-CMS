using System.Globalization;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class ConvertRelatedLinksToMultiUrlPicker : MigrationBase
{
    public ConvertRelatedLinksToMultiUrlPicker(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        Sql<ISqlContext> sqlDataTypes = Sql()
            .Select<DataTypeDto>()
            .From<DataTypeDto>()
            .Where<DataTypeDto>(x => x.EditorAlias == Constants.PropertyEditors.Legacy.Aliases.RelatedLinks
                                     || x.EditorAlias == Constants.PropertyEditors.Legacy.Aliases.RelatedLinks2);

        List<DataTypeDto>? dataTypes = Database.Fetch<DataTypeDto>(sqlDataTypes);
        var dataTypeIds = dataTypes.Select(x => x.NodeId).ToList();

        if (dataTypeIds.Count == 0)
        {
            return;
        }

        foreach (DataTypeDto? dataType in dataTypes)
        {
            dataType.EditorAlias = Constants.PropertyEditors.Aliases.MultiUrlPicker;
            Database.Update(dataType);
        }

        Sql<ISqlContext> sqlPropertyTpes = Sql()
            .Select<PropertyTypeDto80>()
            .From<PropertyTypeDto80>()
            .Where<PropertyTypeDto80>(x => dataTypeIds.Contains(x.DataTypeId));

        var propertyTypeIds = Database.Fetch<PropertyTypeDto80>(sqlPropertyTpes).Select(x => x.Id).ToList();

        if (propertyTypeIds.Count == 0)
        {
            return;
        }

        Sql<ISqlContext> sqlPropertyData = Sql()
            .Select<PropertyDataDto>()
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => propertyTypeIds.Contains(x.PropertyTypeId));

        List<PropertyDataDto>? properties = Database.Fetch<PropertyDataDto>(sqlPropertyData);

        // Create a Multi URL Picker datatype for the converted RelatedLinks data
        foreach (PropertyDataDto? property in properties)
        {
            var value = property.Value?.ToString();
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            List<RelatedLink>? relatedLinks = JsonConvert.DeserializeObject<List<RelatedLink>>(value);
            var links = new List<LinkDto>();
            if (relatedLinks is null)
            {
                return;
            }

            foreach (RelatedLink relatedLink in relatedLinks)
            {
                GuidUdi? udi = null;
                if (relatedLink.IsInternal)
                {
                    var linkIsUdi = UdiParser.TryParse(relatedLink.Link, out udi);
                    if (linkIsUdi == false)
                    {
                        // oh no.. probably an integer, yikes!
                        if (int.TryParse(relatedLink.Link, NumberStyles.Integer, CultureInfo.InvariantCulture,
                                out var intId))
                        {
                            Sql<ISqlContext> sqlNodeData = Sql()
                                .Select<NodeDto>()
                                .From<NodeDto>()
                                .Where<NodeDto>(x => x.NodeId == intId);

                            NodeDto? node = Database.Fetch<NodeDto>(sqlNodeData).FirstOrDefault();
                            if (node != null)

                            // Note: RelatedLinks did not allow for picking media items,
                            // so if there's a value this will be a content item - hence
                            // the hardcoded "document" here
                            {
                                udi = new GuidUdi("document", node.UniqueId);
                            }
                        }
                    }
                }

                var link = new LinkDto
                {
                    Name = relatedLink.Caption,
                    Target = relatedLink.NewWindow ? "_blank" : null,
                    Udi = udi,

                    // Should only have a URL if it's an external link otherwise it wil be a UDI
                    Url = relatedLink.IsInternal == false ? relatedLink.Link : null,
                };

                links.Add(link);
            }

            var json = JsonConvert.SerializeObject(links);

            // Update existing data
            property.TextValue = json;
            Database.Update(property);
        }
    }
}

internal class RelatedLink
{
    public int? Id { get; internal set; }

    [JsonProperty("caption")]
    public string? Caption { get; set; }

    internal bool IsDeleted { get; set; }

    [JsonProperty("link")]
    public string? Link { get; set; }

    [JsonProperty("newWindow")]
    public bool NewWindow { get; set; }

    [JsonProperty("isInternal")]
    public bool IsInternal { get; set; }
}

[DataContract]
internal class LinkDto
{
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    [DataMember(Name = "target")]
    public string? Target { get; set; }

    [DataMember(Name = "udi")]
    public GuidUdi? Udi { get; set; }

    [DataMember(Name = "url")]
    public string? Url { get; set; }
}
