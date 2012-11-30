using System.Data;

namespace Umbraco.Core.Persistence.Migrations.Model
{
    public class ColumnDefinition
    {
        public virtual string Name { get; set; }
        public virtual DbType? Type { get; set; }
        public virtual int Size { get; set; }
        public virtual int Precision { get; set; }
        public virtual string CustomType { get; set; }
        public virtual object DefaultValue { get; set; }
        public virtual bool IsForeignKey { get; set; }
        public virtual bool IsIdentity { get; set; }
        public virtual bool IsIndexed { get; set; }
        public virtual bool IsPrimaryKey { get; set; }
        public virtual string PrimaryKeyName { get; set; }
        public virtual bool IsNullable { get; set; }
        public virtual bool IsUnique { get; set; }
        public virtual string TableName { get; set; }
        public virtual ColumnModificationType ModificationType { get; set; }
    }

    public enum ColumnModificationType
    {
        Create,
        Alter
    }
}