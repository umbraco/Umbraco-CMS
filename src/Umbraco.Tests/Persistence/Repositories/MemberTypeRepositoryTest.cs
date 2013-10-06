using System.Linq;
using NUnit.Framework;
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
        public void MemberRepository_Can_Persist_Member()
        {
            IMemberType sut;
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var memberType = MockedContentTypes.CreateSimpleMemberType();
                repository.AddOrUpdate(memberType);
                unitOfWork.Commit();

                sut = repository.Get(memberType.Id);

                Assert.That(sut, Is.Not.Null);
                Assert.That(sut.PropertyGroups.Count(), Is.EqualTo(1));
                Assert.That(sut.PropertyTypes.Count(), Is.EqualTo(12));

                Assert.That(sut.PropertyGroups.Any(x => x.HasIdentity == false || x.Id == 0), Is.False);
                Assert.That(sut.PropertyTypes.Any(x => x.HasIdentity == false || x.Id == 0), Is.False);
            }
        }

        [Test, NUnit.Framework.Ignore]
        public void MemberTypeRepository_Can_Get_MemberType_By_Id()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {

                var memberType = repository.Get(1340);

                Assert.That(memberType, Is.Not.Null);
                Assert.That(memberType.PropertyTypes.Count(), Is.EqualTo(13));
                Assert.That(memberType.PropertyGroups.Any(), Is.False);

                repository.AddOrUpdate(memberType);
                unitOfWork.Commit();
                Assert.That(memberType.PropertyTypes.Any(x => x.HasIdentity == false), Is.False);
            }
        }
    }
}