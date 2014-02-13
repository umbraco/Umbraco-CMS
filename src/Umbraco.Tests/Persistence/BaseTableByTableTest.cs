using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
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
        [SetUp]
        public virtual void Initialize()
        {
            TestHelper.SetupLog4NetForTests();
            TestHelper.InitializeContentDirectories();

            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            RepositoryResolver.Current = new RepositoryResolver(new RepositoryFactory(true));

            //disable cache
            var cacheHelper = CacheHelper.CreateDisabledCacheHelper();

            ApplicationContext.Current = new ApplicationContext(
                //assign the db context
                new DatabaseContext(new DefaultDatabaseFactory()),
                //assign the service context
                new ServiceContext(new PetaPocoUnitOfWorkProvider(), new FileUnitOfWorkProvider(), new PublishingStrategy(), cacheHelper),                
                cacheHelper)
                {
                    IsReady = true
                };

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
                Database.CreateTable<NodeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoApp_Table()
        {
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<AppDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoAppTree_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<AppTreeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentType2ContentType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<ContentType2ContentTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentTypeAllowedContentType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<ContentTypeDto>();
                Database.CreateTable<ContentTypeAllowedContentTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<ContentTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentVersion_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<ContentDto>();
                Database.CreateTable<ContentVersionDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentXml_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<ContentDto>();
                Database.CreateTable<ContentXmlDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDataType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<DataTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDataTypePreValues_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<DataTypeDto>();
                Database.CreateTable<DataTypePreValueDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDictionary_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<DictionaryDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsLanguageText_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<DictionaryDto>();
                Database.CreateTable<LanguageTextDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTemplate_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<TemplateDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDocument_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<ContentDto>();
                Database.CreateTable<TemplateDto>();
                Database.CreateTable<DocumentDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDocumentType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<ContentTypeDto>();
                Database.CreateTable<TemplateDto>();
                Database.CreateTable<DocumentTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoDomains_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<DomainDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoLanguage_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<LanguageDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoLog_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<LogDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMacro_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<MacroDto>();

                //transaction.Complete();
            }
        }
        
        [Test]
        public void Can_Create_cmsMember_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<ContentDto>();
                Database.CreateTable<MemberDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMember2MemberGroup_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<ContentDto>();
                Database.CreateTable<MemberDto>();
                Database.CreateTable<Member2MemberGroupDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMemberType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<ContentTypeDto>();
                Database.CreateTable<MemberTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPreviewXml_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<ContentDto>();
                Database.CreateTable<ContentVersionDto>();
                Database.CreateTable<PreviewXmlDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPropertyData_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<ContentTypeDto>();
                Database.CreateTable<DataTypeDto>();
                Database.CreateTable<PropertyTypeGroupDto>();
                Database.CreateTable<PropertyTypeDto>();
                Database.CreateTable<PropertyDataDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPropertyType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<ContentTypeDto>();
                Database.CreateTable<DataTypeDto>();
                Database.CreateTable<PropertyTypeGroupDto>();
                Database.CreateTable<PropertyTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPropertyTypeGroup_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<ContentTypeDto>();
                Database.CreateTable<PropertyTypeGroupDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoRelation_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<RelationTypeDto>();
                Database.CreateTable<RelationDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoRelationType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<RelationTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsStylesheet_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<StylesheetDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsStylesheetProperty_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<StylesheetPropertyDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTags_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<TagDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTagRelationship_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<ContentDto>();
                Database.CreateTable<ContentTypeDto>();
                Database.CreateTable<DataTypeDto>();
                Database.CreateTable<PropertyTypeGroupDto>();
                Database.CreateTable<PropertyTypeDto>();
                Database.CreateTable<TagDto>();
                Database.CreateTable<TagRelationshipDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTask_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<UserTypeDto>();
                Database.CreateTable<UserDto>();
                Database.CreateTable<TaskTypeDto>();
                Database.CreateTable<TaskDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTaskType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<TaskTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUserLogins_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<UserLoginDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<UserTypeDto>();
                Database.CreateTable<UserDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUserType_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<UserTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser2app_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<UserTypeDto>();
                Database.CreateTable<UserDto>();
                Database.CreateTable<User2AppDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser2NodeNotify_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<UserTypeDto>();
                Database.CreateTable<UserDto>();
                Database.CreateTable<User2NodeNotifyDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser2NodePermission_Table()
        {
            
            using (Transaction transaction = Database.GetTransaction())
            {
                Database.CreateTable<NodeDto>();
                Database.CreateTable<UserTypeDto>();
                Database.CreateTable<UserDto>();
                Database.CreateTable<User2NodePermissionDto>();

                //transaction.Complete();
            }
        }
    }
}