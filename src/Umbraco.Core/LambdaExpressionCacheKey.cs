using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Umbraco.Core
{
    /// <summary>
    /// Represents a simple <see cref="LambdaExpression"/> in a form which is suitable for using as a dictionary key
    /// by exposing the return type, argument types and expression string form in a single concatenated string.
    /// </summary>
    internal struct LambdaExpressionCacheKey
    {
        public LambdaExpressionCacheKey(string returnType, string expression, params string[] argTypes)
        {
            ReturnType = returnType;
            ExpressionAsString = expression;
            ArgTypes = new HashSet<string>(argTypes);
            _toString = null;
        }

        public LambdaExpressionCacheKey(LambdaExpression obj)
        {
            ReturnType = obj.ReturnType.FullName;
            ExpressionAsString = obj.ToString();
            ArgTypes = new HashSet<string>(obj.Parameters.Select(x => x.Type.FullName));
            _toString = null;
        }

        /// <summary>
        /// The argument type names of the <see cref="LambdaExpression"/>
        /// </summary>
        public readonly HashSet<string> ArgTypes;

        /// <summary>
        /// The return type of the <see cref="LambdaExpression"/>
        /// </summary>
        public readonly string ReturnType;

        /// <summary>
        /// The original string representation of the <see cref="LambdaExpression"/>
        /// </summary>
        public readonly string ExpressionAsString;

        private string _toString;

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return _toString ?? (_toString = String.Concat(String.Join("|", ArgTypes), ",", ReturnType, ",", ExpressionAsString));
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            var casted = (LambdaExpressionCacheKey)obj;
            return casted.ToString() == ToString();
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
