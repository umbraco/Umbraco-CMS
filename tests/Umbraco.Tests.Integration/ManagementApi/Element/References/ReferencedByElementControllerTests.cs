// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element.References;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element.References;

public class ReferencedByElementControllerTests : ManagementApiUserGroupTestBase<ReferencedByElementController>
{
    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _elementKey;

    [SetUp]
    public async Task Setup()
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString())
            .WithName("Test Element")
            .WithIsElement(true)
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = null,
            Variants = [new VariantModel { Name = "Test Element Instance" }],
        };
        var response = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        _elementKey = response.Result!.Content!.Key;
    }

    protected override Expression<Func<ReferencedByElementController, object>> MethodSelector =>
        x => x.ReferencedBy(CancellationToken.None, _elementKey, 0, 20);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Unauthorized };
}
