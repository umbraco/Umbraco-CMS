using System;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Represents a disposable tag scope.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IDisposableTagScope : IDisposable
    {
        /// <summary>
        /// Writes the start tag.
        /// </summary>
        /// <returns>
        /// The current instance as <see cref="IDisposable" />.
        /// </returns>
        IDisposable Start();

        /// <summary>
        /// Writes the end tag.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the end tag was written; otherwise, <c>false</c>.
        /// </returns>
        bool End();
    }
}
