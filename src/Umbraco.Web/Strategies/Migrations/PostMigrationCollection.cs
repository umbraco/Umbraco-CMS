using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Strategies.Migrations
{
    public class PostMigrationCollection : BuilderCollectionBase<IPostMigration>
    {
        public PostMigrationCollection(IEnumerable<IPostMigration> items)
            : base(items)
        { }
    }
}
