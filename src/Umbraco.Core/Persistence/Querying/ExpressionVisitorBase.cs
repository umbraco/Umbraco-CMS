using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Persistence.Querying
{
    // TODO: are we basically duplicating entire parts of NPoco just because of SqlSyntax ?!
    // try to use NPoco's version !

    /// <summary>
    /// An expression tree parser to create SQL statements and SQL parameters based on a strongly typed expression.
    /// </summary>
    /// <remarks>This object is stateful and cannot be re-used to parse an expression.</remarks>
    internal abstract class ExpressionVisitorBase
    {
        protected ExpressionVisitorBase(ISqlSyntaxProvider sqlSyntax)
        {
            SqlSyntax = sqlSyntax;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the visited expression has been visited already,
        /// in which case visiting will just populate the SQL parameters.
        /// </summary>
        protected bool Visited { get; set; }

        /// <summary>
        /// Gets or sets the SQL syntax provider for the current database.
        /// </summary>
        protected ISqlSyntaxProvider SqlSyntax { get; }

        /// <summary>
        /// Gets the list of SQL parameters.
        /// </summary>
        protected readonly List<object> SqlParameters = new List<object>();

        /// <summary>
        /// Gets the SQL parameters.
        /// </summary>
        /// <returns></returns>
        public object[] GetSqlParameters()
        {
            return SqlParameters.ToArray();
        }

        /// <summary>
        /// Visits the expression and produces the corresponding SQL statement.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns>The SQL statement corresponding to the expression.</returns>
        /// <remarks>Also populates the SQL parameters.</remarks>
        public virtual string Visit(Expression expression)
        {
            if (expression == null) return string.Empty;

            // if the expression is a CachedExpression,
            // visit the inner expression if not already visited
            var cachedExpression = expression as CachedExpression;
            if (cachedExpression != null)
            {
                Visited = cachedExpression.Visited;
                expression = cachedExpression.InnerExpression;
            }

            string result;

            switch (expression.NodeType)
            {
                case ExpressionType.Lambda:
                    result = VisitLambda(expression as LambdaExpression);
                    break;
                case ExpressionType.MemberAccess:
                    result = VisitMemberAccess(expression as MemberExpression);
                    break;
                case ExpressionType.Constant:
                    result = VisitConstant(expression as ConstantExpression);
                    break;
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
                    result = VisitBinary(expression as BinaryExpression);
                    break;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    result = VisitUnary(expression as UnaryExpression);
                    break;
                case ExpressionType.Parameter:
                    result = VisitParameter(expression as ParameterExpression);
                    break;
                case ExpressionType.Call:
                    result = VisitMethodCall(expression as MethodCallExpression);
                    break;
                case ExpressionType.New:
                    result = VisitNew(expression as NewExpression);
                    break;
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    result = VisitNewArray(expression as NewArrayExpression);
                    break;
                default:
                    result = expression.ToString();
                    break;
            }

            // if the expression is a CachedExpression,
            // and is not already compiled, assign the result
            if (cachedExpression == null)
                return result;
            if (!cachedExpression.Visited)
                cachedExpression.VisitResult = result;
            return cachedExpression.VisitResult;
        }

        protected abstract string VisitMemberAccess(MemberExpression m);

        protected virtual string VisitLambda(LambdaExpression lambda)
        {
            if (lambda.Body.NodeType == ExpressionType.MemberAccess &&
                lambda.Body is MemberExpression memberExpression && memberExpression.Expression != null)
                {
                    //This deals with members that are boolean (i.e. x => IsTrashed )
                    var result = VisitMemberAccess(memberExpression);

                    SqlParameters.Add(true);

                    return Visited ? string.Empty : $"{result} = @{SqlParameters.Count - 1}";
                }

            return Visit(lambda.Body);
        }

        protected virtual string VisitBinary(BinaryExpression b)
        {
            var left = string.Empty;
            var right = string.Empty;

            var operand = BindOperant(b.NodeType);
            if (operand == "AND" || operand == "OR")
            {
                if (b.Left is MemberExpression mLeft && mLeft.Expression != null)
                {
                    var r = VisitMemberAccess(mLeft);

                    SqlParameters.Add(true);

                    if (Visited == false)
                        left = $"{r} = @{SqlParameters.Count - 1}";
                }
                else
                {
                    left = Visit(b.Left);
                }
                if (b.Right is MemberExpression mRight && mRight.Expression != null)
                {
                    var r = VisitMemberAccess(mRight);

                    SqlParameters.Add(true);

                    if (Visited == false)
                        right = $"{r} = @{SqlParameters.Count - 1}";
                }
                else
                {
                    right = Visit(b.Right);
                }
            }
            else if (operand == "=")
            {
                // deal with (x == true|false) - most common
                if (b.Right is ConstantExpression constRight && constRight.Type == typeof(bool))
                    return (bool) constRight.Value ? VisitNotNot(b.Left) : VisitNot(b.Left);
                right = Visit(b.Right);

                // deal with (true|false == x) - why not
                if (b.Left is ConstantExpression constLeft && constLeft.Type == typeof(bool))
                    return (bool) constLeft.Value ? VisitNotNot(b.Right) : VisitNot(b.Right);
                left = Visit(b.Left);
            }
            else if (operand == "<>")
            {
                // deal with (x != true|false) - most common
                if (b.Right is ConstantExpression constRight && constRight.Type == typeof (bool))
                    return (bool) constRight.Value ? VisitNot(b.Left) : VisitNotNot(b.Left);
                right = Visit(b.Right);

                // deal with (true|false != x) - why not
                if (b.Left is ConstantExpression constLeft && constLeft.Type == typeof (bool))
                    return (bool) constLeft.Value ? VisitNot(b.Right) : VisitNotNot(b.Right);
                left = Visit(b.Left);
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
                //if (IsTrueExpression(right)) right = GetQuotedTrueValue();
                //else if (IsFalseExpression(right)) right = GetQuotedFalseValue();

                //if (IsTrueExpression(left)) left = GetQuotedTrueValue();
                //else if (IsFalseExpression(left)) left = GetQuotedFalseValue();

            }

            switch (operand)
            {
                case "MOD":
                case "COALESCE":
                    return Visited ? string.Empty : $"{operand}({left},{right})";

                default:
                    return Visited ? string.Empty : $"({left} {operand} {right})";
            }
        }

        protected virtual List<object> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            var list = new List<object>();
            for (int i = 0, n = original.Count; i < n; i++)
            {
                if (original[i].NodeType == ExpressionType.NewArrayInit ||
                    original[i].NodeType == ExpressionType.NewArrayBounds)
                {
                    list.AddRange(VisitNewArrayFromExpressionList(original[i] as NewArrayExpression));
                }
                else
                {
                    list.Add(Visit(original[i]));
                }
            }
            return list;
        }

        protected virtual string VisitNew(NewExpression newExpression)
        {
            // TODO: check !
            var member = Expression.Convert(newExpression, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            try
            {
                var getter = lambda.Compile();
                var o = getter();

                SqlParameters.Add(o);

                return Visited ? string.Empty : $"@{SqlParameters.Count - 1}";
            }
            catch (InvalidOperationException)
            {
                if (Visited)
                    return string.Empty;

                var exprs = VisitExpressionList(newExpression.Arguments);
                return string.Join(",", exprs);
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

            SqlParameters.Add(c.Value);

            return Visited ? string.Empty : $"@{SqlParameters.Count - 1}";
        }

        protected virtual string VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    return VisitNot(u.Operand);
                default:
                    return Visit(u.Operand);
            }
        }

        private string VisitNot(Expression exp)
        {
            var o = Visit(exp);

            // use a "NOT (...)" syntax instead of "<>" since we don't know whether "<>" works in all sql servers
            // also, x.StartsWith(...) translates to "x LIKE '...%'" which we cannot "<>" and have to "NOT (...")

            switch (exp.NodeType)
            {
                case ExpressionType.MemberAccess:
                    // false property , i.e. x => !Trashed
                    // BUT we don't want to do a NOT SQL statement since this generally results in indexes not being used
                    // so we want to do an == false
                    SqlParameters.Add(false);
                    return Visited ? string.Empty : $"{o} = @{SqlParameters.Count - 1}";
                    //return Visited ? string.Empty : $"NOT ({o} = @{SqlParameters.Count - 1})";
                default:
                    // could be anything else, such as: x => !x.Path.StartsWith("-20")
                    return Visited ? string.Empty : string.Concat("NOT (", o, ")");
            }
        }

        private string VisitNotNot(Expression exp)
        {
            var o = Visit(exp);

            switch (exp.NodeType)
            {
                case ExpressionType.MemberAccess:
                    // true property, i.e. x => Trashed
                    SqlParameters.Add(true);
                    return Visited ? string.Empty : $"({o} = @{SqlParameters.Count - 1})";
                default:
                    // could be anything else, such as: x => x.Path.StartsWith("-20")
                    return Visited ? string.Empty : o;
            }
        }

        protected virtual string VisitNewArray(NewArrayExpression na)
        {
            var exprs = VisitExpressionList(na.Expressions);
            return Visited ? string.Empty : string.Join(",", exprs);
        }

        protected virtual List<object> VisitNewArrayFromExpressionList(NewArrayExpression na)
            => VisitExpressionList(na.Expressions);

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

        protected virtual string VisitMethodCall(MethodCallExpression m)
        {
            // m.Object is the expression that represent the instance for instance method class, or null for static method calls
            // m.Arguments is the collection of expressions that represent arguments of the called method
            // m.MethodInfo is the method info for the method to be called

            // assume that static methods are extension methods (probably not ok)
            // and then, the method object is its first argument - get "safe" object
            var methodObject = m.Object ?? m.Arguments[0];
            var visitedMethodObject = Visit(methodObject);
            // and then, "safe" arguments are what would come after the first arg
            var methodArgs = m.Object == null
                ? new ReadOnlyCollection<Expression>(m.Arguments.Skip(1).ToList())
                : m.Arguments;

            switch (m.Method.Name)
            {
                case "ToString":
                    SqlParameters.Add(methodObject.ToString());
                    return Visited ? string.Empty : $"@{SqlParameters.Count - 1}";

                case "ToUpper":
                    return Visited ? string.Empty : $"upper({visitedMethodObject})";

                case "ToLower":
                    return Visited ? string.Empty : $"lower({visitedMethodObject})";

                case "Contains":
                    // for 'Contains', it can either be the string.Contains(string) method, or a collection Contains
                    // method, which would then need to become a SQL IN clause - but beware that string is
                    // an enumerable of char, and string.Contains(char) is an extension method - but NOT an SQL IN

                    var isCollectionContains =
                        (
                            m.Object == null && // static (extension?) method
                            m.Arguments.Count == 2 && // with two args
                            m.Arguments[0].Type != typeof(string) && // but not for string
                            TypeHelper.IsTypeAssignableFrom<IEnumerable>(m.Arguments[0].Type) && // first arg being an enumerable
                            m.Arguments[1].NodeType == ExpressionType.MemberAccess // second arg being a member access
                        ) ||
                        (
                            m.Object != null && // instance method
                            TypeHelper.IsTypeAssignableFrom<IEnumerable>(m.Object.Type) && // of an enumerable
                            m.Object.Type != typeof(string) && // but not for string
                            m.Arguments.Count == 1 && // with 1 arg
                            m.Arguments[0].NodeType == ExpressionType.MemberAccess // arg being a member access
                        );

                    if (isCollectionContains)
                        goto case "SqlIn";
                    else
                        goto case "Contains**String";

                case "SqlWildcard":
                case "StartsWith":
                case "EndsWith":
                case "Contains**String": // see "Contains" above
                case "Equals":
                case "SqlStartsWith":
                case "SqlEndsWith":
                case "SqlContains":
                case "SqlEquals":
                case "InvariantStartsWith":
                case "InvariantEndsWith":
                case "InvariantContains":
                case "InvariantEquals":

                    string compareValue;

                    if (methodArgs[0].NodeType != ExpressionType.Constant)
                    {
                        // if it's a field accessor, we could Visit(methodArgs[0]) and get [a].[b]
                        // but then, what if we want more, eg .StartsWith(node.Path + ',') ? => not

                        //This occurs when we are getting a value from a non constant such as: x => x.Path.StartsWith(content.Path)
                        // So we'll go get the value:
                        var member = Expression.Convert(methodArgs[0], typeof(object));
                        var lambda = Expression.Lambda<Func<object>>(member);
                        var getter = lambda.Compile();
                        compareValue = getter().ToString();
                    }
                    else
                    {
                        compareValue = methodArgs[0].ToString();
                    }

                    //default column type
                    var colType = TextColumnType.NVarchar;

                    //then check if the col type argument has been passed to the current method (this will be the case for methods like
                    // SqlContains and other Sql methods)
                    if (methodArgs.Count > 1)
                    {
                        var colTypeArg = methodArgs.FirstOrDefault(x => x is ConstantExpression && x.Type == typeof(TextColumnType));
                        if (colTypeArg != null)
                        {
                            colType = (TextColumnType)((ConstantExpression)colTypeArg).Value;
                        }
                    }

                    return HandleStringComparison(visitedMethodObject, compareValue, m.Method.Name, colType);

                case "Replace":
                    string searchValue;

                    if (methodArgs[0].NodeType != ExpressionType.Constant)
                    {
                        //This occurs when we are getting a value from a non constant such as: x => x.Path.StartsWith(content.Path)
                        // So we'll go get the value:
                        var member = Expression.Convert(methodArgs[0], typeof(object));
                        var lambda = Expression.Lambda<Func<object>>(member);
                        var getter = lambda.Compile();
                        searchValue = getter().ToString();
                    }
                    else
                    {
                        searchValue = methodArgs[0].ToString();
                    }

                    if (methodArgs[0].Type != typeof(string) && TypeHelper.IsTypeAssignableFrom<IEnumerable>(methodArgs[0].Type))
                    {
                        throw new NotSupportedException("An array Contains method is not supported");
                    }

                    string replaceValue;

                    if (methodArgs[1].NodeType != ExpressionType.Constant)
                    {
                        //This occurs when we are getting a value from a non constant such as: x => x.Path.StartsWith(content.Path)
                        // So we'll go get the value:
                        var member = Expression.Convert(methodArgs[1], typeof(object));
                        var lambda = Expression.Lambda<Func<object>>(member);
                        var getter = lambda.Compile();
                        replaceValue = getter().ToString();
                    }
                    else
                    {
                        replaceValue = methodArgs[1].ToString();
                    }

                    if (methodArgs[1].Type != typeof(string) && TypeHelper.IsTypeAssignableFrom<IEnumerable>(methodArgs[1].Type))
                    {
                        throw new NotSupportedException("An array Contains method is not supported");
                    }

                    SqlParameters.Add(RemoveQuote(searchValue));
                    SqlParameters.Add(RemoveQuote(replaceValue));

                    //don't execute if compiled
                    return Visited ? string.Empty : $"replace({visitedMethodObject}, @{SqlParameters.Count - 2}, @{SqlParameters.Count - 1})";

                //case "Substring":
                //    var startIndex = Int32.Parse(args[0].ToString()) + 1;
                //    if (args.Count == 2)
                //    {
                //        var length = Int32.Parse(args[1].ToString());
                //        return string.Format("substring({0} from {1} for {2})",
                //                         r,
                //                         startIndex,
                //                         length);
                //    }
                //    else
                //        return string.Format("substring({0} from {1})",
                //                         r,
                //                         startIndex);
                //case "Round":
                //case "Floor":
                //case "Ceiling":
                //case "Coalesce":
                //case "Abs":
                //case "Sum":
                //    return string.Format("{0}({1}{2})",
                //                         m.Method.Name,
                //                         r,
                //                         args.Count == 1 ? string.Format(",{0}", args[0]) : "");
                //case "Concat":
                //    var s = new StringBuilder();
                //    foreach (Object e in args)
                //    {
                //        s.AppendFormat(" || {0}", e);
                //    }
                //    return string.Format("{0}{1}", r, s);

                case "SqlIn":

                    if (methodArgs.Count != 1 || methodArgs[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("SqlIn must contain the member being accessed.");

                    var memberAccess = VisitMemberAccess((MemberExpression) methodArgs[0]);

                    var inMember = Expression.Convert(methodObject, typeof(object));
                    var inLambda = Expression.Lambda<Func<object>>(inMember);
                    var inGetter = inLambda.Compile();

                    var inArgs = (IEnumerable) inGetter();

                    var inBuilder = new StringBuilder();
                    var inFirst = true;

                    inBuilder.Append(memberAccess);
                    inBuilder.Append(" IN (");

                    foreach (var e in inArgs)
                    {
                        SqlParameters.Add(e);
                        if (inFirst) inFirst = false; else inBuilder.Append(",");
                        inBuilder.Append("@");
                        inBuilder.Append(SqlParameters.Count - 1);
                    }

                    inBuilder.Append(")");
                    return inBuilder.ToString();

                //case "Desc":
                //    return string.Format("{0} DESC", r);
                //case "Alias":
                //case "As":
                //    return string.Format("{0} As {1}", r,
                //                                GetQuotedColumnName(RemoveQuoteFromAlias(RemoveQuote(args[0].ToString()))));

                case "SqlText":
                    if (m.Method.DeclaringType != typeof(NPocoSqlExtensions.Statics))
                        goto default;
                    if (m.Arguments.Count == 2)
                    {
                        var n1 = Visit(m.Arguments[0]);
                        var f = m.Arguments[2];
                        if (!(f is Expression<Func<string, string>> fl))
                            throw new NotSupportedException("Expression is not a proper lambda.");
                        var ff = fl.Compile();
                        return ff(n1);
                    }
                    else if (m.Arguments.Count == 3)
                    {
                        var n1 = Visit(m.Arguments[0]);
                        var n2 = Visit(m.Arguments[1]);
                        var f = m.Arguments[2];
                        if (!(f is Expression<Func<string, string, string>> fl))
                            throw new NotSupportedException("Expression is not a proper lambda.");
                        var ff = fl.Compile();
                        return ff(n1, n2);
                    }
                    else if (m.Arguments.Count == 4)
                    {
                        var n1 = Visit(m.Arguments[0]);
                        var n2 = Visit(m.Arguments[1]);
                        var n3 = Visit(m.Arguments[3]);
                        var f = m.Arguments[3];
                        if (!(f is Expression<Func<string, string, string, string>> fl))
                            throw new NotSupportedException("Expression is not a proper lambda.");
                        var ff = fl.Compile();
                        return ff(n1, n2, n3);
                    }
                    else
                        throw new NotSupportedException("Expression is not a proper lambda.");

                // c# 'x == null' becomes sql 'x IS NULL' which is fine
                // c# 'x == y' becomes sql 'x = @0' which is fine - unless they are nullable types,
                //  because sql 'x = NULL' is always false and the 'IS NULL' syntax is required,
                // so for comparing nullable types, we use x.SqlNullableEquals(y, fb) where fb is a fallback
                // value which will be used when values are null - turning the comparison into
                // sql 'COALESCE(x,fb) = COALESCE(y,fb)' - of course, fb must be a value outside
                // of x and y range - and if that is not possible, then a manual comparison need
                // to be written
                // TODO: support SqlNullableEquals with 0 parameters, using the full syntax below
                case "SqlNullableEquals":
                    var compareTo = Visit(m.Arguments[1]);
                    var fallback = Visit(m.Arguments[2]);
                    // that would work without a fallback value but is more cumbersome
                    //return Visited ? string.Empty : $"((({compareTo} is null) AND ({visitedMethodObject} is null)) OR (({compareTo} is not null) AND ({visitedMethodObject} = {compareTo})))";
                    // use a fallback value
                    return Visited ? string.Empty : $"(COALESCE({visitedMethodObject},{fallback}) = COALESCE({compareTo},{fallback}))";

                default:

                    throw new ArgumentOutOfRangeException("No logic supported for " + m.Method.Name);

                    //var s2 = new StringBuilder();
                    //foreach (Object e in args)
                    //{
                    //    s2.AppendFormat(",{0}", GetQuotedValue(e, e.GetType()));
                    //}
                    //return string.Format("{0}({1}{2})", m.Method.Name, r, s2.ToString());
            }
        }

        public virtual string GetQuotedTableName(string tableName)
            => GetQuotedName(tableName);

        public virtual string GetQuotedColumnName(string columnName)
            => GetQuotedName(columnName);

        public virtual string GetQuotedName(string name)
            => Visited ? name : "\"" + name + "\"";

        protected string HandleStringComparison(string col, string val, string verb, TextColumnType columnType)
        {
            switch (verb)
            {
                case "SqlWildcard":
                    SqlParameters.Add(RemoveQuote(val));
                    return Visited ? string.Empty : SqlSyntax.GetStringColumnWildcardComparison(col, SqlParameters.Count - 1, columnType);

                case "Equals":
                case "InvariantEquals":
                case "SqlEquals":
                    SqlParameters.Add(RemoveQuote(val));
                    return Visited ? string.Empty : SqlSyntax.GetStringColumnEqualComparison(col, SqlParameters.Count - 1, columnType);

                case "StartsWith":
                case "InvariantStartsWith":
                case "SqlStartsWith":
                    SqlParameters.Add(RemoveQuote(val) + SqlSyntax.GetWildcardPlaceholder());
                    return Visited ? string.Empty : SqlSyntax.GetStringColumnWildcardComparison(col, SqlParameters.Count - 1, columnType);

                case "EndsWith":
                case "InvariantEndsWith":
                case "SqlEndsWith":
                    SqlParameters.Add(SqlSyntax.GetWildcardPlaceholder() + RemoveQuote(val));
                    return Visited ? string.Empty : SqlSyntax.GetStringColumnWildcardComparison(col, SqlParameters.Count - 1, columnType);

                case "Contains":
                case "InvariantContains":
                case "SqlContains":
                    var wildcardPlaceholder = SqlSyntax.GetWildcardPlaceholder();
                    SqlParameters.Add(wildcardPlaceholder + RemoveQuote(val) + wildcardPlaceholder);
                    return Visited ? string.Empty : SqlSyntax.GetStringColumnWildcardComparison(col, SqlParameters.Count - 1, columnType);

                default:
                    throw new ArgumentOutOfRangeException(nameof(verb));
            }
        }

        public virtual string EscapeParam(object paramValue, ISqlSyntaxProvider sqlSyntax)
        {
            return paramValue == null
                ? string.Empty
                : sqlSyntax.EscapeString(paramValue.ToString());
        }

        protected virtual string RemoveQuote(string exp)
        {
            if (exp.IsNullOrWhiteSpace()) return exp;

            var c = exp[0];
            return (c == '"' || c == '`' || c == '\'') && exp[exp.Length - 1] == c
                ? exp.Length == 1
                    ? string.Empty
                    : exp.Substring(1, exp.Length - 2)
                : exp;
        }
    }
}
