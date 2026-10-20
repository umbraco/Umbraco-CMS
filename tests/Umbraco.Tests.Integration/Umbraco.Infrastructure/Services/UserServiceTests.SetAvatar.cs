// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

internal sealed partial class UserServiceTests
{
    private ITemporaryFileService TemporaryFileService => GetRequiredService<ITemporaryFileService>();

    public static void ConfigureImageFileTypesWithSvg(IUmbracoBuilder builder) =>
        builder.Services.Configure<ContentSettings>(config =>
            config.Imaging.ImageFileTypes = new HashSet<string> { "png", "svg" });

    [Test]
    public async Task Can_Set_Avatar_With_Allowed_Image_Type()
    {
        var user = UserService.CreateUserWithIdentity("avatarUser", "avatar@umbraco.io");
        var temporaryFileKey = Guid.NewGuid();
        await TemporaryFileService.CreateAsync(new CreateTemporaryFileModel { Key = temporaryFileKey, FileName = "avatar.png" });

        var result = await UserService.SetAvatarAsync(user.Key, temporaryFileKey);

        Assert.AreEqual(UserOperationStatus.Success, result);

        var updatedUser = await UserService.GetAsync(user.Key);
        Assert.IsNotNull(updatedUser?.Avatar);
    }

    [Test]
    public async Task Cannot_Set_Avatar_With_Extension_Not_In_Image_File_Types()
    {
        // "svg" is an allowed upload extension (see appsettings.Tests.json) but is not one of the default
        // image file types, so the file uploads successfully but is rejected as an avatar.
        var user = UserService.CreateUserWithIdentity("avatarUser", "avatar@umbraco.io");
        var temporaryFileKey = Guid.NewGuid();
        await TemporaryFileService.CreateAsync(new CreateTemporaryFileModel { Key = temporaryFileKey, FileName = "avatar.svg" });

        var result = await UserService.SetAvatarAsync(user.Key, temporaryFileKey);

        Assert.AreEqual(UserOperationStatus.InvalidAvatar, result);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureImageFileTypesWithSvg))]
    public async Task Can_Set_Avatar_With_Type_Added_To_Configured_Image_Types()
    {
        // "svg" is not a default image file type but has been added to configuration, so it is accepted.
        var user = UserService.CreateUserWithIdentity("avatarUser", "avatar@umbraco.io");
        var temporaryFileKey = Guid.NewGuid();
        await TemporaryFileService.CreateAsync(new CreateTemporaryFileModel { Key = temporaryFileKey, FileName = "avatar.svg" });

        var result = await UserService.SetAvatarAsync(user.Key, temporaryFileKey);

        Assert.AreEqual(UserOperationStatus.Success, result);
    }
}
