using System.Security.Claims;
using Umbraco.Cms.Core.ServerEvents;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.ServerEvents;


internal class FakeAuthorizer : IEventSourceAuthorizer
{
    private readonly Func<ClaimsPrincipal, string, bool> authorizeFunc;

    public FakeAuthorizer(IEnumerable<string> sources, Func<ClaimsPrincipal, string, bool>? authorizeFunc = null)
    {
        this.authorizeFunc = authorizeFunc ?? ((_, _) => true);
        AuthorizableEventSources = sources;
    }

    public IEnumerable<string> AuthorizableEventSources { get; }

    public Task<bool> AuthorizeAsync(ClaimsPrincipal principal, string connectionId) => Task.FromResult(authorizeFunc(principal, connectionId));
}
