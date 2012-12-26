using System.Collections.Generic;

namespace Umbraco.Core.Persistence.DatabaseModelDefinitions
{
    public class ConstraintDefinition
    {
        public ConstraintDefinition(ConstraintType type)
        {
            constraintType = type;
        }

        private ConstraintType constraintType;
        public bool IsPrimaryKeyConstraint { get { return ConstraintType.PrimaryKey == constraintType; } }
        public bool IsUniqueConstraint { get { return ConstraintType.Unique == constraintType; } }
        public bool IsNonUniqueConstraint { get { return ConstraintType.NonUnique == constraintType; } }

        public string SchemaName { get; set; }
        public string ConstraintName { get; set; }
        public string TableName { get; set; }
        public ICollection<string> Columns = new HashSet<string>();
    }
}