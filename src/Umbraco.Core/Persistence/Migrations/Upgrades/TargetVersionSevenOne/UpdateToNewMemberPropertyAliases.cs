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

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenOne
{
    [Migration("7.1.0", 1, GlobalSettings.UmbracoMigrationName)]
    public class UpdateToNewMemberPropertyAliases : MigrationBase
    {
        public override void Up()
        {
            Execute.Code(Update);
        }

        internal static string Update(Database database)
        {
            if (database != null)
            {
                var aliasMap = new Dictionary<string, string>
                {
                    {"umbracoPasswordRetrievalQuestionPropertyTypeAlias", Constants.Conventions.Member.PasswordQuestion},
                    {"umbracoPasswordRetrievalAnswerPropertyTypeAlias",   Constants.Conventions.Member.PasswordAnswer},
                    {"umbracoCommentPropertyTypeAlias",                   Constants.Conventions.Member.Comments},
                    {"umbracoApprovePropertyTypeAlias",                   Constants.Conventions.Member.IsApproved},
                    {"umbracoLockPropertyTypeAlias",                      Constants.Conventions.Member.IsLockedOut},
                    {"umbracoLastLoginPropertyTypeAlias",                 Constants.Conventions.Member.LastLoginDate},
                    {"umbracoMemberLastPasswordChange",                   Constants.Conventions.Member.LastPasswordChangeDate},
                    {"umbracoMemberLastLockout",                          Constants.Conventions.Member.LastLockoutDate},
                    {"umbracoFailedPasswordAttemptsPropertyTypeAlias",    Constants.Conventions.Member.FailedPasswordAttempts}
                };

                const string propertyTypeUpdateSql = @"UPDATE cmsPropertyType
SET Alias = @newAlias
WHERE Alias = @oldAlias AND contentTypeId IN (
SELECT DISTINCT cmsContentType.nodeId FROM cmsPropertyType
INNER JOIN cmsContentType ON cmsPropertyType.contentTypeId = cmsContentType.nodeId
INNER JOIN umbracoNode ON cmsContentType.nodeId = umbracoNode.id
WHERE umbracoNode.nodeObjectType = @objectType)";

                const string xmlSelectSql = @"SELECT cmsContentXml.* FROM cmsContentXml 
INNER JOIN umbracoNode ON cmsContentXml.nodeId = umbracoNode.id
WHERE umbracoNode.nodeObjectType = @objectType";

                using (var trans = database.GetTransaction())
                {
                    try
                    {

                        //Upate all of the property type aliases
                        foreach (var map in aliasMap)
                        {
                            database.Execute(propertyTypeUpdateSql, new { newAlias = map.Value, oldAlias = map.Key, objectType = Constants.ObjectTypes.MemberType });
                        }

                        //Update all of the XML
                        var items = database.Fetch<ContentXmlDto>(xmlSelectSql, new {objectType = Constants.ObjectTypes.Member});
                        foreach (var item in items)
                        {
                            foreach (var map in aliasMap)
                            {
                                item.Xml = item.Xml.Replace("<" + map.Key + ">", "<" + map.Value + ">");
                                item.Xml = item.Xml.Replace("</" + map.Key + ">", "</" + map.Value + ">");
                            }
                            database.Update(item);
                        }

                        trans.Complete();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<UpdateRelatedLinksData>("Exception was thrown when trying to upgrade old member aliases to the new ones", ex);
                        throw;
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
