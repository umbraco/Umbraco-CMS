using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Resolves the IPropertyValueConverter objects.
    /// </summary>
    public sealed class PropertyValueConvertersResolver : ManyObjectsResolverBase<PropertyValueConvertersResolver, IPropertyValueConverter>
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueConvertersResolver"/> class with 
        /// an initial list of converter types.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="converters">The list of converter types</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal PropertyValueConvertersResolver(IServiceProvider serviceProvider, ILogger logger, IEnumerable<Type> converters)
            : base(serviceProvider, logger, converters)
		{ }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueConvertersResolver"/> class with 
        /// an initial list of converter types.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="converters">The list of converter types</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal PropertyValueConvertersResolver(IServiceProvider serviceProvider, ILogger logger, params Type[] converters)
            : base(serviceProvider, logger, converters)
        { }

        /// <summary>
        /// Gets the converters.
        /// </summary>
        public IEnumerable<IPropertyValueConverter> Converters
        {
            get { return Values; }
        }

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private Tuple<IPropertyValueConverter, DefaultPropertyValueConverterAttribute>[] _defaults = null;

        /// <summary>
        /// Caches and gets the default converters with their metadata
        /// </summary>
        internal Tuple<IPropertyValueConverter, DefaultPropertyValueConverterAttribute>[] DefaultConverters
        {
            get
            {
                using (var locker = new UpgradeableReadLock(_lock))
                {
                    if (_defaults == null)
                    {
                        locker.UpgradeToWriteLock();

                        var defaultConvertersWithAttributes = Converters
                            .Select(x => new
                                {
                                    attribute = x.GetType().GetCustomAttribute<DefaultPropertyValueConverterAttribute>(false),
                                    converter = x
                                })
                            .Where(x => x.attribute != null)
                            .ToArray();

                        _defaults = defaultConvertersWithAttributes
                            .Select(
                                x => new Tuple<IPropertyValueConverter, DefaultPropertyValueConverterAttribute>(x.converter, x.attribute))
                            .ToArray();
                    }

                    return _defaults;
                }
            }
        }
    }
}