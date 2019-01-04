using System;

namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// Represents errors that occurs when a migration exception is not executed.
    /// </summary>
    /// <remarks>
    /// <para>Migration expression such as Alter.Table(...).Do() *must* end with Do() else they are
    /// not executed. When a non-executed expression is detected, an IncompleteMigrationExpressionException
    /// is thrown.</para>
    /// </remarks>
    public class IncompleteMigrationExpressionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteMigrationExpressionException"/> class.
        /// </summary>
        public IncompleteMigrationExpressionException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteMigrationExpressionException"/> class with a message.
        /// </summary>
        public IncompleteMigrationExpressionException(string message)
            : base(message)
        { }
    }
}
