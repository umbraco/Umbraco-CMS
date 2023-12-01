using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi;

[TestFixture]
public class ManagementApiBaseTest<T> : ManagementApiTest<T> where T : ManagementApiControllerBase
{
    protected override Expression<Func<T, object>> MethodSelector { get; }

    [Test]
    public virtual async Task Unauthorized_When_No_Token_Is_Provided()
    {
        var response = await SendUnauthenticatedRequest();

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}
