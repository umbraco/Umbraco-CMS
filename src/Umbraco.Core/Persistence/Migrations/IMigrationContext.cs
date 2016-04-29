using System.Collections.Generic;

namespace Umbraco.Core.Persistence.Migrations
{
    public interface IMigrationContext
    {
        UmbracoDatabase Database { get; }

        ICollection<IMigrationExpression> Expressions { get; set; }
    }
}