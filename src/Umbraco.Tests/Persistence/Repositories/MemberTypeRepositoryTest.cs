using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    public class MemberTypeRepositoryTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        private MemberTypeRepository CreateRepository(IDatabaseUnitOfWork unitOfWork)
        {
            return new MemberTypeRepository(unitOfWork, NullCacheProvider.Current);            
        }

        [Test]
        public void Can_Instantiate_Repository_From_Resolver()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();

            // Act
            var repository = RepositoryResolver.Current.ResolveByType<IMemberTypeRepository>(unitOfWork);

            // Assert
            Assert.That(repository, Is.Not.Null);
        }

        [Test]
        public void Can_Persist_Member_Type()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var memberType = MockedContentTypes.CreateSimpleMemberType();
                repository.AddOrUpdate(memberType);
                unitOfWork.Commit();

                var sut = repository.Get(memberType.Id);

                Assert.That(sut, Is.Not.Null);
                Assert.That(sut.PropertyGroups.Count(), Is.EqualTo(1));
                Assert.That(sut.PropertyTypes.Count(), Is.EqualTo(3 + Constants.Conventions.Member.StandardPropertyTypeStubs.Count));

                Assert.That(sut.PropertyGroups.Any(x => x.HasIdentity == false || x.Id == 0), Is.False);
                Assert.That(sut.PropertyTypes.Any(x => x.HasIdentity == false || x.Id == 0), Is.False);
            }
        }

        [Test]
        public void Can_Get_All_Member_Types()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var memberType1 = MockedContentTypes.CreateSimpleMemberType();
                repository.AddOrUpdate(memberType1);
                unitOfWork.Commit();

                var memberType2 = MockedContentTypes.CreateSimpleMemberType();
                memberType2.Name = "AnotherType";
                memberType2.Alias = "anotherType";
                repository.AddOrUpdate(memberType2);
                unitOfWork.Commit();

                var result = repository.GetAll();

                //there are 3 because of the Member type created for init data
                Assert.AreEqual(3, result.Count());
            }
        }

        [Test]
        public void Can_Get_Member_Type_By_Id()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
                repository.AddOrUpdate(memberType);
                unitOfWork.Commit();
                memberType = repository.Get(memberType.Id);
                Assert.That(memberType, Is.Not.Null);
            }
        }

        [Test]
        public void Built_In_Member_Type_Properties_Are_Automatically_Added_When_Creating()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
                repository.AddOrUpdate(memberType);
                unitOfWork.Commit();

                memberType = repository.Get(memberType.Id);

                Assert.That(memberType.PropertyTypes.Count(), Is.EqualTo(3 + Constants.Conventions.Member.StandardPropertyTypeStubs.Count));
                Assert.That(memberType.PropertyGroups.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public void Can_Delete_MemberType()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                // Act
                IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
                repository.AddOrUpdate(memberType);
                unitOfWork.Commit();

                var contentType2 = repository.Get(memberType.Id);
                repository.Delete(contentType2);
                unitOfWork.Commit();

                var exists = repository.Exists(memberType.Id);

                // Assert
                Assert.That(exists, Is.False);
            }
        }
    }
}