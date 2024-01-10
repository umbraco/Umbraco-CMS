﻿using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Preview;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Preview;

public class EndPreviewTests : ManagementApiTest<EndPreviewController>
{
    protected override Expression<Func<EndPreviewController, object>> MethodSelector =>
        x => x.End();


    [Test]
    public virtual async Task As_Admin_I_Have_Access()
    {
        await AuthenticateClientAsync(Client, "admin@umbraco.com", "1234567890", true);

        var response = await Client.DeleteAsync(Url);

        // Check if the set cookie header is sent
        var doesHeaderExist = response.Headers.TryGetValues("Set-Cookie", out var setCookieValues) &&
                              setCookieValues.Any(value => value.Contains($"{Constants.Web.PreviewCookieName}=; expires"));

        Assert.IsTrue(doesHeaderExist);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,  await response.Content.ReadAsStringAsync());
    }

}
