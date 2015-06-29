using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class TemplateRepositoryTest : BaseDatabaseFactoryTest
    {
        private IFileSystem _masterPageFileSystem;
        private IFileSystem _viewsFileSystem;

        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            _masterPageFileSystem = new PhysicalFileSystem(SystemDirectories.Masterpages);
            _viewsFileSystem = new PhysicalFileSystem(SystemDirectories.MvcViews);
        }

        [Test]
        public void Can_Instantiate_Repository_From_Resolver()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            using (var repository = RepositoryResolver.Current.ResolveByType<ITemplateRepository>(unitOfWork))
            {

                // Assert
                Assert.That(repository, Is.Not.Null);    
            }

        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem))
            {

                // Assert
                Assert.That(repository, Is.Not.Null);    
            }

        }

        [Test]
        public void Can_Perform_Add_MasterPage()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem))
            {
                // Act
                var template = new Template("test-add-masterpage.master", "test", "test") { Content = @"<%@ Master Language=""C#"" %>" };
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                //Assert
                Assert.That(repository.Get("test"), Is.Not.Null);
                Assert.That(_masterPageFileSystem.FileExists("test.master"), Is.True);    
            }
            
        }

        [Test]
        public void Can_Perform_Update_MasterPage()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem))
            {
                // Act
                var template = new Template("test-updated-masterpage.master", "test", "test") { Content = @"<%@ Master Language=""C#"" %>" };
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                template.Content = @"<%@ Master Language=""VB"" %>";
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                var updated = repository.Get("test");

                // Assert
                Assert.That(_masterPageFileSystem.FileExists("test.master"), Is.True);
                Assert.That(updated.Content, Is.EqualTo(@"<%@ Master Language=""VB"" %>"));    
            }

            
        }

        [Test]
        public void Can_Perform_Delete()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem))
            {
                var template = new Template("test-add-masterpage.master", "test", "test") { Content = @"<%@ Master Language=""C#"" %>" };
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                // Act
                var templates = repository.Get("test");
                repository.Delete(templates);
                unitOfWork.Commit();

                // Assert
                Assert.IsNull(repository.Get("test"));
            }

           
        }

        [Test]
        public void Can_Perform_Delete_When_Assigned_To_Doc()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            var templateRepository = new TemplateRepository(unitOfWork, NullCacheProvider.Current);
            var tagRepository = new TagRepository(unitOfWork, NullCacheProvider.Current);
            var contentTypeRepository = new ContentTypeRepository(unitOfWork, NullCacheProvider.Current, templateRepository);
            var contentRepo = new ContentRepository(unitOfWork, NullCacheProvider.Current, contentTypeRepository, templateRepository, tagRepository, CacheHelper.CreateDisabledCacheHelper());

            using (contentRepo)
            {
                var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage2", "Textpage");
                var textpage = MockedContent.CreateSimpleContent(contentType);
                contentTypeRepository.AddOrUpdate(contentType);
                contentRepo.AddOrUpdate(textpage);
                unitOfWork.Commit();

                using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem))
                {
                    var template = new Template("test-add-masterpage.master", "test", "test") { Content = @"<%@ Master Language=""C#"" %>" };
                    repository.AddOrUpdate(template);
                    unitOfWork.Commit();

                    textpage.Template = template;
                    contentRepo.AddOrUpdate(textpage);
                    unitOfWork.Commit();

                    // Act
                    var templates = repository.Get("test");
                    repository.Delete(templates);
                    unitOfWork.Commit();

                    // Assert
                    Assert.IsNull(repository.Get("test"));
                }
            }
            
        }

        [Test]
        public void Can_Perform_Delete_On_Nested_Templates()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem))
            {
                var parent = new Template("test-parent-masterpage.master", "parent", "parent") { Content = @"<%@ Master Language=""C#"" %>" };
                var child = new Template("test-child-masterpage.master", "child", "child") { Content = @"<%@ Master Language=""C#"" %>" };
                var baby = new Template("test-baby-masterpage.master", "baby", "baby") { Content = @"<%@ Master Language=""C#"" %>" };
                child.MasterTemplateAlias = parent.Alias;
                child.MasterTemplateId = new Lazy<int>(() => parent.Id);
                baby.MasterTemplateAlias = child.Alias;
                baby.MasterTemplateId = new Lazy<int>(() => child.Id);
                repository.AddOrUpdate(parent);
                repository.AddOrUpdate(child);
                repository.AddOrUpdate(baby);
                unitOfWork.Commit();

                // Act
                var templates = repository.Get("parent");
                repository.Delete(templates);
                unitOfWork.Commit();

                // Assert
                Assert.IsNull(repository.Get("test"));
            }

            
        }

        [Test]
        public void Can_Get_Template_Tree()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem))
            {
                var parent = new Template("test-parent-masterpage.master", "parent", "parent") { Content = @"<%@ Master Language=""C#"" %>" };

                var child1 = new Template("test-child1-masterpage.master", "child1", "child1") { Content = @"<%@ Master Language=""C#"" %>" };
                var toddler1 = new Template("test-toddler1-masterpage.master", "toddler1", "toddler1") { Content = @"<%@ Master Language=""C#"" %>" };
                var toddler2 = new Template("test-toddler2-masterpage.master", "toddler2", "toddler2") { Content = @"<%@ Master Language=""C#"" %>" };
                var baby1 = new Template("test-baby1-masterpage.master", "baby1", "baby1") { Content = @"<%@ Master Language=""C#"" %>" };

                var child2 = new Template("test-child2-masterpage.master", "child2", "child2") { Content = @"<%@ Master Language=""C#"" %>" };
                var toddler3 = new Template("test-toddler3-masterpage.master", "toddler3", "toddler3") { Content = @"<%@ Master Language=""C#"" %>" };
                var toddler4 = new Template("test-toddler4-masterpage.master", "toddler4", "toddler4") { Content = @"<%@ Master Language=""C#"" %>" };
                var baby2 = new Template("test-baby2-masterpage.master", "baby2", "baby2") { Content = @"<%@ Master Language=""C#"" %>" };


                child1.MasterTemplateAlias = parent.Alias;
                child1.MasterTemplateId = new Lazy<int>(() => parent.Id);
                child2.MasterTemplateAlias = parent.Alias;
                child2.MasterTemplateId = new Lazy<int>(() => parent.Id);

                toddler1.MasterTemplateAlias = child1.Alias;
                toddler1.MasterTemplateId = new Lazy<int>(() => child1.Id);
                toddler2.MasterTemplateAlias = child1.Alias;
                toddler2.MasterTemplateId = new Lazy<int>(() => child1.Id);

                toddler3.MasterTemplateAlias = child2.Alias;
                toddler3.MasterTemplateId = new Lazy<int>(() => child2.Id);
                toddler4.MasterTemplateAlias = child2.Alias;
                toddler4.MasterTemplateId = new Lazy<int>(() => child2.Id);

                baby1.MasterTemplateAlias = toddler2.Alias;
                baby1.MasterTemplateId = new Lazy<int>(() => toddler2.Id);

                baby2.MasterTemplateAlias = toddler4.Alias;
                baby2.MasterTemplateId = new Lazy<int>(() => toddler4.Id);

                repository.AddOrUpdate(parent);
                repository.AddOrUpdate(child1);
                repository.AddOrUpdate(child2);
                repository.AddOrUpdate(toddler1);
                repository.AddOrUpdate(toddler2);
                repository.AddOrUpdate(toddler3);
                repository.AddOrUpdate(toddler4);
                repository.AddOrUpdate(baby1);
                repository.AddOrUpdate(baby2);
                unitOfWork.Commit();

                // Act
                var rootNode = repository.GetTemplateNode("parent");

                // Assert
                Assert.IsNotNull(repository.FindTemplateInTree(rootNode, "parent"));
                Assert.IsNotNull(repository.FindTemplateInTree(rootNode, "child1"));
                Assert.IsNotNull(repository.FindTemplateInTree(rootNode, "child2"));
                Assert.IsNotNull(repository.FindTemplateInTree(rootNode, "toddler1"));
                Assert.IsNotNull(repository.FindTemplateInTree(rootNode, "toddler2"));
                Assert.IsNotNull(repository.FindTemplateInTree(rootNode, "toddler3"));
                Assert.IsNotNull(repository.FindTemplateInTree(rootNode, "toddler4"));
                Assert.IsNotNull(repository.FindTemplateInTree(rootNode, "baby1"));
                Assert.IsNotNull(repository.FindTemplateInTree(rootNode, "baby2"));
            }

            
        }

        //[Test]
        //public void Can_Perform_Get_On_ScriptRepository()
        //{
        //    // Arrange
        //    var provider = new FileUnitOfWorkProvider();
        //    var unitOfWork = provider.GetUnitOfWork();
        //    var repository = new ScriptRepository(unitOfWork, _masterPageFileSystem);

        //    // Act
        //    var exists = repository.Get("test-script.js");

        //    // Assert
        //    Assert.That(exists, Is.Not.Null);
        //    Assert.That(exists.Alias, Is.EqualTo("test-script"));
        //    Assert.That(exists.Name, Is.EqualTo("test-script.js"));
        //}

        //[Test]
        //public void Can_Perform_GetAll_On_ScriptRepository()
        //{
        //    // Arrange
        //    var provider = new FileUnitOfWorkProvider();
        //    var unitOfWork = provider.GetUnitOfWork();
        //    var repository = new ScriptRepository(unitOfWork, _masterPageFileSystem);

        //    var script = new Script("test-script1.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
        //    repository.AddOrUpdate(script);
        //    var script2 = new Script("test-script2.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
        //    repository.AddOrUpdate(script2);
        //    var script3 = new Script("test-script3.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
        //    repository.AddOrUpdate(script3);
        //    unitOfWork.Commit();

        //    // Act
        //    var scripts = repository.GetAll();

        //    // Assert
        //    Assert.That(scripts, Is.Not.Null);
        //    Assert.That(scripts.Any(), Is.True);
        //    Assert.That(scripts.Any(x => x == null), Is.False);
        //    Assert.That(scripts.Count(), Is.EqualTo(4));
        //}

        //[Test]
        //public void Can_Perform_GetAll_With_Params_On_ScriptRepository()
        //{
        //    // Arrange
        //    var provider = new FileUnitOfWorkProvider();
        //    var unitOfWork = provider.GetUnitOfWork();
        //    var repository = new ScriptRepository(unitOfWork, _masterPageFileSystem);

        //    var script = new Script("test-script1.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
        //    repository.AddOrUpdate(script);
        //    var script2 = new Script("test-script2.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
        //    repository.AddOrUpdate(script2);
        //    var script3 = new Script("test-script3.js") { Content = "/// <reference name=\"MicrosoftAjax.js\"/>" };
        //    repository.AddOrUpdate(script3);
        //    unitOfWork.Commit();

        //    // Act
        //    var scripts = repository.GetAll("test-script1.js", "test-script2.js");

        //    // Assert
        //    Assert.That(scripts, Is.Not.Null);
        //    Assert.That(scripts.Any(), Is.True);
        //    Assert.That(scripts.Any(x => x == null), Is.False);
        //    Assert.That(scripts.Count(), Is.EqualTo(2));
        //}

        //[Test]
        //public void Can_Perform_Exists_On_ScriptRepository()
        //{
        //    // Arrange
        //    var provider = new FileUnitOfWorkProvider();
        //    var unitOfWork = provider.GetUnitOfWork();
        //    var repository = new ScriptRepository(unitOfWork, _masterPageFileSystem);

        //    // Act
        //    var exists = repository.Exists("test-script.js");

        //    // Assert
        //    Assert.That(exists, Is.True);
        //}

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            _masterPageFileSystem = null;
            _viewsFileSystem = null;
            //Delete all files
            var fsMaster = new PhysicalFileSystem(SystemDirectories.Masterpages);
            var masterPages = fsMaster.GetFiles("", "*.master");
            foreach (var file in masterPages)
            {
                fsMaster.DeleteFile(file);
            }
            var fsViews = new PhysicalFileSystem(SystemDirectories.MvcViews);
            var views = fsMaster.GetFiles("", "*.cshtml");
            foreach (var file in views)
            {
                fsMaster.DeleteFile(file);
            }
        }

        protected Stream CreateStream(string contents = null)
        {
            if (string.IsNullOrEmpty(contents))
                contents = "/* test */";

            var bytes = Encoding.UTF8.GetBytes(contents);
            var stream = new MemoryStream(bytes);

            return stream;
        }
    }
}