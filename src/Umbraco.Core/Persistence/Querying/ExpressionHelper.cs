using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Persistence.Querying
{
    internal class ExpressionHelper<T>
    {
        private string selectExpression = string.Empty;
        private string whereExpression;
        private string groupBy = string.Empty;
        private string havingExpression;
        private string orderBy = string.Empty;

        IList<string> updateFields = new List<string>();
        IList<string> insertFields = new List<string>();

        private string sep = string.Empty;
        private bool useFieldName = false;
        private Database.PocoData pd;

        public ExpressionHelper()
        {
            Database.Mapper = new ModelDtoMapper();
            pd = new Database.PocoData(typeof(T));
        }

        protected internal virtual string Visit(Expression exp)
        {

            if (exp == null) return string.Empty;
            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return VisitLambda(exp as LambdaExpression);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess(exp as MemberExpression);
                case ExpressionType.Constant:
                    return VisitConstant(exp as ConstantExpression);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return "(" + VisitBinary(exp as BinaryExpression) + ")";
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary(exp as UnaryExpression);
                case ExpressionType.Parameter:
                    return VisitParameter(exp as ParameterExpression);
                case ExpressionType.Call:
                    return VisitMethodCall(exp as MethodCallExpression);
                case ExpressionType.New:
                    return VisitNew(exp as NewExpression);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray(exp as NewArrayExpression);
                default:
                    return exp.ToString();
            }
        }

        protected virtual string VisitLambda(LambdaExpression lambda)
        {
            if (lambda.Body.NodeType == ExpressionType.MemberAccess && sep == " ")
            {
                MemberExpression m = lambda.Body as MemberExpression;

                if (m.Expression != null)
                {
                    string r = VisitMemberAccess(m);
                    return string.Format("{0}={1}", r, GetQuotedTrueValue());
                }

            }
            return Visit(lambda.Body);
        }

        protected virtual string VisitBinary(BinaryExpression b)
        {
            string left, right;
            var operand = BindOperant(b.NodeType);   //sep= " " ??
            if (operand == "AND" || operand == "OR")
            {
                MemberExpression m = b.Left as MemberExpression;
                if (m != null && m.Expression != null)
                {
                    string r = VisitMemberAccess(m);
                    left = string.Format("{0}={1}", r, GetQuotedTrueValue());
                }
                else
                {
                    left = Visit(b.Left);
                }
                m = b.Right as MemberExpression;
                if (m != null && m.Expression != null)
                {
                    string r = VisitMemberAccess(m);
                    right = string.Format("{0}={1}", r, GetQuotedTrueValue());
                }
                else
                {
                    right = Visit(b.Right);
                }
            }
            else
            {
                left = Visit(b.Left);
                right = Visit(b.Right);
            }

            if (operand == "=" && right == "null") operand = "is";
            else if (operand == "<>" && right == "null") operand = "is not";
            else if (operand == "=" || operand == "<>")
            {
                if (IsTrueExpression(right)) right = GetQuotedTrueValue();
                else if (IsFalseExpression(right)) right = GetQuotedFalseValue();

                if (IsTrueExpression(left)) left = GetQuotedTrueValue();
                else if (IsFalseExpression(left)) left = GetQuotedFalseValue();

            }

            switch (operand)
            {
                case "MOD":
                case "COALESCE":
                    return string.Format("{0}({1},{2})", operand, left, right);
                default:
                    return left + sep + operand + sep + right;
            }
        }

        protected virtual string VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null &&
               m.Expression.NodeType == ExpressionType.Parameter
               && m.Expression.Type == typeof(T))
            {
                string field = GetFieldName(pd, m.Member.Name);
                return field;
            }

            if (m.Expression != null && m.Expression.NodeType != ExpressionType.Constant)
            {
                Database.Mapper = new ModelDtoMapper();
                var def = new Database.PocoData(m.Expression.Type);
                string field = GetFieldName(def, m.Member.Name);
                return field;
            }


            var member = Expression.Convert(m, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            object o = getter();
            return GetQuotedValue(o, o != null ? o.GetType() : null);

        }

        protected virtual string VisitNew(NewExpression nex)
        {
            // TODO : check !
            var member = Expression.Convert(nex, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            try
            {
                var getter = lambda.Compile();
                object o = getter();
                return GetQuotedValue(o, o.GetType());
            }
            catch (System.InvalidOperationException)
            { // FieldName ?
                List<Object> exprs = VisitExpressionList(nex.Arguments);
                var r = new StringBuilder();
                foreach (Object e in exprs)
                {
                    r.AppendFormat("{0}{1}",
                                   r.Length > 0 ? "," : "",
                                   e);
                }
                return r.ToString();
            }

        }

        protected virtual string VisitParameter(ParameterExpression p)
        {
            return p.Name;
        }

        protected virtual string VisitConstant(ConstantExpression c)
        {
            if (c.Value == null)
                return "null";
            else if (c.Value.GetType() == typeof(bool))
            {
                object o = GetQuotedValue(c.Value, c.Value.GetType());
                return string.Format("({0}={1})", GetQuotedTrueValue(), o);
            }
            else
                return GetQuotedValue(c.Value, c.Value.GetType());
        }

        protected virtual string VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    string o = Visit(u.Operand);
                    if (IsFieldName(o)) o = o + "=" + GetQuotedValue(true, typeof(bool));
                    return "NOT (" + o + ")";
                default:
                    return Visit(u.Operand);
            }

        }

        protected virtual string VisitMethodCall(MethodCallExpression m)
        {
            List<Object> args = this.VisitExpressionList(m.Arguments);

            Object r;
            if (m.Object != null)
                r = Visit(m.Object);
            else
            {
                r = args[0];
                args.RemoveAt(0);
            }

            switch (m.Method.Name)
            {
                case "ToUpper":
                    return string.Format("upper({0})", r);
                case "ToLower":
                    return string.Format("lower({0})", r);
                case "StartsWith":
                    return string.Format("upper({0}) starting with {1} ", r, args[0].ToString().ToUpper());
                case "EndsWith":
                    return string.Format("upper({0}) like '%{1}'", r, RemoveQuote(args[0].ToString()).ToUpper());
                case "Contains":
                    return string.Format("upper({0}) like '%{1}%'", r, RemoveQuote(args[0].ToString()).ToUpper());
                case "Substring":
                    var startIndex = Int32.Parse(args[0].ToString()) + 1;
                    if (args.Count == 2)
                    {
                        var length = Int32.Parse(args[1].ToString());
                        return string.Format("substring({0} from {1} for {2})",
                                         r,
                                         startIndex,
                                         length);
                    }
                    else
                        return string.Format("substring({0} from {1})",
                                         r,
                                         startIndex);
                case "Round":
                case "Floor":
                case "Ceiling":
                case "Coalesce":
                case "Abs":
                case "Sum":
                    return string.Format("{0}({1}{2})",
                                         m.Method.Name,
                                         r,
                                         args.Count == 1 ? string.Format(",{0}", args[0]) : "");
                case "Concat":
                    var s = new StringBuilder();
                    foreach (Object e in args)
                    {
                        s.AppendFormat(" || {0}", e);
                    }
                    return string.Format("{0}{1}", r, s.ToString());

                case "In":

                    var member = Expression.Convert(m.Arguments[1], typeof(object));
                    var lambda = Expression.Lambda<Func<object>>(member);
                    var getter = lambda.Compile();

                    var inArgs = getter() as object[];

                    var sIn = new StringBuilder();
                    foreach (Object e in inArgs)
                    {
                        if (e.GetType().ToString() != "System.Collections.Generic.List`1[System.Object]")
                        {
                            sIn.AppendFormat("{0}{1}",
                                         sIn.Length > 0 ? "," : "",
                                                        GetQuotedValue(e, e.GetType()));
                        }
                        else
                        {
                            var listArgs = e as IList<Object>;
                            foreach (Object el in listArgs)
                            {
                                sIn.AppendFormat("{0}{1}",
                                         sIn.Length > 0 ? "," : "",
                                                        GetQuotedValue(el, el.GetType()));
                            }
                        }
                    }

                    return string.Format("{0} {1} ({2})", r, m.Method.Name, sIn.ToString());
                case "Desc":
                    return string.Format("{0} DESC", r);
                case "Alias":
                case "As":
                    return string.Format("{0} As {1}", r,
                                                GetQuotedColumnName(RemoveQuoteFromAlias(RemoveQuote(args[0].ToString()))));
                case "ToString":
                    return r.ToString();
                default:
                    var s2 = new StringBuilder();
                    foreach (Object e in args)
                    {
                        s2.AppendFormat(",{0}", GetQuotedValue(e, e.GetType()));
                    }
                    return string.Format("{0}({1}{2})", m.Method.Name, r, s2.ToString());
            }
        }

        protected virtual List<Object> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            var list = new List<Object>();
            for (int i = 0, n = original.Count; i < n; i++)
            {
                if (original[i].NodeType == ExpressionType.NewArrayInit ||
                 original[i].NodeType == ExpressionType.NewArrayBounds)
                {

                    list.AddRange(VisitNewArrayFromExpressionList(original[i] as NewArrayExpression));
                }
                else
                    list.Add(Visit(original[i]));

            }
            return list;
        }

        protected virtual string VisitNewArray(NewArrayExpression na)
        {

            List<Object> exprs = VisitExpressionList(na.Expressions);
            var r = new StringBuilder();
            foreach (Object e in exprs)
            {
                r.Append(r.Length > 0 ? "," + e : e);
            }

            return r.ToString();
        }

        protected virtual List<Object> VisitNewArrayFromExpressionList(NewArrayExpression na)
        {

            List<Object> exprs = VisitExpressionList(na.Expressions);
            return exprs;
        }


        protected virtual string BindOperant(ExpressionType e)
        {

            switch (e)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "MOD";
                case ExpressionType.Coalesce:
                    return "COALESCE";
                default:
                    return e.ToString();
            }
        }

        public virtual string GetQuotedTableName(string tableName)
        {
            return string.Format("\"{0}\"", tableName);
        }

        public virtual string GetQuotedColumnName(string columnName)
        {
            return string.Format("\"{0}\"", columnName);
        }

        public virtual string GetQuotedName(string name)
        {
            return string.Format("\"{0}\"", name);
        }

        private string GetQuotedTrueValue()
        {
            return GetQuotedValue(true, typeof(bool));
        }

        private string GetQuotedFalseValue()
        {
            return GetQuotedValue(false, typeof(bool));
        }

        public virtual string GetQuotedValue(object value, Type fieldType)
        {
            if (value == null) return "NULL";

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

            return ShouldQuoteValue(fieldType)
                    ? "'" + EscapeParam(value) + "'"
                    : value.ToString();
        }

        public virtual string EscapeParam(object paramValue)
        {
            return paramValue.ToString().Replace("'", "''");
        }

        public virtual bool ShouldQuoteValue(Type fieldType)
        {
            return true;
        }

        protected virtual string GetFieldName(Database.PocoData pocoData, string name)
        {
            var column = pocoData.Columns.FirstOrDefault(x => x.Value.PropertyInfo.Name == name);
            return column.Value.ColumnName;
        }

        protected virtual string GetFieldName(string name)
        {

            if (useFieldName)
            {
                //FieldDefinition fd = modelDef.FieldDefinitions.FirstOrDefault(x => x.Name == name);
                //string fn = fd != default(FieldDefinition) ? fd.FieldName : name;
                //return OrmLiteConfig.DialectProvider.GetQuotedColumnName(fn);
                return "[" + name + "]";
            }
            else
            {
                return name;
            }
        }

        protected string RemoveQuote(string exp)
        {

            if (exp.StartsWith("'") && exp.EndsWith("'"))
            {
                exp = exp.Remove(0, 1);
                exp = exp.Remove(exp.Length - 1, 1);
            }
            return exp;
        }

        protected string RemoveQuoteFromAlias(string exp)
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

        private string GetTrueExpression()
        {
            object o = GetQuotedTrueValue();
            return string.Format("({0}={1})", o, o);
        }

        private string GetFalseExpression()
        {

            return string.Format("({0}={1})",
                GetQuotedTrueValue(),
                GetQuotedFalseValue());
        }

        private bool IsTrueExpression(string exp)
        {
            return (exp == GetTrueExpression());
        }

        private bool IsFalseExpression(string exp)
        {
            return (exp == GetFalseExpression());
        }

        protected bool IsFieldName(string quotedExp)
        {
            return true;
        }
    }
}