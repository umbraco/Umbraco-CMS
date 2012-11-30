using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Umbraco.Core.Persistence.Migrations
{
    internal class MigrationContext : IMigrationContext
    {
        public MigrationContext()
        {
            Expressions = new Collection<IMigrationExpression>();
        }

        public virtual ICollection<IMigrationExpression> Expressions { get; set; }
    }

    public interface IMigrationContext
    {
        ICollection<IMigrationExpression> Expressions { get; set; }
    }

    /// <summary>
    /// Marker interface for migration expressions
    /// </summary>
    public interface IMigrationExpression
    {}
}