using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.Filters;

internal sealed class UserPasswordEnsureMinimumResponseTimeAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserPasswordEnsureMinimumResponseTimeAttribute"/> class, which enforces a minimum response time for user password operations to mitigate timing attacks.
    /// </summary>
    public UserPasswordEnsureMinimumResponseTimeAttribute()
        : base(typeof(UserPasswordEnsureMinimumResponseTimeFilter))
    {
    }

    private sealed class UserPasswordEnsureMinimumResponseTimeFilter : EnsureMinimumResponseTimeFilter
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="UserPasswordEnsureMinimumResponseTimeFilter"/> class with the specified user password configuration settings.
    /// </summary>
    /// <param name="options">The options containing user password configuration settings.</param>
        public UserPasswordEnsureMinimumResponseTimeFilter(IOptions<UserPasswordConfigurationSettings> options)
            : base(options.Value.MinimumResponseTime)
        {
        }
    }
}
