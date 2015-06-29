using System;
using System.Reflection;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Tests.Persistence
{
	[TestFixture]
    public class RepositoryResolverTests
    {

		[SetUp]
		public void Setup()
		{
			RepositoryResolver.Current = new RepositoryResolver(
                new RepositoryFactory(true));  //disable all repo caches for tests!

            Resolution.Freeze();
		}

		[TearDown]
		public void Teardown()
		{
			RepositoryResolver.Reset();
		}

		//[TestCase(typeof(IUserTypeRepository))]
		//[TestCase(typeof(IUserRepository))]
		//[TestCase(typeof(IMacroRepository))]
		[TestCase(typeof(IContentRepository))]
		[TestCase(typeof(IMediaRepository))]
		[TestCase(typeof(IMediaTypeRepository))]
		[TestCase(typeof(IContentTypeRepository))]
		[TestCase(typeof(IDataTypeDefinitionRepository))]
		[TestCase(typeof(IDictionaryRepository))]
		[TestCase(typeof(ILanguageRepository))]		
		[TestCase(typeof(IRelationRepository))]
		[TestCase(typeof(IRelationTypeRepository))]
		[TestCase(typeof(IScriptRepository))]
		//[TestCase(typeof(IStylesheetRepository))]
		[TestCase(typeof(ITemplateRepository))]
		public void ResolveRepository(Type repoType)
		{
			var method = typeof(RepositoryResolver).GetMethod("ResolveByType", BindingFlags.NonPublic | BindingFlags.Instance);
			var gMethod = method.MakeGenericMethod(repoType);
			var repo = gMethod.Invoke(RepositoryResolver.Current, new object[] { PetaPocoUnitOfWorkProvider.CreateUnitOfWork() });
			Assert.IsNotNull(repo);
			Assert.IsTrue(TypeHelper.IsTypeAssignableFrom(repoType, repo.GetType()));
			repo.DisposeIfDisposable();
		}

		[Test]
        public void Can_Verify_UOW_In_Repository()
        {
            // Arrange
			var uow = PetaPocoUnitOfWorkProvider.CreateUnitOfWork();

            // Act
            var repository = RepositoryResolver.Current.ResolveByType<IContentRepository>(uow);

            // Assert
            Assert.That(repository, Is.Not.Null);
            Assert.That(uow.Key, Is.EqualTo(((RepositoryBase<int, IContent>)repository).UnitKey));

			repository.Dispose();
        }

        [Test]
        public void Type_Checking()
        {
            var repositoryType = typeof (IContentRepository);
            bool isSubclassOf = repositoryType.IsSubclassOf(typeof(IRepository<int, IContent>));
            bool isAssignableFrom = typeof(IRepository<int, IContent>).IsAssignableFrom(repositoryType);

            Assert.That(isSubclassOf, Is.False);
            Assert.That(isAssignableFrom, Is.True);

			var uow = PetaPocoUnitOfWorkProvider.CreateUnitOfWork();
            var repository = RepositoryResolver.Current.ResolveByType<IContentRepository>(uow);
            bool subclassOf = repository.GetType().IsSubclassOf(typeof (IRepository<int, IContent>));

            Assert.That(subclassOf, Is.False);
            Assert.That((typeof(IRepository<int, IContent>).IsInstanceOfType(repository)), Is.True);

			repository.Dispose();
        }
    }
}