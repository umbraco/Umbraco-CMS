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
using Umbraco.Core.Profiling;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    public abstract class BaseTableByTableTest
    {
        private ILogger _logger;
        private DatabaseSchemaHelper _schemaHelper;

        public abstract Database Database { get; }
        
        protected abstract ISqlSyntaxProvider SqlSyntaxProvider { get; }

        protected DatabaseSchemaHelper DatabaseSchemaHelper
        {
            get { return _schemaHelper ?? (_schemaHelper = new DatabaseSchemaHelper(Database, _logger, SqlSyntaxProvider)); }
        }

        [SetUp]
        public virtual void Initialize()
        {
            _logger = new Logger(new FileInfo(TestHelper.MapPathForTest("~/unit-test-log4net.config")));
            
            TestHelper.InitializeContentDirectories();

            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            //disable cache
            var cacheHelper = CacheHelper.CreateDisabledCacheHelper();


            var dbContext = new DatabaseContext(
                new DefaultDatabaseFactory(GlobalSettings.UmbracoConnectionName, _logger),
                _logger, SqlSyntaxProvider, "System.Data.SqlServerCe.4.0");

            var repositoryFactory = new RepositoryFactory(cacheHelper, _logger, SqlSyntaxProvider, SettingsForTests.GenerateMockSettings());

            ApplicationContext.Current = new ApplicationContext(
                //assign the db context
                dbContext,
                //assign the service context
                new ServiceContext(repositoryFactory, new PetaPocoUnitOfWorkProvider(_logger), new FileUnitOfWorkProvider(), new PublishingStrategy(), cacheHelper, _logger),                
                cacheHelper,
                new ProfilingLogger(_logger, Mock.Of<IProfiler>()))
                {
                    IsReady = true
                };

            Resolution.Freeze();
        }

        [TearDown]
        public virtual void TearDown()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            //reset the app context
            ApplicationContext.Current = null;

            Resolution.Reset();
            //RepositoryResolver.Reset();
        }

        

        [Test]
        public void Can_Create_umbracoNode_Table()
        {
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoAccess_Table()
        {
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<AccessDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoAccessRule_Table()
        {
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<AccessDto>();
                DatabaseSchemaHelper.CreateTable<AccessRuleDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoApp_Table()
        {
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<AppDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoAppTree_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<AppTreeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentType2ContentType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<ContentType2ContentTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentTypeAllowedContentType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<ContentTypeDto>();
                DatabaseSchemaHelper.CreateTable<ContentTypeAllowedContentTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<ContentTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentVersion_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<ContentTypeDto>();
                DatabaseSchemaHelper.CreateTable<ContentDto>();
                DatabaseSchemaHelper.CreateTable<ContentVersionDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentXml_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<ContentTypeDto>();
                DatabaseSchemaHelper.CreateTable<ContentDto>();
                DatabaseSchemaHelper.CreateTable<ContentXmlDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDataType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<DataTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDataTypePreValues_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<DataTypeDto>();
                DatabaseSchemaHelper.CreateTable<DataTypePreValueDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDictionary_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<DictionaryDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsLanguageText_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<DictionaryDto>();
                DatabaseSchemaHelper.CreateTable<LanguageTextDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTemplate_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<TemplateDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDocument_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<ContentTypeDto>();
                DatabaseSchemaHelper.CreateTable<ContentDto>();
                DatabaseSchemaHelper.CreateTable<TemplateDto>();
                DatabaseSchemaHelper.CreateTable<DocumentDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDocumentType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<ContentTypeDto>();
                DatabaseSchemaHelper.CreateTable<TemplateDto>();
                DatabaseSchemaHelper.CreateTable<DocumentTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoDomains_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<DomainDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoLanguage_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<LanguageDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoLog_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<LogDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMacro_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<MacroDto>();

                //transaction.Complete();
            }
        }
        
        [Test]
        public void Can_Create_cmsMember_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<ContentTypeDto>();
                DatabaseSchemaHelper.CreateTable<ContentDto>();
                DatabaseSchemaHelper.CreateTable<MemberDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMember2MemberGroup_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<ContentTypeDto>();
                DatabaseSchemaHelper.CreateTable<ContentDto>();
                DatabaseSchemaHelper.CreateTable<MemberDto>();
                DatabaseSchemaHelper.CreateTable<Member2MemberGroupDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMemberType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<ContentTypeDto>();
                DatabaseSchemaHelper.CreateTable<MemberTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPreviewXml_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<ContentTypeDto>();
                DatabaseSchemaHelper.CreateTable<ContentDto>();
                DatabaseSchemaHelper.CreateTable<ContentVersionDto>();
                DatabaseSchemaHelper.CreateTable<PreviewXmlDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPropertyData_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<ContentTypeDto>();
                DatabaseSchemaHelper.CreateTable<DataTypeDto>();
                DatabaseSchemaHelper.CreateTable<PropertyTypeGroupDto>();
                DatabaseSchemaHelper.CreateTable<PropertyTypeDto>();
                DatabaseSchemaHelper.CreateTable<PropertyDataDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPropertyType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<ContentTypeDto>();
                DatabaseSchemaHelper.CreateTable<DataTypeDto>();
                DatabaseSchemaHelper.CreateTable<PropertyTypeGroupDto>();
                DatabaseSchemaHelper.CreateTable<PropertyTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPropertyTypeGroup_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<ContentTypeDto>();
                DatabaseSchemaHelper.CreateTable<PropertyTypeGroupDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoRelation_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<RelationTypeDto>();
                DatabaseSchemaHelper.CreateTable<RelationDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoRelationType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<RelationTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsStylesheet_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<StylesheetDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsStylesheetProperty_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<StylesheetPropertyDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTags_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<TagDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTagRelationship_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<ContentTypeDto>();
                DatabaseSchemaHelper.CreateTable<ContentDto>();
                DatabaseSchemaHelper.CreateTable<ContentTypeDto>();
                DatabaseSchemaHelper.CreateTable<DataTypeDto>();
                DatabaseSchemaHelper.CreateTable<PropertyTypeGroupDto>();
                DatabaseSchemaHelper.CreateTable<PropertyTypeDto>();
                DatabaseSchemaHelper.CreateTable<TagDto>();
                DatabaseSchemaHelper.CreateTable<TagRelationshipDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTask_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<UserTypeDto>();
                DatabaseSchemaHelper.CreateTable<UserDto>();
                DatabaseSchemaHelper.CreateTable<TaskTypeDto>();
                DatabaseSchemaHelper.CreateTable<TaskDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTaskType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<TaskTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUserLogins_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<UserLoginDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<UserTypeDto>();
                DatabaseSchemaHelper.CreateTable<UserDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUserType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<UserTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser2app_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<UserTypeDto>();
                DatabaseSchemaHelper.CreateTable<UserDto>();
                DatabaseSchemaHelper.CreateTable<User2AppDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser2NodeNotify_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<UserTypeDto>();
                DatabaseSchemaHelper.CreateTable<UserDto>();
                DatabaseSchemaHelper.CreateTable<User2NodeNotifyDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser2NodePermission_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                DatabaseSchemaHelper.CreateTable<NodeDto>();
                DatabaseSchemaHelper.CreateTable<UserTypeDto>();
                DatabaseSchemaHelper.CreateTable<UserDto>();
                DatabaseSchemaHelper.CreateTable<User2NodePermissionDto>();

                //transaction.Complete();
            }
        }
    }
}