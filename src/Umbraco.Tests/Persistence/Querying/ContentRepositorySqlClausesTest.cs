using System;
using NUnit.Framework;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.Persistence.Querying
{
    [TestFixture]
    public class ContentRepositorySqlClausesTest
    {
         public void Can_Verify_Base_Clause()
         {
             var NodeObjectType = new Guid("");

             var sql = new Sql();
             sql.Select("*")
                 .From("cmsDocument")
                 .InnerJoin("cmsContentVersion").On("[cmsDocument].[versionId] = [cmsContentVersion].[VersionId]")
                 .InnerJoin("cmsContent").On("[cmsContentVersion].[ContentId] = [cmsContent].[nodeId]")
                 .InnerJoin("umbracoNode").On("[cmsContent].[nodeId] = [umbracoNode].[id]")
                 .Where("[umbracoNode].[nodeObjectType] = @NodeObjectType", new { NodeObjectType });
         }
    }
}