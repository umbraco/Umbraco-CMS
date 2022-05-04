using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.PostMigrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_1_0;

public class ConvertTinyMceAndGridMediaUrlsToLocalLink : MigrationBase
{
    private readonly IMediaService _mediaService;

    public ConvertTinyMceAndGridMediaUrlsToLocalLink(IMigrationContext context, IMediaService mediaService)
        : base(context) => _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));

    protected override void Migrate()
    {
        var mediaLinkPattern = new Regex(
            @"(<a[^>]*href="")(\/ media[^""\?]*)([^>]*>)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        Sql<ISqlContext> sqlPropertyData = Sql()
            .Select<PropertyDataDto80>(r => r.Select(x => x.PropertyTypeDto, r1 => r1.Select(x => x!.DataTypeDto)))
            .From<PropertyDataDto80>()
            .InnerJoin<PropertyTypeDto80>()
            .On<PropertyDataDto80, PropertyTypeDto80>((left, right) => left.PropertyTypeId == right.Id)
            .InnerJoin<DataTypeDto>()
            .On<PropertyTypeDto80, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId)
            .Where<DataTypeDto>(x =>
                x.EditorAlias == Constants.PropertyEditors.Aliases.TinyMce ||
                x.EditorAlias == Constants.PropertyEditors.Aliases.Grid);

        List<PropertyDataDto80>? properties = Database.Fetch<PropertyDataDto80>(sqlPropertyData);

        var exceptions = new List<Exception>();
        foreach (PropertyDataDto80? property in properties)
        {
            var value = property.TextValue;
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var propertyChanged = false;
            if (property.PropertyTypeDto?.DataTypeDto?.EditorAlias == Constants.PropertyEditors.Aliases.Grid)
            {
                try
                {
                    JObject? obj = JsonConvert.DeserializeObject<JObject>(value);
                    IEnumerable<JToken>? allControls = obj?.SelectTokens("$.sections..rows..areas..controls");

                    if (allControls is not null)
                    {
                        foreach (JObject control in allControls.SelectMany(c => c).OfType<JObject>())
                        {
                            JToken? controlValue = control["value"];
                            if (controlValue?.Type == JTokenType.String)
                            {
                                control["value"] = UpdateMediaUrls(mediaLinkPattern, controlValue.Value<string>()!,
                                    out var controlChanged);
                                propertyChanged |= controlChanged;
                            }
                        }
                    }

                    property.TextValue = JsonConvert.SerializeObject(obj);
                }
                catch (JsonException e)
                {
                    exceptions.Add(new InvalidOperationException(
                        "Cannot deserialize the value as json. This can be because the property editor " +
                        "type is changed from another type into a grid. Old versions of the value in this " +
                        "property can have the structure from the old property editor type. This needs to be " +
                        "changed manually before updating the database.\n" +
                        $"Property info: Id = {property.Id}, LanguageId = {property.LanguageId}, VersionId = {property.VersionId}, Value = {property.Value}",
                        e));
                    continue;
                }
            }
            else
            {
                property.TextValue = UpdateMediaUrls(mediaLinkPattern, value, out propertyChanged);
            }

            if (propertyChanged)
            {
                Database.Update(property);
            }
        }

        if (exceptions.Any())
        {
            throw new AggregateException(
                "One or more errors related to unexpected data in grid values occurred.",
                exceptions);
        }

        Context.AddPostMigration<RebuildPublishedSnapshot>();
    }

    private string UpdateMediaUrls(Regex mediaLinkPattern, string value, out bool changed)
    {
        var matched = false;

        var result = mediaLinkPattern.Replace(value, match =>
        {
            matched = true;

            // match groups:
            // - 1 = from the beginning of the a tag until href attribute value begins
            // - 2 = the href attribute value excluding the querystring (if present)
            // - 3 = anything after group 2 until the a tag is closed
            var href = match.Groups[2].Value;

            IMedia? media = _mediaService.GetMediaByPath(href);
            return media == null
                ? match.Value
                : $"{match.Groups[1].Value}/{{localLink:{media.GetUdi()}}}{match.Groups[3].Value}";
        });

        changed = matched;

        return result;
    }
}
