using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Provides a static service locator for most singletons.
    /// </summary>
    /// <remarks>
    /// <para>This class is initialized with the container in UmbracoApplicationBase,
    /// right after the container is created in UmbracoApplicationBase.HandleApplicationStart.</para>
    /// <para>Obviously, this is a service locator, which some may consider an anti-pattern. And yet,
    /// practically, it works.</para>
    /// </remarks>
    public static class Current
    {
        private static IFactory _factory;

        /// <summary>
        /// Gets or sets the factory.
        /// </summary>
        public static IFactory Factory
        {
            get
            {
                if (_factory == null) throw new InvalidOperationException("No factory has been set.");
                return _factory;
            }
            set
            {
                if (_factory != null) throw new InvalidOperationException("A factory has already been set.");
                _factory = value;
            }
        }

        public static bool HasFactory => _factory != null;

        /// <summary>
        /// Resets <see cref="Current"/>. Indented for testing only, and not supported in production code.
        /// </summary>
        /// <remarks>
        /// <para>For UNIT TESTS exclusively.</para>
        /// <para>Resets everything that is 'current'.</para>
        /// </remarks>
        public static void Reset()
        {
            _factory.DisposeIfDisposable();
            _factory = null;

            Resetted?.Invoke(null, EventArgs.Empty);
        }

        internal static event EventHandler Resetted;

        public static IIOHelper IOHelper
            => Factory.GetInstance<IIOHelper>();

    }
}
