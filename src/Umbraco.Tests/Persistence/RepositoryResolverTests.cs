using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    public class RepositoryResolverTests
    {
        [Test]
        public void Can_Resolve_All_Repositories()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            RepositoryResolver.RegisterRepositories();
            
            // Assert
            Assert.That(RepositoryResolver.RegisteredRepositories(), Is.EqualTo(13));
        }

        [Test]
        public void Can_Resolve_ContentRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_ContentTypeRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<IContentTypeRepository, IContentType, int>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_MediaRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<IMediaRepository, IMedia, int>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_MediaTypeRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<IMediaTypeRepository, IMediaType, int>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_DataTypeDefinitionRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<IDataTypeDefinitionRepository, DataTypeDefinition, int>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_DictionaryRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<IDictionaryRepository, DictionaryItem, int>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_LanguageRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<ILanguageRepository, Language, int>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_MacroRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<IMacroRepository, IMacro, string>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_RelationRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<IRelationRepository, Relation, int>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_RelationTypeRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<IRelationTypeRepository, RelationType, int>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_ScriptRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<IScriptRepository, Script, string>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_StylesheetRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<IStylesheetRepository, Stylesheet, string>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_TemplateRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<ITemplateRepository, Template, string>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }
    }
}