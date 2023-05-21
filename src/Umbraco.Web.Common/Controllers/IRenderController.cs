using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Web.Common.Controllers;

/// <summary>
///     A marker interface to designate that a controller will be used for Umbraco front-end requests and/or route
///     hijacking
/// </summary>
public interface IRenderController : IDiscoverable
{
}
