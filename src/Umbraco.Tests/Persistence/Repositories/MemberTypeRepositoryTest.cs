﻿using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
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
    public class MemberTypeRepositoryTest : TestWithDatabaseBase
    {
        private MemberTypeRepository CreateRepository(IScopeProvider provider)
        {
            var templateRepository = Mock.Of<ITemplateRepository>();
            var commonRepository = new ContentTypeCommonRepository((IScopeAccessor)provider, templateRepository, AppCaches);
            return new MemberTypeRepository((IScopeAccessor) provider, AppCaches.Disabled, Mock.Of<ILogger>(), commonRepository);
        }

        [Test]
        public void Can_Persist_Member_Type()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var memberType = (IMemberType) MockedContentTypes.CreateSimpleMemberType();
                repository.Save(memberType);

                var sut = repository.Get(memberType.Id);

                var standardProps = Constants.Conventions.Member.GetStandardPropertyTypeStubs();

                Assert.That(sut, Is.Not.Null);
                Assert.That(sut.PropertyGroups.Count, Is.EqualTo(2));
                Assert.That(sut.PropertyTypes.Count(), Is.EqualTo(3 + standardProps.Count));

                Assert.That(sut.PropertyGroups.Any(x => x.HasIdentity == false || x.Id == 0), Is.False);
                Assert.That(sut.PropertyTypes.Any(x => x.HasIdentity == false || x.Id == 0), Is.False);

                TestHelper.AssertPropertyValuesAreEqual(sut, memberType, "yyyy-MM-dd HH:mm:ss");
            }
        }

        [Test]
        public void Can_Persist_Member_Type_Same_Property_Keys()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var memberType = (IMemberType)MockedContentTypes.CreateSimpleMemberType();

                repository.Save(memberType);
                scope.Complete();

                var propertyKeys = memberType.PropertyTypes.Select(x => x.Key).OrderBy(x => x).ToArray();
                var groupKeys = memberType.PropertyGroups.Select(x => x.Key).OrderBy(x => x).ToArray();

                memberType = repository.Get(memberType.Id);
                var propertyKeys2 = memberType.PropertyTypes.Select(x => x.Key).OrderBy(x => x).ToArray();
                var groupKeys2 = memberType.PropertyGroups.Select(x => x.Key).OrderBy(x => x).ToArray();

                Assert.IsTrue(propertyKeys.SequenceEqual(propertyKeys2));
                Assert.IsTrue(groupKeys.SequenceEqual(groupKeys2));

            }
        }

        [Test]
        public void Cannot_Persist_Member_Type_Without_Alias()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var memberType = MockedContentTypes.CreateSimpleMemberType();
                memberType.Alias = null;


                Assert.Throws<InvalidOperationException>(() => repository.Save(memberType));
            }
        }

        [Test]
        public void Can_Get_All_Member_Types()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var memberType1 = MockedContentTypes.CreateSimpleMemberType();
                repository.Save(memberType1);


                var memberType2 = MockedContentTypes.CreateSimpleMemberType();
                memberType2.Name = "AnotherType";
                memberType2.Alias = "anotherType";
                repository.Save(memberType2);


                var result = repository.GetMany();

                //there are 3 because of the Member type created for init data
                Assert.AreEqual(3, result.Count());
            }
        }

        [Test]
        public void Can_Get_All_Member_Types_By_Guid_Ids()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var memberType1 = MockedContentTypes.CreateSimpleMemberType();
                repository.Save(memberType1);


                var memberType2 = MockedContentTypes.CreateSimpleMemberType();
                memberType2.Name = "AnotherType";
                memberType2.Alias = "anotherType";
                repository.Save(memberType2);


                var result = ((IReadRepository<Guid, IMemberType>)repository).GetMany(memberType1.Key, memberType2.Key);

                //there are 3 because of the Member type created for init data
                Assert.AreEqual(2, result.Count());
            }
        }

        [Test]
        public void Can_Get_Member_Types_By_Guid_Id()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var memberType1 = MockedContentTypes.CreateSimpleMemberType();
                repository.Save(memberType1);


                var memberType2 = MockedContentTypes.CreateSimpleMemberType();
                memberType2.Name = "AnotherType";
                memberType2.Alias = "anotherType";
                repository.Save(memberType2);


                var result = repository.Get(memberType1.Key);

                //there are 3 because of the Member type created for init data
                Assert.IsNotNull(result);
                Assert.AreEqual(memberType1.Key, result.Key);
            }
        }


        //NOTE: This tests for left join logic (rev 7b14e8eacc65f82d4f184ef46c23340c09569052)
        [Test]
        public void Can_Get_All_Members_When_No_Properties_Assigned()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                var memberType1 = MockedContentTypes.CreateSimpleMemberType();
                memberType1.PropertyTypeCollection.Clear();
                repository.Save(memberType1);


                var memberType2 = MockedContentTypes.CreateSimpleMemberType();
                memberType2.PropertyTypeCollection.Clear();
                memberType2.Name = "AnotherType";
                memberType2.Alias = "anotherType";
                repository.Save(memberType2);


                var result = repository.GetMany();

                //there are 3 because of the Member type created for init data
                Assert.AreEqual(3, result.Count());
            }
        }


        [Test]
        public void Can_Get_Member_Type_By_Id()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
                repository.Save(memberType);

                memberType = repository.Get(memberType.Id);
                Assert.That(memberType, Is.Not.Null);
            }
        }

        [Test]
        public void Can_Get_Member_Type_By_Guid_Id()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
                repository.Save(memberType);

                memberType = repository.Get(memberType.Key);
                Assert.That(memberType, Is.Not.Null);
            }
        }

        [Test]
        public void Built_In_Member_Type_Properties_Are_Automatically_Added_When_Creating()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
                repository.Save(memberType);


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
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                IMemberType memberType1 = MockedContentTypes.CreateSimpleMemberType();
                IMemberType memberType2 = MockedContentTypes.CreateSimpleMemberType("test2");
                repository.Save(memberType1);
                repository.Save(memberType2);


                var m1Ids = memberType1.PropertyTypes.Select(x => x.Id).ToArray();
                var m2Ids = memberType2.PropertyTypes.Select(x => x.Id).ToArray();

                Assert.IsFalse(m1Ids.Any(m2Ids.Contains));
            }
        }

        [Test]
        public void Can_Delete_MemberType()
        {
            // Arrange
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repository = CreateRepository(provider);

                // Act
                IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
                repository.Save(memberType);


                var contentType2 = repository.Get(memberType.Id);
                repository.Delete(contentType2);


                var exists = repository.Exists(memberType.Id);

                // Assert
                Assert.That(exists, Is.False);
            }
        }
    }
}
