// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Mapping.Permissions;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Mapping.Permissions;

[TestFixture]
public class ElementPermissionMapperTests
{
    private readonly Mock<IElementPermissionService> _elementPermissionService = new(MockBehavior.Strict);

    private ElementPermissionMapper CreateMapper()
        => new(
            new Lazy<IEntityService>(() => Mock.Of<IEntityService>()),
            new Lazy<IUserService>(() => Mock.Of<IUserService>()),
            new Lazy<IElementPermissionService>(() => _elementPermissionService.Object));

    [Test]
    public void AggregatePresentationModels_Resolves_Permissions_Through_ElementPermissionService()
    {
        // Arrange
        var user = Mock.Of<IUser>();
        var elementKey = Guid.NewGuid();
        var models = new IPermissionPresentationModel[]
        {
            new ElementPermissionPresentationModel
            {
                Element = new ReferenceByIdModel(elementKey),

                // The verbs on the incoming model are the raw, per-group granular permissions and must be ignored;
                // the aggregate verbs come from the permission service.
                Verbs = new HashSet<string> { "raw-ignored" },
            },
        };

        // The service is the single source of truth for the aggregated permissions, so a custom implementation is respected.
        _elementPermissionService
            .Setup(x => x.GetPermissionsAsync(user, It.Is<IEnumerable<Guid>>(keys => keys.Single() == elementKey)))
            .ReturnsAsync([new NodePermissions { NodeKey = elementKey, Permissions = new HashSet<string> { "X", "Y" } }]);

        // Act
        ElementPermissionPresentationModel[] result = CreateMapper()
            .AggregatePresentationModels(user, models)
            .Cast<ElementPermissionPresentationModel>()
            .ToArray();

        // Assert
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].Element.Id, Is.EqualTo(elementKey));
        Assert.That(result[0].Verbs, Is.EquivalentTo(new[] { "X", "Y" }));
    }
}
