using System.Linq.Expressions;
using System.Net;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.TestHelpers;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Policies;

[TestFixture]
public class CreateDocumentTests : ManagementApiTest<CreateDocumentController>
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IShortStringHelper ShortStringHelper => GetRequiredService<IShortStringHelper>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    protected override Expression<Func<CreateDocumentController, object>> MethodSelector =>
        x => x.Create(CancellationToken.None, null!);

    [Test]
    public async Task ReadonlyUserCannotCreateDocument()
    {
        var userGroup = await CreateReadonlyUserGroupAsync();

        await AuthenticateClientAsync(Client, async userService =>
        {
            var email = "test@test.com";
            var testUserCreateModel = new UserCreateModel
            {
                Email = email,
                Name = "Test Mc.Gee",
                UserName = email,
                UserGroupKeys = new HashSet<Guid> { userGroup.Key },
            };

            var userCreationResult =
                await userService.CreateAsync(Constants.Security.SuperUserKey, testUserCreateModel, true);

            Assert.IsTrue(userCreationResult.Success);

            return (userCreationResult.Result.CreatedUser, "1234567890");
        });

        var (contentType, template) = await CreateDocumentTypeAsync();
        var contentCreateModel = ContentEditingBuilder.CreateSimpleContent(contentType.Key);

        var requestModel = DocumentUpdateHelper.CreateDocumentRequestModel(contentCreateModel);

        var response = await GetManagementApiResponseAsync(requestModel);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task EditorCanCreateDocument()
    {
        await AuthenticateClientAsync(Client, "editor@editor.com", "1234567890", false);

        var (contentType, template) = await CreateDocumentTypeAsync();
        var requestModel = DocumentUpdateHelper.CreateDocumentRequestModel(ContentEditingBuilder.CreateSimpleContent(contentType.Key));

        var response = await GetManagementApiResponseAsync(requestModel);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var locationHeader = response.Headers.GetValues("Location").First();
        var key = Guid.Parse(locationHeader.Split('/')[^1]);
        var createdContent = ContentService.GetById(key);
        Assert.NotNull(createdContent);
    }

    private async Task<HttpResponseMessage> GetManagementApiResponseAsync(CreateDocumentRequestModel requestModel)
    {
        var url = GetManagementApiUrl<CreateDocumentController>(x => x.Create(CancellationToken.None, requestModel));
        var requestBody = new StringContent(JsonSerializer.Serialize(requestModel), Encoding.UTF8, "application/json");
        var response = await Client.PostAsync(url, requestBody);
        return response;
    }

    private async Task<IUserGroup> CreateReadonlyUserGroupAsync()
    {
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = "Test",
            Alias = "test",
            Permissions = new HashSet<string> { ActionBrowse.ActionLetter },
            HasAccessToAllLanguages = true,
            StartContentId = -1,
            StartMediaId = -1
        };
        userGroup.AddAllowedSection("content");
        userGroup.AddAllowedSection("media");

        var groupCreationResult = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(groupCreationResult.Success);
        return groupCreationResult.Result;
    }

    private async Task<(IContentType contentType, ITemplate template)> CreateDocumentTypeAsync()
    {
        var userKey = Constants.Security.SuperUserKey;
        var template = TemplateBuilder.CreateTextPageTemplate();
        var templateAttempt = await TemplateService.CreateAsync(template, userKey);
        Assert.IsTrue(templateAttempt.Success);

        var contentTypeCreateModel = ContentTypeEditingBuilder.CreateSimpleContentType(defaultTemplateKey: template.Key);
        var contentTypeAttempt = await ContentTypeEditingService.CreateAsync(contentTypeCreateModel, userKey);
        Assert.IsTrue(contentTypeAttempt.Success);

        return (contentTypeAttempt.Result!, templateAttempt.Result!);
    }
}
