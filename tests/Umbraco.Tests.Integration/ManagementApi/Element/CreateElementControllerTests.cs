using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element;

public class CreateElementControllerTests : ManagementApiUserGroupTestBase<CreateElementController>
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _elementTypeKey;

    [SetUp]
    public async Task Setup()
    {
        // Create element type
        var elementType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString())
            .WithName("Test Element")
            .WithIsElement(true)
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        _elementTypeKey = elementType.Key;
    }

    protected override Expression<Func<CreateElementController, object>> MethodSelector =>
        x => x.Create(CancellationToken.None, null!);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Created };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Created };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Created };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Unauthorized };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        var createElementRequestModel = new CreateElementRequestModel
        {
            DocumentType = new ReferenceByIdModel(_elementTypeKey),
            Parent = null,
            Id = Guid.NewGuid(),
            Values = [],
            Variants =
            [
                new ElementVariantRequestModel { Culture = null, Segment = null, Name = "Test Element Instance" }
            ],
        };

        return await Client.PostAsync(Url, JsonContent.Create(createElementRequestModel));
    }
}
