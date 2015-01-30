using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Dynamics;
using Umbraco.Web.Models;

namespace Umbraco.Web.Dynamics
{
    //SD: I wish all of this wasn't hacked and was just the original dynamic linq from MS... sigh. Just
    // means we can't really use it for anything other than dynamic node (i think)
    // I'm fairly sure it's just hte convert to dynamic null stuff... still seems to work for normal linq operations would love to make it
    // properly one day.

	internal class ExpressionParser<T>
	{
		struct Token
		{
			public TokenId Id { get; set; }
			public string Text { get; set; }
			public int Pos { get; set; }
		}

		enum TokenId
		{
			Unknown,
			End,
			Identifier,
			StringLiteral,
			IntegerLiteral,
			RealLiteral,
			Exclamation,
			Percent,
			Amphersand,
			OpenParen,
			CloseParen,
			Asterisk,
			Plus,
			Comma,
			Minus,
			Dot,
			Slash,
			Colon,
			LessThan,
			Equal,
			GreaterThan,
			Question,
			OpenBracket,
			CloseBracket,
			Bar,
			ExclamationEqual,
			DoubleAmphersand,
			LessThanEqual,
			LessGreater,
			DoubleEqual,
			GreaterThanEqual,
			DoubleBar
		}

		interface ILogicalSignatures
		{
			void F(bool x, bool y);
			void F(bool? x, bool? y);
		}

		interface IArithmeticSignatures
		{
			void F(int x, int y);
			void F(uint x, uint y);
			void F(long x, long y);
			void F(ulong x, ulong y);
			void F(float x, float y);
			void F(double x, double y);
			void F(decimal x, decimal y);
			void F(int? x, int? y);
			void F(uint? x, uint? y);
			void F(long? x, long? y);
			void F(ulong? x, ulong? y);
			void F(float? x, float? y);
			void F(double? x, double? y);
			void F(decimal? x, decimal? y);
		}

		interface IRelationalSignatures : IArithmeticSignatures
		{
			void F(string x, string y);
			void F(char x, char y);
			void F(DateTime x, DateTime y);
			void F(TimeSpan x, TimeSpan y);
			void F(char? x, char? y);
			void F(DateTime? x, DateTime? y);
			void F(TimeSpan? x, TimeSpan? y);
		}

		interface IEqualitySignatures : IRelationalSignatures
		{
			void F(bool x, bool y);
			void F(bool? x, bool? y);
		}

		interface IAddSignatures : IArithmeticSignatures
		{
			void F(DateTime x, TimeSpan y);
			void F(TimeSpan x, TimeSpan y);
			void F(DateTime? x, TimeSpan? y);
			void F(TimeSpan? x, TimeSpan? y);
		}

		interface ISubtractSignatures : IAddSignatures
		{
			void F(DateTime x, DateTime y);
			void F(DateTime? x, DateTime? y);
		}

		interface INegationSignatures
		{
			void F(int x);
			void F(long x);
			void F(float x);
			void F(double x);
			void F(decimal x);
			void F(int? x);
			void F(long? x);
			void F(float? x);
			void F(double? x);
			void F(decimal? x);
		}

		interface INotSignatures
		{
			void F(bool x);
			void F(bool? x);
		}

		interface IEnumerableSignatures
		{
			void Where(bool predicate);
			void Any();
			void Any(bool predicate);
			void All(bool predicate);
			void Count();
			void Count(bool predicate);
			void Min(object selector);
			void Max(object selector);
			void Sum(int selector);
			void Sum(int? selector);
			void Sum(long selector);
			void Sum(long? selector);
			void Sum(float selector);
			void Sum(float? selector);
			void Sum(double selector);
			void Sum(double? selector);
			void Sum(decimal selector);
			void Sum(decimal? selector);
			void Average(int selector);
			void Average(int? selector);
			void Average(long selector);
			void Average(long? selector);
			void Average(float selector);
			void Average(float? selector);
			void Average(double selector);
			void Average(double? selector);
			void Average(decimal selector);
			void Average(decimal? selector);
		}

		static readonly Type[] predefinedTypes = {
		                                         	typeof(Object),
		                                         	typeof(Boolean),
		                                         	typeof(Char),
		                                         	typeof(String),
		                                         	typeof(SByte),
		                                         	typeof(Byte),
		                                         	typeof(Int16),
		                                         	typeof(UInt16),
		                                         	typeof(Int32),
		                                         	typeof(UInt32),
		                                         	typeof(Int64),
		                                         	typeof(UInt64),
		                                         	typeof(Single),
		                                         	typeof(Double),
		                                         	typeof(Decimal),
		                                         	typeof(DateTime),
		                                         	typeof(TimeSpan),
		                                         	typeof(Guid),
		                                         	typeof(Math),
		                                         	typeof(Convert)
		                                         };

		static readonly Expression trueLiteral = Expression.Constant(true);
		static readonly Expression falseLiteral = Expression.Constant(false);
		static readonly Expression nullLiteral = Expression.Constant(null);

		static readonly string keywordIt = "it";
		static readonly string keywordIif = "iif";
		static readonly string keywordNew = "new";

		static Dictionary<string, object> keywords;

		Dictionary<string, object> symbols;
		IDictionary<string, object> externals;
		Dictionary<Expression, string> literals;
		ParameterExpression it;
		string text;
		private readonly bool _flagConvertDynamicNullToBooleanFalse;
		int textPos;
		int textLen;
		char ch;
		Token token;

		public ExpressionParser(ParameterExpression[] parameters, string expression, object[] values, bool flagConvertDynamicNullToBooleanFalse)
		{
			if (expression == null) throw new ArgumentNullException("expression");
			if (keywords == null) keywords = CreateKeywords();
			symbols = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			literals = new Dictionary<Expression, string>();
			if (parameters != null) ProcessParameters(parameters);
			if (values != null) ProcessValues(values);
			text = expression;
			_flagConvertDynamicNullToBooleanFalse = flagConvertDynamicNullToBooleanFalse;
			textLen = text.Length;
			SetTextPos(0);
			NextToken();
		}

		void ProcessParameters(ParameterExpression[] parameters)
		{
			foreach (ParameterExpression pe in parameters)
				if (!String.IsNullOrEmpty(pe.Name))
					AddSymbol(pe.Name, pe);
			if (parameters.Length == 1 && String.IsNullOrEmpty(parameters[0].Name))
				it = parameters[0];
		}

		void ProcessValues(object[] values)
		{
			for (int i = 0; i < values.Length; i++)
			{
				object value = values[i];
				if (i == values.Length - 1 && value is IDictionary<string, object>)
				{
					externals = (IDictionary<string, object>)value;
				}
				else
				{
					AddSymbol("@" + i.ToString(System.Globalization.CultureInfo.InvariantCulture), value);
				}
			}
		}

		void AddSymbol(string name, object value)
		{
			if (symbols.ContainsKey(name))
				throw ParseError(Res.DuplicateIdentifier, name);
			symbols.Add(name, value);
		}

		public Expression Parse(Type resultType)
		{
			int exprPos = token.Pos;
			Expression expr = ParseExpression();
			if (resultType != null)
				if ((expr = PromoteExpression(expr, resultType, true)) == null)
					throw ParseError(exprPos, Res.ExpressionTypeMismatch, GetTypeName(resultType));
			ValidateToken(TokenId.End, Res.SyntaxError);
			return expr;
		}

#pragma warning disable 0219
		public IEnumerable<DynamicOrdering> ParseOrdering()
		{
			List<DynamicOrdering> orderings = new List<DynamicOrdering>();
			while (true)
			{
				Expression expr = ParseExpression();
				bool ascending = true;
				if (TokenIdentifierIs("asc") || TokenIdentifierIs("ascending"))
				{
					NextToken();
				}
				else if (TokenIdentifierIs("desc") || TokenIdentifierIs("descending"))
				{
					NextToken();
					ascending = false;
				}
				orderings.Add(new DynamicOrdering { Selector = expr, Ascending = ascending });
				if (token.Id != TokenId.Comma) break;
				NextToken();
			}
			ValidateToken(TokenId.End, Res.SyntaxError);
			return orderings;
		}
#pragma warning restore 0219

		// ?: operator
		Expression ParseExpression()
		{
			int errorPos = token.Pos;
			Expression expr = ParseLogicalOr();
			if (token.Id == TokenId.Question)
			{
				NextToken();
				Expression expr1 = ParseExpression();
				ValidateToken(TokenId.Colon, Res.ColonExpected);
				NextToken();
				Expression expr2 = ParseExpression();
				expr = GenerateConditional(expr, expr1, expr2, errorPos);
			}
			return expr;
		}

		// ||, or operator
		Expression ParseLogicalOr()
		{
			Expression left = ParseLogicalAnd();
			while (token.Id == TokenId.DoubleBar || TokenIdentifierIs("or"))
			{
				Token op = token;
				NextToken();
				Expression right = ParseLogicalAnd();
				CheckAndPromoteOperands(typeof(ILogicalSignatures), op.Text, ref left, ref right, op.Pos);
				left = HandleDynamicNodeLambdas(ExpressionType.OrElse, left, right);
			}
			return left;
		}

