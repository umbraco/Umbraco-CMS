using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IDistributedCall
    {
        bool Enabled { get; }

        int UserId { get; }

        IEnumerable<IServerElement> Servers { get; }
    }
}