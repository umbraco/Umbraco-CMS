using System;
using System.Linq;
using System.Reflection;
using Umbraco.Core;

namespace Umbraco.Tests.Testing
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, /*AllowMultiple = false,*/ Inherited = false)]
    public class UmbracoTestAttribute : Attribute
    {
        #region Properties

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
        public bool ResetPluginManager { get => _resetPluginManager.ValueOrDefault(false); set => _resetPluginManager.Set(value); }
        private readonly Settable<bool> _resetPluginManager = new Settable<bool>();

        /// <summary>
        /// Gets or sets a value indicating ... FIXME to be completed
        /// </summary>
        public bool FacadeServiceRepositoryEvents { get => _facadeServiceRepositoryEvents.ValueOrDefault(false); set => _facadeServiceRepositoryEvents.Set(value); }
        private readonly Settable<bool> _facadeServiceRepositoryEvents = new Settable<bool>();

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

        #endregion

        #region Get

        public static UmbracoTestAttribute Get(MethodInfo method)
        {
            var attr = ((UmbracoTestAttribute[]) method.GetCustomAttributes(typeof (UmbracoTestAttribute), true)).FirstOrDefault();
            var type = method.DeclaringType;
            return Get(type, attr);
        }

        public static UmbracoTestAttribute Get(Type type)
        {
            return Get(type, null);
        }

        private static UmbracoTestAttribute Get(Type type, UmbracoTestAttribute attr)
        {
            while (type != null && type != typeof(object))
            {
                var attr2 = ((UmbracoTestAttribute[]) type.GetCustomAttributes(typeof (UmbracoTestAttribute), true)).FirstOrDefault();
                if (attr2 != null)
                    attr = attr == null ? attr2 : attr2.Merge(attr);
                type = type.BaseType;
            }
            return attr ?? new UmbracoTestAttribute();
        }

        private UmbracoTestAttribute Merge(UmbracoTestAttribute other)
        {
            _autoMapper.Set(other._autoMapper);
            _resetPluginManager.Set(other._resetPluginManager);
            _facadeServiceRepositoryEvents.Set(other._facadeServiceRepositoryEvents);
            _logger.Set(other._logger);
            _database.Set(other._database);
            return this;
        }

        #endregion
    }
}
