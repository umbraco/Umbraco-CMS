using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using NUnit.Framework;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    /// <summary>
    /// Test with basic setup of an Sql CE database, which runs tests for the creation of all tables
    /// with their referenced dependencies. The creation isn't committed to the database or asserted, but
    /// the tests will fail if the creation isn't possible.
    /// </summary>
    [TestFixture]
    public class DatabaseExtensionsTest
    {
        [SetUp]
		public virtual void Initialize()
        {
            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            //Delete database file before continueing
            string filePath = string.Concat(path, "\\test.sdf");
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            //Get the connectionstring settings from config
            var settings = ConfigurationManager.ConnectionStrings["umbracoDbDsn"];

            //Create the Sql CE database
            var engine = new SqlCeEngine(settings.ConnectionString);
            engine.CreateDatabase();

            //Create the umbraco database
            //DatabaseFactory.Current.Database.Initialize();
        }

        [Test]
        public void Can_Create_umbracoNode_Table()
        {
            var factory = DatabaseFactory.Current;
            using(Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoApp_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<AppDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoAppTree_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<AppTreeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentType2ContentType_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<ContentType2ContentTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentTypeAllowedContentType_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<ContentTypeDto>();
                factory.Database.CreateTable<ContentTypeAllowedContentTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentType_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<ContentTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentVersion_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<ContentDto>();
                factory.Database.CreateTable<ContentVersionDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsContentXml_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<ContentDto>();
                factory.Database.CreateTable<ContentXmlDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDataType_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<DataTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDataTypePreValues_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<DataTypeDto>();
                factory.Database.CreateTable<DataTypePreValueDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDictionary_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<DictionaryDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsLanguageText_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<DictionaryDto>();
                factory.Database.CreateTable<LanguageTextDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTemplate_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<TemplateDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDocument_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<ContentDto>();
                factory.Database.CreateTable<TemplateDto>();
                factory.Database.CreateTable<DocumentDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsDocumentType_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<ContentTypeDto>();
                factory.Database.CreateTable<TemplateDto>();
                factory.Database.CreateTable<DocumentTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoDomains_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<DomainDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoLanguage_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<LanguageDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoLog_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<LogDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMacro_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<MacroDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMacroPropertyType_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<MacroPropertyTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMacroProperty_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<MacroDto>();
                factory.Database.CreateTable<MacroPropertyTypeDto>();
                factory.Database.CreateTable<MacroPropertyDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMember_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<ContentDto>();
                factory.Database.CreateTable<MemberDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMember2MemberGroup_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<ContentDto>();
                factory.Database.CreateTable<MemberDto>();
                factory.Database.CreateTable<Member2MemberGroupDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsMemberType_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<ContentTypeDto>();
                factory.Database.CreateTable<MemberTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPreviewXml_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<ContentDto>();
                factory.Database.CreateTable<ContentVersionDto>();
                factory.Database.CreateTable<PreviewXmlDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPropertyData_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<ContentTypeDto>();
                factory.Database.CreateTable<DataTypeDto>();
                factory.Database.CreateTable<PropertyTypeGroupDto>();
                factory.Database.CreateTable<PropertyTypeDto>();
                factory.Database.CreateTable<PropertyDataDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPropertyType_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<ContentTypeDto>();
                factory.Database.CreateTable<DataTypeDto>();
                factory.Database.CreateTable<PropertyTypeGroupDto>();
                factory.Database.CreateTable<PropertyTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsPropertyTypeGroup_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<ContentTypeDto>();
                factory.Database.CreateTable<PropertyTypeGroupDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoRelation_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<RelationTypeDto>();
                factory.Database.CreateTable<RelationDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoRelationType_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<RelationTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsStylesheet_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<StylesheetDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsStylesheetProperty_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<StylesheetPropertyDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTags_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<TagDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTagRelationship_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<TagDto>();
                factory.Database.CreateTable<TagRelationshipDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTask_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<UserTypeDto>();
                factory.Database.CreateTable<UserDto>();
                factory.Database.CreateTable<TaskTypeDto>();
                factory.Database.CreateTable<TaskDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_cmsTaskType_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<TaskTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUserLogins_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<UserLoginDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<UserTypeDto>();
                factory.Database.CreateTable<UserDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUserType_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<UserTypeDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser2app_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<UserTypeDto>();
                factory.Database.CreateTable<UserDto>();
                factory.Database.CreateTable<User2AppDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser2NodeNotify_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<UserTypeDto>();
                factory.Database.CreateTable<UserDto>();
                factory.Database.CreateTable<User2NodeNotifyDto>();

                //transaction.Complete();
            }
        }

        [Test]
        public void Can_Create_umbracoUser2NodePermission_Table()
        {
            var factory = DatabaseFactory.Current;
            using (Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();
                factory.Database.CreateTable<UserTypeDto>();
                factory.Database.CreateTable<UserDto>();
                factory.Database.CreateTable<User2NodePermissionDto>();

                //transaction.Complete();
            }
        }
    }
}