using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// A marker interface to designate that a controller will be used for Umbraco front-end requests and/or route hijacking
    /// </summary>
    public interface IRenderController : IController
    {
        
    }
}