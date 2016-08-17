using System.Collections.Generic;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Persistence.Migrations
{
    public interface IMigrationContext
    {
        UmbracoDatabase Database { get; }

        ICollection<IMigrationExpression> Expressions { get; set; }

        ILogger Logger { get; }
    }
}