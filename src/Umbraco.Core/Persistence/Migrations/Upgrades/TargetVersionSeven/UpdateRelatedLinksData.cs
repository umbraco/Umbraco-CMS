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

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    [Migration("7.0.0", 10, GlobalSettings.UmbracoMigrationName)]
    public class UpdateRelatedLinksData : MigrationBase
    {
        public override void Up()
        {
            Execute.Code(UpdateRelatedLinksDataDo);
        }

        public static string UpdateRelatedLinksDataDo(Database database)
        {
            if (database != null)
            {
                var dtSql = new Sql().Select("nodeId").From<DataTypeDto>().Where<DataTypeDto>(dto => dto.PropertyEditorAlias == Constants.PropertyEditors.RelatedLinksAlias);
                var dataTypeIds = database.Fetch<int>(dtSql);

                var propertyData =
                    database.Fetch<PropertyDataDto>(
                        "WHERE propertyTypeId in (SELECT id from cmsPropertyType where dataTypeID IN (@dataTypeIds))", new { dataTypeIds = dataTypeIds });
                if (!propertyData.Any()) return string.Empty;
                
                var nodesIdsWithProperty = propertyData.Select(x => x.NodeId).Distinct();
                var cmsContentXmlEntries = database.Fetch<ContentXmlDto>(
                        "WHERE nodeId in (@nodeIds)", new { nodeIds = nodesIdsWithProperty });
                var propertyTypeIds = propertyData.Select(x => x.PropertyTypeId).Distinct();
                var propertyTypes = database.Fetch<PropertyTypeDto>(
                    "WHERE id in (@propertyTypeIds)", new { propertyTypeIds = propertyTypeIds });

                foreach (var data in propertyData)
                {
                    if (string.IsNullOrEmpty(data.Text) == false)
                    {
                        //fetch the current data (that's in xml format)
                        var xml = new XmlDocument();
                        xml.LoadXml(data.Text);

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
            return string.Empty;
        }

        public override void Down()
        {
            throw new DataLossException("Cannot downgrade from a version 7 database to a prior version, the database schema has already been modified");
        }
    }
}
