using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Abstract handler that must satisfy the requirement so Succeed or Fail will be called no matter what
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// aspnetcore Authz handlers are not required to satisfy the requirement and generally don't explicitly call Fail when the requirement
    /// isn't satisfied, however in many simple cases explicitly calling Succeed or Fail is what we want which is what this class is used for.
    /// </remarks>
    public abstract class MustSatisfyRequirementAuthorizationHandler<T> : AuthorizationHandler<T> where T : IAuthorizationRequirement
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, T requirement)
        {
            var isAuth = await IsAuthorized(context, requirement);
            if (isAuth)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }

        /// <summary>
        /// Return true if the requirement is succeeded or ignored, return false if the requirement is explicitly not met
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected abstract Task<bool> IsAuthorized(AuthorizationHandlerContext context, T requirement);
    }

    /// <summary>
    /// Abstract handler that must satisfy the requirement so Succeed or Fail will be called no matter what
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResource"></typeparam>
    /// <remarks>
    /// aspnetcore Authz handlers are not required to satisfy the requirement and generally don't explicitly call Fail when the requirement
    /// isn't satisfied, however in many simple cases explicitly calling Succeed or Fail is what we want which is what this class is used for.
    /// </remarks>
    public abstract class MustSatisfyRequirementAuthorizationHandler<T, TResource> : AuthorizationHandler<T, TResource> where T : IAuthorizationRequirement
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, T requirement, TResource resource)
        {
            var isAuth = await IsAuthorized(context, requirement, resource);
            if (isAuth)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }

        /// <summary>
        /// Return true if the requirement is succeeded or ignored, return false if the requirement is explicitly not met
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected abstract Task<bool> IsAuthorized(AuthorizationHandlerContext context, T requirement, TResource resource);
    }
}
