using System;

namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Exposes an instance unique identifier.
    /// </summary>
    public interface IInstanceIdentifiable
    {
        /// <summary>
        /// Gets the instance unique identifier.
        /// </summary>
        Guid InstanceId { get; }
    }
}
