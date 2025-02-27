using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.Filters;

internal class UserPasswordEnsureMinimumResponseTimeAttribute : TypeFilterAttribute
{
    public UserPasswordEnsureMinimumResponseTimeAttribute()
        : base(typeof(UserPasswordEnsureMinimumResponseTimeFilter))
    {
    }

    private class UserPasswordEnsureMinimumResponseTimeFilter : EnsureMinimumResponseTimeFilter
    {
        public UserPasswordEnsureMinimumResponseTimeFilter(IOptions<UserPasswordConfigurationSettings> options)
            : base(options.Value.MinimumResponseTime)
        {
        }
    }
}
