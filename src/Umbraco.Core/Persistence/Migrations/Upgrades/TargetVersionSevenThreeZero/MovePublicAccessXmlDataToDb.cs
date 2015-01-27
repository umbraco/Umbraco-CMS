using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 7, GlobalSettings.UmbracoMigrationName)]
    public class MovePublicAccessXmlDataToDb : MigrationBase
    {


        public override void Up()
        {
            //don't run if data is already there.
            var dataExists = Context.Database.Fetch<AccessDto>(new Sql().Select("*").From<AccessDto>(SqlSyntax));
            if (dataExists.Any()) return;

            var xmlFile = IOHelper.MapPath(SystemFiles.AccessXml);
            using (var fileStream = File.OpenRead(xmlFile))
            {
                var xml = XDocument.Load(fileStream);

                foreach (var page in xml.Root.Elements("page"))
                {
                    var pageId = (int?) page.Attribute("id");
                    var loginPageId = (int?) page.Attribute("loginPage");
                    var noRightsPageId = (int?)page.Attribute("noRightsPage");
                    if (pageId.HasValue == false || loginPageId.HasValue == false || noRightsPageId.HasValue == false) 
                        continue;

                    //ensure this page exists!
                    var umbracoNode = Context.Database.FirstOrDefault<NodeDto>("WHERE id = @Id", new {Id = pageId.Value});
                    if (umbracoNode != null)
                    {
                        var loginNode = Context.Database.FirstOrDefault<NodeDto>("WHERE id = @Id", new { Id = loginPageId.Value });
                        if (loginNode != null)
                        {
                            var noRightsPage = Context.Database.FirstOrDefault<NodeDto>("WHERE id = @Id", new { Id = noRightsPageId.Value });
                            if (noRightsPage != null)
                            {
                                var accessId = Guid.NewGuid();
                                Insert.IntoTable("umbracoAccess").Row(new
                                {
                                    id = accessId,
                                    nodeId = umbracoNode.UniqueId,
                                    loginNodeId = loginNode.UniqueId,
                                    noAccessNodeId = noRightsPage.UniqueId,
                                    createDate = DateTime.Now,
                                    updateDate = DateTime.Now
                                });

                                //if a memberId has been specified, then add that as a rule
                                var memberId = (string) page.Attribute("memberId");
                                if (memberId.IsNullOrWhiteSpace() == false)
                                {
                                    Insert.IntoTable("umbracoAccessRule").Row(new
                                    {
                                        id = Guid.NewGuid(),
                                        accessId = accessId,
                                        claim = memberId,
                                        claimType = Constants.Conventions.PublicAccess.MemberIdClaimType,
                                        createDate = DateTime.Now,
                                        updateDate = DateTime.Now
                                    });
                                }

                                //now there should be a member group defined here
                                var memberGroupElement = page.Element("group");
                                if (memberGroupElement != null)
                                {
                                    var memberGroup = (string)memberGroupElement.Attribute("id");
                                    if (memberGroup.IsNullOrWhiteSpace() == false)
                                    {
                                        //create a member group rule
                                        Insert.IntoTable("umbracoAccessRule").Row(new
                                        {
                                            id = Guid.NewGuid(),
                                            accessId = accessId,
                                            claim = memberGroup,
                                            claimType = Constants.Conventions.PublicAccess.MemberGroupClaimType,
                                            createDate = DateTime.Now,
                                            updateDate = DateTime.Now
                                        });
                                    }
                                }

                            }
                            else
                            {
                                Logger.Warn<AddPublicAccessTables>("No umbracoNode could be found with id " + noRightsPageId.Value);
                            }
                        }
                        else
                        {
                            Logger.Warn<AddPublicAccessTables>("No umbracoNode could be found with id " + loginPageId.Value);
                        }
                            
                    }
                    else
                    {
                        Logger.Warn<AddPublicAccessTables>("No umbracoNode could be found with id " + pageId.Value);
                    }
                }

            }

        }

        public override void Down()
        {
        }
    }
}