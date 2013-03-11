using System.Data;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Migrations.Syntax
{
    public abstract class ExpressionBuilder<ExpressionT, NextT> : ExpressionBuilderBase<ExpressionT>
        where ExpressionT : IMigrationExpression
        where NextT : IFluentSyntax
    {
        protected ExpressionBuilder(ExpressionT expression)
            : base(expression)
        {
        }

        public abstract ColumnDefinition GetColumnForType();

        private ColumnDefinition Column
        {
            get { return GetColumnForType(); }
        }

        public NextT AsAnsiString()
        {
            Column.Type = DbType.AnsiString;
            return (NextT)(object)this;
        }

        public NextT AsAnsiString(int size)
        {
            Column.Type = DbType.AnsiString;
            Column.Size = size;
            return (NextT)(object)this;
        }

        public NextT AsBinary()
        {
            Column.Type = DbType.Binary;
            return (NextT)(object)this;
        }

        public NextT AsBinary(int size)
        {
            Column.Type = DbType.Binary;
            Column.Size = size;
            return (NextT)(object)this;
        }

        public NextT AsBoolean()
        {
            Column.Type = DbType.Boolean;
            return (NextT)(object)this;
        }

        public NextT AsByte()
        {
            Column.Type = DbType.Byte;
            return (NextT)(object)this;
        }

        public NextT AsCurrency()
        {
            Column.Type = DbType.Currency;
            return (NextT)(object)this;
        }

        public NextT AsDate()
        {
            Column.Type = DbType.Date;
            return (NextT)(object)this;
        }

        public NextT AsDateTime()
        {
            Column.Type = DbType.DateTime;
            return (NextT)(object)this;
        }

        public NextT AsDecimal()
        {
            Column.Type = DbType.Decimal;
            return (NextT)(object)this;
        }

        public NextT AsDecimal(int size, int precision)
        {
            Column.Type = DbType.Decimal;
            Column.Size = size;
            Column.Precision = precision;
            return (NextT)(object)this;
        }

        public NextT AsDouble()
        {
            Column.Type = DbType.Double;
            return (NextT)(object)this;
        }

        public NextT AsFixedLengthString(int size)
        {
            Column.Type = DbType.StringFixedLength;
            Column.Size = size;
            return (NextT)(object)this;
        }

        public NextT AsFixedLengthAnsiString(int size)
        {
            Column.Type = DbType.AnsiStringFixedLength;
            Column.Size = size;
            return (NextT)(object)this;
        }

        public NextT AsFloat()
        {
            Column.Type = DbType.Single;
            return (NextT)(object)this;
        }

        public NextT AsGuid()
        {
            Column.Type = DbType.Guid;
            return (NextT)(object)this;
        }

        public NextT AsInt16()
        {
            Column.Type = DbType.Int16;
            return (NextT)(object)this;
        }

        public NextT AsInt32()
        {
            Column.Type = DbType.Int32;
            return (NextT)(object)this;
        }

        public NextT AsInt64()
        {
            Column.Type = DbType.Int64;
            return (NextT)(object)this;
        }

        public NextT AsString()
        {
            Column.Type = DbType.String;
            return (NextT)(object)this;
        }

        public NextT AsString(int size)
        {
            Column.Type = DbType.String;
            Column.Size = size;
            return (NextT)(object)this;
        }

        public NextT AsTime()
        {
            Column.Type = DbType.Time;
            return (NextT)(object)this;
        }

        public NextT AsXml()
        {
            Column.Type = DbType.Xml;
            return (NextT)(object)this;
        }

        public NextT AsXml(int size)
        {
            Column.Type = DbType.Xml;
            Column.Size = size;
            return (NextT)(object)this;
        }

        public NextT AsCustom(string customType)
        {
            Column.Type = null;
            Column.CustomType = customType;
            return (NextT)(object)this;
        }
    }
}