using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            where TService : class
            => composition.Services.AddUnique<TService>();

        /// <summary>
        /// Registers a unique service with an implementation type.
        /// </summary>
        public static void RegisterUnique<TService, TImplementing>(this Composition composition)
            where TService : class
            where TImplementing : class, TService
            => composition.Services.AddUnique<TService, TImplementing>();

        /// <summary>
        /// Registers a unique service with an implementing instance.
        /// </summary>
        public static void RegisterUnique<TService>(this Composition composition, TService instance)
            where TService : class
            => composition.Services.Replace(ServiceDescriptor.Singleton(instance));



        /// <summary>
        /// Registers a unique service with an implementation type.
        /// </summary>
        public static void RegisterMultipleUnique<TService1, TService2, TImplementing>(this Composition composition)
            where TImplementing : class, TService1, TService2
            where TService1 : class
            where TService2 : class
        {
            composition.RegisterUnique<TImplementing>();
            composition.Services.AddUnique<TService1>(factory => factory.GetRequiredService<TImplementing>());
            composition.Services.AddUnique<TService2>(factory => factory.GetRequiredService<TImplementing>());
        }
    }
}
