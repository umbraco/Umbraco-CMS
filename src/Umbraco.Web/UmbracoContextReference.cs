using System;

namespace Umbraco.Web
{
    /// <summary>
    /// Represents a reference to an <see cref="UmbracoContext"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>A reference points to an <see cref="UmbracoContext"/> and it may own it (when it
    /// is a root reference) or just reference it. A reference must be disposed after it has
    /// been used. Disposing does nothing if the reference is not a root reference. Otherwise,
    /// it disposes the <see cref="UmbracoContext"/> and clears the
    /// <see cref="IUmbracoContextAccessor"/>.</para>
    /// </remarks>
    public class UmbracoContextReference : IDisposable
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoContextReference"/> class.
        /// </summary>
        internal UmbracoContextReference(UmbracoContext umbracoContext, bool isRoot, IUmbracoContextAccessor umbracoContextAccessor)
        {
            UmbracoContext = umbracoContext;
            IsRoot = isRoot;

            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Gets the <see cref="UmbracoContext"/>.
        /// </summary>
        public UmbracoContext UmbracoContext { get; }

        /// <summary>
        /// Gets a value indicating whether the reference is a root reference.
        /// </summary>
        /// <remarks>
        /// <para></para>
        /// </remarks>
        public bool IsRoot { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            if (IsRoot)
            {
                UmbracoContext.Dispose();
                _umbracoContextAccessor.UmbracoContext = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}