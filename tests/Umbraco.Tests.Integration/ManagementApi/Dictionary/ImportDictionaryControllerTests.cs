using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Dictionary;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Dictionary;

public class ImportDictionaryControllerTests : ManagementApiUserGroupTestBase<ImportDictionaryController>
{
    private ITemporaryFileService TemporaryFileService => GetRequiredService<ITemporaryFileService>();

    private Guid _temporaryFileKey;

    // We need to override the setup to add the file extension for dictionaries
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.Services.Configure<ContentSettings>(options =>
        {
            options.AllowedUploadedFileExtensions = new HashSet<string> { "udt" };
        });
    }

    [SetUp]
    public async Task Setup()
    {
        var createTempFileModel = new CreateTemporaryFileModel
        {
            FileName = "test.udt",
            Key = Guid.NewGuid(),
            OpenReadStream = () =>
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write("<DictionaryItem Key=\"0115a059-4668-49d6-b1a9-7eef8b483fe0\" Name=\"test\" />");
                writer.Flush();
                stream.Position = 0;
                return stream;
            },
        };

        var response = await TemporaryFileService.CreateAsync(createTempFileModel);
        _temporaryFileKey = response.Result.Key;
    }

    protected override Expression<Func<ImportDictionaryController, object>> MethodSelector =>
        x => x.Import(CancellationToken.None, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Created
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Created
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        ImportDictionaryRequestModel importDictionaryRequestModel =
            new() { TemporaryFile = new ReferenceByIdModel(_temporaryFileKey) };
        return await Client.PostAsync(Url, JsonContent.Create(importDictionaryRequestModel));
    }
}
