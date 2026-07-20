// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

internal sealed partial class UserServiceTests
{
    /// <summary>
    ///     Performance benchmark for <see cref="IUserService.GetDocumentPermissionsAsync"/> with 50 sibling nodes
    ///     under a shared parent, which is the typical collection view scenario.
    ///     A small mix of explicit permissions is applied so that the permission inheritance
    ///     resolution logic is exercised realistically.
    /// </summary>
    /// <remarks>
    ///     Run with: dotnet test tests/Umbraco.Tests.Integration --filter "GetDocumentPermissionsAsync_Performance_50_Siblings"
    /// </remarks>
    [Test]
    [LongRunning]
    [Explicit]
    public async Task GetDocumentPermissionsAsync_Performance_50_Siblings()
    {
        const int childCount = 50;
        const int iterations = 5;

        // Arrange — user and group
        var userGroup = await CreateTestUserGroup();
        var user = UserService.CreateUserWithIdentity("perftest", "perftest@test.com");
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        // Arrange — content tree: root → parent → 50 children
        var contentType = ContentTypeBuilder.CreateSimpleContentType();
        contentType.AllowedTemplates = null;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var root = ContentBuilder.CreateSimpleContent(contentType, "root");
        ContentService.Save(root);

        var parent = ContentBuilder.CreateSimpleContent(contentType, "parent", root.Id);
        ContentService.Save(parent);

        var children = new Content[childCount];
        for (var i = 0; i < childCount; i++)
        {
            children[i] = ContentBuilder.CreateSimpleContent(contentType, $"child-{i}", parent.Id);
        }

        ContentService.Save(children);

        // Arrange — realistic permission mix:
        //   - root: explicit Browse + Delete (inherited by most nodes)
        //   - parent: explicit Browse + Move (overrides root for its subtree)
        //   - child-0 through child-4: explicit Browse + Publish (overrides parent)
        //   - child-5 through child-49: inherit from parent
        ContentService.SetPermission(root, ActionBrowse.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(root, ActionDelete.ActionLetter, new[] { userGroup.Id });

        ContentService.SetPermission(parent, ActionBrowse.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(parent, ActionMove.ActionLetter, new[] { userGroup.Id });

        for (var i = 0; i < 5; i++)
        {
            ContentService.SetPermission(children[i], ActionBrowse.ActionLetter, new[] { userGroup.Id });
            ContentService.SetPermission(children[i], ActionPublish.ActionLetter, new[] { userGroup.Id });
        }

        var childKeys = children.Select(c => c.Key).ToArray();

        // Warm-up — prime any caches / JIT
        await UserService.GetDocumentPermissionsAsync(user.Key, childKeys);

        // Act — measure multiple iterations
        var timings = new long[iterations];
        for (var i = 0; i < iterations; i++)
        {
            var watch = Stopwatch.StartNew();
            var result = await UserService.GetDocumentPermissionsAsync(user.Key, childKeys);
            watch.Stop();
            timings[i] = watch.ElapsedMilliseconds;

            // Basic correctness assertion on every iteration
            Assert.IsTrue(result.Success);
            Assert.AreEqual(childCount, result.Result.Count());
        }

        var median = timings.OrderBy(t => t).ElementAt(iterations / 2);
        var min = timings.Min();
        var max = timings.Max();

        Debug.Print(
            "GetDocumentPermissionsAsync — {0} children, {1} iterations: median={2}ms, min={3}ms, max={4}ms, all=[{5}]",
            childCount,
            iterations,
            median,
            min,
            max,
            string.Join(", ", timings.Select(t => $"{t}ms")));

        // Verify correctness of the permission mix:
        var finalResult = await UserService.GetDocumentPermissionsAsync(user.Key, childKeys);
        var permissionsByKey = finalResult.Result.ToDictionary(p => p.NodeKey, p => p.Permissions);

        // child-0 through child-4: explicit Browse + Publish
        for (var i = 0; i < 5; i++)
        {
            var perms = permissionsByKey[children[i].Key];
            Assert.IsTrue(perms.Contains(ActionBrowse.ActionLetter), $"child-{i} should have Browse");
            Assert.IsTrue(perms.Contains(ActionPublish.ActionLetter), $"child-{i} should have Publish");
            Assert.IsFalse(perms.Contains(ActionMove.ActionLetter), $"child-{i} should NOT have Move (overridden)");
            Assert.IsFalse(perms.Contains(ActionDelete.ActionLetter), $"child-{i} should NOT have Delete (overridden)");
        }

        // child-5 through child-49: inherited from parent (Browse + Move)
        for (var i = 5; i < childCount; i++)
        {
            var perms = permissionsByKey[children[i].Key];
            Assert.IsTrue(perms.Contains(ActionBrowse.ActionLetter), $"child-{i} should inherit Browse from parent");
            Assert.IsTrue(perms.Contains(ActionMove.ActionLetter), $"child-{i} should inherit Move from parent");
            Assert.IsFalse(perms.Contains(ActionDelete.ActionLetter), $"child-{i} should NOT have Delete (parent overrides root)");
        }

        // Log the result so it appears in test output
        TestContext.WriteLine(
            "GetDocumentPermissionsAsync — {0} children, {1} iterations: median={2}ms, min={3}ms, max={4}ms, all=[{5}]",
            childCount,
            iterations,
            median,
            min,
            max,
            string.Join(", ", timings.Select(t => $"{t}ms")));
    }
}
