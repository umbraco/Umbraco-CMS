using System;
using System.Globalization;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// Logic that is shared with the expression helpers
    /// </summary>
    internal class BaseExpressionHelper
    {
        protected string HandleStringComparison(string col, string val, string verb, TextColumnType columnType)
        {
            switch (verb)
            {
                case "SqlWildcard":
                    return SqlSyntaxContext.SqlSyntaxProvider.GetStringColumnWildcardComparison(col, RemoveQuote(val), columnType);
                case "Equals":
                    return SqlSyntaxContext.SqlSyntaxProvider.GetStringColumnEqualComparison(col, RemoveQuote(val), columnType);
                case "StartsWith":
                    return SqlSyntaxContext.SqlSyntaxProvider.GetStringColumnStartsWithComparison(col, RemoveQuote(val), columnType);
                case "EndsWith":
                    return SqlSyntaxContext.SqlSyntaxProvider.GetStringColumnEndsWithComparison(col, RemoveQuote(val), columnType);
                case "Contains":
                    return SqlSyntaxContext.SqlSyntaxProvider.GetStringColumnContainsComparison(col, RemoveQuote(val), columnType);
                case "InvariantEquals":
                case "SqlEquals":
                    //recurse
                    return HandleStringComparison(col, val, "Equals", columnType);
                case "InvariantStartsWith":
                case "SqlStartsWith":
                    //recurse
                    return HandleStringComparison(col, val, "StartsWith", columnType);
                case "InvariantEndsWith":
                case "SqlEndsWith":
                    //recurse
                    return HandleStringComparison(col, val, "EndsWith", columnType);
                case "InvariantContains":
                case "SqlContains":
                    //recurse
                    return HandleStringComparison(col, val, "Contains", columnType);
                default:
                    throw new ArgumentOutOfRangeException("verb");
            }
        }

        public virtual string GetQuotedValue(object value, Type fieldType, Func<object, string> escapeCallback = null, Func<Type, bool> shouldQuoteCallback = null)
        {
            if (value == null) return "NULL";

            if (escapeCallback == null)
            {
                escapeCallback = EscapeParam;
            }
            if (shouldQuoteCallback == null)
            {
                shouldQuoteCallback = ShouldQuoteValue;
            }

            if (!fieldType.UnderlyingSystemType.IsValueType && fieldType != typeof(string))
            {
                //if (TypeSerializer.CanCreateFromString(fieldType))
                //{
                //    return "'" + escapeCallback(TypeSerializer.SerializeToString(value)) + "'";
                //}

                throw new NotSupportedException(
                    string.Format("Property of type: {0} is not supported", fieldType.FullName));
            }

            if (fieldType == typeof(int))
                return ((int)value).ToString(CultureInfo.InvariantCulture);

            if (fieldType == typeof(float))
                return ((float)value).ToString(CultureInfo.InvariantCulture);

            if (fieldType == typeof(double))
                return ((double)value).ToString(CultureInfo.InvariantCulture);

            if (fieldType == typeof(decimal))
                return ((decimal)value).ToString(CultureInfo.InvariantCulture);

            if (fieldType == typeof(DateTime))
            {
                return "'" + escapeCallback(((DateTime)value).ToIsoString()) + "'";
            }

            if (fieldType == typeof(bool))
                return ((bool)value) ? Convert.ToString(1, CultureInfo.InvariantCulture) : Convert.ToString(0, CultureInfo.InvariantCulture);

            return shouldQuoteCallback(fieldType)
                       ? "'" + escapeCallback(value) + "'"
                       : value.ToString();
        }

        public virtual string EscapeParam(object paramValue)
        {
            return paramValue == null 
                ? string.Empty 
                : SqlSyntaxContext.SqlSyntaxProvider.EscapeString(paramValue.ToString());
        }
        
        public virtual bool ShouldQuoteValue(Type fieldType)
        {
            return true;
        }

        protected virtual string RemoveQuote(string exp)
        {
            if (exp.StartsWith("'") && exp.EndsWith("'"))
            {
                exp = exp.Remove(0, 1);
                exp = exp.Remove(exp.Length - 1, 1);
            }
            return exp;
        }

        protected virtual string RemoveQuoteFromAlias(string exp)
        {

            if ((exp.StartsWith("\"") || exp.StartsWith("`") || exp.StartsWith("'"))
                &&
                (exp.EndsWith("\"") || exp.EndsWith("`") || exp.EndsWith("'")))
            {
                exp = exp.Remove(0, 1);
                exp = exp.Remove(exp.Length - 1, 1);
            }
            return exp;
        }
    }
}