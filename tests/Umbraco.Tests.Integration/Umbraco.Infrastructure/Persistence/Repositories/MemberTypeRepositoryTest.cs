// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class MemberTypeRepositoryTest : UmbracoIntegrationTest
    {
        private MemberTypeRepository CreateRepository(IScopeProvider provider)
        {
            IContentTypeCommonRepository commonRepository = GetRequiredService<IContentTypeCommonRepository>();
            ILanguageRepository languageRepository = GetRequiredService<ILanguageRepository>();
            return new MemberTypeRepository((IScopeAccessor)provider, AppCaches.Disabled, Mock.Of<ILogger<MemberTypeRepository>>(), commonRepository, languageRepository, ShortStringHelper);
        }

        [Test]
        public void Can_Persist_Member_Type()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberTypeRepository repository = CreateRepository(provider);

                var memberType = (IMemberType)MemberTypeBuilder.CreateSimpleMemberType();
                repository.Save(memberType);

                IMemberType sut = repository.Get(memberType.Id);

                Dictionary<string, PropertyType> standardProps = ConventionsHelper.GetStandardPropertyTypeStubs(ShortStringHelper);

                Assert.That(sut, Is.Not.Null);
                Assert.That(sut.PropertyGroups.Count, Is.EqualTo(2));
                Assert.That(sut.PropertyTypes.Count(), Is.EqualTo(3 + standardProps.Count));

                Assert.That(sut.PropertyGroups.Any(x => x.HasIdentity == false || x.Id == 0), Is.False);
                Assert.That(sut.PropertyTypes.Any(x => x.HasIdentity == false || x.Id == 0), Is.False);

                TestHelper.AssertPropertyValuesAreEqual(sut, memberType);
            }
        }

        [Test]
        public void Can_Persist_Member_Type_Same_Property_Keys()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberTypeRepository repository = CreateRepository(provider);

                var memberType = (IMemberType)MemberTypeBuilder.CreateSimpleMemberType();

                repository.Save(memberType);
                scope.Complete();

                Guid[] propertyKeys = memberType.PropertyTypes.Select(x => x.Key).OrderBy(x => x).ToArray();
                Guid[] groupKeys = memberType.PropertyGroups.Select(x => x.Key).OrderBy(x => x).ToArray();

                memberType = repository.Get(memberType.Id);
                Guid[] propertyKeys2 = memberType.PropertyTypes.Select(x => x.Key).OrderBy(x => x).ToArray();
                Guid[] groupKeys2 = memberType.PropertyGroups.Select(x => x.Key).OrderBy(x => x).ToArray();

                Assert.IsTrue(propertyKeys.SequenceEqual(propertyKeys2));
                Assert.IsTrue(groupKeys.SequenceEqual(groupKeys2));
            }
        }

        [Test]
        public void Cannot_Persist_Member_Type_Without_Alias()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberTypeRepository repository = CreateRepository(provider);

                MemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
                memberType.Alias = null;

                Assert.Throws<InvalidOperationException>(() => repository.Save(memberType));
            }
        }

        [Test]
        public void Can_Get_All_Member_Types()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberTypeRepository repository = CreateRepository(provider);

                MemberType memberType1 = MemberTypeBuilder.CreateSimpleMemberType();
                repository.Save(memberType1);

                MemberType memberType2 = MemberTypeBuilder.CreateSimpleMemberType();
                memberType2.Name = "AnotherType";
                memberType2.Alias = "anotherType";
                repository.Save(memberType2);

                IEnumerable<IMemberType> result = repository.GetMany();

                // there are 3 because of the Member type created for init data
                Assert.AreEqual(3, result.Count());
            }
        }

        [Test]
        public void Can_Get_All_Member_Types_By_Guid_Ids()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberTypeRepository repository = CreateRepository(provider);

                MemberType memberType1 = MemberTypeBuilder.CreateSimpleMemberType();
                repository.Save(memberType1);

                MemberType memberType2 = MemberTypeBuilder.CreateSimpleMemberType();
                memberType2.Name = "AnotherType";
                memberType2.Alias = "anotherType";
                repository.Save(memberType2);

                IEnumerable<IMemberType> result = ((IReadRepository<Guid, IMemberType>)repository).GetMany(memberType1.Key, memberType2.Key);

                // there are 3 because of the Member type created for init data
                Assert.AreEqual(2, result.Count());
            }
        }

        [Test]
        public void Can_Get_Member_Types_By_Guid_Id()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberTypeRepository repository = CreateRepository(provider);

                MemberType memberType1 = MemberTypeBuilder.CreateSimpleMemberType();
                repository.Save(memberType1);

                MemberType memberType2 = MemberTypeBuilder.CreateSimpleMemberType();
                memberType2.Name = "AnotherType";
                memberType2.Alias = "anotherType";
                repository.Save(memberType2);

                IMemberType result = repository.Get(memberType1.Key);

                // there are 3 because of the Member type created for init data
                Assert.IsNotNull(result);
                Assert.AreEqual(memberType1.Key, result.Key);
            }
        }

        // NOTE: This tests for left join logic (rev 7b14e8eacc65f82d4f184ef46c23340c09569052)
        [Test]
        public void Can_Get_All_Members_When_No_Properties_Assigned()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberTypeRepository repository = CreateRepository(provider);

                MemberType memberType1 = MemberTypeBuilder.CreateSimpleMemberType();
                memberType1.PropertyTypeCollection.Clear();
                repository.Save(memberType1);

                MemberType memberType2 = MemberTypeBuilder.CreateSimpleMemberType();
                memberType2.PropertyTypeCollection.Clear();
                memberType2.Name = "AnotherType";
                memberType2.Alias = "anotherType";
                repository.Save(memberType2);

                IEnumerable<IMemberType> result = repository.GetMany();

                // there are 3 because of the Member type created for init data
                Assert.AreEqual(3, result.Count());
            }
        }

        [Test]
        public void Can_Get_Member_Type_By_Id()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberTypeRepository repository = CreateRepository(provider);

                IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
                repository.Save(memberType);

                memberType = repository.Get(memberType.Id);
                Assert.That(memberType, Is.Not.Null);
            }
        }

        [Test]
        public void Can_Get_Member_Type_By_Guid_Id()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberTypeRepository repository = CreateRepository(provider);

                IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
                repository.Save(memberType);

                memberType = repository.Get(memberType.Key);
                Assert.That(memberType, Is.Not.Null);
            }
        }

        // See: https://github.com/umbraco/Umbraco-CMS/issues/4963#issuecomment-483516698
        [Test]
        public void Bug_Changing_Built_In_Member_Type_Property_Type_Aliases_Results_In_Exception()
        {
            Dictionary<string, PropertyType> stubs = ConventionsHelper.GetStandardPropertyTypeStubs(ShortStringHelper);

            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                MemberTypeRepository repository = CreateRepository(provider);

                IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType("mtype");

                // created without the stub properties
                Assert.AreEqual(1, memberType.PropertyGroups.Count);
                Assert.AreEqual(3, memberType.PropertyTypes.Count());

                // saving *new* member type adds the stub properties
                repository.Save(memberType);

                // saving has added (and saved) the stub properties
                Assert.AreEqual(2, memberType.PropertyGroups.Count);
                Assert.AreEqual(3 + stubs.Count, memberType.PropertyTypes.Count());

                foreach (KeyValuePair<string, PropertyType> stub in stubs)
                {
                    IPropertyType prop = memberType.PropertyTypes.First(x => x.Alias == stub.Key);
                    prop.Alias += "__0000";
                }

                // saving *existing* member type does *not* ensure stub properties
                repository.Save(memberType);

                // therefore, nothing has changed
                Assert.AreEqual(2, memberType.PropertyGroups.Count);
                Assert.AreEqual(3 + stubs.Count, memberType.PropertyTypes.Count());

                // fetching ensures that the stub properties are there
                memberType = repository.Get("mtype");
                Assert.IsNotNull(memberType);

                Assert.AreEqual(2, memberType.PropertyGroups.Count);
                Assert.AreEqual(3 + (stubs.Count * 2), memberType.PropertyTypes.Count());
            }
        }

        [Test]
        public void Built_In_Member_Type_Properties_Are_Automatically_Added_When_Creating()
        {
            Dictionary<string, PropertyType> stubs = ConventionsHelper.GetStandardPropertyTypeStubs(ShortStringHelper);

            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                MemberTypeRepository repository = CreateRepository(provider);

                IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();

                // created without the stub properties
                Assert.AreEqual(1, memberType.PropertyGroups.Count);
                Assert.AreEqual(3, memberType.PropertyTypes.Count());

                // saving *new* member type adds the stub properties
                repository.Save(memberType);

                // saving has added (and saved) the stub properties
                Assert.AreEqual(2, memberType.PropertyGroups.Count);
                Assert.AreEqual(3 + stubs.Count, memberType.PropertyTypes.Count());

                // getting with stub properties
                memberType = repository.Get(memberType.Id);

                Assert.AreEqual(2, memberType.PropertyGroups.Count);
                Assert.AreEqual(3 + stubs.Count, memberType.PropertyTypes.Count());
            }
        }

        [Test]
        public void Built_In_Member_Type_Properties_Missing_Are_Automatically_Added_When_Creating()
        {
            Dictionary<string, PropertyType> stubs = ConventionsHelper.GetStandardPropertyTypeStubs(ShortStringHelper);

            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                MemberTypeRepository repository = CreateRepository(provider);

                IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();

                // created without the stub properties
                Assert.AreEqual(1, memberType.PropertyGroups.Count);
                Assert.AreEqual(3, memberType.PropertyTypes.Count());

                // add one stub property, others are still missing
                memberType.AddPropertyType(stubs.First().Value, Constants.Conventions.Member.StandardPropertiesGroupAlias, Constants.Conventions.Member.StandardPropertiesGroupName);

                // saving *new* member type adds the (missing) stub properties
                repository.Save(memberType);

                // saving has added (and saved) the (missing) stub properties
                Assert.AreEqual(2, memberType.PropertyGroups.Count);
                Assert.AreEqual(3 + stubs.Count, memberType.PropertyTypes.Count());

                // getting with stub properties
                memberType = repository.Get(memberType.Id);

                Assert.AreEqual(2, memberType.PropertyGroups.Count);
                Assert.AreEqual(3 + stubs.Count, memberType.PropertyTypes.Count());
            }
        }

        // This is to show that new properties are created for each member type - there was a bug before
        // that was reusing the same properties with the same Ids between member types
        [Test]
        public void Built_In_Member_Type_Properties_Are_Not_Reused_For_Different_Member_Types()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberTypeRepository repository = CreateRepository(provider);

                IMemberType memberType1 = MemberTypeBuilder.CreateSimpleMemberType();
                IMemberType memberType2 = MemberTypeBuilder.CreateSimpleMemberType("test2");
                repository.Save(memberType1);
                repository.Save(memberType2);

                int[] m1Ids = memberType1.PropertyTypes.Select(x => x.Id).ToArray();
                int[] m2Ids = memberType2.PropertyTypes.Select(x => x.Id).ToArray();

                Assert.IsFalse(m1Ids.Any(m2Ids.Contains));
            }
        }

        [Test]
        public void Can_Delete_MemberType()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                MemberTypeRepository repository = CreateRepository(provider);

                // Act
                IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
                repository.Save(memberType);

                IMemberType contentType2 = repository.Get(memberType.Id);
                repository.Delete(contentType2);

                bool exists = repository.Exists(memberType.Id);

                // Assert
                Assert.That(exists, Is.False);
            }
        }
    }
}
