using Umbraco.Core.Composing;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods to the <see cref="Composition"/> class.
    /// </summary>
    public static partial class CompositionExtensions
    {
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



        /// <summary>
        /// Registers a unique service with an implementation type.
        /// </summary>
        public static void RegisterMultipleUnique<TService1, TService2, TImplementing>(this Composition composition)
            where TImplementing : class, TService1, TService2
            where TService1 : class
            where TService2 : class
        {
            composition.RegisterUnique<TImplementing>();
            composition.RegisterUnique<TService1>(factory => factory.GetInstance<TImplementing>());
            composition.RegisterUnique<TService2>(factory => factory.GetInstance<TImplementing>());
        }
    }
}
