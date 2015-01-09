using System;
using System.IO;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    public abstract class BaseTableByTableTest
    {
        private ILogger _logger;
        private DatabaseSchemaHelper _schemaHelper;

        [SetUp]
        public virtual void Initialize()
        {
            _logger = new Logger(new FileInfo(TestHelper.MapPathForTest("~/unit-test-log4net.config")));
            
            TestHelper.InitializeContentDirectories();

            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);
            
            RepositoryResolver.Current = new RepositoryResolver(new RepositoryFactory(true,
                _logger,
                Mock.Of<IUmbracoSettingsSection>()));

            //disable cache
            var cacheHelper = CacheHelper.CreateDisabledCacheHelper();

            var dbContext = new DatabaseContext(new DefaultDatabaseFactory());

            ApplicationContext.Current = new ApplicationContext(
                //assign the db context
                dbContext,
                //assign the service context
                new ServiceContext(new PetaPocoUnitOfWorkProvider(), new FileUnitOfWorkProvider(), new PublishingStrategy(), cacheHelper, _logger),                
                cacheHelper,
                new Logger(new FileInfo(TestHelper.MapPathForTest("~/unit-test-log4net.config"))))
                {
                    IsReady = true
                };

            _schemaHelper = new DatabaseSchemaHelper(dbContext.Database, _logger, new SqlCeSyntaxProvider());

            Resolution.Freeze();
        }

        [TearDown]
        public virtual void TearDown()
        {
            SqlSyntaxContext.SqlSyntaxProvider = null;
            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            //reset the app context
            ApplicationContext.Current = null;

            RepositoryResolver.Reset();
        }

        public abstract Database Database { get; }

        [Test]
        public void Can_Create_umbracoNode_Table()
        {
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoApp_Table()
        {
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<AppDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoAppTree_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<AppTreeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentType2ContentType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<ContentType2ContentTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentTypeAllowedContentType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<ContentTypeDto>();
                _schemaHelper.CreateTable<ContentTypeAllowedContentTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<ContentTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentVersion_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<ContentTypeDto>();
                _schemaHelper.CreateTable<ContentDto>();
                _schemaHelper.CreateTable<ContentVersionDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentXml_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<ContentTypeDto>();
                _schemaHelper.CreateTable<ContentDto>();
                _schemaHelper.CreateTable<ContentXmlDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDataType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<DataTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDataTypePreValues_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<DataTypeDto>();
                _schemaHelper.CreateTable<DataTypePreValueDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDictionary_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<DictionaryDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsLanguageText_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<DictionaryDto>();
                _schemaHelper.CreateTable<LanguageTextDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTemplate_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<TemplateDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDocument_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<ContentTypeDto>();
                _schemaHelper.CreateTable<ContentDto>();
                _schemaHelper.CreateTable<TemplateDto>();
                _schemaHelper.CreateTable<DocumentDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDocumentType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<ContentTypeDto>();
                _schemaHelper.CreateTable<TemplateDto>();
                _schemaHelper.CreateTable<DocumentTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoDomains_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<DomainDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoLanguage_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<LanguageDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoLog_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<LogDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMacro_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<MacroDto>();

                //transaction.Complete();
            }
        }
        
        [Test]
        public void Can_Create_cmsMember_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<ContentTypeDto>();
                _schemaHelper.CreateTable<ContentDto>();
                _schemaHelper.CreateTable<MemberDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMember2MemberGroup_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<ContentTypeDto>();
                _schemaHelper.CreateTable<ContentDto>();
                _schemaHelper.CreateTable<MemberDto>();
                _schemaHelper.CreateTable<Member2MemberGroupDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMemberType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<ContentTypeDto>();
                _schemaHelper.CreateTable<MemberTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPreviewXml_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<ContentTypeDto>();
                _schemaHelper.CreateTable<ContentDto>();
                _schemaHelper.CreateTable<ContentVersionDto>();
                _schemaHelper.CreateTable<PreviewXmlDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPropertyData_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<ContentTypeDto>();
                _schemaHelper.CreateTable<DataTypeDto>();
                _schemaHelper.CreateTable<PropertyTypeGroupDto>();
                _schemaHelper.CreateTable<PropertyTypeDto>();
                _schemaHelper.CreateTable<PropertyDataDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPropertyType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<ContentTypeDto>();
                _schemaHelper.CreateTable<DataTypeDto>();
                _schemaHelper.CreateTable<PropertyTypeGroupDto>();
                _schemaHelper.CreateTable<PropertyTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPropertyTypeGroup_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<ContentTypeDto>();
                _schemaHelper.CreateTable<PropertyTypeGroupDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoRelation_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<RelationTypeDto>();
                _schemaHelper.CreateTable<RelationDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoRelationType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<RelationTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsStylesheet_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<StylesheetDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsStylesheetProperty_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<StylesheetPropertyDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTags_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<TagDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTagRelationship_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<ContentTypeDto>();
                _schemaHelper.CreateTable<ContentDto>();
                _schemaHelper.CreateTable<ContentTypeDto>();
                _schemaHelper.CreateTable<DataTypeDto>();
                _schemaHelper.CreateTable<PropertyTypeGroupDto>();
                _schemaHelper.CreateTable<PropertyTypeDto>();
                _schemaHelper.CreateTable<TagDto>();
                _schemaHelper.CreateTable<TagRelationshipDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTask_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<UserTypeDto>();
                _schemaHelper.CreateTable<UserDto>();
                _schemaHelper.CreateTable<TaskTypeDto>();
                _schemaHelper.CreateTable<TaskDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTaskType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<TaskTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUserLogins_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<UserLoginDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<UserTypeDto>();
                _schemaHelper.CreateTable<UserDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUserType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<UserTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser2app_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<UserTypeDto>();
                _schemaHelper.CreateTable<UserDto>();
                _schemaHelper.CreateTable<User2AppDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser2NodeNotify_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<UserTypeDto>();
                _schemaHelper.CreateTable<UserDto>();
                _schemaHelper.CreateTable<User2NodeNotifyDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser2NodePermission_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                _schemaHelper.CreateTable<NodeDto>();
                _schemaHelper.CreateTable<UserTypeDto>();
                _schemaHelper.CreateTable<UserDto>();
                _schemaHelper.CreateTable<User2NodePermissionDto>();

                //transaction.Complete();
            }
        }
    }
}