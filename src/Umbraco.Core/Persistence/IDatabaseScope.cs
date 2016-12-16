using System;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Represents a database scope.
    /// </summary>
    public interface IDatabaseScope : IDisposable
    {
        IUmbracoDatabase Database { get; }
    }
}