using System;
using WebCurrent = Umbraco.Web.Composing.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Web.Mvc
{
    public class DefaultRenderMvcControllerResolver
    {
        private DefaultRenderMvcControllerResolver()
        { }

        public static bool HasCurrent => true;

        public static DefaultRenderMvcControllerResolver Current { get; }
            = new DefaultRenderMvcControllerResolver();

        public Type GetDefaultControllerType() => WebCurrent.DefaultRenderMvcControllerType;

        public void SetDefaultControllerType(Type type)
        {
            WebCurrent.DefaultRenderMvcControllerType = type;
        }
    }
}
