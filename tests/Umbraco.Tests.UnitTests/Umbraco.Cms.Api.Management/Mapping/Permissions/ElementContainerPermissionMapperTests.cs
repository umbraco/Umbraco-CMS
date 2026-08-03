// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Mapping.Permissions;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup.Permissions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Mapping.Permissions;

[TestFixture]
public class ElementContainerPermissionMapperTests
{
    private readonly Mock<IElementContainerPermissionService> _elementContainerPermissionService = new(MockBehavior.Strict);

    [Test]
    public void MapFromDto_Maps_Key_And_Permission()
    {
        var key = Guid.NewGuid();
        var dto = new UserGroup2GranularPermissionDto
        {
            UniqueId = key,
            Permission = "Umb.ElementContainer.Create",
            Context = ElementContainerGranularPermission.ContextType,
        };

        IGranularPermission result = CreateMapper().MapFromDto(dto);

        var typed = result as ElementContainerGranularPermission;
        Assert.That(typed, Is.Not.Null);
        Assert.That(typed!.Key, Is.EqualTo(key));
        Assert.That(typed.Permission, Is.EqualTo("Umb.ElementContainer.Create"));
    }

    [Test]
    public void MapManyAsync_Groups_By_Key_Into_One_Presentation_Model_With_Combined_Verbs()
    {
        var folderKey = Guid.NewGuid();
        IGranularPermission[] granular =
        [
            new ElementContainerGranularPermission { Key = folderKey, Permission = "Umb.ElementContainer.Create" },
            new ElementContainerGranularPermission { Key = folderKey, Permission = "Umb.Element.Create" }
        ];

        ElementContainerPermissionPresentationModel[] result = CreateMapper()
            .MapManyAsync(granular)
            .Cast<ElementContainerPermissionPresentationModel>()
            .ToArray();

        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].ElementContainer.Id, Is.EqualTo(folderKey));
        Assert.That(result[0].Verbs, Is.EquivalentTo(new[] { "Umb.ElementContainer.Create", "Umb.Element.Create" }));
    }

    [Test]
    public void MapToGranularPermissions_Yields_One_Row_Per_Verb()
    {
        var folderKey = Guid.NewGuid();
        var model = new ElementContainerPermissionPresentationModel
        {
            ElementContainer = new ReferenceByIdModel(folderKey),
            Verbs = new HashSet<string> { "Umb.ElementContainer.Create", "Umb.Element.Create" },
        };

        ElementContainerGranularPermission[] result = CreateMapper()
            .MapToGranularPermissions(model)
            .Cast<ElementContainerGranularPermission>()
            .ToArray();

        Assert.That(result, Has.Length.EqualTo(2));
        Assert.That(result.All(r => r.Key == folderKey), Is.True);
        Assert.That(result.Select(r => r.Permission), Is.EquivalentTo(new[] { "Umb.ElementContainer.Create", "Umb.Element.Create" }));
    }

    [Test]
    public void MapToGranularPermissions_With_Empty_Verbs_Yields_One_Explicit_No_Access_Row()
    {
        var folderKey = Guid.NewGuid();
        var model = new ElementContainerPermissionPresentationModel
        {
            ElementContainer = new ReferenceByIdModel(folderKey),
            Verbs = new HashSet<string>(),
        };

        ElementContainerGranularPermission[] result = CreateMapper()
            .MapToGranularPermissions(model)
            .Cast<ElementContainerGranularPermission>()
            .ToArray();

        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].Key, Is.EqualTo(folderKey));
        Assert.That(result[0].Permission, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Can_Aggregate_Permissions_Through_Element_Container_Permission_Service()
    {
        var user = Mock.Of<IUser>();
        var folderKey = Guid.NewGuid();
        var models = new IPermissionPresentationModel[]
        {
            new ElementContainerPermissionPresentationModel
            {
                ElementContainer = new ReferenceByIdModel(folderKey),
                Verbs = new HashSet<string> { "raw-ignored" },
            },
        };

        _elementContainerPermissionService
            .Setup(x => x.GetPermissionsAsync(user, It.Is<IEnumerable<Guid>>(keys => keys.Single() == folderKey)))
            .ReturnsAsync([new NodePermissions { NodeKey = folderKey, Permissions = new HashSet<string> { "X", "Y" } }]);

        ElementContainerPermissionPresentationModel[] result = CreateMapper()
            .AggregatePresentationModels(user, models)
            .Cast<ElementContainerPermissionPresentationModel>()
            .ToArray();

        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].ElementContainer.Id, Is.EqualTo(folderKey));
        Assert.That(result[0].Verbs, Is.EquivalentTo(new[] { "X", "Y" }));
    }

    private ElementContainerPermissionMapper CreateMapper()
        => new(new Lazy<IElementContainerPermissionService>(() => _elementContainerPermissionService.Object));
}
