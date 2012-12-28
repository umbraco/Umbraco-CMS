using System;

namespace Umbraco.Core.Persistence.Migrations
{
    /// <summary>
    /// Represents the Migration attribute, which is used to mark classes as
    /// database migrations with Up/Down methods for pushing changes UP or pulling them DOWN.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MigrationAttribute : Attribute
    {
        public MigrationAttribute(string targetVersion, int sortOrder, string product)
        {
            TargetVersion = new Version(targetVersion);
            SortOrder = sortOrder;
            ProductName = product;
        }

        /// <summary>
        /// Gets or sets the target version of this migration.
        /// </summary>
        public Version TargetVersion { get; private set; }

        /// <summary>
        /// Gets or sets the sort order, which is the order this migration will be run in.
        /// </summary>
        public int SortOrder { get; private set; }

        /// <summary>
        /// Gets or sets the name of the product, which this migration belongs to.
        /// </summary>
        public string ProductName { get; set; }
    }
}