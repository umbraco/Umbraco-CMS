using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Provides authorization logic for webhook events within server event management in Umbraco CMS.
/// </summary>
public class WebhookEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookEventAuthorizer"/> class with the specified authorization service.
    /// </summary>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> used to authorize webhook events.</param>
    public WebhookEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event sources that can be authorized by the webhook event authorizer.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.Webhook];

    protected override string Policy => AuthorizationPolicies.TreeAccessWebhooks;
}
