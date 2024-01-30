﻿using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Preview;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Preview;

public class EnterPreviewTests : ManagementApiTest<EnterPreviewController>
{
    protected override Expression<Func<EnterPreviewController, object>> MethodSelector =>
        x => x.Enter();


    [Test]
    public virtual async Task As_Admin_I_Have_Access()
    {
        await AuthenticateClientAsync(Client, "admin@umbraco.com", "1234567890", true);

        var response = await Client.PostAsync(Url, null);

        // Check if the set cookie header is sent
        var doesHeaderExist = response.Headers.TryGetValues("Set-Cookie", out var setCookieValues) &&
            setCookieValues.Any(value => value.Contains($"{Constants.Web.PreviewCookieName}=preview; path=/"));

        Assert.IsTrue(doesHeaderExist);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,  await response.Content.ReadAsStringAsync());
    }

}
