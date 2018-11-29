using Umbraco.Core.Components;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Provides extension methods to the <see cref="Composition"/> class.
    /// </summary>
    public static class CompositionExtensions
    {
        #region Unique

        /// <summary>
        /// Registers a unique service as its own implementation.
        /// </summary>
        public static void RegisterUnique<TService>(this Composition composition)
            => composition.RegisterUnique(typeof(TService), typeof(TService));

        /// <summary>
        /// Registers a unique service with an implementation type.
        /// </summary>
        public static void RegisterUnique<TService, TImplementing>(this Composition composition)
            => composition.RegisterUnique(typeof(TService), typeof(TImplementing));

        /// <summary>
        /// Registers a unique service with an implementing instance.
        /// </summary>
        public static void RegisterUnique<TService>(this Composition composition, TService instance)
            => composition.RegisterUnique(typeof(TService), instance);

        #endregion
    }
}
