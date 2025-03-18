// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests covering the UserService
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class UserServiceTests : UmbracoIntegrationTest
{
    private UserService UserService => (UserService)GetRequiredService<IUserService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IFileService FileService => GetRequiredService<IFileService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    [Test]
    public void Get_User_Permissions_For_Unassigned_Permission_Nodes()
    {
        // Arrange
        var user = CreateTestUser(out _);

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        Content[] content =
        {
            ContentBuilder.CreateSimpleContent(contentType), ContentBuilder.CreateSimpleContent(contentType),
            ContentBuilder.CreateSimpleContent(contentType)
        };
        ContentService.Save(content);

        // Act
        var permissions = UserService.GetPermissions(user, content[0].Id, content[1].Id, content[2].Id).ToArray();

        // Assert
        Assert.AreEqual(3, permissions.Length);
        Assert.AreEqual(17, permissions[0].AssignedPermissions.Count);
        Assert.AreEqual(17, permissions[1].AssignedPermissions.Count);
        Assert.AreEqual(17, permissions[2].AssignedPermissions.Count);
    }

    [Test]
    public void Get_User_Permissions_For_Assigned_Permission_Nodes()
    {
        // Arrange
        var user = CreateTestUser(out var userGroup);

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        Content[] content =
        {
            ContentBuilder.CreateSimpleContent(contentType), ContentBuilder.CreateSimpleContent(contentType),
            ContentBuilder.CreateSimpleContent(contentType)
        };
        ContentService.Save(content);
        ContentService.SetPermission(content[0], ActionBrowse.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content[0], ActionDelete.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content[0], ActionMove.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content[1], ActionBrowse.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content[1], ActionDelete.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content[2], ActionBrowse.ActionLetter, new[] { userGroup.Id });

        // Act
        var permissions = UserService.GetPermissions(user, content[0].Id, content[1].Id, content[2].Id).OrderBy(x=>x.EntityId).ToArray();

        // Assert
        Assert.AreEqual(3, permissions.Length);
        Assert.AreEqual(3, permissions[0].AssignedPermissions.Count);
        Assert.AreEqual(2, permissions[1].AssignedPermissions.Count);
        Assert.AreEqual(1, permissions[2].AssignedPermissions.Count);
    }

    [Test]
    public void Get_UserGroup_Assigned_Permissions()
    {
        // Arrange
        var userGroup = CreateTestUserGroup();

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        Content[] content =
        {
            ContentBuilder.CreateSimpleContent(contentType), ContentBuilder.CreateSimpleContent(contentType),
            ContentBuilder.CreateSimpleContent(contentType)
        };
        ContentService.Save(content);
        ContentService.SetPermission(content.ElementAt(0), ActionBrowse.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content.ElementAt(0), ActionDelete.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content.ElementAt(0), ActionMove.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content.ElementAt(1), ActionBrowse.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content.ElementAt(1), ActionDelete.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content.ElementAt(2), ActionBrowse.ActionLetter, new[] { userGroup.Id });

        // Act
        var permissions = UserService.GetPermissions(userGroup, false, content[0].Id, content[1].Id, content[2].Id)
            .ToArray();

        // Assert
        Assert.AreEqual(3, permissions.Length);
        Assert.AreEqual(3, permissions[0].AssignedPermissions.Count);
        Assert.AreEqual(2, permissions[1].AssignedPermissions.Count);
        Assert.AreEqual(1, permissions[2].AssignedPermissions.Count);
    }

    [Test]
    public void Get_UserGroup_Assigned_And_Default_Permissions()
    {
        // Arrange
        var userGroup = CreateTestUserGroup();

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        Content[] content =
        {
            ContentBuilder.CreateSimpleContent(contentType), ContentBuilder.CreateSimpleContent(contentType),
            ContentBuilder.CreateSimpleContent(contentType)
        };
        ContentService.Save(content);
        ContentService.SetPermission(content[0], ActionBrowse.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content[0], ActionDelete.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content[0], ActionMove.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content[1], ActionBrowse.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content[1], ActionDelete.ActionLetter, new[] { userGroup.Id });

        // Act
        var permissions = UserService.GetPermissions(userGroup, true, content[0].Id, content[1].Id, content[2].Id)
            .ToArray();

        // Assert
        Assert.AreEqual(3, permissions.Length);
        Assert.AreEqual(3, permissions[0].AssignedPermissions.Count);
        Assert.AreEqual(2, permissions[1].AssignedPermissions.Count);
        Assert.AreEqual(17, permissions[2].AssignedPermissions.Count);
    }

    [Test]
    public void Get_All_User_Permissions_For_All_Nodes_With_Explicit_Permission()
    {
        // Arrange
        var userGroup1 = CreateTestUserGroup();
        var userGroup2 = CreateTestUserGroup("test2", "Test 2");
        var userGroup3 = CreateTestUserGroup("test3", "Test 3");
        var user = UserService.CreateUserWithIdentity("John Doe", "john@umbraco.io");

        var defaultPermissionCount = userGroup3.Permissions.Count();

        user.AddGroup(userGroup1);
        user.AddGroup(userGroup2);
        user.AddGroup(userGroup3);
        UserService.Save(user);

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        Content[] content =
        {
            ContentBuilder.CreateSimpleContent(contentType), ContentBuilder.CreateSimpleContent(contentType),
            ContentBuilder.CreateSimpleContent(contentType)
        };
        ContentService.Save(content);

        // assign permissions - we aren't assigning anything explicit for group3 and nothing explicit for content[2] /w group2
        ContentService.SetPermission(content[0], ActionBrowse.ActionLetter, new[] { userGroup1.Id });
        ContentService.SetPermission(content[0], ActionDelete.ActionLetter, new[] { userGroup1.Id });
        ContentService.SetPermission(content[0], ActionMove.ActionLetter, new[] { userGroup2.Id });
        ContentService.SetPermission(content[1], ActionBrowse.ActionLetter, new[] { userGroup1.Id });
        ContentService.SetPermission(content[1], ActionDelete.ActionLetter, new[] { userGroup2.Id });
        ContentService.SetPermission(content[2], ActionDelete.ActionLetter, new[] { userGroup1.Id });

        // Act
        // we don't pass in any nodes so it will return all of them
        var result = UserService.GetPermissions(user).ToArray();
        var permissions = result
            .GroupBy(x => x.EntityId)
            .ToDictionary(x => x.Key, x => x.GroupBy(a => a.UserGroupId).ToDictionary(a => a.Key, a => a.ToArray()));

        // Assert

        // there will be 3 since that is how many content items there are
        Assert.AreEqual(3, permissions.Count);

        // test permissions contains content[0]
        Assert.IsTrue(permissions.ContainsKey(content[0].Id));

        // test that this permissions set contains permissions for all groups
        Assert.IsTrue(permissions[content[0].Id].ContainsKey(userGroup1.Id));
        Assert.IsTrue(permissions[content[0].Id].ContainsKey(userGroup2.Id));
        Assert.IsTrue(permissions[content[0].Id].ContainsKey(userGroup3.Id));

        // test that the correct number of permissions are returned for each group
        Assert.AreEqual(2, permissions[content[0].Id][userGroup1.Id].SelectMany(x => x.AssignedPermissions).Count());
        Assert.AreEqual(1, permissions[content[0].Id][userGroup2.Id].SelectMany(x => x.AssignedPermissions).Count());
        Assert.AreEqual(defaultPermissionCount,
            permissions[content[0].Id][userGroup3.Id].SelectMany(x => x.AssignedPermissions).Count());

        // test permissions contains content[1]
        Assert.IsTrue(permissions.ContainsKey(content[1].Id));

        // test that this permissions set contains permissions for all groups
        Assert.IsTrue(permissions[content[1].Id].ContainsKey(userGroup1.Id));
        Assert.IsTrue(permissions[content[1].Id].ContainsKey(userGroup2.Id));
        Assert.IsTrue(permissions[content[1].Id].ContainsKey(userGroup3.Id));

        // test that the correct number of permissions are returned for each group
        Assert.AreEqual(1, permissions[content[1].Id][userGroup1.Id].SelectMany(x => x.AssignedPermissions).Count());
        Assert.AreEqual(1, permissions[content[1].Id][userGroup2.Id].SelectMany(x => x.AssignedPermissions).Count());
        Assert.AreEqual(defaultPermissionCount,
            permissions[content[1].Id][userGroup3.Id].SelectMany(x => x.AssignedPermissions).Count());

        // test permissions contains content[2]
        Assert.IsTrue(permissions.ContainsKey(content[2].Id));

        // test that this permissions set contains permissions for all groups
        Assert.IsTrue(permissions[content[2].Id].ContainsKey(userGroup1.Id));
        Assert.IsTrue(permissions[content[2].Id].ContainsKey(userGroup2.Id));
        Assert.IsTrue(permissions[content[2].Id].ContainsKey(userGroup3.Id));

        // test that the correct number of permissions are returned for each group
        Assert.AreEqual(1, permissions[content[2].Id][userGroup1.Id].SelectMany(x => x.AssignedPermissions).Count());
        Assert.AreEqual(defaultPermissionCount,
            permissions[content[2].Id][userGroup2.Id].SelectMany(x => x.AssignedPermissions).Count());
        Assert.AreEqual(defaultPermissionCount,
            permissions[content[2].Id][userGroup3.Id].SelectMany(x => x.AssignedPermissions).Count());
    }

    [Test]
    public void Get_All_User_Group_Permissions_For_All_Nodes()
    {
        // Arrange
        var userGroup = CreateTestUserGroup();

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        Content[] content =
        {
            ContentBuilder.CreateSimpleContent(contentType), ContentBuilder.CreateSimpleContent(contentType),
            ContentBuilder.CreateSimpleContent(contentType)
        };
        ContentService.Save(content);
        ContentService.SetPermission(content[0], ActionBrowse.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content[0], ActionDelete.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content[0], ActionMove.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content[1], ActionBrowse.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content[1], ActionDelete.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(content[2], ActionDelete.ActionLetter, new[] { userGroup.Id });

        // Act
        // we don't pass in any nodes so it will return all of them
        var permissions = UserService.GetPermissions(userGroup, true)
            .GroupBy(x => x.EntityId)
            .ToDictionary(x => x.Key, x => x);

        // Assert
        Assert.AreEqual(3, permissions.Count);
        Assert.IsTrue(permissions.ContainsKey(content[0].Id));
        Assert.AreEqual(3, permissions[content[0].Id].SelectMany(x => x.AssignedPermissions).Count());
        Assert.IsTrue(permissions.ContainsKey(content[1].Id));
        Assert.AreEqual(2, permissions[content[1].Id].SelectMany(x => x.AssignedPermissions).Count());
        Assert.IsTrue(permissions.ContainsKey(content[2].Id));
        Assert.AreEqual(1, permissions[content[2].Id].SelectMany(x => x.AssignedPermissions).Count());
    }

    [Test]
    public void Calculate_Permissions_For_User_For_Path()
    {
        // Details of what this is testing were originally at http://issues.umbraco.org/issue/U4-10075#comment=67-40085 (and available
        // now at https://web.archive.org/web/20211021021851/http://issues.umbraco.org/issue/U4-10075).
        // Here is the same information copied and reformatted:

        // Here's a mockup of how user group permissions will work with inheritance.
        // User (UserX), belongs to these groups which have the following default permission characters applied:

        // | Groups | Defaults |
        // |--------|----------|
        // | G1     | sdf      |
        // | G2     | sdgk     |
        // | G3     | fg       |

        // Thus the aggregate defaults for UserX is "sdfgk"

        // As an example node structure, here is the node tree and where any explicit permissions are assigned.
        // It shows you the resulting permissions for each node that the user has:

        // | Nodes       | G1   | G2   | G3   | Result                                    |
        // |-------------|------|------|------|-------------------------------------------|
        // | - A         |      |      |      | sdfgk (Defaults)                          |
        // | - - B       |      |  fr  |      | sdfgr (Defaults + Explicit)               |
        // | - - - C     |      |      | qz   | sdfrqz (Defaults + Explicit + Inherited)  |
        // | - - - - D   |      |      |      | sdfrqz (Inherited)                        |
        // | - - - - - E |      |      |      | sdfrqz (Inherited)                        |

        // Notice that 'k' is no longer in the result for node B and below.
        // Notice that 'g' is no longer in the result for node C and below.

        const string path = "-1,1,2,3,4";
        var pathIds = path.GetIdsFromPathReversed();

        // Setup: 3 groups with different default permissions.
        // - Group A has permissions S, D, F.
        // - Group B has permissions S, D, G, K.
        // - Group C has permissions F, G.
        const int groupA = 7;
        const int groupB = 8;
        const int groupC = 9;
        var userGroups = new Dictionary<int, ISet<string>>
        {
            { groupA, new[] {"S", "D", "F"}.ToHashSet() },
            { groupB, new[] {"S", "D", "G", "K"}.ToHashSet() },
            { groupC, new[] {"F", "G"}.ToHashSet() }
        };

        // Setup: combination of default and specific permissions for each group and document.
        EntityPermission[] permissions =
        {
            new(groupA, 1, userGroups[groupA], true),                   // Comes from the default permission on group A.
            new(groupA, 2, userGroups[groupA], true),                   // Comes from the default permission on group A.
            new(groupA, 3, userGroups[groupA], true),                   // Comes from the default permission on group A.
            new(groupA, 4, userGroups[groupA], true),                   // Comes from the default permission on group A.
            new(groupB, 1, userGroups[groupB], true),                   // Comes from the default permission on group B.
            new(groupB, 2, new[] {"F", "R"}.ToHashSet(), false),        // Group B has a specific permission on document 2 of of F, R.
            new(groupB, 3, userGroups[groupB], true),                   // Comes from the default permission on group B.
            new(groupB, 4, userGroups[groupB], true),                   // Comes from the default permission on group B.
            new(groupC, 1, userGroups[groupC], true),                   // Comes from the default permission on group C.
            new(groupC, 2, userGroups[groupC], true),                   // Comes from the default permission on group C.
            new(groupC, 3, new[] {"Q", "Z"}.ToHashSet(), false),        // Group C has a specific permission on document 3 of of Q, Z.
            new(groupC, 4, userGroups[groupC], true)                    // Comes from the default permission on group C.
        };

        // Permissions for document 4
        var result = UserService.CalculatePermissionsForPathForUser(permissions, pathIds);
        Assert.AreEqual(4, result.EntityId);
        var allPermissions = result.GetAllPermissions().ToArray();
        Assert.AreEqual(6, allPermissions.Length, string.Join(",", allPermissions));

        // - has S, D, F from group A, R from group B (specific permission on document 2), Q, Z from group C (specific permission on document 3).
        // - doesn't have K or G due to specific permissions set on group B, document 2.
        Assert.IsTrue(allPermissions.ContainsAll(["S", "D", "F", "R", "Q", "Z"]));

        // Permissions for document 3
        result = UserService.CalculatePermissionsForPathForUser(permissions, pathIds.Skip(1).ToArray());
        Assert.AreEqual(3, result.EntityId);
        allPermissions = result.GetAllPermissions().ToArray();
        Assert.AreEqual(6, allPermissions.Length, string.Join(",", allPermissions));

        // - has S, D, F from group A, R from group B (specific permission on document 2), Q, Z from group C (specific permission on document 3).
        // - doesn't have K or G due to specific permissions set on group B, document 2.
        Assert.IsTrue(allPermissions.ContainsAll(["S", "D", "F", "R", "Q", "Z"]));

        // Permissions for document 2
        result = UserService.CalculatePermissionsForPathForUser(permissions, pathIds.Skip(2).ToArray());
        Assert.AreEqual(2, result.EntityId);
        allPermissions = result.GetAllPermissions().ToArray();
        Assert.AreEqual(5, allPermissions.Length, string.Join(",", allPermissions));

        // - has S, D, F from group A, G from group C, R from group B (specific permission on document 2).
        // - doesn't have K due to specific permissions set on group B, document 2.
        // - this would remove G too, but it's there as it's also defined in group C that doesn't have a specific permission override.
        // - doesn't have Q, Z as these specific permissions are added to a document further down the path.
        Assert.IsTrue(allPermissions.ContainsAll(["S", "D", "F", "G", "R"]));

        // Permissions for document 1
        result = UserService.CalculatePermissionsForPathForUser(permissions, pathIds.Skip(3).ToArray());
        Assert.AreEqual(1, result.EntityId);
        allPermissions = result.GetAllPermissions().ToArray();
        Assert.AreEqual(5, allPermissions.Length, string.Join(",", allPermissions));

        // - has S, D, F from group A, G, K from group B (also G from group C)
        Assert.IsTrue(allPermissions.ContainsAll(["S", "D", "F", "G", "K"]));
    }

    [Test]
    public void Determine_Deepest_Explicit_Permissions_For_Group_For_Path_1()
    {
        var path = "-1,1,2,3";
        var pathIds = path.GetIdsFromPathReversed();
        ISet<string> defaults = new []{ "A", "B" }.ToHashSet();
        var permissions = new List<EntityPermission>
        {
            new(9876, 1, defaults, true), new(9876, 2, new[] {"B", "C", "D"}.ToHashSet(), false), new(9876, 3, defaults, true)
        };
        var result = UserService.GetPermissionsForPathForGroup(permissions, pathIds, true);
        Assert.AreEqual(3, result.AssignedPermissions.Count);
        Assert.IsFalse(result.IsDefaultPermissions);
        Assert.IsTrue(result.AssignedPermissions.ContainsAll(new[] { "B", "C", "D" }));
        Assert.AreEqual(2, result.EntityId);
        Assert.AreEqual(9876, result.UserGroupId);
    }

    [Test]
    public void Determine_Deepest_Explicit_Permissions_For_Group_For_Path_2()
    {
        var path = "-1,1,2,3";
        var pathIds = path.GetIdsFromPathReversed();
        ISet<string> defaults = new []{ "A", "B", "C" }.ToHashSet();
        var permissions = new List<EntityPermission>
        {
            new(9876, 1, defaults, true), new(9876, 2, defaults, true), new(9876, 3, defaults, true)
        };
        var result = UserService.GetPermissionsForPathForGroup(permissions, pathIds);
        Assert.IsNull(result);
    }

    [Test]
    public void Determine_Deepest_Explicit_Permissions_For_Group_For_Path_3()
    {
        var path = "-1,1,2,3";
        var pathIds = path.GetIdsFromPathReversed();
        ISet<string> defaults = new []{ "A", "B" }.ToHashSet();
        var permissions = new List<EntityPermission>
        {
            new(9876, 1, defaults, true), new(9876, 2, defaults, true), new(9876, 3, defaults, true)
        };
        var result = UserService.GetPermissionsForPathForGroup(permissions, pathIds, true);
        Assert.AreEqual(2, result.AssignedPermissions.Count);
        Assert.IsTrue(result.IsDefaultPermissions);
        Assert.IsTrue(result.AssignedPermissions.ContainsAll(defaults));
        Assert.AreEqual(3, result.EntityId);
        Assert.AreEqual(9876, result.UserGroupId);
    }

    [Test]
    public void Get_User_Implicit_Permissions()
    {
        // Arrange
        var userGroup = CreateTestUserGroup();

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        var parent = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(parent);
        var child1 = ContentBuilder.CreateSimpleContent(contentType, "child1", parent.Id);
        ContentService.Save(child1);
        var child2 = ContentBuilder.CreateSimpleContent(contentType, "child2", child1.Id);
        ContentService.Save(child2);

        ContentService.SetPermission(parent, ActionBrowse.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(parent, ActionDelete.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(parent, ActionMove.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(parent, ActionBrowse.ActionLetter, new[] { userGroup.Id });
        ContentService.SetPermission(parent, ActionDelete.ActionLetter, new[] { userGroup.Id });

        // Act
        var permissions = UserService.GetPermissionsForPath(userGroup, child2.Path);

        // Assert
        var allPermissions = permissions.GetAllPermissions().ToArray();
        Assert.AreEqual(3, allPermissions.Length);
    }

    [Test]
    public void Can_Delete_User()
    {
        var user = UserService.CreateUserWithIdentity("JohnDoe", "john@umbraco.io");

        UserService.Delete(user, true);
        var deleted = UserService.GetUserById(user.Id);

        // Assert
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public void Disables_User_Instead_Of_Deleting_If_Flag_Not_Set()
    {
        var user = UserService.CreateUserWithIdentity("JohnDoe", "john@umbraco.io");

        UserService.Delete(user);
        var deleted = UserService.GetUserById(user.Id);

        // Assert
        Assert.That(deleted, Is.Not.Null);
    }

    [Test]
    public void Exists_By_Username()
    {
        var user = UserService.CreateUserWithIdentity("JohnDoe", "john@umbraco.io");
        var user2 = UserService.CreateUserWithIdentity("john2@umbraco.io", "john2@umbraco.io");
        Assert.IsTrue(UserService.Exists("JohnDoe"));
        Assert.IsFalse(UserService.Exists("notFound"));
        Assert.IsTrue(UserService.Exists("john2@umbraco.io"));
    }

    [Test]
    public void Get_By_Email()
    {
        var user = UserService.CreateUserWithIdentity("JohnDoe", "john@umbraco.io");

        Assert.IsNotNull(UserService.GetByEmail(user.Email));
        Assert.IsNull(UserService.GetByEmail("do@not.find"));
    }

    [Test]
    public void Get_By_Username()
    {
        var user = UserService.CreateUserWithIdentity("JohnDoe", "john@umbraco.io");

        Assert.IsNotNull(UserService.GetByUsername(user.Username));
        Assert.IsNull(UserService.GetByUsername("notFound"));
    }

    [Test]
    public void Get_By_Username_With_Backslash()
    {
        var user = UserService.CreateUserWithIdentity("mydomain\\JohnDoe", "john@umbraco.io");

        Assert.IsNotNull(UserService.GetByUsername(user.Username));
        Assert.IsNull(UserService.GetByUsername("notFound"));
    }

    [Test]
    public void Get_By_Object_Id()
    {
        var user = UserService.CreateUserWithIdentity("JohnDoe", "john@umbraco.io");

        Assert.IsNotNull(UserService.GetUserById(user.Id));
        Assert.IsNull(UserService.GetUserById(9876));
    }

    [Test]
    public void Find_By_Email_Starts_With()
    {
        var users = UserBuilder.CreateMulipleUsers(10);
        UserService.Save(users);

        // don't find this
        var customUser = UserBuilder.CreateUser();
        customUser.Email = "hello@hello.com";
        UserService.Save(customUser);

        var found = UserService.FindByEmail("tes", 0, 100, out _);

        Assert.AreEqual(10, found.Count());
    }

    [Test]
    public void Find_By_Email_Ends_With()
    {
        var users = UserBuilder.CreateMulipleUsers(10);
        UserService.Save(users);

        // include this
        var customUser = UserBuilder.CreateUser();
        customUser.Email = "hello@test.com";
        UserService.Save(customUser);

        var found = UserService.FindByEmail("test.com", 0, 100, out _, StringPropertyMatchType.EndsWith);

        Assert.AreEqual(11, found.Count());
    }

    [Test]
    public void Find_By_Email_Contains()
    {
        var users = UserBuilder.CreateMulipleUsers(10);
        UserService.Save(users);

        // include this
        var customUser = UserBuilder.CreateUser();
        customUser.Email = "hello@test.com";
        UserService.Save(customUser);

        var found = UserService.FindByEmail("test", 0, 100, out _, StringPropertyMatchType.Contains);

        Assert.AreEqual(11, found.Count());
    }

    [Test]
    public void Find_By_Email_Exact()
    {
        var users = UserBuilder.CreateMulipleUsers(10);
        UserService.Save(users);

        // include this
        var customUser = UserBuilder.CreateUser();
        customUser.Email = "hello@test.com";
        UserService.Save(customUser);

        var found = UserService.FindByEmail("hello@test.com", 0, 100, out _, StringPropertyMatchType.Exact);

        Assert.AreEqual(1, found.Count());
    }

    [Test]
    public void Get_All_Paged_Users()
    {
        var users = UserBuilder.CreateMulipleUsers(10);
        UserService.Save(users);

        var found = UserService.GetAll(0, 2, out var totalRecs);

        Assert.AreEqual(2, found.Count());

        // + 1 because of the built in admin user
        Assert.AreEqual(11, totalRecs);
        Assert.AreEqual("admin", found.First().Username);
        Assert.AreEqual("test0", found.Last().Username);
    }

    [Test]
    public void Get_All_Paged_Users_With_Filter()
    {
        var users = UserBuilder.CreateMulipleUsers(10).ToArray();
        UserService.Save(users);

        var found = UserService.GetAll(0, 2, out var totalRecs, "username", Direction.Ascending, filter: "test");

        Assert.AreEqual(2, found.Count());
        Assert.AreEqual(10, totalRecs);
        Assert.AreEqual("test0", found.First().Username);
        Assert.AreEqual("test1", found.Last().Username);
    }

    [Test]
    public void Get_All_Paged_Users_For_Group()
    {
        var userGroup = UserGroupBuilder.CreateUserGroup();
        UserService.Save(userGroup);

        var users = UserBuilder.CreateMulipleUsers(10).ToArray();
        for (var i = 0; i < 10;)
        {
            users[i].AddGroup(userGroup.ToReadOnlyGroup());
            i += 2;
        }

        UserService.Save(users);

        var found = UserService.GetAll(0, 2, out long totalRecs, "username", Direction.Ascending,
            includeUserGroups: new[] { userGroup.Alias });

        Assert.AreEqual(2, found.Count());
        Assert.AreEqual(5, totalRecs);
        Assert.AreEqual("test0", found.First().Username);
        Assert.AreEqual("test2", found.Last().Username);
    }

    [Test]
    public void Get_All_Paged_Users_For_Group_With_Filter()
    {
        var userGroup = UserGroupBuilder.CreateUserGroup();
        UserService.Save(userGroup);

        var users = UserBuilder.CreateMulipleUsers(10).ToArray();
        for (var i = 0; i < 10;)
        {
            users[i].AddGroup(userGroup.ToReadOnlyGroup());
            i += 2;
        }

        for (var i = 0; i < 10;)
        {
            users[i].Name = "blah" + users[i].Name;
            i += 3;
        }

        UserService.Save(users);

        var found = UserService.GetAll(0, 2, out long totalRecs, "username", Direction.Ascending,
            userGroups: new[] { userGroup.Alias }, filter: "blah");

        Assert.AreEqual(2, found.Count());
        Assert.AreEqual(2, totalRecs);
        Assert.AreEqual("test0", found.First().Username);
        Assert.AreEqual("test6", found.Last().Username);
    }

    [Test]
    public void Count_All_Users()
    {
        var users = UserBuilder.CreateMulipleUsers(10);
        UserService.Save(users);
        var customUser = UserBuilder.CreateUser();
        UserService.Save(customUser);

        var found = UserService.GetCount(MemberCountType.All);

        // + 1 because of the built in admin user
        Assert.AreEqual(12, found);
    }

    [Ignore("why?")]
    [Test]
    public void Count_All_Online_Users()
    {
        var users = UserBuilder.CreateMulipleUsers(10,
            (i, member) => member.LastLoginDate = DateTime.Now.AddMinutes(i * -2));
        UserService.Save(users);

        var customUser = UserBuilder.CreateUser();
        throw new NotImplementedException();
    }

    [Test]
    public void Count_All_Locked_Users()
    {
        var users = UserBuilder.CreateMulipleUsers(10, (i, member) => member.IsLockedOut = i % 2 == 0);
        UserService.Save(users);

        var customUser = UserBuilder.CreateUser();
        customUser.IsLockedOut = true;
        UserService.Save(customUser);

        var found = UserService.GetCount(MemberCountType.LockedOut);

        Assert.AreEqual(6, found);
    }

    [Test]
    public void Count_All_Approved_Users()
    {
        var users = UserBuilder.CreateMulipleUsers(10, (i, member) => member.IsApproved = i % 2 == 0);
        UserService.Save(users);

        var customUser = UserBuilder.CreateUser();
        customUser.IsApproved = false;
        UserService.Save(customUser);

        var found = UserService.GetCount(MemberCountType.Approved);

        // + 1 because of the built in admin user
        Assert.AreEqual(6, found);
    }

    [Test]
    public void Can_Persist_New_User()
    {
        // Act
        var membershipUser = UserService.CreateUserWithIdentity("JohnDoe", "john@umbraco.io");

        // Assert
        Assert.That(membershipUser.HasIdentity, Is.True);
        Assert.That(membershipUser.Id, Is.GreaterThan(0));
        IUser user = membershipUser as User;
        Assert.That(user, Is.Not.Null);
    }

    [Test]
    public void Can_Persist_New_User_With_Hashed_Password()
    {
        // Act
        // NOTE: Normally the hash'ing would be handled in the membership provider, so the service just saves the password
        var password = "123456";
        var hash = new HMACSHA1
        {
            Key = Encoding.Unicode.GetBytes(password)
        };
        var encodedPassword = Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
        var globalSettings = new GlobalSettings();
        var membershipUser = new User(globalSettings, "JohnDoe", "john@umbraco.io", encodedPassword, encodedPassword);
        UserService.Save(membershipUser);

        // Assert
        Assert.That(membershipUser.HasIdentity, Is.True);
        Assert.That(membershipUser.RawPasswordValue, Is.Not.EqualTo(password));
        Assert.That(membershipUser.RawPasswordValue, Is.EqualTo(encodedPassword));
        IUser user = membershipUser;
        Assert.That(user, Is.Not.Null);
    }

    [Test]
    public void Can_Add_And_Remove_Sections_From_UserGroup()
    {
        var userGroup = new UserGroup(ShortStringHelper) { Alias = "Group1", Name = "Group 1" };
        userGroup.AddAllowedSection("content");
        userGroup.AddAllowedSection("mediat");
        UserService.Save(userGroup);

        var result1 = UserService.GetUserGroupById(userGroup.Id);

        Assert.AreEqual(2, result1.AllowedSections.Count());

        // adds some allowed sections
        userGroup.AddAllowedSection("test1");
        userGroup.AddAllowedSection("test2");
        userGroup.AddAllowedSection("test3");
        userGroup.AddAllowedSection("test4");
        UserService.Save(userGroup);

        result1 = UserService.GetUserGroupById(userGroup.Id);

        Assert.AreEqual(6, result1.AllowedSections.Count());

        // simulate clearing the sections
        foreach (var s in userGroup.AllowedSections)
        {
            result1.RemoveAllowedSection(s);
        }

        // now just re-add a couple
        result1.AddAllowedSection("test3");
        result1.AddAllowedSection("test4");
        UserService.Save(result1);

        // Assert
        // re-get
        result1 = UserService.GetUserGroupById(userGroup.Id);
        Assert.AreEqual(2, result1.AllowedSections.Count());
    }

    [Test]
    public void Can_Remove_Section_From_All_Assigned_UserGroups()
    {
        var userGroup1 = new UserGroup(ShortStringHelper) { Alias = "Group1", Name = "Group 1" };
        var userGroup2 = new UserGroup(ShortStringHelper) { Alias = "Group2", Name = "Group 2" };
        UserService.Save(userGroup1);
        UserService.Save(userGroup2);

        // adds some allowed sections
        userGroup1.AddAllowedSection("test");
        userGroup2.AddAllowedSection("test");
        UserService.Save(userGroup1);
        UserService.Save(userGroup2);

        // now clear the section from all users
        UserService.DeleteSectionFromAllUserGroups("test");

        // Assert
        var result1 = UserService.GetUserGroupById(userGroup1.Id);
        var result2 = UserService.GetUserGroupById(userGroup2.Id);
        Assert.IsFalse(result1.AllowedSections.Contains("test"));
        Assert.IsFalse(result2.AllowedSections.Contains("test"));
    }

    [Test]
    public void Can_Add_Section_To_All_UserGroups()
    {
        var userGroup1 = new UserGroup(ShortStringHelper) { Alias = "Group1", Name = "Group 1" };
        userGroup1.AddAllowedSection("test");

        var userGroup2 = new UserGroup(ShortStringHelper) { Alias = "Group2", Name = "Group 2" };
        userGroup2.AddAllowedSection("test");

        var userGroup3 = new UserGroup(ShortStringHelper) { Alias = "Group3", Name = "Group 3" };
        UserService.Save(userGroup1);
        UserService.Save(userGroup2);
        UserService.Save(userGroup3);

        // Assert
        var result1 = UserService.GetUserGroupById(userGroup1.Id);
        var result2 = UserService.GetUserGroupById(userGroup2.Id);
        var result3 = UserService.GetUserGroupById(userGroup3.Id);
        Assert.IsTrue(result1.AllowedSections.Contains("test"));
        Assert.IsTrue(result2.AllowedSections.Contains("test"));
        Assert.IsFalse(result3.AllowedSections.Contains("test"));

        // now add the section to all groups
        foreach (var userGroup in new[] { userGroup1, userGroup2, userGroup3 })
        {
            userGroup.AddAllowedSection("test");
            UserService.Save(userGroup);
        }

        // Assert
        result1 = UserService.GetUserGroupById(userGroup1.Id);
        result2 = UserService.GetUserGroupById(userGroup2.Id);
        result3 = UserService.GetUserGroupById(userGroup3.Id);
        Assert.IsTrue(result1.AllowedSections.Contains("test"));
        Assert.IsTrue(result2.AllowedSections.Contains("test"));
        Assert.IsTrue(result3.AllowedSections.Contains("test"));
    }

    [Test]
    public void Cannot_Create_User_With_Empty_Username() =>
        // Act & Assert
        Assert.Throws<ArgumentException>(() => UserService.CreateUserWithIdentity(string.Empty, "john@umbraco.io"));

    [Test]
    public void Cannot_Save_User_With_Empty_Username()
    {
        // Arrange
        var user = UserService.CreateUserWithIdentity("John Doe", "john@umbraco.io");
        user.Username = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => UserService.Save(user));
    }

    [Test]
    public void Cannot_Save_User_With_Empty_Name()
    {
        // Arrange
        var user = UserService.CreateUserWithIdentity("John Doe", "john@umbraco.io");
        user.Name = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => UserService.Save(user));
    }

    [Test]
    public void Get_By_Profile_Username()
    {
        // Arrange
        var user = UserService.CreateUserWithIdentity("test1", "test1@test.com");

        // Act
        var profile = UserService.GetProfileByUserName(user.Username);

        // Assert
        Assert.IsNotNull(profile);
        Assert.AreEqual(user.Username, profile.Name);
        Assert.AreEqual(user.Id, profile.Id);
    }

    [Test]
    public void Get_By_Profile_Id()
    {
        // Arrange
        var user = UserService.CreateUserWithIdentity("test1", "test1@test.com");

        // Act
        var profile = UserService.GetProfileById(user.Id);

        // Assert
        Assert.IsNotNull(profile);
        Assert.AreEqual(user.Username, profile.Name);
        Assert.AreEqual(user.Id, profile.Id);
    }

    [Test]
    public void Get_By_Profile_Id_Must_Return_Null_If_User_Does_Not_Exist()
    {
        var profile = UserService.GetProfileById(42);

        // Assert
        Assert.IsNull(profile);
    }

    [Test]
    public void GetProfilesById_Must_Return_Empty_If_User_Does_Not_Exist()
    {
        var profiles = UserService.GetProfilesById(42);

        // Assert
        CollectionAssert.IsEmpty(profiles);
    }

    [Test]
    public void Get_User_By_Username()
    {
        // Arrange
        var originalUser = CreateTestUser(out _);

        // Act
        var updatedItem = (User)UserService.GetByUsername(originalUser.Username);

        // Assert
        Assert.IsNotNull(updatedItem);
        Assert.That(updatedItem.Id, Is.EqualTo(originalUser.Id));
        Assert.That(updatedItem.Name, Is.EqualTo(originalUser.Name));
        Assert.That(updatedItem.Language, Is.EqualTo(originalUser.Language));
        Assert.That(updatedItem.IsApproved, Is.EqualTo(originalUser.IsApproved));
        Assert.That(updatedItem.RawPasswordValue, Is.EqualTo(originalUser.RawPasswordValue));
        Assert.That(updatedItem.IsLockedOut, Is.EqualTo(originalUser.IsLockedOut));
        Assert.IsTrue(updatedItem.StartContentIds.UnsortedSequenceEqual(originalUser.StartContentIds));
        Assert.IsTrue(updatedItem.StartMediaIds.UnsortedSequenceEqual(originalUser.StartMediaIds));
        Assert.That(updatedItem.Email, Is.EqualTo(originalUser.Email));
        Assert.That(updatedItem.Username, Is.EqualTo(originalUser.Username));
        Assert.That(updatedItem.AllowedSections.Count(), Is.EqualTo(originalUser.AllowedSections.Count()));
    }

    [Test]
    public void Can_Get_Assigned_StartNodes_For_User()
    {
        var startContentItems = BuildContentItems(3);

        var testUserGroup = CreateTestUserGroup();

        var userGroupId = testUserGroup.Id;

        CreateTestUsers(startContentItems.Select(x => x.Id).ToArray(), testUserGroup, 3);

        var usersInGroup = UserService.GetAllInGroup(userGroupId);

        foreach (var user in usersInGroup)
        {
            Assert.AreEqual(user.StartContentIds.Length, startContentItems.Length);
        }
    }

    [TestCase(UserKind.Default, UserClientCredentialsOperationStatus.InvalidUser)]
    [TestCase(UserKind.Api, UserClientCredentialsOperationStatus.Success)]
    public async Task Can_Assign_ClientId_To_Api_User(UserKind userKind, UserClientCredentialsOperationStatus expectedResult)
    {
        // Arrange
        var user = await CreateTestUser(userKind);

        // Act
        var result = await UserService.AddClientIdAsync(user.Key, "client-one");

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    [TestCase("abcdefghijklmnopqrstuvwxyz", UserClientCredentialsOperationStatus.Success)]
    [TestCase("ABCDEFGHIJKLMNOPQRSTUVWXYZ", UserClientCredentialsOperationStatus.Success)]
    [TestCase("1234567890", UserClientCredentialsOperationStatus.Success)]
    [TestCase(".-_~", UserClientCredentialsOperationStatus.Success)]
    [TestCase("!", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase("#", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase("$", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase("&", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase("'", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase("(", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase(")", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase("*", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase("+", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase(",", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase("/", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase(":", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase(";", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase("=", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase("?", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase("@", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase("[", UserClientCredentialsOperationStatus.InvalidClientId)]
    [TestCase("]", UserClientCredentialsOperationStatus.InvalidClientId)]
    public async Task Can_Use_Only_Unreserved_Characters_For_ClientId(string clientId, UserClientCredentialsOperationStatus expectedResult)
    {
        // Arrange
        var user = await CreateTestUser(UserKind.Api);

        // Act
        var result = await UserService.AddClientIdAsync(user.Key, clientId);

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    [TestCase("Less_Than_100_characters_0123456789012345678901234567890123456789012345678901234567890123456789", UserClientCredentialsOperationStatus.Success)]
    [TestCase("More_Than_100_characters_01234567890123456789012345678901234567890123456789012345678901234567890123456789", UserClientCredentialsOperationStatus.InvalidClientId)]
    public async Task Cannot_Create_Too_Long_ClientId(string clientId, UserClientCredentialsOperationStatus expectedResult)
    {
        // Arrange
        var user = await CreateTestUser(UserKind.Api);

        // Act
        var result = await UserService.AddClientIdAsync(user.Key, clientId);

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    private Content[] BuildContentItems(int numberToCreate)
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        var startContentItems = new List<Content>();

        for (var i = 0; i < numberToCreate; i++)
        {
            startContentItems.Add(ContentBuilder.CreateSimpleContent(contentType));
        }

        ContentService.Save(startContentItems);

        return startContentItems.ToArray();
    }

    private IUser CreateTestUser(out IUserGroup userGroup)
    {
        userGroup = CreateTestUserGroup();

        var user = UserService.CreateUserWithIdentity("test1", "test1@test.com");

        user.AddGroup(userGroup.ToReadOnlyGroup());

        UserService.Save(user);

        return user;
    }

    private async Task<IUser> CreateTestUser(UserKind userKind)
    {
        var userGroup = CreateTestUserGroup();

        var result = await UserService.CreateAsync(
            Constants.Security.SuperUserKey,
            new UserCreateModel
            {
                Email = "test1@test.com",
                Id = Guid.NewGuid(),
                Kind = userKind,
                Name = "test1@test.com",
                UserName = "test1@test.com",
                UserGroupKeys = new HashSet<Guid> { userGroup.Key }
            });
        Assert.IsTrue(result.Success);
        return result.Result.CreatedUser!;
    }

    private List<IUser> CreateTestUsers(int[] startContentIds, IUserGroup userGroup, int numberToCreate)
    {
        var users = new List<IUser>();

        for (var i = 0; i < numberToCreate; i++)
        {
            var user = UserService.CreateUserWithIdentity($"test{i}", $"test{i}@test.com");
            user.AddGroup(userGroup.ToReadOnlyGroup());

            var updateable = (User)user;
            updateable.StartContentIds = startContentIds;

            UserService.Save(user);

            users.Add(user);
        }

        return users;
    }

    private UserGroup CreateTestUserGroup(string alias = "testGroup", string name = "Test Group")
    {
        var permissions = "ABCDEFGHIJ1234567".ToCharArray().Select(x => x.ToString()).ToHashSet();
        var userGroup = UserGroupBuilder.CreateUserGroup(alias, name, permissions: permissions);

        UserService.Save(userGroup);

        return userGroup;
    }
}
