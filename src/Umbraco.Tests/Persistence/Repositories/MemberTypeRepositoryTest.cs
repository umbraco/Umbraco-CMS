using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;

using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
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
            return new MemberTypeRepository(unitOfWork, CacheHelper.CreateDisabledCacheHelper(), Mock.Of<ILogger>(), SqlSyntax);            
        }

        [Test]
        public void Can_Persist_Member_Type()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var memberType = MockedContentTypes.CreateSimpleMemberType();
                repository.AddOrUpdate(memberType);
                unitOfWork.Commit();

                var sut = repository.Get(memberType.Id);

                var standardProps = Constants.Conventions.Member.GetStandardPropertyTypeStubs();

                Assert.That(sut, Is.Not.Null);
                Assert.That(sut.PropertyGroups.Count(), Is.EqualTo(2));
                Assert.That(sut.PropertyTypes.Count(), Is.EqualTo(3 + standardProps.Count));

                Assert.That(sut.PropertyGroups.Any(x => x.HasIdentity == false || x.Id == 0), Is.False);
                Assert.That(sut.PropertyTypes.Any(x => x.HasIdentity == false || x.Id == 0), Is.False);
            }
        }

        [Test]
        public void Cannot_Persist_Member_Type_Without_Alias()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var memberType = MockedContentTypes.CreateSimpleMemberType();
                memberType.Alias = null;
                repository.AddOrUpdate(memberType);

                Assert.Throws<InvalidOperationException>(unitOfWork.Commit);
            }
        }

        [Test]
        public void Can_Get_All_Member_Types()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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

        //NOTE: This tests for left join logic (rev 7b14e8eacc65f82d4f184ef46c23340c09569052)
        [Test]
        public void Can_Get_All_Members_When_No_Properties_Assigned()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                var memberType1 = MockedContentTypes.CreateSimpleMemberType();
                memberType1.PropertyTypeCollection.Clear();
                repository.AddOrUpdate(memberType1);
                unitOfWork.Commit();

                var memberType2 = MockedContentTypes.CreateSimpleMemberType();
                memberType2.PropertyTypeCollection.Clear();
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
                repository.AddOrUpdate(memberType);
                unitOfWork.Commit();

                memberType = repository.Get(memberType.Id);

                Assert.That(memberType.PropertyTypes.Count(), Is.EqualTo(3 + Constants.Conventions.Member.GetStandardPropertyTypeStubs().Count));
                Assert.That(memberType.PropertyGroups.Count(), Is.EqualTo(2));
            }
        }

        //This is to show that new properties are created for each member type - there was a bug before
        // that was reusing the same properties with the same Ids between member types
        [Test]
        public void Built_In_Member_Type_Properties_Are_Not_Reused_For_Different_Member_Types()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repository = CreateRepository(unitOfWork))
            {
                IMemberType memberType1 = MockedContentTypes.CreateSimpleMemberType();
                IMemberType memberType2 = MockedContentTypes.CreateSimpleMemberType("test2");
                repository.AddOrUpdate(memberType1);
                repository.AddOrUpdate(memberType2);
                unitOfWork.Commit();

                var m1Ids = memberType1.PropertyTypes.Select(x => x.Id).ToArray();
                var m2Ids = memberType2.PropertyTypes.Select(x => x.Id).ToArray();

                Assert.IsFalse(m1Ids.Any(m2Ids.Contains));
            }
        }

        [Test]
        public void Can_Delete_MemberType()
        {
            // Arrange
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
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