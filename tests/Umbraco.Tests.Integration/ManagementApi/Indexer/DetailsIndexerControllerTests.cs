using System.Linq.Expressions;
using System.Net;
using Examine;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Indexer;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Indexer;

[TestFixture]
public class DetailsIndexerControllerTests : ManagementApiUserGroupTestBase<DetailsIndexerController>
{
    private string _indexName;

    protected override Expression<Func<DetailsIndexerController, object>> MethodSelector =>
        x => x.Details(_indexName);

    [SetUp]
    public void Setup()
    {
        
    }

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
}
