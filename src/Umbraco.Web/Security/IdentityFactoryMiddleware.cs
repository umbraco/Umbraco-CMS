using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// Adapted from Microsoft.AspNet.Identity.Owin.IdentityFactoryMiddleware
    /// </summary>
    public class IdentityFactoryMiddleware<TResult, TOptions> : OwinMiddleware
        where TResult : class, IDisposable
        where TOptions : IdentityFactoryOptions<TResult>
    {
        /// <param name="next">The next middleware in the OWIN pipeline to invoke</param>
        /// <param name="options">Configuration options for the middleware</param>
        public IdentityFactoryMiddleware(OwinMiddleware next, TOptions options)
            : base(next)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.Provider == null) throw new ArgumentException("options.Provider");

            Options = options;
        }

        /// <summary>
        ///     Configuration options
        /// </summary>
        public TOptions Options { get; private set; }

        /// <summary>
        ///     Create an object using the Options.Provider, storing it in the OwinContext and then disposes the object when finished
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task Invoke(IOwinContext context)
        {
            var instance = Options.Provider.Create(Options, context);
            try
            {
                context.Set(instance);
                if (Next != null)
                {
                    await Next.Invoke(context);
                }
            }
            finally
            {
                Options.Provider.Dispose(Options, instance);
            }
        }
    }

    public class IdentityFactoryOptions<T> where T : class, IDisposable
    {
        /// <summary>
        ///     Used to configure the data protection provider
        /// </summary>
        public IDataProtectionProvider DataProtectionProvider { get; set; }

        /// <summary>
        ///     Provider used to Create and Dispose objects
        /// </summary>
        public IdentityFactoryProvider<T> Provider { get; set; }
    }

    public class IdentityFactoryProvider<T> where T : class, IDisposable
    {
        public IdentityFactoryProvider()
        {
            OnDispose = (options, instance) => { };
            OnCreate = (options, context) => null;
        }

        /// <summary>
        ///     A delegate assigned to this property will be invoked when the related method is called
        /// </summary>
        public Func<IdentityFactoryOptions<T>, IOwinContext, T> OnCreate { get; set; }

        /// <summary>
        ///     A delegate assigned to this property will be invoked when the related method is called
        /// </summary>
        public Action<IdentityFactoryOptions<T>, T> OnDispose { get; set; }

        /// <summary>
        ///     Calls the OnCreate Delegate
        /// </summary>
        /// <param name="options"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual T Create(IdentityFactoryOptions<T> options, IOwinContext context)
        {
            return OnCreate(options, context);
        }

        /// <summary>
        ///     Calls the OnDispose delegate
        /// </summary>
        /// <param name="options"></param>
        /// <param name="instance"></param>
        public virtual void Dispose(IdentityFactoryOptions<T> options, T instance)
        {
            OnDispose(options, instance);
        }
    }
}
