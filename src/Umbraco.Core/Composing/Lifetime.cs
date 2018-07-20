namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Specifies the lifetime of a registered instance.
    /// </summary>
    public enum Lifetime
    {
        /// <summary>
        /// Always get a new instance.
        /// </summary>
        Transient,

        /// <summary>
        /// One unique instance per request.
        /// </summary>
        Request,

        /// <summary>
        /// One unique instance per container scope.
        /// </summary>
        Scope,

        /// <summary>
        /// One unique instance per container.
        /// </summary>
        Singleton
    }
}
