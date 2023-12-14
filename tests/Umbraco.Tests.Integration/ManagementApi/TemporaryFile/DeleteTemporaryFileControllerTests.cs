﻿using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.TemporaryFile;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.TemporaryFile;

[TestFixture]
public class DeleteTemporaryFileControllerTests : ManagementApiUserGroupTestBase<DeleteTemporaryFileController>
{
    private ITemporaryFileService _temporaryFileService;
    private Guid _key;

    [SetUp]
    public async Task Setup()
    {
        _temporaryFileService = GetRequiredService<ITemporaryFileService>();
        _key = Guid.NewGuid();
        await _temporaryFileService.CreateAsync(new CreateTemporaryFileModel { Key = _key, FileName = "File.png" });
    }

    protected override Expression<Func<DeleteTemporaryFileController, object>> MethodSelector =>
        x => x.Delete(_key);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest() => await Client.DeleteAsync(Url);
}
