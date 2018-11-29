namespace Umbraco.Core.Composing
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
        /// Registers a service as its own implementation.
        /// </summary>
        public static void Register<TService>(this IRegister register, Lifetime lifetime = Lifetime.Transient)
            => register.Register(typeof(TService), lifetime);

        /// <summary>
        /// Registers a service with an implementing instance.
        /// </summary>
        public static void RegisterInstance<TService>(this IRegister register, TService instance)
            => register.RegisterInstance(typeof(TService), instance);

        /// <summary>
        /// Registers a base type for auto-registration.
        /// </summary>
        public static void RegisterAuto<TServiceBase>(this IRegister register)
            => register.RegisterAuto(typeof(TServiceBase));
    }
}
