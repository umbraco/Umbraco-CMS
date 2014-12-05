using System;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
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
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {

                // Assert
                Assert.That(repository, Is.Not.Null);    
            }

        }

        [Test]
        public void Can_Perform_Add_MasterPage_Detect_Content()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {
                // Act
                var template = new Template("test", "test", _viewsFileSystem, _masterPageFileSystem, 
                    //even though the default is MVC, the content is not 
                    Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                //Assert
                Assert.That(repository.Get("test"), Is.Not.Null);
                Assert.That(_masterPageFileSystem.FileExists("test.master"), Is.True);    
            }
            
        }

        [Test]
        public void Can_Perform_Add_MasterPage_With_Default_Content()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {
                // Act
                var template = new Template("test", "test", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.WebForms));
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                //Assert
                Assert.That(repository.Get("test"), Is.Not.Null);
                Assert.That(_masterPageFileSystem.FileExists("test.master"), Is.True);
                Assert.AreEqual(@"<%@ Master Language=""C#"" MasterPageFile=""~/umbraco/masterpages/default.master"" AutoEventWireup=""true"" %>

<asp:Content ContentPlaceHolderID=""ContentPlaceHolderDefault"" runat=""server"">

</asp:Content>
", template.Content);
            }

        }

        [Test]
        public void Can_Perform_Add_MasterPage_With_Default_Content_With_Parent()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {
                //NOTE: This has to be persisted first
                var template = new Template("test", "test", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.WebForms));
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                // Act
                var template2 = new Template("test2", "test2", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.WebForms));
                template2.SetMasterTemplate(template);                
                repository.AddOrUpdate(template2);
                unitOfWork.Commit();

                //Assert
                Assert.That(repository.Get("test2"), Is.Not.Null);
                Assert.That(_masterPageFileSystem.FileExists("test2.master"), Is.True);
                Assert.AreEqual(@"<%@ Master Language=""C#"" MasterPageFile=""~/masterpages/test.master"" AutoEventWireup=""true"" %>

", template2.Content);
            }

        }

        [Test]
        public void Can_Perform_Add_View()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {
                // Act
                var template = new Template("test", "test", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                //Assert
                Assert.That(repository.Get("test"), Is.Not.Null);
                Assert.That(_viewsFileSystem.FileExists("test.cshtml"), Is.True);
            }

        }

        [Test]
        public void Can_Perform_Add_View_With_Default_Content()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {
                // Act
                var template = new Template("test", "test", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = ViewHelper.GetDefaultFileContent()
                };
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                //Assert
                Assert.That(repository.Get("test"), Is.Not.Null);
                Assert.That(_viewsFileSystem.FileExists("test.cshtml"), Is.True);
                Assert.AreEqual(@"@inherits Umbraco.Web.Mvc.UmbracoTemplatePage
@{
    Layout = null;
}", template.Content);
            }

        }

        [Test]
        public void Can_Perform_Add_View_With_Default_Content_With_Parent()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {
                //NOTE: This has to be persisted first
                var template = new Template("test", "test", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                // Act
                var template2 = new Template("test2", "test2", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));
                template2.SetMasterTemplate(template);
                repository.AddOrUpdate(template2);
                unitOfWork.Commit();

                //Assert
                Assert.That(repository.Get("test2"), Is.Not.Null);
                Assert.That(_viewsFileSystem.FileExists("test2.cshtml"), Is.True);
                Assert.AreEqual(@"@inherits Umbraco.Web.Mvc.UmbracoTemplatePage
@{
    Layout = ""test.cshtml"";
}", template2.Content);
            }

        }

        [Test]
        public void Can_Perform_Add_Unique_Alias()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem, 
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {
                // Act
                var template = new Template("test", "test", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = ViewHelper.GetDefaultFileContent()
                };
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                var template2 = new Template("test", "test", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = ViewHelper.GetDefaultFileContent()
                };
                repository.AddOrUpdate(template2);
                unitOfWork.Commit();

                //Assert
                Assert.AreEqual("test1", template2.Alias);
            }

        }

        [Test]
        public void Can_Perform_Update_Unique_Alias()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {
                // Act
                var template = new Template("test", "test", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = ViewHelper.GetDefaultFileContent()
                };
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                var template2 = new Template("test1", "test1", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = ViewHelper.GetDefaultFileContent()
                };
                repository.AddOrUpdate(template2);
                unitOfWork.Commit();

                template.Alias = "test1";
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                //Assert
                Assert.AreEqual("test11", template.Alias);
                Assert.That(_viewsFileSystem.FileExists("test11.cshtml"), Is.True);
                Assert.That(_viewsFileSystem.FileExists("test.cshtml"), Is.False);
            }

        }

        [Test]
        public void Can_Perform_Update_MasterPage()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {
                // Act
                var template = new Template("test", "test", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };
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
        public void Can_Perform_Update_View()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {
                // Act
                var template = new Template("test", "test", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = ViewHelper.GetDefaultFileContent()
                };
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                template.Content += "<html></html>";
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                var updated = repository.Get("test");

                // Assert
                Assert.That(_viewsFileSystem.FileExists("test.cshtml"), Is.True);
                Assert.That(updated.Content, Is.EqualTo(ViewHelper.GetDefaultFileContent() + "<html></html>"));
            }


        }

        [Test]
        public void Can_Perform_Delete_MasterPage()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {
                var template = new Template("test", "test", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                // Act
                var templates = repository.Get("test");
                Assert.That(_masterPageFileSystem.FileExists("test.master"), Is.True);
                repository.Delete(templates);
                unitOfWork.Commit();

                // Assert
                Assert.IsNull(repository.Get("test"));
                Assert.That(_masterPageFileSystem.FileExists("test.master"), Is.False);
            }

           
        }

        [Test]
        public void Can_Perform_Delete_View()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {
                var template = new Template("test", "test", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = ViewHelper.GetDefaultFileContent()
                };
                repository.AddOrUpdate(template);
                unitOfWork.Commit();

                // Act
                var templates = repository.Get("test");
                Assert.That(_viewsFileSystem.FileExists("test.cshtml"), Is.True);
                repository.Delete(templates);
                unitOfWork.Commit();

                // Assert
                Assert.IsNull(repository.Get("test"));
                Assert.That(_viewsFileSystem.FileExists("test.cshtml"), Is.False);
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

                using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
                {
                    var template = new Template("test", "test", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                    {
                        Content = @"<%@ Master Language=""C#"" %>"
                    };
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
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {
                var parent = new Template("parent", "parent", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };
                var child = new Template("child", "child", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };
                var baby = new Template("baby", "baby", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };
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
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {
                var parent = new Template("parent", "parent", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };

                var child1 = new Template("child1", "child1", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };
                var toddler1 = new Template("toddler1", "toddler1", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };
                var toddler2 = new Template("toddler2", "toddler2", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };
                var baby1 = new Template("baby1", "baby1", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };

                var child2 = new Template("child2", "child2", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };
                var toddler3 = new Template("toddler3", "toddler3", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };
                var toddler4 = new Template("toddler4", "toddler4", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };
                var baby2 = new Template("baby2", "baby2", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc))
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };


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

        [Test]
        public void Path_Is_Set_Correctly_On_Creation()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {
                var parent = new Template("parent", "parent", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));
                var child1 = new Template("child1", "child1", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));
                var toddler1 = new Template("toddler1", "toddler1", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));
                var toddler2 = new Template("toddler2", "toddler2", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));
                var baby1 = new Template("baby1", "baby1", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));
                var child2 = new Template("child2", "child2", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));
                var toddler3 = new Template("toddler3", "toddler3", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));
                var toddler4 = new Template("toddler4", "toddler4", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));
                var baby2 = new Template("baby2", "baby2", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));

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
                

                // Act
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

                // Assert
                Assert.AreEqual(string.Format("-1,{0}", parent.Id), parent.Path);
                Assert.AreEqual(string.Format("-1,{0},{1}", parent.Id, child1.Id), child1.Path);
                Assert.AreEqual(string.Format("-1,{0},{1}", parent.Id, child2.Id), child2.Path);
                Assert.AreEqual(string.Format("-1,{0},{1}", parent.Id, child2.Id), child2.Path);
                Assert.AreEqual(string.Format("-1,{0},{1},{2}", parent.Id, child1.Id, toddler1.Id), toddler1.Path);
                Assert.AreEqual(string.Format("-1,{0},{1},{2}", parent.Id, child1.Id, toddler2.Id), toddler2.Path);
                Assert.AreEqual(string.Format("-1,{0},{1},{2}", parent.Id, child2.Id, toddler3.Id), toddler3.Path);
                Assert.AreEqual(string.Format("-1,{0},{1},{2}", parent.Id, child2.Id, toddler4.Id), toddler4.Path);
                Assert.AreEqual(string.Format("-1,{0},{1},{2},{3}", parent.Id, child1.Id, toddler2.Id, baby1.Id), baby1.Path);
                Assert.AreEqual(string.Format("-1,{0},{1},{2},{3}", parent.Id, child2.Id, toddler4.Id, baby2.Id), baby2.Path);
            }


        }

        [Test]
        public void Path_Is_Set_Correctly_On_Update()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = new TemplateRepository(unitOfWork, NullCacheProvider.Current, _masterPageFileSystem, _viewsFileSystem,
                Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc)))
            {
                var parent = new Template("parent", "parent", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));
                var child1 = new Template("child1", "child1", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));
                var child2 = new Template("child2", "child2", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));
                var toddler1 = new Template("toddler1", "toddler1", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));
                var toddler2 = new Template("toddler2", "toddler2", _viewsFileSystem, _masterPageFileSystem, Mock.Of<ITemplatesSection>(t => t.DefaultRenderingEngine == RenderingEngine.Mvc));

                child1.MasterTemplateAlias = parent.Alias;
                child1.MasterTemplateId = new Lazy<int>(() => parent.Id);
                child2.MasterTemplateAlias = parent.Alias;
                child2.MasterTemplateId = new Lazy<int>(() => parent.Id);
                toddler1.MasterTemplateAlias = child1.Alias;
                toddler1.MasterTemplateId = new Lazy<int>(() => child1.Id);
                toddler2.MasterTemplateAlias = child1.Alias;
                toddler2.MasterTemplateId = new Lazy<int>(() => child1.Id);

                repository.AddOrUpdate(parent);
                repository.AddOrUpdate(child1);
                repository.AddOrUpdate(child2);
                repository.AddOrUpdate(toddler1);
                repository.AddOrUpdate(toddler2);
                unitOfWork.Commit();

                //Act
                toddler2.SetMasterTemplate(child2);
                repository.AddOrUpdate(toddler2);
                unitOfWork.Commit();

                //Assert
                Assert.AreEqual(string.Format("-1,{0},{1},{2}", parent.Id, child2.Id, toddler2.Id), toddler2.Path);

            }
        }


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