		// &&, and operator
		Expression ParseLogicalAnd()
		{
			Expression left = ParseComparison();
			while (token.Id == TokenId.DoubleAmphersand || TokenIdentifierIs("and"))
			{
				Token op = token;
				NextToken();
				Expression right = ParseComparison();
				CheckAndPromoteOperands(typeof(ILogicalSignatures), op.Text, ref left, ref right, op.Pos);
				left = HandleDynamicNodeLambdas(ExpressionType.AndAlso, left, right);
			}
			return left;
		}

		// =, ==, !=, <>, >, >=, <, <= operators
		Expression ParseComparison()
		{
			Expression left = ParseAdditive();
			while (token.Id == TokenId.Equal || token.Id == TokenId.DoubleEqual ||
			       token.Id == TokenId.ExclamationEqual || token.Id == TokenId.LessGreater ||
			       token.Id == TokenId.GreaterThan || token.Id == TokenId.GreaterThanEqual ||
			       token.Id == TokenId.LessThan || token.Id == TokenId.LessThanEqual)
			{
				Token op = token;
				NextToken();
				Expression right = ParseAdditive();
				bool isEquality = op.Id == TokenId.Equal || op.Id == TokenId.DoubleEqual ||
				                  op.Id == TokenId.ExclamationEqual || op.Id == TokenId.LessGreater;
				if (isEquality && !left.Type.IsValueType && !right.Type.IsValueType)
				{
					if (left.Type != right.Type)
					{
						if (left.Type.IsAssignableFrom(right.Type))
						{
							right = Expression.Convert(right, left.Type);
						}
						else if (right.Type.IsAssignableFrom(left.Type))
						{
							left = Expression.Convert(left, right.Type);
						}
						else if (left is LambdaExpression || right is LambdaExpression)
						{
							//do nothing here (but further down we'll handle the lambdaexpression)
						}
						else
						{
							throw IncompatibleOperandsError(op.Text, left, right, op.Pos);
						}
					}
				}
				else if (IsEnumType(left.Type) || IsEnumType(right.Type))
				{
					if (left.Type != right.Type)
					{
						Expression e;
						if ((e = PromoteExpression(right, left.Type, true)) != null)
						{
							right = e;
						}
						else if ((e = PromoteExpression(left, right.Type, true)) != null)
						{
							left = e;
						}
						else
						{
							throw IncompatibleOperandsError(op.Text, left, right, op.Pos);
						}
					}
				}
				else
				{
					CheckAndPromoteOperands(isEquality ? typeof(IEqualitySignatures) : typeof(IRelationalSignatures),
					                        op.Text, ref left, ref right, op.Pos);
				}
				switch (op.Id)
				{
					case TokenId.Equal:
					case TokenId.DoubleEqual:
						left = HandleDynamicNodeLambdas(ExpressionType.Equal, left, right);
						break;
					case TokenId.ExclamationEqual:
					case TokenId.LessGreater:
						left = HandleDynamicNodeLambdas(ExpressionType.NotEqual, left, right);
						break;
					case TokenId.GreaterThan:
						left = HandleDynamicNodeLambdas(ExpressionType.GreaterThan, left, right);
						break;
					case TokenId.GreaterThanEqual:
						left = HandleDynamicNodeLambdas(ExpressionType.GreaterThanOrEqual, left, right);
						break;
					case TokenId.LessThan:
						left = HandleDynamicNodeLambdas(ExpressionType.LessThan, left, right);
						break;
					case TokenId.LessThanEqual:
						left = HandleDynamicNodeLambdas(ExpressionType.LessThanOrEqual, left, right);
						break;
				}
			}
			return left;
		}

		// +, -, & operators
		Expression ParseAdditive()
		{
			Expression left = ParseMultiplicative();
			while (token.Id == TokenId.Plus || token.Id == TokenId.Minus ||
			       token.Id == TokenId.Amphersand)
			{
				Token op = token;
				NextToken();
				Expression right = ParseMultiplicative();
				switch (op.Id)
				{
					case TokenId.Plus:
						if (left.Type == typeof(string) || right.Type == typeof(string))
							goto case TokenId.Amphersand;
						CheckAndPromoteOperands(typeof(IAddSignatures), op.Text, ref left, ref right, op.Pos);
						left = GenerateAdd(left, right);
						break;
					case TokenId.Minus:
						CheckAndPromoteOperands(typeof(ISubtractSignatures), op.Text, ref left, ref right, op.Pos);
						left = GenerateSubtract(left, right);
						break;
					case TokenId.Amphersand:
						left = GenerateStringConcat(left, right);
						break;
				}
			}
			return left;
		}

		// *, /, %, mod operators
		Expression ParseMultiplicative()
		{
			Expression left = ParseUnary();
			while (token.Id == TokenId.Asterisk || token.Id == TokenId.Slash ||
			       token.Id == TokenId.Percent || TokenIdentifierIs("mod"))
			{
				Token op = token;
				NextToken();
				Expression right = ParseUnary();
				CheckAndPromoteOperands(typeof(IArithmeticSignatures), op.Text, ref left, ref right, op.Pos);
				switch (op.Id)
				{
					case TokenId.Asterisk:
						left = Expression.Multiply(left, right);
						break;
					case TokenId.Slash:
						left = Expression.Divide(left, right);
						break;
					case TokenId.Percent:
					case TokenId.Identifier:
						left = HandleDynamicNodeLambdas(ExpressionType.Modulo, left, right);
						break;
				}
			}
			return left;
		}

		// -, !, not unary operators
		Expression ParseUnary()
		{
			if (token.Id == TokenId.Minus || token.Id == TokenId.Exclamation ||
			    TokenIdentifierIs("not"))
			{
				Token op = token;
				NextToken();
				if (op.Id == TokenId.Minus && (token.Id == TokenId.IntegerLiteral ||
				                               token.Id == TokenId.RealLiteral))
				{
					token.Text = "-" + token.Text;
					token.Pos = op.Pos;
					return ParsePrimary();
				}
				Expression expr = ParseUnary();
				if (op.Id == TokenId.Minus)
				{
					CheckAndPromoteOperand(typeof(INegationSignatures), op.Text, ref expr, op.Pos);
					expr = Expression.Negate(expr);
				}
				else
				{
					CheckAndPromoteOperand(typeof(INotSignatures), op.Text, ref expr, op.Pos);
					if (expr is LambdaExpression)
					{
						ParameterExpression[] parameters = new ParameterExpression[(expr as LambdaExpression).Parameters.Count];
						(expr as LambdaExpression).Parameters.CopyTo(parameters, 0);
						var invokedExpr = Expression.Invoke(expr, parameters);
						var not = Expression.Not(Expression.TypeAs(invokedExpr, typeof(Nullable<bool>)));
						expr = Expression.Lambda<Func<T, bool>>(
							Expression.Condition(
								Expression.Property(not, "HasValue"),
								Expression.Property(not, "Value"),
								Expression.Constant(false, typeof(bool))
								), parameters);
					}
					else
					{
						expr = Expression.Not(expr);
					}
				}
				return expr;
			}
			return ParsePrimary();
		}

		Expression ParsePrimary()
		{
			Expression expr = ParsePrimaryStart();
			while (true)
			{
				if (token.Id == TokenId.Dot)
				{
					NextToken();
					expr = ParseMemberAccess(null, expr);
				}
				else if (token.Id == TokenId.OpenBracket)
				{
					expr = ParseElementAccess(expr);
				}
				else
				{
					break;
				}
			}
			return expr;
		}

		Expression ParsePrimaryStart()
		{
			switch (token.Id)
			{
				case TokenId.Identifier:
					return ParseIdentifier();
				case TokenId.StringLiteral:
					return ParseStringLiteral();
				case TokenId.IntegerLiteral:
					return ParseIntegerLiteral();
				case TokenId.RealLiteral:
					return ParseRealLiteral();
				case TokenId.OpenParen:
					return ParseParenExpression();
				default:
					throw ParseError(Res.ExpressionExpected);
			}
		}

		Expression ParseStringLiteral()
		{
			ValidateToken(TokenId.StringLiteral);
			char quote = token.Text[0];
			string s = token.Text.Substring(1, token.Text.Length - 2);
			int start = 0;
			while (true)
			{
				int i = s.IndexOf(quote, start);
				if (i < 0) break;
				s = s.Remove(i, 1);
				start = i + 1;
			}
			if (quote == '\'')
			{
				if (s.Length != 1)
					throw ParseError(Res.InvalidCharacterLiteral);
				NextToken();
				return CreateLiteral(s[0], s);
			}
			NextToken();
			return CreateLiteral(s, s);
		}

		Expression ParseIntegerLiteral()
		{
			ValidateToken(TokenId.IntegerLiteral);
			string text = token.Text;
			if (text[0] != '-')
			{
				ulong value;
				if (!UInt64.TryParse(text, out value))
					throw ParseError(Res.InvalidIntegerLiteral, text);
				NextToken();
				if (value <= (ulong)Int32.MaxValue) return CreateLiteral((int)value, text);
				if (value <= (ulong)UInt32.MaxValue) return CreateLiteral((uint)value, text);
				if (value <= (ulong)Int64.MaxValue) return CreateLiteral((long)value, text);
				return CreateLiteral(value, text);
			}
			else
			{
				long value;
				if (!Int64.TryParse(text, out value))
					throw ParseError(Res.InvalidIntegerLiteral, text);
				NextToken();
				if (value >= Int32.MinValue && value <= Int32.MaxValue)
					return CreateLiteral((int)value, text);
				return CreateLiteral(value, text);
			}
		}

