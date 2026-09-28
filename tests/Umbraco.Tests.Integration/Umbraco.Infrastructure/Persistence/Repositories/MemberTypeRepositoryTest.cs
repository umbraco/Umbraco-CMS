// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MemberTypeRepositoryTest : UmbracoIntegrationTest
{
    private MemberTypeRepository CreateRepository(IScopeProvider provider)
    {
        var commonRepository = GetRequiredService<IContentTypeCommonRepository>();
        var languageRepository = GetRequiredService<ILanguageRepository>();
        var efCoreScopeAccessor = GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>();
        return new MemberTypeRepository(AppCaches.Disabled, Mock.Of<ILogger<MemberTypeRepository>>(), commonRepository, languageRepository, ShortStringHelper, Mock.Of<IRepositoryCacheVersionService>(), IdKeyMap, Mock.Of<ICacheSyncService>(), efCoreScopeAccessor);
    }

    [Test]
    public async Task Can_Persist_Member_Type()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var memberType = (IMemberType)MemberTypeBuilder.CreateSimpleMemberType();
            await repository.SaveAsync(memberType, CancellationToken.None);

            var sut = await repository.GetAsync(memberType.Id, CancellationToken.None);

            var standardProps = ConventionsHelper.GetStandardPropertyTypeStubs(ShortStringHelper);

            // if there are any standard properties, they all get added to a single group
            var expectedGroupCount = standardProps.Count > 0 ? 2 : 1;

            Assert.That(sut, Is.Not.Null);
            Assert.That(sut.PropertyGroups.Count, Is.EqualTo(expectedGroupCount));
            Assert.That(sut.PropertyTypes.Count(), Is.EqualTo(3 + standardProps.Count));

            Assert.That(sut.PropertyGroups.Any(x => x.HasIdentity == false || x.Id == 0), Is.False);
            Assert.That(sut.PropertyTypes.Any(x => x.HasIdentity == false || x.Id == 0), Is.False);

            TestHelper.AssertPropertyValuesAreEqual(sut, memberType);
        }
    }

    [Test]
    public async Task Can_Persist_Member_Type_Same_Property_Keys()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var memberType = (IMemberType)MemberTypeBuilder.CreateSimpleMemberType();

            await repository.SaveAsync(memberType, CancellationToken.None);
            scope.Complete();

            var propertyKeys = memberType.PropertyTypes.Select(x => x.Key).OrderBy(x => x).ToArray();
            var groupKeys = memberType.PropertyGroups.Select(x => x.Key).OrderBy(x => x).ToArray();

            memberType = await repository.GetAsync(memberType.Id, CancellationToken.None);
            var propertyKeys2 = memberType.PropertyTypes.Select(x => x.Key).OrderBy(x => x).ToArray();
            var groupKeys2 = memberType.PropertyGroups.Select(x => x.Key).OrderBy(x => x).ToArray();

            Assert.IsTrue(propertyKeys.SequenceEqual(propertyKeys2));
            Assert.IsTrue(groupKeys.SequenceEqual(groupKeys2));
        }
    }

    [Test]
    public async Task Cannot_Persist_Member_Type_Without_Alias()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var memberType = MemberTypeBuilder.CreateSimpleMemberType();
            memberType.Alias = null;

            Assert.ThrowsAsync<InvalidOperationException>(async () => await repository.SaveAsync(memberType, CancellationToken.None));
        }
    }

    [Test]
    public async Task Can_Get_All_Member_Types()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var memberType1 = MemberTypeBuilder.CreateSimpleMemberType();
            await repository.SaveAsync(memberType1, CancellationToken.None);

            var memberType2 = MemberTypeBuilder.CreateSimpleMemberType();
            memberType2.Name = "AnotherType";
            memberType2.Alias = "anotherType";
            await repository.SaveAsync(memberType2, CancellationToken.None);

            var result = await repository.GetAllAsync(CancellationToken.None);

            // there are 3 because of the Member type created for init data
            Assert.AreEqual(3, result.Count());
        }
    }

    [Test]
    public async Task Can_Get_All_Member_Types_By_Guid_Ids()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var memberType1 = MemberTypeBuilder.CreateSimpleMemberType();
            await repository.SaveAsync(memberType1, CancellationToken.None);

            var memberType2 = MemberTypeBuilder.CreateSimpleMemberType();
            memberType2.Name = "AnotherType";
            memberType2.Alias = "anotherType";
            await repository.SaveAsync(memberType2, CancellationToken.None);

            var result = await repository.GetManyAsync([memberType1.Key, memberType2.Key], CancellationToken.None);

            // there are 3 because of the Member type created for init data
            Assert.AreEqual(2, result.Count());
        }
    }

    [Test]
    public async Task Can_Get_Member_Types_By_Guid_Id()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var memberType1 = MemberTypeBuilder.CreateSimpleMemberType();
            await repository.SaveAsync(memberType1, CancellationToken.None);

            var memberType2 = MemberTypeBuilder.CreateSimpleMemberType();
            memberType2.Name = "AnotherType";
            memberType2.Alias = "anotherType";
            await repository.SaveAsync(memberType2, CancellationToken.None);

            var result = await repository.GetAsync(memberType1.Key, CancellationToken.None);

            // there are 3 because of the Member type created for init data
            Assert.IsNotNull(result);
            Assert.AreEqual(memberType1.Key, result.Key);
        }
    }

    // NOTE: This tests for left join logic (rev 7b14e8eacc65f82d4f184ef46c23340c09569052)
    [Test]
    public async Task Can_Get_All_Members_When_No_Properties_Assigned()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var memberType1 = MemberTypeBuilder.CreateSimpleMemberType();
            memberType1.PropertyTypeCollection.Clear();
            await repository.SaveAsync(memberType1, CancellationToken.None);

            var memberType2 = MemberTypeBuilder.CreateSimpleMemberType();
            memberType2.PropertyTypeCollection.Clear();
            memberType2.Name = "AnotherType";
            memberType2.Alias = "anotherType";
            await repository.SaveAsync(memberType2, CancellationToken.None);

            var result = await repository.GetAllAsync(CancellationToken.None);

            // there are 3 because of the Member type created for init data
            Assert.AreEqual(3, result.Count());
        }
    }

    [Test]
    public async Task Can_Get_Member_Type_By_Id()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            await repository.SaveAsync(memberType, CancellationToken.None);

            memberType = await repository.GetAsync(memberType.Id, CancellationToken.None);
            Assert.That(memberType, Is.Not.Null);
        }
    }

    [Test]
    public async Task Can_Get_Member_Type_By_Guid_Id()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            await repository.SaveAsync(memberType, CancellationToken.None);

            memberType = await repository.GetAsync(memberType.Key, CancellationToken.None);
            Assert.That(memberType, Is.Not.Null);
        }
    }

    // See: https://github.com/umbraco/Umbraco-CMS/issues/4963#issuecomment-483516698
    [Test]
    public void Bug_Changing_Built_In_Member_Type_Property_Type_Aliases_Results_In_Exception()
    {
        // This test was initially deleted but that broke the build as it was marked as a breaking change
        // https://github.com/umbraco/Umbraco-CMS/pull/14060
        // Easiest fix for now is to leave the test and just don't do anything
    }

    [Test]
    public void Built_In_Member_Type_Properties_Are_Automatically_Added_When_Creating()
    {
        // This test was initially deleted but that broke the build as it was marked as a breaking change
        // https://github.com/umbraco/Umbraco-CMS/pull/14060
        // Easiest fix for now is to leave the test and just don't do anything
    }

    [Test]
    public void Built_In_Member_Type_Properties_Missing_Are_Automatically_Added_When_Creating()
    {
        // This test was initially deleted but that broke the build as it was marked as a breaking change
        // https://github.com/umbraco/Umbraco-CMS/pull/14060
        // Easiest fix for now is to leave the test and just don't do anything
    }

    // This is to show that new properties are created for each member type - there was a bug before
    // that was reusing the same properties with the same Ids between member types
    [Test]
    public async Task Built_In_Member_Type_Properties_Are_Not_Reused_For_Different_Member_Types()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            IMemberType memberType1 = MemberTypeBuilder.CreateSimpleMemberType();
            IMemberType memberType2 = MemberTypeBuilder.CreateSimpleMemberType("test2");
            await repository.SaveAsync(memberType1, CancellationToken.None);
            await repository.SaveAsync(memberType2, CancellationToken.None);

            var m1Ids = memberType1.PropertyTypes.Select(x => x.Id).ToArray();
            var m2Ids = memberType2.PropertyTypes.Select(x => x.Id).ToArray();

            Assert.IsFalse(m1Ids.Any(m2Ids.Contains));
        }
    }

    [Test]
    public async Task Can_Persist_Member_Type_Property_Metadata()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            var memberType = (MemberType)MemberTypeBuilder.CreateSimpleMemberType();
            memberType.SetMemberCanEditProperty("title", true);
            memberType.SetMemberCanViewProperty("title", false);
            memberType.SetIsSensitiveProperty("title", true);

            await repository.SaveAsync(memberType, CancellationToken.None);
            scope.Complete();

            var sut = (MemberType)await repository.GetAsync(memberType.Id, CancellationToken.None);

            Assert.That(sut.MemberCanEditProperty("title"), Is.True);
            Assert.That(sut.MemberCanViewProperty("title"), Is.False);
            Assert.That(sut.IsSensitiveProperty("title"), Is.True);
        }
    }

    [Test]
    public async Task Can_Delete_MemberType()
    {
        // Arrange
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository(provider);

            // Act
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            await repository.SaveAsync(memberType, CancellationToken.None);

            var contentType2 = await repository.GetAsync(memberType.Id, CancellationToken.None);
            await repository.DeleteAsync(contentType2, CancellationToken.None);

            var exists = await repository.ExistsAsync(memberType.Id, CancellationToken.None);

            // Assert
            Assert.That(exists, Is.False);
        }
    }
}
