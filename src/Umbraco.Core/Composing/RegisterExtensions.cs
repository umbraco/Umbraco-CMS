using System;
using Umbraco.Core.IO;

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
        /// Registers a singleton service as its own implementation.
        /// </summary>
        public static void RegisterSingleton<TService>(this IRegister register)
            => register.Register(typeof(TService), Lifetime.Singleton);

        /// <summary>
        /// Registers a singleton service with an implementation type.
        /// </summary>
        public static void RegisterSingleton<TService, TImplementing>(this IRegister register)
            => register.Register(typeof(TService), typeof(TImplementing), Lifetime.Singleton);

        /// <summary>
        /// Registers a singleton service with an implementation factory.
        /// </summary>
        public static void RegisterSingleton<TService>(this IRegister register, Func<IFactory, TService> factory)
            => register.Register(factory, Lifetime.Singleton);

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

        /// <summary>
        /// Registers a filesystem.
        /// </summary>
        /// <typeparam name="TFileSystem">The type of the filesystem.</typeparam>
        /// <typeparam name="TImplementing">The implementing type.</typeparam>
        /// <param name="register">The register.</param>
        /// <param name="supportingFileSystemFactory">A factory method creating the supporting filesystem.</param>
        /// <returns>The register.</returns>
        public static IRegister RegisterFileSystem<TFileSystem, TImplementing>(this IRegister register, Func<IFactory, IFileSystem> supportingFileSystemFactory)
            where TImplementing : FileSystemWrapper, TFileSystem
        {
            register.RegisterSingleton<TFileSystem>(factory =>
            {
                var fileSystems = factory.GetInstance<FileSystems>();
                return fileSystems.GetFileSystem<TImplementing>(supportingFileSystemFactory(factory));
            });

            return register;
        }

        /// <summary>
        /// Registers a filesystem.
        /// </summary>
        /// <typeparam name="TFileSystem">The type of the filesystem.</typeparam>
        /// <param name="register">The register.</param>
        /// <param name="supportingFileSystemFactory">A factory method creating the supporting filesystem.</param>
        /// <returns>The register.</returns>
        public static IRegister RegisterFileSystem<TFileSystem>(this IRegister register, Func<IFactory, IFileSystem> supportingFileSystemFactory)
            where TFileSystem : FileSystemWrapper
        {
            register.RegisterSingleton(factory =>
            {
                var fileSystems = factory.GetInstance<FileSystems>();
                return fileSystems.GetFileSystem<TFileSystem>(supportingFileSystemFactory(factory));
            });

            return register;
        }
    }
}
