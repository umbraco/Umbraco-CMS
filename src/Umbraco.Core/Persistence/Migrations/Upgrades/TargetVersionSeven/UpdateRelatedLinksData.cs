using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;
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
                try
                {
                    var propertyData =
                        database.Fetch<PropertyDataDto>(
                            "WHERE propertyTypeId in (SELECT id from cmsPropertyType where dataTypeID = 1040)");
                    foreach (var data in propertyData)
                    {
                        if (!string.IsNullOrEmpty(data.Text))
                        {
                             //var cs = ApplicationContext.Current.Services.ContentService;

                            //fetch the current data (that's in xml format)
                            var xml = new XmlDocument();
                            xml.LoadXml(data.Text);

                            if (xml != null)
                            {
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

                                    //try
                                    //{
                                    //    if (type.Equals("internal"))
                                    //    {
                                    //        int nodeId;
                                    //        if (int.TryParse(lnk, out nodeId))
                                    //            link.Add("internalName", cs.GetById(nodeId).Name);
                                    //    }
                                    //}
                                    //catch (Exception ex)
                                    //{
                                    //    LogHelper.Error<UpdateRelatedLinksData>("Exception was thrown when trying to update related links property data, fetching internal node id", ex);
                                    //}

                                    links.Add((ExpandoObject) link);
                                }

                                //store the serialized data
                                data.Text = JsonConvert.SerializeObject(links);

                                database.Update(data);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error<UpdateRelatedLinksData>("Exception was thrown when trying to update related links property data", ex);
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
