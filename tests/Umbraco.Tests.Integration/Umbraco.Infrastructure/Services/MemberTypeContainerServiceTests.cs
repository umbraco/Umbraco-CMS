using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests covering the MemberTypeContainerService.
/// </summary>
[TestFixture]
internal sealed class MemberTypeContainerServiceTests : EntityTypeContainerServiceTestsBase<IMemberType>
{
    private IMemberTypeContainerService MemberTypeContainerService => GetRequiredService<IMemberTypeContainerService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    protected override IEntityTypeContainerService<IMemberType> ContainerService => MemberTypeContainerService;

    protected override async Task<Guid> CreateContainedEntityAsync(EntityContainer container)
    {
        var memberType = new MemberType(ShortStringHelper, container.Id)
        {
            Alias = $"alias{Guid.NewGuid():N}",
            Name = $"Name {Guid.NewGuid():N}",
        };

        var result = await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create member type: {result.Result}");
        return memberType.Key;
    }
}
