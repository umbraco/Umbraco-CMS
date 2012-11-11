using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
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
            Assert.That(RepositoryResolver.RegisteredRepositories(), Is.EqualTo(15));
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
            var repository = RepositoryResolver.ResolveByType<IDataTypeDefinitionRepository, IDataTypeDefinition, int>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_DictionaryRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<IDictionaryRepository, IDictionaryItem, int>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_LanguageRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<ILanguageRepository, ILanguage, int>(uow);

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
            var repository = RepositoryResolver.ResolveByType<ITemplateFileOnlyRepository, Template, string>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_UserRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<IUserRepository, IUser, int>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_UserTypeRepository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<IUserTypeRepository, IUserType, int>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Verify_UOW_In_Repository()
        {
            // Arrange
            var uow = new PetaPocoUnitOfWork();

            // Act
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
            Assert.That(uow.Key, Is.EqualTo(((RepositoryBase<int, IContent>)repository).UnitKey));
        }

        [Test]
        public void Type_Checking()
        {
            var repositoryType = typeof (IContentRepository);
            bool isSubclassOf = repositoryType.IsSubclassOf(typeof(IRepository<int, IContent>));
            bool isAssignableFrom = typeof(IRepository<int, IContent>).IsAssignableFrom(repositoryType);

            Assert.That(isSubclassOf, Is.False);
            Assert.That(isAssignableFrom, Is.True);
            
            var uow = new PetaPocoUnitOfWork();
            var repository = RepositoryResolver.ResolveByType<IContentRepository, IContent, int>(uow);
            bool subclassOf = repository.GetType().IsSubclassOf(typeof (IRepository<int, IContent>));

            Assert.That(subclassOf, Is.False);
            Assert.That((typeof(IRepository<int, IContent>).IsInstanceOfType(repository)), Is.True);
        }
    }
}