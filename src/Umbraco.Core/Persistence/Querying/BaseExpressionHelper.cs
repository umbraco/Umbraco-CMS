using System;
using System.Globalization;

namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// Logic that is shared with the expression helpers
    /// </summary>
    internal class BaseExpressionHelper
    {
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
                //    return "'" + EscapeParam(TypeSerializer.SerializeToString(value)) + "'";
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
                return "'" + EscapeParam(((DateTime)value).ToIsoString()) + "'";
            }

            if (fieldType == typeof(bool))
                return ((bool)value) ? Convert.ToString(1, CultureInfo.InvariantCulture) : Convert.ToString(0, CultureInfo.InvariantCulture);

            return ShouldQuoteValue(fieldType)
                       ? "'" + EscapeParam(value) + "'"
                       : value.ToString();
        }

        public virtual string EscapeParam(object paramValue)
        {
            return paramValue.ToString().Replace("'", "''");
        }

        public virtual string EscapeAtArgument(string exp)
        {
            if (exp.StartsWith("@"))
                return string.Concat("@", exp);

            return exp;
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