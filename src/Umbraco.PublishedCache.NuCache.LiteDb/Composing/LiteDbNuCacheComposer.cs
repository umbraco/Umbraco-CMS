using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Core;
using Umbraco.Web.PublishedCache.NuCache;
using LiteDB;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.PublishedCache.NuCache.LiteDb.Composing
{
    [ComposeAfter(typeof(NuCacheComposer))]
    public class LiteDbNuCacheComposer : ComponentComposer<LiteDbNuCacheComponent>, ICoreComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.RegisterUnique<ITransactableDictionaryFactory, LiteDbTransactableDictionaryFactory>();

            BsonMapper.Global.RegisterType<ContentNodeKit>
              (
                  serialize: (cn) => SerializeContentNodeKit(cn),
                  deserialize: (cn) =>
                  {
                      return DeserializeToContentNodeKit(cn);
                  }
              );

        }
        private static BsonDocument SerializeContentNodeKit(ContentNodeKit cn)
        {
            var publishedProperties = MapProperties(cn.PublishedData);
            var unpublishedProperties = MapProperties(cn.DraftData);
            var publishedCultureInfos = MapCultureInfos(cn.PublishedData?.CultureInfos);
            var unpublishedCultureInfos = MapCultureInfos(cn.DraftData?.CultureInfos);
            return new BsonDocument(new Dictionary<string, BsonValue>
                    {
                        {"id", cn.Key },
                        {"contentTypeId", cn.ContentTypeId },
                        {
                            "contentNode", new BsonDocument
                            {
                                { "id", cn.Node.Id },
                                { "uid", cn.Node.Uid },
                                {"createDate", cn.Node.CreateDate },
                                {"level", cn.Node.Level },
                                {"path", cn.Node.Path },
                                {"name", cn.PublishedData.Name },
                                {"contentTypeAlias",cn.Node.ContentType.Alias },
                                {"creatorId",cn.Node.CreatorId },
                                { "parentContentId",cn.Node.ParentContentId },
                                { "firstChildContentId",cn.Node.FirstChildContentId },
                                { "nextSiblingContentId",cn.Node.NextSiblingContentId },
                                { "previousSiblingContentId",cn.Node.PreviousSiblingContentId },
                                { "sortOrder",cn.Node.SortOrder },
                                { "hasPublished",cn.Node.HasPublished }

                            }
                        },
                        {
                            "draftData", new BsonDocument
                            {
                                {"name",cn.DraftData?.Name },
                                {"published", cn.DraftData?.Published },
                                {"templateId",cn.DraftData?.TemplateId },
                                {"urlSegment",cn.DraftData?.UrlSegment },
                                { "versionDate", cn.DraftData?.VersionDate },
                                {"writerId",cn.DraftData?.WriterId },
                                {
                                    "properties", unpublishedProperties
                                },
                                {
                                    "cultureInfos", unpublishedCultureInfos
                                }

                            }
                        },
                        {
                            "publishedData", new BsonDocument
                            {
                                 {"name",cn.PublishedData?.Name },
                                {"published", cn.PublishedData?.Published },
                                {"templateId",cn.PublishedData?.TemplateId },
                                {"urlSegment",cn.PublishedData?.UrlSegment },
                                { "versionDate", cn.PublishedData?.VersionDate },
                                { "versionId", cn.PublishedData?.VersionId },
                                {"writerId",cn.PublishedData?.WriterId },
                                {
                                    "properties", publishedProperties
                                },
                                {

                                    "cultureInfos", publishedCultureInfos
                                }

                            }
                        }

                    });
        }

        private static BsonDocument MapCultureInfos(IReadOnlyDictionary<string, CultureVariation> cis)
        {
            var doc = new BsonDocument();
            if (cis == null)
            {
                return doc;
            }
            foreach (var item in cis)
            {
                doc.Add(item.Key, new BsonDocument
                {
                    {"name", item.Value.Name },
                     {"urlSegment", item.Value.UrlSegment },
                     {"date", item.Value.Date },
                     {"isDraft", item.Value.IsDraft }
                });
            }
            return doc;
        }

        private static BsonDocument MapProperties(IContentData cd)
        {
            var publishedProperties = new BsonDocument();
            if (cd == null)
            {
                return null;
            }
            foreach (var prop in cd.Properties)
            {
                publishedProperties.Add(prop.Key, MapPropertyVariants(prop));
            }

            return publishedProperties;
        }

        private static BsonArray MapPropertyVariants(KeyValuePair<string, PropertyData[]> prop)
        {
            var propVariants = new BsonArray();
            foreach (var propv in prop.Value)
            {
                propVariants.Add(new BsonDocument
                {
                    {"c", new BsonValue( propv.Culture )},
                      {"s", new BsonValue( propv.Segment )},
                        {"v", new BsonValue( propv.Value )}
                });
            }
            return propVariants;
        }

        private static ContentNodeKit DeserializeToContentNodeKit(BsonValue cn)
        {
            var cnk = cn.AsDocument;

            var cnn = cnk["contentNode"].AsDocument;
            var dd = cnk["draftData"].AsDocument;
            var pd = cnk["publishedData"].AsDocument;
            var contentNode = MapToContentNode(cnn);
            var publishedData = MapToContentData(pd);
            var draftData = MapToContentData(dd);

            return new ContentNodeKit()
            {
                ContentTypeId = cnk["contentTypeId"].AsInt32,
                Key = cnk["id"].AsInt32,
                DraftData = draftData,
                PublishedData = publishedData,
                Node = contentNode
            };
        }

        private static ContentNode MapToContentNode(BsonDocument cnn)
        {
            var cn = new ContentNode(cnn["id"].AsInt32,
                cnn["uid"]?.AsGuid ?? Guid.Empty,
                cnn["level"]?.AsInt32 ?? 0,
                cnn["path"]?.AsString,
                cnn["sortOrder"]?.AsInt32 ?? 0,
                cnn["parentContentId"]?.AsInt32 ?? 0,
                cnn["createDate"]?.AsDateTime ?? DateTime.MinValue,
                cnn["creatorId"]?.AsInt32 ?? 0)
                ;

            return cn;
        }

        private static ContentData MapToContentData(BsonDocument pd)
        {
            try
            {
                if (pd == null)
                {
                    return null;
                }
                var cd = new ContentData()
                {

                };
                var cultureInfos = MapBsonCultureInfos(pd);

                cd.CultureInfos = cultureInfos;
                cd.Properties = MapBsonProperties(pd);
                if (pd.TryGetValue("published", out var publishedB))
                {
                    cd.Published = publishedB.Type == BsonType.Null ? false : publishedB?.AsBoolean ?? false;
                }
                if (pd.TryGetValue("name", out var nameB))
                {
                    cd.Name = nameB.Type == BsonType.Null ? null : nameB?.AsString;
                }
                if (pd.TryGetValue("urlSegment", out var urlSegmentB))
                {
                    cd.UrlSegment = urlSegmentB.Type == BsonType.Null ? null : urlSegmentB?.AsString;
                }
                if (pd.TryGetValue("versionId", out var versionId))
                {
                    cd.VersionId = versionId.Type == BsonType.Null ? 0 : versionId?.AsInt32 ?? 0;
                }
                if (pd.TryGetValue("versionDate", out var versionDate))
                {
                    cd.VersionDate = versionDate.Type == BsonType.Null ? DateTime.MinValue : versionDate?.AsDateTime ?? DateTime.MinValue;
                }
                if (pd.TryGetValue("writerId", out var writerId))
                {
                    cd.WriterId = writerId.Type == BsonType.Null ? 0 : writerId?.AsInt32 ?? 0;
                }
                if (pd.TryGetValue("templateId", out var templateId))
                {
                    cd.TemplateId = templateId.Type == BsonType.Null ? 0 : templateId?.AsInt32;
                }
                return cd;

            }
            catch (Exception e)
            {
                return new ContentData();
            }
        }

        private static Dictionary<string, CultureVariation> MapBsonCultureInfos(BsonDocument pd)
        {
            var cvs = new Dictionary<string, CultureVariation>();

            if (pd.TryGetValue("cultureInfos", out var cultureInfoB))
            {
                if (cultureInfoB.Type == BsonType.Null)
                {
                    return cvs;
                }
                var cdoc = cultureInfoB.AsDocument;

                foreach (var key in cdoc.Keys)
                {
                    if (cdoc.TryGetValue(key, out var cvb))
                    {
                        if(cvb.Type == BsonType.Null)
                        {
                            cvs.Add(key, null);
                        }
                        var cvbDoc = cvb.AsDocument;
                        var cv = new CultureVariation();
                        if (cvbDoc.TryGetValue("name", out var nameB)){
                            cv.Name = nameB.AsString;
                        }
                        if (cvbDoc.TryGetValue("urlSegment", out var urlSegment))
                        {
                            cv.UrlSegment = urlSegment.AsString;
                        }
                        if (cvbDoc.TryGetValue("date", out var date))
                        {
                            cv.Date = date.AsDateTime;
                        }
                        if (cvbDoc.TryGetValue("isDraft", out var isDraft))
                        {
                            cv.IsDraft = isDraft.AsBoolean;
                        }
                    }
                }

            }
            return cvs;
        }

        private static Dictionary<string, PropertyData[]> MapBsonProperties(BsonDocument pd)
        {
            var props = new Dictionary<string, PropertyData[]>();
            if (pd.TryGetValue("properties", out var propertiesB))
            {
                var publishedProperties = propertiesB.AsDocument;
                if (publishedProperties == null || publishedProperties.Type == BsonType.Null)
                {
                    return props;
                }
                foreach (var pk in publishedProperties.Keys)
                {
                    if (publishedProperties.TryGetValue(pk, out var propVb))
                    {
                        if (propVb.Type == BsonType.Null)
                        {
                            props.Add(pk, null);
                            continue;
                        }
                        var pa = MapPropertyDataArrayFromBson(propVb);
                        props.Add(pk, pa.ToArray());
                    }
                }

                return props;
            }

            return props;
        }

        private static List<PropertyData> MapPropertyDataArrayFromBson(BsonValue propVb)
        {
            var array = propVb.AsArray;
            var pa = new List<PropertyData>(array.Count);
            foreach (var pvBd in array)
            {
                var propData = new PropertyData();
                var pvBdoc = pvBd.AsDocument;
                if (pvBdoc.TryGetValue("c", out var cb))
                {
                    if (cb.Type == BsonType.Null)
                    {
                        propData.Culture = null;
                    }
                    else
                    {
                        propData.Culture = cb?.AsString;
                    }
                }
                if (pvBdoc.TryGetValue("s", out var sb))
                {
                    if (sb.Type == BsonType.Null)
                    {
                        propData.Culture = null;
                    }
                    else
                    {
                        propData.Segment = sb?.AsString;
                    }
                }
                if (pvBdoc.TryGetValue("v", out var vb))
                {
                    if (vb.Type == BsonType.Null)
                    {
                        propData.Value = null;
                    }
                    else
                    {
                        MapPropertyValue(propData, vb);
                    }
                }
                pa.Add(propData);
            }

            return pa;
        }

        private static void MapPropertyValue(PropertyData propData, BsonValue vb)
        {
            switch (vb.Type)
            {
                case BsonType.MinValue:
                    break;
                case BsonType.Null:
                    propData.Value = null;
                    break;
                case BsonType.Int32:
                    propData.Value = vb?.AsInt32;
                    break;
                case BsonType.Int64:
                    propData.Value = vb?.AsInt64;
                    break;
                case BsonType.Double:
                    propData.Value = vb?.AsDouble;
                    break;
                case BsonType.Decimal:
                    propData.Value = vb?.AsDecimal;
                    break;
                case BsonType.String:
                    propData.Value = vb?.AsString;
                    break;
                case BsonType.Document:
                    propData.Value = vb?.AsDocument;
                    break;
                case BsonType.Array:
                    propData.Value = vb?.AsArray;
                    break;
                case BsonType.Binary:
                    propData.Value = vb?.AsString;
                    break;
                case BsonType.ObjectId:
                    propData.Value = vb?.AsObjectId;
                    break;
                case BsonType.Guid:
                    propData.Value = vb?.AsGuid;
                    break;
                case BsonType.Boolean:
                    propData.Value = vb?.AsBoolean;
                    break;
                case BsonType.DateTime:
                    propData.Value = vb?.AsDateTime;
                    break;
                case BsonType.MaxValue:
                    break;
                default:
                    break;
            }
        }
    }
}
