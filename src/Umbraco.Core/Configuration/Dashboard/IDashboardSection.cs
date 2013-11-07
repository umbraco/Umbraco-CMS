using System.Collections.Generic;

namespace Umbraco.Core.Configuration.Dashboard
{
    public interface IDashboardSection
    {
        IEnumerable<ISection> Sections { get; }
    }
}