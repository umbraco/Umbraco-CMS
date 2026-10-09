using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed partial class MediaEditingServiceTests : UmbracoIntegrationTest
{
    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    private IRelationService RelationService => GetRequiredService<IRelationService>();

    private IUserService UserService => GetRequiredService<IUserService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IAuditService AuditService => GetRequiredService<IAuditService>();

    private Task<IMedia> CreateFolderMediaAsync(string name, Guid? parentKey = null)
        => CreateFolderMediaAsync(name, Constants.Security.SuperUserKey, parentKey);

    private async Task<IMedia> CreateFolderMediaAsync(string name, Guid userKey, Guid? parentKey = null)
    {
        var folderMediaType = MediaTypeService.Get(Constants.Conventions.MediaTypes.Folder);
        var createModel = new MediaCreateModel
        {
            ContentTypeKey = folderMediaType!.Key,
            ParentKey = parentKey ?? Constants.System.RootKey,
            Key = Guid.NewGuid(),
            Variants = [new VariantModel { Name = name }],
        };

        var result = await MediaEditingService.CreateAsync(createModel, userKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
    }

    private async Task<IUser> CreateAdminUserAsync(string identifier)
    {
        var adminGroup = await UserGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        var createModel = new UserCreateModel
        {
            UserName = $"{identifier}@example.com",
            Email = $"{identifier}@example.com",
            Name = identifier,
            UserGroupKeys = new HashSet<Guid> { adminGroup!.Key },
        };

        var result = await UserService.CreateAsync(Constants.Security.SuperUserKey, createModel);
        Assert.IsTrue(result.Success);
        return result.Result.CreatedUser!;
    }

    private void Relate(IMedia parent, IMedia child)
    {
        var relationType = RelationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelatedMediaAlias);
        var relation = RelationService.Relate(parent.Id, child.Id, relationType);
        RelationService.Save(relation);
    }
}
