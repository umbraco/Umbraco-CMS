using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Relators;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Querying
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class ContentTypeSqlMappingTests : BaseDatabaseFactoryTest
    {
        [Test]
        public void Can_Map_All_Property_Groups_And_Types()
        {
            using (var transaction = DatabaseContext.Database.GetTransaction())
            {
                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("umbracoNode"))));
                DatabaseContext.Database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 55555, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,55555", SortOrder = 1, UniqueId = new Guid("3390BDF4-C974-4211-AA95-3812A8CE7C46"), Text = "Test Data Type", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
                DatabaseContext.Database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 99999, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1,2222", SortOrder = 0, UniqueId = new Guid("129241F0-D24E-4FC3-92D1-BC2D48B7C431"), Text = "Test Content Type", NodeObjectType = new Guid(Constants.ObjectTypes.DocumentType), CreateDate = DateTime.Now });
                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("umbracoNode"))));

                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsDataType"))));
                DatabaseContext.Database.Insert("cmsDataType", "pk", false, new DataTypeDto { PrimaryKey = 44444, DataTypeId = 55555, PropertyEditorAlias = Constants.PropertyEditors.TextboxAlias, DbType = "Nvarchar" });
                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsDataType"))));
                
                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsContentType"))));
                DatabaseContext.Database.Insert("cmsContentType", "pk", false, new ContentTypeDto { PrimaryKey = 88888, NodeId = 99999, Alias = "TestContentType", Icon = "icon-folder", Thumbnail = "folder.png", IsContainer = false, AllowAtRoot = true });
                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsContentType"))));
                
                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsPropertyTypeGroup"))));
                DatabaseContext.Database.Insert("cmsPropertyTypeGroup", "id", false, new PropertyTypeGroupDto { Id = 77776, ContentTypeNodeId = 99999, Text = "Group1", SortOrder = 1 });
                DatabaseContext.Database.Insert("cmsPropertyTypeGroup", "id", false, new PropertyTypeGroupDto { Id = 77777, ContentTypeNodeId = 99999, Text = "Group2", SortOrder = 2 });
                DatabaseContext.Database.Insert("cmsPropertyTypeGroup", "id", false, new PropertyTypeGroupDto { Id = 77778, ContentTypeNodeId = 99999, Text = "Group3", SortOrder = 3 });
                DatabaseContext.Database.Insert("cmsPropertyTypeGroup", "id", false, new PropertyTypeGroupDto { Id = 77779, ContentTypeNodeId = 99999, Text = "Group4", SortOrder = 4 });
                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsPropertyTypeGroup"))));

                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsPropertyType"))));
                DatabaseContext.Database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 66662, DataTypeId = 55555, ContentTypeId = 99999, PropertyTypeGroupId = 77776, Alias = "property1", Name = "Property 1", SortOrder = 0, Mandatory = false, ValidationRegExp = null, Description = null });
                DatabaseContext.Database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 66663, DataTypeId = 55555, ContentTypeId = 99999, PropertyTypeGroupId = 77776, Alias = "property2", Name = "Property 2", SortOrder = 1, Mandatory = false, ValidationRegExp = null, Description = null });
                DatabaseContext.Database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 66664, DataTypeId = 55555, ContentTypeId = 99999, PropertyTypeGroupId = 77777, Alias = "property3", Name = "Property 3", SortOrder = 2, Mandatory = false, ValidationRegExp = null, Description = null });
                DatabaseContext.Database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 66665, DataTypeId = 55555, ContentTypeId = 99999, PropertyTypeGroupId = 77777, Alias = "property4", Name = "Property 4", SortOrder = 3, Mandatory = false, ValidationRegExp = null, Description = null });
                DatabaseContext.Database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 66666, DataTypeId = 55555, ContentTypeId = 99999, PropertyTypeGroupId = null, Alias = "property5", Name = "Property 5", SortOrder = 4, Mandatory = false, ValidationRegExp = null, Description = null });
                DatabaseContext.Database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 66667, DataTypeId = 55555, ContentTypeId = 99999, PropertyTypeGroupId = 77778, Alias = "property6", Name = "Property 6", SortOrder = 5, Mandatory = false, ValidationRegExp = null, Description = null });
                DatabaseContext.Database.Insert("cmsPropertyType", "id", false, new PropertyTypeDto { Id = 66668, DataTypeId = 55555, ContentTypeId = 99999, PropertyTypeGroupId = 77778, Alias = "property7", Name = "Property 7", SortOrder = 6, Mandatory = false, ValidationRegExp = null, Description = null });
                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsPropertyType"))));

                transaction.Complete();
            }

            PropertyTypeCollection propTypeCollection;
            PropertyGroupCollection propGroupCollection;
            ContentTypeRepository.ContentTypeQueries.PopulateGroupsAndProperties(99999, DatabaseContext.Database, out propTypeCollection, out propGroupCollection);

            Assert.AreEqual(4, propGroupCollection.Count);
            Assert.AreEqual(2, propGroupCollection["Group1"].PropertyTypes.Count);
            Assert.IsTrue(propGroupCollection["Group1"].PropertyTypes.Contains("property1"));
            Assert.IsTrue(propGroupCollection["Group1"].PropertyTypes.Contains("property2"));
            Assert.AreEqual(2, propGroupCollection["Group2"].PropertyTypes.Count);
            Assert.IsTrue(propGroupCollection["Group2"].PropertyTypes.Contains("property3"));
            Assert.IsTrue(propGroupCollection["Group2"].PropertyTypes.Contains("property4"));
            Assert.AreEqual(2, propGroupCollection["Group3"].PropertyTypes.Count);
            Assert.IsTrue(propGroupCollection["Group3"].PropertyTypes.Contains("property6"));
            Assert.IsTrue(propGroupCollection["Group3"].PropertyTypes.Contains("property7"));
            Assert.AreEqual(0, propGroupCollection["Group4"].PropertyTypes.Count);
            
            Assert.AreEqual(1, propTypeCollection.Count);
            Assert.IsTrue(propTypeCollection.Contains("property5"));

        }

    }
}
