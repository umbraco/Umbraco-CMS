// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true)]
internal sealed class MemberCacheServiceTests : UmbracoIntegrationTestWithContent
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddUmbracoHybridCache();
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();

        // Use JSON to allow easier verification of data.
        builder.Services.PostConfigure<NuCacheSettings>(options => options.NuCacheSerializerType = NuCacheSerializerType.JSON);
    }

    private ISqlContext SqlContext => GetRequiredService<ISqlContext>();

    private IMemberCacheService MemberCacheService => GetRequiredService<IMemberCacheService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IMemberType MemberType { get; set; }

    private IMember Member { get; set; }

    public override void CreateTestData()
    {
        base.CreateTestData();

        // Create and Save Member "MemberItem" based on "Member" member type
        MemberType = MemberTypeService.Get("Member")!;
        Member = new MemberBuilder()
            .WithMemberType(MemberType)
            .WithLogin("testmember", "password123")
            .WithEmail("test@example.com")
            .WithName("Test Member")
            .Build();
        MemberService.Save(Member);
    }

    [Test]
    public void Rebuild_Creates_Member_Database_Cache_Records_For_Member_Type()
    {
        // Arrange - member is created in Setup()

        // Act - Call Rebuild for the member type
        MemberCacheService.Rebuild([MemberType.Id]);

        // Assert - Verify cmsContentNu table has records for the member item
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == Member.Id);

            var dtos = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql);

            // Verify member has cache entry
            Assert.That(dtos, Has.Count.EqualTo(1), "MemberItem should have exactly one cache entry");

            // Verify cache data is not empty
            var memberItemDto = dtos.First();
            Assert.That(memberItemDto.Data, Is.Not.Null.And.Not.Empty, "Cache data should not be empty");

            // Verify the cached data contains expected structure
            Assert.That(memberItemDto.Data, Does.Contain("\"pd\":"), "Cache data should contain property data");
            Assert.That(memberItemDto.Data, Does.Contain("\"us\":\"test-member\""), "Cache data should contain the url segment");
        }
    }

    [Test]
    public void Rebuild_Replaces_Existing_Member_Database_Cache_Records()
    {
        // Arrange - First rebuild to create initial records
        MemberCacheService.Rebuild([MemberType.Id]);

        // Get initial data
        string initialData;
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == Member.Id);
            var dto = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql).FirstOrDefault();
            Assert.That(dto, Is.Not.Null);
            initialData = dto!.Data!;
        }

        // Modify member
        Member.Name = "Modified Member Name";
        MemberService.Save(Member);

        // Act - Rebuild again
        MemberCacheService.Rebuild([MemberType.Id]);

        // Assert - Verify record was updated (not duplicated)
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == Member.Id);
            var dtos = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql);

            // Should have exactly one record (no duplicates)
            Assert.That(dtos, Has.Count.EqualTo(1), "Should have exactly one cache record");

            // Data should be different (updated)
            var updatedData = dtos[0].Data;
            Assert.That(updatedData, Does.Contain("modified-member-name"), "Cache data should contain the modified name");
        }
    }
}
