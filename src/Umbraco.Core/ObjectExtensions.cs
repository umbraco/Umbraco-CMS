namespace Umbraco.Core
{
    public static class ObjectExtensions
    {
        internal static T As<T>(this object realObject) where T : class 
        {
            if (realObject is T)
                return realObject as T;

            return DynamicWrapper.CreateWrapper<T>(realObject);
        }

        internal static T AsReal<T>(this object wrapper) where T : class
        {
            if (wrapper is T)
                return wrapper as T;

            if (wrapper is DynamicWrapper.DynamicWrapperBase)
                return (T)(wrapper as DynamicWrapper.DynamicWrapperBase).RealObject;

            return null;
        }
    }
}