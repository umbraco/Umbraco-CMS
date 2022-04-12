using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class SqlServerTableByTableTest : UmbracoIntegrationTest
    {
        private IUmbracoVersion UmbracoVersion => GetRequiredService<IUmbracoVersion>();
        private static ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;
        private IEventAggregator EventAggregator => GetRequiredService<IEventAggregator>();

        [Test]
        public void Can_Create_umbracoNode_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = CreateDatabaseSchemaCreator(scope);

                helper.CreateTable<NodeDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoAccess_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentTypeDto>();
                helper.CreateTable<ContentDto>();
                helper.CreateTable<ContentVersionDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDataType_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

                helper.CreateTable<DictionaryDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsLanguageText_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

                helper.CreateTable<NodeDto>();
                helper.CreateTable<DomainDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoLogViewerQuery_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = CreateDatabaseSchemaCreator(scope);

                helper.CreateTable<LogViewerQueryDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoLanguage_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = CreateDatabaseSchemaCreator(scope);

                helper.CreateTable<LanguageDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoLog_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = CreateDatabaseSchemaCreator(scope);

                helper.CreateTable<LogDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMacro_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = CreateDatabaseSchemaCreator(scope);

                helper.CreateTable<MacroDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMember_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

                helper.CreateTable<NodeDto>();
                helper.CreateTable<ContentTypeDto>();
                helper.CreateTable<MemberPropertyTypeDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_PropertyData_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

                helper.CreateTable<RelationTypeDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTags_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = CreateDatabaseSchemaCreator(scope);

                helper.CreateTable<TagDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTagRelationship_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

                helper.CreateTable<UserDto>();

                scope.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUserGroup_Table()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

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
                var helper = CreateDatabaseSchemaCreator(scope);

                helper.CreateTable<NodeDto>();
                helper.CreateTable<UserGroupDto>();
                helper.CreateTable<UserGroup2NodePermissionDto>();

                scope.Complete();
            }
        }

        private DatabaseSchemaCreator CreateDatabaseSchemaCreator(IScope scope) =>
            new DatabaseSchemaCreator(scope.Database, _loggerFactory.CreateLogger<DatabaseSchemaCreator>(), _loggerFactory, UmbracoVersion, EventAggregator, Mock.Of<IOptionsMonitor<InstallDefaultDataSettings>>(x => x.CurrentValue == new InstallDefaultDataSettings()));
    }
}
