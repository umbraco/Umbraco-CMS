using System;
using NuGet;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Packaging
{
    internal class DefaultPackageContext : IPackageContext
    {
        public DefaultPackageContext(Func<string, string> mapPath)
        {}

        private readonly string _localPackageRepoFolderPath;
        private readonly string _pluginInstallFolderPath;
        private readonly Lazy<IPackageManager> _localPackageManager;
        private readonly Lazy<IPackageRepository> _localPackageRepository;
        private readonly Lazy<IPackageManager> _publicPackageManager;
        private readonly Lazy<IPackageManager> _privatePackageManager;
        private readonly Lazy<IPackageRepository> _publicPackageRepository;
        private readonly Lazy<IPackageRepository> _sprivatePackageRepository;

        /// <summary>
        /// Gets the local path resolver.
        /// </summary>
        public IPackagePathResolver LocalPathResolver
        {
            get { return ((PackageManager)LocalPackageManager).PathResolver; }
        }

        /// <summary>
        /// Gets the local package manager.
        /// </summary>
        public IPackageManager LocalPackageManager
        {
            get { return _localPackageManager.Value; }
        }

        /// <summary>
        /// Gets the public package manager.
        /// </summary>
        public IPackageManager PublicPackageManager
        {
            get { return _publicPackageManager.Value; }
        }
    }
}