using Umbraco.Core.DI;
using CoreCurrent = Umbraco.Core.DI.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.Strings
{
    public class ShortStringHelperResolver
    {
        private ShortStringHelperResolver()
        { }

        public static bool HasCurrent => true;

        public static ShortStringHelperResolver Current { get; }
            = new ShortStringHelperResolver();

        public IShortStringHelper Helper => CoreCurrent.ShortStringHelper;

        public void SetHelper(IShortStringHelper helper)
        {
            CoreCurrent.Container.RegisterSingleton(_ => helper);
        }
    }
}
