// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
internal sealed class ConsentServiceTests : UmbracoIntegrationTest
{
    private IConsentService ConsentService => GetRequiredService<IConsentService>();

    [Test]
    public async Task CanCrudConsent()
    {
        // can register
        var consent =
            await ConsentService.RegisterConsentAsync("user/1234", "app1", "do-something", ConsentState.Granted, "no comment");
        Assert.AreNotEqual(0, consent.Id);

        Assert.IsTrue(consent.Current);
        Assert.AreEqual("user/1234", consent.Source);
        Assert.AreEqual("app1", consent.Context);
        Assert.AreEqual("do-something", consent.Action);
        Assert.AreEqual(ConsentState.Granted, consent.State);
        Assert.AreEqual("no comment", consent.Comment);

        Assert.IsTrue(consent.IsGranted());

        // can register more
        await ConsentService.RegisterConsentAsync("user/1234", "app1", "do-something-else", ConsentState.Granted, "no comment");
        await ConsentService.RegisterConsentAsync("user/1236", "app1", "do-something", ConsentState.Granted, "no comment");
        await ConsentService.RegisterConsentAsync("user/1237", "app2", "do-something", ConsentState.Granted, "no comment");

        // can get by source
        var consents = (await ConsentService.LookupConsentAsync("user/1235")).ToArray();
        Assert.IsEmpty(consents);

        consents = (await ConsentService.LookupConsentAsync("user/1234")).ToArray();
        Assert.AreEqual(2, consents.Length);
        Assert.IsTrue(consents.All(x => x.Source == "user/1234"));
        Assert.IsTrue(consents.Any(x => x.Action == "do-something"));
        Assert.IsTrue(consents.Any(x => x.Action == "do-something-else"));

        // can get by context
        consents = (await ConsentService.LookupConsentAsync(context: "app3")).ToArray();
        Assert.IsEmpty(consents);

        consents = (await ConsentService.LookupConsentAsync(context: "app2")).ToArray();
        Assert.AreEqual(1, consents.Length);

        consents = (await ConsentService.LookupConsentAsync(context: "app1")).ToArray();
        Assert.AreEqual(3, consents.Length);
        Assert.IsTrue(consents.Any(x => x.Action == "do-something"));
        Assert.IsTrue(consents.Any(x => x.Action == "do-something-else"));

        // can get by action
        consents = (await ConsentService.LookupConsentAsync(action: "do-whatever")).ToArray();
        Assert.IsEmpty(consents);

        consents = (await ConsentService.LookupConsentAsync(context: "app1", action: "do-something")).ToArray();
        Assert.AreEqual(2, consents.Length);
        Assert.IsTrue(consents.All(x => x.Action == "do-something"));
        Assert.IsTrue(consents.Any(x => x.Source == "user/1234"));
        Assert.IsTrue(consents.Any(x => x.Source == "user/1236"));

        // can revoke
        consent = await ConsentService.RegisterConsentAsync(
            "user/1234",
            "app1",
            "do-something",
            ConsentState.Revoked,
            "no comment");

        consents = (await ConsentService.LookupConsentAsync("user/1234", "app1", "do-something")).ToArray();
        Assert.AreEqual(1, consents.Length);
        Assert.IsTrue(consents[0].Current);
        Assert.AreEqual(ConsentState.Revoked, consents[0].State);

        // can filter
        consents = (await ConsentService.LookupConsentAsync(context: "app1", action: "do-", actionStartsWith: true)).ToArray();
        Assert.AreEqual(3, consents.Length);
        Assert.IsTrue(consents.All(x => x.Context == "app1"));
        Assert.IsTrue(consents.All(x => x.Action.StartsWith("do-")));

        // can get history
        consents = (await ConsentService.LookupConsentAsync("user/1234", "app1", "do-something", includeHistory: true)).ToArray();
        Assert.AreEqual(1, consents.Length);
        Assert.IsTrue(consents[0].Current);
        Assert.AreEqual(ConsentState.Revoked, consents[0].State);
        Assert.IsTrue(consents[0].IsRevoked());
        Assert.IsNotNull(consents[0].History);
        var history = consents[0].History.ToArray();
        Assert.AreEqual(1, history.Length);
        Assert.IsFalse(history[0].Current);
        Assert.AreEqual(ConsentState.Granted, history[0].State);

        // cannot be stupid
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await ConsentService.RegisterConsentAsync(
                "user/1234",
                "app1",
                "do-something",
                ConsentState.Granted | ConsentState.Revoked,
                "no comment"));
    }

    [Test]
    public async Task CanRegisterConsentWithoutComment()
    {
        // Attept to add consent without a comment
        await ConsentService.RegisterConsentAsync("user/1234", "app1", "consentWithoutComment", ConsentState.Granted);

        // Attempt to retrieve the consent we just added without a comment
        var consents = (await ConsentService.LookupConsentAsync("user/1234", action: "consentWithoutComment")).ToArray();

        // Confirm we got our expected consent record
        Assert.AreEqual(1, consents.Length);
    }
}
