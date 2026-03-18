// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Models;

/// <summary>
/// Contains unit tests for validating path-related functionality in the Umbraco infrastructure models.
/// </summary>
[TestFixture]
public class PathValidationTests
{
    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void SetUp() => _builder = new EntitySlimBuilder();

    private EntitySlimBuilder _builder;

    /// <summary>
    /// Tests the ValidatePath method to ensure it correctly validates entity paths under various conditions.
    /// </summary>
    [Test]
    public void Validate_Path()
    {
        var entity = _builder
            .WithoutIdentity()
            .Build();

        // it's empty with no id so we need to allow it
        Assert.IsTrue(entity.ValidatePath());

        entity.Id = 1234;

        // it has an id but no path, so we can't allow it
        Assert.IsFalse(entity.ValidatePath());

        entity.Path = "-1";

        // invalid path
        Assert.IsFalse(entity.ValidatePath());

        entity.Path = string.Concat("-1,", entity.Id);

        // valid path
        Assert.IsTrue(entity.ValidatePath());
    }

    /// <summary>
    /// Tests that the EnsureValidPath method throws an InvalidOperationException when the entity has no Id.
    /// </summary>
    [Test]
    public void Ensure_Path_Throws_Without_Id()
    {
        var entity = _builder
            .WithoutIdentity()
            .Build();

        // no id assigned
        Assert.Throws<InvalidOperationException>(() => entity.EnsureValidPath(
            Mock.Of<ILogger<EntitySlim>>(),
            umbracoEntity => new EntitySlim(),
            umbracoEntity =>
            {
            }));
    }

    /// <summary>
    /// Ensures that the path validation throws an exception when there is no parent entity.
    /// </summary>
    [Test]
    public void Ensure_Path_Throws_Without_Parent()
    {
        var entity = _builder
            .WithId(1234)
            .WithNoParentId()
            .Build();

        // no parent found
        Assert.Throws<NullReferenceException>(() =>
            entity.EnsureValidPath(Mock.Of<ILogger<EntitySlim>>(), umbracoEntity => null, umbracoEntity => { }));
    }

    /// <summary>
    /// Ensures that the path entity is correctly set at the root level.
    /// </summary>
    [Test]
    public void Ensure_Path_Entity_At_Root()
    {
        var entity = _builder
            .WithId(1234)
            .Build();

        entity.EnsureValidPath(Mock.Of<ILogger<EntitySlim>>(), umbracoEntity => null, umbracoEntity => { });

        // works because it's under the root
        Assert.AreEqual("-1,1234", entity.Path);
    }

    /// <summary>
    /// Verifies that <c>EnsureValidPath</c> correctly sets the entity's path when a valid parent entity exists.
    /// Ensures that the resulting path includes both the parent's path and the entity's own ID.
    /// </summary>
    [Test]
    public void Ensure_Path_Entity_Valid_Parent()
    {
        var entity = _builder
            .WithId(1234)
            .WithParentId(888)
            .Build();

        entity.EnsureValidPath(
            Mock.Of<ILogger<EntitySlim>>(),
            umbracoEntity => umbracoEntity.ParentId == 888 ? new EntitySlim { Id = 888, Path = "-1,888" } : null,
            umbracoEntity => { });

        // works because the parent was found
        Assert.AreEqual("-1,888,1234", entity.Path);
    }

    /// <summary>
    /// Tests that the EnsureValidPath method correctly updates the Path property
    /// of an entity and its recursive parents to reflect the proper hierarchical path.
    /// </summary>
    [Test]
    public void Ensure_Path_Entity_Valid_Recursive_Parent()
    {
        var parentA = _builder
            .WithId(999)
            .Build();

        // Re-creating the class-level builder as we need to reset before usage when creating multiple entities.
        _builder = new EntitySlimBuilder();
        var parentB = _builder
            .WithId(888)
            .WithParentId(999)
            .Build();

        _builder = new EntitySlimBuilder();
        var parentC = _builder
            .WithId(777)
            .WithParentId(888)
            .Build();

        _builder = new EntitySlimBuilder();
        var entity = _builder
            .WithId(1234)
            .WithParentId(777)
            .Build();

        IUmbracoEntity GetParent(IUmbracoEntity umbracoEntity)
        {
            switch (umbracoEntity.ParentId)
            {
                case 999:
                    return parentA;
                case 888:
                    return parentB;
                case 777:
                    return parentC;
                case 1234:
                    return entity;
                default:
                    return null;
            }
        }

        // this will recursively fix all paths
        entity.EnsureValidPath(Mock.Of<ILogger<IUmbracoEntity>>(), GetParent, umbracoEntity => { });

        Assert.AreEqual("-1,999", parentA.Path);
        Assert.AreEqual("-1,999,888", parentB.Path);
        Assert.AreEqual("-1,999,888,777", parentC.Path);
        Assert.AreEqual("-1,999,888,777,1234", entity.Path);
    }
}
