using System.Linq.Expressions;
using System.Net;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document;
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

//public class UpdateDocumentTests : ManagementApiTest<UpdateDocumentController>
//{
//    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

//    private IShortStringHelper ShortStringHelper => GetRequiredService<IShortStringHelper>();

//    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

//    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

//    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

//    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

//    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

//    private IContentService ContentService => GetRequiredService<IContentService>();

//    protected override Expression<Func<UpdateDocumentController, object>> MethodSelector =>
//        x => x.Update(CancellationToken.None, Guid.Empty, null!);

//    [Test]
//    public async Task UserWithoutPermissionCannotUpdate()
//    {
//        var userGroup = new UserGroup(ShortStringHelper)
//        {
//            Name = "Test",
//            Alias = "test",
//            Permissions = new HashSet<string> { ActionBrowse.ActionLetter },
//            HasAccessToAllLanguages = true,
//            StartContentId = -1,
//            StartMediaId = -1
//        };
//        userGroup.AddAllowedSection("content");
//        userGroup.AddAllowedSection("media");

//        var groupCreationResult = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
//        Assert.IsTrue(groupCreationResult.Success);

//        await AuthenticateClientAsync(Client, async userService =>
//        {
//            var email = "test@test.com";
//            var testUserCreateModel = new UserCreateModel
//            {
//                Email = email,
//                Name = "Test Mc.Gee",
//                UserName = email,
//                UserGroupKeys = new HashSet<Guid> { groupCreationResult.Result.Key },
//            };

//            var userCreationResult =
//                await userService.CreateAsync(Constants.Security.SuperUserKey, testUserCreateModel, true);

//            Assert.IsTrue(userCreationResult.Success);

//            return (userCreationResult.Result.CreatedUser, "1234567890");
//        });

//        const string UpdatedName = "NewName";

//        var model = await CreateContent();
//        var updateRequestModel = CreateRequestModel(model, UpdatedName);

//        var response = await GetManagementApiResponse(model, updateRequestModel);

//        AssertResponse(response, model, HttpStatusCode.Forbidden, model.InvariantName);
//    }

//    [Test]
//    public async Task UserWithPermissionCanUpdate()
//    {
//        // "Default" version creates an editor that has permission to update content.
//        await AuthenticateClientAsync(Client, "editor@editor.com", "1234567890", false);

//        const string UpdatedName = "NewName";

//        var model = await CreateContent();
//        var updateRequestModel = CreateRequestModel(model, UpdatedName);

//        var response = await GetManagementApiResponse(model, updateRequestModel);

//        AssertResponse(response, model, HttpStatusCode.OK, UpdatedName);
//    }

//    private async Task<ContentCreateModel> CreateContent()
//    {
//        var userKey = Constants.Security.SuperUserKey;
//        var template = TemplateBuilder.CreateTextPageTemplate();
//        var templateAttempt = await TemplateService.CreateAsync(template, userKey);
//        Assert.IsTrue(templateAttempt.Success);

//        var contentTypeCreateModel = ContentTypeEditingBuilder.CreateSimpleContentType(defaultTemplateKey: template.Key);
//        var contentTypeAttempt = await ContentTypeEditingService.CreateAsync(contentTypeCreateModel, userKey);
//        Assert.IsTrue(contentTypeAttempt.Success);

//        var textPage = ContentEditingBuilder.CreateSimpleContent(contentTypeAttempt.Result.Key);
//        textPage.TemplateKey = templateAttempt.Result.Key;
//        textPage.Key = Guid.NewGuid();
//        var createContentResult = await ContentEditingService.CreateAsync(textPage, userKey);
//        Assert.IsTrue(createContentResult.Success);

//        var publishResult = await ContentPublishingService.PublishAsync(
//            createContentResult.Result.Content!.Key,
//            [new() { Culture = "*" }],
//            userKey);

//        Assert.IsTrue(publishResult.Success);
//        return textPage;
//    }

//    private static UpdateDocumentRequestModel CreateRequestModel(ContentCreateModel model, string name)
//    {
//        var updateRequestModel = DocumentUpdateHelper.CreateInvariantDocumentUpdateRequestModel(model);
//        updateRequestModel.Variants.First().Name = name;
//        return updateRequestModel;
//    }

//    private async Task<HttpResponseMessage> GetManagementApiResponse(ContentCreateModel model, UpdateDocumentRequestModel updateRequestModel)
//    {
//        var url = GetManagementApiUrl<UpdateDocumentController>(x => x.Update(CancellationToken.None, model.Key!.Value, null));
//        var requestBody = new StringContent(JsonSerializer.Serialize(updateRequestModel), Encoding.UTF8, "application/json");
//        return await Client.PutAsync(url, requestBody);
//    }

//    private void AssertResponse(HttpResponseMessage response, ContentCreateModel model, HttpStatusCode expectedStatusCode, string expectedContentName)
//    {
//        Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode));
//        var content = ContentService.GetById(model.Key!.Value);
//        Assert.IsNotNull(content);
//        Assert.That(content.Name, Is.EqualTo(expectedContentName));
//    }
//}
