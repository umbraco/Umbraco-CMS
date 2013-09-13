using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IUrlReplacing
    {
        bool RemoveDoubleDashes { get; }

        IEnumerable<IChar> CharCollection { get; }
    }
}