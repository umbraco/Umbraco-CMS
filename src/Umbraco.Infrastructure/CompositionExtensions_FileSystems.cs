﻿using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods to the <see cref="Composition"/> class.
    /// </summary>
    public static partial class CompositionExtensions
    {
        /// <summary>
        /// Registers a filesystem.
        /// </summary>
        /// <typeparam name="TFileSystem">The type of the filesystem.</typeparam>
        /// <typeparam name="TImplementing">The implementing type.</typeparam>
        /// <param name="composition">The composition.</param>
        /// <returns>The register.</returns>
        public static void RegisterFileSystem<TFileSystem, TImplementing>(this Composition composition)
            where TImplementing : FileSystemWrapper, TFileSystem
            where TFileSystem : class
        {
            composition.Services.AddUnique<TFileSystem>(factory =>
            {
                var fileSystems = factory.GetRequiredService<FileSystems>();
                var supporting = factory.GetRequiredService<SupportingFileSystems>();
                return fileSystems.GetFileSystem<TImplementing>(supporting.For<TFileSystem>());
            });
        }

        /// <summary>
        /// Registers a filesystem.
        /// </summary>
        /// <typeparam name="TFileSystem">The type of the filesystem.</typeparam>
        /// <param name="composition">The composition.</param>
        /// <returns>The register.</returns>
        public static void RegisterFileSystem<TFileSystem>(this Composition composition)
            where TFileSystem : FileSystemWrapper
        {
            composition.Services.AddUnique(factory =>
            {
                var fileSystems = factory.GetRequiredService<FileSystems>();
                var supporting = factory.GetRequiredService<SupportingFileSystems>();
                return fileSystems.GetFileSystem<TFileSystem>(supporting.For<TFileSystem>());
            });
        }
    }
}