		Expression ParseRealLiteral()
		{
			ValidateToken(TokenId.RealLiteral);
			string text = token.Text;
			object value = null;
			char last = text[text.Length - 1];
			if (last == 'F' || last == 'f')
			{
				float f;
				if (Single.TryParse(text.Substring(0, text.Length - 1), out f)) value = f;
			}
			else
			{
				double d;
				if (Double.TryParse(text, out d)) value = d;
			}
			if (value == null) throw ParseError(Res.InvalidRealLiteral, text);
			NextToken();
			return CreateLiteral(value, text);
		}

		Expression CreateLiteral(object value, string text)
		{
			ConstantExpression expr = Expression.Constant(value);
			literals.Add(expr, text);
			return expr;
		}

		Expression ParseParenExpression()
		{
			ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
			NextToken();
			Expression e = ParseExpression();
			ValidateToken(TokenId.CloseParen, Res.CloseParenOrOperatorExpected);
			NextToken();
			return e;
		}

		Expression ParseIdentifier()
		{
			ValidateToken(TokenId.Identifier);
			object value;
			if (keywords.TryGetValue(token.Text, out value))
			{
				if (value is Type) return ParseTypeAccess((Type)value);
				if (value == (object)keywordIt) return ParseIt();
				if (value == (object)keywordIif) return ParseIif();
				if (value == (object)keywordNew) return ParseNew();
				NextToken();
				return (Expression)value;
			}
			if (symbols.TryGetValue(token.Text, out value) ||
			    externals != null && externals.TryGetValue(token.Text, out value))
			{
				Expression expr = value as Expression;
				if (expr == null)
				{
					expr = Expression.Constant(value);
				}
				else
				{
					LambdaExpression lambda = expr as LambdaExpression;
					if (lambda != null) return ParseLambdaInvocation(lambda);
				}
				NextToken();
				return expr;
			}
			if (it != null) return ParseMemberAccess(null, it);
			throw ParseError(Res.UnknownIdentifier, token.Text);
		}

		Expression ParseIt()
		{
			if (it == null)
				throw ParseError(Res.NoItInScope);
			NextToken();
			return it;
		}

		Expression ParseIif()
		{
			int errorPos = token.Pos;
			NextToken();
			Expression[] args = ParseArgumentList();
			if (args.Length != 3)
				throw ParseError(errorPos, Res.IifRequiresThreeArgs);
			return GenerateConditional(args[0], args[1], args[2], errorPos);
		}

		Expression GenerateConditional(Expression test, Expression expr1, Expression expr2, int errorPos)
		{
			if (test.Type != typeof(bool))
				throw ParseError(errorPos, Res.FirstExprMustBeBool);
			if (expr1.Type != expr2.Type)
			{
				Expression expr1as2 = expr2 != nullLiteral ? PromoteExpression(expr1, expr2.Type, true) : null;
				Expression expr2as1 = expr1 != nullLiteral ? PromoteExpression(expr2, expr1.Type, true) : null;
				if (expr1as2 != null && expr2as1 == null)
				{
					expr1 = expr1as2;
				}
				else if (expr2as1 != null && expr1as2 == null)
				{
					expr2 = expr2as1;
				}
				else
				{
					string type1 = expr1 != nullLiteral ? expr1.Type.Name : "null";
					string type2 = expr2 != nullLiteral ? expr2.Type.Name : "null";
					if (expr1as2 != null && expr2as1 != null)
						throw ParseError(errorPos, Res.BothTypesConvertToOther, type1, type2);
					throw ParseError(errorPos, Res.NeitherTypeConvertsToOther, type1, type2);
				}
			}
			return Expression.Condition(test, expr1, expr2);
		}

