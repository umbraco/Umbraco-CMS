using System.Security.Claims;
using Umbraco.Cms.Core.ServerEvents;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.ServerEvents;


internal class FakeAuthorizer : IEventSourceAuthorizer
{
    private readonly Func<ClaimsPrincipal, string, bool> authorizeFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeAuthorizer"/> class.
    /// </summary>
    /// <param name="sources">The collection of sources to authorize against.</param>
    /// <param name="authorizeFunc">An optional function to determine authorization based on a ClaimsPrincipal and source string.</param>
    public FakeAuthorizer(IEnumerable<string> sources, Func<ClaimsPrincipal, string, bool>? authorizeFunc = null)
    {
        this.authorizeFunc = authorizeFunc ?? ((_, _) => true);
        AuthorizableEventSources = sources;
    }

    /// <summary>
    /// Gets the collection of event sources that can be authorized.
    /// </summary>
    public IEnumerable<string> AuthorizableEventSources { get; }

    /// <summary>
    /// Asynchronously authorizes a user based on the provided claims principal and connection ID.
    /// </summary>
    /// <param name="principal">The claims principal representing the user to authorize.</param>
    /// <param name="connectionId">The connection ID associated with the authorization request.</param>
    /// <returns>A task that represents the asynchronous authorization operation. The task result contains true if authorized; otherwise, false.</returns>
    public Task<bool> AuthorizeAsync(ClaimsPrincipal principal, string connectionId) => Task.FromResult(authorizeFunc(principal, connectionId));
}
