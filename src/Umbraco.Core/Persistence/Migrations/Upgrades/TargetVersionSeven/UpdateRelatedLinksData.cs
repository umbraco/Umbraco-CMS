using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    [Migration("7.0.0", 10, GlobalSettings.UmbracoMigrationName)]
    public class UpdateRelatedLinksData : MigrationBase
    {
        public UpdateRelatedLinksData(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            Execute.Code(UpdateRelatedLinksDataDo);
        }

        public string UpdateRelatedLinksDataDo(Database database)
        {
            if (database != null)
            {
                var dtSql = new Sql().Select("nodeId").From<DataTypeDto>().Where<DataTypeDto>(dto => dto.PropertyEditorAlias == Constants.PropertyEditors.RelatedLinksAlias);
                var dataTypeIds = database.Fetch<int>(dtSql);

                if (!dataTypeIds.Any()) return string.Empty;

                var propertyData =
                    database.Fetch<PropertyDataDto>(
                        "WHERE propertyTypeId in (SELECT id from cmsPropertyType where dataTypeID IN (@dataTypeIds))", new { dataTypeIds = dataTypeIds });
                if (!propertyData.Any()) return string.Empty;

                var nodesIdsWithProperty = propertyData.Select(x => x.NodeId).Distinct().ToArray();

                var cmsContentXmlEntries = new List<ContentXmlDto>();
                //We're doing an "IN" query here but SQL server only supports 2100 query parameters so we're going to split on that
                // It would be better to do this as an INNER join + sub query but I don't have time for that at the moment and this is only run once
                // so it's not a big deal.
                var batches = nodesIdsWithProperty.InGroupsOf(2000);
                foreach (var batch in batches)
                {
                    cmsContentXmlEntries.AddRange(database.Fetch<ContentXmlDto>("WHERE nodeId in (@nodeIds)", new { nodeIds = batch }));
                }

                var propertyTypeIds = propertyData.Select(x => x.PropertyTypeId).Distinct();
                var propertyTypes = database.Fetch<PropertyTypeDto>(
                    "WHERE id in (@propertyTypeIds)", new { propertyTypeIds = propertyTypeIds });

                foreach (var data in propertyData)
                {
                    if (string.IsNullOrEmpty(data.Text) == false)
                    {
                        XmlDocument xml;
                        //fetch the current data (that's in xml format)
                        try
                        {
                            xml = new XmlDocument();
                            xml.LoadXml(data.Text);
                        }
                        catch (Exception ex) 
                        {
                            Logger.Error<UpdateRelatedLinksData>("The data stored for property id " + data.Id + " on document " + data.NodeId + 
                                " is not valid XML, the data will be removed because it cannot be converted to the new format. The value was: " + data.Text, ex);

                            data.Text = "";
                            database.Update(data);

                            UpdateXmlTable(propertyTypes, data, cmsContentXmlEntries, database);

                            continue;
                        }

                        var links = new List<ExpandoObject>();

                        //loop all the stored links
                        foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                        {
                            var title = node.Attributes["title"].Value;
                            var type = node.Attributes["type"].Value;
                            var newwindow = node.Attributes["newwindow"].Value.Equals("1") ? true : false;
                            var lnk = node.Attributes["link"].Value;

                            //create the links in the format the new prop editor expects it to be
                            var link = new ExpandoObject() as IDictionary<string, Object>;
                            link.Add("title", title);
                            link.Add("caption", title);
                            link.Add("link", lnk);
                            link.Add("newWindow", newwindow);
                            link.Add("type", type.Equals("internal") ? "internal" : "external");
                            link.Add("internal", type.Equals("internal") ? lnk : null);

                            link.Add("edit", false);
                            link.Add("isInternal", type.Equals("internal"));

                            links.Add((ExpandoObject)link);
                        }

                        //store the serialized data
                        data.Text = JsonConvert.SerializeObject(links);

                        database.Update(data);

                        UpdateXmlTable(propertyTypes, data, cmsContentXmlEntries, database);

                    }
                }
            }
            return string.Empty;
        }

        public override void Down()
        {
            throw new DataLossException("Cannot downgrade from a version 7 database to a prior version, the database schema has already been modified");
        }

        private static void UpdateXmlTable(List<PropertyTypeDto> propertyTypes, PropertyDataDto data, List<ContentXmlDto> cmsContentXmlEntries, Database database)
        {
            //now we need to update the cmsContentXml table
            var propertyType = propertyTypes.SingleOrDefault(x => x.Id == data.PropertyTypeId);
            if (propertyType != null)
            {
                var xmlItem = cmsContentXmlEntries.SingleOrDefault(x => x.NodeId == data.NodeId);
                if (xmlItem != null)
                {
                    var x = XElement.Parse(xmlItem.Xml);
                    var prop = x.Element(propertyType.Alias);
                    if (prop != null)
                    {
                        prop.ReplaceAll(new XCData(data.Text));
                        database.Update(xmlItem);
                    }
                }
            }
        }
    }
}
