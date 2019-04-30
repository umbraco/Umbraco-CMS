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
        /// <para>Implies Mapper = true (, ResetPluginManager = false).</para>
        /// </remarks>
        public bool WithApplication { get => _withApplication.ValueOrDefault(false); set => _withApplication.Set(value); }
        private readonly Settable<bool> _withApplication = new Settable<bool>();

        /// <summary>
        /// Gets or sets a value indicating whether to compose and initialize the mapper.
        /// </summary>
        /// <remarks>Default is false unless WithApplication is true, in which case default is true.</remarks>
        public bool Mapper { get => _mapper.ValueOrDefault(WithApplication); set => _mapper.Set(value); }
        private readonly Settable<bool> _mapper = new Settable<bool>();

        // FIXME: to be completed
        /// <summary>
        /// Gets or sets a value indicating ... 
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
        public UmbracoTestOptions.TypeLoader TypeLoader { get => _typeLoader.ValueOrDefault(UmbracoTestOptions.TypeLoader.Default); set => _typeLoader.Set(value); }
        private readonly Settable<UmbracoTestOptions.TypeLoader> _typeLoader = new Settable<UmbracoTestOptions.TypeLoader>();

        protected override TestOptionAttributeBase Merge(TestOptionAttributeBase other)
        {
            if (!(other is UmbracoTestAttribute attr))
                throw new ArgumentException(nameof(other));

            base.Merge(other);

            _mapper.Set(attr._mapper);
            _publishedRepositoryEvents.Set(attr._publishedRepositoryEvents);
            _logger.Set(attr._logger);
            _database.Set(attr._database);
            _typeLoader.Set(attr._typeLoader);

            return this;
        }
    }
}
