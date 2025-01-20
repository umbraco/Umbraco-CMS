using System.Security.Claims;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.ServerEvents;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ServerEvents;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.ServerEvents;

[TestFixture]
public class ServerEventAuthorizerTests
{
    [Test]
    public async Task CanAuthorize()
    {
        var sourceName = "Authorized source";
        var authorizer = new FakeAuthorizer([sourceName]);

        var sut = new ServerEventAuthorizationService(CreateServeEventAuthorizerCollection(authorizer));
        var result = await sut.AuthorizeAsync(CreateFakeUser());

        Assert.That(result.AuthorizedEventSources.Count(), Is.EqualTo(1));
        Assert.That(result.UnauthorizedEventSources.Count(), Is.EqualTo(0));
        Assert.That(result.AuthorizedEventSources.First(), Is.EqualTo(sourceName));
    }

    [Test]
    public async Task CanUnauthorize()
    {
        var sourceName = "unauthorized source";
        var authorizer = new FakeAuthorizer([sourceName], (_, _) => false);

        var sut = new ServerEventAuthorizationService(CreateServeEventAuthorizerCollection(authorizer));
        var result = await sut.AuthorizeAsync(CreateFakeUser());

        Assert.That(result.AuthorizedEventSources.Count(), Is.EqualTo(0));
        Assert.That(result.UnauthorizedEventSources.Count(), Is.EqualTo(1));
        Assert.That(result.UnauthorizedEventSources.First(), Is.EqualTo(sourceName));
    }

    [Test]
    public async Task AnyAuthorizationFailureResultInUnauthorized()
    {
        var sourceName = "Unauthorized source";

        FakeAuthorizer[] authorizers = [new([sourceName]), new([sourceName], (_, _) => false), new([sourceName])];
        var sut = new ServerEventAuthorizationService(CreateServeEventAuthorizerCollection(authorizers));
        var result = await sut.AuthorizeAsync(CreateFakeUser());

        Assert.That(result.AuthorizedEventSources.Count(), Is.EqualTo(0));
        Assert.That(result.UnauthorizedEventSources.Count(), Is.EqualTo(1));
        Assert.That(result.UnauthorizedEventSources.First(), Is.EqualTo(sourceName));
    }

    [Test]
    public async Task CanHandleMultiple()
    {
        string[] authorizedSources = ["first auth", "second auth", "third auth"];
        string[] unauthorizedSources = ["first unauth", "second unauth", "third unauth"];

        FakeAuthorizer[] authorizers = [
            new(authorizedSources),
            new(unauthorizedSources, (_, _) => false)
        ];

        var sut = new ServerEventAuthorizationService(CreateServeEventAuthorizerCollection(authorizers));
        var result = await sut.AuthorizeAsync(CreateFakeUser());

        Assert.That(result.AuthorizedEventSources, Is.EquivalentTo(authorizedSources));
        Assert.That(result.UnauthorizedEventSources, Is.EquivalentTo(unauthorizedSources));
    }

    [Test]
    public async Task CanHandleMultipleAuthorizers()
    {
        string[] authorizedSources = ["first auth", "second auth", "third auth"];
        string[] unauthorizedSources = ["first unauth", "second unauth", "third unauth"];

        FakeAuthorizer[] authorizers = [
            new([authorizedSources[0]]),
            new([authorizedSources[1]]),
            new([unauthorizedSources[0]], (_, _) => false),
            new([unauthorizedSources[1]], (_, _) => false),
            new([authorizedSources[2], unauthorizedSources[2]], (_, source) => source == authorizedSources[2]),
            new(authorizedSources)
        ];

        var sut = new ServerEventAuthorizationService(CreateServeEventAuthorizerCollection(authorizers));
        var result = await sut.AuthorizeAsync(CreateFakeUser());

        Assert.That(result.AuthorizedEventSources, Is.EquivalentTo(authorizedSources));
        Assert.That(result.UnauthorizedEventSources, Is.EquivalentTo(unauthorizedSources));
    }

    private ClaimsPrincipal CreateFakeUser(Guid? key = null) =>
        new(new ClaimsIdentity([

            // This is the claim that's used to store the ID
            new Claim(Constants.Security.OpenIdDictSubClaimType, key is null ? Guid.NewGuid().ToString() : key.ToString())
        ]));

    private EventSourceAuthorizerCollection CreateServeEventAuthorizerCollection(
        params IEnumerable<IEventSourceAuthorizer> authorizers)
        => new(() => authorizers);
}
