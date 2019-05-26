using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Tests.LegacyXmlPublishedCache;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class SqlCeTableByTableTest : TestWithDatabaseBase
    {
        [Test]
        public void Can_Create_umbracoNode_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoAccess_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<AccessDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoAccessRule_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<AccessDto>();
                helper.CreateTable<AccessRuleDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentType2ContentType_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentType2ContentTypeDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentTypeAllowedContentType_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentTypeDto>();
                helper.CreateTable<ContentTypeAllowedContentTypeDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentType_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentTypeDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_ContentVersion_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentTypeDto>();
                helper.CreateTable<ContentDto>();
                helper.CreateTable<ContentVersionDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentXml_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentTypeDto>();
                helper.CreateTable<ContentDto>();
                helper.CreateTable<ContentXmlDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDataType_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<DataTypeDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDictionary_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<DictionaryDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsLanguageText_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<DictionaryDto>();
                helper.CreateTable<LanguageDto>();
                helper.CreateTable<LanguageTextDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTemplate_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<TemplateDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_Document_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentTypeDto>();
                helper.CreateTable<ContentDto>();
                helper.CreateTable<TemplateDto>();
                helper.CreateTable<DocumentDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_DocumentType_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentTypeDto>();
                helper.CreateTable<TemplateDto>();
                helper.CreateTable<ContentTypeTemplateDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoDomains_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<DomainDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoLanguage_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<LanguageDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoLog_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<LogDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMacro_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<MacroDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMember_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentTypeDto>();
                helper.CreateTable<ContentDto>();
                helper.CreateTable<MemberDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMember2MemberGroup_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentTypeDto>();
                helper.CreateTable<ContentDto>();
                helper.CreateTable<MemberDto>();
                helper.CreateTable<Member2MemberGroupDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMemberType_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentTypeDto>();
                helper.CreateTable<MemberPropertyTypeDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPreviewXml_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentTypeDto>();
                helper.CreateTable<ContentDto>();
                helper.CreateTable<ContentVersionDto>();
                helper.CreateTable<PreviewXmlDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_PropertyData_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentTypeDto>();
                helper.CreateTable<DataTypeDto>();
                helper.CreateTable<PropertyTypeGroupDto>();
                helper.CreateTable<PropertyTypeDto>();
                helper.CreateTable<PropertyDataDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPropertyType_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentTypeDto>();
                helper.CreateTable<DataTypeDto>();
                helper.CreateTable<PropertyTypeGroupDto>();
                helper.CreateTable<PropertyTypeDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPropertyTypeGroup_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentTypeDto>();
                helper.CreateTable<PropertyTypeGroupDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoRelation_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<RelationTypeDto>();
                helper.CreateTable<RelationDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoRelationType_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<RelationTypeDto>();

                scope.Complete();
            }
        }
        
        [Test]
        public void Can_Create_cmsTags_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<TagDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTagRelationship_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentTypeDto>();
                helper.CreateTable<ContentDto>();
                helper.CreateTable<ContentTypeDto>();
                helper.CreateTable<DataTypeDto>();
                helper.CreateTable<PropertyTypeGroupDto>();
                helper.CreateTable<PropertyTypeDto>();
                helper.CreateTable<TagDto>();
                helper.CreateTable<TagRelationshipDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<UserDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUserGroup_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<UserGroupDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser2NodeNotify_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<UserDto>();
                helper.CreateTable<User2NodeNotifyDto>();

                scope.Complete();
            }
        }

        public void Can_Create_umbracoGroupUser2app_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<UserGroupDto>();
                helper.CreateTable<UserGroup2AppDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUserGroup2NodePermission_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<UserGroupDto>();
                helper.CreateTable<UserGroup2NodePermissionDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUserGroup2ContentTemplate_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = new DatabaseSchemaCreator(scope.Database, Mock.Of<ILogger>());

                helper.CreateTable<NodeDto>();
                helper.CreateTable<UserGroupDto>();
                helper.CreateTable<UserGroup2ContentTemplateDto>();

                scope.Complete();
            }
        }
    }
}
