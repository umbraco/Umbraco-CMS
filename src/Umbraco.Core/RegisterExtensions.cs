using Umbraco.Core.Composing;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods to the <see cref="IRegister"/> class.
    /// </summary>
    public static class RegisterExtensions
    {
        /// <summary>
        /// Registers a service with an implementation type.
        /// </summary>
        public static void Register<TService, TImplementing>(this IRegister register, Lifetime lifetime = Lifetime.Transient)
            => register.Register(typeof(TService), typeof(TImplementing), lifetime);

        /// <summary>
        /// Registers a service with an implementation type, for a target.
        /// </summary>
        /// <typeparam name="TService">The type you want to resolve as</typeparam>
        /// <typeparam name="TTarget">The type for the registration base (eg: What would be constructed)</typeparam>
        public static void RegisterFor<TService, TImplementing, TTarget>(this IRegister register, Lifetime lifetime = Lifetime.Transient)
            where TService : class
            where TTarget : TService
            => register.RegisterFor<TService, TTarget>(typeof(TImplementing), lifetime);

        /// <summary>
        /// Registers a service as its own implementation.
        /// </summary>
        public static void Register<TService>(this IRegister register, Lifetime lifetime = Lifetime.Transient)
            where TService : class
            => register.Register(typeof(TService), lifetime);

        /// <summary>
        /// Registers a service with an implementing instance.
        /// </summary>
        public static void Register<TService>(this IRegister register, TService instance)
            where TService : class
            => register.Register(typeof(TService), instance);

        /// <summary>
        /// Registers a base type for auto-registration.
        /// </summary>
        public static void RegisterAuto<TServiceBase>(this IRegister register)
            where TServiceBase : class
            => register.RegisterAuto(typeof(TServiceBase));
    }
}
