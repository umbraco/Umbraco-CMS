// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using UmbracoDataType = Umbraco.Cms.Core.Models.DataType;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     Integration tests for <see cref="IMediaPermissionService.FilterAuthorizedAccessAsync(IUser, IEnumerable{Guid}, Guid?)"/>.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaPermissionServiceTests : UmbracoIntegrationTest
{
    private IMediaPermissionService MediaPermissionService => GetRequiredService<IMediaPermissionService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IMedia _accessibleMedia = null!;

    private IMedia _inaccessibleMedia = null!;

    private IUser _user = null!;

    [SetUp]
    public async Task SetUpMediaAndUser()
    {
        IMedia deniedRoot = CreateFolder(Constants.System.Root);
        IMedia allowedRoot = CreateFolder(Constants.System.Root);
        _inaccessibleMedia = CreateFolder(deniedRoot.Id);
        _accessibleMedia = CreateFolder(allowedRoot.Id);

        _user = await CreateUserWithStartMediaAsync(allowedRoot.Id);
    }

    [Test]
    public async Task FilterAuthorizedAccessAsync_Excludes_Media_Outside_User_Start_Nodes()
    {
        ISet<Guid> result = await MediaPermissionService.FilterAuthorizedAccessAsync(_user, [_accessibleMedia.Key, _inaccessibleMedia.Key], null);

        Assert.That(result, Is.EquivalentTo(new[] { _accessibleMedia.Key }));
    }

    [Test]
    public async Task FilterAuthorizedAccessAsync_Includes_Media_Outside_User_Start_Nodes_When_Data_Type_Ignores_Start_Nodes()
    {
        Guid dataTypeKey = await CreateMediaPickerDataTypeAsync(ignoreUserStartNodes: true);

        ISet<Guid> result = await MediaPermissionService.FilterAuthorizedAccessAsync(_user, [_accessibleMedia.Key, _inaccessibleMedia.Key], dataTypeKey);

        Assert.That(result, Is.EquivalentTo(new[] { _accessibleMedia.Key, _inaccessibleMedia.Key }));
    }

    [Test]
    public async Task FilterAuthorizedAccessAsync_Excludes_Media_Outside_User_Start_Nodes_When_Data_Type_Honours_Start_Nodes()
    {
        Guid dataTypeKey = await CreateMediaPickerDataTypeAsync(ignoreUserStartNodes: false);

        ISet<Guid> result = await MediaPermissionService.FilterAuthorizedAccessAsync(_user, [_accessibleMedia.Key, _inaccessibleMedia.Key], dataTypeKey);

        Assert.That(result, Is.EquivalentTo(new[] { _accessibleMedia.Key }));
    }

    [Test]
    public async Task FilterAuthorizedAccessAsync_Excludes_Media_Outside_User_Start_Nodes_When_Data_Type_Does_Not_Exist()
    {
        ISet<Guid> result = await MediaPermissionService.FilterAuthorizedAccessAsync(_user, [_accessibleMedia.Key, _inaccessibleMedia.Key], Guid.NewGuid());

        Assert.That(result, Is.EquivalentTo(new[] { _accessibleMedia.Key }));
    }

    private IMedia CreateFolder(int parentId)
        => MediaService.CreateMediaWithIdentity(Guid.NewGuid().ToString(), parentId, Constants.Conventions.MediaTypes.Folder);

    private async Task<IUser> CreateUserWithStartMediaAsync(int startMediaId)
    {
        IUserGroup userGroup = new UserGroupBuilder()
            .WithAlias("restrictedMedia")
            .WithName("Restricted media")
            .WithStartMediaId(startMediaId)
            .Build();
        Attempt<IUserGroup, UserGroupOperationStatus> userGroupAttempt = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(userGroupAttempt.Success, $"Could not create the user group: {userGroupAttempt.Status}");

        IUser user = UserService.CreateUserWithIdentity("restricted", "restricted@test.com");
        user.AddGroup(userGroup.ToReadOnlyGroup());
        UserService.Save(user);

        return user;
    }

    private async Task<Guid> CreateMediaPickerDataTypeAsync(bool ignoreUserStartNodes)
    {
        var dataType = new UmbracoDataType(
            GetRequiredService<PropertyEditorCollection>()[Constants.PropertyEditors.Aliases.MediaPicker3],
            GetRequiredService<IConfigurationEditorJsonSerializer>())
        {
            ConfigurationData = new Dictionary<string, object>
            {
                { Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes, ignoreUserStartNodes },
            },
            Name = Guid.NewGuid().ToString(),
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
        };

        Attempt<IDataType, DataTypeOperationStatus> attempt = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.IsTrue(attempt.Success, $"Could not create the data type: {attempt.Status}");

        return attempt.Result.Key;
    }
}