		Expression ParseNew()
		{
			NextToken();
			ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
			NextToken();
			List<DynamicProperty> properties = new List<DynamicProperty>();
			List<Expression> expressions = new List<Expression>();
			while (true)
			{
				int exprPos = token.Pos;
				Expression expr = ParseExpression();
				string propName;
				if (TokenIdentifierIs("as"))
				{
					NextToken();
					propName = GetIdentifier();
					NextToken();
				}
				else
				{
					MemberExpression me = expr as MemberExpression;
					if (me == null) throw ParseError(exprPos, Res.MissingAsClause);
					propName = me.Member.Name;
				}
				expressions.Add(expr);
				properties.Add(new DynamicProperty(propName, expr.Type));
				if (token.Id != TokenId.Comma) break;
				NextToken();
			}
			ValidateToken(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
			NextToken();
			Type type = DynamicExpression.CreateClass(properties);
			MemberBinding[] bindings = new MemberBinding[properties.Count];
			for (int i = 0; i < bindings.Length; i++)
				bindings[i] = Expression.Bind(type.GetProperty(properties[i].Name), expressions[i]);
			return Expression.MemberInit(Expression.New(type), bindings);
		}

		Expression ParseLambdaInvocation(LambdaExpression lambda)
		{
			int errorPos = token.Pos;
			NextToken();
			Expression[] args = ParseArgumentList();
			MethodBase method;
			if (FindMethod(lambda.Type, "Invoke", false, args, out method) != 1)
				throw ParseError(errorPos, Res.ArgsIncompatibleWithLambda);
			return Expression.Invoke(lambda, args);
		}

		Expression ParseTypeAccess(Type type)
		{
			int errorPos = token.Pos;
			NextToken();
			if (token.Id == TokenId.Question)
			{
				if (!type.IsValueType || IsNullableType(type))
					throw ParseError(errorPos, Res.TypeHasNoNullableForm, GetTypeName(type));
				type = typeof(Nullable<>).MakeGenericType(type);
				NextToken();
			}
			if (token.Id == TokenId.OpenParen)
			{
				Expression[] args = ParseArgumentList();
				MethodBase method;
				switch (FindBestMethod(type.GetConstructors(), args, out method))
				{
					case 0:
						if (args.Length == 1)
							return GenerateConversion(args[0], type, errorPos);
						throw ParseError(errorPos, Res.NoMatchingConstructor, GetTypeName(type));
					case 1:
						return Expression.New((ConstructorInfo)method, args);
					default:
						throw ParseError(errorPos, Res.AmbiguousConstructorInvocation, GetTypeName(type));
				}
			}
			ValidateToken(TokenId.Dot, Res.DotOrOpenParenExpected);
			NextToken();
			return ParseMemberAccess(type, null);
		}

		Expression GenerateConversion(Expression expr, Type type, int errorPos)
		{
			Type exprType = expr.Type;
			if (exprType == type) return expr;
			if (exprType.IsValueType && type.IsValueType)
			{
				if ((IsNullableType(exprType) || IsNullableType(type)) &&
				    GetNonNullableType(exprType) == GetNonNullableType(type))
					return Expression.Convert(expr, type);
				if ((IsNumericType(exprType) || IsEnumType(exprType)) &&
				    (IsNumericType(type)) || IsEnumType(type))
					return Expression.ConvertChecked(expr, type);
			}
			if (exprType.IsAssignableFrom(type) || type.IsAssignableFrom(exprType) ||
			    exprType.IsInterface || type.IsInterface)
				return Expression.Convert(expr, type);
			throw ParseError(errorPos, Res.CannotConvertValue,
			                 GetTypeName(exprType), GetTypeName(type));
		}

		Expression ParseMemberAccess(Type type, Expression instance)
		{
			//NOTE: SD: There is a lot of string checking going on here and I'm 99% sure this can all be done better
			// in a more generic sense to support any types with any extension methods, etc...
			// Too bad whoever wrote this decided not to put any code comments in :(
			// This is how to support method calls, etc... in dynamic statements.

			if (instance != null) type = instance.Type;
			int errorPos = token.Pos;
			string id = GetIdentifier();
			NextToken();
			if (token.Id == TokenId.OpenParen)
			{
				if (instance != null && type != typeof(string))
				{
					Type enumerableType = FindGenericType(typeof(IEnumerable<>), type);
					if (enumerableType != null)
					{
						Type elementType = enumerableType.GetGenericArguments()[0];
						return ParseAggregate(instance, elementType, id, errorPos);
					}
				}
				Expression[] args = ParseArgumentList();
				MethodBase mb;
				LambdaExpression instanceAsString = null;
				ParameterExpression instanceExpression = Expression.Parameter(typeof(T), "instance");
				if (type.IsGenericType && type != typeof(string))
				{
					var typeArgs = type.GetGenericArguments();
					if (typeArgs[0] == typeof(T))
					{
						if (instance != null && instance is LambdaExpression)
						{							
							//not sure why this is object or why we need to do this but if we change it, things die...
							//also not sure why it is changed to string, i think this might be to ensure string methods are supported
							//but seems to me that we then won't support other types of methods?
							if (typeArgs[1] == typeof(object))
							{
								instanceAsString = StringFormat(instance as LambdaExpression, instanceExpression);
								type = typeof(string);
							}
							else if (typeArgs[1] == typeof(string))
							{
								instanceAsString = instance as LambdaExpression;
								type = typeof(string);
							}
							//else
							//{
							//	instanceAsString = instance as LambdaExpression;
							//	type = typeArgs[1];	
							//}
							
						}
					}
				}
				switch (FindMethod(type, id, instance == null, args, out mb))
				{
					case 0:
						//not found

						//SD: I have yet to see extension methods actually being called in the dynamic parsing... need to unit test these
						// scenarios and figure out why all this type checking occurs.

                        var runtimeCache = ApplicationContext.Current != null ? ApplicationContext.Current.ApplicationCache.RuntimeCache : new NullCacheProvider();

						if (type == typeof(string) && instanceAsString != null)
						{
							Expression[] newArgs = (new List<Expression>() { Expression.Invoke(instanceAsString, instanceExpression) }).Concat(args).ToArray();
							mb = ExtensionMethodFinder.FindExtensionMethod(runtimeCache, typeof(string), newArgs, id, true);
							if (mb != null)
							{
								return CallMethodOnDynamicNode(instance, newArgs, instanceAsString, instanceExpression, (MethodInfo)mb, true);
							}
						}
						if (type == typeof(string) && instanceAsString == null && instance is MemberExpression)
						{
							Expression[] newArgs = (new List<Expression>() { instance }).Concat(args).ToArray();
                            mb = ExtensionMethodFinder.FindExtensionMethod(runtimeCache, typeof(string), newArgs, id, true);
							if (mb != null)
							{
								return Expression.Call(null, (MethodInfo)mb, newArgs);
							}
						}

						throw ParseError(errorPos, Res.NoApplicableMethod,
						                 id, GetTypeName(type));
					case 1:
						MethodInfo method = (MethodInfo)mb;
						if (!IsPredefinedType(method.DeclaringType))
							throw ParseError(errorPos, Res.MethodsAreInaccessible, GetTypeName(method.DeclaringType));
						if (method.ReturnType == typeof(void))
							throw ParseError(errorPos, Res.MethodIsVoid,
							                 id, GetTypeName(method.DeclaringType));
						if (instanceAsString != null)
						{
							return CallMethodOnDynamicNode(instance, args, instanceAsString, instanceExpression, method, false);
						}
						return Expression.Call(instance, (MethodInfo)method, args);
					default:
						throw ParseError(errorPos, Res.AmbiguousMethodInvocation,
						                 id, GetTypeName(type));
				}
			}
			else
			{
				//Looks for a member on the type, but above, we're rerouting that into TryGetMember
				MemberInfo member = FindPropertyOrField(type, id, instance == null);
				if (member == null)
				{
					if (typeof(DynamicObject).IsAssignableFrom(type))
					{
						//We are going to generate a dynamic method by hand coding the expression tree
						//this will invoke TryGetMember (but wrapped in an expression tree)
						//so that when it's evaluated, DynamicNode should be supported

						ParameterExpression instanceExpression = Expression.Parameter(typeof(T), "instance");
						ParameterExpression convertDynamicNullToBooleanFalse = Expression.Parameter(typeof(bool), "convertDynamicNullToBooleanFalse");
						ParameterExpression result = Expression.Parameter(typeof(object), "result");
						ParameterExpression binder = Expression.Variable(typeof(DynamicQueryableGetMemberBinder), "binder");
						ParameterExpression ignoreCase = Expression.Variable(typeof(bool), "ignoreCase");
						ConstructorInfo getMemberBinderConstructor = typeof(DynamicQueryableGetMemberBinder).GetConstructor(new Type[] { typeof(string), typeof(bool) });
						LabelTarget blockReturnLabel = Expression.Label(typeof(object));
						MethodInfo method = typeof(T).GetMethod("TryGetMember");

						BlockExpression block = Expression.Block(
							typeof(object),
							new[] { ignoreCase, binder, result, convertDynamicNullToBooleanFalse },
							Expression.Assign(convertDynamicNullToBooleanFalse, Expression.Constant(_flagConvertDynamicNullToBooleanFalse, typeof(bool))),
							Expression.Assign(ignoreCase, Expression.Constant(false, typeof(bool))),
							Expression.Assign(binder, Expression.New(getMemberBinderConstructor, Expression.Constant(id, typeof(string)), ignoreCase)),
							Expression.Assign(result, Expression.Constant(null)),
							Expression.IfThen(Expression.NotEqual(Expression.Constant(null), instanceExpression),
							                  Expression.Call(instanceExpression, method, binder, result)),
							Expression.IfThen(
								Expression.AndAlso(
									Expression.TypeEqual(result, typeof(DynamicNull)),
									Expression.Equal(convertDynamicNullToBooleanFalse, Expression.Constant(true, typeof(bool)))
									),
								Expression.Assign(result, Expression.Constant(false, typeof(object)))
								),
							Expression.Return(blockReturnLabel, result),
							Expression.Label(blockReturnLabel, Expression.Constant(-2, typeof(object)))
							);
						LambdaExpression lax = Expression.Lambda<Func<T, object>>(block, instanceExpression);
						return lax;
					}
					if (typeof(Func<T, object>).IsAssignableFrom(type))
					{
						//accessing a property off an already resolved DynamicNode TryGetMember call
						//e.g. uBlogsyPostDate.Date
						//SD: Removed the NonPublic accessor here because this will never work in medium trust, wondering why it is NonPublic vs Public ? Have changed to Public.
						//MethodInfo ReflectPropertyValue = this.GetType().GetMethod("ReflectPropertyValue", BindingFlags.NonPublic | BindingFlags.Static);
						MethodInfo reflectPropertyValue = this.GetType().GetMethod("ReflectPropertyValue", BindingFlags.Public | BindingFlags.Static);
						ParameterExpression convertDynamicNullToBooleanFalse = Expression.Parameter(typeof(bool), "convertDynamicNullToBooleanFalse");
						ParameterExpression result = Expression.Parameter(typeof(object), "result");
						ParameterExpression idParam = Expression.Parameter(typeof(string), "id");
						ParameterExpression lambdaResult = Expression.Parameter(typeof(object), "lambdaResult");
						ParameterExpression lambdaInstanceExpression = Expression.Parameter(typeof(T), "lambdaInstanceExpression");
						ParameterExpression instanceExpression = Expression.Parameter(typeof(Func<T, object>), "instance");
						LabelTarget blockReturnLabel = Expression.Label(typeof(object));

						BlockExpression block = Expression.Block(
							typeof(object),
							new[] { lambdaResult, result, idParam, convertDynamicNullToBooleanFalse },
							Expression.Assign(convertDynamicNullToBooleanFalse, Expression.Constant(_flagConvertDynamicNullToBooleanFalse, typeof(bool))),
							Expression.Assign(lambdaResult, Expression.Invoke(instance, lambdaInstanceExpression)),
							Expression.Assign(result, Expression.Call(reflectPropertyValue, lambdaResult, Expression.Constant(id))),
							Expression.IfThen(
								Expression.AndAlso(
									Expression.TypeEqual(result, typeof(DynamicNull)),
									Expression.Equal(convertDynamicNullToBooleanFalse, Expression.Constant(true, typeof(bool)))
									),
								Expression.Assign(result, Expression.Constant(false, typeof(object)))
								),
							Expression.Return(blockReturnLabel, result),
							Expression.Label(blockReturnLabel, Expression.Constant(-2, typeof(object)))
							);
						LambdaExpression lax = Expression.Lambda<Func<T, object>>(block, lambdaInstanceExpression);
						return lax;
					}
				}
				else
				{
					return member is PropertyInfo ?
					                              	Expression.Property(instance, (PropertyInfo)member) :
					                              	                                                    	Expression.Field(instance, (FieldInfo)member);
				}
				throw ParseError(errorPos, Res.UnknownPropertyOrField,
				                 id, GetTypeName(type));

			}
		}
		public static object ReflectPropertyValue(object o, string name)
		{
			PropertyInfo propertyInfo = o.GetType().GetProperty(name);
			if (propertyInfo != null)
			{
				object result = propertyInfo.GetValue(o, null);
				return result;
			}
			return null;
		}
		private static Expression CallMethodOnDynamicNode(Expression instance, Expression[] args, LambdaExpression instanceAsString, ParameterExpression instanceExpression, MethodInfo method, bool isStatic)
		{			
			Type methodReturnType = method.ReturnType;

			var defaultReturnValue = Expression.Constant(methodReturnType.GetDefaultValue(), methodReturnType);			

			ParameterExpression result = Expression.Parameter(method.ReturnType, "result");
			LabelTarget blockReturnLabel = Expression.Label(method.ReturnType);
			BlockExpression block = Expression.Block(
				method.ReturnType,
				new[] { result },
				Expression.Assign(result,
				                  Expression.Call(
				                  	isStatic ? null : Expression.Invoke(instanceAsString, instanceExpression),
				                  	method,
				                  	args)
					),
				Expression.Return(blockReturnLabel, result),
				Expression.Label(blockReturnLabel, defaultReturnValue)
				);

			Type func = typeof(Func<,>);
			Type generic = func.MakeGenericType(typeof(T), methodReturnType);
			return Expression.Lambda(generic, block, instanceExpression);

			//if (methodReturnType == typeof(string))
			//	return Expression.Lambda<Func<T, string>>(block, instanceExpression);
			//if (methodReturnType == typeof(int))
			//	return Expression.Lambda<Func<T, int>>(block, instanceExpression);
			//if (methodReturnType == typeof(bool))
			//	return Expression.Lambda<Func<T, bool>>(block, instanceExpression);
			//if (methodReturnType == typeof(string[]))	
				//return Expression.Lambda<Func<T, string[]>>(block, instanceExpression);
			
			//return Expression.Call(instance, (MethodInfo)method, args);

			//return Expression.Lambda<Func<T, object>>(
			//	Expression.Convert(block, typeof(object)), instanceExpression);
			
		}

		static Type FindGenericType(Type generic, Type type)
		{
			while (type != null && type != typeof(object))
			{
				if (type.IsGenericType && type.GetGenericTypeDefinition() == generic) return type;
				if (generic.IsInterface)
				{
					foreach (Type intfType in type.GetInterfaces())
					{
						Type found = FindGenericType(generic, intfType);
						if (found != null) return found;
					}
				}
				type = type.BaseType;
			}
			return null;
		}
		LambdaExpression StringFormat(LambdaExpression lax, ParameterExpression instanceExpression)
		{
			ParameterExpression cresult = Expression.Parameter(typeof(string), "cresult");
			ParameterExpression temp = Expression.Parameter(typeof(object), "temp");
			ParameterExpression stemp = Expression.Parameter(typeof(string), "string");
			LabelTarget cblockReturnLabel = Expression.Label(typeof(string));

			MethodInfo stringFormat = typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object) });
			BlockExpression cblock = Expression.Block(
				typeof(string),
				new[] { cresult, temp },
				Expression.Assign(temp, Expression.Invoke(lax, instanceExpression)),
				Expression.Assign(cresult, Expression.Call(stringFormat, Expression.Constant("{0}"), temp)),
				Expression.Return(cblockReturnLabel, cresult),
				Expression.Label(cblockReturnLabel, Expression.Constant(null, typeof(string))));

