using System;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Represents a published data type.
    /// </summary>
    /// <remarks>
    /// <para>Instances of the <see cref="PublishedDataType"/> class are immutable, ie
    /// if the data type changes, then a new class needs to be created.</para>
    /// <para>These instances should be created by an <see cref="IPublishedContentTypeFactory"/>.</para>
    /// </remarks>
    public class PublishedDataType
    {
        private readonly IDataTypeConfigurationSource _dataTypeConfigurationSource;
        private object _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedDataType"/> class.
        /// </summary>
        internal PublishedDataType(int id, string editorAlias, IDataTypeConfigurationSource dataTypeConfigurationSource)
        {
            _dataTypeConfigurationSource = dataTypeConfigurationSource ?? throw new ArgumentNullException(nameof(dataTypeConfigurationSource));

            Id = id;
            EditorAlias = editorAlias;
        }

        /// <summary>
        /// Gets the data type identifier.
        /// </summary>
        public int Id { get; } // definition id

        /// <summary>
        /// Gets the data type editor alias.
        /// </summary>
        public string EditorAlias { get; }

        /// <summary>
        /// Gets the data type configuration.
        /// </summary>
        public object Configuration
            => _configuration ?? (_configuration = _dataTypeConfigurationSource.GetDataTypeConfiguration(EditorAlias, Id));

        /// <summary>
        /// Gets the data type configuration.
        /// </summary>
        /// <typeparam name="TConfiguration">The type of the configuration object.</typeparam>
        /// <returns>The data type configuration.</returns>
        public TConfiguration GetConfiguration<TConfiguration>()
            where TConfiguration : class
        {
            var configuration = Configuration;
            if (configuration == null) return default;

            if (!(configuration is TConfiguration asTConfiguration))
                throw new InvalidCastException($"Cannot cast data type configuration of type {configuration.GetType()} to {typeof(TConfiguration)}.");

            return asTConfiguration;
        }
    }
}
