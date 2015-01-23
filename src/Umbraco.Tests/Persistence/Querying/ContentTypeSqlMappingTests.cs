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
        public void Can_Map_Content_Type_Templates_And_Allowed_Types()
        {
            using (var transaction = DatabaseContext.Database.GetTransaction())
            {
                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("umbracoNode"))));
                DatabaseContext.Database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 55554, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,55554", SortOrder = 1, UniqueId = new Guid("87D1EAB6-AB27-4852-B3DF-DE8DBA4A1AA0"), Text = "Template 1", NodeObjectType = new Guid(Constants.ObjectTypes.Template), CreateDate = DateTime.Now });
                DatabaseContext.Database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 55555, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,55555", SortOrder = 1, UniqueId = new Guid("3390BDF4-C974-4211-AA95-3812A8CE7C46"), Text = "Template 2", NodeObjectType = new Guid(Constants.ObjectTypes.Template), CreateDate = DateTime.Now });
                DatabaseContext.Database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 99997, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1,99997", SortOrder = 0, UniqueId = new Guid("BB3241D5-6842-4EFA-A82A-5F56885CF528"), Text = "Test Content Type 1", NodeObjectType = new Guid(Constants.ObjectTypes.DocumentType), CreateDate = DateTime.Now });
                DatabaseContext.Database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 99998, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1,99998", SortOrder = 0, UniqueId = new Guid("EEA66B06-302E-49BA-A8B2-EDF07248BC59"), Text = "Test Content Type 2", NodeObjectType = new Guid(Constants.ObjectTypes.DocumentType), CreateDate = DateTime.Now });
                DatabaseContext.Database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 99999, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1,99999", SortOrder = 0, UniqueId = new Guid("C45CC083-BB27-4C1C-B448-6F703CC9B799"), Text = "Test Content Type 2", NodeObjectType = new Guid(Constants.ObjectTypes.DocumentType), CreateDate = DateTime.Now });
                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("umbracoNode"))));

                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsTemplate"))));
                DatabaseContext.Database.Insert("cmsTemplate", "pk", false, new TemplateDto { NodeId = 55554, Alias = "testTemplate1", Design = "", PrimaryKey = 22221});
                DatabaseContext.Database.Insert("cmsTemplate", "pk", false, new TemplateDto { NodeId = 55555, Alias = "testTemplate2", Design = "", PrimaryKey = 22222 });
                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsTemplate"))));

                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsContentType"))));
                DatabaseContext.Database.Insert("cmsContentType", "pk", false, new ContentTypeDto { PrimaryKey = 88887, NodeId = 99997, Alias = "TestContentType1", Icon = "icon-folder", Thumbnail = "folder.png", IsContainer = false, AllowAtRoot = true });
                DatabaseContext.Database.Insert("cmsContentType", "pk", false, new ContentTypeDto { PrimaryKey = 88888, NodeId = 99998, Alias = "TestContentType2", Icon = "icon-folder", Thumbnail = "folder.png", IsContainer = false, AllowAtRoot = true });
                DatabaseContext.Database.Insert("cmsContentType", "pk", false, new ContentTypeDto { PrimaryKey = 88889, NodeId = 99999, Alias = "TestContentType3", Icon = "icon-folder", Thumbnail = "folder.png", IsContainer = false, AllowAtRoot = true });
                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsContentType"))));

                DatabaseContext.Database.Insert(new DocumentTypeDto { ContentTypeNodeId = 99997, IsDefault = true, TemplateNodeId = 55555 });
                DatabaseContext.Database.Insert(new DocumentTypeDto { ContentTypeNodeId = 99997, IsDefault = false, TemplateNodeId = 55554 });

                DatabaseContext.Database.Insert(new ContentTypeAllowedContentTypeDto { AllowedId = 99998, Id = 99997, SortOrder = 1 });
                DatabaseContext.Database.Insert(new ContentTypeAllowedContentTypeDto { AllowedId = 99999, Id = 99997, SortOrder = 2});

                DatabaseContext.Database.Insert(new ContentType2ContentTypeDto { ChildId = 99999, ParentId = 99997});
                DatabaseContext.Database.Insert(new ContentType2ContentTypeDto { ChildId = 99998, ParentId = 99997 });
                
                transaction.Complete();
            }

            IDictionary<int, IEnumerable<ContentTypeRepository.ContentTypeQueryMapper.AssociatedTemplate>> allAssociatedTemplates;
            IDictionary<int, IEnumerable<int>> allParentContentTypeIds;
            var contentTypes = ContentTypeRepository.ContentTypeQueryMapper.MapContentTypes(
                new[] {99997, 99998}, DatabaseContext.Database, SqlSyntax, out allAssociatedTemplates, out allParentContentTypeIds)
                .ToArray();

            var contentType1 = contentTypes.SingleOrDefault(x => x.Id == 99997);
            Assert.IsNotNull(contentType1);

            var associatedTemplates1 = allAssociatedTemplates[contentType1.Id];
            var parentContentTypes1 = allParentContentTypeIds[contentType1.Id];

            Assert.AreEqual(2, contentType1.AllowedContentTypes.Count());
            Assert.AreEqual(2, associatedTemplates1.Count());
            Assert.AreEqual(0, parentContentTypes1.Count());

            var contentType2 = contentTypes.SingleOrDefault(x => x.Id == 99998);
            Assert.IsNotNull(contentType2);

            var associatedTemplates2 = allAssociatedTemplates[contentType2.Id];
            var parentContentTypes2 = allParentContentTypeIds[contentType2.Id];

            Assert.AreEqual(0, contentType2.AllowedContentTypes.Count());
            Assert.AreEqual(0, associatedTemplates2.Count());
            Assert.AreEqual(1, parentContentTypes2.Count());

        }

        [Test]
        public void Can_Map_Media_Type_And_Allowed_Types()
        {
            using (var transaction = DatabaseContext.Database.GetTransaction())
            {
                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("umbracoNode"))));                
                DatabaseContext.Database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 99997, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1,99997", SortOrder = 0, UniqueId = new Guid("BB3241D5-6842-4EFA-A82A-5F56885CF528"), Text = "Test Media Type 1", NodeObjectType = new Guid(Constants.ObjectTypes.MediaType), CreateDate = DateTime.Now });
                DatabaseContext.Database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 99998, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1,99998", SortOrder = 0, UniqueId = new Guid("EEA66B06-302E-49BA-A8B2-EDF07248BC59"), Text = "Test Media Type 2", NodeObjectType = new Guid(Constants.ObjectTypes.MediaType), CreateDate = DateTime.Now });
                DatabaseContext.Database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 99999, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1,99999", SortOrder = 0, UniqueId = new Guid("C45CC083-BB27-4C1C-B448-6F703CC9B799"), Text = "Test Media Type 2", NodeObjectType = new Guid(Constants.ObjectTypes.MediaType), CreateDate = DateTime.Now });
                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("umbracoNode"))));

                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsContentType"))));
                DatabaseContext.Database.Insert("cmsContentType", "pk", false, new ContentTypeDto { PrimaryKey = 88887, NodeId = 99997, Alias = "TestContentType1", Icon = "icon-folder", Thumbnail = "folder.png", IsContainer = false, AllowAtRoot = true });
                DatabaseContext.Database.Insert("cmsContentType", "pk", false, new ContentTypeDto { PrimaryKey = 88888, NodeId = 99998, Alias = "TestContentType2", Icon = "icon-folder", Thumbnail = "folder.png", IsContainer = false, AllowAtRoot = true });
                DatabaseContext.Database.Insert("cmsContentType", "pk", false, new ContentTypeDto { PrimaryKey = 88889, NodeId = 99999, Alias = "TestContentType3", Icon = "icon-folder", Thumbnail = "folder.png", IsContainer = false, AllowAtRoot = true });
                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsContentType"))));

                DatabaseContext.Database.Insert(new ContentTypeAllowedContentTypeDto { AllowedId = 99998, Id = 99997, SortOrder = 1 });
                DatabaseContext.Database.Insert(new ContentTypeAllowedContentTypeDto { AllowedId = 99999, Id = 99997, SortOrder = 2 });

                DatabaseContext.Database.Insert(new ContentType2ContentTypeDto { ChildId = 99999, ParentId = 99997 });
                DatabaseContext.Database.Insert(new ContentType2ContentTypeDto { ChildId = 99998, ParentId = 99997 });

                transaction.Complete();
            }

            IDictionary<int, IEnumerable<int>> allParentContentTypeIds;
            var contentTypes = ContentTypeRepository.ContentTypeQueryMapper.MapMediaTypes(
                new[] { 99997, 99998 }, DatabaseContext.Database, SqlSyntax, out allParentContentTypeIds)
                .ToArray();

            var contentType1 = contentTypes.SingleOrDefault(x => x.Id == 99997);
            Assert.IsNotNull(contentType1);

            var parentContentTypes1 = allParentContentTypeIds[contentType1.Id];

            Assert.AreEqual(2, contentType1.AllowedContentTypes.Count());
            Assert.AreEqual(0, parentContentTypes1.Count());

            var contentType2 = contentTypes.SingleOrDefault(x => x.Id == 99998);
            Assert.IsNotNull(contentType2);

            var parentContentTypes2 = allParentContentTypeIds[contentType2.Id];

            Assert.AreEqual(0, contentType2.AllowedContentTypes.Count());
            Assert.AreEqual(1, parentContentTypes2.Count());

        }

        [Test]
        public void Can_Map_All_Property_Groups_And_Types()
        {
            using (var transaction = DatabaseContext.Database.GetTransaction())
            {
                DatabaseContext.Database.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("umbracoNode"))));
                DatabaseContext.Database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 55555, Trashed = false, ParentId = -1, UserId = 0, Level = 1, Path = "-1,55555", SortOrder = 1, UniqueId = new Guid("3390BDF4-C974-4211-AA95-3812A8CE7C46"), Text = "Test Data Type", NodeObjectType = new Guid(Constants.ObjectTypes.DataType), CreateDate = DateTime.Now });
                DatabaseContext.Database.Insert("umbracoNode", "id", false, new NodeDto { NodeId = 99999, Trashed = false, ParentId = -1, UserId = 0, Level = 0, Path = "-1,99999", SortOrder = 0, UniqueId = new Guid("129241F0-D24E-4FC3-92D1-BC2D48B7C431"), Text = "Test Content Type", NodeObjectType = new Guid(Constants.ObjectTypes.DocumentType), CreateDate = DateTime.Now });
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

            IDictionary<int, PropertyTypeCollection> allPropTypeCollection;
            IDictionary<int, PropertyGroupCollection> allPropGroupCollection;
            ContentTypeRepository.ContentTypeQueryMapper.MapGroupsAndProperties(new[] { 99999 }, DatabaseContext.Database, SqlSyntax, out allPropTypeCollection, out allPropGroupCollection);

            var propGroupCollection = allPropGroupCollection[99999];
            var propTypeCollection = allPropTypeCollection[99999];

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
