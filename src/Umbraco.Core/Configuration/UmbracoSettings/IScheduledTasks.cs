using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IScheduledTasks
    {
        IEnumerable<IScheduledTask> Tasks { get; }
    }
}