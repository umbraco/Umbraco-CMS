using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IDataSection : IUmbracoConfigurationSection
    {
        SQLRetryPolicyBehaviour SQLRetryPolicyBehaviour { get; }
    }
}