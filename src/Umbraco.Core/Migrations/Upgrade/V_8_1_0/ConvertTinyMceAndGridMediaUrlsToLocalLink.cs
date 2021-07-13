using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Migrations.Upgrade.V_8_0_0.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Services;

namespace Umbraco.Core.Migrations.Upgrade.V_8_1_0
{
    public class ConvertTinyMceAndGridMediaUrlsToLocalLink : MigrationBase
    {
        public ConvertTinyMceAndGridMediaUrlsToLocalLink(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            var mediaLinkPattern = new Regex(
                @"(<a[^>]*href="")(\/ media[^""\?]*)([^>]*>)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            var sqlPropertyData = Sql()
                .Select<PropertyDataDto80>(r => r.Select(x => x.PropertyTypeDto, r1 => r1.Select(x => x.DataTypeDto)))
                .From<PropertyDataDto80>()
                    .InnerJoin<PropertyTypeDto80>().On<PropertyDataDto80, PropertyTypeDto80>((left, right) => left.PropertyTypeId == right.Id)
                    .InnerJoin<DataTypeDto>().On<PropertyTypeDto80, DataTypeDto>((left, right) => left.DataTypeId == right.NodeId)
                .Where<DataTypeDto>(x =>
                    x.EditorAlias == Constants.PropertyEditors.Aliases.TinyMce ||
                    x.EditorAlias == Constants.PropertyEditors.Aliases.Grid);

            var properties = Database.Fetch<PropertyDataDto80>(sqlPropertyData);

            var exceptions = new List<Exception>();
            foreach (var property in properties)
            {
                var value = property.TextValue;
                if (string.IsNullOrWhiteSpace(value)) continue;


                bool propertyChanged = false;
                if (property.PropertyTypeDto.DataTypeDto.EditorAlias == Constants.PropertyEditors.Aliases.Grid)
                {
                    try
                    {
                        var obj = JsonConvert.DeserializeObject<JObject>(value);
                        var allControls = obj.SelectTokens("$.sections..rows..areas..controls");

                        foreach (var control in allControls.SelectMany(c => c).OfType<JObject>())
                        {
                            var controlValue = control["value"];
                            if (controlValue?.Type == JTokenType.String)
                            {
                                control["value"] = UpdateMediaUrls(mediaLinkPattern, controlValue.Value<string>(), out var controlChanged);
                                propertyChanged |= controlChanged;
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
                            $"Property info: Id = {property.Id}, LanguageId = {property.LanguageId}, VersionId = {property.VersionId}, Value = {property.Value}"
                            , e));
                        continue;
                    }

                }
                else
                {
                    property.TextValue = UpdateMediaUrls(mediaLinkPattern, value, out propertyChanged);
                }

                if (propertyChanged)
                    Database.Update(property);
            }


            if (exceptions.Any())
            {
                throw new AggregateException("One or more errors related to unexpected data in grid values occurred.", exceptions);
            }

            Context.AddPostMigration<RebuildPublishedSnapshot>();
        }

        private string UpdateMediaUrls(Regex mediaLinkPattern, string value, out bool changed)
        {
            bool matched = false;

            var result = mediaLinkPattern.Replace(value, match =>
            {
                matched = true;

                // match groups:
                // - 1 = from the beginning of the a tag until href attribute value begins
                // - 2 = the href attribute value excluding the querystring (if present)
                // - 3 = anything after group 2 until the a tag is closed
                var href = match.Groups[2].Value;

                var mediaUdi = GetMediaUdi(href, $"{match.Groups[1].Value}{match.Groups[3].Value}");
                return mediaUdi == null
                    ? match.Value
                    : $"{match.Groups[1].Value}/{{localLink:{mediaUdi}}}{match.Groups[3].Value}";
            });

            changed = matched;

            return result;
        }

        private static readonly Regex DataUdiPattern = new Regex(@"\bdata-udi=""([^""]+)""", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex ResizedPattern = new Regex(".*[_][0-9]+[x][0-9]+[.].*", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private Udi GetMediaUdi(string href, string otherTagContents)
        {
            var match = DataUdiPattern.Match(otherTagContents);
            if (match.Success && Udi.TryParse(match.Groups[1].Value, out var udi)) return udi;

            if (ResizedPattern.IsMatch(href))
            {
                var underscoreIndex = href.LastIndexOf('_');
                var dotIndex = href.LastIndexOf('.');
                href = string.Concat(href.Substring(0, underscoreIndex), href.Substring(dotIndex));
            }

            var udis = GetMediaUdis();
            return udis.TryGetValue(href, out udi) ? udi : null;
        }

        private Dictionary<string, Udi> _mediaUdis;
        private Dictionary<string, Udi> GetMediaUdis()
        {
            if (_mediaUdis != null) return _mediaUdis;

            var sql = Sql()
                .Select<MediaVersionDto>(r => r.Select(x => x.ContentVersionDto, x => x.Select(y => y.ContentDto, y => y.Select(z => z.NodeDto))))
                .From<MediaVersionDto>()
                .InnerJoin<ContentVersionDto>().On<MediaVersionDto, ContentVersionDto>(left => left.Id, right => right.Id)
                .InnerJoin<ContentDto>().On<ContentVersionDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                .InnerJoin<NodeDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Media)
                .Where<ContentVersionDto>(x => x.Current);

            var medias = Database.Fetch<MediaVersionDto>(sql).Where(m => !m.Path.IsNullOrWhiteSpace()).ToList();
            var udis = new Dictionary<string, Udi>(medias.Count, StringComparer.InvariantCultureIgnoreCase);

            medias.ForEach(m => udis[m.Path] = new GuidUdi(Constants.UdiEntityType.Media, m.ContentVersionDto.ContentDto.NodeDto.UniqueId));

            _mediaUdis = udis;

            return udis;
        }
    }
}
