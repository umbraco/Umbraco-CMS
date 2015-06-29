using System;
using System.ComponentModel;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Dictionary
{
	/// <summary>
	/// Resolves the current CultureDictionaryFactory
	/// </summary>
	public sealed class CultureDictionaryFactoryResolver : SingleObjectResolverBase<CultureDictionaryFactoryResolver, ICultureDictionaryFactory>
	{
		internal CultureDictionaryFactoryResolver(ICultureDictionaryFactory factory)
			: base(factory)
		{
		}

        [Obsolete("Use SetDictionaryFactory instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
		public void SetContentStore(ICultureDictionaryFactory factory)
		{
			Value = factory;
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