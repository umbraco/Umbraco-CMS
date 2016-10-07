using Umbraco.Core.DI;
using CoreCurrent = Umbraco.Core.DI.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.Dictionary
{
    public class CultureDictionaryFactoryResolver
    {
        private CultureDictionaryFactoryResolver()
        { }

        public static bool HasCurrent => true;

        public static CultureDictionaryFactoryResolver Current { get; }
            = new CultureDictionaryFactoryResolver();

        public ICultureDictionaryFactory Factory => CoreCurrent.CultureDictionaryFactory;

        public void SetDictionaryFactory(ICultureDictionaryFactory factory)
        {
            CoreCurrent.Container.RegisterSingleton(_ => factory);
        }
    }
}
