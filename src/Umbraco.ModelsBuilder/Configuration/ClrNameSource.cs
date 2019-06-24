namespace Umbraco.ModelsBuilder.Configuration
{
    /// <summary>
    /// Defines the CLR name sources.
    /// </summary>
    public enum ClrNameSource
    {
        /// <summary>
        /// No source.
        /// </summary>
        Nothing = 0,

        /// <summary>
        /// Use the name as source.
        /// </summary>
        Name,

        /// <summary>
        /// Use the alias as source.
        /// </summary>
        Alias,

        /// <summary>
        /// Use the alias directly.
        /// </summary>
        RawAlias
    }
}
