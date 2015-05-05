using System;
using System.Collections.Generic;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixTwoZero
{
    [Migration("7.1.0", 4, GlobalSettings.UmbracoMigrationName)]
    [Migration("6.2.0", 4, GlobalSettings.UmbracoMigrationName)]
    public class UpdateToNewMemberPropertyAliases : MigrationBase
    {
        public UpdateToNewMemberPropertyAliases(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            Execute.Code(Update);
        }

        internal string Update(Database database)
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


                //This query is structured to work with MySql, SQLCE and SqlServer:
                // http://issues.umbraco.org/issue/U4-3876

                const string propertyTypeUpdateSql = @"UPDATE cmsPropertyType
SET Alias = @newAlias
WHERE Alias = @oldAlias AND contentTypeId IN (
SELECT nodeId FROM (SELECT DISTINCT cmsContentType.nodeId FROM cmsPropertyType
INNER JOIN cmsContentType ON cmsPropertyType.contentTypeId = cmsContentType.nodeId
INNER JOIN umbracoNode ON cmsContentType.nodeId = umbracoNode.id
WHERE umbracoNode.nodeObjectType = @objectType) x)";

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
                        var items = database.Fetch<ContentXmlDto>(xmlSelectSql, new { objectType = Constants.ObjectTypes.Member });
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
                        Logger.Error<UpdateToNewMemberPropertyAliases>("Exception was thrown when trying to upgrade old member aliases to the new ones", ex);
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
