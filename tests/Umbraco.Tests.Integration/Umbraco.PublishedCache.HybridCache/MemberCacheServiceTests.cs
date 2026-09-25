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

    private IDatabaseCacheRebuilder DatabaseCacheRebuilder => GetRequiredService<IDatabaseCacheRebuilder>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IMemberType MemberType { get; set; }

    private IMember Member { get; set; }

    public override async Task CreateTestDataAsync()
    {
        await base.CreateTestDataAsync();

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
    public async Task FullRebuild_Does_Not_Create_Member_Database_Cache_Records()
    {
        // Arrange - a member and content are created in Setup()

        // Act - full rebuild (the "Rebuild Database Cache" dashboard button path)
        await DatabaseCacheRebuilder.RebuildAsync(false);

        // Assert - members are mapped from the entity on read, so they get no database cache records,
        // while documents in the same rebuild do.
        Assert.Multiple(() =>
        {
            Assert.That(GetCacheRecords(Member.Id), Is.Empty, "Member should have no cache entry");
            Assert.That(GetCacheRecords(Textpage.Id), Is.Not.Empty, "Document should have a cache entry");
        });
    }

    [Test]
    public async Task FullRebuild_Removes_Existing_Member_Database_Cache_Records()
    {
        // Arrange - a record left behind by a version that cached members
        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            ScopeAccessor.AmbientScope!.Database.Insert(new ContentNuDto
            {
                NodeId = Member.Id,
                Published = false,
                Data = "{}",
                Rv = 1,
            });
        }

        Assume.That(GetCacheRecords(Member.Id), Is.Not.Empty);

        // Act
        await DatabaseCacheRebuilder.RebuildAsync(false);

        // Assert
        Assert.That(GetCacheRecords(Member.Id), Is.Empty, "Existing member cache entries should be removed");
    }

    [Test]
    public void Rebuild_Does_Not_Create_Member_Database_Cache_Records()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        MemberCacheService.Rebuild([MemberType.Id]);
#pragma warning restore CS0618 // Type or member is obsolete

        Assert.That(GetCacheRecords(Member.Id), Is.Empty);
    }

    private List<ContentNuDto> GetCacheRecords(int nodeId)
    {
        using var scope = ScopeProvider.CreateScope(autoComplete: true);

        var selectSql = SqlContext.Sql()
            .Select<ContentNuDto>()
            .From<ContentNuDto>()
            .Where<ContentNuDto>(x => x.NodeId == nodeId);

        return ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql);
    }
}
