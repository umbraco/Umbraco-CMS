using System.Linq.Expressions;
using System.Net;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.TestHelpers;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Policies;

public class UpdateDocumentTests : ManagementApiTest<UpdateDocumentController>
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IShortStringHelper ShortStringHelper => GetRequiredService<IShortStringHelper>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    protected override Expression<Func<UpdateDocumentController, object>> MethodSelector =>
        x => x.Update(CancellationToken.None, Guid.Empty, null!);

    [Test]
    public async Task UserWithoutPermissionCannotUpdate()
    {
        var userGroup = new UserGroup(ShortStringHelper);
        userGroup.Name = "Test";
        userGroup.Alias = "test";
        userGroup.Permissions = new HashSet<string> { ActionBrowse.ActionLetter };
        userGroup.HasAccessToAllLanguages = true;
        userGroup.StartContentId = -1;
        userGroup.StartMediaId = -1;
        userGroup.AddAllowedSection("content");
        userGroup.AddAllowedSection("media");

        var groupCreationResult = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(groupCreationResult.Success);

        await AuthenticateClientAsync(Client, async userService =>
        {
            var email = "test@test.com";
            var testUserCreateModel = new UserCreateModel
            {
                Email = email,
                Name = "Test Mc.Gee",
                UserName = email,
                UserGroupKeys = new HashSet<Guid> { groupCreationResult.Result.Key },
            };

            var userCreationResult =
                await userService.CreateAsync(Constants.Security.SuperUserKey, testUserCreateModel, true);

            Assert.IsTrue(userCreationResult.Success);

            return (userCreationResult.Result.CreatedUser, "1234567890");
        });

        var model = await CreateContent();
        var updateRequestModel = DocumentUpdateHelper.CreateInvariantDocumentUpdateRequestModel(model);
        var updatedName = "NewName";
        updateRequestModel.Variants.First().Name = updatedName;

        var url = GetManagementApiUrl<UpdateDocumentController>(x => x.Update(CancellationToken.None, model.Key!.Value, null));
        var requestBody = new StringContent(JsonSerializer.Serialize(updateRequestModel), Encoding.UTF8, "application/json");
        var response = await Client.PutAsync(url, requestBody);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var content = ContentService.GetById(model.Key!.Value);
        Assert.IsNotNull(content);
        Assert.That(content.Name, Is.Not.EqualTo(updatedName));
    }

    [Test]
    public async Task EditorCanUpdate()
    {
        // "Default" version creates an editor
        await AuthenticateClientAsync(Client, "editor@editor.com", "1234567890", false);

        var model = await CreateContent();
        var updateRequestModel = DocumentUpdateHelper.CreateInvariantDocumentUpdateRequestModel(model);
        var updatedName = "NewName";
        updateRequestModel.Variants.First().Name = updatedName;

        var url = GetManagementApiUrl<UpdateDocumentController>(x => x.Update(CancellationToken.None, model.Key!.Value, null));
        var requestBody = new StringContent(JsonSerializer.Serialize(updateRequestModel), Encoding.UTF8, "application/json");
        var response = await Client.PutAsync(url, requestBody);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = ContentService.GetById(model.Key!.Value);
        Assert.IsNotNull(content);
        Assert.That(content.Name, Is.EqualTo(updatedName));
    }

    private async Task<ContentCreateModel> CreateContent()
    {
        var userKey = Constants.Security.SuperUserKey;
        var template = TemplateBuilder.CreateTextPageTemplate();
        var templateAttempt = await TemplateService.CreateAsync(template, userKey);
        Assert.IsTrue(templateAttempt.Success);

        var contentTypeCreateModel = ContentTypeEditingBuilder.CreateSimpleContentType(defaultTemplateKey: template.Key);
        var contentTypeAttempt = await ContentTypeEditingService.CreateAsync(contentTypeCreateModel, userKey);
        Assert.IsTrue(contentTypeAttempt.Success);

        var textPage = ContentEditingBuilder.CreateSimpleContent(contentTypeAttempt.Result.Key);
        textPage.TemplateKey = templateAttempt.Result.Key;
        textPage.Key = Guid.NewGuid();
        var createContentResult = await ContentEditingService.CreateAsync(textPage, userKey);
        Assert.IsTrue(createContentResult.Success);

        var publishResult = await ContentPublishingService.PublishAsync(
            createContentResult.Result.Content!.Key,
            new List<CulturePublishScheduleModel> { new() { Culture = "*" } },
            userKey);

        Assert.IsTrue(publishResult.Success);
        return textPage;
    }
}
