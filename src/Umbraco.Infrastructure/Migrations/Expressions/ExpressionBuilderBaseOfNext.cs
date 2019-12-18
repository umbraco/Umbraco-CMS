using System.Data;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions
{
    /// <summary>
    /// Provides a base class for expression builders.
    /// </summary>
    public abstract class ExpressionBuilderBase<TExpression, TNext> : ExpressionBuilderBase<TExpression>
        where TExpression : IMigrationExpression
        where TNext : IFluentBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBuilderBase{TExpression, TNext}"/> class.
        /// </summary>
        protected ExpressionBuilderBase(TExpression expression)
            : base(expression)
        { }

        public abstract ColumnDefinition GetColumnForType();

        private ColumnDefinition Column => GetColumnForType();

        public TNext AsAnsiString()
        {
            Column.Type = DbType.AnsiString;
            return (TNext)(object)this;
        }

        public TNext AsAnsiString(int size)
        {
            Column.Type = DbType.AnsiString;
            Column.Size = size;
            return (TNext)(object)this;
        }

        public TNext AsBinary()
        {
            Column.Type = DbType.Binary;
            return (TNext)(object)this;
        }

        public TNext AsBinary(int size)
        {
            Column.Type = DbType.Binary;
            Column.Size = size;
            return (TNext)(object)this;
        }

        public TNext AsBoolean()
        {
            Column.Type = DbType.Boolean;
            return (TNext)(object)this;
        }

        public TNext AsByte()
        {
            Column.Type = DbType.Byte;
            return (TNext)(object)this;
        }

        public TNext AsCurrency()
        {
            Column.Type = DbType.Currency;
            return (TNext)(object)this;
        }

        public TNext AsDate()
        {
            Column.Type = DbType.Date;
            return (TNext)(object)this;
        }

        public TNext AsDateTime()
        {
            Column.Type = DbType.DateTime;
            return (TNext)(object)this;
        }

        public TNext AsDecimal()
        {
            Column.Type = DbType.Decimal;
            return (TNext)(object)this;
        }

        public TNext AsDecimal(int size, int precision)
        {
            Column.Type = DbType.Decimal;
            Column.Size = size;
            Column.Precision = precision;
            return (TNext)(object)this;
        }

        public TNext AsDouble()
        {
            Column.Type = DbType.Double;
            return (TNext)(object)this;
        }

        public TNext AsFixedLengthString(int size)
        {
            Column.Type = DbType.StringFixedLength;
            Column.Size = size;
            return (TNext)(object)this;
        }

        public TNext AsFixedLengthAnsiString(int size)
        {
            Column.Type = DbType.AnsiStringFixedLength;
            Column.Size = size;
            return (TNext)(object)this;
        }

        public TNext AsFloat()
        {
            Column.Type = DbType.Single;
            return (TNext)(object)this;
        }

        public TNext AsGuid()
        {
            Column.Type = DbType.Guid;
            return (TNext)(object)this;
        }

        public TNext AsInt16()
        {
            Column.Type = DbType.Int16;
            return (TNext)(object)this;
        }

        public TNext AsInt32()
        {
            Column.Type = DbType.Int32;
            return (TNext)(object)this;
        }

        public TNext AsInt64()
        {
            Column.Type = DbType.Int64;
            return (TNext)(object)this;
        }

        public TNext AsString()
        {
            Column.Type = DbType.String;
            return (TNext)(object)this;
        }

        public TNext AsString(int size)
        {
            Column.Type = DbType.String;
            Column.Size = size;
            return (TNext)(object)this;
        }

        public TNext AsTime()
        {
            Column.Type = DbType.Time;
            return (TNext)(object)this;
        }

        public TNext AsXml()
        {
            Column.Type = DbType.Xml;
            return (TNext)(object)this;
        }

        public TNext AsXml(int size)
        {
            Column.Type = DbType.Xml;
            Column.Size = size;
            return (TNext)(object)this;
        }

        public TNext AsCustom(string customType)
        {
            Column.Type = null;
            Column.CustomType = customType;
            return (TNext)(object)this;
        }
    }
}
