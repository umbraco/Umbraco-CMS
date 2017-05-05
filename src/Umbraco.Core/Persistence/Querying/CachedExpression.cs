using System;
using System.Linq.Expressions;

namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// Represents an expression which caches the visitor's result.
    /// </summary>
    internal class CachedExpression : Expression
    {
        private string _visitResult;

        /// <summary>
        /// Gets or sets the inner Expression.
        /// </summary>
        public Expression InnerExpression { get; private set; }

        /// <summary>
        /// Gets or sets the compiled SQL statement output.
        /// </summary>
        public string VisitResult
        {
            get { return _visitResult; }
            set
            {
                if (Visited)
                    throw new InvalidOperationException("Cached expression has already been visited.");
                _visitResult = value;
                Visited = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cache Expression has been compiled already.
        /// </summary>
        public bool Visited { get; private set; }

        /// <summary>
        /// Replaces the inner expression.
        /// </summary>
        /// <param name="expression">expression.</param>
        /// <remarks>The new expression is assumed to have different parameter but produce the same SQL statement.</remarks>
        public void Wrap(Expression expression)
        {
            InnerExpression = expression;
        }
    }
}