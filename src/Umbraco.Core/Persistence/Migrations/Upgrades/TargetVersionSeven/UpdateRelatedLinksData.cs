﻿using System;
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
                var dtSql = new Sql().Select("nodeId")
                    .From<DataTypeDto>(SqlSyntax)
                    .Where<DataTypeDto>(dto => dto.PropertyEditorAlias == Constants.PropertyEditors.RelatedLinksAlias);

                var dataTypeIds = database.Fetch<int>(dtSql);
                if (dataTypeIds.Any() == false) return string.Empty;

                // need to use dynamic, as PropertyDataDto has new properties (eg decimal...) in further versions that don't exist yet
                var propertyData = database.Fetch<dynamic>("SELECT * FROM cmsPropertyData"
                        + " WHERE propertyTypeId in (SELECT id from cmsPropertyType where dataTypeID IN (@dataTypeIds))", new { /*dataTypeIds =*/ dataTypeIds });
                if (propertyData.Any() == false) return string.Empty;

                var nodesIdsWithProperty = propertyData.Select(x => (int) x.contentNodeId).Distinct().ToArray();

                var cmsContentXmlEntries = new List<ContentXmlDto>();
                //We're doing an "IN" query here but SQL server only supports 2100 query parameters so we're going to split on that
                // It would be better to do this as an INNER join + sub query but I don't have time for that at the moment and this is only run once
                // so it's not a big deal.
                var batches = nodesIdsWithProperty.InGroupsOf(2000);
                foreach (var batch in batches)
                {
                    cmsContentXmlEntries.AddRange(database.Fetch<ContentXmlDto>("WHERE nodeId in (@nodeIds)", new { nodeIds = batch }));
                }

                var propertyTypeIds = propertyData.Select(x => (int) x.propertytypeid).Distinct();

                //NOTE: We are writing the full query because we've added a column to the PropertyTypeDto in later versions so one of the columns
                // won't exist yet
                var propertyTypes = database.Fetch<PropertyTypeDto>("SELECT * FROM cmsPropertyType WHERE id in (@propertyTypeIds)", new { propertyTypeIds = propertyTypeIds });

                foreach (var data in propertyData)
                {
                    if (string.IsNullOrEmpty(data.dataNtext) == false)
                    {
                        XmlDocument xml;
                        //fetch the current data (that's in xml format)
                        try
                        {
                            xml = new XmlDocument();
                            xml.LoadXml(data.dataNtext);
                        }
                        catch (Exception ex)
                        {
                            int dataId = data.id;
                            int dataNodeId = data.contentNodeId;
                            string dataText = data.dataNtext;
                            Logger.Error<UpdateRelatedLinksData>("The data stored for property id " + dataId + " on document " + dataNodeId +
                                " is not valid XML, the data will be removed because it cannot be converted to the new format. The value was: " + dataText, ex);

                            data.dataNtext = "";
                            database.Update("cmsPropertyData", "id", data, new[] { "dataNText" });

                            UpdateXmlTable(propertyTypes, data, cmsContentXmlEntries, database);

                            continue;
                        }

                        var links = new List<ExpandoObject>();

                        //loop all the stored links
                        foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                        {
                            var title = node.Attributes["title"].Value;
                            var type = node.Attributes["type"].Value;
                            var newwindow = node.Attributes["newwindow"].Value.Equals("1");
                            var lnk = node.Attributes["link"].Value;

                            //create the links in the format the new prop editor expects it to be
                            var link = new ExpandoObject() as IDictionary<string, object>;
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
                        data.dataNtext = JsonConvert.SerializeObject(links);

                        database.Update("cmsPropertyData", "id", data, new[] { "dataNText" });

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

        private static void UpdateXmlTable(List<PropertyTypeDto> propertyTypes, dynamic data, List<ContentXmlDto> cmsContentXmlEntries, Database database)
        {
            //now we need to update the cmsContentXml table
            var propertyType = propertyTypes.SingleOrDefault(x => x.Id == data.propertytypeid);
            if (propertyType != null)
            {
                var xmlItem = cmsContentXmlEntries.SingleOrDefault(x => x.NodeId == data.contentNodeId);
                if (xmlItem != null)
                {
                    var x = XElement.Parse(xmlItem.Xml);
                    var prop = x.Element(propertyType.Alias);
                    if (prop != null)
                    {
                        prop.ReplaceAll(new XCData(data.dataNtext));
                        database.Update(xmlItem);
                    }
                }
            }
        }
    }
}
