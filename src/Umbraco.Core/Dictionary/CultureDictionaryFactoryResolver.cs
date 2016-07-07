using System;
using System.Linq.Expressions;
using System.ComponentModel;
using LightInject;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Dictionary
{
	/// <summary>
	/// Resolves the current CultureDictionaryFactory
	/// </summary>
	public sealed class CultureDictionaryFactoryResolver : ContainerSingleObjectResolver<CultureDictionaryFactoryResolver, ICultureDictionaryFactory>
	{
	    /// <summary>
	    /// Initializes the resolver to use IoC
	    /// </summary>
	    /// <param name="container"></param>
        internal CultureDictionaryFactoryResolver(IServiceContainer container)
            : base(container)
	    { }

	    internal CultureDictionaryFactoryResolver(ICultureDictionaryFactory factory)
			: base(factory)
		{
		}

	    /// <summary>
	    /// Initializes the resolver to use IoC
	    /// </summary>
	    /// <param name="container"></param>
	    /// <param name="implementationType"></param>
        internal CultureDictionaryFactoryResolver(IServiceContainer container, Func<IServiceFactory, ICultureDictionaryFactory> implementationType)
            : base(container, implementationType)
	    {
	    }

	    /// <summary>
        /// Can be used by developers at runtime to set their ICultureDictionaryFactory at app startup
        /// </summary>
        /// <param name="factory"></param>
        public void SetDictionaryFactory(ICultureDictionaryFactory factory)
        {
            Value = factory;
        }

		/// <summary>
		/// Returns the ICultureDictionaryFactory
		/// </summary>
		public ICultureDictionaryFactory Factory
		{
			get { return Value; }
		}
	}
}