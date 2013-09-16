using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IRequestHandler
    {
        bool UseDomainPrefixes { get; }

        bool AddTrailingSlash { get; }

        bool RemoveDoubleDashes { get; }

        IEnumerable<IChar> CharCollection { get; }
    }
}