			LambdaExpression lax2 = Expression.Lambda<Func<T, string>>(cblock, instanceExpression);
			var expression = Expression.Lambda<Func<T, string>>(cblock, instanceExpression);
			return expression;

		}
		Expression ParseAggregate(Expression instance, Type elementType, string methodName, int errorPos)
		{
			ParameterExpression outerIt = it;
			ParameterExpression innerIt = Expression.Parameter(elementType, "");
			it = innerIt;
			Expression[] args = ParseArgumentList();
			it = outerIt;
			MethodBase signature;
			if (FindMethod(typeof(IEnumerableSignatures), methodName, false, args, out signature) != 1)
				throw ParseError(errorPos, Res.NoApplicableAggregate, methodName);
			Type[] typeArgs;
			if (signature.Name == "Min" || signature.Name == "Max")
			{
				typeArgs = new Type[] { elementType, args[0].Type };
			}
			else
			{
				typeArgs = new Type[] { elementType };
			}
			if (args.Length == 0)
			{
				args = new Expression[] { instance };
			}
			else
			{
				args = new Expression[] { instance, Expression.Lambda(args[0], innerIt) };
			}
			return Expression.Call(typeof(Enumerable), signature.Name, typeArgs, args);
		}

		Expression[] ParseArgumentList()
		{
			ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
			NextToken();
			Expression[] args = token.Id != TokenId.CloseParen ? ParseArguments() : new Expression[0];
			ValidateToken(TokenId.CloseParen, Res.CloseParenOrCommaExpected);
			NextToken();
			return args;
		}

		Expression[] ParseArguments()
		{
			List<Expression> argList = new List<Expression>();
			while (true)
			{
				argList.Add(ParseExpression());
				if (token.Id != TokenId.Comma) break;
				NextToken();
			}
			return argList.ToArray();
		}

		Expression ParseElementAccess(Expression expr)
		{
			int errorPos = token.Pos;
			ValidateToken(TokenId.OpenBracket, Res.OpenParenExpected);
			NextToken();
			Expression[] args = ParseArguments();
			ValidateToken(TokenId.CloseBracket, Res.CloseBracketOrCommaExpected);
			NextToken();
			if (expr.Type.IsArray)
			{
				if (expr.Type.GetArrayRank() != 1 || args.Length != 1)
					throw ParseError(errorPos, Res.CannotIndexMultiDimArray);
				Expression index = PromoteExpression(args[0], typeof(int), true);
				if (index == null)
					throw ParseError(errorPos, Res.InvalidIndex);
				return Expression.ArrayIndex(expr, index);
			}
			else
			{
				MethodBase mb;
				switch (FindIndexer(expr.Type, args, out mb))
				{
					case 0:
						throw ParseError(errorPos, Res.NoApplicableIndexer,
						                 GetTypeName(expr.Type));
					case 1:
						return Expression.Call(expr, (MethodInfo)mb, args);
					default:
						throw ParseError(errorPos, Res.AmbiguousIndexerInvocation,
						                 GetTypeName(expr.Type));
				}
			}
		}

		static bool IsPredefinedType(Type type)
		{
			foreach (Type t in predefinedTypes) if (t == type) return true;
			return false;
		}

		static bool IsNullableType(Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		static Type GetNonNullableType(Type type)
		{
			return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
		}

		static string GetTypeName(Type type)
		{
			Type baseType = GetNonNullableType(type);
			string s = baseType.Name;
			if (type != baseType) s += '?';
			return s;
		}

		static bool IsNumericType(Type type)
		{
			return GetNumericTypeKind(type) != 0;
		}

		static bool IsSignedIntegralType(Type type)
		{
			return GetNumericTypeKind(type) == 2;
		}

		static bool IsUnsignedIntegralType(Type type)
		{
			return GetNumericTypeKind(type) == 3;
		}

		static int GetNumericTypeKind(Type type)
		{
			type = GetNonNullableType(type);
			if (type.IsEnum) return 0;
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Char:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
					return 1;
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
					return 2;
				case TypeCode.Byte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return 3;
				default:
					return 0;
			}
		}

		static bool IsEnumType(Type type)
		{
			return GetNonNullableType(type).IsEnum;
		}

		void CheckAndPromoteOperand(Type signatures, string opName, ref Expression expr, int errorPos)
		{
			Expression[] args = new Expression[] { expr };
			MethodBase method;
			if (FindMethod(signatures, "F", false, args, out method) != 1)
				throw ParseError(errorPos, Res.IncompatibleOperand,
				                 opName, GetTypeName(args[0].Type));
			expr = args[0];
		}

		void CheckAndPromoteOperands(Type signatures, string opName, ref Expression left, ref Expression right, int errorPos)
		{
			Expression[] args = new Expression[] { left, right };
			MethodBase method;
			if (FindMethod(signatures, "F", false, args, out method) != 1)
				throw IncompatibleOperandsError(opName, left, right, errorPos);
			left = args[0];
			right = args[1];
		}

		Exception IncompatibleOperandsError(string opName, Expression left, Expression right, int pos)
		{
			return ParseError(pos, Res.IncompatibleOperands,
			                  opName, GetTypeName(left.Type), GetTypeName(right.Type));
		}

		MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess)
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
			                     (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
			foreach (Type t in SelfAndBaseTypes(type))
			{
				MemberInfo[] members = t.FindMembers(MemberTypes.Property | MemberTypes.Field,
				                                     flags, Type.FilterNameIgnoreCase, memberName);
				if (members.Length != 0) return members[0];
			}
			return null;
		}

		int FindMethod(Type type, string methodName, bool staticAccess, Expression[] args, out MethodBase method)
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
			                     (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
			foreach (Type t in SelfAndBaseTypes(type))
			{
				MemberInfo[] members = t.FindMembers(MemberTypes.Method,
				                                     flags, Type.FilterNameIgnoreCase, methodName);
				int count = FindBestMethod(members.Cast<MethodBase>(), args, out method);
				if (count != 0) return count;
			}
			method = null;
			return 0;
		}

		int FindIndexer(Type type, Expression[] args, out MethodBase method)
		{
			foreach (Type t in SelfAndBaseTypes(type))
			{
				MemberInfo[] members = t.GetDefaultMembers();
				if (members.Length != 0)
				{
					IEnumerable<MethodBase> methods = members.
						OfType<PropertyInfo>().
						Select(p => (MethodBase)p.GetGetMethod()).
						Where(m => m != null);
					int count = FindBestMethod(methods, args, out method);
					if (count != 0) return count;
				}
			}
			method = null;
			return 0;
		}

		static IEnumerable<Type> SelfAndBaseTypes(Type type)
		{
			if (type.IsInterface)
			{
				List<Type> types = new List<Type>();
				AddInterface(types, type);
				return types;
			}
			return SelfAndBaseClasses(type);
		}

		static IEnumerable<Type> SelfAndBaseClasses(Type type)
		{
			while (type != null)
			{
				yield return type;
				type = type.BaseType;
			}
		}

		static void AddInterface(List<Type> types, Type type)
		{
			if (!types.Contains(type))
			{
				types.Add(type);
				foreach (Type t in type.GetInterfaces()) AddInterface(types, t);
			}
		}

		class MethodData
		{
			public MethodBase MethodBase;
			public ParameterInfo[] Parameters;
			public Expression[] Args;
		}

		int FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args, out MethodBase method)
		{
			MethodData[] applicable = methods.
				Select(m => new MethodData { MethodBase = m, Parameters = m.GetParameters() }).
				Where(m => IsApplicable(m, args)).
				ToArray();
			if (applicable.Length > 1)
			{
				applicable = applicable.
					Where(m => applicable.All(n => m == n || IsBetterThan(args, m, n))).
					ToArray();
			}
			if (applicable.Length == 1)
			{
				MethodData md = applicable[0];
				for (int i = 0; i < args.Length; i++) args[i] = md.Args[i];
				method = md.MethodBase;
			}
			else
			{
				method = null;
			}
			return applicable.Length;
		}

		bool IsApplicable(MethodData method, Expression[] args)
		{
			if (method.Parameters.Length != args.Length) return false;
			Expression[] promotedArgs = new Expression[args.Length];
			for (int i = 0; i < args.Length; i++)
			{
				ParameterInfo pi = method.Parameters[i];
				if (pi.IsOut) return false;
				Expression promoted;
				
				//TODO: Turns out this is real difficult to parse and don't really have time to figure this out at the moment
				// to parse params parameter arrays.
				
				////here we need to check if it is a params array parameter
				//if (pi.ParameterType.IsArray 
				//	&& pi.ParameterType.GetElementType() != null
				//	&& pi.GetCustomAttributes(typeof(ParamArrayAttribute), false).Any())
				//{
				//	//it is a params parameter so convert the value to an array
				//	promoted = PromoteExpression(args[i], pi.ParameterType.GetElementType(), false);	
				//}
				//else
				//{
				promoted = PromoteExpression(args[i], pi.ParameterType, false);	
				//}
				if (promoted == null) return false;
				promotedArgs[i] = promoted;
			}
			method.Args = promotedArgs;
			return true;
		}

		Expression PromoteExpression(Expression expr, Type type, bool exact)
		{
			//if the type of the expression is the correct target type, just return it here
			if (expr.Type == type) return expr;
			//if the type of the expression is a func<DynamicNode, object> - invokable returning object, 
			//we are going to return it here, because we can get the real value when we actually have the instance
			//if (typeof(Func<DynamicNode, object>).IsAssignableFrom(expr.Type)) return expr;
			if (expr is LambdaExpression && ((LambdaExpression)expr).Parameters.Count > 0 && ((LambdaExpression)expr).Parameters[0].Type == typeof(T))
			{
				return expr;
			}
			if (expr is ConstantExpression)
			{
				ConstantExpression ce = (ConstantExpression)expr;
				if (ce == nullLiteral)
				{
					if (!type.IsValueType || IsNullableType(type))
						return Expression.Constant(null, type);
				}
				else
				{
					string text;
					if (literals.TryGetValue(ce, out text))
					{
						Type target = GetNonNullableType(type);
						Object value = null;
						switch (Type.GetTypeCode(ce.Type))
						{
							case TypeCode.Int32:
							case TypeCode.UInt32:
							case TypeCode.Int64:
							case TypeCode.UInt64:
								value = ParseNumber(text, target);
								break;
							case TypeCode.Double:
								if (target == typeof(decimal)) value = ParseNumber(text, target);
								break;
							case TypeCode.String:
								value = ParseEnum(text, target);
								break;
						}
						if (value != null)
							return Expression.Constant(value, type);
					}
				}
			}
			if (IsCompatibleWith(expr.Type, type))
			{
				if (type.IsValueType || exact) return Expression.Convert(expr, type);
				return expr;
			}
			return null;
		}

		static object ParseNumber(string text, Type type)
		{
			switch (Type.GetTypeCode(GetNonNullableType(type)))
			{
				case TypeCode.SByte:
					sbyte sb;
					if (sbyte.TryParse(text, out sb)) return sb;
					break;
				case TypeCode.Byte:
					byte b;
					if (byte.TryParse(text, out b)) return b;
					break;
				case TypeCode.Int16:
					short s;
					if (short.TryParse(text, out s)) return s;
					break;
				case TypeCode.UInt16:
					ushort us;
					if (ushort.TryParse(text, out us)) return us;
					break;
				case TypeCode.Int32:
					int i;
					if (int.TryParse(text, out i)) return i;
					break;
				case TypeCode.UInt32:
					uint ui;
					if (uint.TryParse(text, out ui)) return ui;
					break;
				case TypeCode.Int64:
					long l;
					if (long.TryParse(text, out l)) return l;
					break;
				case TypeCode.UInt64:
					ulong ul;
					if (ulong.TryParse(text, out ul)) return ul;
					break;
				case TypeCode.Single:
					float f;
					if (float.TryParse(text, out f)) return f;
					break;
				case TypeCode.Double:
					double d;
					if (double.TryParse(text, out d)) return d;
					break;
				case TypeCode.Decimal:
					decimal e;
					if (decimal.TryParse(text, out e)) return e;
					break;
			}
			return null;
		}

		static object ParseEnum(string name, Type type)
		{
			if (type.IsEnum)
			{
				MemberInfo[] memberInfos = type.FindMembers(MemberTypes.Field,
				                                            BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static,
				                                            Type.FilterNameIgnoreCase, name);
				if (memberInfos.Length != 0) return ((FieldInfo)memberInfos[0]).GetValue(null);
			}
			return null;
		}

		static bool IsCompatibleWith(Type source, Type target)
		{
			if (source == target) return true;
			if (!target.IsValueType) return target.IsAssignableFrom(source);
			Type st = GetNonNullableType(source);
			Type tt = GetNonNullableType(target);
			if (st != source && tt == target) return false;
			TypeCode sc = st.IsEnum ? TypeCode.Object : Type.GetTypeCode(st);
			TypeCode tc = tt.IsEnum ? TypeCode.Object : Type.GetTypeCode(tt);
			switch (sc)
			{
				case TypeCode.SByte:
					switch (tc)
					{
						case TypeCode.SByte:
						case TypeCode.Int16:
						case TypeCode.Int32:
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
							return true;
					}
					break;
				case TypeCode.Byte:
					switch (tc)
					{
						case TypeCode.Byte:
						case TypeCode.Int16:
						case TypeCode.UInt16:
						case TypeCode.Int32:
						case TypeCode.UInt32:
						case TypeCode.Int64:
						case TypeCode.UInt64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
							return true;
					}
					break;
				case TypeCode.Int16:
					switch (tc)
					{
						case TypeCode.Int16:
						case TypeCode.Int32:
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
							return true;
					}
					break;
				case TypeCode.UInt16:
					switch (tc)
					{
						case TypeCode.UInt16:
						case TypeCode.Int32:
						case TypeCode.UInt32:
						case TypeCode.Int64:
						case TypeCode.UInt64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
							return true;
					}
					break;
				case TypeCode.Int32:
					switch (tc)
					{
						case TypeCode.Int32:
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
							return true;
					}
					break;
				case TypeCode.UInt32:
					switch (tc)
					{
						case TypeCode.UInt32:
						case TypeCode.Int64:
						case TypeCode.UInt64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
							return true;
					}
					break;
				case TypeCode.Int64:
					switch (tc)
					{
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
							return true;
					}
					break;
				case TypeCode.UInt64:
					switch (tc)
					{
						case TypeCode.UInt64:
						case TypeCode.Single:
						case TypeCode.Double:
						case TypeCode.Decimal:
							return true;
					}
					break;
				case TypeCode.Single:
					switch (tc)
					{
						case TypeCode.Single:
						case TypeCode.Double:
							return true;
					}
					break;
				default:
					if (st == tt) return true;
					break;
			}
			return false;
		}

		static bool IsBetterThan(Expression[] args, MethodData m1, MethodData m2)
		{
			bool better = false;
			for (int i = 0; i < args.Length; i++)
			{
				int c = CompareConversions(args[i].Type,
				                           m1.Parameters[i].ParameterType,
				                           m2.Parameters[i].ParameterType);
				if (c < 0) return false;
				if (c > 0) better = true;
			}
			return better;
		}

		// Return 1 if s -> t1 is a better conversion than s -> t2
		// Return -1 if s -> t2 is a better conversion than s -> t1
		// Return 0 if neither conversion is better
		static int CompareConversions(Type s, Type t1, Type t2)
		{
			if (t1 == t2) return 0;
			if (s == t1) return 1;
			if (s == t2) return -1;
			bool t1t2 = IsCompatibleWith(t1, t2);
			bool t2t1 = IsCompatibleWith(t2, t1);
			if (t1t2 && !t2t1) return 1;
			if (t2t1 && !t1t2) return -1;
			if (IsSignedIntegralType(t1) && IsUnsignedIntegralType(t2)) return 1;
			if (IsSignedIntegralType(t2) && IsUnsignedIntegralType(t1)) return -1;
			return 0;
		}

		Expression GenerateEqual(Expression left, Expression right)
		{
			return HandleDynamicNodeLambdas(ExpressionType.Equal, left, right);
		}

		private static Expression HandleDynamicNodeLambdas(ExpressionType expressionType, Expression left, Expression right)
		{
			bool leftIsLambda = false, rightIsLambda = false;
			Expression innerLeft = null;
			Expression innerRight = null;
			UnaryExpression unboxedLeft = null, unboxedRight = null;
			ParameterExpression[] parameters = null;

			if (left is LambdaExpression && (left as LambdaExpression).Type.GetGenericArguments().First() == typeof(T))
			{
				leftIsLambda = true;
			}

			if (right is LambdaExpression && (right as LambdaExpression).Type.GetGenericArguments().First() == typeof(T))
			{
				rightIsLambda = true;
			}

			if (leftIsLambda && !rightIsLambda)
			{
				parameters = new ParameterExpression[(left as LambdaExpression).Parameters.Count];
				(left as LambdaExpression).Parameters.CopyTo(parameters, 0);
				if (right is ConstantExpression)
				{
					//left lambda, right constant
					var invokedExpr = Expression.Invoke(left, (left as LambdaExpression).Parameters.Cast<Expression>());
					innerLeft = Expression.Convert(invokedExpr, (right as ConstantExpression).Type);
				}
				if (leftIsLambda && !rightIsLambda && right is MemberExpression)
				{
					var invokedExpr = Expression.Invoke(left, (left as LambdaExpression).Parameters.Cast<Expression>());
					innerLeft = Expression.Convert(invokedExpr, (right as MemberExpression).Type);
				}
			}
			if (rightIsLambda && !leftIsLambda)
			{
				parameters = new ParameterExpression[(right as LambdaExpression).Parameters.Count];
				(right as LambdaExpression).Parameters.CopyTo(parameters, 0);
				if (left is ConstantExpression)
				{
					//right lambda, left constant
					var invokedExpr = Expression.Invoke(right, (right as LambdaExpression).Parameters.Cast<Expression>());
					innerRight = Expression.Convert(invokedExpr, (left as ConstantExpression).Type);
				}
				if (right is MemberExpression)
				{
					var invokedExpr = Expression.Invoke(right, (right as LambdaExpression).Parameters.Cast<Expression>());
					innerRight = Expression.Convert(invokedExpr, (left as MemberExpression).Type);
				}
			}
			bool sequenceEqual = false;
			if (leftIsLambda && rightIsLambda)
			{
				{
					Type leftType = ((LambdaExpression)left).Type;
					Type rightType = ((LambdaExpression)right).Type;
					Type[] leftTypeGenericArguments = leftType.GetGenericArguments();
					Type[] rightTypeGenericArguments = rightType.GetGenericArguments();
					if (leftTypeGenericArguments.SequenceEqual(rightTypeGenericArguments))
					{
						sequenceEqual = true;
						if (leftTypeGenericArguments.Length == 2)
						{
							Type TOut = leftTypeGenericArguments[1];

							if (expressionType == ExpressionType.AndAlso)
							{
								return ExpressionExtensions.And<T>(left as Expression<Func<T, bool>>, right as Expression<Func<T, bool>>);
							}
							if (expressionType == ExpressionType.OrElse)
							{
								return ExpressionExtensions.Or<T>(left as Expression<Func<T, bool>>, right as Expression<Func<T, bool>>);
							}

						}
					}
					else
					{
						if (leftTypeGenericArguments.Length == 2)
						{
							//sequence not equal - could be Func<DynamicNode,object> && Func<DynamicNode,bool>
							if (leftTypeGenericArguments.First() == rightTypeGenericArguments.First())
							{
								bool leftIsObject = leftTypeGenericArguments.ElementAt(1) == typeof(object);
								bool rightIsObject = rightTypeGenericArguments.ElementAt(1) == typeof(object);
								//if one is an object but not the other
								if (leftIsObject ^ rightIsObject)
								{
									if (leftIsObject)
									{
										//left side is object
										if (innerLeft == null)
										{
											parameters = new ParameterExpression[(left as LambdaExpression).Parameters.Count];
											(left as LambdaExpression).Parameters.CopyTo(parameters, 0);
											innerLeft = Expression.Invoke(left, parameters);
										}
										unboxedLeft = Expression.Unbox(innerLeft, rightTypeGenericArguments.ElementAt(1));

										//left is invoked and unboxed to right's TOut, right was not boxed
										if (expressionType == ExpressionType.AndAlso)
										{
											return ExpressionExtensions.And<T>(right as Expression<Func<T, bool>>, Expression.Lambda<Func<T, bool>>(unboxedLeft, parameters) as Expression<Func<T, bool>>);
										}
										if (expressionType == ExpressionType.OrElse)
										{
											return ExpressionExtensions.And<T>(right as Expression<Func<T, bool>>, Expression.Lambda<Func<T, bool>>(unboxedLeft, parameters) as Expression<Func<T, bool>>);
										}
									}
									else
									{
										//right side is object
										if (innerRight == null)
										{
											parameters = new ParameterExpression[(right as LambdaExpression).Parameters.Count];
											(right as LambdaExpression).Parameters.CopyTo(parameters, 0);
											innerRight = Expression.Invoke(right, parameters);
										}
										unboxedRight = Expression.Unbox(innerRight, leftTypeGenericArguments.ElementAt(1));

										//right is invoked and unboxed to left's TOut, left was not boxed
										if (expressionType == ExpressionType.AndAlso)
										{
											return ExpressionExtensions.And<T>(left as Expression<Func<T, bool>>, Expression.Lambda<Func<T, bool>>(unboxedRight, parameters) as Expression<Func<T, bool>>);
										}
										if (expressionType == ExpressionType.OrElse)
										{
											return ExpressionExtensions.And<T>(left as Expression<Func<T, bool>>, Expression.Lambda<Func<T, bool>>(unboxedRight, parameters) as Expression<Func<T, bool>>);
										}
									}

								}
							}
						}
					}
				}
			}

			if (leftIsLambda && innerLeft == null)
			{
				//left is a lambda, but the right was an unhandled expression type
				//!ConstantExpression, !MemberExpression
				//make sure the left gets invoked
				if (parameters == null)
				{
					parameters = new ParameterExpression[(left as LambdaExpression).Parameters.Count];
					(left as LambdaExpression).Parameters.CopyTo(parameters, 0);
				}
				innerLeft = Expression.Invoke(left, parameters);
			}
			if (rightIsLambda && innerRight == null)
			{
				//right is a lambda, but the left was an unhandled expression type
				//!ConstantExpression, !MemberExpression
				//make sure the right gets invoked
				if (parameters == null)
				{
					parameters = new ParameterExpression[(right as LambdaExpression).Parameters.Count];
					(right as LambdaExpression).Parameters.CopyTo(parameters, 0);
				}
				innerRight = Expression.Invoke(right, parameters);
			}
			if (leftIsLambda && !rightIsLambda && innerLeft != null && !(innerLeft is UnaryExpression) && innerLeft.Type == typeof(object))
			{
				//innerLeft is an invoke
				unboxedLeft = Expression.Unbox(innerLeft, right.Type);
			}
			if (rightIsLambda && !leftIsLambda && innerRight != null && !(innerRight is UnaryExpression) && innerRight.Type == typeof(object))
			{
				//innerRight is an invoke
				unboxedRight = Expression.Unbox(innerRight, left.Type);
			}

			BinaryExpression binaryExpression = null;
			var finalLeft = unboxedLeft ?? innerLeft ?? left;
			var finalRight = unboxedRight ?? innerRight ?? right;
			switch (expressionType)
			{
				case ExpressionType.Equal:
					binaryExpression = Expression.Equal(finalLeft, finalRight);
					break;
				case ExpressionType.NotEqual:
					binaryExpression = Expression.NotEqual(finalLeft, finalRight);
					break;
				case ExpressionType.GreaterThan:
					binaryExpression = Expression.GreaterThan(finalLeft, finalRight);
					break;
				case ExpressionType.LessThan:
					binaryExpression = Expression.LessThan(finalLeft, finalRight);
					break;
				case ExpressionType.GreaterThanOrEqual:
					binaryExpression = Expression.GreaterThanOrEqual(finalLeft, finalRight);
					break;
				case ExpressionType.LessThanOrEqual:
					binaryExpression = Expression.LessThanOrEqual(finalLeft, finalRight);
					break;
				case ExpressionType.Modulo:
					binaryExpression = Expression.Modulo(finalLeft, finalRight);
					return (Expression.Lambda<Func<T, int>>(binaryExpression, parameters));
				case ExpressionType.AndAlso:
					if ((leftIsLambda && rightIsLambda && sequenceEqual) || (!leftIsLambda && !rightIsLambda))
					{
						return Expression.AndAlso(left, right);
					}
					else
					{
						return (Expression.Lambda<Func<T, Boolean>>(Expression.AndAlso(finalLeft, finalRight), parameters));
					}
				case ExpressionType.OrElse:
					if (leftIsLambda && rightIsLambda && sequenceEqual || (!leftIsLambda && !rightIsLambda))
					{
						return Expression.OrElse(left, right);
					}
					else
					{
						return (Expression.Lambda<Func<T, Boolean>>(Expression.OrElse(finalLeft, finalRight), parameters));
					}
				default:
					return Expression.Equal(left, right);
			}
			if (leftIsLambda || rightIsLambda)
			{
				var body = Expression.Condition(Expression.TypeEqual(innerLeft, right.Type), binaryExpression, Expression.Constant(false));
				return Expression.Lambda<Func<T, bool>>(body, parameters);
			}
			else
			{
				return binaryExpression;
			}

		}

		Expression GenerateNotEqual(Expression left, Expression right)
		{
			return HandleDynamicNodeLambdas(ExpressionType.NotEqual, left, right);
		}

		Expression GenerateGreaterThan(Expression left, Expression right)
		{
			if (left.Type == typeof(string))
			{
				return Expression.GreaterThan(
					GenerateStaticMethodCall("Compare", left, right),
					Expression.Constant(0)
					);
			}
			return HandleDynamicNodeLambdas(ExpressionType.GreaterThan, left, right);
		}

		Expression GenerateGreaterThanEqual(Expression left, Expression right)
		{
			if (left.Type == typeof(string))
			{
				return Expression.GreaterThanOrEqual(
					GenerateStaticMethodCall("Compare", left, right),
					Expression.Constant(0)
					);
			}
			return HandleDynamicNodeLambdas(ExpressionType.GreaterThanOrEqual, left, right);
		}

		Expression GenerateLessThan(Expression left, Expression right)
		{
			if (left.Type == typeof(string))
			{
				return Expression.LessThan(
					GenerateStaticMethodCall("Compare", left, right),
					Expression.Constant(0)
					);
			}
			return HandleDynamicNodeLambdas(ExpressionType.LessThan, left, right);
		}

		Expression GenerateLessThanEqual(Expression left, Expression right)
		{
			if (left.Type == typeof(string))
			{
				return Expression.LessThanOrEqual(
					GenerateStaticMethodCall("Compare", left, right),
					Expression.Constant(0)
					);
			}
			return HandleDynamicNodeLambdas(ExpressionType.LessThanOrEqual, left, right);
		}

		Expression GenerateAdd(Expression left, Expression right)
		{
			if (left.Type == typeof(string) && right.Type == typeof(string))
			{
				return GenerateStaticMethodCall("Concat", left, right);
			}
			return Expression.Add(left, right);
		}

		Expression GenerateSubtract(Expression left, Expression right)
		{
			return Expression.Subtract(left, right);
		}

		Expression GenerateStringConcat(Expression left, Expression right)
		{
			return Expression.Call(
				null,
				typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) }),
				new[] { left, right });
		}

		MethodInfo GetStaticMethod(string methodName, Expression left, Expression right)
		{
			return left.Type.GetMethod(methodName, new[] { left.Type, right.Type });
		}

		Expression GenerateStaticMethodCall(string methodName, Expression left, Expression right)
		{
			return Expression.Call(null, GetStaticMethod(methodName, left, right), new[] { left, right });
		}

		void SetTextPos(int pos)
		{
			textPos = pos;
			ch = textPos < textLen ? text[textPos] : '\0';
		}

		void NextChar()
		{
			if (textPos < textLen) textPos++;
			ch = textPos < textLen ? text[textPos] : '\0';
		}

		void NextToken()
		{
			while (Char.IsWhiteSpace(ch)) NextChar();
			TokenId t;
			int tokenPos = textPos;
			switch (ch)
			{
				case '!':
					NextChar();
					if (ch == '=')
					{
						NextChar();
						t = TokenId.ExclamationEqual;
					}
					else
					{
						t = TokenId.Exclamation;
					}
					break;
				case '%':
					NextChar();
					t = TokenId.Percent;
					break;
				case '&':
					NextChar();
					if (ch == '&')
					{
						NextChar();
						t = TokenId.DoubleAmphersand;
					}
					else
					{
						t = TokenId.Amphersand;
					}
					break;
				case '(':
					NextChar();
					t = TokenId.OpenParen;
					break;
				case ')':
					NextChar();
					t = TokenId.CloseParen;
					break;
				case '*':
					NextChar();
					t = TokenId.Asterisk;
					break;
				case '+':
					NextChar();
					t = TokenId.Plus;
					break;
				case ',':
					NextChar();
					t = TokenId.Comma;
					break;
				case '-':
					NextChar();
					t = TokenId.Minus;
					break;
				case '.':
					NextChar();
					t = TokenId.Dot;
					break;
				case '/':
					NextChar();
					t = TokenId.Slash;
					break;
				case ':':
					NextChar();
					t = TokenId.Colon;
					break;
				case '<':
					NextChar();
					if (ch == '=')
					{
						NextChar();
						t = TokenId.LessThanEqual;
					}
					else if (ch == '>')
					{
						NextChar();
						t = TokenId.LessGreater;
					}
					else
					{
						t = TokenId.LessThan;
					}
					break;
				case '=':
					NextChar();
					if (ch == '=')
					{
						NextChar();
						t = TokenId.DoubleEqual;
					}
					else
					{
						t = TokenId.Equal;
					}
					break;
				case '>':
					NextChar();
					if (ch == '=')
					{
						NextChar();
						t = TokenId.GreaterThanEqual;
					}
					else
					{
						t = TokenId.GreaterThan;
					}
					break;
				case '?':
					NextChar();
					t = TokenId.Question;
					break;
				case '[':
					NextChar();
					t = TokenId.OpenBracket;
					break;
				case ']':
					NextChar();
					t = TokenId.CloseBracket;
					break;
				case '|':
					NextChar();
					if (ch == '|')
					{
						NextChar();
						t = TokenId.DoubleBar;
					}
					else
					{
						t = TokenId.Bar;
					}
					break;
				case '"':
				case '\'':
					char quote = ch;
					do
					{
						NextChar();
						while (textPos < textLen && ch != quote) NextChar();
						if (textPos == textLen)
							throw ParseError(textPos, Res.UnterminatedStringLiteral);
						NextChar();
					} while (ch == quote);
					t = TokenId.StringLiteral;
					break;
				default:
					if (Char.IsLetter(ch) || ch == '@' || ch == '_')
					{
						do
						{
							NextChar();
						} while (Char.IsLetterOrDigit(ch) || ch == '_');
						t = TokenId.Identifier;
						break;
					}
					if (Char.IsDigit(ch))
					{
						t = TokenId.IntegerLiteral;
						do
						{
							NextChar();
						} while (Char.IsDigit(ch));
						if (ch == '.')
						{
							t = TokenId.RealLiteral;
							NextChar();
							ValidateDigit();
							do
							{
								NextChar();
							} while (Char.IsDigit(ch));
						}
						if (ch == 'E' || ch == 'e')
						{
							t = TokenId.RealLiteral;
							NextChar();
							if (ch == '+' || ch == '-') NextChar();
							ValidateDigit();
							do
							{
								NextChar();
							} while (Char.IsDigit(ch));
						}
						if (ch == 'F' || ch == 'f') NextChar();
						break;
					}
					if (textPos == textLen)
					{
						t = TokenId.End;
						break;
					}
					throw ParseError(textPos, Res.InvalidCharacter, ch);
			}
			token.Id = t;
			token.Text = text.Substring(tokenPos, textPos - tokenPos);
			token.Pos = tokenPos;
		}

		bool TokenIdentifierIs(string id)
		{
			return token.Id == TokenId.Identifier && String.Equals(id, token.Text, StringComparison.OrdinalIgnoreCase);
		}

		string GetIdentifier()
		{
			ValidateToken(TokenId.Identifier, Res.IdentifierExpected);
			string id = token.Text;
			if (id.Length > 1 && id[0] == '@') id = id.Substring(1);
			return id;
		}

		void ValidateDigit()
		{
			if (!Char.IsDigit(ch)) throw ParseError(textPos, Res.DigitExpected);
		}

		void ValidateToken(TokenId t, string errorMessage)
		{
			if (token.Id != t) throw ParseError(errorMessage);
		}

		void ValidateToken(TokenId t)
		{
			if (token.Id != t) throw ParseError(Res.SyntaxError);
		}

		Exception ParseError(string format, params object[] args)
		{
			return ParseError(token.Pos, format, args);
		}

		Exception ParseError(int pos, string format, params object[] args)
		{
			return new ParseException(string.Format(System.Globalization.CultureInfo.CurrentCulture, format, args), pos);
		}

		static Dictionary<string, object> CreateKeywords()
		{
			Dictionary<string, object> d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			d.Add("true", trueLiteral);
			d.Add("false", falseLiteral);
			d.Add("null", nullLiteral);
			d.Add(keywordIt, keywordIt);
			d.Add(keywordIif, keywordIif);
			d.Add(keywordNew, keywordNew);
			foreach (Type type in predefinedTypes) d.Add(type.Name, type);
			return d;
		}
	}
}