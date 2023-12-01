using System.Net;

namespace Umbraco.Cms.Tests.Integration.ManagementApi;

public class UserGroupAssertionModel
{
    public bool Allowed { get; set; }

    public HttpStatusCode ExpectedStatusCode { get; set; }
}
