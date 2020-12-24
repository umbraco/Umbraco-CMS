using Umbraco.Core.Composing;
namespace Umbraco.Web.Common.Runtime
{

    /// <summary>
    /// Adds/replaces AspNetCore specific services
    /// </summary>
    [ComposeBefore(typeof(ICoreComposer))]
    public class AspNetCoreComposer : ComponentComposer<AspNetCoreComponent>, IComposer
    {
    }
}
