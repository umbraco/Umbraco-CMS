using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;
using Umbraco.Core.Configuration;
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
                var propertyData = database.Fetch<PropertyDataDto>("WHERE propertyTypeId in (SELECT id from propertyType where dataTypeID = 21)");
                foreach (var data in propertyData)
                {
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
                            link.Add("caption", title);
                            link.Add("link", type.Equals("internal") ? null : lnk);
                            link.Add("newWindow", newwindow);
                            link.Add("internal", type.Equals("internal") ? lnk : null);
                            link.Add("edit", false);
                            link.Add("isInternal", type.Equals("internal"));

                            links.Add((ExpandoObject)link);
                        }

                        //store the serialized data
                        data.Text = new JavaScriptSerializer().Serialize(links);
                    }
                }
            }
            return string.Empty;
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
