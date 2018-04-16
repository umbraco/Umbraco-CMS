using System;
using Umbraco.Core;

namespace Umbraco.Tests.Testing
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, /*AllowMultiple = false,*/ Inherited = false)]
    public class UmbracoTestAttribute : TestOptionAttributeBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether tests are "WithApplication".
        /// </summary>
        /// <remarks>
        /// <para>Default is false.</para>
        /// <para>This is for tests that inherited from TestWithApplicationBase.</para>
        /// <para>Implies AutoMapper = true (, ResetPluginManager = false).</para>
        /// </remarks>
        public bool WithApplication { get => _withApplication.ValueOrDefault(false); set => _withApplication.Set(value); }
        private readonly Settable<bool> _withApplication = new Settable<bool>();

        /// <summary>
        /// Gets or sets a value indicating whether to compose and initialize AutoMapper.
        /// </summary>
        /// <remarks>Default is false unless WithApplication is true, in which case default is true.</remarks>
        public bool AutoMapper { get => _autoMapper.ValueOrDefault(WithApplication); set => _autoMapper.Set(value); }
        private readonly Settable<bool> _autoMapper = new Settable<bool>();

        /// <summary>
        /// Gets or sets a value indicating ... FIXME to be completed
        /// </summary>
        public bool PublishedRepositoryEvents { get => _publishedRepositoryEvents.ValueOrDefault(false); set => _publishedRepositoryEvents.Set(value); }
        private readonly Settable<bool> _publishedRepositoryEvents = new Settable<bool>();

        /// <summary>
        /// Gets or sets a value indicating the required logging support.
        /// </summary>
        /// <remarks>Default is to mock logging.</remarks>
        public UmbracoTestOptions.Logger Logger { get => _logger.ValueOrDefault(UmbracoTestOptions.Logger.Mock); set => _logger.Set(value); }
        private readonly Settable<UmbracoTestOptions.Logger> _logger = new Settable<UmbracoTestOptions.Logger>();

        /// <summary>
        /// Gets or sets a value indicating the required database support.
        /// </summary>
        /// <remarks>Default is no database support.</remarks>
        public UmbracoTestOptions.Database Database { get => _database.ValueOrDefault(UmbracoTestOptions.Database.None); set => _database.Set(value); }
        private readonly Settable<UmbracoTestOptions.Database> _database = new Settable<UmbracoTestOptions.Database>();

        /// <summary>
        /// Gets or sets a value indicating the required plugin manager support.
        /// </summary>
        /// <remarks>Default is to use the global tests plugin manager.</remarks>
        public UmbracoTestOptions.PluginManager PluginManager { get => _pluginManager.ValueOrDefault(UmbracoTestOptions.PluginManager.Default); set => _pluginManager.Set(value); }
        private readonly Settable<UmbracoTestOptions.PluginManager> _pluginManager = new Settable<UmbracoTestOptions.PluginManager>();

        protected override TestOptionAttributeBase Merge(TestOptionAttributeBase other)
        {
            if (!(other is UmbracoTestAttribute attr))
                throw new ArgumentException(nameof(other));

            base.Merge(other);

            _autoMapper.Set(attr._autoMapper);
            _publishedRepositoryEvents.Set(attr._publishedRepositoryEvents);
            _logger.Set(attr._logger);
            _database.Set(attr._database);
            _pluginManager.Set(attr._pluginManager);

            return this;
        }
    }
}
