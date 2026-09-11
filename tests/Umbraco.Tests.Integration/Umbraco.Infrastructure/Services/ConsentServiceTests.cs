// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
internal sealed class ConsentServiceTests : UmbracoIntegrationTest
{
    private IConsentService ConsentService => GetRequiredService<IConsentService>();

    [Test]
    public void CanCrudConsent()
    {
        // can register
        var consent =
            ConsentService.RegisterConsent("user/1234", "app1", "do-something", ConsentState.Granted, "no comment");
        Assert.That(consent.Id, Is.Not.EqualTo(0));

        Assert.That(consent.Current, Is.True);
        Assert.That(consent.Source, Is.EqualTo("user/1234"));
        Assert.That(consent.Context, Is.EqualTo("app1"));
        Assert.That(consent.Action, Is.EqualTo("do-something"));
        Assert.That(consent.State, Is.EqualTo(ConsentState.Granted));
        Assert.That(consent.Comment, Is.EqualTo("no comment"));

        Assert.That(consent.IsGranted(), Is.True);

        // can register more
        ConsentService.RegisterConsent("user/1234", "app1", "do-something-else", ConsentState.Granted, "no comment");
        ConsentService.RegisterConsent("user/1236", "app1", "do-something", ConsentState.Granted, "no comment");
        ConsentService.RegisterConsent("user/1237", "app2", "do-something", ConsentState.Granted, "no comment");

        // can get by source
        var consents = ConsentService.LookupConsent("user/1235").ToArray();
        Assert.That(consents, Is.Empty);

        consents = ConsentService.LookupConsent("user/1234").ToArray();
        Assert.That(consents, Has.Length.EqualTo(2));
        Assert.That(consents.All(x => x.Source == "user/1234"), Is.True);
        Assert.That(consents.Any(x => x.Action == "do-something"), Is.True);
        Assert.That(consents.Any(x => x.Action == "do-something-else"), Is.True);

        // can get by context
        consents = ConsentService.LookupConsent(context: "app3").ToArray();
        Assert.That(consents, Is.Empty);

        consents = ConsentService.LookupConsent(context: "app2").ToArray();
        Assert.That(consents, Has.Length.EqualTo(1));

        consents = ConsentService.LookupConsent(context: "app1").ToArray();
        Assert.That(consents, Has.Length.EqualTo(3));
        Assert.That(consents.Any(x => x.Action == "do-something"), Is.True);
        Assert.That(consents.Any(x => x.Action == "do-something-else"), Is.True);

        // can get by action
        consents = ConsentService.LookupConsent(action: "do-whatever").ToArray();
        Assert.That(consents, Is.Empty);

        consents = ConsentService.LookupConsent(context: "app1", action: "do-something").ToArray();
        Assert.That(consents, Has.Length.EqualTo(2));
        Assert.That(consents.All(x => x.Action == "do-something"), Is.True);
        Assert.That(consents.Any(x => x.Source == "user/1234"), Is.True);
        Assert.That(consents.Any(x => x.Source == "user/1236"), Is.True);

        // can revoke
        consent = ConsentService.RegisterConsent(
            "user/1234",
            "app1",
            "do-something",
            ConsentState.Revoked,
            "no comment");

        consents = ConsentService.LookupConsent("user/1234", "app1", "do-something").ToArray();
        Assert.That(consents, Has.Length.EqualTo(1));
        Assert.That(consents[0].Current, Is.True);
        Assert.That(consents[0].State, Is.EqualTo(ConsentState.Revoked));

        // can filter
        consents = ConsentService.LookupConsent(context: "app1", action: "do-", actionStartsWith: true).ToArray();
        Assert.That(consents, Has.Length.EqualTo(3));
        Assert.That(consents.All(x => x.Context == "app1"), Is.True);
        Assert.That(consents.All(x => x.Action.StartsWith("do-")), Is.True);

        // can get history
        consents = ConsentService.LookupConsent("user/1234", "app1", "do-something", includeHistory: true).ToArray();
        Assert.That(consents, Has.Length.EqualTo(1));
        Assert.That(consents[0].Current, Is.True);
        Assert.That(consents[0].State, Is.EqualTo(ConsentState.Revoked));
        Assert.That(consents[0].IsRevoked(), Is.True);
        Assert.That(consents[0].History, Is.Not.Null);
        var history = consents[0].History.ToArray();
        Assert.That(history, Has.Length.EqualTo(1));
        Assert.That(history[0].Current, Is.False);
        Assert.That(history[0].State, Is.EqualTo(ConsentState.Granted));

        // cannot be stupid
        Assert.Throws<ArgumentException>(() =>
            ConsentService.RegisterConsent(
                "user/1234",
                "app1",
                "do-something",
                ConsentState.Granted | ConsentState.Revoked,
                "no comment"));
    }

    [Test]
    public void CanRegisterConsentWithoutComment()
    {
        // Attept to add consent without a comment
        ConsentService.RegisterConsent("user/1234", "app1", "consentWithoutComment", ConsentState.Granted);

        // Attempt to retrieve the consent we just added without a comment
        var consents = ConsentService.LookupConsent("user/1234", action: "consentWithoutComment").ToArray();

        // Confirm we got our expected consent record
        Assert.That(consents, Has.Length.EqualTo(1));
    }
}
