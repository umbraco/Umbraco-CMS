using System.Collections.Generic;

namespace Umbraco.Core.Persistence.DatabaseModelDefinitions
{
    public class ConstraintDefinition
    {
        public ConstraintDefinition(ConstraintType type)
        {
            _constraintType = type;
        }

        private readonly ConstraintType _constraintType;
        public bool IsPrimaryKeyConstraint => ConstraintType.PrimaryKey == _constraintType;
        public bool IsUniqueConstraint => ConstraintType.Unique == _constraintType;
        public bool IsNonUniqueConstraint => ConstraintType.NonUnique == _constraintType;

        public string SchemaName { get; set; }
        public string ConstraintName { get; set; }
        public string TableName { get; set; }
        public ICollection<string> Columns = new HashSet<string>();
        public bool IsPrimaryKeyClustered { get; set; }
    }
}
