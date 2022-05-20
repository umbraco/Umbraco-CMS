using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Extensions;

public static class ControllerExtensions
{
    /// <summary>
    ///     Runs the authentication process
    /// </summary>
    /// <param name="controller"></param>
    /// <returns></returns>
    public static async Task<AuthenticateResult> AuthenticateBackOfficeAsync(this ControllerBase controller) =>
        await controller.HttpContext.AuthenticateBackOfficeAsync();

    /// <summary>
    ///     Return the controller name from the controller type
    /// </summary>
    /// <param name="controllerType"></param>
    /// <returns></returns>
    public static string GetControllerName(Type controllerType)
    {
        if (!controllerType.Name.EndsWith("Controller") && !controllerType.Name.EndsWith("Controller`1"))
        {
            throw new InvalidOperationException("The controller type " + controllerType +
                                                " does not follow conventions, MVC Controller class names must be suffixed with the term 'Controller'");
        }

        return controllerType.Name[..controllerType.Name.LastIndexOf("Controller", StringComparison.Ordinal)];
    }

    /// <summary>
    ///     Return the controller name from the controller instance
    /// </summary>
    /// <param name="controllerInstance"></param>
    /// <returns></returns>
    public static string GetControllerName(this Controller controllerInstance) =>
        GetControllerName(controllerInstance.GetType());

    /// <summary>
    ///     Return the controller name from the controller type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <remarks></remarks>
    public static string GetControllerName<T>() => GetControllerName(typeof(T));
}
