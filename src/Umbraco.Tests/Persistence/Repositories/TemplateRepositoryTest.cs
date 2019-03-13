using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class TemplateRepositoryTest : TestWithDatabaseBase
    {
        private IFileSystems _fileSystems;

        private ITemplateRepository CreateRepository(IScopeProvider provider)
        {
            return new TemplateRepository((IScopeAccessor) provider, AppCaches.Disabled, Logger, _fileSystems);
        }

        public override void SetUp()
        {
            base.SetUp();

            _fileSystems = Mock.Of<IFileSystems>();
            var viewsFileSystem = new PhysicalFileSystem(SystemDirectories.MvcViews);
            Mock.Get(_fileSystems).Setup(x => x.MvcViewsFileSystem).Returns(viewsFileSystem);
        }

        [Test]
        public void Can_Instantiate_Repository()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider);

                // Assert
                Assert.That(repository, Is.Not.Null);
            }
        }

        [Test]
        public void Can_Perform_Add_View()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider);

                // Act
                var template = new Template("test", "test");
                repository.Save(template);
                

                //Assert
                Assert.That(repository.Get("test"), Is.Not.Null);
                Assert.That(_fileSystems.MvcViewsFileSystem.FileExists("test.cshtml"), Is.True);
            }
        }

        [Test]
        public void Can_Perform_Add_View_With_Default_Content()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider);

                // Act
                var template = new Template("test", "test")
                {
                    Content = ViewHelper.GetDefaultFileContent()
                };
                repository.Save(template);

                //Assert
                Assert.That(repository.Get("test"), Is.Not.Null);
                Assert.That(_fileSystems.MvcViewsFileSystem.FileExists("test.cshtml"), Is.True);
                Assert.AreEqual(
                    @"@inherits Umbraco.Web.Mvc.UmbracoViewPage @{ Layout = null;}".StripWhitespace(),
                    template.Content.StripWhitespace());
            }
        }

        [Test]
        public void Can_Perform_Add_View_With_Default_Content_With_Parent()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider);

                //NOTE: This has to be persisted first
                var template = new Template("test", "test");
                repository.Save(template);

                // Act
                var template2 = new Template("test2", "test2");
                template2.SetMasterTemplate(template);
                repository.Save(template2);

                //Assert
                Assert.That(repository.Get("test2"), Is.Not.Null);
                Assert.That(_fileSystems.MvcViewsFileSystem.FileExists("test2.cshtml"), Is.True);
                Assert.AreEqual(
                    "@inherits Umbraco.Web.Mvc.UmbracoViewPage @{ Layout = \"test.cshtml\";}".StripWhitespace(),
                    template2.Content.StripWhitespace());
            }
        }

        [Test]
        public void Can_Perform_Add_Unique_Alias()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider);

                // Act
                var template = new Template("test", "test")
                {
                    Content = ViewHelper.GetDefaultFileContent()
                };
                repository.Save(template);

                var template2 = new Template("test", "test")
                {
                    Content = ViewHelper.GetDefaultFileContent()
                };
                repository.Save(template2);

                //Assert
                Assert.AreEqual("test1", template2.Alias);
            }
        }

        [Test]
        public void Can_Perform_Update_Unique_Alias()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider);

                // Act
                var template = new Template("test", "test")
                {
                    Content = ViewHelper.GetDefaultFileContent()
                };
                repository.Save(template);

                var template2 = new Template("test1", "test1")
                {
                    Content = ViewHelper.GetDefaultFileContent()
                };
                repository.Save(template2);

                template.Alias = "test1";
                repository.Save(template);

                //Assert
                Assert.AreEqual("test11", template.Alias);
                Assert.That(_fileSystems.MvcViewsFileSystem.FileExists("test11.cshtml"), Is.True);
                Assert.That(_fileSystems.MvcViewsFileSystem.FileExists("test.cshtml"), Is.False);
            }
        }

        [Test]
        public void Can_Perform_Update_View()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider);

                // Act
                var template = new Template("test", "test")
                {
                    Content = ViewHelper.GetDefaultFileContent()
                };
                repository.Save(template);

                template.Content += "<html></html>";
                repository.Save(template);

                var updated = repository.Get("test");

                // Assert
                Assert.That(_fileSystems.MvcViewsFileSystem.FileExists("test.cshtml"), Is.True);
                Assert.That(updated.Content, Is.EqualTo(ViewHelper.GetDefaultFileContent() + "<html></html>"));
            }
        }

        [Test]
        public void Can_Perform_Delete_View()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider);

                var template = new Template("test", "test")
                {
                    Content = ViewHelper.GetDefaultFileContent()
                };
                repository.Save(template);

                // Act
                var templates = repository.Get("test");
                Assert.That(_fileSystems.MvcViewsFileSystem.FileExists("test.cshtml"), Is.True);
                repository.Delete(templates);

                // Assert
                Assert.IsNull(repository.Get("test"));
                Assert.That(_fileSystems.MvcViewsFileSystem.FileExists("test.cshtml"), Is.False);
            }
        }

        [Test]
        public void Can_Perform_Delete_When_Assigned_To_Doc()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var templateRepository = CreateRepository(ScopeProvider);

                var tagRepository = new TagRepository((IScopeAccessor) ScopeProvider, AppCaches.Disabled, Logger);
                var commonRepository = new ContentTypeCommonRepository(ScopeProvider, templateRepository);
                var contentTypeRepository = new ContentTypeRepository((IScopeAccessor) ScopeProvider, AppCaches.Disabled, Logger, commonRepository);
                var languageRepository = new LanguageRepository((IScopeAccessor) ScopeProvider, AppCaches.Disabled, Logger);
                var contentRepo = new DocumentRepository((IScopeAccessor) ScopeProvider, AppCaches.Disabled, Logger, contentTypeRepository, templateRepository, tagRepository, languageRepository);

                var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage2", "Textpage");
                ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                contentTypeRepository.Save(contentType);
                var textpage = MockedContent.CreateSimpleContent(contentType);
                contentRepo.Save(textpage);

                var template = new Template("test", "test")
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };
                templateRepository.Save(template);

                textpage.TemplateId = template.Id;
                contentRepo.Save(textpage);

                // Act
                var templates = templateRepository.Get("test");
                templateRepository.Delete(templates);

                // Assert
                Assert.IsNull(templateRepository.Get("test"));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_Nested_Templates()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider);

                var parent = new Template("parent", "parent")
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };
                var child = new Template("child", "child")
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };
                var baby = new Template("baby", "baby")
                {
                    Content = @"<%@ Master Language=""C#"" %>"
                };
                child.MasterTemplateAlias = parent.Alias;
                child.MasterTemplateId = new Lazy<int>(() => parent.Id);
                baby.MasterTemplateAlias = child.Alias;
                baby.MasterTemplateId = new Lazy<int>(() => child.Id);
                repository.Save(parent);
                repository.Save(child);
                repository.Save(baby);

                // Act
                var templates = repository.Get("parent");
                repository.Delete(templates);

                // Assert
                Assert.IsNull(repository.Get("test"));
            }
        }

        [Test]
        public void Can_Get_All()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider);

                var created = CreateHierarchy(repository).ToArray();

                // Act
                var all = repository.GetAll();
                var allByAlias = repository.GetAll("parent", "child2", "baby2", "notFound");
                var allById = repository.GetMany(created[0].Id, created[2].Id, created[4].Id, created[5].Id, 999999);

                // Assert
                Assert.AreEqual(9, all.Count());
                Assert.AreEqual(9, all.DistinctBy(x => x.Id).Count());

                Assert.AreEqual(3, allByAlias.Count());
                Assert.AreEqual(3, allByAlias.DistinctBy(x => x.Id).Count());

                Assert.AreEqual(4, allById.Count());
                Assert.AreEqual(4, allById.DistinctBy(x => x.Id).Count());
            }
        }

        [Test]
        public void Can_Get_Children()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider);

                var created = CreateHierarchy(repository).ToArray();

                // Act
                var childrenById = repository.GetChildren(created[1].Id);
                var childrenByAlias = repository.GetChildren(created[1].Alias);

                // Assert
                Assert.AreEqual(2, childrenById.Count());
                Assert.AreEqual(2, childrenById.DistinctBy(x => x.Id).Count());
                Assert.AreEqual(2, childrenByAlias.Count());
                Assert.AreEqual(2, childrenByAlias.DistinctBy(x => x.Id).Count());
            }
        }

        [Test]
        public void Can_Get_Children_At_Root()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider);

                CreateHierarchy(repository).ToArray();

                // Act
                var children = repository.GetChildren(-1);

                // Assert
                Assert.AreEqual(1, children.Count());
                Assert.AreEqual(1, children.DistinctBy(x => x.Id).Count());
            }
        }

        [Test]
        public void Can_Get_Descendants()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider);
                var created = CreateHierarchy(repository).ToArray();

                // Act
                var descendantsById = repository.GetDescendants(created[1].Id);
                var descendantsByAlias = repository.GetDescendants(created[1].Alias);

                // Assert
                Assert.AreEqual(3, descendantsById.Count());
                Assert.AreEqual(3, descendantsById.DistinctBy(x => x.Id).Count());

                Assert.AreEqual(3, descendantsByAlias.Count());
                Assert.AreEqual(3, descendantsByAlias.DistinctBy(x => x.Id).Count());
            }
        }

        [Test]
        public void Path_Is_Set_Correctly_On_Creation()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider);

                var parent = new Template("parent", "parent");
                var child1 = new Template("child1", "child1");
                var toddler1 = new Template("toddler1", "toddler1");
                var toddler2 = new Template("toddler2", "toddler2");
                var baby1 = new Template("baby1", "baby1");
                var child2 = new Template("child2", "child2");
                var toddler3 = new Template("toddler3", "toddler3");
                var toddler4 = new Template("toddler4", "toddler4");
                var baby2 = new Template("baby2", "baby2");

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
                repository.Save(parent);
                repository.Save(child1);
                repository.Save(child2);
                repository.Save(toddler1);
                repository.Save(toddler2);
                repository.Save(toddler3);
                repository.Save(toddler4);
                repository.Save(baby1);
                repository.Save(baby2);

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
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider);

                var parent = new Template("parent", "parent");
                var child1 = new Template("child1", "child1");
                var child2 = new Template("child2", "child2");
                var toddler1 = new Template("toddler1", "toddler1");
                var toddler2 = new Template("toddler2", "toddler2");

                child1.MasterTemplateAlias = parent.Alias;
                child1.MasterTemplateId = new Lazy<int>(() => parent.Id);
                child2.MasterTemplateAlias = parent.Alias;
                child2.MasterTemplateId = new Lazy<int>(() => parent.Id);
                toddler1.MasterTemplateAlias = child1.Alias;
                toddler1.MasterTemplateId = new Lazy<int>(() => child1.Id);
                toddler2.MasterTemplateAlias = child1.Alias;
                toddler2.MasterTemplateId = new Lazy<int>(() => child1.Id);

                repository.Save(parent);
                repository.Save(child1);
                repository.Save(child2);
                repository.Save(toddler1);
                repository.Save(toddler2);

                //Act
                toddler2.SetMasterTemplate(child2);
                repository.Save(toddler2);

                //Assert
                Assert.AreEqual($"-1,{parent.Id},{child2.Id},{toddler2.Id}", toddler2.Path);
            }
        }

        [Test]
        public void Path_Is_Set_Correctly_On_Update_With_Master_Template_Removal()
        {
            // Arrange
            using (ScopeProvider.CreateScope())
            {
                var repository = CreateRepository(ScopeProvider);

                var parent = new Template("parent", "parent");
                var child1 = new Template("child1", "child1");

                child1.MasterTemplateAlias = parent.Alias;
                child1.MasterTemplateId = new Lazy<int>(() => parent.Id);

                repository.Save(parent);
                repository.Save(child1);

                //Act
                child1.SetMasterTemplate(null);
                repository.Save(child1);

                //Assert
                Assert.AreEqual($"-1,{child1.Id}", child1.Path);
            }
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            _fileSystems  = null;

            //Delete all files
            var fsViews = new PhysicalFileSystem(SystemDirectories.MvcViews);
            var views = fsViews.GetFiles("", "*.cshtml");
            foreach (var file in views)
                fsViews.DeleteFile(file);
        }

        protected Stream CreateStream(string contents = null)
        {
            if (string.IsNullOrEmpty(contents))
                contents = "/* test */";

            var bytes = Encoding.UTF8.GetBytes(contents);
            var stream = new MemoryStream(bytes);

            return stream;
        }

        private IEnumerable<ITemplate> CreateHierarchy(ITemplateRepository repository)
        {
            var parent = new Template("parent", "parent")
            {
                Content = @"<%@ Master Language=""C#"" %>"
            };

            var child1 = new Template("child1", "child1")
            {
                Content = @"<%@ Master Language=""C#"" %>"
            };
            var toddler1 = new Template("toddler1", "toddler1")
            {
                Content = @"<%@ Master Language=""C#"" %>"
            };
            var toddler2 = new Template("toddler2", "toddler2")
            {
                Content = @"<%@ Master Language=""C#"" %>"
            };
            var baby1 = new Template("baby1", "baby1")
            {
                Content = @"<%@ Master Language=""C#"" %>"
            };

            var child2 = new Template("child2", "child2")
            {
                Content = @"<%@ Master Language=""C#"" %>"
            };
            var toddler3 = new Template("toddler3", "toddler3")
            {
                Content = @"<%@ Master Language=""C#"" %>"
            };
            var toddler4 = new Template("toddler4", "toddler4")
            {
                Content = @"<%@ Master Language=""C#"" %>"
            };
            var baby2 = new Template("baby2", "baby2")
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

            repository.Save(parent);
            repository.Save(child1);
            repository.Save(child2);
            repository.Save(toddler1);
            repository.Save(toddler2);
            repository.Save(toddler3);
            repository.Save(toddler4);
            repository.Save(baby1);
            repository.Save(baby2);
            

            return new[] {parent, child1, child2, toddler1, toddler2, toddler3, toddler4, baby1, baby2};
        }
    }
}
